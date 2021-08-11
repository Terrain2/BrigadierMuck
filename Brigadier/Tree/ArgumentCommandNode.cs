using System;
using System.Threading.Tasks;
using Brigadier.Arguments;

namespace Brigadier
{
    public abstract class ArgumentCommandNode : CommandNode
    {
        public override string Name { get; init; }
        public override string Usage => $"<{Name}>";
        public readonly SuggestionProvider CustomSuggestions;
        protected ArgumentCommandNode(string name, Command command, Predicate<CommandSender> requirement, CommandNode redirect, RedirectModifier modifier, bool forks, SuggestionProvider customSuggestions)
            : base(command, requirement, redirect, modifier, forks)
        {
            Name = name;
            CustomSuggestions = customSuggestions;
        }

    }
    public class ArgumentCommandNode<T> : ArgumentCommandNode
    {
        public readonly ArgumentType<T> type;
        public ArgumentCommandNode(string name, ArgumentType<T> type, Command command, Predicate<CommandSender> requirement, CommandNode redirect, RedirectModifier modifier, bool forks, SuggestionProvider customSuggestions)
            : base(name, command, requirement, redirect, modifier, forks, customSuggestions)
        {
            this.type = type;
        }

        public override Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder)
        {
            if (CustomSuggestions == null) return type.ListSuggestions(context, builder);
            return CustomSuggestions(context, builder);
        }

        public override void Parse(StringScanner scanner, CommandContextBuilder builder)
        {
            var start = scanner.Cursor;
            var result = type.Parse(scanner);
            var parsed = new ParsedArgument<T>(start..scanner.Cursor, result);
            builder.Arguments.Add(Name, parsed);
            builder.AddNode(this, parsed.Range);
        }

        protected override bool IsValidInput(string input)
        {
            try
            {
                var scanner = new StringScanner(input);
                type.Parse(scanner);
                return !scanner.CanRead() || char.IsWhiteSpace(scanner.Next);
            }
            catch (CommandSyntaxException)
            {
                return false;
            }
        }

        public override BaseArgumentBuilder CreateBuilder() => new RequiredArgumentBuilder<T>(Name, type)
            .Requires(Requirement)
            .Forward(Redirect, RedirectModifier, Forks)
            .Suggests(CustomSuggestions)
            .ExecutesRaw(Command);
    }
}