using System.Numerics;

namespace vMenu.Enhanced.Freecam;

// Rotations are degrees in the game's rotation order 2 (YXZ): X pitch, Y roll, Z yaw.
internal static class Orientation
{
    internal const int RotationOrder = 2;

    private const float ToRadians = MathF.PI / 180f;

    private const float ToDegrees = 180f / MathF.PI;

    internal static Quaternion FromEuler(Vector3 rotation) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rotation.Z * ToRadians)
        * Quaternion.CreateFromAxisAngle(Vector3.UnitX, rotation.X * ToRadians)
        * Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotation.Y * ToRadians);

    internal static Vector3 ToEuler(Quaternion q)
    {
        q = Quaternion.Normalize(q);

        var m21 = 2f * ((q.Y * q.Z) + (q.W * q.X));

        if (MathF.Abs(m21) > 0.99995f)
        {
            var m10 = 2f * ((q.X * q.Y) + (q.W * q.Z));
            var m00 = 1f - (2f * ((q.Y * q.Y) + (q.Z * q.Z)));

            return new Vector3(MathF.Sign(m21) * 90f, 0f, MathF.Atan2(m10, m00) * ToDegrees);
        }

        var m01 = 2f * ((q.X * q.Y) - (q.W * q.Z));
        var m11 = 1f - (2f * ((q.X * q.X) + (q.Z * q.Z)));
        var m20 = 2f * ((q.X * q.Z) - (q.W * q.Y));
        var m22 = 1f - (2f * ((q.X * q.X) + (q.Y * q.Y)));

        return new Vector3(
            MathF.Asin(m21) * ToDegrees,
            MathF.Atan2(-m20, m22) * ToDegrees,
            MathF.Atan2(-m01, m11) * ToDegrees);
    }

    internal static Quaternion Heading(float degrees) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitZ, degrees * ToRadians);

    internal static Vector3 Forward(Quaternion q) => Vector3.Transform(Vector3.UnitY, q);

    internal static Vector3 Right(Quaternion q) => Vector3.Transform(Vector3.UnitX, q);

    internal static (float Pitch, float Yaw) LookAngles(Vector3 direction)
    {
        if (direction.LengthSquared() < 0.0001f)
        {
            return (0f, 0f);
        }

        direction = Vector3.Normalize(direction);

        return (
            MathF.Asin(Math.Clamp(direction.Z, -1f, 1f)) * ToDegrees,
            MathF.Atan2(-direction.X, direction.Y) * ToDegrees);
    }

    internal static float Wrap(float degrees)
    {
        degrees %= 360f;

        return degrees switch
        {
            > 180f => degrees - 360f,
            <= -180f => degrees + 360f,
            _ => degrees,
        };
    }
}
