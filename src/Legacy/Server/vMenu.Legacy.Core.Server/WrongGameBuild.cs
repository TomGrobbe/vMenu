using CitizenFX.Core.Native;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Core.Server;

internal static class WrongGameBuild
{
    public static bool Detected() =>
        string.Equals(API.GetConvar("gamename", string.Empty), "gta5enhanced", StringComparison.OrdinalIgnoreCase);

    public static void Report()
    {
        Log.Error("[vMenu] This is the FiveM Legacy build of vMenu Enhanced, but this server runs GTA V Enhanced, so vMenu did not start.");
        Log.Error("[vMenu] Install the Enhanced build instead. That is the vMenu-Enhanced zip without \"Legacy\" in its name, from https://github.com/TomGrobbe/vMenu/releases/latest");
    }
}
