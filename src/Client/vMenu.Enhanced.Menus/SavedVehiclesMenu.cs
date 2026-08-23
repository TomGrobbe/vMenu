using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Vehicles.Appearance;
using vMenu.Enhanced.Menus.Vehicles.Saved;
using vMenu.Enhanced.Permissions;

using SavedVehiclesPermissions = vMenu.Enhanced.Data.Permissions.Menus.SavedVehicles;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Vehicles the player put away, kept on their own machine.
/// </summary>
/// <remarks>
/// Three levels: the categories, the vehicles in one of them, and what can be done to one vehicle.
/// Categories are made, renamed and deleted from the top level only, because managing a category
/// from inside it is how you end up deleting the page you are standing on.
///
/// <para>
/// Laid out like the teleport menu: one shared child menu per level rather than a menu per row. The
/// rows are runtime data and there could be a great many of them, and MenuAPI cannot take a menu back
/// out once it has been added.
/// </para>
/// </remarks>
[VMenu(
    TitleKey = Loc.SavedVehicles.Title,
    SubtitleKey = Loc.SavedVehicles.Subtitle,
    DescriptionKey = Loc.SavedVehicles.LinkDescription,
    Permission = SavedVehiclesPermissions.Menu)]
public sealed class SavedVehiclesMenu : MenuDefinition
{
    private const int NameLength = 30;

    private const int DescriptionLength = 100;

    private MenuBuilder? _root;

    private DetachedMenu? _vehicleMenu;

    private DetachedMenu? _detailMenu;

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

        menu.AddRange(RootRows());

