using System.Runtime.CompilerServices;

namespace FixedPointMath;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable IdentifierTypo
public struct Vector2X : IEquatable<Vector2X>
{
	public Fixed64 X;
	public Fixed64 Y;
	
	public static Vector2X Zero => new(Fixed64.Zero, Fixed64.Zero);
	public static Vector2X One => new(Fixed64.One, Fixed64.One);
	public static Vector2X Up => new(Fixed64.Zero, Fixed64.NegativeOne);
	public static Vector2X Down => new(Fixed64.Zero, Fixed64.One);
	public static Vector2X Left => new(Fixed64.NegativeOne, Fixed64.Zero);
	public static Vector2X Right => new(Fixed64.One, Fixed64.Zero);
	
	public Vector2X(Fixed64 x, Fixed64 y)
	{
		this.X = x;
		this.Y = y;
	}
	
	public readonly Vector2X Abs() => new(X.Abs(), Y.Abs());
	
	public readonly Fixed64 Angle() => Fixed64.Atan2(Y, X);
	
	public readonly Fixed64 AngleTo(in Vector2X vec) => Fixed64.Atan2(Cross(vec), Dot(vec));
	
	public readonly Fixed64 AngleToPoint(in Vector2X vec) => Fixed64.Atan2(vec.Y - Y, vec.X - X);
	
	public readonly Vector2X Ceil() => new(X.Ceil(), Y.Ceil());
	
	public readonly Vector2X Clamp(in Vector2X min, in Vector2X max)
	{
		return new(X.Clamp(min.X, max.X), Y.Clamp(min.Y, max.Y));
	}
	
	public readonly Fixed64 Cross(in Vector2X vec)
	{
		return X * vec.Y - Y * vec.X;
	}
	
	public readonly Vector2X DirectionTo(in Vector2X vec)
	{
		return new Vector2X(vec.X - X, vec.Y - Y).Normalized();
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
		return X * vec.X + Y * vec.Y;
	}
	
	public readonly Vector2X Floor() => new(X.Floor(), Y.Floor());
	
	public readonly Vector2X Fract() => new(X.Fract(), Y.Fract());
	
	public readonly Fixed64 Length() => Fixed64.Sqrt(X * X + Y * Y);
	
	public readonly Fixed64 LengthSquared() => X * X + Y * Y;
	
	public readonly Vector2X Lerp(in Vector2X target, Fixed64 weight)
	{
		return new(X.Lerp(target.X, weight), Y.Lerp(target.Y, weight));
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
	
	public Vector2X PosMod(Fixed64 divisor) => new(X.PosMod(divisor), Y.PosMod(divisor));
	
	public Vector2X PosMod(Vector2X vec) => new(X.PosMod(vec.X), Y.PosMod(vec.Y));
	
	public readonly Vector2X Rotated(Fixed64 angle)
	{
		var (sin, cos) = angle.SinCos();
		return new Vector2X(X * cos - Y * sin, X * sin + Y * cos);
	}
	
	public readonly Vector2X Round() => new(X.Round(), Y.Round());
	
	public readonly Vector2X Sign() => new(X.Sign(), Y.Sign());
	
	public Vector2X Snapped(Vector2X step) => new(X.Snapped(step.X), Y.Snapped(step.Y));
	
	public static Vector2X FromAngle(Fixed64 angle)
	{
		var (sin, cos) = angle.SinCos();
		return new Vector2X(cos, sin);
	}
	
	public static Vector2X operator +(Vector2X left, in Vector2X right)
	{
		left.X += right.X;
		left.Y += right.Y;
		return left;
	}
	
	public static Vector2X operator -(Vector2X left, in Vector2X right)
	{
		left.X -= right.X;
		left.Y -= right.Y;
		return left;
	}
	
	public static Vector2X operator -(Vector2X vec)
	{
		vec.X = -vec.X;
		vec.Y = -vec.Y;
		return vec;
	}
	
	public static Vector2X operator *(Vector2X vec, Fixed64 scalar)
	{
		vec.X *= scalar;
		vec.Y *= scalar;
		return vec;
	}
	
	public static Vector2X operator *(Fixed64 scalar, Vector2X vec)
	{
		vec.X *= scalar;
		vec.Y *= scalar;
		return vec;
	}
	
	public static Vector2X operator *(Vector2X left, in Vector2X right)
	{
		left.X *= right.X;
		left.Y *= right.Y;
		return left;
	}
	
	public static Vector2X operator /(Vector2X vec, Fixed64 divisor)
	{
		vec.X /= divisor;
		vec.Y /= divisor;
		return vec;
	}
	
	public static Vector2X operator /(Vector2X left, in Vector2X right)
	{
		left.X /= right.X;
		left.Y /= right.Y;
		return left;
	}
	
	public static Vector2X operator %(Vector2X vec, Fixed64 divisor)
	{
		vec.X %= divisor;
		vec.Y %= divisor;
		return vec;
	}
	
	public static Vector2X operator %(Vector2X left, in Vector2X right)
	{
		left.X %= right.X;
		left.Y %= right.Y;
		return left;
	}
	
	public static bool operator ==(Vector2X left, in Vector2X right) => left.Equals(right);
	
	public static bool operator !=(Vector2X left, in Vector2X right) => !left.Equals(right);
	
	public bool Equals(Vector2X other)
	{
		return X == other.X && Y == other.Y;
	}
	
	public override bool Equals(object? obj)
	{
		return obj is Vector2X other && Equals(other);
	}
	
	public override int GetHashCode()
	{
		return HashCode.Combine(X.RawValue, Y.RawValue);
	}
	
	public override string ToString() => $"({X}, {Y})";
}