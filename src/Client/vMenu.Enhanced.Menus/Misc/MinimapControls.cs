using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using NoClipState = vMenu.Enhanced.NoClip.NoClip;

namespace vMenu.Enhanced.Menus.Misc;

/// <summary>
/// Trying to mimic the minimap controls from GTAV SP and MP Freemode 
/// as much as possible using community research as a base, then tweaked
/// until I was happy.
/// </summary>
public static class MinimapControls
{
    public const int Off = 0;

    public const int Expand = 1;

    public const int Zoom = 2;

    public const int MinZoom = 1;

    public const int MaxZoom = 10;

    private const int HoldMs = 10_000;

    private const int GlideMs = 350;

    private const float NoZoom = 1f;

    /// <summary>The two ends of the zoom slider, as multiples of that reach.</summary>
    // The game's own zoom out is 2.1.
    private const float NearestZoom = 1.5f;

    private const float FurthestZoom = 6f;

    /// <summary>
    /// The ranges the game picks between, from <c>CMiniMap_CameraTunables</c>.
    /// </summary>
    private const float RangeToMetres = 5000f;

    private const float FootRange = 83f;

    private const float FootWantedRange = 60f;

    private const float VehicleIdleRange = 96f;

    private const float VehicleIdleWantedRange = 66f;

    /// <summary>How far a vehicle's range falls, and the floor it stops at.</summary>
    private const float SpeedRangeScalar = 0.8f;

    private const float VehicleMovingRange = 48f;

    private const float InteriorRange = 500f;

    /// <summary>The extra the game gives a plane or helicopter on top of everything else.</summary>
    private const float AircraftWidening = 1.8f;

    private static TickHandle? _zoom;

    private static TickHandle? _state;

    private static bool _registered;

    /// <summary>A key press is live and has not run out yet.</summary>
    private static bool _temporary;

    private static int _expiresAt;

    /// <summary>Whether the bigmap on screen is one this feature turned on.</summary>
    private static bool _expanded;

    private static float _glideFrom = NoZoom;

    private static float _glideTo = NoZoom;

    private static int _glideStartedAt;

    /// <summary>What the key does: <see cref="Off"/>, <see cref="Expand"/> or <see cref="Zoom"/>.</summary>
    public static int Action
    {
        get => UserDefaults.MiscMinimapAction.Value;

        set
        {
            if (value == Action)
            {
                return;
            }

            UserDefaults.MiscMinimapAction.Value = value;

            Apply();
        }
    }

    /// <summary>A slider position, not a zoom value. <see cref="ZoomValue"/> turns it into one.</summary>
    public static int ZoomAmount
    {
        get => Math.Clamp(UserDefaults.MiscMinimapZoom.Value, MinZoom, MaxZoom);

        set
        {
            UserDefaults.MiscMinimapZoom.Value = Math.Clamp(value, MinZoom, MaxZoom);

            // Retargeted rather than reapplied, so moving the slider mid press glides to the new
            // amount instead of cancelling the ten seconds the player is in the middle of.
            if (ZoomWanted)
            {
                GlideTo(ZoomValue(ZoomAmount));
            }
        }
    }

    /// <summary>Whether the chosen action is held on permanently, leaving the key nothing to toggle.</summary>
    public static bool AlwaysOn
    {
        get => UserDefaults.MiscMinimapAlwaysOn.Value;

        set
        {
            UserDefaults.MiscMinimapAlwaysOn.Value = value;

            Apply();
        }
    }

    /// <summary>Call after <see cref="UserDefaults.Initialize"/>, whose store the ticks read.</summary>
    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        MinimapKeyBinding.Register(OnPressed);

        _zoom = TickRegistry.Register(
            "Minimap.Zoom",
            ZoomFrame,
            TickRate.PerFrame,
            condition: () => ZoomWanted || IsGliding);

