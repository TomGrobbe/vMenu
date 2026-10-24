using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class EveryoneIgnoresPlayer
{
    public static readonly ToggleFeature Toggle = new(
        UserDefaults.PlayerEveryoneIgnores,
        PlayerOptionsPermissions.Ignored,
        Apply);

    private static void Apply(bool on)
    {
        var player = Native.PlayerId();

        Native.SetEveryoneIgnorePlayer(player, on);
        Native.SetPoliceIgnorePlayer(player, on);
        Native.SetPlayerCanBeHassledByGangs(player, !on);
    }
}
