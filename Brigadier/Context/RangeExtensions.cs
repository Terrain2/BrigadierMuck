using System;

namespace Brigadier
{
    public static class RangeExtensions
    {
        public static bool IsEmpty(this Range range) => range.Start.Equals(range.End);
        public static (int Start, int End) Normalized(this Range range, int length) => (range.Start.GetOffset(length), range.End.GetOffset(length));
    }
}