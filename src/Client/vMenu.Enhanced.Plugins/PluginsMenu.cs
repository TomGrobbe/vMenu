using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Plugins;

/// <summary>
/// The main menu row every plugin lives under. Hidden while nothing is registered, so a server
/// without plugins never advertises the feature. Its rows are rebuilt by the host whenever a
/// plugin registers or goes away.
/// </summary>
[VMenu(
    TitleKey = Loc.Plugins.Title,
    SubtitleKey = Loc.Plugins.Subtitle,
    DescriptionKey = Loc.Plugins.LinkDescription)]
public sealed class PluginsMenu : MenuDefinition
{
    public override MenuGate Gate => MenuGate.When(static () => PluginHost.Count > 0);

    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    protected override void Build(MenuBuilder menu) => PluginHost.AttachPluginsMenu(menu);
}
