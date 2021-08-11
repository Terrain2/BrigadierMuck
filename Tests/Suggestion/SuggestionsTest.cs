using System;
using System.Collections.Generic;
using Brigadier;
using Xunit;

namespace BrigadierTests
{
    public class SuggestionsTest
    {
        [Fact]
        public void MergeEmpty()
        {
            Assert.Empty(Suggestions.Merge("foo b", Array.Empty<Suggestions>()).List);
        }

        [Fact]
        public void MergeSingle()
        {
            var suggestions = new Suggestions(5..5, new List<Suggestion> { new Suggestion<string>(5..5, "ar") });
            var merged = Suggestions.Merge("foo b", new[] { suggestions });
            Assert.Equal(suggestions, merged);
        }

        [Fact]
        public void MergeMultiple()
        {
            var a = new Suggestions(5..5, new List<Suggestion> {
                new Suggestion<string>(5..5, "ar"),
                new Suggestion<string>(5..5, "az"),
                new Suggestion<string>(5..5, "Az"),
            });
            var b = new Suggestions(4..5, new List<Suggestion> {
                new Suggestion<string>(4..5, "foo"),
                new Suggestion<string>(4..5, "qux"),
                new Suggestion<string>(4..5, "apple"),
                new Suggestion<string>(4..5, "Bar"),
            });
            var merged = Suggestions.Merge("foo b", new[] { a, b });
            Assert.Equal(new List<Suggestion> {
                new Suggestion<string>(4..5, "apple"),
                new Suggestion<string>(4..5, "bar"),
                new Suggestion<string>(4..5, "Bar"),
                new Suggestion<string>(4..5, "baz"),
                new Suggestion<string>(4..5, "bAz"),
                new Suggestion<string>(4..5, "foo"),
                new Suggestion<string>(4..5, "qux"),
            }, merged.List);
        }
    }
}