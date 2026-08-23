using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.Script;

using vMenu.Enhanced.Actions.Server;
using vMenu.Enhanced.Actions.Server.Events;
using vMenu.Enhanced.Actions.Server.Handlers;
using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Plugins.Server;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Ticks.Server;
using vMenu.Enhanced.Updates.Server;

namespace vMenu.Enhanced.Core.Server;

public class CoreServer : IScript
{
    public void Initialize()
    {
        var resource = Native.GetCurrentResourceName();

        if (!ResourceIdentity.IsCorrectlyNamed(resource))
        {
            foreach (var line in ResourceIdentity.MismatchReport(resource, "server"))
            {
                Log.Error(line);
            }

            return;
        }

        ServerJson.Verify();

        ServerPermissions.Initialize();
        ServerConfig.Initialize();

        DebugCommands.Source(
            () => ServerConfig.Value(Debugging.Server),
            Debugging.Server.Name,
            message => Log.Info($"[vMenu] {message}"));

        ServerTickRegistry.Initialize();

        ServerClock.Initialize();
        ServerState.Initialize();
        UpdateChecker.Initialize();

        ConfigurationExampleFile.Write();
        PermissionsExampleFile.Write();

        WalkingStyles.Load();
        WeaponComponentCatalog.Load();

        PermissionsSync.RegisterEventHandlers();
        PedCategories.RegisterEventHandlers();
        WeaponCatalog.RegisterEventHandlers();
        WalkingStyles.RegisterEventHandlers();

        VehicleActions.Register();
        VehicleSpawnActions.Register();
        VehicleExtraActions.Register();
        PropSpawnActions.Register();
        PersonalVehicleActions.Register();
        TeleportActions.Register();
        LocationBlipActions.Register();
        ClothingPresetActions.Register();
        WorldActions.Register();
        OnlinePlayerActions.Register();
        StaffAlertActions.Register();
        MiscActions.Register();
        RemoteVehicleControl.RegisterEventHandlers();
        ActionRegistry.RegisterEventHandlers();

        PedDeathBroadcast.Register();
        PlayerNoClipState.Register();
        PlayerPresenceBroadcast.Register();
        PersonalVehicleBroadcast.Register();
        JoinLeaveBroadcast.Register();

        PluginRegistry.RegisterEventHandlers();
        PluginRegistry.AnnounceReady();

        Log.Debug("vMenu Server side started");
    }
}
