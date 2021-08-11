using System;
using System.Text.RegularExpressions;
using Dawn;

namespace Brigadier.Arguments
{
    public class MinMaxRegexArgumentType<T> : RegexArgumentType<T> where T : IComparable<T>, IEquatable<T>
    {
        public T Min;
        public T Max;

        public MinMaxRegexArgumentType(T min, T max, Regex regex) : base(regex)
        {
            Min = min;
            Max = max;
        }

        public override bool Equals(RegexArgumentType<T> other)
        {
            if (other is MinMaxRegexArgumentType<T> minmax) return Equals(minmax);
            return false;
        }
        public bool Equals(MinMaxRegexArgumentType<T> other) => Regex == other.Regex && Min.Equals(other.Min) && Max.Equals(other.Max);

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is MinMaxRegexArgumentType<T> regex) return Equals(regex);
            return false;
        }

        public override int GetHashCode() => (Regex, Min, Max).GetHashCode();

        public override T Parse(StringScanner scanner)
        {
            var value = scanner.MatchParse<T>(Regex);
            scanner.ParseStack.Push(scanner.Cursor);
            if (value.CompareTo(Min) < 0) throw scanner.MakeException($"{typeof(T).Name} must not be less than {Min}, found {value}");
            if (value.CompareTo(Max) > 0) throw scanner.MakeException($"{typeof(T).Name} must not be more than {Max}, found {value}");
            scanner.ParseStack.Pop();
            return value;
        }
    }
}