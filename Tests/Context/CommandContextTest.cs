using System;
using Brigadier;
using Moq;
using Xunit;

namespace BrigadierTests
{
    public class CommandContextTest
    {
        private readonly CommandContextBuilder builder;
        private readonly CommandSender source;
        private readonly CommandDispatcher dispatcher;
        private readonly CommandNode root;

        public CommandContextTest()
        {
            root = Mock.Of<CommandNode>();
            source = Mock.Of<CommandSender>();
            dispatcher = Mock.Of<CommandDispatcher>();

            builder = new CommandContextBuilder(dispatcher, source, root, 0);
        }

        [Fact]
        public void GetNonexistentArgument()
        {
            Assert.Throws<ArgumentException>(() => builder.Build("").Argument<object>("foo"));
        }

        [Fact]
        public void GetWrongArgumentType()
        {
            Assert.Throws<ArgumentException>(() => builder.WithArgument("foo", new ParsedArgument<int>(0..1, 123)).Build("123").Argument<string>("foo"));
        }

        [Fact]
        public void GetArgument()
        {
            Assert.Equal(123, builder.WithArgument("foo", new ParsedArgument<int>(0..1, 123)).Build("123").Argument<int>("foo"));
        }

        [Fact]
        public void Source()
        {
            Assert.Same(source, builder.Build("").Sender);
        }

        [Fact]
        public void RootNode()
        {
            Assert.Same(root, builder.Build("").RootNode);
        }

        [Fact]
        public void Equality()
        {
            var otherSource = new CommandSender();
            var command = Mock.Of<Command>();
            var otherCommand = Mock.Of<Command>();
            var rootNode = Mock.Of<CommandNode>();
            var otherRootNode = Mock.Of<CommandNode>();
            var node = Mock.Of<CommandNode>();
            var otherNode = Mock.Of<CommandNode>();
            Assert.Equal(
                new CommandContextBuilder(dispatcher, source, rootNode, 0).Build(""),
                new CommandContextBuilder(dispatcher, source, rootNode, 0).Build("")
            );
            Assert.Equal(
                new CommandContextBuilder(dispatcher, source, otherRootNode, 0).Build(""),
                new CommandContextBuilder(dispatcher, source, otherRootNode, 0).Build("")
            );
            Assert.Equal(
                new CommandContextBuilder(dispatcher, otherSource, rootNode, 0).Build(""),
                new CommandContextBuilder(dispatcher, otherSource, rootNode, 0).Build("")
            );
            Assert.Equal(
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithCommand(command).Build(""),
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithCommand(command).Build("")
            );
            Assert.Equal(
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithCommand(otherCommand).Build(""),
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithCommand(otherCommand).Build("")
            );
            Assert.Equal(
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithArgument("foo", new ParsedArgument<int>(0..1, 123)).Build("123"),
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithArgument("foo", new ParsedArgument<int>(0..1, 123)).Build("123")
            );
            Assert.Equal(
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithNode(node, 0..3).WithNode(otherNode, 4..6).Build("123 456"),
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithNode(node, 0..3).WithNode(otherNode, 4..6).Build("123 456")
            );
            Assert.Equal(
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithNode(otherNode, 0..3).WithNode(node, 4..6).Build("123 456"),
                new CommandContextBuilder(dispatcher, source, rootNode, 0).WithNode(otherNode, 0..3).WithNode(node, 4..6).Build("123 456")
            );
        }
    }

    static class CommandContextBuilderExtensions
    {
        public static CommandContextBuilder WithNode(this CommandContextBuilder instance, CommandNode node, System.Range range)
        {
            instance.AddNode(node, range);
            return instance;
        }

        public static CommandContextBuilder WithArgument<T>(this CommandContextBuilder instance, string name, ParsedArgument<T> argument)
        {
            instance.Arguments[name] = argument;
            return instance;
        }

        public static CommandContextBuilder WithCommand(this CommandContextBuilder instance, Command command)
        {
            instance.Command = command;
            return instance;
        }
    }
}