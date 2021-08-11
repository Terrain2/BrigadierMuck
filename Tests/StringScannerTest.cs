using System.Text.RegularExpressions;
using Brigadier;
using Xunit;

namespace BrigadierTests
{
    public class StringScannerTest
    {
        private static readonly Regex IntegerRegex = new(@"^[+-]?\d+");
        private static readonly Regex NumberRegex = new(@"^[+-?]?(\d+(\.\d*)?|\d*\.\d+)");
        [Fact]
        public void CanRead()
        {
            var scanner = new StringScanner("abc");
            Assert.True(scanner.CanRead());
            scanner.Skip();
            Assert.True(scanner.CanRead());
            scanner.Skip();
            Assert.True(scanner.CanRead());
            scanner.Skip();
            Assert.False(scanner.CanRead());
        }

        [Fact]
        public void RemainingLength()
        {
            var scanner = new StringScanner("abc");
            Assert.Equal(3, scanner.RemainingLength);
            scanner.Cursor = 1;
            Assert.Equal(2, scanner.RemainingLength);
            scanner.Cursor = 2;
            Assert.Equal(1, scanner.RemainingLength);
            scanner.Cursor = 3;
            Assert.Equal(0, scanner.RemainingLength);
        }

        [Fact]
        public void CanReadLength()
        {
            var scanner = new StringScanner("abc");
            Assert.True(scanner.CanRead(1));
            Assert.True(scanner.CanRead(2));
            Assert.True(scanner.CanRead(3));
            Assert.False(scanner.CanRead(4));
            Assert.False(scanner.CanRead(5));
        }

        [Fact]
        public void Next()
        {
            var scanner = new StringScanner("abc");
            Assert.Equal('a', scanner.Next);
            Assert.Equal(0, scanner.Cursor);
            scanner.Cursor = 2;
            Assert.Equal('c', scanner.Next);
            Assert.Equal(2, scanner.Cursor);
        }

        [Fact]
        public void Peek()
        {
            var scanner = new StringScanner("abc");
            Assert.Equal('a', scanner.Peek(0));
            Assert.Equal('c', scanner.Peek(2));
            Assert.Equal(0, scanner.Cursor);
            scanner.Cursor = 1;
            Assert.Equal('c', scanner.Peek(1));
            Assert.Equal(1, scanner.Cursor);
        }

        [Fact]
        public void Read()
        {
            var scanner = new StringScanner("abc");
            Assert.Equal('a', scanner.Read());
            Assert.Equal('b', scanner.Read());
            Assert.Equal('c', scanner.Read());
            Assert.Equal(3, scanner.Cursor);
        }

        [Fact]
        public void Skip()
        {
            var scanner = new StringScanner("abc");
            scanner.Skip();
            Assert.Equal(1, scanner.Cursor);
        }

