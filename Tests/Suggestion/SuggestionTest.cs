using Brigadier;
using Xunit;

namespace BrigadierTests
{
    public class SuggestionTest
    {
        [Fact]
        public void InsertStart()
        {
            var suggestion = new Suggestion<string>(0..0, "And so I said: ");
            Assert.Equal("And so I said: Hello world!", suggestion.Apply("Hello world!"));
        }

        [Fact]
        public void InsertMiddle()
        {
            var suggestion = new Suggestion<string>(6..6, "small ");
            Assert.Equal("Hello small world!", suggestion.Apply("Hello world!"));
        }

        [Fact]
        public void InsertEnd()
        {
            var suggestion = new Suggestion<string>(5..5, " world!");
            Assert.Equal("Hello world!", suggestion.Apply("Hello"));
        }

        [Fact]
        public void ReplaceStart()
        {
            var suggestion = new Suggestion<string>(0..5, "Goodbye");
            Assert.Equal("Goodbye world!", suggestion.Apply("Hello world!"));
        }

        [Fact]
        public void ReplaceMiddle()
        {
            var suggestion = new Suggestion<string>(6..11, "Alex");
            Assert.Equal("Hello Alex!", suggestion.Apply("Hello world!"));
        }

        [Fact]
        public void ReplaceEnd()
        {
            var suggestion = new Suggestion<string>(6..12, "Creeper!");
            Assert.Equal("Hello Creeper!", suggestion.Apply("Hello world!"));
        }

        [Fact]
        public void ReplaceEverything()
        {
            var suggestion = new Suggestion<string>(0..12, "Oh dear.");
            Assert.Equal("Oh dear.", suggestion.Apply("Hello world!"));
        }

        [Fact]
        public void ExpandUnchanged()
        {
            var suggestion = new Suggestion<string>(1..1, "oo");
            Assert.Equal(suggestion, suggestion.Expand("f", 1..1));
        }

        [Fact]
        public void ExpandLeft()
        {
            var suggestion = new Suggestion<string>(1..1, "oo");
            Assert.Equal(new Suggestion<string>(0..1, "foo"), suggestion.Expand("f", 0..1));
        }

        [Fact]
        public void ExpandRight()
        {
            var suggestion = new Suggestion<string>(0..0, "minecraft:");
            Assert.Equal(new Suggestion<string>(0..4, "minecraft:fish"), suggestion.Expand("fish", 0..4));
        }

        [Fact]
        public void ExpandBoth()
        {
            var suggestion = new Suggestion<string>(11..11, "minecraft:");
            Assert.Equal(new Suggestion<string>(5..21, "Steve minecraft:fish_block"), suggestion.Expand("give Steve fish_block", 5..21));
        }

        [Fact]
        public void ExpandReplacement()
        {
            var suggestion = new Suggestion<string>(6..11, "strangers");
            Assert.Equal(new Suggestion<string>(0..12, "Hello strangers!"), suggestion.Expand("Hello world!", 0..12));
        }
    }
}