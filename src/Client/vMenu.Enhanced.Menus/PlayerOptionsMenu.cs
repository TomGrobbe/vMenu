using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.PlayerOptions.Title,
    SubtitleKey = Loc.PlayerOptions.Subtitle,
    DescriptionKey = Loc.PlayerOptions.LinkDescription,
    Permission = PlayerOptionsPermissions.Menu)]
public sealed class PlayerOptionsMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu) =>
        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.GodMode),
            Description = MenuText.Key(Loc.PlayerOptions.GodModeDescription),
            Gate = PlayerOptionsPermissions.Godmode,
            ReadState = () => PlayerGodMode.Enabled,
            OnChanged = changed => PlayerGodMode.SetEnabled(changed.Checked),
        });
}
