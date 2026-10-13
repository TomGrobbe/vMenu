using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Permissions.Server;

public static partial class VehicleCategories
{
    public static void LoadAndRegister() =>
        Load(API.LoadResourceFile(API.GetCurrentResourceName(), ConfigFile));
}
