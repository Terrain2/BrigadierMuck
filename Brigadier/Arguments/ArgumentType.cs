using System;
using System.Threading.Tasks;

namespace Brigadier.Arguments
{
    public abstract class ArgumentType<T>
    {
        public abstract T Parse(StringScanner scanner);
        public virtual Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder) => Suggestions.Empty;
    }
}