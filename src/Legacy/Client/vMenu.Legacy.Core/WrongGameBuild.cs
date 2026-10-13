using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace vMenu.Enhanced.Core;

internal static class WrongGameBuild
{
    private const ulong IsGameEnhancedVersion = 0x4DD998F6;

    public static bool Detected()
    {
        try
        {
            return Function.Call<bool>((Hash)IsGameEnhancedVersion);
        }
        catch
        {
            return false;
        }
    }

    public static void Report()
    {
        Debug.WriteLine("^1[vMenu] This is the FiveM Legacy build of vMenu Enhanced, but this game is GTA V Enhanced, so vMenu did not start.^7");
        Debug.WriteLine("^1[vMenu] The server owner has to install the Enhanced build instead. That is the vMenu-Enhanced zip without \"Legacy\" in its name, from https://github.com/TomGrobbe/vMenu/releases/latest^7");
    }
}
