using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Weapons;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerStatusReport
{
    private const string UnarmedSpawnName = "weapon_unarmed";

    public static void OnRequested(string requestId)
    {
        var ped = Native.PlayerPedId();
        var vehicle = Native.GetVehiclePedIsIn(ped, false);

        var vehicleName = string.Empty;
        var vehicleModel = string.Empty;

        if (vehicle != 0 && Native.DoesEntityExist(vehicle))
        {
            var hash = Native.GetEntityModel(vehicle);

            vehicleModel = VehicleModelNames.Resolve(hash);
            vehicleName = DisplayName(hash, vehicleModel);
        }

        API.EmitServer(
            PlayerEvents.StatusReportAck,
            requestId,
            PlayerGodMode.Enabled,
            VehicleGodMode.Enabled,
            vehicleName,
            vehicleModel,
            WeaponName(Native.GetSelectedPedWeapon(ped)));
    }

    private static string DisplayName(uint hash, string fallback)
    {
        var label = Native.GetDisplayNameFromVehicleModel(hash);
        var text = Native.GetLabelText(label);

        return string.IsNullOrWhiteSpace(text) || text == "NULL" ? fallback : text;
    }

    private static string WeaponName(uint hash)
    {
        if (hash == API.Hash(UnarmedSpawnName))
        {
            return "Unarmed";
        }

        foreach (var category in WeaponSync.Categories)
        {
            foreach (var weapon in category.Weapons)
            {
                if (API.Hash(weapon.SpawnName) == hash)
                {
                    return WeaponNames.Resolve(weapon.Label, weapon.SpawnName);
                }
            }
        }

        return $"0x{hash:X8}";
    }
}
