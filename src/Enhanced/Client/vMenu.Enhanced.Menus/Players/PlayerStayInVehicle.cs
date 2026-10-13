using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerStayInVehicle
{
    private static readonly PedProtection.Claim Protection = PedProtection.Register();

    public static bool Enabled => UserDefaults.PlayerStayInVehicle.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.StayInVehicle);

    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetEnabled(bool enabled)
    {
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.PlayerStayInVehicle.Value = enabled;

        Apply();
    }

    private static void Apply() => Protection.Set(Enabled, PedProtections.NotDraggedOut);
}
