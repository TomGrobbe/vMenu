using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.World;

namespace vMenu.Enhanced.Menus.World;

// Both natives are sticky global state, so this is applied on the change and never re-asserted.
public static class WorldBlackout
{
    private static BlackoutMode _applied = BlackoutMode.Off;

    public static void Initialize() => WorldState.Changed += Apply;

    public static string Describe() => $"applied: {BlackoutModes.NameOf(_applied)}";

    private static void Apply()
    {
        var mode = WorldState.Blackout;

        if (mode == _applied)
        {
            return;
        }

        _applied = mode;

        Native.SetArtificialLightsState(mode != BlackoutMode.Off);
        Native.SetArtificialLightsStateAffectsVehicles(mode == BlackoutMode.CityAndVehicles);
    }
}
