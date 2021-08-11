using Brigadier;
using Xunit;

namespace BrigadierTests
{
    public class ParsedArgumentTest
    {
        [Fact]
        public void Equality()
        {
            Assert.Equal(new ParsedArgument<string>(0..3, "bar"), new ParsedArgument<string>(0..3, "bar"));
            Assert.Equal(new ParsedArgument<string>(3..6, "baz"), new ParsedArgument<string>(3..6, "baz"));
            Assert.Equal(new ParsedArgument<string>(6..9, "baz"), new ParsedArgument<string>(6..9, "baz"));
        }

        [Fact]
        public void Raw()
        {
            var scanner = new StringScanner("0123456789");
            var argument = new ParsedArgument<string>(2..5, "");
            Assert.Equal("234", scanner.Content[argument.Range]);
        }
    }
}