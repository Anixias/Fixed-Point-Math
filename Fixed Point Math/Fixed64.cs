using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Numerics;

namespace FixedPointMath;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
public readonly struct Fixed64(long rawValue) : IFixedPoint<Fixed64>
{
	[StructLayout(LayoutKind.Explicit)]
	private readonly struct ToUnsigned(long sourceValue)
	{
		[FieldOffset(0)] public readonly long sourceValue = sourceValue;
		[FieldOffset(0)] public readonly ulong castedValue;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	private readonly struct ToSigned(ulong sourceValue)
	{
		[FieldOffset(0)] public readonly ulong sourceValue = sourceValue;
		[FieldOffset(0)] public readonly long castedValue;
	}
	
	public static Fixed64 Epsilon { get; } = new(1L);
	public static Fixed64 MaxValue { get; } = new(long.MaxValue);
	public static Fixed64 MinValue { get; } = new(long.MinValue);
	public static Fixed64 Zero { get; } = new(0L);
	public static Fixed64 One { get; } = new(RawOne);
	public static Fixed64 NegativeOne { get; } = new(RawNegativeOne);
	public static Fixed64 Half { get; } = new(RawHalf);
	public static Fixed64 Pi { get; } = new(RawPi);
	public static Fixed64 PiOver2 { get; } = new(RawPiOver2);
	public static Fixed64 Tau { get; } = new(RawTau);
	public static Fixed64 Ln2 { get; } = new(RawLn2);
	public static Fixed64 E { get; } = new(RawE);
	
	private static readonly Fixed64 _log2Max = BitCount - DecimalPlaces - 1;
	private static readonly Fixed64 _log2Min = DecimalPlaces - BitCount;
	private static readonly Fixed64 _degToRadConstant = Pi / 180;
	private static readonly Fixed64 _radToDegConstant = 180 / Pi;
	
	private const int BitCount = 64;
	internal const int DecimalPlaces = 32;
	private const long RawOne = 1L << DecimalPlaces;
	private const long RawHalf = 0x0_80000000L;
	private const long RawNegativeOne = -(1L << DecimalPlaces);
	private const long RawPi = 0x3_243F6A89L;
	private const long RawPiOver2 = 0x1_921FB544L;
	private const long RawTau = 0x6_487ED511L;
	private const long RawLn2 = 0x0_B17217F8L;
	private const long RawE = 0x2_B7E15163L;
	
	public long RawValue { get; } = rawValue;
	public ulong Bits => new ToUnsigned(RawValue).castedValue;
	
	public Fixed64(int value) : this(value * RawOne)
	{
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsZero(Fixed64 value)
	{
		return value.RawValue == 0L;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsZero()
	{
		return IsZero(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(Fixed64 value)
	{
		return value.RawValue < 0L;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsNegative()
	{
		return IsNegative(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOddInteger(Fixed64 value)
	{
		return IsInteger(value) && ((value.RawValue >> DecimalPlaces) & 1) == 1;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEvenInteger(Fixed64 value)
	{
		return IsInteger(value) && ((value.RawValue >> DecimalPlaces) & 1) == 0;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(Fixed64 value)
	{
		return value.RawValue > 0L;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsPositive()
	{
		return IsPositive(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInteger(Fixed64 value)
	{
		return Fract(value) == Zero;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInteger()
	{
		return IsInteger(this);
	}
	
	public static Fixed64 DegToRad(Fixed64 value)
	{
		return value * _degToRadConstant;
	}
	
	public Fixed64 DegToRad() => DegToRad(this);
	
	public static Fixed64 RadToDeg(Fixed64 value)
	{
		return value * _radToDegConstant;
	}
	
	public Fixed64 RadToDeg() => RadToDeg(this);
	
	public static Fixed64 Lerp(Fixed64 a, Fixed64 b, Fixed64 t)
	{
		return a + t * (b - a);
	}
	
	public Fixed64 Lerp(Fixed64 target, Fixed64 weight) => Lerp(this, target, weight);
	
	public static int Sign(Fixed64 value)
	{
		return
			value.RawValue < 0 ? -1 :
			value.RawValue > 0 ? 1 :
			0;
	}
	
	public int Sign() => Sign(this);
	
	public static Fixed64 Abs(Fixed64 value)
	{
		if (value == MinValue)
			return MaxValue;
		
		var mask = value.RawValue >> (BitCount - 1);
		return new Fixed64((value.RawValue + mask) ^ mask);
	}
	
	public Fixed64 Abs() => Abs(this);
	
	public static Fixed64 Floor(Fixed64 value)
	{
		var rawValue = new ToUnsigned(value.RawValue).castedValue;
		var flooredValue = new ToSigned(rawValue & 0xFFFF_FFFF_0000_0000uL).castedValue;
		return new Fixed64(flooredValue);
	}
	
	public Fixed64 Floor() => Floor(this);
	
	public static Fixed64 Ceil(Fixed64 value)
	{
		var hasDecimalPart = (value.RawValue & 0x0000_0000_FFFF_FFFFL) != 0L;
		return hasDecimalPart ? Floor(value) + One : value;
	}
	
	public Fixed64 Ceil() => Ceil(this);
	
	public static Fixed64 Fract(Fixed64 value)
	{
		return new Fixed64(value.RawValue & 0x0000_0000_FFFF_FFFFL);
	}
	
	public Fixed64 Fract() => Fract(this);
	
	public static Fixed64 Round(Fixed64 value)
	{
		var decimalPart = value.RawValue & 0x0000_0000_FFFF_FFFFL;
		var integerPart = Floor(value);
		
		if (decimalPart < 0x8000_0000L)
			return integerPart;
		
		if (decimalPart > 0x8000_0000L)
			return integerPart + One;
		
		return (integerPart.RawValue & RawOne) == 0L
			? integerPart
			: integerPart + One;
	}
	
	public Fixed64 Round() => Round(this);
	
	public static Fixed64 Clamp(Fixed64 value, Fixed64 min, Fixed64 max)
	{
		if (value < min)
			return min;
		
		if (value > max)
			return max;
		
		return value;
	}
	
	public Fixed64 Clamp(Fixed64 min, Fixed64 max) => Clamp(this, min, max);
	
	public static Fixed64 Min(Fixed64 a, Fixed64 b)
	{
		return a < b ? a : b;
	}
	
	public static Fixed64 Max(Fixed64 a, Fixed64 b)
	{
		return a > b ? a : b;
	}
	
	public static Fixed64 PosMod(Fixed64 a, Fixed64 b)
	{
		var result = a % b;
		
		if (a.RawValue < 0L && b.RawValue > 0L || result.RawValue > 0L && b.RawValue < 0L)
			result += b;
		
		return result;
	}
	
	public Fixed64 PosMod(Fixed64 divisor) => PosMod(this, divisor);
	
	public static int FractionalBits => DecimalPlaces;
	
	public Int128 RawBits => RawValue;
	
	public static Fixed64 FromRawBits(Int128 rawBits) =>
		rawBits > long.MaxValue ? MaxValue :
		rawBits < long.MinValue ? MinValue : new Fixed64((long)rawBits);
	
	public static Fixed64 Parse(string number) => FixedMath.Parse<Fixed64>(number);
	
	public static bool TryParse(string number, out Fixed64 result) => FixedMath.TryParse(number, out result);
	
	public int ToInt() => (int)this;
	public long ToLong() => (long)this;
	
	public static Fixed64 Sin(Fixed64 value) => FixedMath.Sin(value);
	
	public static Fixed64 Cos(Fixed64 value) => FixedMath.Cos(value);
	
	public static (Fixed64 sin, Fixed64 cos) SinCos(Fixed64 angle) => FixedMath.SinCos(angle);
	
	public (Fixed64 sin, Fixed64 cos) SinCos() => SinCos(this);
	
	public static Fixed64 Snapped(Fixed64 value, Fixed64 step)
	{
		return step.RawValue != 0L ? Floor(value / step + Half) * step : value;
	}
	
	public Fixed64 Snapped(Fixed64 step) => Snapped(this, step);
	
	public static Fixed64 operator +(Fixed64 left, Fixed64 right)
	{
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		var sum = leftRaw + rightRaw;
		
		if ((~(leftRaw ^ rightRaw) & (leftRaw ^ sum) & long.MinValue) != 0L)
			sum = leftRaw > 0L ? long.MaxValue : long.MinValue;
		
		return new Fixed64(sum);
	}
	
	public static Fixed64 operator -(Fixed64 left, Fixed64 right)
	{
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		var difference = leftRaw - rightRaw;
		
		if (((leftRaw ^ rightRaw) & (leftRaw ^ difference) & long.MinValue) != 0L)
			difference = leftRaw < 0L ? long.MinValue : long.MaxValue;
		
		return new Fixed64(difference);
	}
	
	private const ulong MultiplyRounding = 1uL << (DecimalPlaces - 1);
	
	private static long AddOverflow(long left, long right, ref bool overflow)
	{
		var sum = left + right;
		overflow |= ((left ^ right ^ sum) & long.MinValue) != 0L;
		return sum;
	}
	
	public static Fixed64 operator *(Fixed64 left, Fixed64 right)
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
		
		if (leftRaw == 0L || rightRaw == 0L)
			return Zero;
		
		var leftLow = leftRaw & 0x0000_0000_FFFF_FFFFL;
		var leftHigh = leftRaw >> DecimalPlaces;
		var rightLow = rightRaw & 0x0000_0000_FFFF_FFFFL;
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
		
		var opSignsEqual = ((leftRaw ^ rightRaw) & long.MinValue) == 0L;
		
		if (opSignsEqual)
		{
			if (sum < 0L || (overflow && leftRaw > 0L))
				return MaxValue;
		}
		else
		{
			if (sum > 0L)
				return MinValue;
		}
		
		var topCarry = highHigh >> DecimalPlaces;
		if (topCarry != 0L && topCarry != -1)
			return opSignsEqual ? MaxValue : MinValue;
		
		if (opSignsEqual)
			return new Fixed64(sum);
		
		long posOp, negOp;
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
		
		return new Fixed64(sum);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int CountLeadingZeroes(ulong value)
	{
		var result = 0;
		
		while ((value & 0xF000_0000_0000_0000uL) == 0)
		{
			result += 4;
			value <<= 4;
		}
		
		while ((value & 0x8000_0000_0000_0000uL) == 0)
		{
			result += 1;
			value <<= 1;
		}
		
		return result;
	}
	
	public static Fixed64 operator /(Fixed64 left, Fixed64 right)
	{
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		
		if (rightRaw == 0L)
			throw new DivideByZeroException();
		
		if (right == 2)
			return new Fixed64(leftRaw >> 1);
		
		var remainder = new ToUnsigned(leftRaw >= 0L ? leftRaw : -leftRaw).castedValue;
		var divisor = new ToUnsigned(rightRaw >= 0L ? rightRaw : -rightRaw).castedValue;
		var quotient = 0uL;
		var bitPos = BitCount / 2 + 1;
		
		while ((divisor & 0xFuL) == 0uL && bitPos >= 4)
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
			
			if ((division & ~(ulong.MaxValue >> bitPos)) != 0uL)
				return ((leftRaw ^ rightRaw) & long.MinValue) == 0L ? MaxValue : MinValue;
			
			remainder <<= 1;
			bitPos--;
		}
		
		quotient++;
		var result = (long)(quotient >> 1);
		if (((leftRaw ^ rightRaw) & long.MinValue) != 0L)
			result = -result;
		
		return new Fixed64(result);
	}
	
	public static Fixed64 operator %(Fixed64 left, Fixed64 right)
	{
		return new Fixed64(left.RawValue == long.MinValue & right.RawValue == -1L
			? 0L
			: left.RawValue % right.RawValue);
	}
	
	private static Fixed64 Pow2(Fixed64 exponent)
	{
		if (exponent.RawValue == 0L)
			return One;
		
		var negative = exponent.RawValue < 0L;
		if (negative)
			exponent = -exponent;
		
		if (exponent == One)
			return negative ? Half : 2;
		
		if (exponent >= _log2Max)
			return negative ? One / MaxValue : MaxValue;
		
		if (exponent <= _log2Min)
			return negative ? MaxValue : Zero;
		
		var integerPart = (int)Floor(exponent);
		exponent = Fract(exponent);
		
		var result = One;
		var term = One;
		var i = 1;
		
		while (term.RawValue != 0L)
		{
			term = exponent * term * Ln2 / i;
			result += term;
			i++;
		}
		
		result = new Fixed64(result.RawValue << integerPart);
		if (negative)
			result = One / result;
		
		return result;
	}
	
	public static Fixed64 Log2(Fixed64 value)
	{
		if (!IsPositive(value))
			throw new ArgumentOutOfRangeException(nameof(value));
		
		var b = 1L << (DecimalPlaces - 1);
		var y = 0L;
		
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
		
		var z = new Fixed64(rawValue);
		
		for (var i = 0; i < DecimalPlaces; i++)
		{
			z *= z;
			if (z.RawValue >= RawOne << 1)
			{
				z = new Fixed64(z.RawValue >> 1);
				y += b;
			}
			
			b >>= 1;
		}
		
		return new Fixed64(y);
	}
	
	public static Fixed64 Ln(Fixed64 value)
	{
		return Log2(value) * Ln2;
	}
	
	public static Fixed64 Pow(Fixed64 @base, Fixed64 exponent)
	{
		if (@base < Zero)
		{
			// Todo: Handle properly
			if (!exponent.IsInteger())
				return Zero;
			
			var pow = Pow(-@base, exponent);
			if (exponent % 2 == 0)
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
	
	public static Fixed64 Sqrt(Fixed64 value)
	{
		var rawValue = value.RawValue;
		if (rawValue < 0L)
			throw new ArgumentOutOfRangeException(nameof(value));
		
		var number = new ToUnsigned(rawValue).castedValue;
		var result = 0uL;
		var bit = 1uL << (BitCount - 2);
		
		while (bit > number)
		{
			bit >>= 2;
		}
		
		for (var i = 0; i < 2; i++)
		{
			while (bit != 0uL)
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
			
			if (number > (1uL << (BitCount / 2)) - 1)
			{
				number -= result;
				number = (number << (BitCount / 2)) - 0x8000_0000uL;
				result = (result << (BitCount / 2)) + 0x8000_0000uL;
			}
			else
			{
				number <<= BitCount / 2;
				result <<= BitCount / 2;
			}
			
			bit = 1uL << (BitCount / 2 - 2);
		}
		
		if (number > result)
			result++;
		
		return new Fixed64(new ToSigned(result).castedValue);
	}
	
	public static Fixed64 Cbrt(Fixed64 value)
	{
		var rawValue = value.RawValue;
		if (rawValue == 0L)
			return Zero;
		
		var negative = rawValue < 0L;
		var magnitude = (UInt128)(negative ? -(Int128)rawValue : rawValue);
		var root = (long)CubeRootFloor(magnitude << (DecimalPlaces * 2));
		
		return new Fixed64(negative ? -root : root);
	}
	
	private static UInt128 CubeRootFloor(UInt128 value)
	{
		var root = UInt128.Zero;
		var remainder = UInt128.Zero;
		
		for (var shift = 126; shift >= 0; shift -= 3)
		{
			remainder = (remainder << 3) | ((value >> shift) & 7);
			var step = 12u * root * root + 6u * root + UInt128.One;
			root <<= 1;
			
			if (remainder < step)
				continue;
			
			remainder -= step;
			root |= UInt128.One;
		}
		
		return root;
	}
	
	public static Fixed64 Wrap(Fixed64 value, Fixed64 minimum, Fixed64 maximum)
	{
		while (value < minimum)
			value += maximum - minimum;
		
		while (value >= maximum)
			value -= maximum - minimum;
		
		return value;
	}
	
	public static Fixed64 Tan(Fixed64 value)
	{
		return Sin(value) / Cos(value);
	}
	
	public static Fixed64 Acos(Fixed64 value)
	{
		if (value < NegativeOne || value > One)
			throw new ArgumentOutOfRangeException(nameof(value));
		
		if (IsZero(value))
			return PiOver2;
		
		var result = Atan(Sqrt(One - value * value) / value);
		return value.RawValue < 0 ? result + Pi : result;
	}
	
	public static Fixed64 Atan(Fixed64 value)
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
		var squared2 = squared * 2;
		var squaredPlusOne = squared + One;
		var squaredPlusOne2 = squaredPlusOne * 2;
		var dividend = squared2;
		var divisor = squaredPlusOne * 3;
		
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
	
	public static Fixed64 Atan2(Fixed64 y, Fixed64 x)
	{
		var rawY = y.RawValue;
		var rawX = x.RawValue;
		
		if (rawX == 0L)
		{
			return rawY switch
			{
				> 0L => PiOver2,
				< 0L => -PiOver2,
				_ => Zero
			};
		}
		
		const long rawPointTwoEight = 1202590844L;
		var pointTwoEight = new Fixed64(rawPointTwoEight);
		
		Fixed64 atan;
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
	
	public static Fixed64 operator -(Fixed64 operand)
	{
		return operand.RawValue == long.MinValue ? MaxValue : new Fixed64(-operand.RawValue);
	}
	
	public static bool operator ==(Fixed64 left, Fixed64 right)
	{
		return left.RawValue == right.RawValue;
	}
	
	public static bool operator !=(Fixed64 left, Fixed64 right)
	{
		return left.RawValue != right.RawValue;
	}
	
	public static bool operator >(Fixed64 left, Fixed64 right)
	{
		return left.RawValue > right.RawValue;
	}
	
	public static bool operator <(Fixed64 left, Fixed64 right)
	{
		return left.RawValue < right.RawValue;
	}
	
	public static bool operator >=(Fixed64 left, Fixed64 right)
	{
		return left.RawValue >= right.RawValue;
	}
	
	public static bool operator <=(Fixed64 left, Fixed64 right)
	{
		return left.RawValue <= right.RawValue;
	}
	
	public static explicit operator Fixed64(float value)
	{
		if (float.IsNaN(value))
			return Zero;
		
		var scaled = Math.Round((double)value * RawOne, MidpointRounding.AwayFromZero);
		
		return scaled >= 9.2233720368547758E18 ? MaxValue :
			scaled <= -9.2233720368547758E18 ? MinValue : new Fixed64((long)scaled);
	}
	
	public static explicit operator float(Fixed64 value)
	{
		return (float)value.RawValue / RawOne;
	}
	
	public static explicit operator Fixed64(double value)
	{
		if (double.IsNaN(value))
			return Zero;
		
		var scaled = Math.Round(value * RawOne, MidpointRounding.AwayFromZero);
		
		return scaled >= 9.2233720368547758E18 ? MaxValue :
			scaled <= -9.2233720368547758E18 ? MinValue : new Fixed64((long)scaled);
	}
	
	public static explicit operator double(Fixed64 value)
	{
		return (double)value.RawValue / RawOne;
	}
	
	public static explicit operator Fixed64(decimal value)
	{
		var scaled = Math.Round(value * RawOne, MidpointRounding.AwayFromZero);
		
		return scaled >= 9223372036854775807m ? MaxValue :
			scaled <= -9223372036854775807m ? MinValue : new Fixed64((long)scaled);
	}
	
	public static explicit operator decimal(Fixed64 value)
	{
		return (decimal)value.RawValue / RawOne;
	}
	
	public static implicit operator Fixed64(Fixed32 value)
	{
		const int shiftSize = DecimalPlaces - Fixed32.DecimalPlaces;
		var rawValue = (long)value.RawValue << shiftSize;
		return new Fixed64(rawValue);
	}
	
	public static explicit operator Fixed64(Fixed128 value)
	{
		const int shiftSize = Fixed128.DecimalPlaces - DecimalPlaces;
		var rawValue = new ToSigned((ulong)(value.Bits >> shiftSize)).castedValue;
		return new Fixed64(rawValue);
	}
	
	private long ToIntegerTruncated()
	{
		var integer = RawValue >> DecimalPlaces;
		if (RawValue < 0L && (RawValue & (RawOne - 1L)) != 0L)
			integer++;
		
		return integer;
	}
	
	private static Int128 ClampToRange(Int128 value, Int128 minimum, Int128 maximum) =>
		value < minimum ? minimum : value > maximum ? maximum : value;
	
	public static Fixed64 FromInteger(Int128 value) =>
		value > long.MaxValue >> DecimalPlaces ? MaxValue :
		value < long.MinValue >> DecimalPlaces ? MinValue : new Fixed64((long)value * RawOne);
	
	public static explicit operator sbyte(Fixed64 value) =>
		(sbyte)ClampToRange(value.ToIntegerTruncated(), sbyte.MinValue, sbyte.MaxValue);
	
	public static explicit operator byte(Fixed64 value) =>
		(byte)ClampToRange(value.ToIntegerTruncated(), byte.MinValue, byte.MaxValue);
	
	public static explicit operator short(Fixed64 value) =>
		(short)ClampToRange(value.ToIntegerTruncated(), short.MinValue, short.MaxValue);
	
	public static explicit operator ushort(Fixed64 value) =>
		(ushort)ClampToRange(value.ToIntegerTruncated(), ushort.MinValue, ushort.MaxValue);
	
	public static explicit operator int(Fixed64 value) =>
		(int)ClampToRange(value.ToIntegerTruncated(), int.MinValue, int.MaxValue);
	
	public static explicit operator uint(Fixed64 value) =>
		(uint)ClampToRange(value.ToIntegerTruncated(), uint.MinValue, uint.MaxValue);
	
	public static explicit operator long(Fixed64 value) =>
		(long)ClampToRange(value.ToIntegerTruncated(), long.MinValue, long.MaxValue);
	
	public static explicit operator ulong(Fixed64 value) =>
		(ulong)ClampToRange(value.ToIntegerTruncated(), ulong.MinValue, ulong.MaxValue);
	
	public static explicit operator Int128(Fixed64 value) => value.ToIntegerTruncated();
	
	public static explicit operator UInt128(Fixed64 value)
	{
		var integer = value.ToIntegerTruncated();
		return integer <= 0L ? UInt128.Zero : (UInt128)integer;
	}
	
	public static implicit operator Fixed64(int value) => FromInteger(value);
	
	public static explicit operator Fixed64(uint value) => FromInteger(value);
	
	public static explicit operator Fixed64(long value) => FromInteger(value);
	
	public static explicit operator Fixed64(ulong value) => FromInteger(value);
	
	public static explicit operator Fixed64(Int128 value) => FromInteger(value);
	
	public static explicit operator Fixed64(UInt128 value) =>
		value > (UInt128)Int128.MaxValue ? MaxValue : FromInteger((Int128)value);
	
	public static int Radix => 2;
	public static Fixed64 AdditiveIdentity => Zero;
	public static Fixed64 MultiplicativeIdentity => One;
	
	public static Fixed64 operator ++(Fixed64 value) => value + One;
	
	public static Fixed64 operator --(Fixed64 value) => value - One;
	
	public static Fixed64 operator +(Fixed64 value) => value;
	
	public static bool IsCanonical(Fixed64 value) => true;
	public static bool IsComplexNumber(Fixed64 value) => false;
	public static bool IsFinite(Fixed64 value) => true;
	public static bool IsImaginaryNumber(Fixed64 value) => false;
	public static bool IsInfinity(Fixed64 value) => false;
	public static bool IsNaN(Fixed64 value) => false;
	public static bool IsNegativeInfinity(Fixed64 value) => false;
	public static bool IsPositiveInfinity(Fixed64 value) => false;
	public static bool IsNormal(Fixed64 value) => value != Zero;
	public static bool IsRealNumber(Fixed64 value) => true;
	public static bool IsSubnormal(Fixed64 value) => false;
	
	public static Fixed64 MaxMagnitude(Fixed64 x, Fixed64 y) => Abs(x) < Abs(y) ? y : x;
	public static Fixed64 MinMagnitude(Fixed64 x, Fixed64 y) => Abs(x) > Abs(y) ? y : x;
	public static Fixed64 MaxMagnitudeNumber(Fixed64 x, Fixed64 y) => MaxMagnitude(x, y);
	public static Fixed64 MinMagnitudeNumber(Fixed64 x, Fixed64 y) => MinMagnitude(x, y);
	
	public static Fixed64 CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther> =>
		FixedMath.CreateChecked<Fixed64, TOther>(value);
	
	public static Fixed64 CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther> =>
		FixedMath.CreateSaturating<Fixed64, TOther>(value);
	
	public static Fixed64 CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther> =>
		FixedMath.CreateTruncating<Fixed64, TOther>(value);
	
	public static Fixed64 Parse(string number, NumberStyles style, IFormatProvider? provider) => Parse(number);
	
	public static Fixed64 Parse(string number, IFormatProvider? provider) => Parse(number);
	
	public static Fixed64 Parse(ReadOnlySpan<char> number, IFormatProvider? provider) => Parse(new string(number));
	
	public static Fixed64 Parse(ReadOnlySpan<char> number, NumberStyles style, IFormatProvider? provider) =>
		Parse(new string(number));
	
	public static bool TryParse(string? number, NumberStyles style, IFormatProvider? provider, out Fixed64 result) =>
		TryParse(number ?? "", out result);
	
	public static bool TryParse(string? number, IFormatProvider? provider, out Fixed64 result) =>
		TryParse(number ?? "", out result);
	
	public static bool TryParse(ReadOnlySpan<char> number, IFormatProvider? provider, out Fixed64 result) =>
		TryParse(new string(number), out result);
	
	public static bool TryParse(ReadOnlySpan<char> number, NumberStyles style, IFormatProvider? provider,
		out Fixed64 result) => TryParse(new string(number), out result);
	
	public static Fixed64 Exp(Fixed64 x) => Pow(E, x);
	public static Fixed64 ExpM1(Fixed64 x) => Pow(E, x) - One;
	public static Fixed64 Exp2(Fixed64 x) => Pow(One + One, x);
	public static Fixed64 Exp2M1(Fixed64 x) => Pow(One + One, x) - One;
	public static Fixed64 Exp10(Fixed64 x) => Pow(FromInteger(10), x);
	public static Fixed64 Exp10M1(Fixed64 x) => Pow(FromInteger(10), x) - One;
	
	public override bool Equals(object? obj)
	{
		return obj is Fixed64 fixedValue && Equals(fixedValue);
	}
	
	public override int GetHashCode()
	{
		return RawValue.GetHashCode();
	}
	
	public bool Equals(Fixed64 other)
	{
		return RawValue.Equals(other.RawValue);
	}
	
	public int CompareTo(Fixed64 other)
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