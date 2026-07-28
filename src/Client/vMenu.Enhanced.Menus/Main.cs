using CitizenFX.Core;
using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Script;

using MenuAPI;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Placeholder so the project produces a valid assembly. Replace with the
/// individual menu classes as the port lands.
/// </summary>
public sealed class Main : Entrypoint, IScript
{
    public async void Initialize()
    {
        //new NoClip.NoClip().Initialize();

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

            //var time = TimeProvider.System.GetUtcNow();
            //while (!Native.HasModelLoaded(weaponHash))
            //{
            //    await API.Yield();
            //    if (TimeProvider.System.GetUtcNow() - time > TimeSpan.FromSeconds(5))
            //    {
            //        API.Log.Warn("Failed to load weapon model in time. ModelValid? {0}", Native.IsModelValid(weaponHash));
            //        return;
            //    }
            //}

            Native.GiveWeaponToPed(ped, weaponHash, 1000, true, true);
        });

        SharedAPI.Commands.RegisterCommand("checkmodel", false, async (string? hash = null) =>
        {
            API.Log.Info("[Log] args: {0}", hash);
            if (string.IsNullOrWhiteSpace(hash))
            {
                API.Log.Error("Invalid hash: {0}", hash);
                return;
            }
            var hashHash = API.Hash(hash!);
            var valid = Native.IsModelValid(hashHash);

            if (valid)
            {
                API.Log.Info("Model {0} is valid. Hash: {1}", hash, hashHash);
            }
            else
            {
                API.Log.Warn("Model {0} is NOT valid. Hash: {1}", hash, hashHash);
            }

            var isValidWeapon = Native.IsWeaponValid(hashHash);
            if (isValidWeapon)
            {
                API.Log.Info("Weapon {0} is valid. Hash: {1}", hash, hashHash);
            }
            else
            {
                API.Log.Warn("Weapon {0} is NOT valid. Hash: {1}", hash, hashHash);
            }
        });

        await API.Yield();
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
