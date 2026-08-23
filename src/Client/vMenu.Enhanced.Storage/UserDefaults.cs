using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Storage;

/// <summary>Every preference vMenu remembers for the player.</summary>
public static class UserDefaults
{
    private const string DumpCommand = "vmenu_defaults";

    private const string ResetCommand = "vmenu_defaults_reset";

    #region Misc Settings

    public static BoolDefault MiscRightAlignMenu { get; } =
        new("miscRightAlignMenu") { Default = true };

    public static StringDefault Language { get; } =
        new("language") { Default = "en" };

    public static BoolDefault MiscDisableIdleCamera { get; } =
        new("miscDisableIdleCamera") { Default = false };

    public static BoolDefault MiscDisableVehicleIdleCamera { get; } =
        new("miscDisableVehicleIdleCamera") { Default = false };

    public static BoolDefault MiscDeathNotifications { get; } =
        new("miscDeathNotifications") { Default = true };

    public static BoolDefault MiscJoinLeaveNotifications { get; } =
        new("miscJoinLeaveNotifications") { Default = true };

    public static BoolDefault MiscFingerPointing { get; } =
        new("miscFingerPointing") { Default = true };

    public static BoolDefault MiscShowPlayerBlips { get; } =
        new("miscShowPlayerBlips") { Default = false };

    public static BoolDefault MiscShowOverheadNames { get; } =
        new("miscShowOverheadNames") { Default = false };

    public static BoolDefault MiscSeeNoClipPlayers { get; } =
        new("miscSeeNoClipPlayers") { Default = true };

    public static BoolDefault MiscHideStaffAlerts { get; } =
        new("miscHideStaffAlerts") { Default = false };

    #endregion

    #region Display Settings

    public static IntDefault DisplayMinimapAction { get; } =
        new("displayMinimapAction") { Default = 0 };

    public static IntDefault DisplayMinimapZoom { get; } =
        new("displayMinimapZoom") { Default = 5 };

    public static BoolDefault DisplayMinimapAlwaysOn { get; } =
        new("displayMinimapAlwaysOn") { Default = false };

    public static IntDefault DisplaySpeedometer { get; } =
        new("displaySpeedometer") { Default = 0 };

    public static IntDefault DisplaySpeedometerPosition { get; } =
        new("displaySpeedometerPosition") { Default = 0 };

    public static BoolDefault DisplayShowLocation { get; } =
        new("displayShowLocation") { Default = false };

    public static BoolDefault DisplayShowCoordinates { get; } =
        new("displayShowCoordinates") { Default = false };

    public static BoolDefault DisplayWeatherForecast { get; } =
        new("displayWeatherForecast") { Default = false };

    public static BoolDefault DisplayShowTime { get; } =
        new("displayShowTime") { Default = false };

    public static BoolDefault DisplayLocationBlips { get; } =
        new("displayLocationBlips") { Default = false };

    public static IntDefault DisplayWeatherForecastStyle { get; } =
        new("displayWeatherForecastStyle") { Default = 0 };

    #endregion

    #region Player Options
    // Stored whether or not the server grants the matching permission, for the reason on UserDefault:
    // a player who set this on one server still has their choice on the next. Whoever applies it
    // checks the permission.

    public static BoolDefault PlayerGodMode { get; } = new("playerGodMode") { Default = false };

    public static BoolDefault PlayerSuperJump { get; } = new("playerSuperJump") { Default = false };

    public static BoolDefault PlayerFastRun { get; } = new("playerFastRun") { Default = false };

    public static BoolDefault PlayerFastSwim { get; } = new("playerFastSwim") { Default = false };

    public static IntDefault PlayerStatShooting { get; } = new("playerStatShooting") { Default = 100 };

    public static IntDefault PlayerStatStrength { get; } = new("playerStatStrength") { Default = 100 };

    public static IntDefault PlayerStatStamina { get; } = new("playerStatStamina") { Default = 100 };

