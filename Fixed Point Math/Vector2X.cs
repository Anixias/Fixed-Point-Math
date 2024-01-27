using System.Runtime.CompilerServices;

namespace FixedPointMath;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable IdentifierTypo
public struct Vector2X : IEquatable<Vector2X>
{
	public Fixed64 x;
	public Fixed64 y;

	public static Vector2X Zero => new(Fixed64.Zero, Fixed64.Zero);
	public static Vector2X One => new(Fixed64.One, Fixed64.One);
	public static Vector2X Up => new(Fixed64.Zero, Fixed64.NegativeOne);
	public static Vector2X Down => new(Fixed64.Zero, Fixed64.One);
	public static Vector2X Left => new(Fixed64.NegativeOne, Fixed64.Zero);
	public static Vector2X Right => new(Fixed64.One, Fixed64.Zero);

	public Vector2X(Fixed64 x, Fixed64 y)
	{
		this.x = x;
		this.y = y;
	}

	public readonly Vector2X Abs() => new(x.Abs(), y.Abs());
	
	public readonly Fixed64 Angle() => Fixed64.Atan2(y, x);
	
	public readonly Fixed64 AngleTo(in Vector2X vec) => Fixed64.Atan2(Cross(vec), Dot(vec));
	
	public readonly Fixed64 AngleToPoint(in Vector2X vec) => Fixed64.Atan2(vec.y - y, vec.x - x);
	
	public readonly Vector2X Ceil() => new(x.Ceil(), y.Ceil());

	public readonly Vector2X Clamp(in Vector2X min, in Vector2X max)
	{
		return new(x.Clamp(min.x, max.x), y.Clamp(min.y, max.y));
	}

	public readonly Fixed64 Cross(in Vector2X vec)
	{
		return x * vec.y - y * vec.x;
	}
	
	public readonly Vector2X DirectionTo(in Vector2X vec)
	{
		return new Vector2X(vec.x - x, vec.y - y).Normalized();
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Fixed64 DistanceSquaredTo(in Vector2X vec)
	{
		return (vec - this).LengthSquared();
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Fixed64 DistanceTo(in Vector2X vec)
	{
		return (vec - this).Length();
	}
	
	public readonly Fixed64 Dot(in Vector2X vec)
	{
		return x * vec.x + y * vec.y;
	}
	
	public readonly Vector2X Floor() => new(x.Floor(), y.Floor());
	
	public readonly Vector2X Fract() => new(x.Fract(), y.Fract());
	
	public readonly Fixed64 Length() => Fixed64.Sqrt(x * x + y * y);
	
	public readonly Fixed64 LengthSquared() => x * x + y * y;

	public readonly Vector2X Lerp(in Vector2X target, Fixed64 weight)
	{
		return new(x.Lerp(target.x, weight), y.Lerp(target.y, weight));
	}
	
	public readonly Vector2X MoveToward(in Vector2X target, Fixed64 step)
	{
		var difference = target - this;
		var distance = difference.Length();

		return distance <= step ? target : this + difference / distance * step;
	}

	public readonly Vector2X Normalized()
	{
		var lengthSquared = LengthSquared();
		if (lengthSquared.IsZero())
			return Zero;

		return this / Fixed64.Sqrt(lengthSquared);
	}

	public Vector2X PosMod(Fixed64 divisor) => new(x.PosMod(divisor), y.PosMod(divisor));

	public Vector2X PosMod(Vector2X vec) => new(x.PosMod(vec.x), y.PosMod(vec.y));
	
	public readonly Vector2X Rotated(Fixed64 angle)
	{
		var (sin, cos) = angle.SinCos();
		return new Vector2X(x * cos - y * sin, x * sin + y * cos);
	}
	
	public readonly Vector2X Round() => new(x.Round(), y.Round());
	
	public readonly Vector2X Sign() => new(x.Sign(), y.Sign());

	public Vector2X Snapped(Vector2X step) => new(x.Snapped(step.x), y.Snapped(step.y));
	
	public static Vector2X FromAngle(Fixed64 angle)
	{
		var (sin, cos) = angle.SinCos();
		return new Vector2X(cos, sin);
	}
	
	public static Vector2X operator +(Vector2X left, in Vector2X right)
	{
		left.x += right.x;
		left.y += right.y;
		return left;
	}
	
	public static Vector2X operator -(Vector2X left, in Vector2X right)
	{
		left.x -= right.x;
		left.y -= right.y;
		return left;
	}
	
	public static Vector2X operator -(Vector2X vec)
	{
		vec.x = -vec.x;
		vec.y = -vec.y;
		return vec;
	}
	
	public static Vector2X operator *(Vector2X vec, Fixed64 scalar)
	{
		vec.x *= scalar;
		vec.y *= scalar;
		return vec;
	}
	
	public static Vector2X operator *(Fixed64 scalar, Vector2X vec)
	{
		vec.x *= scalar;
		vec.y *= scalar;
		return vec;
	}
	
	public static Vector2X operator *(Vector2X left, in Vector2X right)
	{
		left.x *= right.x;
		left.y *= right.y;
		return left;
	}
	
	public static Vector2X operator /(Vector2X vec, Fixed64 divisor)
	{
		vec.x /= divisor;
		vec.y /= divisor;
		return vec;
	}
	
	public static Vector2X operator /(Vector2X left, in Vector2X right)
	{
		left.x /= right.x;
		left.y /= right.y;
		return left;
	}
	
	public static Vector2X operator %(Vector2X vec, Fixed64 divisor)
	{
		vec.x %= divisor;
		vec.y %= divisor;
		return vec;
	}
	
	public static Vector2X operator %(Vector2X left, in Vector2X right)
	{
		left.x %= right.x;
		left.y %= right.y;
		return left;
	}

	public static bool operator ==(Vector2X left, in Vector2X right) => left.Equals(right);
	
	public static bool operator !=(Vector2X left, in Vector2X right) => !left.Equals(right);

	public bool Equals(Vector2X other)
	{
		return x == other.x && y == other.y;
	}

	public override bool Equals(object? obj)
	{
		return obj is Vector2X other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x.RawValue, y.RawValue);
	}

	public override string ToString() => $"({x}, {y})";
}
