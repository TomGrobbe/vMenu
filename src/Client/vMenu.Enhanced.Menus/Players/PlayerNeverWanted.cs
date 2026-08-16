using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerNeverWanted
{
    private const int NoWanted = 0;

    private const int DefaultCeiling = 5;

    private static int _restore = DefaultCeiling;

    private static bool _held;

    public static bool Enabled => UserDefaults.PlayerNeverWanted.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.NeverWanted);

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

        UserDefaults.PlayerNeverWanted.Value = enabled;

        Apply();
    }

    private static void Apply()
    {
        var on = Enabled;

        if (on == _held)
        {
            return;
        }

        _held = on;

        if (!on)
        {
            Native.SetMaxWantedLevel(_restore);

            return;
        }

        _restore = Native.GetMaxWantedLevel();

        Native.SetMaxWantedLevel(NoWanted);

        var player = Native.PlayerId();

        Native.SetPlayerWantedLevel(player, NoWanted, false);
        Native.SetPlayerWantedLevelNow(player, false);
    }
}
