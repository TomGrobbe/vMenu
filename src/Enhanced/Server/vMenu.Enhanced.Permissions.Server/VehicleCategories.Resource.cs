using CitizenFX.FiveM.Server;

namespace vMenu.Enhanced.Permissions.Server;

public static partial class VehicleCategories
{
    public static void LoadAndRegister() =>
        Load(Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile));
}
