using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

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

        // Wrapped here and not inside Toggle, because the menu already calls that from the main thread.
        Commands.Register(Command, false, new Action(() => Script.RunOnMainThread(VehicleEngine.Toggle)));

        KeyMapping.Register(Command, null, "vMenu: Toggle vehicle engine", DefaultKey, null);
    }
}
