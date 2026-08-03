using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.Script;

using vMenu.Enhanced.Actions.Server;
using vMenu.Enhanced.Actions.Server.Handlers;
using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Permissions.Server;

namespace vMenu.Enhanced.Core.Server;

public class CoreServer : IScript
{
    public void Initialize()
    {
        ServerPermissions.Initialize();
        ServerConfig.Initialize();

        // After the model whitelist has been loaded, so the permissions it registers at runtime are
        // in the tree the example file describes.
        ConfigurationExampleFile.Write();
        PermissionsExampleFile.Write();

        PermissionsSync.RegisterEventHandlers();

        VehicleActions.Register();
        ActionRegistry.RegisterEventHandlers();

        API.Log.Info("Server started");
    }
}
