using System.Threading.Tasks;
using Brigadier;
using Xunit;
using static Brigadier.StaticUtils;

namespace BrigadierTests
{
    public class ArgumentCommandNodeTest : AbstractCommandNodeTest
    {
        private readonly ArgumentCommandNode<int> node;
        private readonly CommandContextBuilder contextBuilder;
        protected override CommandNode Node => node;

        public ArgumentCommandNodeTest()
        {
            node = Argument("foo", Arguments.Int32()).Build() as ArgumentCommandNode<int>;
            contextBuilder = new CommandContextBuilder(new CommandDispatcher(), new CommandSender(), new RootCommandNode(), 0);
        }

        [Fact]
        public void Parse()
        {
            var scanner = new StringScanner("123, 456");
            node.Parse(scanner, contextBuilder);

            var foo = Assert.IsType<ParsedArgument<int>>(contextBuilder.Arguments["foo"]);
            Assert.Equal(123, foo.Result);
        }

        [Fact]
        public void Usage()
        {
            Assert.Equal("<foo>", node.Usage);
        }

        [Fact]
        public async Task Suggestions()
        {
            var result = await node.ListSuggestions(contextBuilder.Build(""), new SuggestionsBuilder("", 0));
            Assert.Empty(result.List);
        }

        [Fact]
        public void Equality()
        {
            Assert.Equal(
                Argument("foo", Arguments.Int32()).Build(),
                Argument("foo", Arguments.Int32()).Build()
            );
            Assert.Equal(
                Argument("foo", Arguments.Int32()).ExecutesRaw(command).Build(),
                Argument("foo", Arguments.Int32()).ExecutesRaw(command).Build()
            );
            Assert.Equal(
                Argument("bar", Arguments.Int32(-100, 100)).Build(),
                Argument("bar", Arguments.Int32(-100, 100)).Build()
            );
            Assert.Equal(
                Argument("foo", Arguments.Int32(-100, 100)).Build(),
                Argument("foo", Arguments.Int32(-100, 100)).Build()
            );
            Assert.Equal(
                Argument("foo", Arguments.Int32()).Then(
                    Argument("bar", Arguments.Int32())
                ).Build(),
                Argument("foo", Arguments.Int32()).Then(
                    Argument("bar", Arguments.Int32())
                ).Build()
            );
        }

        [Fact]
        public void CreateBuilder()
        {
            var builder = node.CreateBuilder() as RequiredArgumentBuilder<int>;
            Assert.Equal(node.Name, builder.Name);
            Assert.Same(node.type, builder.type);
            Assert.Same(node.Requirement, builder.Requirement);
            Assert.Same(node.Command, builder.Command);
        }
    }
}