using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.Script;

namespace vMenu.Enhanced.Core.Server;

/// <summary>
/// Placeholder so the project produces a valid assembly. Replace with the
/// server core (main server, event handling) as the port lands.
/// </summary>
public class Placeholder : IScript
{
    public void Initialize()
    {
        API.Log.Info("Resource Loaded");
        //Native.SaveResourceFile()
        Native.SetConvar("add_filesystem_permission", "vMenu.Enhanced write vMenu.Enhanced");

        BrokenNatives.Server.NativeFixer.SaveResourceFile("vMenu.Enhanced", "test.txt", "Hello world");
    }
}
