using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerUnlimitedOxygen
{
    // Eleven days of air, which the player will not outlast.
    private const float Forever = 1_000_000f;

    private const float FallbackCeiling = 20f;

    private static float _ceiling = FallbackCeiling;

    private static int _ceilingOf;

    // Whether the ceiling is currently raised, so switching off knows there is work to do.
    private static bool _raised;

    // Drowning and being fished back out keeps the handle, so a ped change alone never catches it.
    public static readonly ToggleFeature Toggle = new(
        UserDefaults.PlayerUnlimitedOxygen,
        PlayerOptionsPermissions.UnlimitedOxygen,
        Apply,
        ToggleWatch.PedChanged | ToggleWatch.PedRevived);

    private static void Apply(bool on)
    {
        // Off and never raised is every player who has not touched this, and writing a ceiling at them would
        // cost them whatever breath capacity they had earned.
        if (!on && !_raised)
        {
            return;
        }

        var ped = API.PlayerPedId();

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            return;
        }

        if (on)
        {
            Remember(ped);
        }

        _raised = on;

        API.SetPedMaxTimeUnderwater(ped, on ? Forever : _ceiling);
        API.SetPedDiesInWater(ped, !on);
    }

    private static void Remember(int ped)
    {
        // Reading a ceiling vMenu has already raised would answer Forever, which is not a ceiling worth
        // remembering.
        if (_raised && ped == _ceilingOf)
        {
            return;
        }

        var remaining = API.GetPlayerUnderwaterTimeRemaining(API.PlayerId());

        _ceilingOf = ped;

        // Zero means the player was already holding their breath when they switched this on, so the reading
        // is how much they had left rather than how much they started with.
        _ceiling = remaining > 0f ? remaining : FallbackCeiling;
    }
}
