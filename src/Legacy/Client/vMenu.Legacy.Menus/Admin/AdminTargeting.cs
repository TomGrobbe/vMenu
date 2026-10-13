
using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Menus.Players;

using AdminSettings = vMenu.Enhanced.Data.Configuration.Settings.Admin;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Admin;

public static class AdminTargeting
{
    public static RosteredPlayer? Closest()
    {
        var ped = API.PlayerPedId();

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            return null;
        }

        PlayerRoster.Refresh();

        var self = API.GetPlayerServerId(API.PlayerId());
        var origin = API.GetEntityCoords(ped, true);
        var reach = (float)Range();
        var limit = reach * reach;

        RosteredPlayer? closest = null;
        var closestDistance = float.MaxValue;

        foreach (var player in PlayerRoster.All)
        {
            if (player.ServerId == self)
            {
                continue;
            }

            var distance = Vector3.DistanceSquared(origin, player.Position);

            if (distance > limit || distance >= closestDistance)
            {
                continue;
            }

            closest = player;
            closestDistance = distance;
        }

        return closest;
    }

    private static int Range() =>
        AdminSettings.ClampClosestPlayerRange(ClientConfig.Value(AdminSettings.ClosestPlayerRange));
}
