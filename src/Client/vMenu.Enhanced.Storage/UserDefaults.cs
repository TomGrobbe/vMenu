using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;

namespace vMenu.Enhanced.Storage;

/// <summary>Every preference vMenu remembers for the player.</summary>
public static class UserDefaults
{
    private const string DumpCommand = "vmenu_defaults";

    private const string ResetCommand = "vmenu_defaults_reset";

    #region Misc Settings

    public static BoolDefault MiscRightAlignMenu { get; } =
        new("miscRightAlignMenu") { Default = true };

    /// <summary>A <c>LanguageId</c> code.</summary>
    // A code rather than an index, which would point at a different language once one is added. A
    // plain string because the localizer sits above this assembly.
    public static StringDefault Language { get; } =
        new("language") { Default = "en" };

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

    public static IReadOnlyList<UserDefault> All { get; } =
    [
        MiscRightAlignMenu,
        Language,

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
    ];

    /// <summary>Call once, after <c>ClientJson.Verify</c>.</summary>
    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(DumpCommand, false, DebugCommands.Gate(Dump));
        SharedAPI.Commands.RegisterCommand(ResetCommand, false, DebugCommands.Gate(ResetAll));
    }

    public static void Dump()
    {
        API.Log.Info("[Defaults] Declared:");

        foreach (var preference in All)
        {
            API.Log.Info($"[Defaults]   {preference.Name} = {preference.CurrentText} (default {preference.DefaultText})");
        }

        API.Log.Info("[Defaults] Stored:");

        foreach (var line in KvpStore.Describe(UserDefault.KeyPrefix))
        {
            API.Log.Info("[Defaults]   " + line);
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

        API.Log.Info("[Defaults] Every stored preference has been reset.");
    }
}
