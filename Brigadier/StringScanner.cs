using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dawn;

namespace Brigadier
{
    public class StringScanner
    {
        public const char EscapeSequence = '\\';
        public const char DoubleQuote = '"';
        public const char SingleQuote = '\'';
        public readonly string Content;
        public int Cursor;
        public Stack<int> ParseStack = new();
        public CommandSyntaxException MakeException(string message, int pos = 0)
        {
            var error = new CommandSyntaxException(message, Content, Cursor + pos);
            while (ParseStack.Any())
            {
                Cursor = ParseStack.Pop();
            }
            return error;
        }

        public StringScanner(StringScanner original)
        {
            Content = original.Content;
            Cursor = original.Cursor;
        }

        public StringScanner(string content)
        {
            Content = content ?? string.Empty;
        }

        public char Next => Content[Cursor];
        public int Length => Content.Length;
        public int RemainingLength => Content.Length - Cursor;

        public string Consumed => Content[..Cursor];
        public string Remaining => Content[Cursor..];

        public bool CanRead(int amount = 1) => Cursor + amount <= Length;
        public void Skip(int amount = 1) => Cursor += amount;
        public char Read() => Content[Cursor++];
        public string Read(int amount) => Content[Cursor..(Cursor += amount)];
        public char Peek(int offset = 0) => Content[Cursor + offset];

        public void SkipWhitespace()
        {
            while (CanRead() && char.IsWhiteSpace(Next)) Skip();
        }

        public void SkipToWhitespace()
        {
            while (CanRead() && !char.IsWhiteSpace(Next)) Skip();
        }

        public string SubstringWhile(Predicate<char> predicate)
        {
            ParseStack.Push(Cursor);
            while (CanRead() && predicate(Next)) Skip();
            return Content[ParseStack.Pop()..Cursor];
        }

        public delegate bool GenericTryParse<T>(string input, NumberStyles style, IFormatProvider format, out T result);
        public T MatchParse<T>(Regex regex)
        {
            ParseStack.Push(Cursor);
            var result = MatchRegex(regex);

            if (string.IsNullOrEmpty(result)) throw MakeException($"Expected {typeof(T).Name}");
            if (!ValueString.Of(result).Is(out T value)) throw MakeException($"Invalid {typeof(T).Name}");
            ParseStack.Pop();
            return value;
        }

        public string MatchRegex(Regex regex)
        {
            var match = regex.Match(Remaining);
            if (match.Success)
            {
                Cursor += match.Length;
                return match.Value;
            }
            return null;
        }

        public string ReadUnquotedString() => SubstringWhile(ch => char.IsLetterOrDigit(ch) || char.IsSymbol(ch));
        public string ReadQuotedString()
        {
            if (!CanRead()) return "";
            ParseStack.Push(Cursor);
            if (Next == DoubleQuote || Next == SingleQuote) return ReadStringUntil(Read());
            throw MakeException("Expected quote to start a string");
        }

        public string ReadStringUntil(char terminator)
        {
            ParseStack.Push(Cursor);
            var result = new StringBuilder();
            var escaped = false;
            while (CanRead())
            {
                var ch = Read();
                if (escaped)
                {

                    if (ch == terminator || ch == EscapeSequence)
                    {
                        result.Append(ch);
                        escaped = false;
                        continue;
                    }
                    throw MakeException($"Invalid escape sequence '{EscapeSequence}{ch}' in quoted string");
                }

                if (ch == terminator)
                {
                    ParseStack.Pop();
                    return result.ToString();
                }

                if (ch == EscapeSequence)
                {
                    escaped = true;
                }
                else
                {
                    result.Append(ch);
                }
            }
            throw MakeException("Unclosed quoted string");
        }

        public string ReadString()
        {
            if (!CanRead()) return "";
            if (Next == DoubleQuote || Next == SingleQuote) return ReadStringUntil(Read());
            return ReadUnquotedString();
        }

        public bool ReadBoolean()
        {
            ParseStack.Push(Cursor);
            var value = ReadString();
            if (string.IsNullOrEmpty(value)) throw MakeException("Expected bool");

            if (value == "true") { ParseStack.Pop(); return true; }
            if (value == "false") { ParseStack.Pop(); return false; }
            throw MakeException($"Invalid bool, expected true or false but found '{value}'");
        }

        public void Expect(char ch)
        {
            ParseStack.Push(Cursor);
            if (!CanRead() || Read() != ch) throw MakeException($"Expected '{ch}'");
            ParseStack.Pop();
        }
    }

}