using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Ticks;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Misc;

public static class VisionModes
{
    private const int GuardIntervalMs = 1000;

    private static TickHandle? _night;

    private static TickHandle? _thermal;

    private static bool _wantNight;

    private static bool _wantThermal;

    public static bool NightVision =>
        _wantNight && ClientPermissions.IsAllowed(DisplaySettingsPermissions.NightVision);

    public static bool ThermalVision =>
        _wantThermal && ClientPermissions.IsAllowed(DisplaySettingsPermissions.ThermalVision);

    public static void Initialize()
    {
        _night = TickRegistry.Register(
            "Display.NightVision",
            HoldNight,
            TickRate.Every(GuardIntervalMs),
            () => NightVision,
            onStarted: () => API.SetNightvision(true),
            onStopped: () => API.SetNightvision(false),
            autoStart: false);

        _thermal = TickRegistry.Register(
            "Display.ThermalVision",
            HoldThermal,
            TickRate.Every(GuardIntervalMs),
            () => ThermalVision,
            onStarted: () => API.SetSeethrough(true),
            onStopped: () => API.SetSeethrough(false),
            autoStart: false);

        ClientPermissions.PermissionsChanged += Reevaluate;
    }

    public static void SetNightVision(bool on)
    {
        if (on && !ClientPermissions.IsAllowed(DisplaySettingsPermissions.NightVision))
        {
            return;
        }

        _wantNight = on;

        _night?.Reevaluate();
    }

    public static void SetThermalVision(bool on)
    {
        if (on && !ClientPermissions.IsAllowed(DisplaySettingsPermissions.ThermalVision))
        {
            return;
        }

        _wantThermal = on;

        _thermal?.Reevaluate();
    }

    private static void Reevaluate()
    {
        _night?.Reevaluate();
        _thermal?.Reevaluate();
    }

    private static void HoldNight()
    {
        if (!API.GetUsingnightvision())
        {
            API.SetNightvision(true);
        }
    }

    private static void HoldThermal()
    {
        if (!API.GetUsingseethrough())
        {
            API.SetSeethrough(true);
        }
    }
}
