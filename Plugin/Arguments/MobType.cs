using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using static Brigadier.StaticUtils;

namespace Brigadier.Arguments
{
    public class MobTypeArgument : ArgumentType<MobType>
    {
        public override MobType Parse(StringScanner scanner)
        {
            scanner.ParseStack.Push(scanner.Cursor);
            if (scanner.CanRead() && scanner.Next == '#')
            {
                scanner.Skip();
                var id = scanner.MatchParse<int>(IntegerRegex);
                if (id >= 0 && id < MobSpawner.Instance.allMobs.Length)
                {
                    var mob = MobSpawner.Instance.allMobs[id];
                    scanner.ParseStack.Pop();
                    return mob;
                }
                else
                {
                    throw scanner.MakeException("Unknown mob ID");
                }
            }

            MobType result = null;
            int max = 0;
            foreach (var mob in MobSpawner.Instance.allMobs)
            {
                var name = ((Object)mob).name;
                if (name.Length > max && scanner.CanRead(name.Length) && scanner.Read(name.Length) == name)
                {
                    result = mob;
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
            throw scanner.MakeException("Unknown mob");
        }

        public override Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder)
        {
            if (builder.Remaining.StartsWith("#"))
            {
                foreach (var mob in MobSpawner.Instance.allMobs.ToHashSet())
                {
                    if ($"#{mob.id}".StartsWith(builder.RemainingLowercase))
                    {
                        builder.Suggest(mob.id, $"#{mob.id}");
                    }
                }
                return builder.BuildTask();
            }
            if (builder.Remaining.IsNullOrWhiteSpace())
            {
                builder.Suggest("#");
            }
            foreach (var mob in MobSpawner.Instance.allMobs.ToHashSet())
            {
                var name = ((ScriptableObject)mob).name;
                if (name.ToLower().StartsWith(builder.RemainingLowercase))
                {
                    builder.Suggest(name);
                }
            }
            return builder.BuildTask();
        }
    }
}