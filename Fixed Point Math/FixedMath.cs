using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace FixedPointMath;

public static partial class FixedMath
{
	[GeneratedRegex(@"\s+")]
	private static partial Regex WhitespaceRegexImpl();
	
	private static readonly Regex _whitespaceRegex = WhitespaceRegexImpl();
	
	private static class Trig<T> where T : IFixedPoint<T>
	{
		public static readonly T TwoOverPi = T.One / T.PiOver2;
		public static readonly T[] Cosine = Series(0);
		public static readonly T[] Sine = Series(1);
		
		private static T[] Series(int firstDegree)
		{
			var coefficients = new List<T>();
			var scale = BigInteger.One << T.FractionalBits;
			var factorial = BigInteger.One;
			for (var i = 2; i <= firstDegree; i++)
				factorial *= i;
			
			var degree = firstDegree;
			while (true)
			{
				var rawBits = (scale + factorial / 2) / factorial;
				if (rawBits.IsZero)
					return [..coefficients];
				
				var term = T.FromRawBits((Int128)rawBits);
				coefficients.Add(coefficients.Count % 2 == 0 ? term : -term);
				
				factorial *= (degree + 1) * (degree + 2);
				degree += 2;
			}
		}
	}
	
	public static bool TryConvertFrom<T, TOther>(TOther value, out T result)
		where T : IFixedPoint<T> where TOther : INumberBase<TOther>
	{
		try
		{
			result = FromDouble<T>(double.CreateChecked(value));
			return true;
		}
		catch (Exception exception) when (exception is NotSupportedException or OverflowException)
		{
			result = T.Zero;
			return false;
		}
	}
	
	public static bool TryConvertTo<T, TOther>(T value, out TOther result)
		where T : IFixedPoint<T> where TOther : INumberBase<TOther>
	{
		try
		{
			result = TOther.CreateChecked(ToDouble(value));
			return true;
		}
		catch (Exception exception) when (exception is NotSupportedException or OverflowException)
		{
			result = TOther.Zero;
			return false;
		}
	}
	
	public static T CreateChecked<T, TOther>(TOther value)
		where T : IFixedPoint<T> where TOther : INumberBase<TOther> => Create<T, TOther>(value);
	
	public static T CreateSaturating<T, TOther>(TOther value)
		where T : IFixedPoint<T> where TOther : INumberBase<TOther> => Create<T, TOther>(value);
	
	public static T CreateTruncating<T, TOther>(TOther value)
		where T : IFixedPoint<T> where TOther : INumberBase<TOther> => Create<T, TOther>(value);
	
	private static T Create<T, TOther>(TOther value)
		where T : IFixedPoint<T> where TOther : INumberBase<TOther> =>
		TryConvertFrom<T, TOther>(value, out var result)
			? result
			: throw new NotSupportedException($"Cannot convert {typeof(TOther).Name} to {typeof(T).Name}.");
	
	public static T FromDouble<T>(double value) where T : IFixedPoint<T>
	{
		if (double.IsNaN(value))
			return T.Zero;
		
		var scaled = Math.Round(value * Math.ScaleB(1.0, T.FractionalBits), MidpointRounding.AwayFromZero);
		
		return scaled >= 1.7014118346046923E38 ? T.MaxValue :
			scaled <= -1.7014118346046923E38 ? T.MinValue : T.FromRawBits((Int128)scaled);
	}
	
	public static double ToDouble<T>(T value) where T : IFixedPoint<T> =>
		(double)value.RawBits / Math.ScaleB(1.0, T.FractionalBits);
	
	private static class DecimalScale<T> where T : IFixedPoint<T>
	{
		// ReSharper disable once StaticMemberInGenericType
		public static readonly BigInteger FiveToTheFractionalBits = BigInteger.Pow(5, T.FractionalBits);
	}
	
	public static string ToString<T>(T value) where T : IFixedPoint<T> =>
		Exact(value, NumberFormatInfo.InvariantInfo);
	
