using System.Collections.Generic;
namespace Brigadier.Arguments
{
    public abstract class StringArgumentType : ArgumentType<string>
    {
        public class Greedy : StringArgumentType
        {
            public override string Parse(StringScanner scanner)
            {
                var result = scanner.Remaining;
                scanner.Cursor = scanner.Length;
                return result;
            }
        }

        public class Quotable : StringArgumentType
        {
            public override string Parse(StringScanner scanner) => scanner.ReadString();
        }

        public class Word : StringArgumentType
        {
            public override string Parse(StringScanner scanner) => scanner.ReadUnquotedString();
        }
    }
}