using System.Linq;
using Brigadier;
using Brigadier.Arguments;
using Moq;
using Xunit;
using static Brigadier.StaticUtils;

namespace BrigadierTests
{
    public class RequiredArgumentBuilderTest
    {
        private readonly ArgumentType<int> type;
        private readonly RequiredArgumentBuilder<int> builder;
        private readonly Command command;

        public RequiredArgumentBuilderTest()
        {
            type = Mock.Of<ArgumentType<int>>();
            command = Mock.Of<Command>();
            builder = Argument("foo", type);
        }

        [Fact]
        public void Build()
        {
            var node = builder.Build() as ArgumentCommandNode<int>;
            Assert.Equal("foo", node.Name);
            Assert.Same(type, node.type);
        }

        [Fact]
        public void BuildWithExecutor()
        {
            var node = builder.ExecutesRaw(command).Build() as ArgumentCommandNode<int>;
            Assert.Equal("foo", node.Name);
            Assert.Same(type, node.type);
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