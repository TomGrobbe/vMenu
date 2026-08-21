using vMenu.Enhanced.Data.Configuration.Settings;

using JoinLeaveSettings = vMenu.Enhanced.Data.Configuration.Settings.JoinLeave;
using OnlinePlayerSettings = vMenu.Enhanced.Data.Configuration.Settings.OnlinePlayers;
using StaffAlertSettings = vMenu.Enhanced.Data.Configuration.Settings.StaffAlerts;
using UpdateSettings = vMenu.Enhanced.Data.Configuration.Settings.Updates;

namespace vMenu.Enhanced.Data.Configuration;

public sealed class ConfigSection(string title, IReadOnlyList<Setting> settings)
{
    public string Title { get; } = title;

    public IReadOnlyList<Setting> Settings { get; } = settings;
}

/// <summary>
/// Every setting vMenu knows about, in the order the generated example file lists them.
/// </summary>
/// <remarks>
/// An explicit list rather than attribute discovery, for the same reason
/// <c>MainMenuComposition</c> is one: grouping and order are a product decision that belongs in one
/// readable place, and scanning assemblies would be the wrong cost to pay in the client runtime, per
/// player, on script start.
/// </remarks>
public static class ConfigCatalog
{
    public static IReadOnlyList<ConfigSection> Sections { get; } =
    [
        new("Languages", [Localization.Languages]),
        new("About", [About.DocumentationUrl, About.DiscordUrl]),
        new("Updates", [UpdateSettings.CheckMode]),
        new("Key Bindings",
        [
            KeyBindings.MenuToggleKey,
            KeyBindings.NoClipToggleKey,
            KeyBindings.TeleportKey,
        ]),
        new("Menu Appearance",
        [
            MenuAppearance.TitleAlignment,
            MenuAppearance.TitleFont,
            MenuAppearance.HeaderGlare,
        ]),
        new("Gameplay", [Gameplay.PvpMode]),
        new("Staff Alerts",
        [
            StaffAlertSettings.Enabled,
            StaffAlertSettings.CooldownSeconds,
            StaffAlertSettings.ExpireSeconds,
            StaffAlertSettings.DisplaySeconds,
        ]),
        new("Online Players",
        [
            OnlinePlayerSettings.ActionLimit,
            OnlinePlayerSettings.ActionLimitSeconds,
        ]),
        new("Join and Leave", [JoinLeaveSettings.LogToConsole]),
        new("Player Stats",
        [
            PlayerStats.MaxShooting,
            PlayerStats.MaxStrength,
            PlayerStats.MaxStamina,
            PlayerStats.MaxStealth,
            PlayerStats.MaxFlying,
            PlayerStats.MaxDriving,
            PlayerStats.MaxLungCapacity,
        ]),
        new("Vehicle Options",
        [
            VehicleOptions.DeleteVehicleDistance,
            VehicleOptions.DeleteVehicleCommand,
            VehicleOptions.RepairVehicleCommand,
            VehicleOptions.WashVehicleCommand,
            VehicleOptions.ClearGodModeOnExit,
        ]),
        new("Personal Vehicle",
        [
            PersonalVehicle.ActionLimit,
            PersonalVehicle.ActionLimitSeconds,
            PersonalVehicle.ControlRange,
            PersonalVehicle.ControlTimeout,
        ]),
        new("Vehicle Spawner",
        [
            VehicleSpawner.OrphanMode,
            VehicleSpawner.KeepSpawnedVehiclesPersistent,
            VehicleSpawner.SpawnLimitSeconds,
            VehicleSpawner.SpawnLimitTier1,
            VehicleSpawner.SpawnLimitTier2,
            VehicleSpawner.SpawnLimitTier3,
        ]),
        new("Weather Options", [WeatherOptions.Enabled, WeatherOptions.SyncClouds, WeatherOptions.TransitionSeconds]),
        new("Time Options",
        [
            TimeOptions.Enabled,
            TimeOptions.SpeedMultiplier,
            TimeOptions.Presets,
            TimeOptions.TransitionSeconds,
        ]),
        new("Developer Features", [DeveloperFeatures.Enabled]),
        new("Debugging",
        [
            Debugging.LogLevel,
            Debugging.Client,
            Debugging.Server,
            Debugging.ExperimentalFeatures,
        ]),
    ];

    public static IEnumerable<Setting> All => Sections.SelectMany(static section => section.Settings);
}
