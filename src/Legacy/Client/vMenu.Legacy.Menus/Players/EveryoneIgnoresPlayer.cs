using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players;

public static class EveryoneIgnoresPlayer
{
    public static readonly ToggleFeature Toggle = new(
        UserDefaults.PlayerEveryoneIgnores,
        PlayerOptionsPermissions.Ignored,
        Apply);

    private static void Apply(bool on)
    {
        var player = API.PlayerId();

        API.SetEveryoneIgnorePlayer(player, on);
        API.SetPoliceIgnorePlayer(player, on);
        API.SetPlayerCanBeHassledByGangs(player, !on);
    }
}
