using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerUnlimitedStamina
{
    private const string Stat = "MP0_STAMINA";

    private const int Full = 100;

    private const int None = 0;

    /// <summary>What the player asked for and what the server allows, which together are the only answer.</summary>
    public static bool Enabled => UserDefaults.PlayerUnlimitedStamina.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.UnlimitedStamina);

    /// <summary>Call once at startup, before permissions have arrived.</summary>
    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.PlayerUnlimitedStamina.Value = enabled;

        Apply();
    }

    // Switching off puts the skill back to nothing rather than to whatever it was before, which is
    // what vMenu has always done here. The game has no way to hand back the old number.
    private static void Apply() => Native.StatSetInt(API.Hash(Stat), Enabled ? Full : None, true);
}