        _state = TickRegistry.Register(
            "Minimap.State",
            StateFrame,
            TickRate.Every(250),
            condition: () => _temporary || ExpandWanted);
    }

    /// <summary>Puts the stored preferences into effect, forgetting any key press in flight.</summary>
    public static void Apply()
    {
        _temporary = false;

        ApplyWanted();
    }

    private static bool ExpandWanted => Action == Expand && (AlwaysOn || _temporary);

    private static bool ZoomWanted => Action == Zoom && (AlwaysOn || _temporary);

    private static bool IsGliding => Native.GetGameTimer() - _glideStartedAt < GlideMs;

    private static bool CanToggle() =>
        !MenuController.IsAnyMenuOpen()
        && !Native.IsPauseMenuActive()
        && !NoClipState.IsActive
        && !Native.IsRadarHidden()
        && Native.IsRadarPreferenceSwitchedOn()
        && !Native.IsPlayerSwitchInProgress()
        && Native.IsScreenFadedIn();

    // Main thread needed for some natives.
    private static void OnPressed() => SharedAPI.RunOnMainThread(Toggle);

    private static void Toggle()
    {
        // Silent in every case: this is the player's own choice and their own game state, not
        // something a server refused them.
        if (Action == Off || AlwaysOn || !CanToggle())
        {
            return;
        }

        if (_temporary)
        {
            _temporary = false;
        }
        else
        {
            _temporary = true;
            _expiresAt = Native.GetGameTimer() + HoldMs;
        }

        ApplyWanted();
    }

    /// <summary>The one place that pushes what is wanted into the game.</summary>
    private static void ApplyWanted()
    {
        if (ExpandWanted)
        {
            Native.SetBigmapActive(true, false);

            _expanded = true;
        }
        else if (_expanded)
        {
            Native.SetBigmapActive(false, false);

            _expanded = false;
        }

        GlideTo(ZoomWanted ? ZoomValue(ZoomAmount) : NoZoom);

        _zoom?.Reevaluate();
        _state?.Reevaluate();
    }

    private static void ZoomFrame()
    {
        Native.SetRadarZoomToDistance(DefaultReach() * CurrentZoom());

        // Asked for here rather than by whoever ended the zoom, because this is the frame the glide
        // home lands on and the earliest one it is safe to stop feeding the native.
        if (!ZoomWanted && !IsGliding)
        {
            _zoom?.Reevaluate();
        }
    }

    private static void StateFrame()
    {
        if (_temporary && Native.GetGameTimer() >= _expiresAt)
        {
            _temporary = false;

            ApplyWanted();

            return;
        }

        // The game drops the bigmap on its own after a respawn or a switch, so what was asked for is
        // asked for again rather than assumed to have stuck.
        if (ExpandWanted && !Native.IsRadarHidden() && !Native.IsBigmapActive())
        {
            Native.SetBigmapActive(true, false);

            _expanded = true;
        }
    }

    private static void GlideTo(float zoom)
    {
        // Nothing to glide to. Worth the check because a pointless one would still run the tick for
        // its length, and every frame of it overrides the radar and clears whatever zoom another
        // resource had set.
        if (!IsGliding && Math.Abs(_glideTo - zoom) < 0.001f)
        {
            return;
        }

        // From where the radar actually is, not from where the last glide was headed, so changing
        // your mind halfway turns around instead of jumping back to the start.
        _glideFrom = CurrentZoom();
        _glideTo = zoom;
        _glideStartedAt = Native.GetGameTimer();
    }

    // Smoothstep: starts slow, runs, settles, and needs no state beyond the two ends and the clock.
    private static float CurrentZoom()
    {
        var elapsed = Native.GetGameTimer() - _glideStartedAt;

        if (elapsed >= GlideMs)
        {
            return _glideTo;
        }

        var t = elapsed / (float)GlideMs;

        return _glideFrom + ((_glideTo - _glideFrom) * (t * t * (3f - (2f * t))));
    }

    /// <summary>How far the radar would reach this frame if nothing were touching it, in metres (estimated).</summary>
    private static float DefaultReach()
    {
        var ped = Native.PlayerPedId();
        var wanted = Native.GetPlayerWantedLevel(Native.PlayerId()) > 0;

        if (Native.IsPedInAnyVehicle(ped, false))
        {
            var idle = wanted ? VehicleIdleWantedRange : VehicleIdleRange;
            var speed = Native.GetEntitySpeed(Native.GetVehiclePedIsIn(ped, false));

            var reach = RangeToMetres / Math.Max(idle - (speed * SpeedRangeScalar), VehicleMovingRange);

            return Native.IsPedInAnyPlane(ped) || Native.IsPedInAnyHeli(ped)
                ? reach * AircraftWidening
                : reach;
        }

        // Checked after the vehicle, the game only using the interior range for a player on foot.
        if (Native.IsMinimapInInterior())
        {
            return RangeToMetres / InteriorRange;
        }

        return RangeToMetres / (wanted ? FootWantedRange : FootRange);
    }

    private static float ZoomValue(int step) =>
        NearestZoom + ((FurthestZoom - NearestZoom) * ((step - MinZoom) / (float)(MaxZoom - MinZoom)));
}
