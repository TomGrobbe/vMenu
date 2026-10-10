using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Bully;
using vMenu.Enhanced.Menus.Misc;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerIntoxication
{
    public const int StrengthLevels = 10;

    private const int ControlGroup = 0;

    private const int Sprint = 21;

    private const float BlendSeconds = 1f;

    private static readonly DrunkSteering Steering = new();

    private static TickHandle? _frameTick;

    private static TickHandle? _watchTick;

    private static bool _screen;

    private static bool _walk;

    private static bool _drunk;

    private static int _timecycle = -1;

    public static bool Enabled => UserDefaults.PlayerIntoxication.Value && IsAllowed;

    public static bool Drugged => UserDefaults.PlayerIntoxicationDrugged.Value;

    public static bool ScreenEffects => UserDefaults.PlayerIntoxicationScreen.Value;

    public static int Strength => Math.Clamp(UserDefaults.PlayerIntoxicationStrength.Value, 1, StrengthLevels);

    public static bool WalkingStyle => UserDefaults.PlayerIntoxicationWalk.Value;

    public static bool PreventSprint => UserDefaults.PlayerIntoxicationNoSprint.Value;

    public static bool Driving => UserDefaults.PlayerIntoxicationDriving.Value;

    public static bool HoldsWalk => Enabled && WalkingStyle;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.Intoxication);

    // Call once at startup, before permissions have arrived.
    public static void Initialize()
    {
        _frameTick = TickRegistry.Register(
            "Player.Intoxication",
            OnFrame,
            TickRate.PerFrame,
            () => Enabled && (PreventSprint || Driving),
            onStopped: Steering.Release);

        _watchTick = TickRegistry.Register("Player.IntoxicationWatch", SyncAsync, TickRate.Every(1000), () => Enabled);

        ClientPermissions.PermissionsChanged += OnPermissionsChanged;
        LocalPlayerTicks.PlayerPedIdChanged += _ => OnNewPed();
        LocalPlayerTicks.PlayerPedRevived += _ => OnNewPed();
        ResourceShutdown.Stopping += OnShutdown;
    }

    public static async Task SetEnabledAsync(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.PlayerIntoxication.Value = enabled;

        // Not awaited, its sober up fade would hold the checkbox for seconds.
        if (enabled)
        {
            _ = ScreenPranks.EndIntoxicationAsync();
        }

        await RefreshAsync();
    }

    public static Task SetDruggedAsync(bool drugged)
    {
        UserDefaults.PlayerIntoxicationDrugged.Value = drugged;

        return RestartScreenAsync();
    }

    public static Task SetScreenEffectsAsync(bool enabled)
    {
        UserDefaults.PlayerIntoxicationScreen.Value = enabled;

        return RefreshAsync();
    }

    public static Task SetStrengthAsync(int strength)
    {
        UserDefaults.PlayerIntoxicationStrength.Value = Math.Clamp(strength, 1, StrengthLevels);

        return RestartScreenAsync();
    }

    public static Task SetWalkingStyleAsync(bool enabled)
    {
        UserDefaults.PlayerIntoxicationWalk.Value = enabled;

        return RefreshAsync();
    }

    public static Task SetPreventSprintAsync(bool enabled)
    {
        UserDefaults.PlayerIntoxicationNoSprint.Value = enabled;

        return RefreshAsync();
    }

    public static Task SetDrivingAsync(bool enabled)
    {
        UserDefaults.PlayerIntoxicationDriving.Value = enabled;

        return RefreshAsync();
    }

    private static Task RestartScreenAsync()
    {
        if (_screen)
        {
            ClearScreen();
        }

        return RefreshAsync();
    }

    private static async Task RefreshAsync()
    {
        await SyncAsync();

        _frameTick?.Reevaluate();
        _watchTick?.Reevaluate();
    }

    private static void OnPermissionsChanged() => _ = SyncAsync();

    // A new or revived ped comes without the drunk walk and the drunk voice.
    private static void OnNewPed()
    {
        _walk = false;
        _drunk = false;

        _ = SyncAsync();
    }

    private static void OnShutdown()
    {
        var ped = Native.PlayerPedId();

        if (_screen)
        {
            ClearScreen();
        }

        if (_walk)
        {
            Native.ResetPedMovementClipset(ped, 0f);
        }

        if (_drunk)
        {
            Native.SetPedIsDrunk(ped, false);
        }

        Steering.Release();
    }

    private static void OnFrame()
    {
        if (PreventSprint)
        {
            Native.DisableControlAction(ControlGroup, Sprint, true);
        }

        if (Driving)
        {
            Steering.Update(Drugged);

            return;
        }

        Steering.Release();
    }

    private static async Task SyncAsync()
    {
        var on = Enabled;
        var ped = Native.PlayerPedId();

        SyncScreen(on && ScreenEffects);

        if (on || _drunk)
        {
            Native.SetPedIsDrunk(ped, on);

            _drunk = on;
        }

        await SyncWalkAsync(on && WalkingStyle);
    }

    private static void SyncScreen(bool wanted)
    {
        if (!wanted)
        {
            if (_screen)
            {
                ClearScreen();
            }

            return;
        }

        // A bully screen colour has the timecycle for now, the watch takes it back once that ends.
        if (ScreenPranks.IsColouring)
        {
            return;
        }

        var drugged = Drugged;
        var strength = Strength / (float)StrengthLevels;

        if (!_screen || Native.GetTimecycleModifierIndex() != _timecycle)
        {
            Native.SetTimecycleModifier(Intoxication.Timecycle(drugged));
            Native.SetTimecycleModifierStrength(strength);

            _timecycle = Native.GetTimecycleModifierIndex();
        }

        if (!Native.IsGameplayCamShaking())
        {
            Native.ShakeGameplayCam(Intoxication.Shake, Intoxication.ShakeAmplitude(drugged) * strength);
        }

        if (drugged && !Native.AnimpostfxIsRunning(Intoxication.DrugsIn))
        {
            Native.AnimpostfxPlay(Intoxication.DrugsIn, 0, true);
        }

        _screen = true;
    }

    private static void ClearScreen()
    {
        _screen = false;
        _timecycle = -1;

        Native.AnimpostfxStop(Intoxication.DrugsIn);
        Native.StopGameplayCamShaking(true);

        if (ScreenPranks.IsColouring)
        {
            return;
        }

        Native.ClearTimecycleModifier();

        TimecycleState.Reapply();
    }

    private static async Task SyncWalkAsync(bool wanted)
    {
        if (wanted == _walk)
        {
            return;
        }

        if (!wanted)
        {
            _walk = false;

            Native.ResetPedMovementClipset(Native.PlayerPedId(), BlendSeconds);

            await PedWalkingStyle.ReapplyAsync();

            return;
        }

        // Checked again after the load, because the player may have turned it off while it streamed in.
        if (!await Streaming.ClipSetAsync(Intoxication.DrunkWalk) || !HoldsWalk)
        {
            return;
        }

        Native.SetPedMovementClipset(Native.PlayerPedId(), Intoxication.DrunkWalk, BlendSeconds);

        _walk = true;
    }
}
