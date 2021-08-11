using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brigadier
{
    public class Suggestions
    {
        public readonly Range Range;
        public readonly List<Suggestion> List;

        public Suggestions(Range range, List<Suggestion> suggestions)
        {
            Range = range;
            List = suggestions;
        }
        public Suggestions(string command, IEnumerable<Suggestion> suggestions)
        {
            if (!suggestions.Any())
            {
                Range = 0..0;
                List = new List<Suggestion>();
                return;
            }
            var start = int.MaxValue;
            var end = int.MinValue;
            foreach (var suggestion in suggestions)
            {
                start = Math.Min(suggestion.Range.Start.GetOffset(command.Length), start);
                end = Math.Max(suggestion.Range.End.GetOffset(command.Length), end);
            }
            Range = start..end;

            List = suggestions.Select(suggestion => suggestion.Expand(command, Range)).ToList();
            try
            {
                // non-transitive sorting by value, and string if types mismatch
                // 3 < 11
                // 11 < "2"
                // "2" < 3
                // no correct order (may still sort)
                List.Sort();
            }
            catch
            {
                // transitive fallback sorting by type then value
                // 3 < 11
                // 11 < "2" ("Int32" < "String")
                // 3 < "2" ("Int32" < "String")
                // correct order is 3, 11, "2"
                List.Sort((a, b) =>
                {
                    var atype = a.GetType();
                    var btype = b.GetType();
                    if (atype == btype) return a.CompareTo(b);
                    return atype.Name.CompareTo(btype.Name);
                });
            }
        }

        public static readonly Suggestions EmptySync = new(0..0, new List<Suggestion>());
        public static Task<Suggestions> Empty => Task.FromResult(EmptySync);

        public static Suggestions Merge(string command, IEnumerable<Suggestions> input)
        {
            if (!input.Any()) return EmptySync;
            if (input.Count() == 1) return input.Single();

            var texts = new HashSet<Suggestion>();
            foreach (var suggestions in input) texts = texts.Concat(suggestions.List).ToHashSet();

            return new Suggestions(command, texts);
        }

    }
}