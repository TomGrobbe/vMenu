using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Developer;
using vMenu.Enhanced.Ticks;

using DeveloperFeaturesSetting = vMenu.Enhanced.Data.Configuration.Settings.DeveloperFeatures;

namespace vMenu.Enhanced.Menus;

/// <summary>Debugging overlays for people building content on the server.</summary>
// Gated by a convar, not a permission: an owner turns this on for the server rather than granting it
// to a person. Hidden rather than locked while off, so a server that never wanted it does not
// advertise it. The items only write DeveloperFeaturesState, which DeveloperOverlay watches.
[VMenu(
    TitleKey = Loc.DeveloperFeatures.Title,
    SubtitleKey = Loc.DeveloperFeatures.Subtitle,
    DescriptionKey = Loc.DeveloperFeatures.LinkDescription)]
public sealed class DeveloperFeaturesMenu : MenuDefinition
{
    // A slider cannot carry a label for the value, MenuSliderItem.Draw clearing it every frame. A
    // property, not a static readonly field: resolving a field of that shape takes the address of an
    // initonly field, which the client's IL verifier rejects at JIT time.
    private static MenuText RadiusDescription => MenuText.Key(
        Loc.DeveloperFeatures.DrawRadiusDescription,
        ("radius", MenuText.From(() => $"{DeveloperFeaturesState.DrawRadiusMetres} m")));

    // See RadiusDescription.
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

                // Moving a slider triggers no refresh pass, so this would show a stale distance.
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

        // button on purpose, don't want a checkbox cause that'll be confusing
        // since this one doesn't have a user defaults that stays on/off, it's
        // always off by default.
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.DeveloperFeatures.TicksOverlay),
            Description = MenuText.Key(Loc.DeveloperFeatures.TicksOverlayDescription),
            OnSelected = _ => TickOverlay.Toggle(),
        });
    }
}
