using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier;
using Xunit;
using static Brigadier.StaticUtils;

namespace BrigadierTests
{
    public class LiteralCommandNodeTest : AbstractCommandNodeTest
    {
        private readonly LiteralCommandNode node;
        protected override CommandNode Node => node;
        private readonly CommandContextBuilder contextBuilder;
        public LiteralCommandNodeTest()
        {
            node = Literal("foo").Build() as LiteralCommandNode;
            contextBuilder = new CommandContextBuilder(new CommandDispatcher(), new CommandSender(), new RootCommandNode(), 0);
        }

        private async Task<List<Suggestion>> SuggestionsFor(string command)
        {
            var suggestions = await node.ListSuggestions(contextBuilder.Build(command), new SuggestionsBuilder(command, 0));
            return suggestions.List;
        }

        [Fact]
        public void Parse()
        {
            var scanner = new StringScanner("foo bar");
            node.Parse(scanner, contextBuilder);
            Assert.Equal(" bar", scanner.Remaining);
        }

        [Fact]
        public void ParseExact()
        {
            var scanner = new StringScanner("foo");
            node.Parse(scanner, contextBuilder);
            Assert.Equal("", scanner.Remaining);
        }

        // i think including the incorrect literal in the error is useful, so this behaviour is intentionally different from Mojang's brigadier.

        [Fact]
        public void ParseSimilar()
        {
            var scanner = new StringScanner("foobar");
            var ex = Assert.Throws<CommandSyntaxException>(() => node.Parse(scanner, contextBuilder));
            Assert.Equal("Expected literal foo", ex.Message);
            Assert.Equal(3, ex.Cursor);
        }

        [Fact]
        public void ParseInvalid()
        {
            var scanner = new StringScanner("bar");
            var ex = Assert.Throws<CommandSyntaxException>(() => node.Parse(scanner, contextBuilder));
            Assert.Equal("Expected literal foo", ex.Message);
            Assert.Equal(3, ex.Cursor);
        }

        [Fact]
        public void Usage()
        {
            Assert.Equal("foo", node.Usage);
        }

        [Fact]
        public async Task Suggestions()
        {
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(0..0, "foo"),
            }, await SuggestionsFor(""));
            Assert.Empty(await SuggestionsFor("foo"));
            Assert.Empty(await SuggestionsFor("food"));
            Assert.Empty(await SuggestionsFor("b"));
        }

        [Fact]
        public void Equality()
        {
            Assert.Equal(
                Literal("foo").Build(),
                Literal("foo").Build()
            );
            Assert.Equal(
                Literal("bar").ExecutesRaw(command).Build(),
                Literal("bar").ExecutesRaw(command).Build()
            );
            Assert.Equal(
                Literal("bar").Build(),
                Literal("bar").Build()
            );
            Assert.Equal(
                Literal("foo").Then(Literal("bar")).Build(),
                Literal("foo").Then(Literal("bar")).Build()
            );
        }

        [Fact]
        public void CreateBuilder()
        {
            var builder = node.CreateBuilder() as LiteralArgumentBuilder;
            Assert.Equal(node.Name, builder.Literal);
            Assert.Same(node.Requirement, builder.Requirement);
            Assert.Same(node.Command, builder.Command);
        }
    }
}