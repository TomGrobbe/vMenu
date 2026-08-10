using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Script;

using MenuAPI;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus;
using vMenu.Enhanced.Menus.Developer;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Teleport;
using vMenu.Enhanced.Menus.Vehicles;
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
        TickRegistry.Initialize();

        GameEvents.Initialize();

        SharedAPI.Commands.RegisterCommand("give", false, async (string? weapon) =>
        {
            var ped = Native.PlayerPedId();
            if (string.IsNullOrWhiteSpace(weapon))
            {
                API.Log.Error("Invalid weapon");
                return;
            }
            var weaponHash = API.Hash(weapon);
            if (!Native.IsWeaponValid(weaponHash))
            {
                API.Log.Error("Invalid weapon hash: {0}", weaponHash);
                return;
            }

            Native.RequestModel(weaponHash);

            Native.GiveWeaponToPed(ped, weaponHash, 1000, true, true);
        });

        ClientJson.Verify();

        UserDefaults.Initialize();

        _ = MenuController.IsAnyMenuOpen();

        PermissionsSync.RegisterEventHandlers();

        ServerActions.RegisterEventHandlers();

        TeleportSync.RegisterEventHandlers();

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
        DeveloperOverlay.Initialize();
        NoClip.NoClip.Initialize();

        PlayerGodMode.Initialize();
        VehicleGodMode.Initialize();
        VehicleKeepClean.Initialize();

        // Noclip puts every flag on the entity it was moving back to the game's defaults, which takes
        // god mode with it. Both are idempotent and find their own entity, so the handle is ignored.
        NoClip.NoClip.EntityReleased += _ =>
        {
            PlayerGodMode.Reapply();
            VehicleGodMode.Reapply();
        };

        TeleportKeyBinding.Initialize();

        PlayerPushEvents.Initialize();

        WorldSync.Initialize();

        while (!ClientPermissions.HasReceivedPermissions)
        {
            await API.Yield();
        }

        TeleportSync.Request();

        UserPreferences.Restore();

        await MenuRegistry.BuildAsync(MainMenuComposition.Definitions);
    }
}
