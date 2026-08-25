using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleEngineKeyBinding
{
    private const string Command = "vmenu:toggleengine";

    private const string DefaultKey = "RCONTROL";

    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        SharedAPI.Commands.RegisterCommand(Command, false, new Action(VehicleEngine.Toggle));

        KeyMapping.Register(Command, null, "vMenu: Toggle vehicle engine", DefaultKey, null);
    }
}
