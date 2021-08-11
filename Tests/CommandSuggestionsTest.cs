using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brigadier;
using Moq;
using Xunit;
using static Brigadier.StaticUtils;

namespace BrigadierTests
{
    public class CommandSuggestionsTest
    {
        private readonly CommandDispatcher subject;
        private readonly CommandSender source;

        public CommandSuggestionsTest()
        {
            subject = new CommandDispatcher();
            source = Mock.Of<CommandSender>();
        }

        private async Task TestSuggestions(string contents, int cursor, System.Range range, params string[] suggestions)
        {
            var result = await subject.GetCompletionSuggestions(subject.Parse(contents, source), cursor);
            Assert.Equal(range, result.Range);

            var expected = suggestions.Select(suggestion => new Suggestion<string>(range, suggestion)).ToList();

            Assert.Equal(expected, result.List);
        }

        private static StringScanner InputWithOffset(string input, int offset)
        {
            var scanner = new StringScanner(input);
            scanner.Cursor = offset;
            return scanner;
        }

        [Fact]
        private async Task RootCommands()
        {
            subject.Register(Literal("foo"));
            subject.Register(Literal("bar"));
            subject.Register(Literal("baz"));

            var result = await subject.GetCompletionSuggestions(subject.Parse("", source));

            Assert.Equal(0..0, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(0..0, "bar"),
                new Suggestion<string>(0..0, "baz"),
                new Suggestion<string>(0..0, "foo"),
            }, result.List);
        }

