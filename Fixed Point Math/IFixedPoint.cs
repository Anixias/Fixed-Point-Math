using System.Globalization;
using System.Numerics;
using System.Text;

namespace FixedPointMath;

// ReSharper disable UnusedMemberInSuper.Global
public interface IFixedPoint<T> : INumber<T>, IMinMaxValue<T>, IExponentialFunctions<T>
	where T : IFixedPoint<T>
{
	static abstract int FractionalBits { get; }
	
	static abstract T Half { get; }
	static abstract T PiOver2 { get; }
	static abstract T Ln2 { get; }
	
	Int128 RawBits { get; }
	
	static abstract T FromRawBits(Int128 rawBits);
	static abstract T FromInteger(Int128 value);
	
	static abstract T Floor(T value);
	static abstract T Round(T value);
	static abstract T Pow(T value, T exponent);
	
	static abstract T Parse(string number);
	static abstract bool TryParse(string number, out T result);
	
	static T IExponentialFunctions<T>.Exp(T x) => T.Pow(T.E, x);
	static T IExponentialFunctions<T>.ExpM1(T x) => T.Pow(T.E, x) - T.One;
	static T IExponentialFunctions<T>.Exp2(T x) => T.Pow(T.One + T.One, x);
	static T IExponentialFunctions<T>.Exp2M1(T x) => T.Pow(T.One + T.One, x) - T.One;
	static T IExponentialFunctions<T>.Exp10(T x) => T.Pow(T.FromInteger(10), x);
	static T IExponentialFunctions<T>.Exp10M1(T x) => T.Pow(T.FromInteger(10), x) - T.One;
	
	static int INumberBase<T>.Radix => 2;
	static T IAdditiveIdentity<T, T>.AdditiveIdentity => T.Zero;
	static T IMultiplicativeIdentity<T, T>.MultiplicativeIdentity => T.One;
	
	static T IIncrementOperators<T>.operator ++(T value) => value + T.One;
	static T IDecrementOperators<T>.operator --(T value) => value - T.One;
	static T IUnaryPlusOperators<T, T>.operator +(T value) => value;
	
	static bool INumberBase<T>.IsCanonical(T value) => true;
	static bool INumberBase<T>.IsComplexNumber(T value) => false;
	static bool INumberBase<T>.IsFinite(T value) => true;
	static bool INumberBase<T>.IsImaginaryNumber(T value) => false;
	static bool INumberBase<T>.IsInfinity(T value) => false;
	static bool INumberBase<T>.IsNaN(T value) => false;
	static bool INumberBase<T>.IsNegativeInfinity(T value) => false;
	static bool INumberBase<T>.IsPositiveInfinity(T value) => false;
	static bool INumberBase<T>.IsNormal(T value) => value != T.Zero;
	static bool INumberBase<T>.IsRealNumber(T value) => true;
	static bool INumberBase<T>.IsSubnormal(T value) => false;
	
	static T INumberBase<T>.MaxMagnitude(T x, T y) => T.Abs(x) < T.Abs(y) ? y : x;
	static T INumberBase<T>.MinMagnitude(T x, T y) => T.Abs(x) > T.Abs(y) ? y : x;
	static T INumberBase<T>.MaxMagnitudeNumber(T x, T y) => T.MaxMagnitude(x, y);
	static T INumberBase<T>.MinMagnitudeNumber(T x, T y) => T.MinMagnitude(x, y);
	
	static bool INumberBase<T>.TryConvertFromChecked<TOther>(TOther value, out T result) =>
		FixedMath.TryConvertFrom(value, out result);
	
	static bool INumberBase<T>.TryConvertFromSaturating<TOther>(TOther value, out T result) =>
		FixedMath.TryConvertFrom(value, out result);
	
	static bool INumberBase<T>.TryConvertFromTruncating<TOther>(TOther value, out T result) =>
		FixedMath.TryConvertFrom(value, out result);
	
	static bool INumberBase<T>.TryConvertToChecked<TOther>(T value, out TOther result) =>
		FixedMath.TryConvertTo(value, out result!);
	
	static bool INumberBase<T>.TryConvertToSaturating<TOther>(T value, out TOther result) =>
		FixedMath.TryConvertTo(value, out result!);
	
	static bool INumberBase<T>.TryConvertToTruncating<TOther>(T value, out TOther result) =>
		FixedMath.TryConvertTo(value, out result!);
	
	static T INumberBase<T>.Parse(string number, NumberStyles style, IFormatProvider? provider) => T.Parse(number);
	
	static T INumberBase<T>.Parse(ReadOnlySpan<char> number, NumberStyles style, IFormatProvider? provider) =>
		T.Parse(new string(number));
	
	static T IParsable<T>.Parse(string number, IFormatProvider? provider) => T.Parse(number);
	
	static T ISpanParsable<T>.Parse(ReadOnlySpan<char> number, IFormatProvider? provider) =>
		T.Parse(new string(number));
	
	static T IUtf8SpanParsable<T>.Parse(ReadOnlySpan<byte> number, IFormatProvider? provider) =>
		T.Parse(Encoding.UTF8.GetString(number));
	
	static bool INumberBase<T>.TryParse(string? number, NumberStyles style, IFormatProvider? provider, out T result) =>
		TryParseOrZero(number, out result);
	
	static bool INumberBase<T>.TryParse(ReadOnlySpan<char> number, NumberStyles style, IFormatProvider? provider,
		out T result) => TryParseOrZero(new string(number), out result);
	
	static bool IParsable<T>.TryParse(string? number, IFormatProvider? provider, out T result) =>
		TryParseOrZero(number, out result);
	
	static bool ISpanParsable<T>.TryParse(ReadOnlySpan<char> number, IFormatProvider? provider, out T result) =>
		TryParseOrZero(new string(number), out result);
	
	static bool IUtf8SpanParsable<T>.TryParse(ReadOnlySpan<byte> number, IFormatProvider? provider, out T result) =>
		TryParseOrZero(Encoding.UTF8.GetString(number), out result);
	
	int IComparable.CompareTo(object? value) => value switch
	{
		null => 1,
		T other => CompareTo(other),
		_ => throw new ArgumentException($"Cannot compare a fixed-point value to {value.GetType().Name}.",
			nameof(value))
	};
	
	int ToInt();
	long ToLong();
	
	private static bool TryParseOrZero(string? number, out T result)
	{
		if (number is not null)
			return T.TryParse(number, out result);
		
		result = T.Zero;
		return false;
	}
	
	string ToString(string? format);
}