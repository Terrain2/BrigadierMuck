using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;

namespace Brigadier
{
    public class SuggestionsBuilder
    {
        public readonly string Input;
        private readonly string InputLowercase;
        public readonly int Start;
        public readonly string Remaining;
        public readonly string RemainingLowercase;
        public List<Suggestion> Result { get; private set; } = new List<Suggestion>();

        public SuggestionsBuilder(string input, string inputLowercase, int start)
        {
            Input = input;
            InputLowercase = inputLowercase;
            Start = start;
            Remaining = input[start..];
            RemainingLowercase = inputLowercase[start..];
        }

        public SuggestionsBuilder(string input, int start) : this(input, input.ToLower(), start) { }

        public Suggestions Build() => new(Input, Result);
        public Task<Suggestions> BuildTask() => Task.FromResult(Build());

        public SuggestionsBuilder Suggest<T>(T value, string text = null, string tooltip = null) where T : IComparable<T>, IEquatable<T>
        {
            text ??= ValueString.Of(value).ToString();
            if (text == Remaining) return this;
            Result.Add(new Suggestion<T>(Start..Input.Length, value, text, tooltip));
            return this;
        }

        public void Add(SuggestionsBuilder other)
        {
            Result = Result.Concat(other.Result).ToList();
        }

        public SuggestionsBuilder CreateOffset(int start) => new(Input, InputLowercase, start);
        public SuggestionsBuilder Restart() => CreateOffset(Start);
    }
}