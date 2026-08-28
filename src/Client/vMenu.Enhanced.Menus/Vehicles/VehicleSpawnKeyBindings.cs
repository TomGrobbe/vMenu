using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Menus.Vehicles;

internal static class VehicleSpawnKeyBindings
{
    private const string SpawnInsideCommand = "vmenu:vehiclespawner:spawninside";

    private const string ReplacePreviousCommand = "vmenu:vehiclespawner:replaceprevious";

    private static bool _registered;

    internal static int SpawnInsideControl { get; } = BindingControl(SpawnInsideCommand);

    internal static int ReplacePreviousControl { get; } = BindingControl(ReplacePreviousCommand);

    internal static void Register(Action onSpawnInside, Action onReplacePrevious)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        SharedAPI.Commands.RegisterCommand(SpawnInsideCommand, false, onSpawnInside);
        SharedAPI.Commands.RegisterCommand(ReplacePreviousCommand, false, onReplacePrevious);

        Native.RegisterKeyMapping(
            SpawnInsideCommand,
            "vMenu: Spawn vehicles inside or outside",
            "keyboard",
            "DELETE");

        Native.RegisterKeyMapping(
            ReplacePreviousCommand,
            "vMenu: Keep or replace the previous vehicle",
            "keyboard",
            "END");
    }

    private static int BindingControl(string command) => API.HashSigned(command) | int.MinValue;
}
