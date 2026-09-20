namespace FixedPointMath;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable IdentifierTypo
public struct Rect2X : IEquatable<Rect2X>
{
	public Vector2X Start
	{
		readonly get => position;
		set
		{
			var end = End;
			
			if (value == end)
			{
				position = end;
				size = Vector2X.Zero;
				return;
			}
			
			position = value;
			End = end;
		}
	}
	
	public Vector2X End
	{
		readonly get => position + size;
		set
		{
			if (value == position)
			{
				size = Vector2X.Zero;
				return;
			}
			
			var minX = Fixed64.Min(position.X, value.X);
			var minY = Fixed64.Min(position.Y, value.Y);
			
			var maxX = Fixed64.Max(position.X, value.X);
			var maxY = Fixed64.Max(position.Y, value.Y);
			
			position = new Vector2X(minX, minY);
			size = new Vector2X(maxX, maxY) - position;
		}
	}
	
	private Vector2X position;
	private Vector2X size;
	
	public Rect2X(Vector2X position, Vector2X size)
	{
		this.position = position;
		this.size = size;
	}
	
	public static Rect2X FromBounds(Vector2X start, Vector2X end)
	{
		var position = new Vector2X(Fixed64.Min(start.X, end.X), Fixed64.Min(start.Y, end.Y));
		var size = new Vector2X(Fixed64.Max(start.X, end.X), Fixed64.Max(start.Y, end.Y)) - position;
		return new Rect2X(position, size);
	}
	
	public readonly bool Intersects(Rect2X rect)
	{
		return position.X < rect.End.X && End.X > rect.position.X &&
		       position.Y < rect.End.Y && End.Y > rect.position.Y;
	}
	
	public static bool operator ==(Rect2X left, Rect2X right) => left.Equals(right);
	
	public static bool operator !=(Rect2X left, Rect2X right) => !(left == right);
	
	public bool Equals(Rect2X other)
	{
		return position == other.position && size == other.size;
	}
	
	public override bool Equals(object? obj)
	{
		return obj is Rect2X other && Equals(other);
	}
	
	public override int GetHashCode()
	{
		return HashCode.Combine(position, size);
	}
}