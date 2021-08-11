using System;
using System.Text;

namespace Brigadier
{
    public class CommandSyntaxException : Exception
    {
        const int ContextAmount = 10;
        public readonly string Input;
        public readonly int Cursor;

        public CommandSyntaxException(string message, string input = null, int cursor = -1) : base(message)
        {
            Input = input;
            Cursor = cursor;
        }

        public CommandSyntaxException(Exception innerException, string message, string input = null, int cursor = -1) : base(message, innerException)
        {
            Input = input;
            Cursor = cursor;
        }

        public string FriendlyMessage
        {
            get
            {
                var message = InnerException != null ? $"{Message}: {InnerException.Message}" : Message;
                if (Context != null)
                {
                    return $"{message} at position {Cursor}: {Context}";
                }
                return message;
            }
        }

        public string Context
        {
            get
            {
                if (Input == null || Cursor < 0) return null;
                var builder = new StringBuilder();

                var cursor = Math.Min(Input.Length, Cursor);

                if (cursor > ContextAmount)
                {
                    builder.Append("...");
                }

                builder.Append(Input[Math.Max(0, cursor - ContextAmount)..cursor]);
                builder.Append("<--[HERE]");

                return builder.ToString();
            }
        }

        public string ContextAndRest
        {
            get
            {
                if (Input == null || Cursor < 0) return null;
                var builder = new StringBuilder(ChatColors.Gray);

                var cursor = Math.Min(Input.Length, Cursor);

                if (cursor > ContextAmount)
                {
                    builder.Append("...");
                }

                var start = Math.Max(0, cursor - ContextAmount);

                builder.Append(Input[start..cursor]);
                builder.Append($"{ChatColors.End}{ChatColors.Error}");
                if (cursor < Input.Length) builder.Append(Input[cursor..]);
                builder.Append($"<--[HERE]{ChatColors.End}");

                return builder.ToString();
            }
        }
    }
}