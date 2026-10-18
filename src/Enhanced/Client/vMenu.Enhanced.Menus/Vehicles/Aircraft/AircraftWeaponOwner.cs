using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

public static class AircraftWeaponOwner
{
    public static void RegisterEventHandlers() =>
        API.OnNetEvent(AircraftWeaponEvents.Apply, new Action<string, string>(OnApply), false);

    internal static void Request(int vehicle, string effect)
    {
        if (!Native.DoesEntityExist(vehicle))
        {
            return;
        }

        if (Native.NetworkHasControlOfEntity(vehicle))
        {
            Apply(vehicle, effect);

            return;
        }

        var networkId = Native.NetworkGetNetworkIdFromEntity(vehicle);

        if (networkId != 0)
        {
            Forward(networkId.ToString(CultureInfo.InvariantCulture), effect);
        }
    }

    private static async void Forward(string networkId, string effect)
    {
        try
        {
            var result = await ServerActions.InvokeAsync(ActionIds.VehicleOptions.AircraftWeapon, networkId, effect);

            if (!result.IsOk)
            {
                Log.Warning($"[Vehicles] The server did not pass '{effect}' on to the owner of {networkId}: {result.Status}.");
            }
        }
        catch (Exception exception)
        {
            Log.Error($"[Vehicles] Could not ask the owner of {networkId} for '{effect}': {exception}");
        }
    }

    // What the other seat asked for through the server can no longer arrive once the resource stops.
    internal static void LetGo(int vehicle)
    {
        if (!Native.DoesEntityExist(vehicle) || !Native.NetworkHasControlOfEntity(vehicle))
        {
            return;
        }

        if (Native.GetAreBombBayDoorsOpen(vehicle))
        {
            Apply(vehicle, AircraftWeaponEvents.CloseBombBay);
        }

        Apply(vehicle, AircraftWeaponEvents.AllowLockOn);
    }

    private static void OnApply(string networkId, string effect)
    {
        if (!int.TryParse(networkId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            return;
        }

        var vehicle = NetworkEntity.Find(id);

        if (vehicle != 0 && Native.NetworkHasControlOfEntity(vehicle))
        {
            Apply(vehicle, effect);
        }
    }

    private static void Apply(int vehicle, string effect)
    {
        switch (effect)
        {
            case AircraftWeaponEvents.OpenBombBay:
                Native.OpenBombBayDoors(vehicle);
                break;
            case AircraftWeaponEvents.CloseBombBay:
                Native.CloseBombBayDoors(vehicle);
                break;
            case AircraftWeaponEvents.AllowLockOn:
                Native.SetVehicleAllowHomingMissleLockonSynced(vehicle, true, true);
                break;
            case AircraftWeaponEvents.BlockLockOn:
                Native.SetVehicleAllowHomingMissleLockonSynced(vehicle, false, true);
                break;
        }
    }
}
