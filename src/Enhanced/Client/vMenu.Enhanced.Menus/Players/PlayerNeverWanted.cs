using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

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
            Native.SetMaxWantedLevel(_restore);

            return;
        }

        var ceiling = Native.GetMaxWantedLevel();
        _restore = ceiling > NoWanted ? ceiling : DefaultCeiling;

        Native.SetMaxWantedLevel(NoWanted);

        var player = Native.PlayerId();

        Native.SetPlayerWantedLevel(player, NoWanted, false);
        Native.SetPlayerWantedLevelNow(player, false);
    }
}
