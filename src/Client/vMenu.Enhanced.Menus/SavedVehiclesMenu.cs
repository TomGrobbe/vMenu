using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Data;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Vehicles.Appearance;
using vMenu.Enhanced.Menus.Vehicles.Saved;
using vMenu.Enhanced.Permissions;

using SavedVehiclesPermissions = vMenu.Enhanced.Data.Permissions.Menus.SavedVehicles;
using SavedVehiclesSettings = vMenu.Enhanced.Data.Configuration.Settings.SavedVehicles;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Vehicles the player put away, kept on their own machine.
/// </summary>
/// <remarks>
/// Laid out like the teleport menu: one shared child menu for the vehicles in a category and one for
/// what can be done to a vehicle, rather than a menu per row. The rows are runtime data and there
/// could be a great many of them, and MenuAPI cannot take a menu back out once it has been added.
/// </remarks>
[VMenu(
    TitleKey = Loc.SavedVehicles.Title,
    SubtitleKey = Loc.SavedVehicles.Subtitle,
    DescriptionKey = Loc.SavedVehicles.LinkDescription,
    Permission = SavedVehiclesPermissions.Menu)]
public sealed class SavedVehiclesMenu : MenuDefinition
{
    private const int NameLength = 30;

    private MenuBuilder? _root;

    private DetachedMenu? _vehicleMenu;

    private DetachedMenu? _detailMenu;

    private DetachedMenu? _categoryMenu;

    /// <summary>Empty is the uncategorised group, which is a real place rather than a missing one.</summary>
    private string _category = string.Empty;

    private SavedVehicleEntry? _selected;

    protected override void Build(MenuBuilder menu)
    {
        _root = menu;

        _vehicleMenu = menu.AddDetachedMenu(
            MenuText.From(CategoryTitle),
            MenuText.From(CategoryTitle),
            _ => { });

        _vehicleMenu.Builder.OnOpened = _ => Fill(_vehicleMenu, VehicleRows());

        _detailMenu = menu.AddDetachedMenu(
            MenuText.From(() => _selected?.Vehicle.Name ?? string.Empty),
            MenuText.Key(Loc.SavedVehicles.Title),
            _ => { });

        _detailMenu.Builder.OnOpened = _ => Fill(_detailMenu, DetailRows());

        _categoryMenu = menu.AddDetachedMenu(
            MenuText.Key(Loc.SavedVehicles.Categories),
            MenuText.Key(Loc.SavedVehicles.Categories),
            _ => { },
            SavedVehiclesPermissions.Manage);

        _categoryMenu.Builder.OnOpened = _ => Fill(_categoryMenu, CategoryRows());

        menu.AddRange(RootRows());

        // The store changes from inside this menu, so the rows are rebuilt whenever the player comes
        // back up to this level.
        menu.OnOpened = _ => Refill(menu, RootRows());
    }

    #region Rows

