using System.Globalization;

using MenuAPI;

using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Menus.Weapons;
using vMenu.Enhanced.Menus.Weapons.Saved;

using WeaponLoadoutsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponLoadouts;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.WeaponLoadouts.Title,
    SubtitleKey = Loc.WeaponLoadouts.Subtitle,
    DescriptionKey = Loc.WeaponLoadouts.LinkDescription,
    Permission = WeaponLoadoutsPermissions.Menu)]
public sealed class WeaponLoadoutsMenu : MenuDefinition
{
    private const int MaxNameLength = 30;

    private DetachedMenu? _listMenu;
    private DetachedMenu? _detailMenu;

    private WeaponLoadout? _selected;

    protected override void Build(MenuBuilder menu)
    {
        // Declared before the rows that open them, so every row has something to point at.
        _detailMenu = menu.AddDetachedMenu(
            MenuText.From(() => _selected?.Name ?? string.Empty),
            MenuText.Key(Loc.WeaponLoadouts.ManageTitle),
            BuildDetailMenu,
            MenuGate.When(() => _selected is not null));

        _listMenu = menu.AddDetachedMenu(
            MenuText.Key(Loc.WeaponLoadouts.Title),
            MenuText.Key(Loc.WeaponLoadouts.ManageSubtitle),
            _ => { });

        // Rebuilt on every open: saving, renaming and deleting all change what belongs in it.
        _listMenu.Builder.OnOpened = _ => FillList();

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.Save),
            Description = MenuText.Key(Loc.WeaponLoadouts.SaveDescription),
            Gate = WeaponLoadoutsPermissions.Save,
            OnSelectedAsync = _ => SaveAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.Manage),
            Description = MenuText.Key(Loc.WeaponLoadouts.ManageDescription),
            Label = "→",
            OnSelected = _ => _listMenu?.Open(),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.RestoreOnRespawn),
            Description = MenuText.Key(Loc.WeaponLoadouts.RestoreOnRespawnDescription),
            Gate = WeaponLoadoutsPermissions.EquipOnRespawn,
            ReadState = () => WeaponLoadoutRespawn.Enabled,
            OnChanged = changed => WeaponLoadoutRespawn.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.KeepOnPedChange),
            Description = MenuText.Key(Loc.WeaponLoadouts.KeepOnPedChangeDescription),
            Gate = WeaponLoadoutsPermissions.KeepOnPedChange,
            ReadState = () => WeaponCarryOver.Enabled,
            OnChanged = changed => WeaponCarryOver.SetEnabled(changed.Checked),
        });
    }

    private void FillList()
    {
        if (_listMenu is not { } listMenu)
        {
            return;
        }

        listMenu.Builder.ClearEntries();

        var loadouts = WeaponLoadoutStore.All();

        if (loadouts.Count == 0)
        {
            listMenu.Builder.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.WeaponLoadouts.Empty),
                Description = MenuText.Key(Loc.WeaponLoadouts.EmptyDescription),
            });

            return;
        }

        foreach (var loadout in loadouts)
        {
            var current = loadout;

            listMenu.Builder.Add(new ButtonEntry
            {
                Text = MenuText.Literal(current.Name),
                Description = MenuText.Key(
                    Loc.WeaponLoadouts.LoadoutDescription,
                    ("name", MenuText.Literal(current.Name))),
                Label = MenuText.Key(
                    Loc.WeaponLoadouts.WeaponCount,
                    ("count", MenuText.Literal(current.Weapons.Count.ToString(CultureInfo.InvariantCulture)))),
                // The one the player picked to come back with, marked so they can see which it is without opening
                // each one.
                ReadLeftIcon = () => WeaponLoadoutStore.IsDefault(current.Name)
                    ? MenuItem.Icon.TICK
                    : MenuItem.Icon.NONE,
                OnSelected = _ =>
                {
                    _selected = current;

                    _detailMenu?.Open();
                },
            });
        }
    }

    private void BuildDetailMenu(MenuBuilder detailMenu)
    {
        detailMenu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.Equip),
            Description = MenuText.Key(Loc.WeaponLoadouts.EquipDescription),
            Gate = WeaponLoadoutsPermissions.Equip,
            OnSelectedAsync = _ => EquipAsync(),
        });

        detailMenu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.Rename),
            Description = MenuText.Key(Loc.WeaponLoadouts.RenameDescription),
            Gate = WeaponLoadoutsPermissions.Manage,
            OnSelectedAsync = _ => RenameAsync(),
        });

        detailMenu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.Clone),
            Description = MenuText.Key(Loc.WeaponLoadouts.CloneDescription),
            Gate = WeaponLoadoutsPermissions.Save,
            OnSelectedAsync = _ => CloneAsync(),
        });

        detailMenu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.SetDefault),
            Description = MenuText.Key(Loc.WeaponLoadouts.SetDefaultDescription),
            Gate = WeaponLoadoutsPermissions.Manage,
            ReadLeftIcon = () => _selected is { } loadout && WeaponLoadoutStore.IsDefault(loadout.Name)
                ? MenuItem.Icon.TICK
                : MenuItem.Icon.NONE,
            OnSelected = _ =>
            {
                if (_selected is not { } loadout)
                {
                    return;
                }

                WeaponLoadoutStore.SetDefault(loadout.Name);

                Notifications.Success(MenuText.Key(Loc.WeaponLoadouts.DefaultSet, ("name", MenuText.Literal(loadout.Name))));

                _detailMenu?.Refresh();
            },
        });

        // Two press rows. The framework clears a primed confirmation on open, on close and when the player
        // moves to another row, so none of them can be left armed the way vMenu's Replace was.
        detailMenu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.Replace),
            Description = MenuText.Key(Loc.WeaponLoadouts.ReplaceDescription),
            Gate = WeaponLoadoutsPermissions.Save,
            OnConfirmed = _ => Replace(),
        });

        detailMenu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponLoadouts.Delete),
            Description = MenuText.Key(Loc.WeaponLoadouts.DeleteDescription),
            Gate = WeaponLoadoutsPermissions.Manage,
            OnConfirmed = _ => Delete(),
        });
    }

    private async Task SaveAsync()
    {
        var captured = WeaponLoadoutStore.Capture(string.Empty);

        if (captured.Weapons.Count == 0)
        {
            Notifications.Error(MenuText.Key(Loc.WeaponLoadouts.SaveNothingHeld));
            return;
        }

        var typed = await UserInput.GetTextAsync(MenuText.Key(Loc.WeaponLoadouts.SavePrompt), MaxNameLength);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        captured.Name = typed.Trim();

        Report(WeaponLoadoutStore.Save(captured, replacing: false), captured.Name, Loc.WeaponLoadouts.Saved);
    }

    private async Task EquipAsync()
    {
        if (_selected is not { } loadout)
        {
            return;
        }

        var report = await WeaponLoadoutApply.ApplyAsync(loadout, append: false, ignorePermissions: false);

        MenuAudit.ReportAction(AuditActions.LoadoutEquipped, loadout.Name, string.Join(", ", report.Names));

        // Said once at the end rather than per weapon, so a loadout full of things this server does not
        // allow does not bury the player in messages.
        if (report.Skipped > 0)
        {
            Notifications.Warning(
                MenuText.Key(
                    Loc.WeaponLoadouts.EquippedWithSkipped,
                    ("name", MenuText.Literal(loadout.Name)),
                    ("count", MenuText.Literal(report.Skipped.ToString(CultureInfo.InvariantCulture)))),
                Notifications.SpawnDurationMs);

            return;
        }

        Notifications.Success(
            MenuText.Key(Loc.WeaponLoadouts.Equipped, ("name", MenuText.Literal(loadout.Name))),
            Notifications.SpawnDurationMs);
    }

    private async Task RenameAsync()
    {
        if (_selected is not { } loadout)
        {
            return;
        }

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.WeaponLoadouts.RenamePrompt),
            MaxNameLength,
            loadout.Name);

        if (string.IsNullOrWhiteSpace(typed) || typed.Trim() == loadout.Name)
        {
            return;
        }

        var newName = typed.Trim();

        if (!WeaponLoadoutStore.Rename(loadout, newName))
        {
            Notifications.Error(MenuText.Key(Loc.WeaponLoadouts.SaveNameTaken, ("name", MenuText.Literal(newName))));
            return;
        }

        Notifications.Success(MenuText.Key(Loc.WeaponLoadouts.Renamed, ("name", MenuText.Literal(newName))));

        _detailMenu?.Refresh();
    }

    private async Task CloneAsync()
    {
        if (_selected is not { } loadout)
        {
            return;
        }

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.WeaponLoadouts.ClonePrompt),
            MaxNameLength,
            loadout.Name);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var newName = typed.Trim();

        if (!WeaponLoadoutStore.Duplicate(loadout, newName))
        {
            Notifications.Error(MenuText.Key(Loc.WeaponLoadouts.SaveNameTaken, ("name", MenuText.Literal(newName))));
            return;
        }

        Notifications.Success(MenuText.Key(Loc.WeaponLoadouts.Cloned, ("name", MenuText.Literal(newName))));
    }

    private void Replace()
    {
        if (_selected is not { } loadout)
        {
            return;
        }

        var captured = WeaponLoadoutStore.Capture(loadout.Name);

        if (captured.Weapons.Count == 0)
        {
            Notifications.Error(MenuText.Key(Loc.WeaponLoadouts.SaveNothingHeld));
            return;
        }

        if (Report(WeaponLoadoutStore.Save(captured, replacing: true), loadout.Name, Loc.WeaponLoadouts.Replaced))
        {
            _selected = captured;
        }
    }

    private void Delete()
    {
        if (_selected is not { } loadout)
        {
            return;
        }

        WeaponLoadoutStore.Delete(loadout.Name);

        Notifications.Success(MenuText.Key(Loc.WeaponLoadouts.Deleted, ("name", MenuText.Literal(loadout.Name))));

        _selected = null;

        // Straight back to the list, which rebuilds itself, rather than leaving the player looking at the
        // options for something that is no longer there.
        _listMenu?.Open();
    }

    private static bool Report(SaveOutcome outcome, string name, string successKey)
    {
        switch (outcome)
        {
            case SaveOutcome.Saved:
                Notifications.Success(MenuText.Key(successKey, ("name", MenuText.Literal(name))));
                return true;

            case SaveOutcome.NameTaken:
                Notifications.Error(MenuText.Key(Loc.WeaponLoadouts.SaveNameTaken, ("name", MenuText.Literal(name))));
                return false;

            default:
                Notifications.Error(MenuText.Key(Loc.WeaponLoadouts.SaveFailed));
                return false;
        }
    }
}
