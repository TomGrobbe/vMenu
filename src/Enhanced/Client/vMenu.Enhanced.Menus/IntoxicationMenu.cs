using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.PlayerOptions.Intoxication,
    SubtitleKey = Loc.PlayerOptions.IntoxicationSubtitle,
    DescriptionKey = Loc.PlayerOptions.IntoxicationDescription,
    Permission = PlayerOptionsPermissions.Intoxication)]
public sealed class IntoxicationMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.IntoxicationEnabled),
            Description = MenuText.Key(Loc.PlayerOptions.IntoxicationEnabledDescription),
            ReadState = () => PlayerIntoxication.Enabled,
            OnChangedAsync = async changed =>
            {
                await PlayerIntoxication.SetEnabledAsync(changed.Checked);

                MenuRegistry.Refresh(menu.Menu);
            },
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.IntoxicationType),
            Description = MenuText.Key(Loc.PlayerOptions.IntoxicationTypeDescription),
            Options = [MenuText.Key(Loc.PlayerOptions.IntoxicationDrunk), MenuText.Key(Loc.PlayerOptions.IntoxicationDrugged)],
            ReadSelectedIndex = () => PlayerIntoxication.Drugged ? 1 : 0,
            ReadEnabled = () => PlayerIntoxication.Enabled,
            OnIndexChanged = changed => _ = PlayerIntoxication.SetDruggedAsync(changed.NewIndex == 1),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.IntoxicationScreen),
            Description = MenuText.Key(Loc.PlayerOptions.IntoxicationScreenDescription),
            ReadState = () => PlayerIntoxication.ScreenEffects,
            ReadEnabled = () => PlayerIntoxication.Enabled,
            OnChangedAsync = async changed =>
            {
                await PlayerIntoxication.SetScreenEffectsAsync(changed.Checked);

                MenuRegistry.Refresh(menu.Menu);
            },
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.IntoxicationStrength),
            Description = MenuText.Key(Loc.PlayerOptions.IntoxicationStrengthDescription),
            Options = Strengths(),
            ReadSelectedIndex = () => PlayerIntoxication.Strength - 1,
            ReadEnabled = () => PlayerIntoxication.Enabled && PlayerIntoxication.ScreenEffects,
            OnIndexChanged = changed => _ = PlayerIntoxication.SetStrengthAsync(changed.NewIndex + 1),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.IntoxicationWalk),
            Description = MenuText.Key(Loc.PlayerOptions.IntoxicationWalkDescription),
            ReadState = () => PlayerIntoxication.WalkingStyle,
            ReadEnabled = () => PlayerIntoxication.Enabled,
            OnChangedAsync = changed => PlayerIntoxication.SetWalkingStyleAsync(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.IntoxicationNoSprint),
            Description = MenuText.Key(Loc.PlayerOptions.IntoxicationNoSprintDescription),
            ReadState = () => PlayerIntoxication.PreventSprint,
            ReadEnabled = () => PlayerIntoxication.Enabled,
            OnChangedAsync = changed => PlayerIntoxication.SetPreventSprintAsync(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.IntoxicationDriving),
            Description = MenuText.Key(Loc.PlayerOptions.IntoxicationDrivingDescription),
            ReadState = () => PlayerIntoxication.Driving,
            ReadEnabled = () => PlayerIntoxication.Enabled,
            OnChangedAsync = changed => PlayerIntoxication.SetDrivingAsync(changed.Checked),
        });
    }

    private static MenuText[] Strengths()
    {
        var options = new MenuText[PlayerIntoxication.StrengthLevels];

        for (var level = 1; level <= options.Length; level++)
        {
            options[level - 1] = MenuText.Literal($"{level * 100 / PlayerIntoxication.StrengthLevels}%");
        }

        return options;
    }
}
