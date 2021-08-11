using System;
using System.Threading.Tasks;

namespace Brigadier
{
    public class LiteralCommandNode : CommandNode
    {
        public override string Name { get; init; }
        private readonly string NameLowercase;
        public override string Usage => Name;
        public LiteralCommandNode(string literal, Command command, Predicate<CommandSender> requirement, CommandNode redirect, RedirectModifier modifier, bool forks)
            : base(command, requirement, redirect, modifier, forks)
        {
            Name = literal;
            NameLowercase = literal.ToLower();
        }

        public override void Parse(StringScanner scanner, CommandContextBuilder builder)
        {
            scanner.ParseStack.Push(scanner.Cursor);
            var end = Parse(scanner);
            if (end == -1) throw scanner.MakeException($"Expected literal {Name}");
            builder.AddNode(this, scanner.ParseStack.Pop()..end);
        }

        private int Parse(StringScanner scanner)
        {
            if (scanner.CanRead(Name.Length) && scanner.Read(Name.Length) == Name)
            {
                if (!scanner.CanRead() || char.IsWhiteSpace(scanner.Next)) return scanner.Cursor;
            }
            return -1;
        }

        protected override bool IsValidInput(string input) => Parse(new StringScanner(input)) != -1;
        public override Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder)
        {
            if (NameLowercase.StartsWith(builder.RemainingLowercase)) return builder.Suggest(Name).BuildTask();
            return Suggestions.Empty;
        }

        public override BaseArgumentBuilder CreateBuilder() =>
            new LiteralArgumentBuilder(Name)
                .Requires(Requirement)
                .Forward(Redirect, RedirectModifier, Forks)
                .ExecutesRaw(Command);
    }
}