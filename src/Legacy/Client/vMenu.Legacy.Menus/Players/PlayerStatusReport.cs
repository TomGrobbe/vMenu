using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Weapons;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerStatusReport
{
    private const string UnarmedSpawnName = "weapon_unarmed";

    public static void OnRequested(string requestId)
    {
        var ped = API.PlayerPedId();
        var vehicle = API.GetVehiclePedIsIn(ped, false);

        var vehicleName = string.Empty;
        var vehicleModel = string.Empty;

        if (vehicle != 0 && API.DoesEntityExist(vehicle))
        {
            var hash = NativeFixer.GetEntityModel(vehicle);

            vehicleModel = VehicleModelNames.Resolve(hash);
            vehicleName = DisplayName(hash, vehicleModel);
        }

        BaseScript.TriggerServerEvent(
            PlayerEvents.StatusReportAck,
            requestId,
            PlayerGodMode.Toggle.Enabled,
            VehicleGodMode.Enabled,
            vehicleName,
            vehicleModel,
            WeaponName(NativeFixer.GetSelectedPedWeapon(ped)));
    }

    private static string DisplayName(uint hash, string fallback)
    {
        var label = API.GetDisplayNameFromVehicleModel(hash);
        var text = API.GetLabelText(label);

        return string.IsNullOrWhiteSpace(text) || text == "NULL" ? fallback : text;
    }

    private static string WeaponName(uint hash)
    {
        if (hash == Hashing.Joaat(UnarmedSpawnName))
        {
            return "Unarmed";
        }

        foreach (var category in WeaponSync.Categories)
        {
            foreach (var weapon in category.Weapons)
            {
                if (Hashing.Joaat(weapon.SpawnName) == hash)
                {
                    return WeaponNames.Resolve(weapon.Label, weapon.SpawnName);
                }
            }
        }

        return $"0x{hash:X8}";
    }
}
