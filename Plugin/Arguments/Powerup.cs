using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using static Brigadier.StaticUtils;

namespace Brigadier.Arguments
{
    public class PowerupArgument : ArgumentType<Powerup>
    {
        public override Powerup Parse(StringScanner scanner)
        {
            scanner.ParseStack.Push(scanner.Cursor);
            if (scanner.CanRead() && scanner.Next == '#')
            {
                scanner.Skip();
                if (ItemManager.Instance.allPowerups.TryGetValue(scanner.MatchParse<int>(IntegerRegex), out var powerup))
                {
                    scanner.ParseStack.Pop();
                    return powerup;
                }
                else
                {
                    throw scanner.MakeException("Unknown powerup ID");
                }
            }

            Powerup result = null;
            int max = 0;
            foreach (var powerup in ItemManager.Instance.allPowerups.Values)
            {
                var name = ((Object)powerup).name;
                if (name.Length > max && scanner.CanRead(name.Length) && scanner.Read(name.Length) == name)
                {
                    result = powerup;
                    max = name.Length;
                }
                scanner.Cursor = scanner.ParseStack.Peek();
            }
            if (result != null)
            {
                scanner.ParseStack.Pop();
                scanner.Skip(max);
                return result;
            }
            throw scanner.MakeException("Unknown powerup");
        }

        public override Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder)
        {
            if (builder.Remaining.StartsWith("#"))
            {
                foreach (var powerup in ItemManager.Instance.allPowerups.Values)
                {
                    if ($"#{powerup.id}".StartsWith(builder.RemainingLowercase))
                    {
                        builder.Suggest(powerup.id, $"#{powerup.id}");
                    }
                }
                return builder.BuildTask();
            }
            if (builder.Remaining.IsNullOrWhiteSpace())
            {
                builder.Suggest("#");
            }
            foreach (var powerup in ItemManager.Instance.allPowerups.Values)
            {
                var name = ((ScriptableObject)powerup).name;
                if (name.ToLower().StartsWith(builder.RemainingLowercase))
                {
                    builder.Suggest(name);
                }
            }
            return builder.BuildTask();
        }
    }
}