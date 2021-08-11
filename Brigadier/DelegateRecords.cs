using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brigadier
{
    public delegate void Command(CommandContext context);
    public delegate Task<Suggestions> SuggestionProvider(CommandContext context, SuggestionsBuilder builder);
    public delegate CommandSender SingleRedirectModifier(CommandContext context);
    public delegate IEnumerable<CommandSender> RedirectModifier(CommandContext context);

    public abstract record ParsedArgument(Range Range);
    public record ParsedArgument<T>(Range Range, T Result) : ParsedArgument(Range);
    public record ParsedCommandNode(CommandNode Node, Range Range);
    public record ParseResults(CommandContextBuilder Context, StringScanner Scanner, Dictionary<CommandNode, CommandSyntaxException> Exceptions);
    public record SuggestionContext(CommandNode Parent, Index Start);
}