    public static IntDefault PlayerStatStealth { get; } = new("playerStatStealth") { Default = 100 };

    public static IntDefault PlayerStatFlying { get; } = new("playerStatFlying") { Default = 100 };

    public static IntDefault PlayerStatDriving { get; } = new("playerStatDriving") { Default = 100 };

    public static IntDefault PlayerStatLungCapacity { get; } = new("playerStatLungCapacity") { Default = 100 };

    public static BoolDefault PlayerUnlimitedOxygen { get; } = new("playerUnlimitedOxygen") { Default = false };

    public static BoolDefault PlayerNoRagdoll { get; } = new("playerNoRagdoll") { Default = false };

    public static BoolDefault PlayerNoHelmet { get; } = new("playerNoHelmet") { Default = false };

    public static BoolDefault PlayerInvisible { get; } = new("playerInvisible") { Default = false };

    public static BoolDefault PlayerStayInVehicle { get; } = new("playerStayInVehicle") { Default = false };

    public static BoolDefault PlayerEveryoneIgnores { get; } = new("playerEveryoneIgnores") { Default = false };

    public static BoolDefault PlayerNeverWanted { get; } = new("playerNeverWanted") { Default = false };

    /// <summary>The movement clip set the player picked, or empty for the ped's own walk.</summary>
    // Stored rather than read back off the ped, because the game offers no way to ask which clip set
    // a ped is using. It is also why this survives a model change: nothing else remembers.
    public static StringDefault PlayerWalkingStyle { get; } =
        new("playerWalkingStyle") { Default = string.Empty };

    /// <summary>How the player's glowing clothes behave: 0 off, 1 solid, 2 fade, 3 flash.</summary>
    public static IntDefault PlayerClothingGlow { get; } = new("playerClothingGlow") { Default = 0 };

    #endregion

    #region Vehicle Options

    public static BoolDefault VehicleGodMode { get; } = new("vehicleGodMode") { Default = false };

    // The seven below answer to the master toggle above, which is why six of them start on: switching
    // god mode on and getting nothing would read as broken.
    public static BoolDefault VehicleGodInvincible { get; } = new("vehicleGodInvincible") { Default = true };

    public static BoolDefault VehicleGodEngine { get; } = new("vehicleGodEngine") { Default = true };

    public static BoolDefault VehicleGodVisual { get; } = new("vehicleGodVisual") { Default = true };

    public static BoolDefault VehicleGodStrongWheels { get; } = new("vehicleGodStrongWheels") { Default = true };

    public static BoolDefault VehicleGodBulletproofTyres { get; } = new("vehicleGodBulletproofTyres") { Default = true };

    public static BoolDefault VehicleGodRamp { get; } = new("vehicleGodRamp") { Default = true };

    /// <summary>The exception: it fixes the car out from under the player, so it is opt in.</summary>
    public static BoolDefault VehicleGodAutoRepair { get; } = new("vehicleGodAutoRepair") { Default = false };

    public static BoolDefault VehicleKeepClean { get; } = new("vehicleKeepClean") { Default = false };

    public static BoolDefault PersonalVehicleBlip { get; } = new("personalVehicleBlip") { Default = true };

    #endregion

    #region Vehicle Spawner

    public static BoolDefault VehicleSpawnerSpawnInside { get; } =
        new("vehicleSpawnerSpawnInside") { Default = true };

    public static BoolDefault VehicleSpawnerReplacePrevious { get; } =
        new("vehicleSpawnerReplacePrevious") { Default = true };

    #endregion

    #region World

    #endregion

    #region Teleport

    /// <summary>What the teleport key does: 0 nothing, 1 to the waypoint, 2 to typed coordinates.</summary>
    // Stored whether or not the server grants the matching permission, so a player who sets this on
    // one server still has their choice on the next. The key checks the permission when it is pressed.
    public static IntDefault TeleportKeyAction { get; } = new("teleportKeyAction") { Default = 0 };

