using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Brigadier;
using Moq;
using Xunit;
using static Brigadier.StaticUtils;

namespace BrigadierTests
{
    public class CommandDispatcherTest
    {
        private readonly CommandDispatcher subject;
        private readonly Command command;
        private readonly CommandSender source;

        private readonly Expression<Action<Command>> any = cmd => cmd(It.IsAny<CommandContext>());

        public CommandDispatcherTest()
        {
            subject = new CommandDispatcher();
            command = Mock.Of<Command>();
            source = Mock.Of<CommandSender>();
        }

        private static StringScanner InputWithOffset(string input, int offset)
        {
            var scanner = new StringScanner(input);
            scanner.Cursor = offset;
            return scanner;
        }

        [Fact]
        public void CreateAndExecuteCommand()
        {
            subject.Register(Literal("foo").ExecutesRaw(command));

            subject.Execute("foo", source);
            Mock.Get(command).Verify(any, Times.Once);
        }

        [Fact]
        public void CreateAndExecuteCommandWithOffset()
        {
            subject.Register(Literal("foo").ExecutesRaw(command));

            subject.Execute(InputWithOffset("/foo", 1), source);
            Mock.Get(command).Verify(any, Times.Once);
        }

        [Fact]
        public void CreateAndMergeCommands()
        {
            subject.Register(Literal("base").Then(Literal("foo").ExecutesRaw(command)));
            subject.Register(Literal("base").Then(Literal("bar").ExecutesRaw(command)));

            subject.Execute("base foo", source);
            subject.Execute("base bar", source);
            Mock.Get(command).Verify(any, Times.Exactly(2));

        }

        [Fact]
        public void ExecuteUnknownCommand()
        {
            subject.Register(Literal("foo").ExecutesRaw(command));
            subject.Register(Literal("bar").ExecutesRaw(command));

            var ex = Assert.Throws<CommandSyntaxException>(() => subject.Execute("baz", source));
            Assert.Equal("Unknown command", ex.Message);
            Assert.Equal(0, ex.Cursor);

            Mock.Get(command).Verify(any, Times.Never);
        }

        [Fact]
        public void ExecuteImpermissibleCommand()
        {
            subject.Register(Literal("foo").Requires(s => false).ExecutesRaw(command));

            var ex = Assert.Throws<CommandSyntaxException>(() => subject.Execute("foo", source));
            Assert.Equal("Unknown command", ex.Message);
            Assert.Equal(0, ex.Cursor);

            Mock.Get(command).Verify(any, Times.Never);
        }

        [Fact]
        public void ExecuteEmptyCommand()
        {
            subject.Register(Literal(""));
            var ex = Assert.Throws<CommandSyntaxException>(() => subject.Execute("", source));
            Assert.Equal("Unknown command", ex.Message);
            Assert.Equal(0, ex.Cursor);
        }

        [Fact]
        public void ExecuteUnknownSubcommand()
        {
            subject.Register(Literal("foo").ExecutesRaw(command));

            var ex = Assert.Throws<CommandSyntaxException>(() => subject.Execute("foo bar", source));
            Assert.Equal("Incorrect argument for command", ex.Message);
            Assert.Equal(4, ex.Cursor);

            Mock.Get(command).Verify(any, Times.Never);
        }

        [Fact]
        public void ExecuteIncorrectLiteral()
        {
            subject.Register(Literal("foo").ExecutesRaw(command).Then(Literal("bar")));

            var ex = Assert.Throws<CommandSyntaxException>(() => subject.Execute("foo baz", source));
            Assert.Equal("Incorrect argument for command", ex.Message);
            Assert.Equal(4, ex.Cursor);

            Mock.Get(command).Verify(any, Times.Never);
        }

        [Fact]
        public void ExecuteAmbiguousIncorrectArgument()
        {
            subject.Register(
                Literal("foo").ExecutesRaw(command)
                    .Then(Literal("bar"))
                    .Then(Literal("baz"))
            );

            var ex = Assert.Throws<CommandSyntaxException>(() => subject.Execute("foo unknown", source));
            Assert.Equal("Incorrect argument for command", ex.Message);
            Assert.Equal(4, ex.Cursor);

            Mock.Get(command).Verify(any, Times.Never);
        }

