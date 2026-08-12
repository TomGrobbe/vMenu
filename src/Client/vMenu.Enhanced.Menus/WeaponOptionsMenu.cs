using System.Globalization;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Weapons;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Ticks;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.WeaponOptions.Title,
    SubtitleKey = Loc.WeaponOptions.Subtitle,
    DescriptionKey = Loc.WeaponOptions.LinkDescription,
    Permission = WeaponOptionsPermissions.Menu)]
public sealed class WeaponOptionsMenu : MenuDefinition
{
    private const int WatchMs = 250;

    private const int MaxAmmoInput = 10;

    private WeaponCategory[] _categories = [];

    private DetachedMenu? _weaponMenu;
    private TickHandle? _watch;

    private WeaponEntry? _selected;
    private string _selectedCategory = string.Empty;
    private uint _selectedHash;

    /// <summary>What the open weapon looked like last time round, so a change can be spotted.</summary>
    private string _lastSeen = string.Empty;

    public override async Task PrepareAsync()
    {
        await WeaponSync.WaitForFirstAsync();

        _categories = [.. WeaponSync.Categories];
    }

    protected override void Build(MenuBuilder menu)
    {
        BuildTopLevel(menu);

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.Parachute),
            Description = MenuText.Key(Loc.WeaponOptions.ParachuteDescription),
            MenuTitle = MenuText.Key(Loc.WeaponOptions.Title),
            MenuSubtitle = MenuText.Key(Loc.WeaponOptions.ParachuteSubtitle),
            Gate = WeaponOptionsPermissions.Parachute,
            Build = BuildParachuteMenu,
        });

        // Declared before the category rows so every one of them has something to open.
        _weaponMenu = menu.AddDetachedMenu(
            MenuText.From(() => _selected is { } weapon
                ? WeaponNames.Resolve(weapon.Label, weapon.SpawnName)
                : string.Empty),
            MenuText.Key(Loc.WeaponOptions.WeaponSubtitle),
            _ => { },
            MenuGate.When(() => _selected is { } weapon
                && ClientWeaponPermissions.CanUseWeapon(weapon.SpawnName, _selectedCategory)));

        _watch ??= TickRegistry.Register("Weapons.OpenWeaponMenu", Resync, TickRate.Every(WatchMs), autoStart: false);

        _weaponMenu.Builder.OnOpened = _ => _watch?.Start();

        _weaponMenu.Builder.OnClosed = _ => _watch?.Stop();

        foreach (var category in _categories)
        {
            var current = category;

            menu.Entries.Add(new SubmenuEntry
            {
                Text = MenuText.Literal(current.Name),
                Description = MenuText.Key(
                    Loc.WeaponOptions.CategoryDescription,
                    ("category", MenuText.Literal(current.Name))),
                MenuTitle = MenuText.Literal(current.Name),
                MenuSubtitle = MenuText.Key(Loc.WeaponOptions.CategorySubtitle),
                Gate = MenuGate.When(() => ClientWeaponPermissions.CanUseCategory(current.Name)),
                Build = categoryMenu => BuildCategoryMenu(categoryMenu, current),
            });
        }

        if (_categories.Length == 0)
        {
            menu.Entries.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.WeaponOptions.Empty),
                Description = MenuText.Key(Loc.WeaponOptions.EmptyDescription),
            });
        }
    }

    private void BuildTopLevel(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.GetAll),
            Description = MenuText.Key(Loc.WeaponOptions.GetAllDescription),
            Gate = WeaponOptionsPermissions.GetAll,
            OnSelected = _ => Notifications.Success(MenuText.Key(
                Loc.WeaponOptions.GetAllDone,
                ("count", MenuText.Literal(WeaponInventory.GiveAll().ToString(CultureInfo.InvariantCulture))))),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.RemoveAll),
            Description = MenuText.Key(Loc.WeaponOptions.RemoveAllDescription),
            Gate = WeaponOptionsPermissions.RemoveAll,
            OnSelected = _ =>
            {
                WeaponInventory.RemoveAll();

                Notifications.Success(MenuText.Key(Loc.WeaponOptions.RemoveAllDone));
            },
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.UnlimitedAmmo),
            Description = MenuText.Key(Loc.WeaponOptions.UnlimitedAmmoDescription),
            Gate = WeaponOptionsPermissions.UnlimitedAmmo,
            ReadState = () => WeaponUnlimitedAmmo.Enabled,
            OnChanged = changed => WeaponUnlimitedAmmo.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.NoReload),
            Description = MenuText.Key(Loc.WeaponOptions.NoReloadDescription),
            Gate = WeaponOptionsPermissions.NoReload,
            ReadState = () => WeaponNoReload.Enabled,
            OnChanged = changed => WeaponNoReload.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.SetAllAmmo),
            Description = MenuText.Key(Loc.WeaponOptions.SetAllAmmoDescription),
            Gate = WeaponOptionsPermissions.SetAllAmmo,
            OnSelectedAsync = _ => SetAllAmmoAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.RefillAll),
            Description = MenuText.Key(Loc.WeaponOptions.RefillAllDescription),
            Gate = WeaponOptionsPermissions.SetAllAmmo,
            OnSelected = _ => Notifications.Success(MenuText.Key(
                Loc.WeaponOptions.RefillAllDone,
                ("count", MenuText.Literal(WeaponInventory.RefillAll().ToString(CultureInfo.InvariantCulture))))),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.SpawnByName),
            Description = MenuText.Key(Loc.WeaponOptions.SpawnByNameDescription),
            Gate = WeaponOptionsPermissions.SpawnByName,
            OnSelectedAsync = _ => SpawnByNameAsync(),
        });
    }

    private void BuildCategoryMenu(MenuBuilder categoryMenu, WeaponCategory category)
    {
        var categoryName = category.Name;

        foreach (var weapon in category.Weapons)
        {
            var current = weapon;
            var hash = API.Hash(current.SpawnName);

            // Asked for here rather than from the row below, because the game only hands the bars
            // back a frame later and this runs long before anybody opens the menu.
            WeaponStatistics.Request(hash);

            categoryMenu.Entries.Add(new ButtonEntry
            {
                Text = WeaponNames.Display(current.Label, current.SpawnName),
                Description = MenuText.Key(
                    Loc.WeaponOptions.WeaponDescription,
                    ("weapon", WeaponNames.Display(current.Label, current.SpawnName))),
                LeftIcon = MenuItem.Icon.GUN,
                Gate = MenuGate.When(() => ClientWeaponPermissions.CanUseWeapon(current.SpawnName, categoryName)),
                WeaponStats = () => WeaponStatistics.For(hash),
                // The weapon's own menu is built in the frame it opens, which is too late to ask
                // about its components, so highlighting the row that opens it is the cue instead.
                OnHighlighted = _ => RequestComponentStats(hash),
                OnSelected = _ => OpenWeapon(current, categoryName, hash),
            });
        }
    }

    private void OpenWeapon(WeaponEntry weapon, string categoryName, uint hash)
    {
        if (_weaponMenu is not { } weaponMenu)
        {
            return;
        }

        _selected = weapon;
        _selectedCategory = categoryName;
        _selectedHash = hash;
        _lastSeen = Snapshot();

        weaponMenu.Builder.ClearEntries();

        BuildWeaponMenu(weaponMenu.Builder, hash);

        weaponMenu.Open();
    }

    private static void RequestComponentStats(uint weaponHash)
    {
        foreach (var component in WeaponComponentProbe.For(weaponHash))
        {
            WeaponStatistics.RequestComponent(API.Hash(component.SpawnName));
        }
    }

    private void BuildWeaponMenu(MenuBuilder weaponMenu, uint hash)
    {
        weaponMenu.Add(new ButtonEntry
        {
            // One row that says which of the two it currently is, rather than two rows where one is
            // always the wrong thing to press.
            Text = MenuText.From(() => Localizer.Current.Get(WeaponInventory.Has(hash)
                ? Loc.WeaponOptions.RemoveWeapon
                : Loc.WeaponOptions.EquipWeapon)),
            Description = MenuText.From(() => Localizer.Current.Get(WeaponInventory.Has(hash)
                ? Loc.WeaponOptions.RemoveWeaponDescription
                : Loc.WeaponOptions.EquipWeaponDescription)),
            LeftIcon = MenuItem.Icon.GUN,
            Gate = WeaponOptionsPermissions.Spawn,
            WeaponStats = () => WeaponStatistics.For(hash),
            OnSelected = _ =>
            {
                WeaponInventory.Toggle(hash);

                Resync(force: true);
            },
        });

        weaponMenu.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.RefillAmmo),
            Description = MenuText.Key(Loc.WeaponOptions.RefillAmmoDescription),
            Label = MenuText.From(() => AmmoLabel(hash)),
            LeftIcon = MenuItem.Icon.AMMO,
            WeaponStats = () => WeaponStatistics.For(hash),
            OnSelected = _ =>
            {
                if (!WeaponInventory.Refill(hash))
                {
                    Notifications.Error(MenuText.Key(Loc.WeaponOptions.NotHeld));
                }

                Resync(force: true);
            },
        });

        weaponMenu.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.Tints),
            Description = MenuText.Key(Loc.WeaponOptions.TintsDescription),
            Options = WeaponTints.Options(WeaponInventory.TintCount(hash)),
            Gate = WeaponOptionsPermissions.Modify,
            WeaponStats = () => WeaponStatistics.For(hash),
            ReadSelectedIndex = () => WeaponInventory.Tint(hash),
            OnIndexChanged = changed =>
            {
                if (!WeaponInventory.Has(hash))
                {
                    Notifications.Error(MenuText.Key(Loc.WeaponOptions.NotHeld));
                    return;
                }

                WeaponInventory.SetTint(hash, changed.NewIndex);
            },
        });

        foreach (var component in WeaponComponentProbe.For(hash))
        {
            var componentHash = API.Hash(component.SpawnName);

            weaponMenu.Add(new ButtonEntry
            {
                Text = WeaponNames.Display(component.Label, component.SpawnName),
                Description = MenuText.Key(Loc.WeaponOptions.ComponentDescription),
                Gate = WeaponOptionsPermissions.Modify,
                WeaponStats = () => WeaponStatistics.For(hash),
                // Only this component's own effect, whether or not it is fitted, so the panel answers
                // "what would this one do" rather than showing the weapon as it already is.
                WeaponComponentStats = () => WeaponStatistics.ForComponent(componentHash),
                // Read live rather than set once, so a component another resource fits shows up here
                // without the player having to leave the menu and come back.
                ReadLeftIcon = () => WeaponInventory.HasComponent(hash, componentHash)
                    ? MenuItem.Icon.TICK
                    : MenuItem.Icon.NONE,
                OnSelected = _ =>
                {
                    if (!WeaponInventory.Has(hash))
                    {
                        Notifications.Error(MenuText.Key(Loc.WeaponOptions.NotHeld));
                        return;
                    }

                    WeaponInventory.ToggleComponent(hash, componentHash);

                    Resync(force: true);
                },
            });
        }
    }

    private void BuildParachuteMenu(MenuBuilder parachuteMenu)
    {
        parachuteMenu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.TogglePrimary),
            Description = MenuText.Key(Loc.WeaponOptions.TogglePrimaryDescription),
            OnSelected = _ => Notifications.Success(MenuText.Key(ParachuteOptions.TogglePrimary()
                ? Loc.WeaponOptions.PrimaryAdded
                : Loc.WeaponOptions.PrimaryRemoved)),
        });

        parachuteMenu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.EnableReserve),
            Description = MenuText.Key(Loc.WeaponOptions.EnableReserveDescription),
            OnSelected = _ =>
            {
                ParachuteOptions.EnableReserve();

                Notifications.Success(MenuText.Key(Loc.WeaponOptions.ReserveAdded));
            },
        });

        parachuteMenu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.AutoEquip),
            Description = MenuText.Key(Loc.WeaponOptions.AutoEquipDescription),
            ReadState = () => ParachuteOptions.AutoEquipEnabled,
            OnChanged = changed => ParachuteOptions.SetAutoEquip(changed.Checked),
        });

        parachuteMenu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.UnlimitedParachutes),
            Description = MenuText.Key(Loc.WeaponOptions.UnlimitedParachutesDescription),
            ReadState = () => ParachuteOptions.UnlimitedEnabled,
            OnChanged = changed => ParachuteOptions.SetUnlimited(changed.Checked),
        });

        parachuteMenu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.SmokeColor),
            Description = MenuText.Key(Loc.WeaponOptions.SmokeColorDescription),
            Options = [.. Enumerable.Range(0, ParachuteOptions.SmokeCount).Select(ParachuteOptions.SmokeName)],
            // Applied on select rather than as the player scrolls: each change takes a few seconds,
            // during which the trail cannot be used.
            OnSelectedAsync = async selected =>
            {
                Notifications.Info(MenuText.Key(Loc.WeaponOptions.SmokeColorChanging));

                await ParachuteOptions.SetSmokeColourAsync(selected.SelectedIndex);
            },
        });

        parachuteMenu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.PrimaryStyle),
            Description = MenuText.Key(Loc.WeaponOptions.PrimaryStyleDescription),
            Options = [.. Enumerable.Range(0, ParachuteOptions.StyleCount).Select(ParachuteOptions.StyleName)],
            OnIndexChanged = changed => ParachuteOptions.SetPrimaryStyle(changed.NewIndex),
        });

        parachuteMenu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.WeaponOptions.ReserveStyle),
            Description = MenuText.Key(Loc.WeaponOptions.ReserveStyleDescription),
            Options = [.. Enumerable.Range(0, ParachuteOptions.StyleCount).Select(ParachuteOptions.StyleName)],
            OnIndexChanged = changed => ParachuteOptions.SetReserveStyle(changed.NewIndex),
        });
    }

    private async Task SetAllAmmoAsync()
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.WeaponOptions.SetAllAmmoPrompt),
            MaxAmmoInput,
            "100");

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        if (!int.TryParse(typed.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ammo) || ammo < 0)
        {
            Notifications.Error(MenuText.Key(Loc.WeaponOptions.SetAllAmmoInvalid));
            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.WeaponOptions.SetAllAmmoDone,
            ("count", MenuText.Literal(WeaponInventory.SetAllAmmo(ammo).ToString(CultureInfo.InvariantCulture)))));
    }

    private async Task SpawnByNameAsync()
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.WeaponOptions.SpawnByNamePrompt),
            maxLength: 40,
            suggestions: Suggestions());

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var spawnName = typed.Trim().ToLowerInvariant();
        var hash = API.Hash(spawnName);

        if (!Native.IsWeaponValid(hash))
        {
            Notifications.Error(MenuText.Key(Loc.WeaponOptions.SpawnByNameInvalid, ("weapon", MenuText.Literal(spawnName))));
            return;
        }

        // A weapon that is in the list answers to its category, and one that is not is only reachable
        // through this row, which has already been gated.
        var known = WeaponSync.Find(spawnName);

        if (known is { } found && !ClientWeaponPermissions.CanUseWeapon(found.SpawnName, found.Category))
        {
            Notifications.Warning(MenuText.Key(Loc.WeaponOptions.SpawnByNameDenied, ("weapon", MenuText.Literal(spawnName))));
            return;
        }

        WeaponInventory.Give(hash);
    }

    /// <summary>Built per opening: a permission refresh in between changes what belongs in it.</summary>
    private IReadOnlyList<InputSuggestion> Suggestions() =>
        [.. _categories
            .SelectMany(category => category.Weapons.Select(weapon => (weapon.SpawnName, weapon.Label, category.Name)))
            .Where(weapon => ClientWeaponPermissions.CanUseWeapon(weapon.SpawnName, weapon.Name))
            .Select(weapon => new InputSuggestion
            {
                Value = weapon.SpawnName,
                Label = WeaponNames.Resolve(weapon.Label, weapon.SpawnName),
                Detail = weapon.Name,
            })];

    private string AmmoLabel(uint weaponHash) =>
        WeaponInventory.Has(weaponHash)
            ? WeaponInventory.Ammo(weaponHash).ToString(CultureInfo.InvariantCulture)
            : string.Empty;

    private void Resync() => Resync(force: false);

    /// <summary>
    /// Puts the open weapon's rows back in step with the weapon itself. Nothing is rebuilt: every row
    /// that can go stale reads its own state, so re-applying them is the whole of it, and the
    /// player's place in the list is kept.
    /// </summary>
    private void Resync(bool force)
    {
        if (_weaponMenu is not { } weaponMenu || _selected is null)
        {
            return;
        }

        var seen = Snapshot();

        if (!force && seen == _lastSeen)
        {
            return;
        }

        _lastSeen = seen;

        weaponMenu.Refresh();
    }

    /// <summary>
    /// Everything about the open weapon that another resource could change, as one string. Compared
    /// rather than acted on, so a menu that is already right is left alone.
    /// </summary>
    private string Snapshot()
    {
        if (_selected is null)
        {
            return string.Empty;
        }

        var hash = _selectedHash;

        if (!WeaponInventory.Has(hash))
        {
            return "-";
        }

        var fitted = string.Concat(WeaponComponentProbe
            .For(hash)
            .Select(component => WeaponInventory.HasComponent(hash, API.Hash(component.SpawnName)) ? '1' : '0'));

        return $"{WeaponInventory.Ammo(hash)}:{WeaponInventory.Tint(hash)}:{fitted}";
    }
}
