using System.Globalization;

using CitizenFX.FiveM.Server;

namespace vMenu.Enhanced.Actions.Server;

/// <summary>Finding the player or entity an action is aimed at, from what a client sent.</summary>
public static class Targets
{
    private const int VehicleEntityType = 2;

    public static string Id(int serverId) => serverId.ToString(CultureInfo.InvariantCulture);

    public static bool TryResolve(string[] args, out int serverId) =>
        ActionArgs.TryInt(args, 0, out serverId) && Native.DoesPlayerExist(Id(serverId));

    /// <summary>Null for somebody still connecting: they already hold a server id, so they answer DoesPlayerExist
    /// while having nothing in the world.</summary>
    public static int? PedOf(int serverId)
    {
        var ped = Native.GetPlayerPed(Id(serverId));

        return ped != 0 && Native.DoesEntityExist(ped) ? ped : null;
    }

    /// <summary>There is no IsEntityAVehicle server side, so the entity type is the check.</summary>
    public static bool IsVehicle(int entity) =>
        entity != 0 && Native.DoesEntityExist(entity) && Native.GetEntityType(entity) == VehicleEntityType;
}
