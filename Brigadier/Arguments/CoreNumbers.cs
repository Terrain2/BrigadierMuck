// namespace Brigadier.Arguments
// {
//     public abstract class BaseNumberArgument<T> : ArgumentType<T>
//     {
//         public readonly T Min;
//         public readonly T Max;

//         protected BaseNumberArgument(T min, T max)
//         {
//             Min = min;
//             Max = max;
//         }

//         public override bool Equals(object obj)
//         {
//             if (ReferenceEquals(this, obj)) return true;
//             if (GetType() != obj.GetType()) return false;
//             var arg = obj as BaseNumberArgument<T>;
//             return Equals(Min, arg.Min) && Equals(Max, arg.Max);
//         }

//         public override int GetHashCode() => (Min, Max).GetHashCode();

//         public static bool operator ==(BaseNumberArgument<T> lhs, BaseNumberArgument<T> rhs) => Equals(lhs, rhs);
//         public static bool operator !=(BaseNumberArgument<T> lhs, BaseNumberArgument<T> rhs) => !Equals(lhs, rhs);
//     }
//     public class SByteArgumentType : BaseNumberArgument<sbyte>
//     {
//         public SByteArgumentType(sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue) : base(min, max) { }
//         public override sbyte Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<sbyte>(sbyte.TryParse);
//             if (result < Min) throw scanner.MakeException($"SByte must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"SByte must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class ByteArgumentType : BaseNumberArgument<byte>
//     {
//         public ByteArgumentType(byte min = byte.MinValue, byte max = byte.MaxValue) : base(min, max) { }
//         public override byte Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<byte>(byte.TryParse);
//             if (result < Min) throw scanner.MakeException($"Byte must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"Byte must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class Int16ArgumentType : BaseNumberArgument<short>
//     {
//         public Int16ArgumentType(short min = short.MinValue, short max = short.MaxValue) : base(min, max) { }
//         public override short Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<short>(short.TryParse);
//             if (result < Min) throw scanner.MakeException($"Int16 must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"Int16 must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class UInt16ArgumentType : BaseNumberArgument<ushort>
//     {
//         public UInt16ArgumentType(ushort min = ushort.MinValue, ushort max = ushort.MaxValue) : base(min, max) { }
//         public override ushort Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<ushort>(ushort.TryParse);
//             if (result < Min) throw scanner.MakeException($"UInt16 must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"UInt16 must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class Int32ArgumentType : BaseNumberArgument<int>
//     {
//         public Int32ArgumentType(int min = int.MinValue, int max = int.MaxValue) : base(min, max) { }
//         public override int Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<int>(int.TryParse);
//             if (result < Min) throw scanner.MakeException($"Int32 must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"Int32 must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class UInt32ArgumentType : BaseNumberArgument<uint>
//     {
//         public UInt32ArgumentType(uint min = uint.MinValue, uint max = uint.MaxValue) : base(min, max) { }
//         public override uint Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<uint>(uint.TryParse);
//             if (result < Min) throw scanner.MakeException($"UInt32 must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"UInt32 must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class Int64ArgumentType : BaseNumberArgument<long>
//     {
//         public Int64ArgumentType(long min = long.MinValue, long max = long.MaxValue) : base(min, max) { }
//         public override long Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<long>(long.TryParse);
//             if (result < Min) throw scanner.MakeException($"Int64 must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"Int64 must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class UInt64ArgumentType : BaseNumberArgument<ulong>
//     {
//         public UInt64ArgumentType(ulong min = ulong.MinValue, ulong max = ulong.MaxValue) : base(min, max) { }
//         public override ulong Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<ulong>(ulong.TryParse);
//             if (result < Min) throw scanner.MakeException($"UInt64 must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"UInt64 must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class SingleArgumentType : BaseNumberArgument<float>
//     {
//         public SingleArgumentType(float min = float.MinValue, float max = float.MaxValue) : base(min, max) { }
//         public override float Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<float>(float.TryParse);
//             if (result < Min) throw scanner.MakeException($"undefined must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"undefined must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class DoubleArgumentType : BaseNumberArgument<double>
//     {
//         public DoubleArgumentType(double min = double.MinValue, double max = double.MaxValue) : base(min, max) { }
//         public override double Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<double>(double.TryParse);
//             if (result < Min) throw scanner.MakeException($"undefined must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"undefined must not be more than {Max}, found {result}");
//             return result;
//         }
//     }

//     public class DecimalArgumentType : BaseNumberArgument<decimal>
//     {
//         public DecimalArgumentType(decimal min = decimal.MinValue, decimal max = decimal.MaxValue) : base(min, max) { }
//         public override decimal Parse(StringScanner scanner)
//         {
//             var result = scanner.ReadNumber<decimal>(decimal.TryParse);
//             if (result < Min) throw scanner.MakeException($"undefined must not be less than {Min}, found {result}");
//             if (result > Max) throw scanner.MakeException($"undefined must not be more than {Max}, found {result}");
//             return result;
//         }
//     }
// }