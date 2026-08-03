using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Developer;

using DeveloperFeaturesSetting = vMenu.Enhanced.Data.Configuration.Settings.DeveloperFeatures;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Debugging overlays for people building content on the server.
/// </summary>
/// <remarks>
/// Gated by a convar rather than a permission: this is not something an owner grants to a person,
/// it is something they turn on for the server. Hidden rather than locked while it is off, so a
/// server that never wanted it does not advertise it.
/// <para>
/// The items here only write <see cref="DeveloperFeaturesState"/>. <see cref="DeveloperOverlay"/>
/// watches it and starts or stops its own ticks, so nothing here has to know a tick exists.
/// </para>
/// </remarks>
[VMenu(
    TitleKey = Loc.DeveloperFeatures.Title,
    SubtitleKey = Loc.DeveloperFeatures.Subtitle,
    DescriptionKey = Loc.DeveloperFeatures.LinkDescription)]
public sealed class DeveloperFeaturesMenu : MenuDefinition
{
    /// <summary>
    /// Held rather than written inline so the slider handler can re-resolve it. A slider cannot
    /// carry a label to put the value in: <c>MenuSliderItem.Draw</c> clears it every frame, because
    /// the bar occupies that side of the row.
    /// </summary>
    /// <remarks>
    /// A property, not a <see langword="static" /> <see langword="readonly" /> field: calling
    /// <see cref="MenuText.Resolve" /> on a field of that shape takes the address of an initonly
    /// field, which the FiveM client's IL verifier rejects at JIT time.
    /// </remarks>
    private static MenuText RadiusDescription => MenuText.Key(
        Loc.DeveloperFeatures.DrawRadiusDescription,
        ("radius", MenuText.From(() => $"{DeveloperFeaturesState.DrawRadiusMetres} m")));

    /// <inheritdoc cref="RadiusDescription"/>
    private static MenuText BoxOpacityDescription => MenuText.Key(
        Loc.DeveloperFeatures.BoxOpacityDescription,
        ("opacity", MenuText.From(() => $"{DeveloperFeaturesState.BoxOpacityPercent}%")));

    public override MenuGate Gate => MenuGate.Setting(DeveloperFeaturesSetting.Enabled);

    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DeveloperFeatures.VehicleDimensions),
            Description = MenuText.Key(Loc.DeveloperFeatures.VehicleDimensionsDescription),
            ReadState = () => DeveloperFeaturesState.ShowVehicleDimensions,
            OnChanged = changed => DeveloperFeaturesState.ShowVehicleDimensions = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DeveloperFeatures.PropDimensions),
            Description = MenuText.Key(Loc.DeveloperFeatures.PropDimensionsDescription),
            ReadState = () => DeveloperFeaturesState.ShowPropDimensions,
            OnChanged = changed => DeveloperFeaturesState.ShowPropDimensions = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DeveloperFeatures.PedDimensions),
            Description = MenuText.Key(Loc.DeveloperFeatures.PedDimensionsDescription),
            ReadState = () => DeveloperFeaturesState.ShowPedDimensions,
            OnChanged = changed => DeveloperFeaturesState.ShowPedDimensions = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DeveloperFeatures.EntityHandles),
            Description = MenuText.Key(Loc.DeveloperFeatures.EntityHandlesDescription),
            ReadState = () => DeveloperFeaturesState.ShowEntityHandles,
            OnChanged = changed => DeveloperFeaturesState.ShowEntityHandles = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DeveloperFeatures.EntityModels),
            Description = MenuText.Key(Loc.DeveloperFeatures.EntityModelsDescription),
            ReadState = () => DeveloperFeaturesState.ShowEntityModels,
            OnChanged = changed => DeveloperFeaturesState.ShowEntityModels = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DeveloperFeatures.NetworkOwners),
            Description = MenuText.Key(Loc.DeveloperFeatures.NetworkOwnersDescription),
            ReadState = () => DeveloperFeaturesState.ShowNetworkOwners,
            OnChanged = changed => DeveloperFeaturesState.ShowNetworkOwners = changed.Checked,
        });

        menu.Entries.Add(new SliderEntry
        {
            Text = MenuText.Key(Loc.DeveloperFeatures.DrawRadius),
            Description = RadiusDescription,
            Min = DeveloperFeaturesState.MinDrawRadius,
            Max = DeveloperFeaturesState.MaxDrawRadius,
            ReadPosition = () => DeveloperFeaturesState.DrawRadius,
            OnMoved = moved =>
            {
                DeveloperFeaturesState.DrawRadius = moved.NewPosition;

                // Moving a slider does not trigger a refresh pass, so the description would keep
                // showing the previous distance until something else caused one.
                moved.Item.Description = RadiusDescription.Resolve(Localizer.Current);
            },
        });

        menu.Entries.Add(new SliderEntry
        {
            Text = MenuText.Key(Loc.DeveloperFeatures.BoxOpacity),
            Description = BoxOpacityDescription,
            Min = DeveloperFeaturesState.MinBoxOpacity,
            Max = DeveloperFeaturesState.MaxBoxOpacity,
            ReadPosition = () => DeveloperFeaturesState.BoxOpacity,
            OnMoved = moved =>
            {
                DeveloperFeaturesState.BoxOpacity = moved.NewPosition;

                moved.Item.Description = BoxOpacityDescription.Resolve(Localizer.Current);
            },
        });
    }
}