        [Fact]
        private async Task RootCommandsWithInputOffset()
        {
            subject.Register(Literal("foo"));
            subject.Register(Literal("bar"));
            subject.Register(Literal("baz"));

            var result = await subject.GetCompletionSuggestions(subject.Parse(InputWithOffset("000", 3), source));

            Assert.Equal(3..3, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(3..3, "bar"),
                new Suggestion<string>(3..3, "baz"),
                new Suggestion<string>(3..3, "foo"),
            }, result.List);
        }

        [Fact]
        private async Task PartialRootCommands()
        {
            subject.Register(Literal("foo"));
            subject.Register(Literal("bar"));
            subject.Register(Literal("baz"));

            var result = await subject.GetCompletionSuggestions(subject.Parse("b", source));

            Assert.Equal(0..1, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(0..1, "bar"),
                new Suggestion<string>(0..1, "baz"),
            }, result.List);
        }

        [Fact]
        private async Task PartialRootCommandsWithOffset()
        {
            subject.Register(Literal("foo"));
            subject.Register(Literal("bar"));
            subject.Register(Literal("baz"));

            var result = await subject.GetCompletionSuggestions(subject.Parse(InputWithOffset("Zb", 1), source));

            Assert.Equal(1..2, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(1..2, "bar"),
                new Suggestion<string>(1..2, "baz"),
            }, result.List);
        }

        [Fact]
        private async Task SubCommands()
        {
            subject.Register(
                Literal("parent")
                    .Then(Literal("foo"))
                    .Then(Literal("bar"))
                    .Then(Literal("baz"))
            );

            var result = await subject.GetCompletionSuggestions(subject.Parse("parent ", source));
            Assert.Equal(7..7, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(7..7, "bar"),
                new Suggestion<string>(7..7, "baz"),
                new Suggestion<string>(7..7, "foo"),
            }, result.List);
        }

        [Fact]
        private async Task SubcommandsWithMovingCursor()
        {
            subject.Register(
                Literal("parent_one")
                    .Then(Literal("faz"))
                    .Then(Literal("fbz"))
                    .Then(Literal("gaz"))
            );

            subject.Register(Literal("parent_two"));

            await Task.WhenAll(
                TestSuggestions("parent_one faz ", 0, 0..0, "parent_one", "parent_two"),
                TestSuggestions("parent_one faz ", 1, 0..1, "parent_one", "parent_two"),
                TestSuggestions("parent_one faz ", 7, 0..7, "parent_one", "parent_two"),
                TestSuggestions("parent_one faz ", 8, 0..8, "parent_one"),
                TestSuggestions("parent_one faz ", 10, 0..0),
                TestSuggestions("parent_one faz ", 11, 11..11, "faz", "fbz", "gaz"),
                TestSuggestions("parent_one faz ", 12, 11..12, "faz", "fbz"),
                TestSuggestions("parent_one faz ", 13, 11..13, "faz"),
                TestSuggestions("parent_one faz ", 14, 0..0),
                TestSuggestions("parent_one faz ", 15, 0..0)
            );
        }

        [Fact]
        private async Task PartialSubcommands()
        {
            subject.Register(
                Literal("parent")
                    .Then(Literal("foo"))
                    .Then(Literal("bar"))
                    .Then(Literal("baz"))
            );


            var result = await subject.GetCompletionSuggestions(subject.Parse("parent b", source));
            Assert.Equal(7..8, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(7..8, "bar"),
                new Suggestion<string>(7..8, "baz"),
            }, result.List);
        }

        [Fact]
        private async Task PartialSubcommandsWithInputOffset()
        {
            subject.Register(
                Literal("parent")
                    .Then(Literal("foo"))
                    .Then(Literal("bar"))
                    .Then(Literal("baz"))
            );


            var result = await subject.GetCompletionSuggestions(subject.Parse(InputWithOffset("junk parent b", 5), source));
            Assert.Equal(12..13, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(12..13, "bar"),
                new Suggestion<string>(12..13, "baz"),
            }, result.List);
        }

        [Fact]
        private async Task Redirect()
        {
            var actual = subject.Register(Literal("actual").Then(Literal("sub")));
            subject.Register(Literal("redirect").Redirect(actual));

            var result = await subject.GetCompletionSuggestions(subject.Parse("redirect ", source));

            Assert.Equal(9..9, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(9..9, "sub"),
            }, result.List);
        }

        [Fact]
        private async Task PartialRedirect()
        {
            var actual = subject.Register(Literal("actual").Then(Literal("sub")));
            subject.Register(Literal("redirect").Redirect(actual));

            var result = await subject.GetCompletionSuggestions(subject.Parse("redirect s", source));

            Assert.Equal(9..10, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(9..10, "sub"),
            }, result.List);
        }

        [Fact]
        private async Task RedirectWithMovingCursor()
        {
            var actualOne = subject.Register(
                Literal("actual_one")
                    .Then(Literal("faz"))
                    .Then(Literal("fbz"))
                    .Then(Literal("gaz"))
            );
            var actualTwo = subject.Register(Literal("actual_two"));

            subject.Register(Literal("redirect_one").Redirect(actualOne));
            subject.Register(Literal("redirect_two").Redirect(actualTwo)); // i assume this was a typo in the original tests but it doesn't matter because this redirect is never actually used

            await Task.WhenAll(
                TestSuggestions("redirect_one faz ", 0, 0..0, "actual_one", "actual_two", "redirect_one", "redirect_two"),
                TestSuggestions("redirect_one faz ", 9, 0..9, "redirect_one", "redirect_two"),
                TestSuggestions("redirect_one faz ", 10, 0..10, "redirect_one"),
                TestSuggestions("redirect_one faz ", 12, 0..0),
                TestSuggestions("redirect_one faz ", 13, 13..13, "faz", "fbz", "gaz"),
                TestSuggestions("redirect_one faz ", 14, 13..14, "faz", "fbz"),
                TestSuggestions("redirect_one faz ", 15, 13..15, "faz"),
                TestSuggestions("redirect_one faz ", 16, 0..0),
                TestSuggestions("redirect_one faz ", 17, 0..0)
            );
        }

        [Fact]
        private async Task PartialRedirectWithInputOffset()
        {
            var actual = subject.Register(Literal("actual").Then(Literal("sub")));
            subject.Register(Literal("redirect").Redirect(actual));

            var result = await subject.GetCompletionSuggestions(subject.Parse(InputWithOffset("/redirect s", 1), source));

            Assert.Equal(10..11, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(10..11, "sub"),
            }, result.List);
        }

        [Fact]
        private async Task LotsOfRedirects()
        {
            var loop = subject.Register(Literal("redirect"));
            subject.Register(
                Literal("redirect")
                    .Then(
                        Literal("loop")
                            .Then(
                                Argument("loop", Arguments.Int32())
                                    .Redirect(loop)
                            )
                    )
            );

            var result = await subject.GetCompletionSuggestions(subject.Parse("redirect loop 1 loop 02 loop 003 ", source));

            Assert.Equal(33..33, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(33..33, "loop"),
            }, result.List);
        }

        [Fact]
        private async Task MinecraftExecuteSimulation()
        {
            var execute = subject.Register(Literal("execute"));
            subject.Register(
                Literal("execute")
                    .Then(
                        Literal("as")
                            .Then(
                                Argument("name", Arguments.Word())
                                    .Redirect(execute)
                            )
                    )
                .Then(
                    Literal("store")
                        .Then(
                            Argument("name", Arguments.Word())
                                .Redirect(execute)
                        )
                )
                .Then(
                    Literal("run")
                )
            );

            var result = await subject.GetCompletionSuggestions(subject.Parse("execute as Dinnerbone as", source));

            Assert.Empty(result.List);
        }

        [Fact]
        private async Task MinecraftExecutePartialSimulation()
        {
            var execute = subject.Register(Literal("execute"));
            subject.Register(
                Literal("execute")
                    .Then(
                        Literal("as")
                            .Then(Literal("bar").Redirect(execute))
                            .Then(Literal("baz").Redirect(execute))
                    )
                .Then(
                    Literal("store")
                        .Then(
                            Argument("name", Arguments.Word())
                                .Redirect(execute)
                        )
                )
                .Then(
                    Literal("run")
                )
            );

            var result = await subject.GetCompletionSuggestions(subject.Parse("execute as bar as ", source));

            Assert.Equal(18..18, result.Range);
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(18..18, "bar"),
                new Suggestion<string>(18..18, "baz"),
            }, result.List);
        }
    }
}