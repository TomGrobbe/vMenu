using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using PedModelsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PedModels;
using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus;

/// <summary>Groups the player menus under one item on the main menu.</summary>
[VMenu(
    TitleKey = Loc.PlayerMenu.Title,
    SubtitleKey = Loc.PlayerMenu.Subtitle,
    DescriptionKey = Loc.PlayerMenu.LinkDescription)]
public sealed class PlayerMenu : MenuDefinition
{
    /// <summary>Open to anybody who can reach at least one of the menus inside it.</summary>
    public override MenuGate Gate { get; } =
        MenuGate.Permission(PlayerOptionsPermissions.Menu)
        | MenuGate.Permission(PedModelsPermissions.Menu);

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(SubmenuEntry.For(new PlayerOptionsMenu()));
        menu.Entries.Add(SubmenuEntry.For(new PedModelsMenu()));
    }
}