    #endregion

    #region Developer Features

    // Stored regardless of the DeveloperFeatures.Enabled convar. The overlay's tick condition
    // carries that gate, so a server turning the feature off makes these inert rather than erasing
    // what the player had switched on.
    public static BoolDefault DevVehicleDimensions { get; } = new("devVehicleDimensions") { Default = false };

    public static BoolDefault DevPropDimensions { get; } = new("devPropDimensions") { Default = false };

    public static BoolDefault DevPedDimensions { get; } = new("devPedDimensions") { Default = false };

    public static BoolDefault DevEntityHandles { get; } = new("devEntityHandles") { Default = false };

    public static BoolDefault DevEntityModels { get; } = new("devEntityModels") { Default = false };

    public static BoolDefault DevNetworkOwners { get; } = new("devNetworkOwners") { Default = false };

    /// <summary>Slider positions, not metres or percentages.</summary>
    // The bounds live on DeveloperFeaturesState, above this assembly, so these are its maxima
    // written out. That state clamps on read in case they ever disagree.
    public static IntDefault DevDrawRadius { get; } = new("devDrawRadius") { Default = 20 };

    /// <inheritdoc cref="DevDrawRadius"/>
    public static IntDefault DevBoxOpacity { get; } = new("devBoxOpacity") { Default = 10 };

    #endregion

    #region Ticks Overlay

    /// <summary>Deliberately outside the Developer Features region, being gated by neither its convar nor a permission.</summary>
    // The panel only names vMenu's own loops, and its toggle command is open to everyone, so a player
    // who left it on gets it back wherever they play and can always switch it off again.
    public static BoolDefault TicksOverlay { get; } = new("ticksOverlay") { Default = false };

    #endregion

    #region Pointing Debug

    /// <summary>Outside the Developer Features region for the same reason <see cref="TicksOverlay"/> is.</summary>
    // It draws where pointing thinks the player's shoulder is and whether the arm is blocked. Its
    // toggle command is open to everyone, so a player who left it on gets it back wherever they play
    // and can always switch it off again.
    public static BoolDefault PointingDebug { get; } = new("pointingDebug") { Default = false };

    #endregion

    #region Weapons

    public static BoolDefault WeaponsUnlimitedAmmo { get; } = new("weaponsUnlimitedAmmo") { Default = false };

    public static BoolDefault WeaponsNoReload { get; } = new("weaponsNoReload") { Default = false };

    public static BoolDefault WeaponsAutoEquipParachute { get; } = new("weaponsAutoEquipParachute") { Default = false };

    public static BoolDefault WeaponsUnlimitedParachutes { get; } = new("weaponsUnlimitedParachutes") { Default = false };

    public static BoolDefault WeaponLoadoutOnRespawn { get; } = new("weaponLoadoutOnRespawn") { Default = false };

    /// <summary>Whether the weapons already held are carried through a change of ped.</summary>
    // The only preference here that starts on. Changing ped has always kept the player's weapons, so
    // defaulting this off would take something away from everybody who upgrades.
    public static BoolDefault WeaponsKeepOnPedChange { get; } = new("weaponsKeepOnPedChange") { Default = true };

    /// <summary>The name of the loadout to hand back on respawn, or empty for none.</summary>
    // A name rather than the loadout itself, so renaming or replacing one keeps the choice pointing
    // at the right thing.
    public static StringDefault WeaponLoadoutDefaultName { get; } =
        new("weaponLoadoutDefaultName") { Default = string.Empty };

    #endregion

    #region Character Creator

    public static StringDefault DefaultCharacterName { get; } =
        new("defaultCharacterName") { Default = string.Empty };

    public static BoolDefault CharacterCreatorFitTorso { get; } =
        new("characterCreatorFitTorso") { Default = true };

