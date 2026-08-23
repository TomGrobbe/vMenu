using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared.Script;

using MenuAPI;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus;
using vMenu.Enhanced.Menus.Developer;
using vMenu.Enhanced.Menus.Misc;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Players.Character;
using vMenu.Enhanced.Menus.Teleport;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Weapons;
using vMenu.Enhanced.Menus.Weapons.Saved;
using vMenu.Enhanced.Menus.World;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Plugins;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using PersonalVehicleFeature = vMenu.Enhanced.Menus.Vehicles.Personal.PersonalVehicle;
using StaffAlertsFeature = vMenu.Enhanced.Menus.Misc.StaffAlerts;

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
                Log.Error(line);
            }

            return;
        }

        TickRegistry.Initialize();

        ResourceShutdown.Initialize(resource);

        GameEvents.Initialize();

        ClientJson.Verify();

        UserDefaults.Initialize();

        _ = MenuController.IsAnyMenuOpen();

        PermissionsSync.RegisterEventHandlers();

        ServerActions.RegisterEventHandlers();

        TeleportSync.RegisterEventHandlers();
        ClothingPresetSync.RegisterEventHandlers();
        VehicleExtraLabels.RegisterEventHandlers();
        LocationBlipSync.RegisterEventHandlers();

        PedModelSync.RegisterEventHandlers();

        WeaponSync.RegisterEventHandlers();

        WalkingStyleSync.RegisterEventHandlers();

        NotificationEvents.RegisterEventHandlers();

        PluginHost.RegisterEventHandlers();

        ClientConfig.Initialize();

        DebugCommands.Source(
            () => ClientConfig.Value(Debugging.Client),
            Debugging.Client.Name,
            message => Log.Info($"[vMenu] {message}"));

        MenuController.MenuToggleKeyDefault = ClientConfig.Value(KeyBindings.MenuToggleKey);

        HeaderStyle.Initialize();

        LanguageLoader.Load();

        TattooCatalogue.Load();

        ClientPermissions.PermissionsChanged += TickRegistry.Reevaluate;

        VehicleCommands.Initialize();
        VehicleDumpCommands.Initialize();
        PedDumpCommands.Initialize();
        CharacterDumpCommands.Initialize();
        CharacterCamera.Initialize();
        PedHeadFit.Initialize();
        DeveloperOverlay.Initialize();
        NoClip.NoClip.Initialize();

        PvpMode.Initialize();

        PlayerGodMode.Initialize();
        PlayerSuperJump.Initialize();
        PlayerFastRun.Initialize();
        PlayerFastSwim.Initialize();
        MpStats.Initialize();
        PlayerUnlimitedOxygen.Initialize();
        PlayerNoRagdoll.Initialize();
        PlayerNoHelmet.Initialize();
        PlayerInvisible.Initialize();
        PlayerStayInVehicle.Initialize();
        PlayerFreeze.Initialize();
        EveryoneIgnoresPlayer.Initialize();
        PlayerNeverWanted.Initialize();
        PedIlluminatedClothing.Initialize();
        PedKeepProps.Initialize();
        VehicleGodMode.Initialize();
        VehicleKeepClean.Initialize();
        PersonalVehicleFeature.Initialize();

        WeaponUnlimitedAmmo.Initialize();
        WeaponNoReload.Initialize();
        ParachuteOptions.Initialize();
        CharacterRespawn.Initialize();
        WeaponLoadoutRespawn.Initialize();

        NoClip.NoClip.EntityReleased += _ =>
        {
            PlayerGodMode.Reapply();
            VehicleGodMode.Reapply();
            PlayerInvisible.Reapply();
            EveryoneIgnoresPlayer.Reapply();
            PlayerFreeze.Reapply();
        };

        TeleportKeyBinding.Initialize();
        VisorKeyBinding.Initialize();
        MinimapControls.Initialize();
        HudVisibility.Initialize();
        VisionModes.Initialize();
        TimecycleState.Initialize();
        LocationBlips.Initialize();
        FingerPointing.Initialize();
        Speedometer.Initialize();
        LocationDisplay.Initialize();
        ClearArea.Initialize();
        PlayerPresence.Initialize();
        PlayerBlipsDebugCommands.Initialize();

        PlayerPushEvents.Initialize();
        StaffAlertsFeature.Initialize();

        DeathNotifications.Initialize();
        JoinLeaveNotifications.Initialize();

        UpdateNotice.Initialize();

        WorldSync.Initialize();

        while (!ClientPermissions.HasReceivedPermissions)
        {
            await API.Yield();
        }

        TeleportSync.Request();
        ClothingPresetSync.Request();
        VehicleExtraLabels.Request();
        LocationBlipSync.Request();

        PedModelSync.Request();

        WeaponSync.Request();

        WalkingStyleSync.Request();

        UpdateNotice.Request();

        UserPreferences.Restore();

        await MenuRegistry.BuildAsync(MainMenuComposition.Definitions);

        PluginHost.AnnounceReady();

        await CharacterRespawn.ApplyOnJoinAsync();
        await WeaponLoadoutRespawn.RestoreOnJoinAsync();
    }
}