        // The store changes from inside this menu, so the rows are rebuilt whenever the player comes
        // back up to this level.
        menu.OnOpened = _ => Refill(menu, RootRows());
    }

    #region Rows

    private IReadOnlyList<MenuEntry> RootRows()
    {
        var vehicles = SavedVehicleStore.All();
        var groups = GroupNames(vehicles);

        // Every category the player can see, whether they made it with Create Category or a saved car
        // simply names one, so all of them can be renamed and deleted, not only the declared ones.
        var categories = ManageableCategories(groups);

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
                Text = MenuText.Key(Loc.SavedVehicles.CreateCategory),
                Description = MenuText.Key(Loc.SavedVehicles.CreateCategoryDescription),
                Gate = SavedVehiclesPermissions.Manage,
                OnSelectedAsync = _ => CreateCategoryAsync(),
            },
        };

        // Nothing to pick from until a category exists, and a list row with no options is a row that
        // cannot do anything.
        if (categories.Count > 0)
        {
            rows.Add(EditCategoryRow(categories));
            rows.Add(DeleteCategoryRow(categories));
        }

        if (groups.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.NoVehicles),
                Description = MenuText.Key(Loc.SavedVehicles.NoVehiclesDescription),
            });

            return rows;
        }

        foreach (var name in groups)
        {
            var group = name;
            var count = Count(vehicles, group);

            rows.Add(new ButtonEntry
            {
                Text = group.Length == 0 ? MenuText.Key(Loc.SavedVehicles.Uncategorised) : MenuText.Literal(group),
                Label = MenuText.Literal($"({count.ToString(CultureInfo.InvariantCulture)})"),
                Description = CategoryDescription(categories, group, count),
                OnSelected = _ =>
                {
                    _category = group;

                    _vehicleMenu?.Open();
                },
            });
        }

        return rows;
    }

    /// <summary>What the owner wrote about the group, or a count when they wrote nothing.</summary>
    private static MenuText CategoryDescription(List<SavedVehicleCategory> categories, string group, int count)
    {
        foreach (var category in categories)
        {
            if (string.Equals(category.Name, group, StringComparison.OrdinalIgnoreCase)
                && category.Description.Length > 0)
            {
                return MenuText.Literal(category.Description);
            }
        }

        return MenuText.Key(
            Loc.SavedVehicles.CategoryRowDescription,
            ("count", MenuText.Literal(count.ToString(CultureInfo.InvariantCulture))));
    }

    private ListEntry EditCategoryRow(List<SavedVehicleCategory> categories)
    {
        var options = new List<MenuText>(categories.Count);

        foreach (var category in categories)
        {
            options.Add(MenuText.Literal(category.Name));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.SavedVehicles.EditCategory),
            Description = MenuText.Key(Loc.SavedVehicles.EditCategoryDescription),
            Options = options,
            Gate = SavedVehiclesPermissions.Manage,
            OnSelectedAsync = selected => EditCategoryAsync(categories, selected.SelectedIndex),
        };
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
            Gate = SavedVehiclesPermissions.Manage,
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

    /// <summary>
    /// What the player wrote about this one, falling back to the model when they wrote nothing.
    /// </summary>
    // Their own words win over ours: a description is only there because somebody typed it, so it is
    // the more useful of the two.
    private static MenuText Describe(SavedVehicleEntry entry, bool available, MenuText model)
    {
        if (entry.IsFromNewerBuild)
        {
            return MenuText.Key(Loc.SavedVehicles.NewerBuildDescription);
        }

        if (!available)
        {
            return MenuText.Key(Loc.SavedVehicles.ModelUnavailable, ("model", model));
        }

        return entry.Vehicle.Description.Length > 0
            ? MenuText.Literal(entry.Vehicle.Description)
            : MenuText.Key(Loc.SavedVehicles.VehicleRowDescription, ("model", model));
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
            EditRow(entry),
            MoveRow(entry),
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedVehicles.Duplicate),
                Description = MenuText.Key(Loc.SavedVehicles.DuplicateDescription),
                Gate = SavedVehiclesPermissions.Save,
                OnSelectedAsync = _ => DuplicateAsync(entry),
            },
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

    /// <summary>Renaming and re-describing. Locked for a save this build cannot fully read.</summary>
    // Editing rewrites the whole save, so a newer build's extra fields would be dropped on the way
    // through. Locked rather than hidden, so the reason is on screen.
    private ButtonEntry EditRow(SavedVehicleEntry entry) => entry.IsFromNewerBuild
        ? new ButtonEntry
        {
            Text = MenuText.Key(Loc.SavedVehicles.Edit),
            Description = MenuText.Key(Loc.SavedVehicles.NewerBuildDescription),
            Gate = MenuGate.Never,
        }
        : new ButtonEntry
        {
            Text = MenuText.Key(Loc.SavedVehicles.Edit),
            Description = MenuText.Key(Loc.SavedVehicles.EditDescription),
            Gate = SavedVehiclesPermissions.Manage,
            OnSelectedAsync = _ => EditAsync(entry),
        };

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
            new InputPrompt(MenuText.Key(Loc.SavedVehicles.DescriptionPrompt), DescriptionLength),
            new InputPrompt(
                MenuText.Key(Loc.SavedVehicles.CategoryPrompt),
                NameLength,
                suggestions: CategorySuggestions(),
                suggestWhenEmpty: true)) is not { } answers)
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
                    Description = answers[1].Trim(),
                    Category = answers[2].Trim(),
                    Appearance = appearance,
                },
                replacing: false),
            name);
    }

    private void Replace(SavedVehicleEntry entry)
    {
        if (OwnVehicle.RequireDriven(Loc.SavedVehicles.NoVehicle, Loc.SavedVehicles.NotDriver) is not { } vehicle)
        {
            return;
        }

        entry.Vehicle.Appearance = VehicleAppearanceReader.Read(vehicle);

        var outcome = SavedVehicleStore.Save(entry.Vehicle, replacing: true);

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

        if (!VehicleSpawnLimit.TryTakeOrWarn())
        {
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
            Notifications.Success(
                MenuText.Key(Loc.SavedVehicles.RestoredExactly, ("name", name)),
                Notifications.SpawnDurationMs);

            return;
        }

        Notifications.Warning(
            MenuText.Key(
                Loc.SavedVehicles.RestoredPartially,
                ("name", name),
                ("count", MenuText.Literal(differences.Count.ToString(CultureInfo.InvariantCulture)))),
            Notifications.SpawnDurationMs);
    }

    private async Task EditAsync(SavedVehicleEntry entry)
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedVehicles.NamePrompt), NameLength, entry.Vehicle.Name),
            new InputPrompt(
                MenuText.Key(Loc.SavedVehicles.DescriptionPrompt),
                DescriptionLength,
                entry.Vehicle.Description)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (!SavedVehicleStore.Edit(entry, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.SavedVehicles.NameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.SavedVehicles.Edited, ("name", MenuText.Literal(name))));

        RebuildEverything();
    }

    private async Task DuplicateAsync(SavedVehicleEntry entry)
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.SavedVehicles.DuplicatePrompt, ("name", MenuText.Literal(entry.Vehicle.Name))),
            NameLength,
            entry.Vehicle.Name);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var name = typed.Trim();
        var outcome = SavedVehicleStore.Duplicate(entry, name);

        if (outcome is not SaveOutcome.Saved)
        {
            Report(outcome, name);

            return;
        }

        Notifications.Success(MenuText.Key(Loc.SavedVehicles.Duplicated, ("name", MenuText.Literal(name))));

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

        // The vehicle this page was about is gone, so its detail menu now has nothing and no title.
        // Step back to the list it came from rather than leave the player on an empty page.
        _detailMenu?.Menu.GoBack();

        // If that was the last vehicle in the category, the list behind this one is now empty. Step
        // back once more rather than drop the player onto a blank page. An undeclared category has
        // also just disappeared from the root menu, so there would be nothing to come back to.
        if (Count(SavedVehicleStore.All(), _category) == 0)
        {
            _vehicleMenu?.Menu.GoBack();
        }
    }

    private async Task CreateCategoryAsync()
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedVehicles.CategoryName), NameLength),
            new InputPrompt(MenuText.Key(Loc.SavedVehicles.CategoryDescriptionPrompt), DescriptionLength)) is not { } answers)
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

    private async Task EditCategoryAsync(List<SavedVehicleCategory> categories, int index)
    {
        if (index < 0 || index >= categories.Count)
        {
            return;
        }

        var category = categories[index];

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedVehicles.CategoryName), NameLength, category.Name),
            new InputPrompt(
                MenuText.Key(Loc.SavedVehicles.CategoryDescriptionPrompt),
                DescriptionLength,
                category.Description)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (!SavedVehicleStore.EditCategory(category.Name, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.SavedVehicles.CategoryNameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        // The player may be standing in a category that just changed its name, and the vehicle menu
        // filters on that name.
        if (string.Equals(_category, category.Name, StringComparison.OrdinalIgnoreCase))
        {
            _category = name;
        }

        Notifications.Success(MenuText.Key(Loc.SavedVehicles.CategoryEdited, ("name", MenuText.Literal(name))));

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
                _ => Loc.SavedVehicles.SaveFailed,
            },
            ("name", named)));
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

    private static IReadOnlyList<InputSuggestion> CategorySuggestions()
    {
        var vehicles = SavedVehicleStore.All();
        var categories = ManageableCategories(GroupNames(vehicles));
        var rows = new InputSuggestion[categories.Count];

        for (var index = 0; index < rows.Length; index++)
        {
            var category = categories[index];

            rows[index] = new InputSuggestion
            {
                Value = category.Name,
                Label = $"({Count(vehicles, category.Name).ToString(CultureInfo.InvariantCulture)})",
                Detail = category.Description,
            };
        }

        return rows;
    }

    /// <summary>
    /// Every named category the player can act on, from the same groups the menu shows. A declared
    /// one keeps its description; a category a saved car merely names gets an empty one, and editing
    /// or deleting it works all the same, since neither reads a stored category to do its job.
    /// </summary>
    // Uncategorised is left out: it is the absence of a category, not one to rename or remove.
    private static List<SavedVehicleCategory> ManageableCategories(List<string> groups)
    {
        var declared = SavedVehicleStore.Categories();
        var result = new List<SavedVehicleCategory>();

        foreach (var name in groups)
        {
            if (name.Length == 0)
            {
                continue;
            }

            var description = string.Empty;

            foreach (var category in declared)
            {
                if (string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    description = category.Description;

                    break;
                }
            }

            result.Add(new SavedVehicleCategory { Name = name, Description = description });
        }

        return result;
    }

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
