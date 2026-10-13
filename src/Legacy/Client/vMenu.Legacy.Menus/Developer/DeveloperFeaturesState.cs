using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Developer;

// Backed by UserDefaults, so a setting survives a reconnect. Reads hit the KvpStore cache, and the
// overlay hoists them out of its per entity loops, so this stays off the frame budget.
public static class DeveloperFeaturesState
{
    public const int MinDrawRadius = 0;

    public const int MaxDrawRadius = 20;

    // Slider steps are coarse on purpose: a metre of extra reach is not worth a keypress.
    public const int MetresPerStep = 2;

    public const int MinBoxOpacity = 1;

    public const int MaxBoxOpacity = 10;

    // Ten points a step, so the slider runs from barely there to what it draws at now.
    public const int OpacityPercentPerStep = 10;

    // Enough to read the shape, not enough to hide the entity.
    public const int OpaqueBoxFillAlpha = 100;

    // Raised only when a value actually moves.
    public static event Action? Changed;

    // An import writes the stored values straight into the KvpStore, so none of the setters below ran
    // and the overlay is still drawing what was there before.
    public static void Reevaluate() => Changed?.Invoke();

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

    // Slider position, not a distance. Clamped, because a stored position falls outside the slider if
    // the bounds are narrowed later or the value is edited by hand.
    public static int DrawRadius
    {
        get => Math.Clamp(UserDefaults.DevDrawRadius.Value, MinDrawRadius, MaxDrawRadius);
        set => Set(UserDefaults.DevDrawRadius, Math.Clamp(value, MinDrawRadius, MaxDrawRadius));
    }

    public static int DrawRadiusMetres => DrawRadius * MetresPerStep;

    // Slider position, not a percentage. Clamped, for the same reason as DrawRadius.
    public static int BoxOpacity
    {
        get => Math.Clamp(UserDefaults.DevBoxOpacity.Value, MinBoxOpacity, MaxBoxOpacity);
        set => Set(UserDefaults.DevBoxOpacity, Math.Clamp(value, MinBoxOpacity, MaxBoxOpacity));
    }

    public static int BoxOpacityPercent => BoxOpacity * OpacityPercentPerStep;

    // What the shaded faces of a box draw at. Edges and labels keep their own opacity.
    public static int BoxFillAlpha => OpaqueBoxFillAlpha * BoxOpacityPercent / 100;

    // The handle, model and owner labels hang off an outline, so none of them counts on its own.
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
