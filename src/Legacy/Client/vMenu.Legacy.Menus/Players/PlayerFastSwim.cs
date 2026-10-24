using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerFastSwim
{
    // The game clamps anything above this back down, so it is the real ceiling.
    private const float Fast = 1.49f;

    private const float Normal = 1f;

    // The multiplier travels with the ped's movement data rather than with the player, so a respawn or a
    // model swap hands back one swimming at normal speed.
    public static readonly ToggleFeature Toggle = new(
        UserDefaults.PlayerFastSwim,
        PlayerOptionsPermissions.FastSwim,
        Apply,
        ToggleWatch.PedChanged);

    private static void Apply(bool on) =>
        API.SetSwimMultiplierForPlayer(API.PlayerId(), on ? Fast : Normal);
}
