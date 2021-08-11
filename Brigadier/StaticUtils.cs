using System.Text.RegularExpressions;
using Brigadier.Arguments;

namespace Brigadier
{
    public static partial class StaticUtils
    {
        internal static readonly Regex IntegerRegex = new(@"^[+-]?\d+");
        internal static readonly Regex UnsignedIntegerRegex = new(@"^\+?\d+");
        // no Infinity, -Infinity, NaN
        internal static readonly Regex NumberRegex = new(@"^[+-?]?(\d+(\.\d*)?|\d*\.\d+)");

        public static LiteralArgumentBuilder Literal(string literal) => new(literal);
        public static RequiredArgumentBuilder<T> Argument<T>(string name, ArgumentType<T> type) => new(name, type);

        public static CommandDispatcher Dispatcher = new();

        public static partial class Arguments
        {
            public static BoolArgumentType Boolean() => new();
            public static StringArgumentType.Word Word() => new();
            public static StringArgumentType.Quotable String() => new();
            public static StringArgumentType.Greedy GreedyString() => new();

            public static MinMaxRegexArgumentType<sbyte> SByte(sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue) => new(min, max, IntegerRegex);
            public static MinMaxRegexArgumentType<byte> Byte(byte min = byte.MinValue, byte max = byte.MaxValue) => new(min, max, UnsignedIntegerRegex);
            public static MinMaxRegexArgumentType<short> Int16(short min = short.MinValue, short max = short.MaxValue) => new(min, max, IntegerRegex);
            public static MinMaxRegexArgumentType<ushort> UInt16(ushort min = ushort.MinValue, ushort max = ushort.MaxValue) => new(min, max, UnsignedIntegerRegex);
            public static MinMaxRegexArgumentType<int> Int32(int min = int.MinValue, int max = int.MaxValue) => new(min, max, IntegerRegex);
            public static MinMaxRegexArgumentType<uint> UInt32(uint min = uint.MinValue, uint max = uint.MaxValue) => new(min, max, UnsignedIntegerRegex);
            public static MinMaxRegexArgumentType<long> Int64(long min = long.MinValue, long max = long.MaxValue) => new(min, max, IntegerRegex);
            public static MinMaxRegexArgumentType<ulong> UInt64(ulong min = ulong.MinValue, ulong max = ulong.MaxValue) => new(min, max, UnsignedIntegerRegex);
            public static MinMaxRegexArgumentType<float> Single(float min = float.MinValue, float max = float.MaxValue) => new(min, max, NumberRegex);
            public static MinMaxRegexArgumentType<double> Double(double min = double.MinValue, double max = double.MaxValue) => new(min, max, NumberRegex);
            public static MinMaxRegexArgumentType<decimal> Decimal(decimal min = decimal.MinValue, decimal max = decimal.MaxValue) => new(min, max, NumberRegex);
        }
    }
}