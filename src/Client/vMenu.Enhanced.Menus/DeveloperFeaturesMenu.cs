using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using DeveloperFeaturesSetting = vMenu.Enhanced.Data.Configuration.Settings.DeveloperFeatures;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Debugging overlays for people building content on the server.
/// </summary>
/// <remarks>
/// Gated by a convar rather than a permission: this is not something an owner grants to a person,
/// it is something they turn on for the server. Hidden rather than locked while it is off, so a
/// server that never wanted it does not advertise it.
/// <para>Every item here is a placeholder. The state is tracked; nothing draws it yet.</para>
/// </remarks>
[VMenu(
    TitleKey = Loc.DeveloperFeatures.Title,
    SubtitleKey = Loc.DeveloperFeatures.Subtitle,
    DescriptionKey = Loc.DeveloperFeatures.LinkDescription)]
public sealed class DeveloperFeaturesMenu : MenuDefinition
{
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
            Description = MenuText.Key(Loc.DeveloperFeatures.DrawRadiusDescription),
            Min = DeveloperFeaturesState.MinDrawRadius,
            Max = DeveloperFeaturesState.MaxDrawRadius,
            ReadPosition = () => DeveloperFeaturesState.DrawRadius,
            OnMoved = moved => DeveloperFeaturesState.DrawRadius = moved.NewPosition,
        });
    }
}