	public static string ToString<T>(T value, string? format, IFormatProvider? provider) where T : IFixedPoint<T>
	{
		var info = NumberFormatInfo.GetInstance(provider);
		if (string.IsNullOrEmpty(format))
			return Exact(value, info);
		
		var hasPlaces = int.TryParse(format.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var places);
		
		switch (char.ToUpperInvariant(format[0]))
		{
			case 'R':
			case 'G' when !hasPlaces:
				return Exact(value, info);
			
			case 'F':
				return Rounded(value, hasPlaces ? places : info.NumberDecimalDigits, info, false);
			
			case 'N':
				return Rounded(value, hasPlaces ? places : info.NumberDecimalDigits, info, true);
			
			default:
				return ToDouble(value).ToString(format, provider);
		}
	}
	
	public static bool TryFormat<T>(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
		IFormatProvider? provider) where T : IFixedPoint<T>
	{
		var text = ToString(value, format.IsEmpty ? null : new string(format), provider);
		if (text.Length > destination.Length)
		{
			charsWritten = 0;
			return false;
		}
		
		text.CopyTo(destination);
		charsWritten = text.Length;
		return true;
	}
	
	public static bool TryFormat<T>(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format,
		IFormatProvider? provider) where T : IFixedPoint<T> =>
		Encoding.UTF8.TryGetBytes(ToString(value, format.IsEmpty ? null : new string(format), provider), destination,
			out bytesWritten);
	
	private static string Exact<T>(T value, NumberFormatInfo info) where T : IFixedPoint<T>
	{
		var rawBits = (BigInteger)value.RawBits;
		var magnitude = BigInteger.Abs(rawBits);
		var integerPart = magnitude >> T.FractionalBits;
		var fraction = magnitude - (integerPart << T.FractionalBits);
		
		var builder = new StringBuilder();
		if (rawBits.Sign < 0)
			builder.Append(info.NegativeSign);
		
		builder.Append(integerPart.ToString(CultureInfo.InvariantCulture));
		
		if (fraction.IsZero)
			return builder.ToString();
		
		var digits = (fraction * DecimalScale<T>.FiveToTheFractionalBits)
			.ToString(CultureInfo.InvariantCulture)
			.PadLeft(T.FractionalBits, '0')
			.TrimEnd('0');
		
		return builder.Append(info.NumberDecimalSeparator).Append(digits).ToString();
	}
	
	private static string Rounded<T>(T value, int places, NumberFormatInfo info, bool grouped)
		where T : IFixedPoint<T>
	{
		ArgumentOutOfRangeException.ThrowIfNegative(places);
		
		var rawBits = (BigInteger)value.RawBits;
		var magnitude = BigInteger.Abs(rawBits);
		var power = BigInteger.Pow(10, places);
		var denominator = BigInteger.One << T.FractionalBits;
		var scaled = (magnitude * power * 2 + denominator) / (denominator * 2);
		
		var integerPart = scaled / power;
		var fraction = scaled - integerPart * power;
		
		var builder = new StringBuilder();
		if (rawBits.Sign < 0)
			builder.Append(info.NegativeSign);
		
		var whole = integerPart.ToString(CultureInfo.InvariantCulture);
		builder.Append(grouped ? Group(whole, info) : whole);
		
		if (places > 0)
		{
			builder.Append(info.NumberDecimalSeparator)
				.Append(fraction.ToString(CultureInfo.InvariantCulture).PadLeft(places, '0'));
		}
		
		return builder.ToString();
	}
	
	private static string Group(string digits, NumberFormatInfo info)
	{
		var sizes = info.NumberGroupSizes;
		if (sizes.Length == 0 || sizes[0] <= 0 || info.NumberGroupSeparator.Length == 0)
			return digits;
		
		var size = sizes[0];
		var builder = new StringBuilder();
		for (var i = 0; i < digits.Length; i++)
		{
			if (i > 0 && (digits.Length - i) % size == 0)
				builder.Append(info.NumberGroupSeparator);
			
			builder.Append(digits[i]);
		}
		
		return builder.ToString();
	}
	
