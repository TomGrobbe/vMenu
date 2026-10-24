using System.Globalization;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
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
        menu.Entries.Add(new SeparatorEntry(Loc.PlayerOptions.GroupProtection));

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.GodMode)
        {
            Gate = PlayerOptionsPermissions.Godmode,
            ReadState = () => PlayerGodMode.Enabled,
            OnChanged = changed => PlayerGodMode.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.Invisible)
        {
            Gate = PlayerOptionsPermissions.Invisible,
            ReadState = () => PlayerInvisible.Toggle.Enabled,
            OnChanged = changed => PlayerInvisible.Toggle.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.NoRagdoll)
        {
            Gate = PlayerOptionsPermissions.NoRagdoll,
            ReadState = () => PlayerNoRagdoll.Toggle.Enabled,
            OnChanged = changed => PlayerNoRagdoll.Toggle.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.StayInVehicle)
        {
            Gate = PlayerOptionsPermissions.StayInVehicle,
            ReadState = () => PlayerStayInVehicle.Enabled,
            OnChanged = changed => PlayerStayInVehicle.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.Freeze)
        {
            Gate = PlayerOptionsPermissions.Freeze,
            ReadState = () => PlayerFreeze.Enabled,
            OnChanged = changed => PlayerFreeze.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.EveryoneIgnores)
        {
            Gate = PlayerOptionsPermissions.Ignored,
            ReadState = () => EveryoneIgnoresPlayer.Enabled,
            OnChanged = changed => EveryoneIgnoresPlayer.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new SeparatorEntry(Loc.PlayerOptions.GroupMovement));

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.SuperJump)
        {
            Gate = PlayerOptionsPermissions.SuperJump,
            ReadState = () => PlayerSuperJump.Enabled,
            OnChanged = changed =>
            {
                PlayerSuperJump.SetEnabled(changed.Checked);

                MenuRegistry.Refresh(menu.Menu);
            },
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.BeastMode)
        {
            Gate = PlayerOptionsPermissions.SuperJump,
            ReadState = () => PlayerBeastMode.Enabled,
            OnChanged = changed =>
            {
                PlayerBeastMode.SetEnabled(changed.Checked);

                MenuRegistry.Refresh(menu.Menu);
            },
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.FastRun)
        {
            Gate = PlayerOptionsPermissions.FastRun,
            ReadState = () => PlayerFastRun.Toggle.Enabled,
            OnChanged = changed => PlayerFastRun.Toggle.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.FastSwim)
        {
            Gate = PlayerOptionsPermissions.FastSwim,
            ReadState = () => PlayerFastSwim.Toggle.Enabled,
            OnChanged = changed => PlayerFastSwim.Toggle.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.UnlimitedOxygen)
        {
            Gate = PlayerOptionsPermissions.UnlimitedOxygen,
            ReadState = () => PlayerUnlimitedOxygen.Toggle.Enabled,
            OnChanged = changed => PlayerUnlimitedOxygen.Toggle.SetEnabled(changed.Checked),
        });

        AddSpeedEntries(
            menu,
            PlayerMoveSpeed.Walk,
            PlayerOptionsPermissions.MoveSpeed,
            Loc.PlayerOptions.MoveSpeed,
            Loc.PlayerOptions.MoveSpeedRate);

        AddSpeedEntries(
            menu,
            PlayerMoveSpeed.Swim,
            PlayerOptionsPermissions.SwimSpeed,
            Loc.PlayerOptions.SwimSpeed,
            Loc.PlayerOptions.SwimSpeedRate);

        menu.Entries.Add(SubmenuEntry.For(new IntoxicationMenu()));

        menu.Entries.Add(new SeparatorEntry(Loc.PlayerOptions.GroupWanted));

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.NeverWanted)
        {
            Gate = PlayerOptionsPermissions.NeverWanted,
            ReadState = () => PlayerNeverWanted.Enabled,
            OnChanged = changed => PlayerNeverWanted.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new ListEntry(Loc.PlayerOptions.SetWanted, WantedLevels())
        {
            Gate = PlayerOptionsPermissions.SetWanted,
            ReadSelectedIndex = PlayerActions.WantedLevel,
            OnSelected = selected => PlayerActions.SetWantedLevel(selected.SelectedIndex),
        });

        menu.Entries.Add(new SeparatorEntry(Loc.PlayerOptions.GroupHealth));

        menu.Entries.Add(new ButtonEntry(Loc.PlayerOptions.HealPlayer)
        {
            Gate = PlayerOptionsPermissions.MaxHealth,
            OnSelected = _ => PlayerActions.Heal(),
        });

        menu.Entries.Add(new ListEntry(Loc.PlayerOptions.SetArmor, ArmorTiers())
        {
            Gate = PlayerOptionsPermissions.MaxArmor,
            ReadSelectedIndex = PlayerActions.ArmorTier,
            OnSelected = selected => PlayerActions.SetArmorTier(selected.SelectedIndex),
        });

        menu.Entries.Add(new ConfirmButtonEntry(Loc.PlayerOptions.Suicide)
        {
            ConfirmationDescription = MenuText.Key(Loc.PlayerOptions.SuicideConfirm),
            Gate = PlayerOptionsPermissions.Suicide,
            OnConfirmed = _ => PlayerActions.CommitSuicide(),
        });

        menu.Entries.Add(new SeparatorEntry(Loc.PlayerOptions.GroupAppearance));

        menu.Entries.Add(new CheckboxEntry(Loc.PlayerOptions.NoHelmet)
        {
            Gate = PlayerOptionsPermissions.NoHelmet,
            ReadState = () => PlayerNoHelmet.Toggle.Enabled,
            OnChanged = changed => PlayerNoHelmet.Toggle.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new ButtonEntry(Loc.PlayerOptions.ClearBlood)
        {
            Gate = PlayerOptionsPermissions.ClearBlood,
            OnSelected = _ => PlayerActions.ClearBlood(),
        });

        menu.Entries.Add(new ListEntry(Loc.PlayerOptions.SetBlood, DamagePacks())
        {
            Gate = PlayerOptionsPermissions.SetBlood,
            OnSelected = selected => PlayerActions.ApplyDamagePack(selected.SelectedIndex),
        });

        menu.Entries.Add(new ButtonEntry(Loc.PlayerOptions.CleanPlayer)
        {
            Gate = PlayerOptionsPermissions.CleanPlayer,
            OnSelected = _ => PlayerActions.CleanClothes(),
        });

        menu.Entries.Add(new ButtonEntry(Loc.PlayerOptions.DryPlayer)
        {
            Gate = PlayerOptionsPermissions.DryPlayer,
            OnSelected = _ => PlayerActions.DryClothes(),
        });

        menu.Entries.Add(new ButtonEntry(Loc.PlayerOptions.WetPlayer)
        {
            Gate = PlayerOptionsPermissions.WetPlayer,
            OnSelected = _ => PlayerActions.WetClothes(),
        });

        menu.Entries.Add(new SeparatorEntry(Loc.PlayerOptions.GroupStats));

        menu.Entries.Add(new SubmenuEntry(Loc.PlayerOptions.MpStats)
        {
            LockedDescription = MenuText.Key(Loc.PlayerOptions.MpStatsLocked),
            MenuSubtitle = MenuText.Key(Loc.PlayerOptions.MpStatsSubtitle),
            Definition = new MpStatsMenu(),
        });

        menu.Entries.Add(new SeparatorEntry(Loc.PlayerOptions.GroupScenarios));

        menu.Entries.Add(new ListEntry(Loc.PlayerOptions.Scenarios, ScenarioNames())
        {
            Gate = PlayerOptionsPermissions.Scenarios,
            OnSelected = selected => PlayerScenarios.Play(selected.SelectedIndex),
        });

        menu.Entries.Add(new ButtonEntry(Loc.PlayerOptions.StopScenario)
        {
            Gate = PlayerOptionsPermissions.Scenarios,
            OnSelected = _ => PlayerScenarios.ForceStop(),
        });
    }

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

    private static void AddSpeedEntries(
        MenuBuilder menu,
        PlayerMoveSpeed speed,
        string permission,
        string toggleKey,
        string rateKey)
    {
        menu.Entries.Add(new CheckboxEntry(toggleKey)
        {
            Gate = permission,
            ReadState = () => speed.Enabled,
            OnChanged = changed => speed.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new ListEntry(
            rateKey,
            [.. speed.Speeds.Select(rate => MenuText.Literal(rate.ToString("0.0x", CultureInfo.InvariantCulture)))])
        {
            Gate = permission,
            ReadSelectedIndex = () => speed.SpeedIndex,
            OnIndexChanged = changed => speed.SetSpeedIndex(changed.NewIndex),
        });
    }
}
