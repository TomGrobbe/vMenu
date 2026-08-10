using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerUnlimitedOxygen
{
    /// <summary>Eleven days of air, which the player will not outlast.</summary>
    private const float Forever = 1_000_000f;

    private const float FallbackCeiling = 20f;

    private static float _ceiling = FallbackCeiling;

    private static int _ceilingOf;

    /// <summary>Whether the ceiling is currently raised, so switching off knows there is work to do.</summary>
    private static bool _raised;

    private static bool _watching;

    public static bool Enabled => UserDefaults.PlayerUnlimitedOxygen.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.UnlimitedOxygen);

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

        UserDefaults.PlayerUnlimitedOxygen.Value = enabled;

        Apply();
    }

    private static void Apply()
    {
        var on = Enabled;

        Watch(on);

        // Off and never raised is every player who has not touched this, and writing a ceiling at
        // them would cost them whatever breath capacity they had earned.
        if (!on && !_raised)
        {
            return;
        }

        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        if (on)
        {
            Remember(ped);
        }

        _raised = on;

        Native.SetPedMaxTimeUnderwater(ped, on ? Forever : _ceiling);
        Native.SetPedDiesInWater(ped, !on);
    }

    private static void Remember(int ped)
    {
        // Reading a ceiling vMenu has already raised would answer Forever, which is not a ceiling
        // worth remembering.
        if (_raised && ped == _ceilingOf)
        {
            return;
        }

        var remaining = Native.GetPlayerUnderwaterTimeRemaining(Native.PlayerId());

        _ceilingOf = ped;

        // Zero means the player was already holding their breath when they switched this on, so the
        // reading is how much they had left rather than how much they started with.
        _ceiling = remaining > 0f ? remaining : FallbackCeiling;
    }

    private static void Watch(bool watching)
    {
        if (watching == _watching)
        {
            return;
        }

        _watching = watching;

        if (watching)
        {
            LocalPlayerTicks.PlayerPedIdChanged += OnPedChanged;
            LocalPlayerTicks.PlayerPedRevived += OnRevived;

            return;
        }

        LocalPlayerTicks.PlayerPedIdChanged -= OnPedChanged;
        LocalPlayerTicks.PlayerPedRevived -= OnRevived;
    }

    // A respawn hands the player a new ped, on the game's defaults, so the answer has to be written again.
    private static void OnPedChanged(PlayerPedIdChanged _) => Apply();

    // Drowning and being fished back out keeps the handle, so the change above never fires for it.
    private static void OnRevived(PlayerPedRevived _) => Apply();
}