        [Fact]
        public void Remaining()
        {
            var scanner = new StringScanner("Hello!");
            Assert.Equal("Hello!", scanner.Remaining);
            scanner.Cursor = 3;
            Assert.Equal("lo!", scanner.Remaining);
            scanner.Cursor = 6;
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void Consumed()
        {
            var scanner = new StringScanner("Hello!");
            Assert.Equal("", scanner.Consumed);
            scanner.Cursor = 3;
            Assert.Equal("Hel", scanner.Consumed);
            scanner.Cursor = 6;
            Assert.Equal("Hello!", scanner.Consumed);
        }

        [Fact]
        public void SkipNoWhitespace()
        {
            var scanner = new StringScanner("Hello!");
            scanner.SkipWhitespace();
            Assert.Equal(0, scanner.Cursor);
        }

        [Fact]
        public void SkipMixedWhitespace()
        {
            var scanner = new StringScanner(" \t \t\nHello!");
            scanner.SkipWhitespace();
            Assert.Equal(5, scanner.Cursor);
        }

        [Fact]
        public void SkipEmptyWhitespace()
        {
            var scanner = new StringScanner("");
            scanner.SkipWhitespace();
            Assert.Equal(0, scanner.Cursor);
        }

        [Fact]
        public void ReadUnquotedString()
        {
            var scanner = new StringScanner("hello world");
            Assert.Equal("hello", scanner.ReadUnquotedString());
            Assert.Equal("hello", scanner.Consumed);
            Assert.Equal(" world", scanner.Remaining);
        }

        [Fact]
        public void ReadUnquotedEmptyString()
        {
            var scanner = new StringScanner("");
            Assert.Equal("", scanner.ReadUnquotedString());
            Assert.Equal("", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadUnquotedEmptyStringWithRemainder() // fuck me this looks like a java class name
        {
            var scanner = new StringScanner(" hello world");
            Assert.Equal("", scanner.ReadUnquotedString());
            Assert.Equal("", scanner.Consumed);
            Assert.Equal(" hello world", scanner.Remaining);
        }

        [Fact]
        public void ReadDoubleQuotedString()
        {
            var scanner = new StringScanner("\"hello world\"");
            Assert.Equal("hello world", scanner.ReadQuotedString());
            Assert.Equal("\"hello world\"", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadSingleQuotedString()
        {
            var scanner = new StringScanner("'hello world'");
            Assert.Equal("hello world", scanner.ReadQuotedString());
            Assert.Equal("'hello world'", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadMixedDoubleQuotedString()
        {
            var scanner = new StringScanner("\"hello 'world'\"");
            Assert.Equal("hello 'world'", scanner.ReadQuotedString());
            Assert.Equal("\"hello 'world'\"", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadMixedSingleQuotedString()
        {
            var scanner = new StringScanner("'hello \"world\"'");
            Assert.Equal("hello \"world\"", scanner.ReadQuotedString());
            Assert.Equal("'hello \"world\"'", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadEmptyQuotedString()
        {
            var scanner = new StringScanner("");
            Assert.Equal("", scanner.ReadQuotedString());
            Assert.Equal("", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadBlankQuotedString()
        {
            var scanner = new StringScanner("\"\"");
            Assert.Equal("", scanner.ReadQuotedString());
            Assert.Equal("\"\"", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadBlankQuotedStringWithRemainder()
        {
            var scanner = new StringScanner("\"\" hello world");
            Assert.Equal("", scanner.ReadQuotedString());
            Assert.Equal("\"\"", scanner.Consumed);
            Assert.Equal(" hello world", scanner.Remaining);
        }

        [Fact]
        public void ReadQuotedStringWithEscapedTerminator()
        {
            var scanner = new StringScanner(@"""hello \""world\"""""); // fucking hell the original testcase for this looks like it was written in \\\
            Assert.Equal("hello \"world\"", scanner.ReadQuotedString());
            Assert.Equal(@"""hello \""world\""""", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadQuotedStringWithEscapedEscapes()
        {
            var scanner = new StringScanner(@"""\\o/"""); // i really like the raw strings in C# because it makes this slightly more readable than the original thing
            Assert.Equal(@"\o/", scanner.ReadQuotedString());
            Assert.Equal(@"""\\o/""", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadQuotedStringWithRemainder()
        {
            var scanner = new StringScanner("\"hello world\" foo bar");
            Assert.Equal("hello world", scanner.ReadQuotedString());
            Assert.Equal("\"hello world\"", scanner.Consumed);
            Assert.Equal(" foo bar", scanner.Remaining);
        }

        [Fact]
        public void ReadQuotedStringWithImmediateRemainder()
        {
            var scanner = new StringScanner("\"hello world\"foo bar");
            Assert.Equal("hello world", scanner.ReadQuotedString());
            Assert.Equal("\"hello world\"", scanner.Consumed);
            Assert.Equal("foo bar", scanner.Remaining);
        }

        [Fact]
        public void ReadQuotedStringWithoutStart()
        {
            var scanner = new StringScanner("hello world\"");
            var ex = Assert.Throws<CommandSyntaxException>(scanner.ReadQuotedString);
            Assert.Equal("Expected quote to start a string", ex.Message);
            Assert.Equal(0, ex.Cursor);
            Assert.Equal(0, scanner.Cursor);
        }

        [Fact]
        public void ReadQuotedStringWithoutEnd()
        {
            var scanner = new StringScanner("\"hello world");
            var ex = Assert.Throws<CommandSyntaxException>(scanner.ReadQuotedString);
            Assert.Equal("Unclosed quoted string", ex.Message);
            Assert.Equal(12, ex.Cursor);
            Assert.Equal(0, scanner.Cursor);
        }

        [Fact]
        public void ReadQuotedStringWithInvalidEscape()
        {
            var scanner = new StringScanner(@"""hello\nworld""");
            var ex = Assert.Throws<CommandSyntaxException>(scanner.ReadQuotedString);
            Assert.Equal(@"Invalid escape sequence '\n' in quoted string", ex.Message);
            Assert.Equal(8, ex.Cursor);
            Assert.Equal(0, scanner.Cursor);
        }

        [Fact]
        public void ReadStringWithoutQuotes()
        {
            var scanner = new StringScanner("hello world");
            Assert.Equal("hello", scanner.ReadString());
            Assert.Equal("hello", scanner.Consumed);
            Assert.Equal(" world", scanner.Remaining);
        }

        [Fact]
        public void ReadStringWithSingleQuotes()
        {
            var scanner = new StringScanner("'hello world'");
            Assert.Equal("hello world", scanner.ReadString());
            Assert.Equal("'hello world'", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadStringWithDoubleQuotes()
        {
            var scanner = new StringScanner("\"hello world\"");
            Assert.Equal("hello world", scanner.ReadString());
            Assert.Equal("\"hello world\"", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadInt32()
        {
            var scanner = new StringScanner("1234567890");
            Assert.Equal(1234567890, scanner.MatchParse<int>(IntegerRegex));
            Assert.Equal("1234567890", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadInt32Negative()
        {
            var scanner = new StringScanner("-1234567890");
            Assert.Equal(-1234567890, scanner.MatchParse<int>(IntegerRegex));
            Assert.Equal("-1234567890", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadInt32Fraction()
        {
            var scanner = new StringScanner("12.34");
            Assert.Equal(12, scanner.MatchParse<int>(IntegerRegex));
            Assert.Equal(".34", scanner.Remaining);
            Assert.Equal(2, scanner.Cursor);
        }

        [Fact]
        public void ReadBlankInt32()
        {
            var scanner = new StringScanner("");
            var ex = Assert.Throws<CommandSyntaxException>(() => scanner.MatchParse<int>(IntegerRegex));
            Assert.Equal("Expected Int32", ex.Message);
            Assert.Equal(0, ex.Cursor);
            Assert.Equal(0, scanner.Cursor);
        }

        [Fact]
        public void ReadInt32WithRemainder()
        {
            var scanner = new StringScanner("1234567890 foo bar");
            Assert.Equal(1234567890, scanner.MatchParse<int>(IntegerRegex));
            Assert.Equal("1234567890", scanner.Consumed);
            Assert.Equal(" foo bar", scanner.Remaining);
        }

        [Fact]
        public void ReadInt32WithImmediateRemainder()
        {
            var scanner = new StringScanner("1234567890foo bar");
            Assert.Equal(1234567890, scanner.MatchParse<int>(IntegerRegex));
            Assert.Equal("1234567890", scanner.Consumed);
            Assert.Equal("foo bar", scanner.Remaining);
        }

        [Fact]
        public void ReadDouble()
        {
            var scanner = new StringScanner("1234567890");
            Assert.Equal(1234567890D, scanner.MatchParse<double>(NumberRegex));
            Assert.Equal("1234567890", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadDoubleNegative()
        {
            var scanner = new StringScanner("-1234567890");
            Assert.Equal(-1234567890D, scanner.MatchParse<double>(NumberRegex));
            Assert.Equal("-1234567890", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadDoubleFraction()
        {
            var scanner = new StringScanner("12.34");
            Assert.Equal(12.34, scanner.MatchParse<double>(NumberRegex));
            Assert.Equal("12.34", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadBlankDouble()
        {
            var scanner = new StringScanner("");
            var ex = Assert.Throws<CommandSyntaxException>(() => scanner.MatchParse<double>(NumberRegex));
            Assert.Equal("Expected Double", ex.Message);
            Assert.Equal(0, ex.Cursor);
            Assert.Equal(0, scanner.Cursor);
        }

        [Fact]
        public void ReadDoubleWithRemainder()
        {
            var scanner = new StringScanner("1234567890 foo bar");
            Assert.Equal(1234567890D, scanner.MatchParse<double>(NumberRegex));
            Assert.Equal("1234567890", scanner.Consumed);
            Assert.Equal(" foo bar", scanner.Remaining);
        }

        [Fact]
        public void ReadDoubleWithImmediateRemainder()
        {
            var scanner = new StringScanner("1234567890foo bar");
            Assert.Equal(1234567890D, scanner.MatchParse<double>(NumberRegex));
            Assert.Equal("1234567890", scanner.Consumed);
            Assert.Equal("foo bar", scanner.Remaining);
        }

        [Fact]
        public void ExpectCorrect()
        {
            var scanner = new StringScanner("abc");
            scanner.Expect('a');
            Assert.Equal(1, scanner.Cursor);
        }

        [Fact]
        public void ExpectIncorrect()
        {
            var scanner = new StringScanner("bcd");
            var ex = Assert.Throws<CommandSyntaxException>(() => scanner.Expect('a'));
            Assert.Equal("Expected 'a'", ex.Message);
            Assert.Equal(1, ex.Cursor);
            Assert.Equal(0, scanner.Cursor);
        }

        [Fact]
        public void ExpectEmpty()
        {
            var scanner = new StringScanner("");
            var ex = Assert.Throws<CommandSyntaxException>(() => scanner.Expect('a'));
            Assert.Equal("Expected 'a'", ex.Message);
            Assert.Equal(0, ex.Cursor);
            Assert.Equal(0, scanner.Cursor);
        }

        [Fact]
        public void ReadBoolCorrect()
        {
            var scanner = new StringScanner("true");
            Assert.True(scanner.ReadBoolean());
            Assert.Equal("true", scanner.Consumed);
            Assert.Equal("", scanner.Remaining);
        }

        [Fact]
        public void ReadBoolIncorrect()
        {
            var scanner = new StringScanner("tuesday");
            var ex = Assert.Throws<CommandSyntaxException>(() => scanner.ReadBoolean());
            Assert.Equal("Invalid bool, expected true or false but found 'tuesday'", ex.Message);
            Assert.Equal(7, ex.Cursor);
            Assert.Equal(0, scanner.Cursor);
        }
    }
}
