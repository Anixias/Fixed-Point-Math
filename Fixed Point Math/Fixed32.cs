using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Numerics;

namespace FixedPointMath;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
public readonly struct Fixed32(int rawValue) : IFixedPoint<Fixed32>
{
	[StructLayout(LayoutKind.Explicit)]
	private readonly struct ToUnsigned(int sourceValue)
	{
		[FieldOffset(0)] public readonly int sourceValue = sourceValue;
		[FieldOffset(0)] public readonly uint castedValue;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	private readonly struct ToSigned(uint sourceValue)
	{
		[FieldOffset(0)] public readonly uint sourceValue = sourceValue;
		[FieldOffset(0)] public readonly int castedValue;
	}
	
	public static Fixed32 Epsilon { get; } = new(1);
	public static Fixed32 MaxValue { get; } = new(int.MaxValue);
	public static Fixed32 MinValue { get; } = new(int.MinValue);
	public static Fixed32 Zero { get; } = new(0);
	public static Fixed32 One { get; } = new(RawOne);
	public static Fixed32 NegativeOne { get; } = new(RawNegativeOne);
	public static Fixed32 Half { get; } = new(RawHalf);
	public static Fixed32 E { get; } = new(RawE);
	public static Fixed32 Pi { get; } = new(RawPi);
	public static Fixed32 PiOver2 { get; } = new(RawPiOver2);
	public static Fixed32 Tau { get; } = new(RawTau);
	public static Fixed32 Ln2 { get; } = new(RawLn2);
	
	private static readonly Fixed32 _log2Max = (Fixed32)(BitCount - DecimalPlaces - 1);
	private static readonly Fixed32 _log2Min = (Fixed32)(DecimalPlaces - BitCount);
	private static readonly Fixed32 _degToRadConstant = Pi / (Fixed32)180;
	private static readonly Fixed32 _radToDegConstant = (Fixed32)180 / Pi;
	
	private const int BitCount = 32;
	internal const int DecimalPlaces = 16;
	private const int RawOne = 1 << DecimalPlaces;
	private const int RawHalf = 0x8000;
	private const int RawNegativeOne = -(1 << DecimalPlaces);
	private const int RawPi = 0x3_243F;
	private const int RawE = 0x2_B7E1;
	private const int RawPiOver2 = 0x1_9220;
	private const int RawTau = 0x6_487F;
	private const int RawLn2 = 0x0_B172;
	
	public int RawValue { get; } = rawValue;
	public uint Bits => new ToUnsigned(RawValue).castedValue;
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsZero(Fixed32 value)
	{
		return value.RawValue == 0;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsZero()
	{
		return IsZero(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(Fixed32 value)
	{
		return value.RawValue < 0;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsNegative()
	{
		return IsNegative(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOddInteger(Fixed32 value)
	{
		return IsInteger(value) && ((value.RawValue >> DecimalPlaces) & 1) == 1;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEvenInteger(Fixed32 value)
	{
		return IsInteger(value) && ((value.RawValue >> DecimalPlaces) & 1) == 0;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(Fixed32 value)
	{
		return value.RawValue > 0;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsPositive()
	{
		return IsPositive(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInteger(Fixed32 value)
	{
		return Fract(value) == Zero;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInteger()
	{
		return IsInteger(this);
	}
	
	public static Fixed32 DegToRad(Fixed32 value)
	{
		return value * _degToRadConstant;
	}
	
	public Fixed32 DegToRad() => DegToRad(this);
	
	public static Fixed32 RadToDeg(Fixed32 value)
	{
		return value * _radToDegConstant;
	}
	
	public Fixed32 RadToDeg() => RadToDeg(this);
	
	public static Fixed32 Lerp(Fixed32 a, Fixed32 b, Fixed32 t)
	{
		return a + t * (b - a);
	}
	
	public Fixed32 Lerp(Fixed32 target, Fixed32 weight) => Lerp(this, target, weight);
	
	public static int Sign(Fixed32 value)
	{
		return
			value.RawValue < 0 ? -1 :
			value.RawValue > 0 ? 1 :
			0;
	}
	
	public int Sign() => Sign(this);
	
	public static Fixed32 Abs(Fixed32 value)
	{
		if (value == MinValue)
			return MaxValue;
		
		var mask = value.RawValue >> (BitCount - 1);
		return new Fixed32((value.RawValue + mask) ^ mask);
	}
	
	public Fixed32 Abs() => Abs(this);
	
	public static Fixed32 Floor(Fixed32 value)
	{
		var rawValue = new ToUnsigned(value.RawValue).castedValue;
		var flooredValue = new ToSigned(rawValue & 0xFFFF_0000u).castedValue;
		return new Fixed32(flooredValue);
	}
	
	public Fixed32 Floor() => Floor(this);
	
	public static Fixed32 Ceil(Fixed32 value)
	{
		var hasDecimalPart = (value.RawValue & 0x0000_FFFF) != 0L;
		return hasDecimalPart ? Floor(value) + One : value;
	}
	
	public Fixed32 Ceil() => Ceil(this);
	
	public static Fixed32 Fract(Fixed32 value)
	{
		return new Fixed32(value.RawValue & 0x0000_FFFF);
	}
	
	public Fixed32 Fract() => Fract(this);
	
	public static Fixed32 Round(Fixed32 value)
	{
		var decimalPart = value.RawValue & 0x0000_FFFF;
		var integerPart = Floor(value);
		
		if (decimalPart < 0x8000)
			return integerPart;
		
		if (decimalPart > 0x8000)
			return integerPart + One;
		
		return (integerPart.RawValue & RawOne) == 0
			? integerPart
			: integerPart + One;
	}
	
	public Fixed32 Round() => Round(this);
	
	public static Fixed32 Clamp(Fixed32 value, Fixed32 min, Fixed32 max)
	{
		if (value < min)
			return min;
		
		if (value > max)
			return max;
		
		return value;
	}
	
	public Fixed32 Clamp(Fixed32 min, Fixed32 max) => Clamp(this, min, max);
	
	public static Fixed32 Min(Fixed32 a, Fixed32 b)
	{
		return a < b ? a : b;
	}
	
	public static Fixed32 Max(Fixed32 a, Fixed32 b)
	{
		return a > b ? a : b;
	}
	
	public static Fixed32 PosMod(Fixed32 a, Fixed32 b)
	{
		var result = a % b;
		
		if (a.RawValue < 0 && b.RawValue > 0 || result.RawValue > 0 && b.RawValue < 0)
			result += b;
		
		return result;
	}
	
	public Fixed32 PosMod(Fixed32 divisor) => PosMod(this, divisor);
	
	public static int FractionalBits => DecimalPlaces;
	
	public Int128 RawBits => RawValue;
	
	public static Fixed32 FromRawBits(Int128 rawBits) =>
		rawBits > int.MaxValue ? MaxValue :
		rawBits < int.MinValue ? MinValue : new Fixed32((int)rawBits);
	
	public static Fixed32 Parse(string number) => FixedMath.Parse<Fixed32>(number);
	
	public static bool TryParse(string number, out Fixed32 result) => FixedMath.TryParse(number, out result);
	
	public int ToInt() => (int)this;
	public long ToLong() => (long)this;
	
	public static Fixed32 Sin(Fixed32 value) => FixedMath.Sin(value);
	
	public static Fixed32 Cos(Fixed32 value) => FixedMath.Cos(value);
	
	public static (Fixed32 sin, Fixed32 cos) SinCos(Fixed32 angle) => FixedMath.SinCos(angle);
	
	public (Fixed32 sin, Fixed32 cos) SinCos() => SinCos(this);
	
	public static Fixed32 Snapped(Fixed32 value, Fixed32 step)
	{
		return step.RawValue != 0 ? Floor(value / step + Half) * step : value;
	}
	
	public Fixed32 Snapped(Fixed32 step) => Snapped(this, step);
	
	public static Fixed32 operator +(Fixed32 left, Fixed32 right)
	{
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		var sum = leftRaw + rightRaw;
		
		if ((~(leftRaw ^ rightRaw) & (leftRaw ^ sum) & int.MinValue) != 0)
			sum = leftRaw > 0 ? int.MaxValue : int.MinValue;
		
		return new Fixed32(sum);
	}
	
	public static Fixed32 operator -(Fixed32 left, Fixed32 right)
	{
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		var difference = leftRaw - rightRaw;
		
		if (((leftRaw ^ rightRaw) & (leftRaw ^ difference) & int.MinValue) != 0)
			difference = leftRaw < 0 ? int.MinValue : int.MaxValue;
		
		return new Fixed32(difference);
	}
	
	private const uint MultiplyRounding = 1u << (DecimalPlaces - 1);
	
	private static int AddOverflow(int left, int right, ref bool overflow)
	{
		var sum = left + right;
		overflow |= ((left ^ right ^ sum) & int.MinValue) != 0;
		return sum;
	}
	
	public static Fixed32 operator *(Fixed32 left, Fixed32 right)
	{
		if (left == One)
			return right;
		
		if (right == One)
			return left;
		
		if (left == NegativeOne)
			return -right;
		
		if (right == NegativeOne)
			return -left;
		
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		
		if (leftRaw == 0 || rightRaw == 0)
			return Zero;
		
		var leftLow = leftRaw & 0x0000_FFFF;
		var leftHigh = leftRaw >> DecimalPlaces;
		var rightLow = rightRaw & 0x0000_FFFF;
		var rightHigh = rightRaw >> DecimalPlaces;
		
		var leftLowCast = new ToUnsigned(leftLow);
		var rightLowCast = new ToUnsigned(rightLow);
		
		var lowLow = leftLowCast.castedValue * rightLowCast.castedValue;
		var lowHigh = leftLow * rightHigh;
		var highLow = leftHigh * rightLow;
		var highHigh = leftHigh * rightHigh;
		
		var lowResult = new ToSigned((lowLow + MultiplyRounding) >> DecimalPlaces).castedValue;
		var highResult = highHigh << DecimalPlaces;
		
		var overflow = false;
		var sum = AddOverflow(lowResult, lowHigh, ref overflow);
		sum = AddOverflow(sum, highLow, ref overflow);
		sum = AddOverflow(sum, highResult, ref overflow);
		
		var opSignsEqual = ((leftRaw ^ rightRaw) & int.MinValue) == 0;
		
		if (opSignsEqual)
		{
			if (sum < 0 || (overflow && leftRaw > 0))
				return MaxValue;
		}
		else
		{
			if (sum > 0)
				return MinValue;
		}
		
		var topCarry = highHigh >> DecimalPlaces;
		if (topCarry != 0 && topCarry != -1)
			return opSignsEqual ? MaxValue : MinValue;
		
		if (opSignsEqual)
			return new Fixed32(sum);
		
		int posOp, negOp;
		if (leftRaw > rightRaw)
		{
			posOp = leftRaw;
			negOp = rightRaw;
		}
		else
		{
			posOp = rightRaw;
			negOp = leftRaw;
		}
		
		if (sum > negOp && negOp < -RawOne && posOp > RawOne)
			return MinValue;
		
		return new Fixed32(sum);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int CountLeadingZeroes(uint value)
	{
		var result = 0;
		
		while ((value & 0xF000_0000u) == 0)
		{
			result += 4;
			value <<= 4;
		}
		
		while ((value & 0x8000_0000u) == 0)
		{
			result += 1;
			value <<= 1;
		}
		
		return result;
	}
	
	public static Fixed32 operator /(Fixed32 left, Fixed32 right)
	{
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		
		if (rightRaw == 0L)
			throw new DivideByZeroException();
		
		if (right == (Fixed32)2)
			return new Fixed32(leftRaw >> 1);
		
		var remainder = new ToUnsigned(leftRaw >= 0 ? leftRaw : -leftRaw).castedValue;
		var divisor = new ToUnsigned(rightRaw >= 0 ? rightRaw : -rightRaw).castedValue;
		var quotient = 0u;
		var bitPos = BitCount / 2 + 1;
		
		while ((divisor & 0xFu) == 0u && bitPos >= 4)
		{
			divisor >>= 4;
			bitPos -= 4;
		}
		
		while (remainder != 0 && bitPos >= 0)
		{
			var shift = CountLeadingZeroes(remainder);
			if (shift > bitPos)
				shift = bitPos;
			
			remainder <<= shift;
			bitPos -= shift;
			
			var division = remainder / divisor;
			remainder %= divisor;
			quotient += division << bitPos;
			
			if ((division & ~(uint.MaxValue >> bitPos)) != 0u)
				return ((leftRaw ^ rightRaw) & int.MinValue) == 0 ? MaxValue : MinValue;
			
			remainder <<= 1;
			bitPos--;
		}
		
		quotient++;
		var result = (int)(quotient >> 1);
		if (((leftRaw ^ rightRaw) & int.MinValue) != 0)
			result = -result;
		
		return new Fixed32(result);
	}
	
	public static Fixed32 operator %(Fixed32 left, Fixed32 right)
	{
		return new Fixed32(left.RawValue == int.MinValue & right.RawValue == -1 ? 0 : left.RawValue % right.RawValue);
	}
	
	private static Fixed32 Pow2(Fixed32 exponent)
	{
		if (exponent.RawValue == 0)
			return One;
		
		var negative = exponent.RawValue < 0;
		if (negative)
			exponent = -exponent;
		
		if (exponent == One)
			return negative ? Half : (Fixed32)2;
		
		if (exponent >= _log2Max)
			return negative ? One / MaxValue : MaxValue;
		
		if (exponent <= _log2Min)
			return negative ? MaxValue : Zero;
		
		var integerPart = (int)Floor(exponent);
		exponent = Fract(exponent);
		
		var result = One;
		var term = One;
		var i = 1;
		
		while (term.RawValue != 0)
		{
			term = exponent * term * Ln2 / (Fixed32)i;
			result += term;
			i++;
		}
		
		result = new Fixed32(result.RawValue << integerPart);
		if (negative)
			result = One / result;
		
		return result;
	}
	
	public static Fixed32 Log2(Fixed32 value)
	{
		if (!IsPositive(value))
			throw new ArgumentOutOfRangeException(nameof(value));
		
		var b = 1 << (DecimalPlaces - 1);
		var y = 0;
		
		var rawValue = value.RawValue;
		while (rawValue < RawOne)
		{
			rawValue <<= 1;
			y -= RawOne;
		}
		
		while (rawValue >= RawOne << 1)
		{
			rawValue >>= 1;
			y += RawOne;
		}
		
		var z = new Fixed32(rawValue);
		
		for (var i = 0; i < DecimalPlaces; i++)
		{
			z *= z;
			if (z.RawValue >= RawOne << 1)
			{
				z = new Fixed32(z.RawValue >> 1);
				y += b;
			}
			
			b >>= 1;
		}
		
		return new Fixed32(y);
	}
	
	public static Fixed32 Ln(Fixed32 value)
	{
		return Log2(value) * Ln2;
	}
	
	public static Fixed32 Pow(Fixed32 @base, Fixed32 exponent)
	{
		if (@base < Zero)
		{
			// Todo: Handle properly
			if (!exponent.IsInteger())
				return Zero;
			
			var pow = Pow(-@base, exponent);
			if (exponent % (Fixed32)2 == Zero)
				return pow;
			
			return -pow;
		}
		
		if (@base == One)
			return One;
		
		if (exponent == Zero)
			return One;
		
		if (@base == Zero)
		{
			if (exponent < Zero)
				throw new DivideByZeroException();
			
			return Zero;
		}
		
		var log2 = Log2(@base);
		return Pow2(exponent * log2);
	}
	
	public static Fixed32 Sqrt(Fixed32 value)
	{
		var rawValue = value.RawValue;
		if (rawValue < 0)
			throw new ArgumentOutOfRangeException(nameof(value));
		
		var number = new ToUnsigned(rawValue).castedValue;
		var result = 0u;
		var bit = 1u << (BitCount - 2);
		
		while (bit > number)
		{
			bit >>= 2;
		}
		
		for (var i = 0; i < 2; i++)
		{
			while (bit != 0u)
			{
				if (number >= result + bit)
				{
					number -= result + bit;
					result = (result >> 1) + bit;
				}
				else
				{
					result >>= 1;
				}
				
				bit >>= 2;
			}
			
			if (i != 0)
				continue;
			
			if (number > (1u << (BitCount / 2)) - 1)
			{
				number -= result;
				number = (number << (BitCount / 2)) - 0x8000u;
				result = (result << (BitCount / 2)) + 0x8000u;
			}
			else
			{
				number <<= BitCount / 2;
				result <<= BitCount / 2;
			}
			
			bit = 1u << (BitCount / 2 - 2);
		}
		
		if (number > result)
			result++;
		
		return new Fixed32(new ToSigned(result).castedValue);
	}
	
	public static Fixed32 Cbrt(Fixed32 value)
	{
		var rawValue = value.RawValue;
		if (rawValue == 0)
			return Zero;
		
		var negative = rawValue < 0;
		var magnitude = (ulong)(negative ? -(long)rawValue : rawValue);
		var root = (int)CubeRootFloor(magnitude << (DecimalPlaces * 2));
		
		return new Fixed32(negative ? -root : root);
	}
	
	private static ulong CubeRootFloor(ulong value)
	{
		var root = 0uL;
		var remainder = 0uL;
		
		for (var shift = 63; shift >= 0; shift -= 3)
		{
			remainder = (remainder << 3) | ((value >> shift) & 7uL);
			var step = 12uL * root * root + 6uL * root + 1uL;
			root <<= 1;
			
			if (remainder < step)
				continue;
			
			remainder -= step;
			root |= 1uL;
		}
		
		return root;
	}
	
	public static Fixed32 Wrap(Fixed32 value, Fixed32 minimum, Fixed32 maximum)
	{
		while (value < minimum)
			value += maximum - minimum;
		
		while (value >= maximum)
			value -= maximum - minimum;
		
		return value;
	}
	
	public static Fixed32 Tan(Fixed32 value)
	{
		return Sin(value) / Cos(value);
	}
	
	public static Fixed32 Acos(Fixed32 value)
	{
		if (value < NegativeOne || value > One)
			throw new ArgumentOutOfRangeException(nameof(value));
		
		if (IsZero(value))
			return PiOver2;
		
		var result = Atan(Sqrt(One - value * value) / value);
		return value.RawValue < 0 ? result + Pi : result;
	}
	
	public static Fixed32 Atan(Fixed32 value)
	{
		if (IsZero(value))
			return Zero;
		
		var negative = value.RawValue < 0;
		if (negative)
			value = -value;
		
		var invert = value > One;
		if (invert)
			value = One / value;
		
		var result = One;
		var term = One;
		
		var squared = value * value;
		var squared2 = squared * (Fixed32)2;
		var squaredPlusOne = squared + One;
		var squaredPlusOne2 = squaredPlusOne * (Fixed32)2;
		var dividend = squared2;
		var divisor = squaredPlusOne * (Fixed32)3;
		
		for (var i = 2; i < 30; i++)
		{
			term *= dividend / divisor;
			result += term;
			
			dividend += squared2;
			divisor += squaredPlusOne2;
			
			if (IsZero(term))
				break;
		}
		
		result = result * value / squaredPlusOne;
		
		if (invert)
			result = PiOver2 - result;
		
		if (negative)
			result = -result;
		
		return result;
	}
	
	public static Fixed32 Atan2(Fixed32 y, Fixed32 x)
	{
		var rawY = y.RawValue;
		var rawX = x.RawValue;
		
		if (rawX == 0L)
		{
			return rawY switch
			{
				> 0 => PiOver2,
				< 0 => -PiOver2,
				_ => Zero
			};
		}
		
		const int rawPointTwoEight = 0x0_47AE;
		var pointTwoEight = new Fixed32(rawPointTwoEight);
		
		Fixed32 atan;
		var z = y / x;
		var zSquared = z * z;
		
		if (One + pointTwoEight * zSquared == MaxValue)
			return y < Zero ? -PiOver2 : PiOver2;
		
		if (Abs(z) < One)
		{
			atan = z / (One + pointTwoEight * zSquared);
			
			if (rawX >= 0L)
				return atan;
			
			if (rawY < 0L)
				return atan - Pi;
			
			return atan + Pi;
		}
		
		atan = PiOver2 - z / (zSquared + pointTwoEight);
		
		if (rawY < 0L)
			return atan - Pi;
		
		return atan;
	}
	
	public static Fixed32 operator -(Fixed32 operand)
	{
		return operand.RawValue == int.MinValue ? MaxValue : new Fixed32(-operand.RawValue);
	}
	
	public static bool operator ==(Fixed32 left, Fixed32 right)
	{
		return left.RawValue == right.RawValue;
	}
	
	public static bool operator !=(Fixed32 left, Fixed32 right)
	{
		return left.RawValue != right.RawValue;
	}
	
	public static bool operator >(Fixed32 left, Fixed32 right)
	{
		return left.RawValue > right.RawValue;
	}
	
	public static bool operator <(Fixed32 left, Fixed32 right)
	{
		return left.RawValue < right.RawValue;
	}
	
	public static bool operator >=(Fixed32 left, Fixed32 right)
	{
		return left.RawValue >= right.RawValue;
	}
	
	public static bool operator <=(Fixed32 left, Fixed32 right)
	{
		return left.RawValue <= right.RawValue;
	}
	
	public static explicit operator Fixed32(float value)
	{
		if (float.IsNaN(value))
			return Zero;
		
		var scaled = Math.Round((double)value * RawOne, MidpointRounding.AwayFromZero);
		
		return scaled >= 2147483647.0 ? MaxValue :
			scaled <= -2147483647.0 ? MinValue : new Fixed32((int)scaled);
	}
	
	public static explicit operator float(Fixed32 value)
	{
		return (float)value.RawValue / RawOne;
	}
	
	public static explicit operator Fixed32(double value)
	{
		if (double.IsNaN(value))
			return Zero;
		
		var scaled = Math.Round(value * RawOne, MidpointRounding.AwayFromZero);
		
		return scaled >= 2147483647.0 ? MaxValue :
			scaled <= -2147483647.0 ? MinValue : new Fixed32((int)scaled);
	}
	
	public static explicit operator double(Fixed32 value)
	{
		return (double)value.RawValue / RawOne;
	}
	
	public static explicit operator Fixed32(decimal value)
	{
		var scaled = Math.Round(value * RawOne, MidpointRounding.AwayFromZero);
		
		return scaled >= 2147483647.0m ? MaxValue :
			scaled <= -2147483647.0m ? MinValue : new Fixed32((int)scaled);
	}
	
	public static explicit operator decimal(Fixed32 value)
	{
		return (decimal)value.RawValue / RawOne;
	}
	
	public static explicit operator Fixed32(Fixed64 value)
	{
		const int shiftSize = 16;
		var rawValue = new ToSigned((uint)(value.Bits >> shiftSize)).castedValue;
		return new Fixed32(rawValue);
	}
	
	public static explicit operator Fixed32(Fixed128 value)
	{
		const int shiftSize = 48;
		var rawValue = new ToSigned((uint)(value.Bits >> shiftSize)).castedValue;
		return new Fixed32(rawValue);
	}
	
	private int ToIntegerTruncated()
	{
		var integer = RawValue >> DecimalPlaces;
		if (RawValue < 0 && (RawValue & (RawOne - 1)) != 0)
			integer++;
		
		return integer;
	}
	
	private static Int128 ClampToRange(Int128 value, Int128 minimum, Int128 maximum) =>
		value < minimum ? minimum : value > maximum ? maximum : value;
	
	public static Fixed32 FromInteger(Int128 value) =>
		value > int.MaxValue >> DecimalPlaces ? MaxValue :
		value < int.MinValue >> DecimalPlaces ? MinValue : new Fixed32((int)value * RawOne);
	
	public static explicit operator sbyte(Fixed32 value) =>
		(sbyte)ClampToRange(value.ToIntegerTruncated(), sbyte.MinValue, sbyte.MaxValue);
	
	public static explicit operator byte(Fixed32 value) =>
		(byte)ClampToRange(value.ToIntegerTruncated(), byte.MinValue, byte.MaxValue);
	
	public static explicit operator short(Fixed32 value) =>
		(short)ClampToRange(value.ToIntegerTruncated(), short.MinValue, short.MaxValue);
	
	public static explicit operator ushort(Fixed32 value) =>
		(ushort)ClampToRange(value.ToIntegerTruncated(), ushort.MinValue, ushort.MaxValue);
	
	public static explicit operator int(Fixed32 value) =>
		(int)ClampToRange(value.ToIntegerTruncated(), int.MinValue, int.MaxValue);
	
	public static explicit operator uint(Fixed32 value) =>
		(uint)ClampToRange(value.ToIntegerTruncated(), uint.MinValue, uint.MaxValue);
	
	public static explicit operator long(Fixed32 value) =>
		(long)ClampToRange(value.ToIntegerTruncated(), long.MinValue, long.MaxValue);
	
	public static explicit operator ulong(Fixed32 value) =>
		(ulong)ClampToRange(value.ToIntegerTruncated(), ulong.MinValue, ulong.MaxValue);
	
	public static explicit operator Int128(Fixed32 value) => value.ToIntegerTruncated();
	
	public static explicit operator UInt128(Fixed32 value)
	{
		var integer = value.ToIntegerTruncated();
		return integer <= 0 ? UInt128.Zero : (UInt128)integer;
	}
	
	public static explicit operator Fixed32(int value) => FromInteger(value);
	
	public static explicit operator Fixed32(uint value) => FromInteger(value);
	
	public static explicit operator Fixed32(long value) => FromInteger(value);
	
	public static explicit operator Fixed32(ulong value) => FromInteger(value);
	
	public static explicit operator Fixed32(Int128 value) => FromInteger(value);
	
	public static explicit operator Fixed32(UInt128 value) =>
		value > (UInt128)Int128.MaxValue ? MaxValue : FromInteger((Int128)value);
	
	public static int Radix => 2;
	public static Fixed32 AdditiveIdentity => Zero;
	public static Fixed32 MultiplicativeIdentity => One;
	
	public static Fixed32 operator ++(Fixed32 value) => value + One;
	
	public static Fixed32 operator --(Fixed32 value) => value - One;
	
	public static Fixed32 operator +(Fixed32 value) => value;
	
	public static bool IsCanonical(Fixed32 value) => true;
	public static bool IsComplexNumber(Fixed32 value) => false;
	public static bool IsFinite(Fixed32 value) => true;
	public static bool IsImaginaryNumber(Fixed32 value) => false;
	public static bool IsInfinity(Fixed32 value) => false;
	public static bool IsNaN(Fixed32 value) => false;
	public static bool IsNegativeInfinity(Fixed32 value) => false;
	public static bool IsPositiveInfinity(Fixed32 value) => false;
	public static bool IsNormal(Fixed32 value) => value != Zero;
	public static bool IsRealNumber(Fixed32 value) => true;
	public static bool IsSubnormal(Fixed32 value) => false;
	
	public static Fixed32 MaxMagnitude(Fixed32 x, Fixed32 y) => Abs(x) < Abs(y) ? y : x;
	public static Fixed32 MinMagnitude(Fixed32 x, Fixed32 y) => Abs(x) > Abs(y) ? y : x;
	public static Fixed32 MaxMagnitudeNumber(Fixed32 x, Fixed32 y) => MaxMagnitude(x, y);
	public static Fixed32 MinMagnitudeNumber(Fixed32 x, Fixed32 y) => MinMagnitude(x, y);
	
	public static Fixed32 CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther> =>
		FixedMath.CreateChecked<Fixed32, TOther>(value);
	
	public static Fixed32 CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther> =>
		FixedMath.CreateSaturating<Fixed32, TOther>(value);
	
	public static Fixed32 CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther> =>
		FixedMath.CreateTruncating<Fixed32, TOther>(value);
	
	public static Fixed32 Parse(string number, NumberStyles style, IFormatProvider? provider) => Parse(number);
	
	public static Fixed32 Parse(string number, IFormatProvider? provider) => Parse(number);
	
	public static Fixed32 Parse(ReadOnlySpan<char> number, IFormatProvider? provider) => Parse(new string(number));
	
	public static Fixed32 Parse(ReadOnlySpan<char> number, NumberStyles style, IFormatProvider? provider) =>
		Parse(new string(number));
	
	public static bool TryParse(string? number, NumberStyles style, IFormatProvider? provider, out Fixed32 result) =>
		TryParse(number ?? "", out result);
	
	public static bool TryParse(string? number, IFormatProvider? provider, out Fixed32 result) =>
		TryParse(number ?? "", out result);
	
	public static bool TryParse(ReadOnlySpan<char> number, IFormatProvider? provider, out Fixed32 result) =>
		TryParse(new string(number), out result);
	
	public static bool TryParse(ReadOnlySpan<char> number, NumberStyles style, IFormatProvider? provider,
		out Fixed32 result) => TryParse(new string(number), out result);
	
	public static Fixed32 Exp(Fixed32 x) => Pow(E, x);
	public static Fixed32 ExpM1(Fixed32 x) => Pow(E, x) - One;
	public static Fixed32 Exp2(Fixed32 x) => Pow(One + One, x);
	public static Fixed32 Exp2M1(Fixed32 x) => Pow(One + One, x) - One;
	public static Fixed32 Exp10(Fixed32 x) => Pow(FromInteger(10), x);
	public static Fixed32 Exp10M1(Fixed32 x) => Pow(FromInteger(10), x) - One;
	
	public override bool Equals(object? obj)
	{
		return obj is Fixed32 fixedValue && Equals(fixedValue);
	}
	
	public override int GetHashCode()
	{
		return RawValue.GetHashCode();
	}
	
	public bool Equals(Fixed32 other)
	{
		return RawValue.Equals(other.RawValue);
	}
	
	public int CompareTo(Fixed32 other)
	{
		return RawValue.CompareTo(other.RawValue);
	}
	
	public override string ToString() => FixedMath.ToString(this);
	public string ToString(string? format) => FixedMath.ToString(this, format, NumberFormatInfo.CurrentInfo);
	
	public string ToString(string? format, IFormatProvider? formatProvider) =>
		FixedMath.ToString(this, format, formatProvider);
	
	public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
		IFormatProvider? provider) => FixedMath.TryFormat(this, destination, out charsWritten, format, provider);
	
	public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format,
		IFormatProvider? provider) => FixedMath.TryFormat(this, utf8Destination, out bytesWritten, format, provider);
}