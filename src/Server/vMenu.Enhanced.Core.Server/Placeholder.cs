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
    }
}
