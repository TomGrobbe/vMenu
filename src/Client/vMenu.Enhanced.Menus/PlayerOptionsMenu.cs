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
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.GodMode),
            Description = MenuText.Key(Loc.PlayerOptions.GodModeDescription),
            Gate = PlayerOptionsPermissions.Godmode,
            ReadState = () => PlayerGodMode.Enabled,
            OnChanged = changed => PlayerGodMode.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.SuperJump),
            Description = MenuText.Key(Loc.PlayerOptions.SuperJumpDescription),
            Gate = PlayerOptionsPermissions.SuperJump,
            ReadState = () => PlayerSuperJump.Enabled,
            OnChanged = changed => PlayerSuperJump.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.FastRun),
            Description = MenuText.Key(Loc.PlayerOptions.FastRunDescription),
            Gate = PlayerOptionsPermissions.FastRun,
            ReadState = () => PlayerFastRun.Enabled,
            OnChanged = changed => PlayerFastRun.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.FastSwim),
            Description = MenuText.Key(Loc.PlayerOptions.FastSwimDescription),
            Gate = PlayerOptionsPermissions.FastSwim,
            ReadState = () => PlayerFastSwim.Enabled,
            OnChanged = changed => PlayerFastSwim.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.UnlimitedStamina),
            Description = MenuText.Key(Loc.PlayerOptions.UnlimitedStaminaDescription),
            Gate = PlayerOptionsPermissions.UnlimitedStamina,
            ReadState = () => PlayerUnlimitedStamina.Enabled,
            OnChanged = changed => PlayerUnlimitedStamina.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.UnlimitedOxygen),
            Description = MenuText.Key(Loc.PlayerOptions.UnlimitedOxygenDescription),
            Gate = PlayerOptionsPermissions.UnlimitedOxygen,
            ReadState = () => PlayerUnlimitedOxygen.Enabled,
            OnChanged = changed => PlayerUnlimitedOxygen.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.NoRagdoll),
            Description = MenuText.Key(Loc.PlayerOptions.NoRagdollDescription),
            Gate = PlayerOptionsPermissions.NoRagdoll,
            ReadState = () => PlayerNoRagdoll.Enabled,
            OnChanged = changed => PlayerNoRagdoll.SetEnabled(changed.Checked),
        });
    }
}
