
using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Props;

internal static class CameraRay
{
    private const int MapAndObjects = 1 | 16;

    private const int ShapeTestOptions = 7;

    private const int ShapeTestReady = 2;

    internal static Vector3 Direction(Vector3 rotation)
    {
        const float ToRadians = MathF.PI / 180f;

        var pitch = rotation.X * ToRadians;
        var yaw = rotation.Z * ToRadians;
        var flat = MathF.Abs(MathF.Cos(pitch));

        return new Vector3(-MathF.Sin(yaw) * flat, MathF.Cos(yaw) * flat, MathF.Sin(pitch));
    }

    // A ray, because GetGroundZFor_3dCoord faults inside the game in this runtime.
    internal static Vector3 Hit(float distance, int ignore)
    {
        var origin = API.GetGameplayCamCoord();
        var end = origin + (Direction(API.GetGameplayCamRot(0)) * distance);

        var test = API.StartExpensiveSynchronousShapeTestLosProbe(
            origin.X, origin.Y, origin.Z,
            end.X, end.Y, end.Z,
            MapAndObjects,
            ignore,
            ShapeTestOptions);

        var status = NativeFixer.GetShapeTestResult(test, out var didHit, out var where, out _, out _);

        return status == ShapeTestReady && didHit != 0 ? where : end;
    }
}
