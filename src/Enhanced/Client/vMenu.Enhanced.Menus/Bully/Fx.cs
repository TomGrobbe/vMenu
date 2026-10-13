using System.Numerics;

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Fx
{
    public static void Burst(string asset, string effect, Vector3 at, float scale = 1f)
    {
        Native.UseParticleFxAssetNextCall(asset);
        Native.StartParticleFxNonLoopedAtCoord(effect, at.X, at.Y, at.Z, 0f, 0f, 0f, scale, false, false, false);
    }

    public static int Loop(string asset, string effect, Vector3 at, float yaw, float scale = 1f)
    {
        Native.UseParticleFxAssetNextCall(asset);

        return Native.StartParticleFxLoopedAtCoord(effect, at.X, at.Y, at.Z, 0f, 0f, yaw, scale, false, false, false, true);
    }

    public static void Sound(string name, string soundSet, Vector3 at, bool networked = false, int range = 0) =>
        Native.PlaySoundFromCoord(-1, name, at.X, at.Y, at.Z, soundSet, networked, range, false);

    public static float HeadingTowards(Vector3 from, Vector3 to) =>
        MathF.Atan2(to.X - from.X, to.Y - from.Y) * -180f / MathF.PI;
}

// A duration that a repeat of the same effect extends instead of starting a second copy.
internal sealed class Window
{
    private int _until;

    public bool IsOpen => Native.GetGameTimer() < _until;

    public bool Extend(int durationMs)
    {
        var wasOpen = IsOpen;

        _until = Native.GetGameTimer() + durationMs;

        return !wasOpen;
    }

    public async Task WaitAsync()
    {
        while (IsOpen)
        {
            await API.Delay(250);
        }
    }

    public bool Close()
    {
        var wasSet = _until != 0;

        _until = 0;

        return wasSet;
    }
}
