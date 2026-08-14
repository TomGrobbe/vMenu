using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared.Script;

using MenuAPI;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus;
using vMenu.Enhanced.Menus.Developer;
using vMenu.Enhanced.Menus.Misc;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Teleport;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Weapons;
using vMenu.Enhanced.Menus.Weapons.Saved;
using vMenu.Enhanced.Menus.World;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Core;

public sealed class Main : IScript
{
    public async void Initialize()
    {
        var resource = Native.GetCurrentResourceName();

        if (!ResourceIdentity.IsCorrectlyNamed(resource))
        {
            foreach (var line in ResourceIdentity.MismatchReport(resource, "client"))
            {
                API.Log.Error(line);
            }

            return;
        }

        TickRegistry.Initialize();

        GameEvents.Initialize();

        ClientJson.Verify();

        UserDefaults.Initialize();

        _ = MenuController.IsAnyMenuOpen();

        PermissionsSync.RegisterEventHandlers();

        ServerActions.RegisterEventHandlers();

        TeleportSync.RegisterEventHandlers();

        PedModelSync.RegisterEventHandlers();

        WeaponSync.RegisterEventHandlers();

        WalkingStyleSync.RegisterEventHandlers();

        NotificationEvents.RegisterEventHandlers();

        ClientConfig.Initialize();

        DebugCommands.Source(
            () => ClientConfig.Value(Debugging.Client),
            Debugging.Client.Name,
            message => API.Log.Info($"[vMenu] {message}"));

        MenuController.MenuToggleKeyDefault = ClientConfig.Value(KeyBindings.MenuToggleKey);

        HeaderStyle.Initialize();

        LanguageLoader.Load();

        ClientConfig.Changed += TickRegistry.Reevaluate;
        ClientPermissions.PermissionsChanged += TickRegistry.Reevaluate;

        VehicleCommands.Initialize();
        VehicleDumpCommands.Initialize();
        PedDumpCommands.Initialize();
        DeveloperOverlay.Initialize();
        NoClip.NoClip.Initialize();

        PvpMode.Initialize();

        PlayerGodMode.Initialize();
        PlayerSuperJump.Initialize();
        PlayerFastRun.Initialize();
        PlayerFastSwim.Initialize();
        PlayerUnlimitedStamina.Initialize();
        PlayerUnlimitedOxygen.Initialize();
        PlayerNoRagdoll.Initialize();
        PedIlluminatedClothing.Initialize();
        VehicleGodMode.Initialize();
        VehicleKeepClean.Initialize();

        WeaponUnlimitedAmmo.Initialize();
        WeaponNoReload.Initialize();
        ParachuteOptions.Initialize();
        WeaponLoadoutRespawn.Initialize();

        NoClip.NoClip.EntityReleased += _ =>
        {
            PlayerGodMode.Reapply();
            VehicleGodMode.Reapply();
        };

        TeleportKeyBinding.Initialize();
        VisorKeyBinding.Initialize();
        MinimapControls.Initialize();

        PlayerPushEvents.Initialize();

        DeathNotifications.Initialize();

        WorldSync.Initialize();

        while (!ClientPermissions.HasReceivedPermissions)
        {
            await API.Yield();
        }

        TeleportSync.Request();

        PedModelSync.Request();

        WeaponSync.Request();

        WalkingStyleSync.Request();

        UserPreferences.Restore();

        await MenuRegistry.BuildAsync(MainMenuComposition.Definitions);

        await WeaponLoadoutRespawn.RestoreOnJoinAsync();
    }
}
