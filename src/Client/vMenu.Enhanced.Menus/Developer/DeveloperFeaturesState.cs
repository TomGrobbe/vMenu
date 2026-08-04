using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Developer;

/// <summary>
/// What the developer overlays are currently showing.
/// </summary>
/// <remarks>
/// Backed by <see cref="UserDefaults"/>, so a setting survives a reconnect. Reads go through
/// <c>KvpStore</c>'s cache rather than the store itself; the overlay hoists them out of its per
/// entity loops, so this stays off the frame budget.
/// </remarks>
public static class DeveloperFeaturesState
{
    public const int MinDrawRadius = 0;

    public const int MaxDrawRadius = 20;

    /// <summary>Slider steps are coarse on purpose: a metre of extra reach is not worth a keypress.</summary>
    public const int MetresPerStep = 2;

    public const int MinBoxOpacity = 1;

    public const int MaxBoxOpacity = 10;

    /// <summary>Ten points a step, so the slider runs from barely there to what it draws at now.</summary>
    public const int OpacityPercentPerStep = 10;

    /// <summary>
    /// Legacy's face opacity: enough to read the shape, not enough to hide the entity. The top of
    /// the opacity slider, never exceeded.
    /// </summary>
    public const int OpaqueBoxFillAlpha = 100;

    /// <summary>
    /// Raised only when a value actually moves, so the overlay can re-evaluate its ticks without
    /// every caller having to remember to.
    /// </summary>
    public static event Action? Changed;

    public static bool ShowVehicleDimensions
    {
        get => UserDefaults.DevVehicleDimensions.Value;
        set => Set(UserDefaults.DevVehicleDimensions, value);
    }

    public static bool ShowPropDimensions
    {
        get => UserDefaults.DevPropDimensions.Value;
        set => Set(UserDefaults.DevPropDimensions, value);
    }

    public static bool ShowPedDimensions
    {
        get => UserDefaults.DevPedDimensions.Value;
        set => Set(UserDefaults.DevPedDimensions, value);
    }

    public static bool ShowEntityHandles
    {
        get => UserDefaults.DevEntityHandles.Value;
        set => Set(UserDefaults.DevEntityHandles, value);
    }

    public static bool ShowEntityModels
    {
        get => UserDefaults.DevEntityModels.Value;
        set => Set(UserDefaults.DevEntityModels, value);
    }

    public static bool ShowNetworkOwners
    {
        get => UserDefaults.DevNetworkOwners.Value;
        set => Set(UserDefaults.DevNetworkOwners, value);
    }

    /// <summary>Slider position, not a distance. See <see cref="DrawRadiusMetres"/>.</summary>
    /// <remarks>
    /// Clamped on read. A stored position can fall outside the slider if the bounds above are
    /// narrowed in a later version, or if the value is edited by hand — either would hand MenuAPI a
    /// position its slider has no room for.
    /// </remarks>
    public static int DrawRadius
    {
        get => Math.Clamp(UserDefaults.DevDrawRadius.Value, MinDrawRadius, MaxDrawRadius);
        set => Set(UserDefaults.DevDrawRadius, Math.Clamp(value, MinDrawRadius, MaxDrawRadius));
    }

    public static int DrawRadiusMetres => DrawRadius * MetresPerStep;

    /// <summary>Slider position, not a percentage. See <see cref="BoxOpacityPercent"/>.</summary>
    /// <inheritdoc cref="DrawRadius" path="/remarks"/>
    public static int BoxOpacity
    {
        get => Math.Clamp(UserDefaults.DevBoxOpacity.Value, MinBoxOpacity, MaxBoxOpacity);
        set => Set(UserDefaults.DevBoxOpacity, Math.Clamp(value, MinBoxOpacity, MaxBoxOpacity));
    }

    public static int BoxOpacityPercent => BoxOpacity * OpacityPercentPerStep;

    /// <summary>
    /// What the shaded faces of a box draw at. The edges around them and the labels beside them keep
    /// their own opacity: at the bottom of the slider the shape is still outlined, just barely filled.
    /// </summary>
    public static int BoxFillAlpha => OpaqueBoxFillAlpha * BoxOpacityPercent / 100;

    /// <summary>
    /// Whether anything can be drawn at all. The handle, model and owner labels hang off an outline,
    /// so none of them counts on its own.
    /// </summary>
    public static bool AnyOutlineEnabled =>
        ShowVehicleDimensions || ShowPropDimensions || ShowPedDimensions;

    private static void Set(BoolDefault preference, bool value)
    {
        if (preference.Value == value)
        {
            return;
        }

        preference.Value = value;

        Changed?.Invoke();
    }

    private static void Set(IntDefault preference, int value)
    {
        if (preference.Value == value)
        {
            return;
        }

        preference.Value = value;

        Changed?.Invoke();
    }
}
