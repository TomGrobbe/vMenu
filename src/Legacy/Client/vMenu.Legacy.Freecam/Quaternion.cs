namespace System.Numerics;

// FiveM's trimmed mscorlib has no System.Numerics, so this copies the parts the freecam uses with the same maths.
internal readonly struct Quaternion(float x, float y, float z, float w)
{
    public float X { get; } = x;

    public float Y { get; } = y;

    public float Z { get; } = z;

    public float W { get; } = w;

    public static Quaternion Identity => new(0f, 0f, 0f, 1f);

    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        var half = angle * 0.5f;
        var sin = MathF.Sin(half);

        return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, MathF.Cos(half));
    }

    public static Quaternion Normalize(Quaternion value)
    {
        var length = MathF.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z) + (value.W * value.W));

        return new Quaternion(value.X / length, value.Y / length, value.Z / length, value.W / length);
    }

    public static Quaternion Inverse(Quaternion value)
    {
        var lengthSquared = (value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z) + (value.W * value.W);

        return new Quaternion(-value.X / lengthSquared, -value.Y / lengthSquared, -value.Z / lengthSquared, value.W / lengthSquared);
    }

    public static Quaternion Slerp(Quaternion from, Quaternion to, float amount)
    {
        var cos = (from.X * to.X) + (from.Y * to.Y) + (from.Z * to.Z) + (from.W * to.W);
        var flip = cos < 0f;

        if (flip)
        {
            cos = -cos;
        }

        float fromWeight;
        float toWeight;

        if (cos > 0.999999f)
        {
            fromWeight = 1f - amount;
            toWeight = flip ? -amount : amount;
        }
        else
        {
            var angle = MathF.Acos(cos);
            var sin = 1f / MathF.Sin(angle);

            fromWeight = MathF.Sin((1f - amount) * angle) * sin;
            toWeight = flip ? -MathF.Sin(amount * angle) * sin : MathF.Sin(amount * angle) * sin;
        }

        return new Quaternion(
            (fromWeight * from.X) + (toWeight * to.X),
            (fromWeight * from.Y) + (toWeight * to.Y),
            (fromWeight * from.Z) + (toWeight * to.Z),
            (fromWeight * from.W) + (toWeight * to.W));
    }

    public static Quaternion operator *(Quaternion left, Quaternion right)
    {
        var cx = (left.Y * right.Z) - (left.Z * right.Y);
        var cy = (left.Z * right.X) - (left.X * right.Z);
        var cz = (left.X * right.Y) - (left.Y * right.X);
        var dot = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

        return new Quaternion(
            (left.X * right.W) + (right.X * left.W) + cx,
            (left.Y * right.W) + (right.Y * left.W) + cy,
            (left.Z * right.W) + (right.Z * left.W) + cz,
            (left.W * right.W) - dot);
    }

    public static implicit operator CitizenFX.Core.Quaternion(Quaternion value) =>
        new() { X = value.X, Y = value.Y, Z = value.Z, W = value.W };
}
