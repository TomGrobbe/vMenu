using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

namespace vMenu.Enhanced.Menus.Weapons;

/// <summary>
/// Parachutes, their colours, and the two loops that keep them topped up.
/// </summary>
public static class ParachuteOptions
{
    internal const string ParachuteModel = "gadget_parachute";

    /// <summary>Neither loop is doing anything most of the time, so neither needs a frame.</summary>
    private const int CheckMs = 500;

    /// <summary>How long the game takes to change the smoke colour, during which it cannot be used.</summary>
    private const int SmokeChangeMs = 4000;

    private static TickHandle? _autoEquip;
    private static TickHandle? _unlimited;

    private static bool _changingSmoke;

    /// <summary>
    /// The chute styles, in the order the game numbers them. The last six are the canopy styles,
    /// which the game accepts but does not draw in FiveM.
    /// </summary>
    private static readonly string[] StyleLabels =
    [
        "PM_TINT0", "PM_TINT1", "PM_TINT2", "PM_TINT3", "PM_TINT4", "PM_TINT5", "PM_TINT6", "PM_TINT7",
        "PS_CAN_0", "PS_CAN_1", "PS_CAN_2", "PS_CAN_3", "PS_CAN_4", "PS_CAN_5",
    ];

    private static readonly string[] SmokeLabels =
    [
        "PM_TINT8", "PM_TINT9", "PM_TINT10", "PM_TINT11", "PM_TINT12", "PM_TINT13",
    ];

    /// <summary>Index aligned with <see cref="SmokeLabels"/>. The first is "no smoke".</summary>
    private static readonly (int R, int G, int B)[] SmokeColours =
    [
        (255, 255, 255),
        (255, 0, 0),
        (255, 165, 0),
        (255, 255, 0),
        (0, 0, 255),
        (20, 20, 20),
    ];

    public static bool IsAllowed => ClientPermissions.IsAllowed(WeaponOptionsPermissions.Parachute);

    /// <summary>What the player asked for and what the server allows, which together are the only answer.</summary>
    public static bool AutoEquipEnabled => UserDefaults.WeaponsAutoEquipParachute.Value && IsAllowed;

    /// <inheritdoc cref="AutoEquipEnabled"/>
    public static bool UnlimitedEnabled => UserDefaults.WeaponsUnlimitedParachutes.Value && IsAllowed;

    public static int StyleCount => StyleLabels.Length;

    public static int SmokeCount => SmokeLabels.Length;

    /// <summary>Call once at startup, before permissions have arrived.</summary>
    // Both conditions include the permission, so revoking it stops the loops on their own. vMenu
    // used to check only the player's own toggle here, which let a saved preference keep working on
    // a server that had taken the permission away.
    public static void Initialize()
    {
        _autoEquip = TickRegistry.Register(
            "Weapons.AutoEquipParachute", ApplyAutoEquip, TickRate.Every(CheckMs), () => AutoEquipEnabled);

        _unlimited = TickRegistry.Register(
            "Weapons.UnlimitedParachutes", ApplyUnlimited, TickRate.Every(CheckMs), () => UnlimitedEnabled);
    }

    public static void SetAutoEquip(bool enabled)
    {
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.WeaponsAutoEquipParachute.Value = enabled;

        _autoEquip?.Reevaluate();
    }

    public static void SetUnlimited(bool enabled)
    {
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.WeaponsUnlimitedParachutes.Value = enabled;

        _unlimited?.Reevaluate();
    }

    public static bool HasPrimary() =>
        Native.HasPedGotWeapon(Native.PlayerPedId(), API.Hash(ParachuteModel), 0);

    /// <summary>Gives the parachute, or takes it back. True when the player now has one.</summary>
    public static bool TogglePrimary()
    {
        var ped = Native.PlayerPedId();
        var hash = API.Hash(ParachuteModel);

        if (HasPrimary())
        {
            Native.RemoveWeaponFromPed(ped, hash);

            return false;
        }

        Native.GiveWeaponToPed(ped, hash, 1, false, false);

        return true;
    }

    /// <summary>One way only. The game offers no way to take a reserve chute back off.</summary>
    public static void EnableReserve() => Native.SetPlayerHasReserveParachute(Native.PlayerId());

    public static MenuText StyleName(int index) =>
        MenuText.From(() => WeaponNames.Resolve(StyleLabels[index], string.Empty));

    public static MenuText SmokeName(int index) =>
        MenuText.From(() => WeaponNames.Resolve(SmokeLabels[index], string.Empty));

    /// <summary>
    /// The game's own description of a chute style. The canopy ones say plainly that they do not
    /// work, rather than leaving the player wondering why nothing changed.
    /// </summary>
    public static MenuText StyleDescription(int index) =>
        MenuText.From(() =>
        {
            var text = WeaponNames.Resolve(index < 8 ? $"PD_TINT{index}" : $"PSD_CAN_{index - 8}", string.Empty);

            return index < 8
                ? text
                : $"{text} ~r~This one does not work in FiveM.";
        });

    public static void SetPrimaryStyle(int index)
    {
        if (IsAllowed)
        {
            Native.SetPlayerParachuteTintIndex(Native.PlayerId(), index);
        }
    }

    public static void SetReserveStyle(int index)
    {
        if (IsAllowed)
        {
            Native.SetPlayerReserveParachuteTintIndex(Native.PlayerId(), index);
        }
    }

    /// <summary>
    /// Changing the colour means turning the trail off, waiting, and turning it back on, so a second
    /// change while one is running is dropped rather than queued.
    /// </summary>
    public static async Task SetSmokeColourAsync(int index)
    {
        if (!IsAllowed || _changingSmoke || index < 0 || index >= SmokeColours.Length)
        {
            return;
        }

        _changingSmoke = true;

        try
        {
            var player = Native.PlayerId();
            var (r, g, b) = SmokeColours[index];

            Native.SetPlayerCanLeaveParachuteSmokeTrail(player, false);

            await API.Delay(SmokeChangeMs);

            Native.SetPlayerParachuteSmokeTrailColor(player, r, g, b);

            // The first entry is the "no smoke" one, so it is set and then deliberately left off.
            Native.SetPlayerCanLeaveParachuteSmokeTrail(player, index != 0);
        }
        finally
        {
            _changingSmoke = false;
        }
    }

    private static void ApplyAutoEquip()
    {
        var ped = Native.PlayerPedId();

        if (!Native.IsPedInAnyHeli(ped) && !Native.IsPedInAnyPlane(ped))
        {
            return;
        }

        if (!HasPrimary())
        {
            Native.GiveWeaponToPed(ped, API.Hash(ParachuteModel), 1, false, false);
        }

        if (!Native.GetPlayerHasReserveParachute(Native.PlayerId()))
        {
            Native.SetPlayerHasReserveParachute(Native.PlayerId());
        }
    }

    private static void ApplyUnlimited()
    {
        var ped = Native.PlayerPedId();

        if (!HasPrimary())
        {
            Native.GiveWeaponToPed(ped, API.Hash(ParachuteModel), 1, false, false);
        }

        if (!Native.GetPlayerHasReserveParachute(Native.PlayerId()))
        {
            Native.SetPlayerHasReserveParachute(Native.PlayerId());
        }
    }
}
