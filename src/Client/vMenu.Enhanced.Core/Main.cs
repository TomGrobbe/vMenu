using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Script;

using MenuAPI;

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

        //foreach (var subClass in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
        //{
        //    if (subClass.Namespace == "vMenu.Enhanced.Configuration")
        //    {
        //        API.Log.Info("Found class inside vMenu.Enhanced.Configuration: {0}", subClass.Name);
        //    }
        //}
    }

    public Main()
    {
        var menu = new Menu("Test menu");
        MenuController.AddMenu(menu);

        for (var i = 0; i < 20; i++)
        {
            menu.AddMenuItem(new MenuItem($"Menu item #{i + 1}"));
        }
    }
}
