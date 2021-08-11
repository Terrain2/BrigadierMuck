using System.Threading.Tasks;

namespace Brigadier.Arguments
{
    public class BoolArgumentType : ArgumentType<bool>
    {
        public override bool Parse(StringScanner scanner) => scanner.ReadBoolean();
        public override Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder)
        {
            if ("true".StartsWith(builder.RemainingLowercase)) builder.Suggest("true");
            if ("false".StartsWith(builder.RemainingLowercase)) builder.Suggest("false");
            return builder.BuildTask();
        }
    }
}