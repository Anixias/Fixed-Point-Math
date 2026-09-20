using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Numerics;

namespace FixedPointMath;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
public readonly struct Fixed128(Int128 rawValue) : IFixedPoint<Fixed128>
{
	[StructLayout(LayoutKind.Explicit)]
	private readonly struct ToUnsigned(Int128 sourceValue)
	{
		[FieldOffset(0)] public readonly Int128 sourceValue = sourceValue;
		[FieldOffset(0)] public readonly UInt128 castedValue;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	private readonly struct ToSigned(UInt128 sourceValue)
	{
		[FieldOffset(0)] public readonly UInt128 sourceValue = sourceValue;
		[FieldOffset(0)] public readonly Int128 castedValue;
	}
	
	private const int BitCount = 128;
	internal const int DecimalPlaces = 64;
	private static readonly Int128 _rawOne = Int128.One << DecimalPlaces;
	private static readonly Int128 _rawHalf = _rawOne >> 1;
	private static readonly Int128 _rawNegativeOne = -_rawOne;
	private static readonly Int128 _rawPi = new(3uL, 2611923443488327891uL);
	private static readonly Int128 _rawPiOver2 = new(1uL, 10529333758598939754uL);
	private static readonly Int128 _rawLn2 = new(0uL, 12786308645202655660uL);
	private static readonly Int128 _rawE = new(2uL, 13249961062380153451uL);
	
	public static Fixed128 Epsilon { get; } = new(Int128.One);
	public static Fixed128 MaxValue { get; } = new(Int128.MaxValue);
	public static Fixed128 MinValue { get; } = new(Int128.MinValue);
	public static Fixed128 Zero { get; } = new(Int128.Zero);
	public static Fixed128 One { get; } = new(_rawOne);
	public static Fixed128 NegativeOne { get; } = new(_rawNegativeOne);
	public static Fixed128 Half { get; } = new(_rawHalf);
	public static Fixed128 Pi { get; } = new(_rawPi);
	public static Fixed128 PiOver2 { get; } = new(_rawPiOver2);
	public static Fixed128 Tau { get; } = Pi * 2;
	public static Fixed128 Ln2 { get; } = new(_rawLn2);
	public static Fixed128 E { get; } = new(_rawE);
	
	private static readonly Fixed128 _log2Max = BitCount - DecimalPlaces - 1;
	private static readonly Fixed128 _log2Min = DecimalPlaces - BitCount;
	private static readonly Fixed128 _degToRadConstant = Pi / 180;
	private static readonly Fixed128 _radToDegConstant = 180 / Pi;
	
	public Int128 RawValue { get; } = rawValue;
	public UInt128 Bits => new ToUnsigned(RawValue).castedValue;
	
	public Fixed128(int value) : this(value * _rawOne)
	{
	}
	
	public Fixed128(long value) : this(value * _rawOne)
	{
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsZero(Fixed128 value)
	{
		return value.RawValue == 0;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsZero()
	{
		return IsZero(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegative(Fixed128 value)
	{
		return value.RawValue < 0;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsNegative()
	{
		return IsNegative(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOddInteger(Fixed128 value)
	{
		return IsInteger(value) && ((value.RawValue >> DecimalPlaces) & 1) == 1;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEvenInteger(Fixed128 value)
	{
		return IsInteger(value) && ((value.RawValue >> DecimalPlaces) & 1) == 0;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPositive(Fixed128 value)
	{
		return value.RawValue > 0;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsPositive()
	{
		return IsPositive(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInteger(Fixed128 value)
	{
		return Fract(value) == Zero;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInteger()
	{
		return IsInteger(this);
	}
	
	public static Fixed128 DegToRad(Fixed128 value)
	{
		return value * _degToRadConstant;
	}
	
	public Fixed128 DegToRad() => DegToRad(this);
	
	public static Fixed128 RadToDeg(Fixed128 value)
	{
		return value * _radToDegConstant;
	}
	
	public Fixed128 RadToDeg() => RadToDeg(this);
	
	public static Fixed128 Lerp(Fixed128 a, Fixed128 b, Fixed128 t)
	{
		return a + t * (b - a);
	}
	
	public Fixed128 Lerp(Fixed128 target, Fixed128 weight) => Lerp(this, target, weight);
	
	public static int Sign(Fixed128 value)
	{
		return
			value.RawValue < 0 ? -1 :
			value.RawValue > 0 ? 1 :
			0;
	}
	
	public int Sign() => Sign(this);
	
	public static Fixed128 Abs(Fixed128 value)
	{
		if (value == MinValue)
			return MaxValue;
		
		var mask = value.RawValue >> (BitCount - 1);
		return new Fixed128((value.RawValue + mask) ^ mask);
	}
	
	public Fixed128 Abs() => Abs(this);
	
	public static Fixed128 Floor(Fixed128 value)
	{
		var rawValueCast = new ToUnsigned(value.RawValue);
		var flooredValue = new ToSigned(rawValueCast.castedValue & ((UInt128)0xFFFF_FFFF_FFFF_FFFFuL << DecimalPlaces));
		return new Fixed128(flooredValue.castedValue);
	}
	
	public Fixed128 Floor() => Floor(this);
	
	public static Fixed128 Ceil(Fixed128 value)
	{
		var hasDecimalPart = (value.RawValue & 0xFFFF_FFFF_FFFF_FFFFuL) != Int128.Zero;
		return hasDecimalPart ? Floor(value) + One : value;
	}
	
	public Fixed128 Ceil() => Ceil(this);
	
	public static Fixed128 Fract(Fixed128 value)
	{
		return new Fixed128(value.RawValue & 0xFFFF_FFFF_FFFF_FFFFuL);
	}
	
	public Fixed128 Fract() => Fract(this);
	
	public static Fixed128 Round(Fixed128 value)
	{
		var decimalPart = value.RawValue & 0xFFFF_FFFF_FFFF_FFFFuL;
		var integerPart = Floor(value);
		
		if (decimalPart < 0x8000_0000_0000_0000L)
			return integerPart;
		
		if (decimalPart > 0x8000_0000_0000_0000L)
			return integerPart + One;
		
		return (integerPart.RawValue & _rawOne) == Int128.Zero
			? integerPart
			: integerPart + One;
	}
	
	public Fixed128 Round() => Round(this);
	
	public static Fixed128 Clamp(Fixed128 value, Fixed128 min, Fixed128 max)
	{
		if (value < min)
			return min;
		
		if (value > max)
			return max;
		
		return value;
	}
	
	public Fixed128 Clamp(Fixed128 min, Fixed128 max) => Clamp(this, min, max);
	
	public static Fixed128 Min(Fixed128 a, Fixed128 b)
	{
		return a < b ? a : b;
	}
	
	public static Fixed128 Max(Fixed128 a, Fixed128 b)
	{
		return a > b ? a : b;
	}
	
	public static Fixed128 PosMod(Fixed128 a, Fixed128 b)
	{
		var result = a % b;
		
		if (a.RawValue < Int128.Zero && b.RawValue > Int128.Zero ||
		    result.RawValue > Int128.Zero && b.RawValue < Int128.Zero)
			result += b;
		
		return result;
	}
	
	public Fixed128 PosMod(Fixed128 divisor) => PosMod(this, divisor);
	
	public static int FractionalBits => DecimalPlaces;
	
	public Int128 RawBits => RawValue;
	
	public static Fixed128 FromRawBits(Int128 rawBits) =>
		rawBits > Int128.MaxValue ? MaxValue :
		rawBits < Int128.MinValue ? MinValue : new Fixed128(rawBits);
	
	public static Fixed128 Parse(string number) => FixedMath.Parse<Fixed128>(number);
	
	public static bool TryParse(string number, out Fixed128 result) => FixedMath.TryParse(number, out result);
	
	public static Fixed128 Sin(Fixed128 value) => FixedMath.Sin(value);
	
	public static Fixed128 Cos(Fixed128 value) => FixedMath.Cos(value);
	
	public static (Fixed128 sin, Fixed128 cos) SinCos(Fixed128 angle) => FixedMath.SinCos(angle);
	
	public (Fixed128 sin, Fixed128 cos) SinCos() => SinCos(this);
	
	public static Fixed128 Snapped(Fixed128 value, Fixed128 step)
	{
		return step.RawValue != Int128.Zero ? Floor(value / step + Half) * step : value;
	}
	
	public Fixed128 Snapped(Fixed128 step) => Snapped(this, step);
	
	public static Fixed128 operator +(Fixed128 left, Fixed128 right)
	{
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		var sum = leftRaw + rightRaw;
		
		if ((~(leftRaw ^ rightRaw) & (leftRaw ^ sum) & Int128.MinValue) != Int128.Zero)
			sum = leftRaw > Int128.Zero ? Int128.MaxValue : Int128.MinValue;
		
		return new Fixed128(sum);
	}
	
	public static Fixed128 operator -(Fixed128 left, Fixed128 right)
	{
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		var difference = leftRaw - rightRaw;
		
		if (((leftRaw ^ rightRaw) & (leftRaw ^ difference) & Int128.MinValue) != Int128.Zero)
			difference = leftRaw < Int128.Zero ? Int128.MinValue : Int128.MaxValue;
		
		return new Fixed128(difference);
	}
	
	private static readonly UInt128 _multiplyRounding = UInt128.One << (DecimalPlaces - 1);
	
	private static Int128 AddOverflow(Int128 left, Int128 right, ref bool overflow)
	{
		var sum = left + right;
		overflow |= ((left ^ right ^ sum) & Int128.MinValue) != Int128.Zero;
		return sum;
	}
	
	public static Fixed128 operator *(Fixed128 left, Fixed128 right)
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
		
		if (leftRaw == Int128.Zero || rightRaw == Int128.Zero)
			return Zero;
		
		var leftLow = leftRaw & 0xFFFF_FFFF_FFFF_FFFFuL;
		var leftHigh = leftRaw >> DecimalPlaces;
		var rightLow = rightRaw & 0xFFFF_FFFF_FFFF_FFFFuL;
		var rightHigh = rightRaw >> DecimalPlaces;
		
		var leftLowCast = new ToUnsigned(leftLow);
		var rightLowCast = new ToUnsigned(rightLow);
		
		var lowLow = leftLowCast.castedValue * rightLowCast.castedValue;
		var lowHigh = leftLow * rightHigh;
		var highLow = leftHigh * rightLow;
		var highHigh = leftHigh * rightHigh;
		
		var lowResult = new ToSigned((lowLow + _multiplyRounding) >> DecimalPlaces).castedValue;
		var highResult = highHigh << DecimalPlaces;
		
		var overflow = false;
		var sum = AddOverflow(lowResult, lowHigh, ref overflow);
		sum = AddOverflow(sum, highLow, ref overflow);
		sum = AddOverflow(sum, highResult, ref overflow);
		
		var opSignsEqual = ((leftRaw ^ rightRaw) & Int128.MinValue) == Int128.Zero;
		
		if (opSignsEqual)
		{
			if (sum < Int128.Zero || (overflow && leftRaw > Int128.Zero))
				return MaxValue;
		}
		else
		{
			if (sum > Int128.Zero)
				return MinValue;
		}
		
		var topCarry = highHigh >> DecimalPlaces;
		if (topCarry != Int128.Zero && topCarry != -1)
			return opSignsEqual ? MaxValue : MinValue;
		
		if (opSignsEqual)
			return new Fixed128(sum);
		
		Int128 posOp, negOp;
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
		
		if (sum > negOp && negOp < -_rawOne && posOp > _rawOne)
			return MinValue;
		
		return new Fixed128(sum);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int CountLeadingZeroes(UInt128 value)
	{
		var result = 0;
		
		while ((value & ((UInt128)0xF000_0000_0000_0000uL << DecimalPlaces)) == 0)
		{
			result += 4;
			value <<= 4;
		}
		
		while ((value & ((UInt128)0x8000_0000_0000_0000uL << DecimalPlaces)) == 0)
		{
			result += 1;
			value <<= 1;
		}
		
		return result;
	}
	
	public static Fixed128 operator /(Fixed128 left, Fixed128 right)
	{
		var leftRaw = left.RawValue;
		var rightRaw = right.RawValue;
		
		if (rightRaw == Int128.Zero)
			throw new DivideByZeroException();
		
		if (right == 2)
			return new Fixed128(leftRaw >> 1);
		
		var remainder = new ToUnsigned(leftRaw >= Int128.Zero ? leftRaw : -leftRaw).castedValue;
		var divisor = new ToUnsigned(rightRaw >= Int128.Zero ? rightRaw : -rightRaw).castedValue;
		var quotient = UInt128.Zero;
		var bitPos = BitCount / 2 + 1;
		
		while ((divisor & 0xFuL) == UInt128.Zero && bitPos >= 4)
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
			
			if ((division & ~(UInt128.MaxValue >> bitPos)) != UInt128.Zero)
				return ((leftRaw ^ rightRaw) & Int128.MinValue) == Int128.Zero ? MaxValue : MinValue;
			
			remainder <<= 1;
			bitPos--;
		}
		
		quotient++;
		var result = (Int128)(quotient >> 1);
		if (((leftRaw ^ rightRaw) & Int128.MinValue) != Int128.Zero)
			result = -result;
		
		return new Fixed128(result);
	}
	
	public static Fixed128 operator %(Fixed128 left, Fixed128 right)
	{
		return new Fixed128(left.RawValue == Int128.MinValue & right.RawValue == -1L
			? Int128.Zero
			: left.RawValue % right.RawValue);
	}
	
	private static Fixed128 Pow2(Fixed128 exponent)
	{
		if (exponent.RawValue == Int128.Zero)
			return One;
		
		var negative = exponent.RawValue < Int128.Zero;
		if (negative)
			exponent = -exponent;
		
		if (exponent == One)
			return negative ? Half : 2;
		
		if (exponent >= _log2Max)
			return negative ? One / MaxValue : MaxValue;
		
		if (exponent <= _log2Min)
			return negative ? MaxValue : Zero;
		
		var integerPart = (long)Floor(exponent);
		exponent = Fract(exponent);
		
		var result = One;
		var term = One;
		var i = 1;
		
		while (term.RawValue != Int128.Zero)
		{
			term = exponent * term * Ln2 / i;
			result += term;
			i++;
		}
		
		var resultShift = result.RawValue << (int)integerPart;
		resultShift <<= (int)(integerPart >> 32);
		result = new Fixed128(resultShift);
		if (negative)
			result = One / result;
		
		return result;
	}
	
	public static Fixed128 Log2(Fixed128 value)
	{
		if (!IsPositive(value))
			throw new ArgumentOutOfRangeException(nameof(value));
		
		var b = Int128.One << (DecimalPlaces - 1);
		var y = Int128.Zero;
		
		var rawValue = value.RawValue;
		while (rawValue < _rawOne)
		{
			rawValue <<= 1;
			y -= _rawOne;
		}
		
		while (rawValue >= _rawOne << 1)
		{
			rawValue >>= 1;
			y += _rawOne;
		}
		
		var z = new Fixed128(rawValue);
		
		for (var i = 0; i < DecimalPlaces; i++)
		{
			z *= z;
			if (z.RawValue >= _rawOne << 1)
			{
				z = new Fixed128(z.RawValue >> 1);
				y += b;
			}
			
			b >>= 1;
		}
		
		return new Fixed128(y);
	}
	
	public static Fixed128 Ln(Fixed128 value)
	{
		return Log2(value) * Ln2;
	}
	
	public static Fixed128 Pow(Fixed128 @base, Fixed128 exponent)
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
	
	public static Fixed128 Sqrt(Fixed128 value)
	{
		var rawValue = value.RawValue;
		if (rawValue < Int128.Zero)
			throw new ArgumentOutOfRangeException(nameof(value));
		
		var number = new ToUnsigned(rawValue).castedValue;
		var result = UInt128.Zero;
		var bit = UInt128.One << (BitCount - 2);
		
		while (bit > number)
		{
			bit >>= 2;
		}
		
		for (var i = 0; i < 2; i++)
		{
			while (bit != UInt128.Zero)
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
			
			if (number > (UInt128.One << (BitCount / 2)) - 1)
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
		
		return new Fixed128(new ToSigned(result).castedValue);
	}
	
	private static readonly Fixed128 _cbrtSeedLower = One;
	private static readonly Fixed128 _cbrtSeedUpper = FromInteger(8);
	private const int CbrtRefinements = 3;
	
	public static Fixed128 Cbrt(Fixed128 value)
	{
		if (value == Zero)
			return Zero;
		
		var negative = value < Zero;
		var magnitude = Abs(value);
		var eight = FromInteger(8);
		var three = FromInteger(3);
		var two = FromInteger(2);
		var octaves = 0;
		
		while (magnitude < _cbrtSeedLower)
		{
			magnitude *= eight;
			octaves++;
		}
		
		while (magnitude >= _cbrtSeedUpper)
		{
			magnitude /= eight;
			octaves--;
		}
		
		var root = (Fixed128)Fixed64.Cbrt((Fixed64)magnitude);
		for (var refinement = 0; refinement < CbrtRefinements; refinement++)
			root = (root + root + magnitude / (root * root)) / three;
		
		while (octaves > 0)
		{
			root /= two;
			octaves--;
		}
		
		while (octaves < 0)
		{
			root *= two;
			octaves++;
		}
		
		return negative ? -root : root;
	}
	
	public static Fixed128 Wrap(Fixed128 value, Fixed128 minimum, Fixed128 maximum)
	{
		while (value < minimum)
			value += maximum - minimum;
		
		while (value >= maximum)
			value -= maximum - minimum;
		
		return value;
	}
	
	public static Fixed128 Tan(Fixed128 value)
	{
		return Sin(value) / Cos(value);
	}
	
	public static Fixed128 Acos(Fixed128 value)
	{
		if (value < NegativeOne || value > One)
			throw new ArgumentOutOfRangeException(nameof(value));
		
		if (IsZero(value))
			return PiOver2;
		
		var result = Atan(Sqrt(One - value * value) / value);
		return value.RawValue < 0 ? result + Pi : result;
	}
	
	public static Fixed128 Atan(Fixed128 value)
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
	
	public static Fixed128 Atan2(Fixed128 y, Fixed128 x)
	{
		var rawY = y.RawValue;
		var rawX = x.RawValue;
		
		if (rawX == Int128.Zero)
		{
			if (rawY > Int128.Zero)
				return PiOver2;
			
			if (rawY < Int128.Zero)
				return -PiOver2;
			
			return Zero;
		}
		
		var rawPointTwoEight = new Int128(0uL, 5165088340638674452uL);
		var pointTwoEight = new Fixed128(rawPointTwoEight);
		
		Fixed128 atan;
		var z = y / x;
		var zSquared = z * z;
		
		if (One + pointTwoEight * zSquared == MaxValue)
			return y < Zero ? -PiOver2 : PiOver2;
		
		if (Abs(z) < One)
		{
			atan = z / (One + pointTwoEight * zSquared);
			
			if (rawX >= Int128.Zero)
				return atan;
			
			if (rawY < Int128.Zero)
				return atan - Pi;
			
			return atan + Pi;
		}
		
		atan = PiOver2 - z / (zSquared + pointTwoEight);
		
		if (rawY < Int128.Zero)
			return atan - Pi;
		
		return atan;
	}
	
	public static Fixed128 operator -(Fixed128 operand)
	{
		return operand.RawValue == long.MinValue ? MaxValue : new Fixed128(-operand.RawValue);
	}
	
	public static bool operator ==(Fixed128 left, Fixed128 right)
	{
		return left.RawValue == right.RawValue;
	}
	
	public static bool operator !=(Fixed128 left, Fixed128 right)
	{
		return left.RawValue != right.RawValue;
	}
	
	public static bool operator >(Fixed128 left, Fixed128 right)
	{
		return left.RawValue > right.RawValue;
	}
	
	public static bool operator <(Fixed128 left, Fixed128 right)
	{
		return left.RawValue < right.RawValue;
	}
	
	public static bool operator >=(Fixed128 left, Fixed128 right)
	{
		return left.RawValue >= right.RawValue;
	}
	
	public static bool operator <=(Fixed128 left, Fixed128 right)
	{
		return left.RawValue <= right.RawValue;
	}
	
	private const double RawOneAsDouble = 18446744073709551616.0;
	private const decimal RawOneAsDecimal = 18446744073709551616.0m;
	
	public static explicit operator Fixed128(float value)
	{
		if (float.IsNaN(value))
			return Zero;
		
		var scaled = Math.Round(value * RawOneAsDouble, MidpointRounding.AwayFromZero);
		
		return scaled >= 1.7014118346046923E38 ? MaxValue :
			scaled <= -1.7014118346046923E38 ? MinValue : new Fixed128((Int128)scaled);
	}
	
	public static explicit operator float(Fixed128 value)
	{
		return (float)((double)value.RawValue / RawOneAsDouble);
	}
	
	public static explicit operator Fixed128(double value)
	{
		if (double.IsNaN(value))
			return Zero;
		
		var scaled = Math.Round(value * RawOneAsDouble, MidpointRounding.AwayFromZero);
		
		return scaled >= 1.7014118346046923E38 ? MaxValue :
			scaled <= -1.7014118346046923E38 ? MinValue : new Fixed128((Int128)scaled);
	}
	
	public static explicit operator double(Fixed128 value)
	{
		return (double)value.RawValue / RawOneAsDouble;
	}
	
	public static explicit operator Fixed128(decimal value)
	{
		var scaled = Math.Round(value * RawOneAsDecimal, MidpointRounding.AwayFromZero);
		
		return scaled >= 79228162514264337593543950335m ? MaxValue :
			scaled <= -79228162514264337593543950335m ? MinValue : new Fixed128((Int128)scaled);
	}
	
	public static explicit operator decimal(Fixed128 value)
	{
		return (decimal)value.RawValue / RawOneAsDecimal;
	}
	
	public static implicit operator Fixed128(Fixed32 value)
	{
		var lower = (ulong)((long)Fixed32.Fract(value).RawValue << (DecimalPlaces - Fixed32.DecimalPlaces));
		var upper = (ulong)((long)Fixed32.Floor(value).RawValue >> Fixed32.DecimalPlaces);
		return new Fixed128(new Int128(upper, lower));
	}
	
	public static implicit operator Fixed128(Fixed64 value)
	{
		var lower = (ulong)(Fixed64.Fract(value).RawValue << (DecimalPlaces - Fixed64.DecimalPlaces));
		var upper = (ulong)(Fixed64.Floor(value).RawValue >> Fixed64.DecimalPlaces);
		return new Fixed128(new Int128(upper, lower));
	}
	
	private Int128 ToIntegerTruncated()
	{
		var integer = RawValue >> DecimalPlaces;
		if (RawValue < Int128.Zero && (RawValue & (_rawOne - Int128.One)) != Int128.Zero)
			integer++;
		
		return integer;
	}
	
	private static Int128 ClampToRange(Int128 value, Int128 minimum, Int128 maximum) =>
		value < minimum ? minimum : value > maximum ? maximum : value;
	
	public static Fixed128 FromInteger(Int128 value) =>
		value > Int128.MaxValue >> DecimalPlaces ? MaxValue :
		value < Int128.MinValue >> DecimalPlaces ? MinValue : new Fixed128(value * _rawOne);
	
	public static explicit operator sbyte(Fixed128 value) =>
		(sbyte)ClampToRange(value.ToIntegerTruncated(), sbyte.MinValue, sbyte.MaxValue);
	
	public static explicit operator byte(Fixed128 value) =>
		(byte)ClampToRange(value.ToIntegerTruncated(), byte.MinValue, byte.MaxValue);
	
	public static explicit operator short(Fixed128 value) =>
		(short)ClampToRange(value.ToIntegerTruncated(), short.MinValue, short.MaxValue);
	
	public static explicit operator ushort(Fixed128 value) =>
		(ushort)ClampToRange(value.ToIntegerTruncated(), ushort.MinValue, ushort.MaxValue);
	
	public static explicit operator int(Fixed128 value) =>
		(int)ClampToRange(value.ToIntegerTruncated(), int.MinValue, int.MaxValue);
	
	public static explicit operator uint(Fixed128 value) =>
		(uint)ClampToRange(value.ToIntegerTruncated(), uint.MinValue, uint.MaxValue);
	
	public static explicit operator long(Fixed128 value) =>
		(long)ClampToRange(value.ToIntegerTruncated(), long.MinValue, long.MaxValue);
	
	public static explicit operator ulong(Fixed128 value) =>
		(ulong)ClampToRange(value.ToIntegerTruncated(), ulong.MinValue, ulong.MaxValue);
	
	public static explicit operator Int128(Fixed128 value) => value.ToIntegerTruncated();
	
	public static explicit operator UInt128(Fixed128 value)
	{
		var integer = value.ToIntegerTruncated();
		return integer <= Int128.Zero ? UInt128.Zero : (UInt128)integer;
	}
	
	public static implicit operator Fixed128(int value) => FromInteger(value);
	
	public static explicit operator Fixed128(uint value) => FromInteger(value);
	
	public static implicit operator Fixed128(long value) => FromInteger(value);
	
	public static explicit operator Fixed128(ulong value) => FromInteger(value);
	
	public static explicit operator Fixed128(Int128 value) => FromInteger(value);
	
	public static explicit operator Fixed128(UInt128 value) =>
		value > (UInt128)Int128.MaxValue ? MaxValue : FromInteger((Int128)value);
	
	public static int Radix => 2;
	public static Fixed128 AdditiveIdentity => Zero;
	public static Fixed128 MultiplicativeIdentity => One;
	
	public static Fixed128 operator ++(Fixed128 value) => value + One;
	
	public static Fixed128 operator --(Fixed128 value) => value - One;
	
	public static Fixed128 operator +(Fixed128 value) => value;
	
	public static bool IsCanonical(Fixed128 value) => true;
	public static bool IsComplexNumber(Fixed128 value) => false;
	public static bool IsFinite(Fixed128 value) => true;
	public static bool IsImaginaryNumber(Fixed128 value) => false;
	public static bool IsInfinity(Fixed128 value) => false;
	public static bool IsNaN(Fixed128 value) => false;
	public static bool IsNegativeInfinity(Fixed128 value) => false;
	public static bool IsPositiveInfinity(Fixed128 value) => false;
	public static bool IsNormal(Fixed128 value) => value != Zero;
	public static bool IsRealNumber(Fixed128 value) => true;
	public static bool IsSubnormal(Fixed128 value) => false;
	
	public static Fixed128 MaxMagnitude(Fixed128 x, Fixed128 y) => Abs(x) < Abs(y) ? y : x;
	public static Fixed128 MinMagnitude(Fixed128 x, Fixed128 y) => Abs(x) > Abs(y) ? y : x;
	public static Fixed128 MaxMagnitudeNumber(Fixed128 x, Fixed128 y) => MaxMagnitude(x, y);
	public static Fixed128 MinMagnitudeNumber(Fixed128 x, Fixed128 y) => MinMagnitude(x, y);
	
	public static Fixed128 CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther> =>
		FixedMath.CreateChecked<Fixed128, TOther>(value);
	
	public static Fixed128 CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther> =>
		FixedMath.CreateSaturating<Fixed128, TOther>(value);
	
	public static Fixed128 CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther> =>
		FixedMath.CreateTruncating<Fixed128, TOther>(value);
	
	public static Fixed128 Parse(string number, NumberStyles style, IFormatProvider? provider) => Parse(number);
	
	public static Fixed128 Parse(string number, IFormatProvider? provider) => Parse(number);
	
	public static Fixed128 Parse(ReadOnlySpan<char> number, IFormatProvider? provider) => Parse(new string(number));
	
	public static Fixed128 Parse(ReadOnlySpan<char> number, NumberStyles style, IFormatProvider? provider) =>
		Parse(new string(number));
	
	public static bool TryParse(string? number, NumberStyles style, IFormatProvider? provider, out Fixed128 result) =>
		TryParse(number ?? "", out result);
	
	public static bool TryParse(string? number, IFormatProvider? provider, out Fixed128 result) =>
		TryParse(number ?? "", out result);
	
	public static bool TryParse(ReadOnlySpan<char> number, IFormatProvider? provider, out Fixed128 result) =>
		TryParse(new string(number), out result);
	
	public int ToInt() => (int)this;
	public long ToLong() => (long)this;
	
	public static bool TryParse(ReadOnlySpan<char> number, NumberStyles style, IFormatProvider? provider,
		out Fixed128 result) => TryParse(new string(number), out result);
	
	public static Fixed128 Exp(Fixed128 x) => Pow(E, x);
	public static Fixed128 ExpM1(Fixed128 x) => Pow(E, x) - One;
	public static Fixed128 Exp2(Fixed128 x) => Pow(One + One, x);
	public static Fixed128 Exp2M1(Fixed128 x) => Pow(One + One, x) - One;
	public static Fixed128 Exp10(Fixed128 x) => Pow(FromInteger(10), x);
	public static Fixed128 Exp10M1(Fixed128 x) => Pow(FromInteger(10), x) - One;
	
	public override bool Equals(object? obj)
	{
		return obj is Fixed128 fixedValue && Equals(fixedValue);
	}
	
	public override int GetHashCode()
	{
		return RawValue.GetHashCode();
	}
	
	public bool Equals(Fixed128 other)
	{
		return RawValue.Equals(other.RawValue);
	}
	
	public int CompareTo(Fixed128 other)
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