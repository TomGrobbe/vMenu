using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerStayInVehicle
{
    private static readonly PedProtection.Claim Protection = PedProtection.Register();

    public static readonly ToggleFeature Toggle = new(
        UserDefaults.PlayerStayInVehicle,
        PlayerOptionsPermissions.StayInVehicle,
        on => Protection.Set(on, PedProtections.NotDraggedOut));
}