    private IReadOnlyList<MenuEntry> RootRows()
    {
        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.SaveCurrent),
                Description = MenuText.Key(Loc.SavedVehicles.SaveCurrentDescription),
                Gate = SavedVehiclesPermissions.Save,
                OnSelectedAsync = _ => SaveCurrentAsync(),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.Categories),
                Description = MenuText.Key(Loc.SavedVehicles.CategoriesDescription),
                Gate = SavedVehiclesPermissions.Manage,
                OnSelected = _ => _categoryMenu?.Open(),
            },
        };

        var vehicles = SavedVehicleStore.All();

        if (vehicles.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.NoVehicles),
                Description = MenuText.Key(Loc.SavedVehicles.NoVehiclesDescription),
            });

            return rows;
        }

        foreach (var name in GroupNames(vehicles))
        {
            var group = name;
            var count = Count(vehicles, group);

            rows.Add(new ButtonEntry
            {
                Text = group.Length == 0 ? MenuText.Key(Loc.SavedVehicles.Uncategorised) : MenuText.Literal(group),
                Description = MenuText.Key(
                    Loc.SavedVehicles.CategoryRowDescription,
                    ("count", MenuText.Literal(count.ToString(CultureInfo.InvariantCulture)))),
                OnSelected = _ =>
                {
                    _category = group;

                    _vehicleMenu?.Open();
                },
            });
        }

        return rows;
    }

    private IReadOnlyList<MenuEntry> VehicleRows()
    {
        var rows = new List<MenuEntry>();

        foreach (var entry in SavedVehicleStore.All())
        {
            if (!string.Equals(GroupOf(entry), _category, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            rows.Add(VehicleRow(entry));
        }

        if (rows.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.NoVehicles),
                Description = MenuText.Key(Loc.SavedVehicles.NoVehiclesDescription),
            });
        }

        return rows;
    }

    private ButtonEntry VehicleRow(SavedVehicleEntry entry)
    {
        var appearance = entry.Vehicle.Appearance;
        var model = MenuText.Literal(VehicleModelNames.Resolve(appearance.ModelHash, appearance.ModelName));
        var available = Native.IsModelInCdimage(appearance.ModelHash);

        // A model this server does not have cannot be asked for its class, let alone its handling.
        VehicleStats? stats = available
            ? VehicleClassStats.Normalise(appearance.ModelHash, Native.GetVehicleClassFromName(appearance.ModelHash))
            : null;

        return new ButtonEntry
        {
            Text = MenuText.Literal(entry.Vehicle.Name),
            Label = entry.IsFromNewerBuild
                ? MenuText.Key(Loc.SavedVehicles.NewerBuildLabel)
                : MenuText.From(() => VehicleSpawning.DisplayName(appearance.ModelHash)),
            Description = Describe(entry, available, model),
            VehicleStats = () => stats,
            OnSelected = _ =>
            {
                _selected = entry;

                _detailMenu?.Open();
            },
        };
    }

    private static MenuText Describe(SavedVehicleEntry entry, bool available, MenuText model)
    {
        if (entry.IsFromNewerBuild)
        {
            return MenuText.Key(Loc.SavedVehicles.NewerBuildDescription);
        }

        return available
            ? MenuText.Key(Loc.SavedVehicles.VehicleRowDescription, ("model", model))
            : MenuText.Key(Loc.SavedVehicles.ModelUnavailable, ("model", model));
    }

    private IReadOnlyList<MenuEntry> DetailRows()
    {
        if (_selected is not { } entry)
        {
            return
            [
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.SavedVehicles.NoVehicles),
                    Description = MenuText.Key(Loc.SavedVehicles.NoVehiclesDescription),
                },
            ];
        }

        var name = MenuText.Literal(entry.Vehicle.Name);

        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.Spawn),
                Description = MenuText.Key(Loc.SavedVehicles.SpawnDescription),
                Gate = SavedVehiclesPermissions.Spawn,
                OnSelectedAsync = _ => SpawnAsync(entry),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.Rename),
                Description = MenuText.Key(Loc.SavedVehicles.RenameDescription),
                Gate = SavedVehiclesPermissions.Manage,
                OnSelectedAsync = _ => RenameAsync(entry),
            },
            MoveRow(entry),
        };

        // Overwriting a save this build cannot fully read would silently drop whatever the newer
        // version put in it, so the row becomes an offer to save alongside it instead.
        if (entry.IsFromNewerBuild)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.SaveAsNew),
                Description = MenuText.Key(Loc.SavedVehicles.SaveAsNewDescription),
                Gate = SavedVehiclesPermissions.Save,
                OnSelectedAsync = _ => SaveCurrentAsync(),
            });
        }
        else
        {
            rows.Add(new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.Replace),
                Description = MenuText.Key(Loc.SavedVehicles.ReplaceDescription),
                ConfirmationDescription = MenuText.Key(Loc.SavedVehicles.ReplaceConfirm, ("name", name)),
                Gate = SavedVehiclesPermissions.Manage,
                OnConfirmed = _ => Replace(entry),
            });
        }

        rows.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.SavedVehicles.Delete),
            Description = MenuText.Key(Loc.SavedVehicles.DeleteDescription),
            ConfirmationDescription = MenuText.Key(Loc.SavedVehicles.DeleteConfirm, ("name", name)),
            Gate = SavedVehiclesPermissions.Manage,
            OnConfirmed = _ => Delete(entry),
        });

        return rows;
    }

    private ListEntry MoveRow(SavedVehicleEntry entry)
    {
        var groups = GroupNames(SavedVehicleStore.All());

        var options = new List<MenuText>(groups.Count);

        foreach (var group in groups)
        {
            options.Add(group.Length == 0 ? MenuText.Key(Loc.SavedVehicles.Uncategorised) : MenuText.Literal(group));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.SavedVehicles.MoveToCategory),
            Description = MenuText.Key(Loc.SavedVehicles.MoveToCategoryDescription),
            Options = options,
            Gate = SavedVehiclesPermissions.Manage,
            ReadSelectedIndex = () => Math.Max(0, IndexOf(groups, GroupOf(entry))),

            // On enter rather than on scroll, because every step past a category would otherwise be
            // a write to disk and a rebuild of the menu the player is standing in.
            OnSelected = selected =>
            {
                if (selected.SelectedIndex < 0 || selected.SelectedIndex >= groups.Count)
                {
                    return;
                }

                Move(entry, groups[selected.SelectedIndex]);
            },
        };
    }

    private IReadOnlyList<MenuEntry> CategoryRows()
    {
        var categories = SavedVehicleStore.Categories();

        var rows = new List<MenuEntry>();

        foreach (var category in categories)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(category.Name),
                Description = MenuText.Literal(category.Description),
            });
        }

        if (rows.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.NoCategories),
                Description = MenuText.Key(Loc.SavedVehicles.NoCategoriesDescription),
            });
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.SavedVehicles.CreateCategory),
            Description = MenuText.Key(Loc.SavedVehicles.CreateCategoryDescription),
            OnSelectedAsync = _ => CreateCategoryAsync(),
        });

        if (categories.Count > 0)
        {
            rows.Add(DeleteCategoryRow(categories));
        }

        return rows;
    }

    private ConfirmListEntry DeleteCategoryRow(List<SavedVehicleCategory> categories)
    {
        var options = new List<MenuText>(categories.Count);

        foreach (var category in categories)
        {
            options.Add(MenuText.Literal(category.Name));
        }

        var picked = 0;

        return new ConfirmListEntry
        {
            Text = MenuText.Key(Loc.SavedVehicles.DeleteCategory),
            Description = MenuText.Key(Loc.SavedVehicles.DeleteCategoryDescription),
            ConfirmationDescription = MenuText.Key(
                Loc.SavedVehicles.DeleteCategoryConfirm,
                ("name", MenuText.From(() => NameAt(categories, picked)))),
            Options = options,
            OnIndexChanged = changed => picked = changed.NewIndex,
            OnConfirmed = confirmed =>
            {
                var name = NameAt(categories, confirmed.SelectedIndex);

                if (name.Length == 0)
                {
                    return;
                }

                SavedVehicleStore.DeleteCategory(name);

                Notifications.Success(MenuText.Key(Loc.SavedVehicles.CategoryDeleted));

                RebuildEverything();
            },
        };
    }

    #endregion

    #region Actions

    private async Task SaveCurrentAsync()
    {
        if (OwnVehicle.RequireDriven(Loc.SavedVehicles.NoVehicle, Loc.SavedVehicles.NotDriver) is not { } vehicle)
        {
            return;
        }

        // Read before the prompt as well as after it, so a player who drives off mid-prompt saves
        // nothing rather than saving the wrong car.
        var appearance = VehicleAppearanceReader.Read(vehicle);

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedVehicles.NamePrompt), NameLength),
            new InputPrompt(MenuText.Key(Loc.SavedVehicles.CategoryPrompt), NameLength)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        Report(
            SavedVehicleStore.Save(
                new SavedVehicle
                {
                    Name = name,
                    Category = answers[1].Trim(),
                    Appearance = appearance,
                },
                replacing: false,
                limit: ClientConfig.Value(SavedVehiclesSettings.MaxSavedVehicles)),
            name);
    }

    private void Replace(SavedVehicleEntry entry)
    {
        if (OwnVehicle.RequireDriven(Loc.SavedVehicles.NoVehicle, Loc.SavedVehicles.NotDriver) is not { } vehicle)
        {
            return;
        }

        entry.Vehicle.Appearance = VehicleAppearanceReader.Read(vehicle);

        var outcome = SavedVehicleStore.Save(entry.Vehicle, replacing: true, limit: 0);

        if (outcome is SaveOutcome.Saved)
        {
            Notifications.Success(MenuText.Key(
                Loc.SavedVehicles.Replaced,
                ("name", MenuText.Literal(entry.Vehicle.Name))));

            RebuildEverything();

            return;
        }

        Report(outcome, entry.Vehicle.Name);
    }

    private static async Task SpawnAsync(SavedVehicleEntry entry)
    {
        var appearance = entry.Vehicle.Appearance;
        var modelName = VehicleModelNames.Resolve(appearance.ModelHash, appearance.ModelName);
        var model = MenuText.Literal(modelName);

        if (!Native.IsModelInCdimage(appearance.ModelHash))
        {
            Notifications.Error(MenuText.Key(Loc.SavedVehicles.SpawnModelMissing, ("model", model)));

            return;
        }

        var vehicleClass = Native.GetVehicleClassFromName(appearance.ModelHash);

        // The saved vehicles menu is not a way around a restricted vehicle list, so the spawner's
        // own rules still apply.
        if (!ClientVehiclePermissions.CanSpawnVehicle(modelName, vehicleClass))
        {
            Notifications.Warning(MenuText.Key(Loc.SavedVehicles.SpawnDenied, ("model", model)));

            return;
        }

        if (await VehicleSpawning.SpawnAsync(appearance.ModelHash) is not { } spawned)
        {
            Notifications.Error(MenuText.Key(Loc.SavedVehicles.SpawnModelMissing, ("model", model)));

            return;
        }

        var name = MenuText.Literal(entry.Vehicle.Name);
        var differences = await VehicleAppearanceWriter.ApplyAsync(spawned, appearance);

        if (differences.Count == 0)
        {
            Notifications.Success(MenuText.Key(Loc.SavedVehicles.RestoredExactly, ("name", name)));

            return;
        }

        Notifications.Warning(MenuText.Key(
            Loc.SavedVehicles.RestoredPartially,
            ("name", name),
            ("count", MenuText.Literal(differences.Count.ToString(CultureInfo.InvariantCulture)))));
    }

    private async Task RenameAsync(SavedVehicleEntry entry)
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.SavedVehicles.RenamePrompt, ("name", MenuText.Literal(entry.Vehicle.Name))),
            NameLength,
            entry.Vehicle.Name);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var name = typed.Trim();

        if (!SavedVehicleStore.Rename(entry.Vehicle, name))
        {
            Notifications.Error(MenuText.Key(Loc.SavedVehicles.NameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.SavedVehicles.Renamed, ("name", MenuText.Literal(name))));

        RebuildEverything();
    }

    private void Move(SavedVehicleEntry entry, string category)
    {
        if (!SavedVehicleStore.MoveToCategory(entry.Vehicle, category))
        {
            Notifications.Error(MenuText.Key(
                Loc.SavedVehicles.OverwriteRefused,
                ("name", MenuText.Literal(entry.Vehicle.Name))));

            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.SavedVehicles.Moved,
            ("name", category.Length == 0
                ? MenuText.Key(Loc.SavedVehicles.Uncategorised)
                : MenuText.Literal(category))));

        RebuildEverything();
    }

    private void Delete(SavedVehicleEntry entry)
    {
        SavedVehicleStore.Delete(entry.Vehicle.Name);

        _selected = null;

        Notifications.Success(MenuText.Key(Loc.SavedVehicles.Deleted));

        RebuildEverything();
    }

    private async Task CreateCategoryAsync()
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedVehicles.CategoryName), NameLength),
            new InputPrompt(MenuText.Key(Loc.SavedVehicles.CategoryDescriptionPrompt), NameLength)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (!SavedVehicleStore.AddCategory(name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.SavedVehicles.CategoryNameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.SavedVehicles.CategoryCreated, ("name", MenuText.Literal(name))));

        RebuildEverything();
    }

    private void Report(SaveOutcome outcome, string name)
    {
        var named = MenuText.Literal(name);

        if (outcome is SaveOutcome.Saved)
        {
            Notifications.Success(MenuText.Key(Loc.SavedVehicles.Saved, ("name", named)));

            RebuildEverything();

            return;
        }

        Notifications.Error(MenuText.Key(
            outcome switch
            {
                SaveOutcome.NameTaken => Loc.SavedVehicles.NameTaken,
                SaveOutcome.Refused => Loc.SavedVehicles.OverwriteRefused,
                SaveOutcome.LimitReached => Loc.SavedVehicles.LimitReached,
                _ => Loc.SavedVehicles.SaveFailed,
            },
            ("name", named),
            ("limit", MenuText.From(() =>
                ClientConfig.Value(SavedVehiclesSettings.MaxSavedVehicles).ToString(CultureInfo.InvariantCulture)))));
    }

    #endregion

    #region Plumbing

    private void RebuildEverything()
    {
        if (_root is { } root)
        {
            Refill(root, RootRows());
        }

        Fill(_vehicleMenu, VehicleRows());
        Fill(_detailMenu, DetailRows());
        Fill(_categoryMenu, CategoryRows());
    }

    private static void Fill(DetachedMenu? menu, IReadOnlyList<MenuEntry> rows)
    {
        if (menu is not { } detached)
        {
            return;
        }

        Refill(detached.Builder, rows);
    }

    // Rebuilding drops every item and MenuAPI puts the highlight back on the first one, which moves
    // the player's selection out from under them.
    private static void Refill(MenuBuilder builder, IReadOnlyList<MenuEntry> rows)
    {
        var was = builder.Menu.CurrentIndex;
        var offset = builder.Menu.ViewIndexOffset;

        builder.ClearEntries();
        builder.AddRange(rows);

        var keep = was < builder.Menu.GetMenuItems().Count;

        builder.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }

    private string CategoryTitle() =>
        _category.Length == 0 ? Localizer.Current.Get(Loc.SavedVehicles.Uncategorised) : _category;

    /// <summary>
    /// Every group with something in it, plus every category that was declared, so a vehicle naming
    /// a category nobody made is still reachable.
    /// </summary>
    private static List<string> GroupNames(List<SavedVehicleEntry> vehicles)
    {
        var names = new List<string>();

        foreach (var entry in vehicles)
        {
            Include(names, GroupOf(entry));
        }

        foreach (var category in SavedVehicleStore.Categories())
        {
            Include(names, category.Name);
        }

        names.Sort(static (left, right) => string.Compare(left, right, StringComparison.OrdinalIgnoreCase));

        // Uncategorised first, so the group everything lands in by default is where people look.
        if (IndexOf(names, string.Empty) is > 0 and var index)
        {
            names.RemoveAt(index);
            names.Insert(0, string.Empty);
        }

        return names;
    }

    private static void Include(List<string> names, string name)
    {
        if (IndexOf(names, name) < 0)
        {
            names.Add(name);
        }
    }

    // By hand rather than List.IndexOf or Contains, which reach for EqualityComparer<string>.Default
    // and the client sandbox refuses to load it.
    private static int IndexOf(List<string> names, string name)
    {
        for (var index = 0; index < names.Count; index++)
        {
            if (string.Equals(names[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>A vehicle's category, with a name nobody declared treated as its own group.</summary>
    private static string GroupOf(SavedVehicleEntry entry) => entry.Vehicle.Category.Trim();

    private static int Count(List<SavedVehicleEntry> vehicles, string group)
    {
        var count = 0;

        foreach (var entry in vehicles)
        {
            if (string.Equals(GroupOf(entry), group, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    private static string NameAt(List<SavedVehicleCategory> categories, int index) =>
        index >= 0 && index < categories.Count ? categories[index].Name : string.Empty;

    #endregion
}
