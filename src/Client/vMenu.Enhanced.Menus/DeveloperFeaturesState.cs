namespace vMenu.Enhanced.Menus;

/// <summary>
/// What the developer overlays are currently showing. Deliberately not persisted: these are debugging
/// aids, and having them survive a reconnect is more surprising than useful.
/// </summary>
public static class DeveloperFeaturesState
{
    public const int MinDrawRadius = 0;

    public const int MaxDrawRadius = 20;

    public static bool ShowVehicleDimensions { get; set; }

    public static bool ShowPropDimensions { get; set; }

    public static bool ShowPedDimensions { get; set; }

    public static bool ShowEntityHandles { get; set; }

    public static bool ShowEntityModels { get; set; }

    public static bool ShowNetworkOwners { get; set; }

    public static int DrawRadius { get; set; } = MaxDrawRadius;
}
