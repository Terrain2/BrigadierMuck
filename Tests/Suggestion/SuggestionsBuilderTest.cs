using System.Collections.Generic;
using System.Linq;
using Brigadier;
using Xunit;

namespace BrigadierTests
{
    public class SuggestionsBuilderTest
    {
        private readonly SuggestionsBuilder builder;

        public SuggestionsBuilderTest()
        {
            builder = new SuggestionsBuilder("Hello w", 6);
        }

        [Fact]
        public void SuggestAppends()
        {
            var result = builder.Suggest("world!").Build();
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(6..7, "world!"),
            }, result.List);
            Assert.Equal(6..7, result.Range);
        }

        [Fact]
        public void SuggestReplaces()
        {
            var result = builder.Suggest("everybody").Build();
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(6..7, "everybody"),
            }, result.List);
            Assert.Equal(6..7, result.Range);
        }

        [Fact]
        public void SuggestNoOp()
        {
            var result = builder.Suggest("w").Build();
            Assert.Empty(result.List);
        }

        [Fact]
        public void SuggestMultiple()
        {
            var result = builder.Suggest("world!").Suggest("everybody").Suggest("weekend").Build();
            Assert.Equal(new List<Suggestion<string>> {
                new Suggestion<string>(6..7, "everybody"),
                new Suggestion<string>(6..7, "weekend"),
                new Suggestion<string>(6..7, "world!"),
            }, result.List);
            Assert.Equal(6..7, result.Range);
        }

        [Fact]
        public void Restart()
        {
            builder.Suggest("won't be included in restart");
            var other = builder.Restart();
            Assert.NotSame(builder, other);
            Assert.Equal(builder.Input, other.Input);
            Assert.Equal(builder.Start, other.Start);
            Assert.Equal(builder.Remaining, other.Remaining);
        }

        [Fact]
        public void SortAlphabetical()
        {
            var result = builder.Suggest("2").Suggest("4").Suggest("6").Suggest("8").Suggest("30").Suggest("32").Build();
            Assert.Equal(new List<string> {
                "2", "30", "32", "4", "6", "8"
            }, result.List.Select(suggestion => suggestion.Text).ToList());
        }

        [Fact]
        public void SortNumerical()
        {
            var result = builder.Suggest(2).Suggest(4).Suggest(6).Suggest(8).Suggest(30).Suggest(32).Build();
            Assert.Equal(new List<string> {
                "2", "4", "6", "8", "30", "32"
            }, result.List.Select(suggestion => suggestion.Text).ToList());
        }

        // sort is non-transitive so a test like this is not acceptable.
        // Why does original brigadier test the output of an undefined sort?

        // [Fact]
        // public void SortMixed()
        // {
        //     var result = builder.Suggest("11").Suggest("22").Suggest("33").Suggest("a").Suggest("b").Suggest("c").Suggest(2).Suggest(4).Suggest(6).Suggest(8).Suggest(30).Suggest(32).Suggest("3a").Suggest("a3").Build();
        //     Assert.Equal(new List<string> {
        //         "11", "2", "22", "33", "4", "6", "8", "30", "32", "3a", "a", "a3", "b", "c"
        //     }, result.List.Select(suggestion => suggestion.Text).ToList());
        // }
    }
}