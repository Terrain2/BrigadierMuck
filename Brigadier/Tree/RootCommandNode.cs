using System;
using System.Threading.Tasks;

namespace Brigadier
{
    public class RootCommandNode : CommandNode
    {
        public override string Name { get; init; } = "";
        public override string Usage => "";
        public RootCommandNode() : base(null, s => true, null, c => new[] { c.Sender }, false) { }

        public override void Parse(StringScanner scanner, CommandContextBuilder builder) { }
        public override Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder) => Suggestions.Empty;
        protected override bool IsValidInput(string input) => false;

        public override BaseArgumentBuilder CreateBuilder() => throw new InvalidOperationException("Cannot convert root into a builder");
    }
}