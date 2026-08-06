using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Script;

using MenuAPI;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus;
using vMenu.Enhanced.Menus.Developer;
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
        Native.DisableIdleCamera(true);
        Native.DisableVehiclePassengerIdleCamera(true);

        TickRegistry.Initialize();

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

        // Calling something to do with MenuController is required, otherwise the compiler optimizes
        // the dependency away and MenuAPI won't run at all.
        _ = MenuController.IsAnyMenuOpen();

        // Registered before the menus are built: the build awaits, so a permission set pushed during
        // startup would otherwise arrive with nothing listening. The build ends with its own gate
        // pass, so whatever has landed by then is picked up regardless.
        PermissionsSync.RegisterEventHandlers();

        // Before anything can invoke one, or its reply arrives with nothing listening.
        ServerActions.RegisterEventHandlers();

        // Before the build, so the gate pass at the end of it reads real values rather than
        // treating every convar backed menu as switched off.
        ClientConfig.Initialize();

        // MenuAPI registers its key mappings on the first tick, and nothing here awaits before then,
        // so this lands in time. It is only the starting key: a player who has rebound it keeps theirs.
        MenuController.MenuToggleKeyDefault = ClientConfig.Value(KeyBindings.MenuToggleKey);

        // After the config, whose convar names the languages, and before the build, since the
        // picker's options are fixed once its item exists.
        LanguageLoader.Load();

        // Every gated tick answers to the same two events the menu gates do, so a convar edit or an
        // ACL change starts and stops loops without anything else having to subscribe.
        ClientConfig.Changed += TickRegistry.Reevaluate;
        ClientPermissions.PermissionsChanged += TickRegistry.Reevaluate;

        // All four watch their own setting and permission, so they need no particular order here.
        VehicleCommands.Initialize();
        DeveloperOverlay.Initialize();
        NoClip.NoClip.Initialize();

        // Before the wait below, so the sky and the clock are right on the player's first frame
        // rather than snapping into place after they spawn.
        WorldSync.Initialize();

        while (!ClientPermissions.HasReceivedPermissions)
        {
            await API.Yield();
        }

        UserPreferences.Restore();

        await MenuRegistry.BuildAsync(MainMenuComposition.Definitions);
    }
}
