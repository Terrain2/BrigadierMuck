using System.Linq;
using Brigadier;
using Moq;
using Xunit;
using static Brigadier.StaticUtils;

namespace BrigadierTests
{
    public class LiteralArgumentBuilderTest
    {
        private readonly LiteralArgumentBuilder builder;
        private readonly Command command;

        public LiteralArgumentBuilderTest()
        {
            builder = Literal("foo");
            command = Mock.Of<Command>();
        }

        [Fact]
        public void Build()
        {
            var node = builder.Build();
            Assert.Equal("foo", node.Name);
        }

        [Fact]
        public void BuildWithExecutor()
        {
            var node = builder.ExecutesRaw(command).Build();
            Assert.Equal("foo", node.Name);
            Assert.Same(command, node.Command);
        }

        [Fact]
        public void BuildWithChildren()
        {
            var node = builder
                .Then(Argument("bar", Arguments.Int32()))
                .Then(Argument("baz", Arguments.Int32()))
                .Build();
            Assert.Equal(2, node.Children.Count());
        }
    }
}