using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Script;

using MenuAPI;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.Core;

public sealed class Main : IScript
{
    public async void Initialize()
    {
        Native.DisableIdleCamera(true);
        Native.DisableVehiclePassengerIdleCamera(true);

        _ = new NoClip.NoClip();

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

        // Calling something to do with MenuController is required, otherwise the compiler optimizes
        // the dependency away and MenuAPI won't run at all.
        _ = MenuController.MenuToggleKeyIsValid;

        // Registered before the menus are built: the build awaits, so a permission set pushed during
        // startup would otherwise arrive with nothing listening. The build ends with its own gate
        // pass, so whatever has landed by then is picked up regardless.
        PermissionsSync.RegisterEventHandlers();

        // Before the build, so the gate pass at the end of it reads real values rather than
        // treating every convar backed menu as switched off.
        ClientConfig.Initialize();

        while (!ClientPermissions.HasReceivedPermissions)
        {
            await API.Yield();
        }

        await MenuRegistry.BuildAsync(MainMenuComposition.Definitions);
    }
}
