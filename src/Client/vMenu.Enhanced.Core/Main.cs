using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Script;

using MenuAPI;

using vMenu.Enhanced.Menus;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.Core;

public sealed class Main : IScript
{
    public async void Initialize()
    {

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

        SharedAPI.Commands.RegisterCommand("spawnadder", false, async (string? weapon) =>
        {
            await API.Vehicles.RequestAndCreate(API.Hash("adder"), API.Players.Local.Position, 0, true, true, true);
        });

        var menu = new Menu("vMenu Enhanced", "Main Menu");
        MenuController.AddMenu(menu);

        await (new VehicleSpawnerMenu()).Initialize();


        PermissionsSync.RegisterEventHandlers();
    }
}
