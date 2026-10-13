using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Ticks;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

namespace vMenu.Enhanced.Menus.Misc;

public static class HudVisibility
{
    private const int GuardIntervalMs = 500;

    private static TickHandle? _hud;

    private static TickHandle? _radar;

    private static bool _wantHudHidden;

    private static bool _wantRadarHidden;

    public static bool HudHidden =>
        _wantHudHidden && ClientPermissions.IsAllowed(DisplaySettingsPermissions.HideHud);

    public static bool RadarHidden =>
        _wantRadarHidden && ClientPermissions.IsAllowed(DisplaySettingsPermissions.HideRadar);

    public static void Initialize()
    {
        _hud = TickRegistry.Register(
            "Display.HideHud",
            HoldHud,
            TickRate.Every(GuardIntervalMs),
            () => HudHidden,
            onStarted: () => Native.DisplayHud(false),
            onStopped: () => Native.DisplayHud(true),
            autoStart: false);

        _radar = TickRegistry.Register(
            "Display.HideRadar",
            HoldRadar,
            TickRate.Every(GuardIntervalMs),
            () => RadarHidden,
            onStarted: HideRadarNow,
            onStopped: ShowRadarAgain,
            autoStart: false);

        ClientPermissions.PermissionsChanged += Reevaluate;
    }

    public static void SetHudHidden(bool hidden)
    {
        if (hidden && !ClientPermissions.IsAllowed(DisplaySettingsPermissions.HideHud))
        {
            return;
        }

        _wantHudHidden = hidden;

        _hud?.Reevaluate();
    }

    public static void SetRadarHidden(bool hidden)
    {
        if (hidden && !ClientPermissions.IsAllowed(DisplaySettingsPermissions.HideRadar))
        {
            return;
        }

        _wantRadarHidden = hidden;

        _radar?.Reevaluate();
    }

    private static void Reevaluate()
    {
        _hud?.Reevaluate();
        _radar?.Reevaluate();
    }

    // Only written on drift, so a resource hiding the hud for its own reasons is not fought.
    private static void HoldHud()
    {
        if (!Native.IsHudHidden())
        {
            Native.DisplayHud(false);
        }
    }

    private static void HoldRadar()
    {
        if (!Native.IsRadarHidden())
        {
            Native.DisplayRadar(false);
        }
    }

    private static void HideRadarNow()
    {
        Native.DisplayRadar(false);

        LocationDisplay.RefreshAnchor();
    }

    private static void ShowRadarAgain()
    {
        // Their pause menu setting, not a flat true.
        Native.DisplayRadar(Native.IsRadarPreferenceSwitchedOn());

        MinimapControls.Apply();

        LocationDisplay.RefreshAnchor();
    }
}
