using System;
using System.Text;
using Dawn;

namespace Brigadier
{
    public abstract record Suggestion(Range Range, string Text, string Tooltip) : IComparable<Suggestion>
    {
        // public readonly Range Range;
        // public readonly string Text;
        // public readonly string Tooltip;
        // protected Suggestion(Range range, string text, string tooltip = null)
        // {
        //     Range = range;
        //     Text = text;
        //     Tooltip = tooltip;
        // }

        public string Apply(string input)
        {
            var start = Range.Start.GetOffset(input.Length);
            var end = Range.End.GetOffset(input.Length);
            if (start == 0 && end == input.Length) return Text;

            var result = new StringBuilder();

            if (start > 0) result.Append(input[0..start]);
            result.Append(Text);
            if (end < input.Length) result.Append(input[end..]);

            return result.ToString();
        }

        public Suggestion Expand(string command, Range range)
        {
            if (Range.Equals(range)) return this;
#pragma warning disable IDE0042 // deconstruct tuple
            var mine = Range.Normalized(command.Length);
            var other = range.Normalized(command.Length);
#pragma warning restore IDE0042
            var result = new StringBuilder();
            if (other.Start < mine.Start)
            {
                result.Append(command[other.Start..mine.Start]);
            }
            result.Append(Text);
            if (other.End > mine.End)
            {
                result.Append(command[mine.End..other.End]);
            }
            return new Suggestion<string>(range, result.ToString(), Tooltip);
        }

        public virtual int CompareTo(Suggestion other) => Text.CompareTo(other.Text);

        // public virtual bool Equals(Suggestion other) => Range.Equals(other.Range) && Text == other.Text && Tooltip == other.Tooltip;

        // public override bool Equals(object other)
        // {
        //     if (ReferenceEquals(this, other)) return true;
        //     if (other is Suggestion suggestion) return Equals(suggestion);
        //     return false;
        // }

        // public override int GetHashCode() => (Range, Text, Tooltip).GetHashCode();

        // public static bool operator ==(Suggestion lhs, Suggestion rhs) => Equals(lhs, rhs);
        // public static bool operator !=(Suggestion lhs, Suggestion rhs) => !Equals(lhs, rhs);
    }

    public record Suggestion<T>(Range Range, T Value, string Text, string Tooltip) : Suggestion(Range, Text, Tooltip), IComparable<Suggestion<T>> where T : IComparable<T>, IEquatable<T>
    {
        // public readonly T Value;

        // internal Suggestion(Range range, T value, string text, string tooltip) : base(range, text, tooltip)
        // {
        //     Value = value;
        // }

        public Suggestion(Range range, T value, string tooltip = null) : this(range, value, ValueString.Of(value).ToString(), tooltip) { }

        public int CompareTo(Suggestion<T> other) => Value.CompareTo(other.Value);

        // public bool Equals(Suggestion<T> other) => Range.Equals(other.Range) && Value.Equals(other.Value) && Tooltip == other.Tooltip;

        public override int CompareTo(Suggestion other)
        {
            if (other is Suggestion<T> suggestion) return CompareTo(suggestion);
            return base.CompareTo(other);
        }

        // public override bool Equals(Suggestion other) {
        //     if (other is Suggestion<T> suggestion) return Equals(suggestion);
        //     return base.Equals(other);
        // }

        // public override bool Equals(object other)
        // {
        //     if (ReferenceEquals(this, other)) return true;
        //     if (other is Suggestion<T> suggestion) return Equals(suggestion);
        //     return false;
        // }

        // public override int GetHashCode() => (Range, Value, Tooltip).GetHashCode();
    }
}