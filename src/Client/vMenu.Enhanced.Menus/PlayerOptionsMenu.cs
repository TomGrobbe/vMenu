using vMenu.Enhanced.Data.PedModels;
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
        menu.Entries.Add(Group(Loc.PlayerOptions.GroupProtection, Loc.PlayerOptions.GroupProtectionDescription));

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
            Text = MenuText.Key(Loc.PlayerOptions.Invisible),
            Description = MenuText.Key(Loc.PlayerOptions.InvisibleDescription),
            Gate = PlayerOptionsPermissions.Invisible,
            ReadState = () => PlayerInvisible.Enabled,
            OnChanged = changed => PlayerInvisible.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.NoRagdoll),
            Description = MenuText.Key(Loc.PlayerOptions.NoRagdollDescription),
            Gate = PlayerOptionsPermissions.NoRagdoll,
            ReadState = () => PlayerNoRagdoll.Enabled,
            OnChanged = changed => PlayerNoRagdoll.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.StayInVehicle),
            Description = MenuText.Key(Loc.PlayerOptions.StayInVehicleDescription),
            Gate = PlayerOptionsPermissions.StayInVehicle,
            ReadState = () => PlayerStayInVehicle.Enabled,
            OnChanged = changed => PlayerStayInVehicle.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.EveryoneIgnores),
            Description = MenuText.Key(Loc.PlayerOptions.EveryoneIgnoresDescription),
            Gate = PlayerOptionsPermissions.Ignored,
            ReadState = () => EveryoneIgnoresPlayer.Enabled,
            OnChanged = changed => EveryoneIgnoresPlayer.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(Group(Loc.PlayerOptions.GroupMovement, Loc.PlayerOptions.GroupMovementDescription));

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
            Text = MenuText.Key(Loc.PlayerOptions.UnlimitedOxygen),
            Description = MenuText.Key(Loc.PlayerOptions.UnlimitedOxygenDescription),
            Gate = PlayerOptionsPermissions.UnlimitedOxygen,
            ReadState = () => PlayerUnlimitedOxygen.Enabled,
            OnChanged = changed => PlayerUnlimitedOxygen.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(Group(Loc.PlayerOptions.GroupWanted, Loc.PlayerOptions.GroupWantedDescription));

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.NeverWanted),
            Description = MenuText.Key(Loc.PlayerOptions.NeverWantedDescription),
            Gate = PlayerOptionsPermissions.NeverWanted,
            ReadState = () => PlayerNeverWanted.Enabled,
            OnChanged = changed => PlayerNeverWanted.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.SetWanted),
            Description = MenuText.Key(Loc.PlayerOptions.SetWantedDescription),
            Gate = PlayerOptionsPermissions.SetWanted,
            Options = WantedLevels(),
            ReadSelectedIndex = PlayerActions.WantedLevel,
            OnSelected = selected => PlayerActions.SetWantedLevel(selected.SelectedIndex),
        });

        menu.Entries.Add(Group(Loc.PlayerOptions.GroupHealth, Loc.PlayerOptions.GroupHealthDescription));

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.HealPlayer),
            Description = MenuText.Key(Loc.PlayerOptions.HealPlayerDescription),
            Gate = PlayerOptionsPermissions.MaxHealth,
            OnSelected = _ => PlayerActions.Heal(),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.SetArmor),
            Description = MenuText.Key(Loc.PlayerOptions.SetArmorDescription),
            Gate = PlayerOptionsPermissions.MaxArmor,
            Options = ArmorTiers(),
            ReadSelectedIndex = PlayerActions.ArmorTier,
            OnSelected = selected => PlayerActions.SetArmorTier(selected.SelectedIndex),
        });

        menu.Entries.Add(Group(Loc.PlayerOptions.GroupAppearance, Loc.PlayerOptions.GroupAppearanceDescription));

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.ClearBlood),
            Description = MenuText.Key(Loc.PlayerOptions.ClearBloodDescription),
            Gate = PlayerOptionsPermissions.ClearBlood,
            OnSelected = _ => PlayerActions.ClearBlood(),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.SetBlood),
            Description = MenuText.Key(Loc.PlayerOptions.SetBloodDescription),
            Gate = PlayerOptionsPermissions.SetBlood,
            Options = DamagePacks(),
            OnSelected = selected => PlayerActions.ApplyDamagePack(selected.SelectedIndex),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.CleanPlayer),
            Description = MenuText.Key(Loc.PlayerOptions.CleanPlayerDescription),
            Gate = PlayerOptionsPermissions.CleanPlayer,
            OnSelected = _ => PlayerActions.CleanClothes(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.DryPlayer),
            Description = MenuText.Key(Loc.PlayerOptions.DryPlayerDescription),
            Gate = PlayerOptionsPermissions.DryPlayer,
            OnSelected = _ => PlayerActions.DryClothes(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.WetPlayer),
            Description = MenuText.Key(Loc.PlayerOptions.WetPlayerDescription),
            Gate = PlayerOptionsPermissions.WetPlayer,
            OnSelected = _ => PlayerActions.WetClothes(),
        });

        menu.Entries.Add(Group(Loc.PlayerOptions.GroupStats, Loc.PlayerOptions.GroupStatsDescription));

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.MpStats),
            Description = MenuText.Key(Loc.PlayerOptions.MpStatsDescription),
            LockedDescription = MenuText.Key(Loc.PlayerOptions.MpStatsLocked),
            MenuSubtitle = MenuText.Key(Loc.PlayerOptions.MpStatsSubtitle),
            Definition = new MpStatsMenu(),
        });

        menu.Entries.Add(Group(Loc.PlayerOptions.GroupScenarios, Loc.PlayerOptions.GroupScenariosDescription));

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.Scenarios),
            Description = MenuText.Key(Loc.PlayerOptions.ScenariosDescription),
            Gate = PlayerOptionsPermissions.Scenarios,
            Options = ScenarioNames(),
            OnSelected = selected => PlayerScenarios.Play(selected.SelectedIndex),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.StopScenario),
            Description = MenuText.Key(Loc.PlayerOptions.StopScenarioDescription),
            Gate = PlayerOptionsPermissions.Scenarios,
            OnSelected = _ => PlayerScenarios.ForceStop(),
        });
    }

    private static SeparatorEntry Group(string textKey, string descriptionKey) => new()
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(descriptionKey),
    };

    private static MenuText[] WantedLevels()
    {
        var options = new MenuText[6];

        options[0] = MenuText.Key(Loc.PlayerOptions.WantedNone);

        for (var stars = 1; stars < options.Length; stars++)
        {
            options[stars] = MenuText.Literal(stars.ToString());
        }

        return options;
    }

    private static MenuText[] ArmorTiers()
    {
        var options = new MenuText[PlayerActions.ArmorTiers + 1];

        options[0] = MenuText.Key(Loc.PlayerOptions.ArmorNone);

        for (var tier = 1; tier < options.Length; tier++)
        {
            options[tier] = MenuText.Literal($"{tier * 100 / PlayerActions.ArmorTiers}%");
        }

        return options;
    }

    private static MenuText[] DamagePacks()
    {
        var options = new MenuText[PedDamagePacks.Names.Length];

        for (var i = 0; i < options.Length; i++)
        {
            options[i] = MenuText.Literal(PedDamagePacks.Names[i]);
        }

        return options;
    }

    private static MenuText[] ScenarioNames()
    {
        var options = new MenuText[PedScenarios.All.Length];

        for (var i = 0; i < options.Length; i++)
        {
            options[i] = MenuText.Literal(PedScenarios.All[i].Label);
        }

        return options;
    }
}