        [Fact]
        public void ExecuteSubcommand()
        {
            var subcommand = Mock.Of<Command>();

            subject.Register(Literal("foo").Then(
                Literal("a")
            ).Then(
                Literal("=").ExecutesRaw(subcommand)
            ).Then(
                Literal("c")
            ).ExecutesRaw(command));

            subject.Execute("foo =", source);

            Mock.Get(subcommand).Verify(any, Times.Once);
            Mock.Get(command).Verify(any, Times.Never);
        }

        [Fact]
        public void ParseIncompleteLiteral()
        {
            subject.Register(Literal("foo").Then(Literal("bar").ExecutesRaw(command)));
            var parse = subject.Parse("foo ", source);
            Assert.Equal(" ", parse.Scanner.Remaining);
            Assert.Single(parse.Context.Nodes);
        }

        [Fact]
        public void ParseIncompleteArgument()
        {
            subject.Register(Literal("foo").Then(Argument("bar", Arguments.Int32()).ExecutesRaw(command)));
            var parse = subject.Parse("foo ", source);
            Assert.Equal(" ", parse.Scanner.Remaining);
            Assert.Single(parse.Context.Nodes);
        }

        [Fact]
        public void ExecuteAmbiguousParentSubcommand()
        {
            var subcommand = Mock.Of<Command>();

            subject.Register(
                Literal("test")
                    .Then(
                        Argument("incorrect", Arguments.Int32())
                            .ExecutesRaw(command)
                    )
                    .Then(
                        Argument("right", Arguments.Int32())
                            .Then(
                                Argument("sub", Arguments.Int32())
                                    .ExecutesRaw(subcommand)
                            )
                    )
            );

            subject.Execute("test 1 2", source);

            Mock.Get(subcommand).Verify(any, Times.Once);
            Mock.Get(command).Verify(any, Times.Never);
        }

        [Fact]
        public void ExecuteAmbiguousParentSubcommandViaRedirect()
        {
            var subcommand = Mock.Of<Command>();

            var real = subject.Register(
                Literal("test")
                    .Then(
                        Argument("incorrect", Arguments.Int32())
                            .ExecutesRaw(command)
                    )
                    .Then(
                        Argument("right", Arguments.Int32())
                            .Then(
                                Argument("sub", Arguments.Int32())
                                    .ExecutesRaw(subcommand)
                            )
                    )
            );
            subject.Register(Literal("redirect").Redirect(real));

            subject.Execute("redirect 1 2", source);

            Mock.Get(subcommand).Verify(any, Times.Once);
            Mock.Get(command).Verify(any, Times.Never);
        }

        [Fact]
        public void ExecuteRedirectedMultipleTimes()
        {
            var concreteNode = subject.Register(Literal("actual").ExecutesRaw(command));
            var redirectNode = subject.Register(Literal("redirected").Redirect(subject.RootNode));

            var input = "redirected redirected actual";

            var parse = subject.Parse(input, source);
            Assert.Equal("redirected", input[parse.Context.Range]);
            var parsed0 = Assert.Single(parse.Context.Nodes);
            Assert.Equal(parse.Context.Range, parsed0.Range);
            Assert.Same(subject.RootNode, parse.Context.RootNode);
            Assert.Same(redirectNode, parsed0.Node);

            var child1 = parse.Context.Child;
            Assert.NotNull(child1);
            Assert.Equal("redirected", input[child1.Range]);
            var parsed1 = Assert.Single(child1.Nodes);
            Assert.Equal(child1.Range, parsed1.Range);
            Assert.Same(subject.RootNode, child1.RootNode);
            Assert.Same(redirectNode, parsed1.Node);

            var child2 = child1.Child;
            Assert.NotNull(child2);
            Assert.Equal("actual", input[child2.Range]);
            var parsed2 = Assert.Single(child2.Nodes);
            Assert.Equal(child2.Range, parsed2.Range);
            Assert.Same(subject.RootNode, child2.RootNode);
            Assert.Same(concreteNode, parsed2.Node);

            subject.Execute(parse);
            Mock.Get(command).Verify(any, Times.Once);
        }

