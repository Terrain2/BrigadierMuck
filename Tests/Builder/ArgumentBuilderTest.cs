using System;
using Brigadier;
using Moq;
using Xunit;
using static Brigadier.StaticUtils;

namespace BrigadierTests
{
    public class ArgumentBuilderTest
    {
        private readonly TestableArgumentBuilder builder;
        public ArgumentBuilderTest()
        {
            builder = new TestableArgumentBuilder();
        }

        [Fact]
        public void TestArguments()
        {
            var argument = Argument("bar", Arguments.Int32());
            Assert.Equal(argument.Build(), Assert.Single(builder.Then(argument).Arguments));
        }

        [Fact]
        public void Redirect()
        {
            var target = Mock.Of<CommandNode>();
            Assert.Same(target, builder.Redirect(target).Target);
        }

        [Fact]
        public void RedirectWithChild()
        {
            var target = Mock.Of<CommandNode>();
            builder.Redirect(target);
            Assert.Throws<InvalidOperationException>(() => builder.Then(Literal("foo")));
        }

        [Fact]
        public void AddChildWithRedirect()
        {
            var target = Mock.Of<CommandNode>();
            builder.Then(Literal("foo"));
            Assert.Throws<InvalidOperationException>(() => builder.Redirect(target));
        }

        private class TestableArgumentBuilder : ArgumentBuilder<TestableArgumentBuilder>
        {
            protected override TestableArgumentBuilder This => this;
            public override CommandNode Build() => null;
        }
    }
}