    public static BoolDefault CharacterCreatorDisableAutoCamera { get; } =
        new("characterCreatorDisableAutoCamera") { Default = false };

    #endregion

    public static IReadOnlyList<UserDefault> All { get; } =
    [
        DefaultCharacterName,
        CharacterCreatorFitTorso,
        CharacterCreatorDisableAutoCamera,
        MiscRightAlignMenu,
        Language,
        MiscDisableIdleCamera,
        MiscDisableVehicleIdleCamera,
        MiscDeathNotifications,
        MiscJoinLeaveNotifications,
        DisplayMinimapAction,
        DisplayMinimapZoom,
        DisplayMinimapAlwaysOn,
        MiscFingerPointing,
        MiscShowPlayerBlips,
        MiscShowOverheadNames,
        MiscSeeNoClipPlayers,
        MiscHideStaffAlerts,
        DisplaySpeedometer,
        DisplaySpeedometerPosition,
        DisplayShowLocation,
        DisplayShowCoordinates,

        PlayerGodMode,
        PlayerSuperJump,
        PlayerFastRun,
        PlayerFastSwim,
        PlayerStatShooting,
        PlayerStatStrength,
        PlayerStatStamina,
        PlayerStatStealth,
        PlayerStatFlying,
        PlayerStatDriving,
        PlayerStatLungCapacity,
        PlayerUnlimitedOxygen,
        PlayerNoRagdoll,
        PlayerNoHelmet,
        PlayerInvisible,
        PlayerStayInVehicle,
        PlayerEveryoneIgnores,
        PlayerNeverWanted,
        PlayerWalkingStyle,
        PlayerClothingGlow,

        VehicleGodMode,
        VehicleGodInvincible,
        VehicleGodEngine,
        VehicleGodVisual,
        VehicleGodStrongWheels,
        VehicleGodBulletproofTyres,
        VehicleGodRamp,
        VehicleGodAutoRepair,
        VehicleKeepClean,
        PersonalVehicleBlip,

        VehicleSpawnerSpawnInside,
        VehicleSpawnerReplacePrevious,

        DisplayWeatherForecast,
        DisplayWeatherForecastStyle,
        DisplayShowTime,
        DisplayLocationBlips,

        WeaponsUnlimitedAmmo,
        WeaponsNoReload,
        WeaponsAutoEquipParachute,
        WeaponsUnlimitedParachutes,
        WeaponLoadoutOnRespawn,
        WeaponLoadoutDefaultName,
        WeaponsKeepOnPedChange,

        TeleportKeyAction,

        DevVehicleDimensions,
        DevPropDimensions,
        DevPedDimensions,
        DevEntityHandles,
        DevEntityModels,
        DevNetworkOwners,
        DevDrawRadius,
        DevBoxOpacity,

        TicksOverlay,

        PointingDebug,
    ];

    /// <summary>Call once, after <c>ClientJson.Verify</c>.</summary>
    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(DumpCommand, false, DebugCommands.Gate(Dump));
        SharedAPI.Commands.RegisterCommand(ResetCommand, false, DebugCommands.Gate(ResetAll));
    }

    public static void Dump()
    {
        Log.Info("[Defaults] Declared:");

        foreach (var preference in All)
        {
            Log.Info($"[Defaults]   {preference.Name} = {preference.CurrentText} (default {preference.DefaultText})");
        }

        Log.Info("[Defaults] Stored:");

        foreach (var line in KvpStore.Describe(UserDefault.KeyPrefix))
        {
            Log.Info("[Defaults]   " + line);
        }
    }

    /// <summary>Forgets every declared preference, and anything under the prefix no longer declared.</summary>
    public static void ResetAll()
    {
        foreach (var preference in All)
        {
            preference.Reset();
        }

        foreach (var key in KvpStore.Keys(UserDefault.KeyPrefix))
        {
            KvpStore.Delete(key);
        }

        Log.Info("[Defaults] Every stored preference has been reset.");
    }
}