        [Fact]
        public void ExecuteRedirected()
        {
            var source1 = new CommandSender();
            var source2 = new CommandSender();

            var mock = new Mock<RedirectModifier>();
            mock.Setup(m => m(It.Is<CommandContext>(ctx => ReferenceEquals(ctx.Sender, source)))).Returns(new List<CommandSender> { source1, source2 });

            var concreteNode = subject.Register(Literal("actual").ExecutesRaw(command));
            var redirectNode = subject.Register(Literal("redirected").Fork(subject.RootNode, mock.Object));

            var input = "redirected actual";

            var parse = subject.Parse(input, source);
            Assert.Equal("redirected", input[parse.Context.Range]);
            var parsed = Assert.Single(parse.Context.Nodes);
            Assert.Equal(parse.Context.Range, parsed.Range);
            Assert.Same(subject.RootNode, parse.Context.RootNode);
            Assert.Same(redirectNode, parsed.Node);
            Assert.Same(source, parse.Context.Sender);

            var parent = parse.Context.Child;
            Assert.NotNull(parent);
            Assert.Equal("actual", input[parent.Range]);
            var parentParsed = Assert.Single(parent.Nodes);
            Assert.Equal(parent.Range, parentParsed.Range);
            Assert.Same(subject.RootNode, parent.RootNode);
            Assert.Same(concreteNode, parentParsed.Node);
            Assert.Same(source, parse.Context.Sender);

            subject.Execute(input, source);
            Mock.Get(command).Verify(cmd => cmd(It.Is<CommandContext>(ctx => ReferenceEquals(ctx.Sender, source1))), Times.Once);
            Mock.Get(command).Verify(cmd => cmd(It.Is<CommandContext>(ctx => ReferenceEquals(ctx.Sender, source2))), Times.Once);
        }

        [Fact]
        public void ExecuteOrphanedSubcommand()
        {
            subject.Register(Literal("foo").Then(
                Argument("bar", Arguments.Int32())
            ).ExecutesRaw(command));

            var ex = Assert.Throws<CommandSyntaxException>(() => subject.Execute("foo 5", source));
            Assert.Equal("Unknown command", ex.Message);
            Assert.Equal(5, ex.Cursor);
        }

        [Fact]
        public void ExecuteInvalidOther()
        {
            var wrong = Mock.Of<Command>();

            subject.Register(Literal("w").ExecutesRaw(wrong));
            subject.Register(Literal("world").ExecutesRaw(command));

            subject.Execute("world", source);
            Mock.Get(wrong).Verify(any, Times.Never);
            Mock.Get(command).Verify(any, Times.Once);
        }

        [Fact]
        public void ParseNoSpaceSeparator()
        {
            subject.Register(Literal("foo").Then(
                Argument("bar", Arguments.Int32()).ExecutesRaw(command)
            ));

            var ex = Assert.Throws<CommandSyntaxException>(() => subject.Execute("foo$", source));
            Assert.Equal("Unknown command", ex.Message);
            Assert.Equal(0, ex.Cursor);
        }

        [Fact]
        public void ExecuteInvalidSubcommand()
        {
            subject.Register(Literal("foo").Then(
                Argument("bar", Arguments.Int32())
            ).ExecutesRaw(command));

            var ex = Assert.Throws<CommandSyntaxException>(() => subject.Execute("foo bar", source));
            Assert.Equal("Expected Int32", ex.Message);
            Assert.Equal(4, ex.Cursor);
        }

        [Fact]
        public void GetPath()
        {
            var bar = Literal("bar").Build();
            subject.Register(Literal("foo").Then(bar));

            Assert.Equal(new[] { "foo", "bar" }, subject.GetPath(bar));
        }

        [Fact]
        public void FindNodeExists()
        {
            var bar = Literal("bar").Build();
            subject.Register(Literal("foo").Then(bar));

            Assert.Same(bar, subject.FindNode("foo", "bar"));
        }

        [Fact]
        public void FindNodeDoesntExist()
        {
            Assert.Throws<KeyNotFoundException>(() => subject.FindNode("foo", "bar"));
            Assert.False(subject.TryFindNode(out var result, "foo", "bar"));
            Assert.Null(result);
        }
    }
}