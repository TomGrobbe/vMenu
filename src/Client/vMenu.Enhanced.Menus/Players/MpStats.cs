using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;
using PlayerStatsSettings = vMenu.Enhanced.Data.Configuration.Settings.PlayerStats;

namespace vMenu.Enhanced.Menus.Players;

public sealed class MpStat
{
    public required string Stat { get; init; }

    public required IntDefault Preference { get; init; }

    public required IntSetting Limit { get; init; }

    public required string TextKey { get; init; }

    public required string DescriptionKey { get; init; }
}

public static class MpStats
{
    public const int Step = 5;

    public const int Full = 100;

    public static MpStat[] All { get; } =
    [
        new()
        {
            Stat = "MP0_SHOOTING_ABILITY",
            Preference = UserDefaults.PlayerStatShooting,
            Limit = PlayerStatsSettings.MaxShooting,
            TextKey = Loc.PlayerOptions.StatShooting,
            DescriptionKey = Loc.PlayerOptions.StatShootingDescription,
        },
        new()
        {
            Stat = "MP0_STRENGTH",
            Preference = UserDefaults.PlayerStatStrength,
            Limit = PlayerStatsSettings.MaxStrength,
            TextKey = Loc.PlayerOptions.StatStrength,
            DescriptionKey = Loc.PlayerOptions.StatStrengthDescription,
        },
        new()
        {
            Stat = "MP0_STAMINA",
            Preference = UserDefaults.PlayerStatStamina,
            Limit = PlayerStatsSettings.MaxStamina,
            TextKey = Loc.PlayerOptions.StatStamina,
            DescriptionKey = Loc.PlayerOptions.StatStaminaDescription,
        },
        new()
        {
            Stat = "MP0_STEALTH_ABILITY",
            Preference = UserDefaults.PlayerStatStealth,
            Limit = PlayerStatsSettings.MaxStealth,
            TextKey = Loc.PlayerOptions.StatStealth,
            DescriptionKey = Loc.PlayerOptions.StatStealthDescription,
        },
        new()
        {
            Stat = "MP0_FLYING_ABILITY",
            Preference = UserDefaults.PlayerStatFlying,
            Limit = PlayerStatsSettings.MaxFlying,
            TextKey = Loc.PlayerOptions.StatFlying,
            DescriptionKey = Loc.PlayerOptions.StatFlyingDescription,
        },
        new()
        {
            Stat = "MP0_WHEELIE_ABILITY",
            Preference = UserDefaults.PlayerStatDriving,
            Limit = PlayerStatsSettings.MaxDriving,
            TextKey = Loc.PlayerOptions.StatDriving,
            DescriptionKey = Loc.PlayerOptions.StatDrivingDescription,
        },
        new()
        {
            Stat = "MP0_LUNG_CAPACITY",
            Preference = UserDefaults.PlayerStatLungCapacity,
            Limit = PlayerStatsSettings.MaxLungCapacity,
            TextKey = Loc.PlayerOptions.StatLungCapacity,
            DescriptionKey = Loc.PlayerOptions.StatLungCapacityDescription,
        },
    ];

    public static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.MpStats);

    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        ClientConfig.AddEventListenerFor(Limits(), Apply);

        Apply();
    }

    public static int Chosen(MpStat stat) => Round(stat.Preference.Value);

    public static int LimitOf(MpStat stat) => Math.Clamp(ClientConfig.Value(stat.Limit), 0, Full);

    public static int Applied(MpStat stat) => Math.Min(Chosen(stat), LimitOf(stat));

    public static void SetChosen(MpStat stat, int percent)
    {
        if (!IsAllowed)
        {
            return;
        }

        stat.Preference.Value = Round(percent);

        Apply();
    }

    private static Setting[] Limits()
    {
        var settings = new Setting[All.Length];

        for (var index = 0; index < All.Length; index++)
        {
            settings[index] = All[index].Limit;
        }

        return settings;
    }

    private static int Round(int percent) => Math.Clamp(percent, 0, Full) / Step * Step;

    private static void Apply()
    {
        if (!IsAllowed)
        {
            return;
        }

        foreach (var stat in All)
        {
            Native.StatSetInt(API.Hash(stat.Stat), Applied(stat), false);
        }
    }
}
