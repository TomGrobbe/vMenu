using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class EveryoneIgnoresPlayer
{
    public static bool Enabled => UserDefaults.PlayerEveryoneIgnores.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.Ignored);

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

        UserDefaults.PlayerEveryoneIgnores.Value = enabled;

        Apply();
    }

    public static void Reapply() => Apply();

    private static void Apply()
    {
        var on = Enabled;
        var player = Native.PlayerId();

        Native.SetEveryoneIgnorePlayer(player, on);
        Native.SetPoliceIgnorePlayer(player, on);
        Native.SetPlayerCanBeHassledByGangs(player, !on);
    }
}
