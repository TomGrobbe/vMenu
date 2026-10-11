using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Bully;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Misc;

public static class NoClipSmoke
{
    private const string Asset = "scr_powerplay";

    private const string Vanish = "scr_powerplay_beast_vanish";

    private const string Appear = "scr_powerplay_beast_appear";

    private const float VanishScale = 1f;

    private const float AppearScale = 0.75f;

    private const float HeightOffset = -0.3f;

    private const int ScanMs = 200;

    private const int HoldMs = 5000;

    private static Dictionary<int, bool> _known = [];

    private static Dictionary<int, bool> _seen = [];

    private static TickHandle? _tick;

    private static Task<bool>? _loading;

    private static int _releaseAt;

    private static bool Enabled => ClientConfig.Value(Gameplay.NoClipSmoke);

    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "Misc.NoClipSmoke",
            Scan,
            TickRate.Every(ScanMs),
            () => Enabled,
            onStopped: Stop);

        ClientConfig.AddEventListenerFor([Gameplay.NoClipSmoke], () => _tick?.Reevaluate());
    }

    private static void Scan()
    {
        PlayerRoster.Refresh();

        _seen.Clear();

        foreach (var player in PlayerRoster.All)
        {
            var noClip = StateBags.GetPlayer<bool>(player.ServerId, PlayerStateKeys.NoClip);

            if (_known.TryGetValue(player.ServerId, out var was) && was != noClip)
            {
                _ = PuffAsync(player.Position, noClip);
            }

            _seen[player.ServerId] = noClip;
        }

        (_known, _seen) = (_seen, _known);

        if (_loading is { IsCompleted: true } && Native.GetGameTimer() >= _releaseAt)
        {
            Unload();
        }
    }

    // The particle set is about 25 MiB, so it is only held for a while after a puff instead of all session.
    private static async Task PuffAsync(Vector3 position, bool entering)
    {
        _loading ??= Streaming.PtfxAsync(Asset);

        if (!await _loading)
        {
            _loading = null;

            return;
        }

        _releaseAt = Native.GetGameTimer() + HoldMs;

        Native.UseParticleFxAssetNextCall(Asset);
        Native.StartParticleFxNonLoopedAtCoord(
            entering ? Vanish : Appear,
            position.X,
            position.Y,
            position.Z + HeightOffset,
            0f,
            0f,
            0f,
            entering ? VanishScale : AppearScale,
            false,
            false,
            false);
    }

    private static void Stop()
    {
        _known.Clear();

        Unload();
    }

    private static void Unload()
    {
        if (_loading is not { } loading)
        {
            return;
        }

        _loading = null;

        _ = ReleaseAsync(loading);
    }

    private static async Task ReleaseAsync(Task<bool> loading)
    {
        if (await loading)
        {
            Streaming.ReleasePtfx(Asset);
        }
    }
}
