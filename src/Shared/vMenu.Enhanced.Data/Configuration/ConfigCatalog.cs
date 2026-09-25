using vMenu.Enhanced.Data.Configuration.Settings;

using AdminSettings = vMenu.Enhanced.Data.Configuration.Settings.Admin;
using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;
using LoggingSettings = vMenu.Enhanced.Data.Configuration.Settings.Logging;
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

// An explicit list, not attribute discovery: order and grouping are a product decision, and scanning
// assemblies would cost too much in the client runtime, per player, on script start.
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
            MenuAppearance.Skin,
            MenuAppearance.TitleAlignment,
            MenuAppearance.HeaderGlare,
        ]),
        new("Gameplay", [Gameplay.PvpMode]),
        new("Admin",
        [
            AdminSettings.ClearAreaRadius,
            AdminSettings.ClosestPlayerRange,
            AdminSettings.ScheduledAnnouncements,
            AdminSettings.AnnouncementSeconds,
        ]),
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
            OnlinePlayerSettings.MatchRoutingBucket,
        ]),
        new("Join and Leave", [JoinLeaveSettings.LogToConsole]),
        new("Webhook Logging",
        [
            LoggingSettings.Enabled,
            LoggingSettings.EventsWebhook,
            LoggingSettings.ActionsWebhook,
            LoggingSettings.StaffWebhook,
            LoggingSettings.SecurityWebhook,
            LoggingSettings.GenericWebhook,
            LoggingSettings.FlushSeconds,
            LoggingSettings.QueueLimit,
            LoggingSettings.MenuActionLimit,
            LoggingSettings.MenuActionLimitSeconds,
            LoggingSettings.SecurityLimit,
            LoggingSettings.SecurityLimitSeconds,
        ]),
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
        new("World API", [WorldApi.Token]),
        new("Integration",
        [
            IntegrationSettings.ApiKey,
            IntegrationSettings.Endpoint,
            IntegrationSettings.AllowActions,
            IntegrationSettings.Allowlist,
            IntegrationSettings.AllowlistMessage,
            IntegrationSettings.Queue,
        ]),
        new("Developer Features", [DeveloperFeatures.Enabled]),
        new("Debugging",
        [
            Debugging.Client,
            Debugging.Server,
            Debugging.ExperimentalFeatures,
        ]),
    ];

    public static IEnumerable<Setting> All => Sections.SelectMany(static section => section.Settings);
}
