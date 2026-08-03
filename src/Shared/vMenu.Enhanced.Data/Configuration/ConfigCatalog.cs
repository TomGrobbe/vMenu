using vMenu.Enhanced.Data.Configuration.Settings;

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
        new("Vehicle Options", [VehicleOptions.DeleteVehicleDistance, VehicleOptions.DeleteVehicleCommand]),
        new("Developer Features", [DeveloperFeatures.Enabled]),
    ];

    public static IEnumerable<Setting> All => Sections.SelectMany(static section => section.Settings);
}
