using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

internal static class AircraftWeaponClaim
{
    private static int _claimed;

    public static bool Holds(AircraftSession session) => _claimed != 0 && _claimed == session.NetworkId;

    // Both seats can take it within one state bag delay, and then the pilot keeps it.
    public static bool Contested(AircraftSession session) =>
        !session.IsPilot && _claimed != 0 && _claimed == session.NetworkId && OtherHolds(session, _claimed);

    public static bool TryTake(AircraftSession session, bool quiet = false)
    {
        var networkId = session.NetworkId;

        if (networkId == 0 || _claimed == networkId)
        {
            return true;
        }

        if (OtherHolds(session, networkId))
        {
            if (!quiet)
            {
                Notifications.Info(MenuText.Key(Loc.VehicleOptions.AircraftWeaponsInUse));
            }

            return false;
        }

        _claimed = networkId;

        StateBags.Set(StateBags.LocalPlayerBag, PlayerStateKeys.AircraftWeapons, networkId);

        return true;
    }

    public static void Release()
    {
        if (_claimed == 0)
        {
            return;
        }

        _claimed = 0;

        StateBags.Set(StateBags.LocalPlayerBag, PlayerStateKeys.AircraftWeapons, 0);
    }

    private static bool OtherHolds(AircraftSession session, int networkId)
    {
        var other = Native.GetPedInVehicleSeat(session.Vehicle, session.IsPilot ? 0 : -1, false);

        if (other == 0 || !Native.IsPedAPlayer(other))
        {
            return false;
        }

        var player = Native.NetworkGetPlayerIndexFromPed(other);

        return player != -1
            && StateBags.GetPlayer<int>(Native.GetPlayerServerId(player), PlayerStateKeys.AircraftWeapons) == networkId;
    }
}
