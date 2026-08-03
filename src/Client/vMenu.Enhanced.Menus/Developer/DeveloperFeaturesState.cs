namespace vMenu.Enhanced.Menus.Developer;

/// <summary>
/// What the developer overlays are currently showing. Deliberately not persisted: these are debugging
/// aids, and having them survive a reconnect is more surprising than useful.
/// </summary>
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

    private static bool _showVehicleDimensions;

    private static bool _showPropDimensions;

    private static bool _showPedDimensions;

    private static bool _showEntityHandles;

    private static bool _showEntityModels;

    private static bool _showNetworkOwners;

    private static int _drawRadius = MaxDrawRadius;

    private static int _boxOpacity = MaxBoxOpacity;

    public static bool ShowVehicleDimensions
    {
        get => _showVehicleDimensions;
        set => Set(ref _showVehicleDimensions, value);
    }

    public static bool ShowPropDimensions
    {
        get => _showPropDimensions;
        set => Set(ref _showPropDimensions, value);
    }

    public static bool ShowPedDimensions
    {
        get => _showPedDimensions;
        set => Set(ref _showPedDimensions, value);
    }

    public static bool ShowEntityHandles
    {
        get => _showEntityHandles;
        set => Set(ref _showEntityHandles, value);
    }

    public static bool ShowEntityModels
    {
        get => _showEntityModels;
        set => Set(ref _showEntityModels, value);
    }

    public static bool ShowNetworkOwners
    {
        get => _showNetworkOwners;
        set => Set(ref _showNetworkOwners, value);
    }

    /// <summary>Slider position, not a distance. See <see cref="DrawRadiusMetres"/>.</summary>
    public static int DrawRadius
    {
        get => _drawRadius;
        set => Set(ref _drawRadius, value);
    }

    public static int DrawRadiusMetres => DrawRadius * MetresPerStep;

    /// <summary>Slider position, not a percentage. See <see cref="BoxOpacityPercent"/>.</summary>
    public static int BoxOpacity
    {
        get => _boxOpacity;
        set => Set(ref _boxOpacity, value);
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

    private static void Set(ref bool field, bool value)
    {
        if (field == value)
        {
            return;
        }

        field = value;

        Changed?.Invoke();
    }

    private static void Set(ref int field, int value)
    {
        if (field == value)
        {
            return;
        }

        field = value;

        Changed?.Invoke();
    }
}
