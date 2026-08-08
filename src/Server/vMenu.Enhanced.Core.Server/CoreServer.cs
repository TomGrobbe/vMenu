using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.Script;

using vMenu.Enhanced.Actions.Server;
using vMenu.Enhanced.Actions.Server.Handlers;
using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Ticks.Server;

namespace vMenu.Enhanced.Core.Server;

public class CoreServer : IScript
{
    public void Initialize()
    {
        ServerPermissions.Initialize();
        ServerConfig.Initialize();

        DebugCommands.Source(
            () => ServerConfig.Value(Debugging.Server),
            Debugging.Server.Name,
            message => API.Log.Info($"[vMenu] {message}"));

        ServerTickRegistry.Initialize();

        ServerConfig.Changed += ServerTickRegistry.Reevaluate;

        ServerClock.Initialize();
        ServerState.Initialize();

        // After the model whitelist has been loaded, so the permissions it registers at runtime are
        // in the tree the example file describes.
        ConfigurationExampleFile.Write();
        PermissionsExampleFile.Write();

        PermissionsSync.RegisterEventHandlers();

        VehicleActions.Register();
        TeleportActions.Register();
        WorldActions.Register();
        OnlinePlayerActions.Register();
        ActionRegistry.RegisterEventHandlers();

        API.Log.Info("Server started");
    }
}
