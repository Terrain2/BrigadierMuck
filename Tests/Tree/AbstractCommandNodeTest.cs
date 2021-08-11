using System;
using System.Linq;
using Brigadier;
using Moq;
using Xunit;
using static Brigadier.StaticUtils;

namespace BrigadierTests
{
    public abstract class AbstractCommandNodeTest
    {
        protected Command command;

        protected abstract CommandNode Node { get; }

        public AbstractCommandNodeTest()
        {
            command = Mock.Of<Command>();
        }

        [Fact]
        public void AddChild()
        {
            Node.AddChild(Literal("child1").Build());
            Node.AddChild(Literal("child2").Build());
            Node.AddChild(Literal("child1").Build());

            Assert.Equal(2, Node.Children.Count());
        }

        [Fact]
        public void AddChildMergesGrandchildren()
        {
            Node.AddChild(Literal("child").Then(
                Literal("grandchild1")
            ).Build());

            Node.AddChild(Literal("child").Then(
                Literal("grandchild2")
            ).Build());

            var child = Assert.Single(Node.Children);
            Assert.Equal(2, child.Children.Count());
        }

        [Fact]
        public void AddChildPreservesCommand()
        {
            Node.AddChild(Literal("child").ExecutesRaw(command).Build());
            Node.AddChild(Literal("child").Build());

            Assert.Same(command, Assert.Single(Node.Children).Command);
        }

        [Fact]
        public void AddChildOverwritesCommand()
        {
            Node.AddChild(Literal("child").Build());
            Node.AddChild(Literal("child").ExecutesRaw(command).Build());

            Assert.Same(command, Assert.Single(Node.Children).Command);
        }

        // Mojang/brigadier#32
        [Fact]
        public void AddChildWrongTypeThrows()
        {
            Node.AddChild(Argument("child", Arguments.Int32()).Build());
            Assert.Throws<InvalidOperationException>(() => Node.AddChild(Argument("child", Arguments.String()).Build()));
        }
    }
}