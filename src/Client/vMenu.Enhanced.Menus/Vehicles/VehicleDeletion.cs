using System.Globalization;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Extensions;
using CitizenFX.FiveM.Shared.Data;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using VehicleOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleDeletion
{
    private static bool _running;

    /// <summary>
    /// Deletes whatever the player is targeting and tells them how it went. Shared by the menu option
    /// and the /dv command.
    /// </summary>
    public static async Task DeleteTargetAsync()
    {
        // The menu button drops re-entrant selections itself; the command has nothing that would.
        if (_running)
        {
            return;
        }

        _running = true;

        try
        {
            var ped = API.Players.Local.Ped;

            if (ped is null || ped.IsDeadOrDying)
            {
                return;
            }

            var target = await VehicleTargeting.ResolveAsync(
                ped,
                ClientConfig.Value(VehicleOptionsSettings.DeleteVehicleDistance));

            // Passengers do not get to delete the car out from under the driver.
            if (target.Kind is VehicleTargetKind.Passenger)
            {
                Notifications.Error(MenuText.Key(Loc.VehicleOptions.DeleteNotDriver));

                return;
            }

            if (!target.Found)
            {
                Notifications.Error(MenuText.Key(Loc.VehicleOptions.DeleteNoVehicle));

                return;
            }

            Notify(await SendDeleteAsync(target.Handle));
        }
        finally
        {
            _running = false;
        }
    }

    /// <summary>
    /// The server does the deleting, so this client never needs control of the entity and cannot
    /// decide on its own that somebody else's vehicle may go.
    /// </summary>
    private static async Task<ActionStatus> SendDeleteAsync(int entity)
    {
        // The server cannot see this one, and by definition nobody else can either.
        if (!Native.NetworkGetEntityIsNetworked(entity))
        {
            return DeleteLocally(entity) ? ActionStatus.Ok : ActionStatus.Failed;
        }

        var networkId = Native.NetworkGetNetworkIdFromEntity(entity);

        var result = await ServerActions.InvokeAsync(
            ActionIds.VehicleOptions.DeleteVehicle,
            networkId.ToString(CultureInfo.InvariantCulture));

        return result.Status;
    }

    internal static bool DeleteLocally(int entity)
    {
        Native.SetVehicleHasBeenOwnedByPlayer(entity, false);
        Native.SetEntityAsMissionEntity(entity, true, true);

        // Ref<T> is a ref struct and cannot live across an await. The out overload of this native
        // pushes a literal 0 and can only ever delete entity 0.
        var handle = entity;
        Native.DeleteVehicle(new Ref<int>(ref handle));

        return !Native.DoesEntityExist(entity);
    }

    private static void Notify(ActionStatus status)
    {
        if (status == ActionStatus.Ok)
        {
            Notifications.Success(MenuText.Key(Loc.VehicleOptions.Deleted));

            return;
        }

        Notifications.Error(MenuText.Key(status switch
        {
            ActionStatus.Denied => Loc.VehicleOptions.DeleteDenied,
            ActionStatus.TooFar => Loc.VehicleOptions.DeleteTooFar,

            // The only rule this action refuses on.
            ActionStatus.Refused => Loc.VehicleOptions.DeleteNotDriver,

            _ => Loc.VehicleOptions.DeleteFailed,
        }));
    }
}
