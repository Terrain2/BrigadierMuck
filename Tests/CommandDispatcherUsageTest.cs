using System.Collections.Generic;
using System.Linq;
using Brigadier;
using Moq;
using Xunit;
using static Brigadier.StaticUtils;

namespace BrigadierTests
{
    public class CommandDispatcherUsageTest
    {
        private CommandDispatcher subject;
        private readonly CommandSender source;
        private readonly Command command;

        public CommandDispatcherUsageTest()
        {
            source = Mock.Of<CommandSender>();
            command = Mock.Of<Command>();

            subject = new CommandDispatcher();
            subject.Register(
                Literal("a")
                    .Then(
                        Literal("1")
                            .Then(Literal("i").ExecutesRaw(command))
                            .Then(Literal("ii").ExecutesRaw(command))
                    )
                    .Then(
                        Literal("2")
                            .Then(Literal("i").ExecutesRaw(command))
                            .Then(Literal("ii").ExecutesRaw(command))
                    )
            );
            subject.Register(Literal("b").Then(Literal("1").ExecutesRaw(command)));
            subject.Register(Literal("c").ExecutesRaw(command));
            subject.Register(Literal("d").Requires(s => false).ExecutesRaw(command));
            subject.Register(
                Literal("e")
                    .ExecutesRaw(command)
                    .Then(
                        Literal("1")
                            .ExecutesRaw(command)
                            .Then(Literal("i").ExecutesRaw(command))
                            .Then(Literal("ii").ExecutesRaw(command))
                    )
            );
            subject.Register(
                Literal("f")
                    .Then(
                        Literal("1")
                            .Then(Literal("i").ExecutesRaw(command))
                            .Then(Literal("ii").ExecutesRaw(command).Requires(s => false))
                    )
                    .Then(
                        Literal("2")
                            .Then(Literal("i").ExecutesRaw(command).Requires(s => false))
                            .Then(Literal("ii").ExecutesRaw(command))
                    )
            );
            subject.Register(
            Literal("g")
                .ExecutesRaw(command)
                .Then(Literal("1").Then(Literal("i").ExecutesRaw(command)))
        );
            subject.Register(
                Literal("h")
                    .ExecutesRaw(command)
                    .Then(Literal("1").Then(Literal("i").ExecutesRaw(command)))
                    .Then(Literal("2").Then(Literal("i").Then(Literal("ii").ExecutesRaw(command))))
                    .Then(Literal("3").ExecutesRaw(command))
            );
            subject.Register(
                Literal("i")
                    .ExecutesRaw(command)
                    .Then(Literal("1").ExecutesRaw(command))
                    .Then(Literal("2").ExecutesRaw(command))
            );
            subject.Register(
                Literal("j")
                    .Redirect(subject.RootNode)
            );
            subject.Register(
                Literal("k")
                    .Redirect(Get("h"))
            );
        }

        private CommandNode Get(string command) => Get(new StringScanner(command));
        private CommandNode Get(StringScanner command) => subject.Parse(command, source).Context.Nodes.Last().Node;

        [Fact]
        public void AllUsageNoCommands()
        {
            subject = new CommandDispatcher();
            var results = subject.GetAllUsage(subject.RootNode, source, true);
            Assert.Empty(results);
        }

        [Fact]
        public void SmartUsageNoCommands()
        {
            subject = new CommandDispatcher();
            var results = subject.GetSmartUsage(subject.RootNode, source);
            Assert.Empty(results);
        }

        [Fact]
        public void AllUsageRoot()
        {
            var results = subject.GetAllUsage(subject.RootNode, source, true);
            Assert.Equal(new List<string> {
                "a 1 i",
                "a 1 ii",
                "a 2 i",
                "a 2 ii",
                "b 1",
                "c",
                "e",
                "e 1",
                "e 1 i",
                "e 1 ii",
                "f 1 i",
                "f 2 ii",
                "g",
                "g 1 i",
                "h",
                "h 1 i",
                "h 2 i ii",
                "h 3",
                "i",
                "i 1",
                "i 2",
                "j ...",
                "k -> h",
            }, results);
        }

        [Fact]
        public void SmartUsageRoot()
        {
            var results = subject.GetSmartUsage(subject.RootNode, source);
            Assert.Equal(new Dictionary<CommandNode, string> {
                { Get("a"), "a (1|2)" },
                { Get("b"), "b 1" },
                { Get("c"), "c" },
                { Get("e"), "e [1]" },
                { Get("f"), "f (1|2)" },
                { Get("g"), "g [1]" },
                { Get("h"), "h [1|2|3]" },
                { Get("i"), "i [1|2]" },
                { Get("j"), "j ..." },
                { Get("k"), "k -> h" },
            }, results);
        }

        [Fact]
        public void SmartUsageH()
        {
            var results = subject.GetSmartUsage(Get("h"), source);
            Assert.Equal(new Dictionary<CommandNode, string> {
                { Get("h 1"), "[1] i" },
                { Get("h 2"), "[2] i ii"},
                { Get("h 3"), "[3]"},
            }, results);
        }

        [Fact]
        public void SmartUsageOffsetH()
        {
            var offsetH = new StringScanner("/|/|/h");
            offsetH.Cursor = 5;

            var results = subject.GetSmartUsage(Get(offsetH), source);
            Assert.Equal(new Dictionary<CommandNode, string> {
                { Get("h 1"), "[1] i" },
                { Get("h 2"), "[2] i ii"},
                { Get("h 3"), "[3]"},
            }, results);
        }
    }
}