using System;
using System.Text.RegularExpressions;
using Dawn;

namespace Brigadier.Arguments
{
    public class RegexArgumentType<T> : ArgumentType<T>, IEquatable<RegexArgumentType<T>>
    {
        public readonly Regex Regex;
        public RegexArgumentType(Regex regex)
        {
            Regex = regex;
        }

        public override T Parse(StringScanner scanner) => scanner.MatchParse<T>(Regex);

        public virtual bool Equals(RegexArgumentType<T> other) => Regex == other.Regex;

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is RegexArgumentType<T> regex) return Equals(regex);
            return false;
        }

        public override int GetHashCode() => Regex.GetHashCode();
    }
}