	public static ReadOnlySpan<T> CosineSeries<T>() where T : IFixedPoint<T> => Trig<T>.Cosine;
	
	public static ReadOnlySpan<T> SineSeries<T>() where T : IFixedPoint<T> => Trig<T>.Sine;
	
	public static T Sin<T>(T value) where T : IFixedPoint<T> => SinCos(value).Sin;
	
	public static T Cos<T>(T value) where T : IFixedPoint<T> => SinCos(value).Cos;
	
	public static (T Sin, T Cos) SinCos<T>(T value) where T : IFixedPoint<T>
	{
		var turns = T.Round(value * Trig<T>.TwoOverPi);
		var remainder = value - turns * T.PiOver2;
		
		var sine = Polynomial(remainder, Trig<T>.Sine) * remainder;
		var cosine = Polynomial(remainder, Trig<T>.Cosine);
		
		var quadrant = (int)((turns.RawBits >> T.FractionalBits) & 3);
		
		return quadrant switch
		{
			0 => (sine, cosine),
			1 => (cosine, -sine),
			2 => (-sine, -cosine),
			_ => (-cosine, sine)
		};
	}
	
	private static T Polynomial<T>(T value, T[] coefficients) where T : IFixedPoint<T>
	{
		var squared = value * value;
		var result = coefficients[^1];
		
		for (var i = coefficients.Length - 2; i >= 0; i--)
			result = coefficients[i] + squared * result;
		
		return result;
	}
	
	public static T Parse<T>(string number) where T : IFixedPoint<T>
	{
		ArgumentNullException.ThrowIfNull(number);
		
		var text = _whitespaceRegex.Replace(number, "");
		if (text.Length == 0)
			throw new ArgumentException("Cannot parse an empty number.", nameof(number));
		
		var negative = text[0] == '-';
		if (negative || text[0] == '+')
			text = text[1..];
		
		var separator = text.IndexOf('.');
		if (separator >= 0 && text.IndexOf('.', separator + 1) >= 0)
			throw new ArgumentException("Cannot have more than one decimal point", nameof(number));
		
		var wholeText = separator < 0 ? text : text[..separator];
		var fractionText = separator < 0 ? "" : text[(separator + 1)..];
		
		if (wholeText.Length == 0 && fractionText.Length == 0)
			throw new ArgumentException("Failed to parse integer part", nameof(number));
		
		if (!IsDigits(wholeText) || !IsDigits(fractionText))
			throw new ArgumentException("Failed to parse integer part", nameof(number));
		
		var bits = T.FractionalBits;
		var rawBits = wholeText.Length == 0 ? BigInteger.Zero : BigInteger.Parse(wholeText) << bits;
		
		if (fractionText.Length > 0)
		{
			var scale = BigInteger.Pow(10, fractionText.Length);
			var fraction = BigInteger.Parse(fractionText);
			rawBits += ((fraction << (bits + 1)) + scale) / (scale << 1);
		}
		
		if (negative)
			rawBits = -rawBits;
		
		return T.FromRawBits(Saturate(rawBits));
	}
	
	public static bool TryParse<T>(string number, out T result) where T : IFixedPoint<T>
	{
		try
		{
			result = Parse<T>(number);
			return true;
		}
		catch (Exception e) when (e is ArgumentException or FormatException)
		{
			result = T.Zero;
			return false;
		}
	}
	
	private static Int128 Saturate(BigInteger rawBits) =>
		rawBits > Int128.MaxValue ? Int128.MaxValue :
		rawBits < Int128.MinValue ? Int128.MinValue : (Int128)rawBits;
	
	private static bool IsDigits(string text)
	{
		foreach (var character in text)
		{
			if (character is < '0' or > '9')
				return false;
		}
		
		return true;
	}
}