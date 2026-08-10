using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerSuperJump
{
    private static TickHandle? _tick;

    /// <summary>What the player asked for and what the server allows, which together are the only answer.</summary>
    public static bool Enabled => UserDefaults.PlayerSuperJump.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.SuperJump);

    /// <summary>Call once at startup, before permissions have arrived.</summary>
    // A permission arriving or being revoked re-runs every tick condition through the registry, so
    // this deliberately does not subscribe to that itself.
    public static void Initialize() =>
        _tick = TickRegistry.Register("Player.SuperJump", Apply, TickRate.PerFrame, () => Enabled);

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.PlayerSuperJump.Value = enabled;

        _tick?.Reevaluate();
    }

    private static void Apply() => Native.SetSuperJumpThisFrame(Native.PlayerId());
}
