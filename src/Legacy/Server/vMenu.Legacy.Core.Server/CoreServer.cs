using System.Reflection;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Actions.Server;
using vMenu.Enhanced.Actions.Server.Events;
using vMenu.Enhanced.Actions.Server.Handlers;
using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Http.Server;
using vMenu.Enhanced.Http.Server.Bridge;
using vMenu.Enhanced.Integration.Server;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Plugins.Server;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Ticks.Server;
using vMenu.Enhanced.Updates.Server;
using vMenu.Enhanced.Webhooks.Server;
using vMenu.Enhanced.World.Server;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Core.Server;

public class CoreServer : BaseScript
{
    public CoreServer()
    {
        Log.Writer = WriteLog;

        if (WrongGameBuild.Detected())
        {
            WrongGameBuild.Report();

            return;
        }

        Script.Attach(EventHandlers, Exports, GlobalState, Players, tick => Tick += tick, tick => Tick -= tick);

        Tick += FirstTick;
    }

    private Task FirstTick()
    {
        Tick -= FirstTick;

        Initialize();

        return Task.CompletedTask;
    }

    public void Initialize()
    {
        var core = typeof(CoreServer).Assembly;
        var version = core.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? core.GetName().Version?.ToString();

        Log.Info($"[Core] Loaded {core.GetName().Name} v{version}.");

        var resource = API.GetCurrentResourceName();

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

        ServerPermissions.SourceStaffOnly(() => ServerConfig.Value(Admin.StaffOnly));

        NetBridge.Initialize();

        WebhookLog.Initialize();
        SecurityLog.Initialize();

        DebugCommands.Source(
            () => ServerConfig.Value(Debugging.Server),
            Debugging.Server.Name,
            message => Log.Info($"[vMenu] {message}"));

        ServerTickRegistry.Initialize();

        ServerWeatherCycle.Initialize();
        ServerClock.Initialize();
        ServerState.Initialize();
        HttpRouter.Initialize();
        WorldEndpoint.Initialize();
        IntegrationSocket.Initialize();
        IntegrationRoleSync.Initialize();
        IntegrationConnectCheck.Initialize();
        IntegrationPlayersPush.Initialize();
        ConnectionGate.Initialize();
        RoutingBuckets.Initialize();
        BullyPrivateWorlds.ManagedWorlds = RoutingBuckets.WorldIds;
        RoutingBucketCommands.Initialize();
        UpdateChecker.Initialize();

        ConfigurationExampleFile.Write();
        PermissionsExampleFile.Write();

        WalkingStyles.Load();
        WeaponComponentCatalog.Load();

        PermissionsSync.RegisterEventHandlers();
        ServerConfig.AddEventListenerFor([Admin.StaffOnly], () => PermissionsSync.RefreshAll());
        PedCategories.RegisterEventHandlers();
        WeaponCatalog.RegisterEventHandlers();
        WalkingStyles.RegisterEventHandlers();

        VehicleActions.Register();
        VehicleSpawnActions.Register();
        VehicleExtraActions.Register();
        AircraftWeaponActions.Register();
        PropSpawnActions.Register();
        PersonalVehicleActions.Register();
        TeleportActions.Register();
        LocationBlipActions.Register();
        ClothingPresetActions.Register();
        WorldActions.Register();
        OnlinePlayerActions.Register();
        BullyActions.Register();
        CutsceneActions.Register();
        StaffAlertActions.Register();
        PrivateMessageCopies.Register();
        AdminActions.Register();
        AnnouncementSchedule.Register();
        RemoteVehicleControl.RegisterEventHandlers();
        RemotePlayerStatus.RegisterEventHandlers();
        ActionRegistry.RegisterEventHandlers();
        MenuAuditHandler.Register();

        PedDeathBroadcast.Register();
        PlayerNoClipState.Register();
        PlayerPresenceBroadcast.Register();
        PersonalVehicleBroadcast.Register();
        JoinLeaveBroadcast.Register();

        PluginRegistry.RegisterEventHandlers();
        PluginAuditHandler.Register();
        PluginRegistry.AnnounceReady();

        WebhookLog.Event("vMenu Enhanced started.");

        Log.Debug("vMenu Server side started");
    }

    private static void WriteLog(LogLevel level, string message) => Debug.WriteLine(level switch
    {
        LogLevel.Debug => $"^5[DEBUG]^7 {message}",
        LogLevel.Warning => $"^3[WARN] {message}^7",
        LogLevel.Error => $"^1[ERROR] {message}^7",
        _ => message,
    });
}
