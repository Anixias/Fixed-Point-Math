namespace FixedPointMath;

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

			var minX = Fixed64.Min(value.x, end.x);
			var minY = Fixed64.Min(value.y, end.y);
			
			var maxX = Fixed64.Max(value.x, end.x);
			var maxY = Fixed64.Max(value.y, end.y);

			position = new Vector2X(minX, minY);
			size = new Vector2X(maxX, maxY) - position;
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
			
			var minX = Fixed64.Min(position.x, value.x);
			var minY = Fixed64.Min(position.y, value.y);
			
			var maxX = Fixed64.Max(position.x, value.x);
			var maxY = Fixed64.Max(position.y, value.y);

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
		var position = new Vector2X(Fixed64.Min(start.x, end.x), Fixed64.Min(start.y, end.y));
		var size = new Vector2X(Fixed64.Max(start.x, end.x), Fixed64.Max(start.y, end.y)) - position;
		return new Rect2X(position, size);
	}
	
	public readonly bool Intersects(Rect2X rect)
	{
		return position.x < rect.End.x && End.x > rect.position.x && 
		       position.y < rect.End.y && End.y > rect.position.y;
	}

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