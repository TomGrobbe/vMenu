using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.Script;

using vMenu.Enhanced.Permissions.Server;

namespace vMenu.Enhanced.Core.Server;

/// <summary>
/// Placeholder so the project produces a valid assembly. Replace with the
/// server core (main server, event handling) as the port lands.
/// </summary>
public class CoreServer : IScript
{
    public void Initialize()
    {
        ServerPermissions.Initialize();
        PermissionsSync.RegisterEventHandlers();

        API.Log.Info("Server started");
    }
}
