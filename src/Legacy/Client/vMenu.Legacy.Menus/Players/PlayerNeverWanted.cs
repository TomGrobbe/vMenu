using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerNeverWanted
{
    private const int NoWanted = 0;

    private const int DefaultCeiling = 5;

    private static int _restore = DefaultCeiling;

    private static bool _held;

    public static readonly ToggleFeature Toggle = new(
        UserDefaults.PlayerNeverWanted,
        PlayerOptionsPermissions.NeverWanted,
        Apply);

    private static void Apply(bool on)
    {
        if (on == _held)
        {
            return;
        }

        _held = on;

        if (!on)
        {
            API.SetMaxWantedLevel(_restore);

            return;
        }

        var ceiling = API.GetMaxWantedLevel();
        _restore = ceiling > NoWanted ? ceiling : DefaultCeiling;

        API.SetMaxWantedLevel(NoWanted);

        var player = API.PlayerId();

        API.SetPlayerWantedLevel(player, NoWanted, false);
        API.SetPlayerWantedLevelNow(player, false);
    }
}
