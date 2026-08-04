using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Storage;

/// <summary>
/// Every preference vMenu remembers for the player.
/// </summary>
/// <remarks>
/// An explicit list rather than attribute discovery, for the same reason <c>ConfigCatalog</c> and
/// <c>MainMenuComposition</c> are. Only preferences with a feature behind them belong here; the list
/// grows as features land.
/// </remarks>
public static class UserDefaults
{
    private const string DumpCommand = "vmenu_defaults";

    private const string ResetCommand = "vmenu_defaults_reset";

    #region Misc Settings

    /// <summary>MenuAPI can refuse the alignment, so whoever applies this has to check that it took.</summary>
    public static BoolDefault MiscRightAlignMenu { get; } =
        new("miscRightAlignMenu") { Default = true };

    /// <summary>
    /// A <c>LanguageId</c> code rather than an index into the available languages, which would
    /// silently point at a different language the moment one is added. A plain string because the
    /// localizer sits above this assembly.
    /// </summary>
    public static StringDefault Language { get; } =
        new("language") { Default = "en" };

    #endregion

    #region Developer Features

    /// <remarks>
    /// Stored regardless of the <c>DeveloperFeatures.Enabled</c> convar. The overlay's tick condition
    /// already carries that gate, so a server turning the feature off makes these inert rather than
    /// erasing what the player had switched on.
    /// </remarks>
    public static BoolDefault DevVehicleDimensions { get; } = new("devVehicleDimensions") { Default = false };

    public static BoolDefault DevPropDimensions { get; } = new("devPropDimensions") { Default = false };

    public static BoolDefault DevPedDimensions { get; } = new("devPedDimensions") { Default = false };

    public static BoolDefault DevEntityHandles { get; } = new("devEntityHandles") { Default = false };

    public static BoolDefault DevEntityModels { get; } = new("devEntityModels") { Default = false };

    public static BoolDefault DevNetworkOwners { get; } = new("devNetworkOwners") { Default = false };

    /// <summary>
    /// Slider positions, not metres or percentages. The bounds live on
    /// <c>DeveloperFeaturesState</c>, which sits above this assembly, so these two defaults are its
    /// maxima written out — and the state clamps on read in case they ever disagree.
    /// </summary>
    public static IntDefault DevDrawRadius { get; } = new("devDrawRadius") { Default = 20 };

    /// <inheritdoc cref="DevDrawRadius"/>
    public static IntDefault DevBoxOpacity { get; } = new("devBoxOpacity") { Default = 10 };

    #endregion

    public static IReadOnlyList<UserDefault> All { get; } =
    [
        MiscRightAlignMenu,
        Language,

        DevVehicleDimensions,
        DevPropDimensions,
        DevPedDimensions,
        DevEntityHandles,
        DevEntityModels,
        DevNetworkOwners,
        DevDrawRadius,
        DevBoxOpacity,
    ];

    /// <summary>
    /// Call once, after <c>ClientJson.Verify</c>: every value here is a JSON envelope, so a client
    /// that cannot serialize cannot store anything either.
    /// </summary>
    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(DumpCommand, false, new Action(Dump));
        SharedAPI.Commands.RegisterCommand(ResetCommand, false, new Action(ResetAll));
    }

    public static void Dump()
    {
        API.Log.Info("[Defaults] Declared:");

        foreach (var preference in All)
        {
            API.Log.Info($"[Defaults]   {preference.Name} = {preference.CurrentText} (default {preference.DefaultText})");
        }

        API.Log.Info("[Defaults] Stored:");

        // Raw rather than reformatted: the reason a value carries its own key and type is so a dump
        // is useful when the code that writes it has gone wrong.
        foreach (var line in KvpStore.Describe(UserDefault.KeyPrefix))
        {
            API.Log.Info("[Defaults]   " + line);
        }
    }

    /// <summary>
    /// Forgets every declared preference, and anything left under the prefix that is no longer
    /// declared, so a preference dropped in a later version does not linger forever.
    /// </summary>
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
