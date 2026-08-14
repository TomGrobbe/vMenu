using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Players.Saved;
using vMenu.Enhanced.Menus.Saved;

using SavedPedsPermissions = vMenu.Enhanced.Data.Permissions.Menus.SavedPeds;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Peds the player put away, kept on their own machine.
/// </summary>
/// <remarks>
/// Laid out exactly like the saved vehicles menu, because they are the same idea applied to a
/// different thing and a player who has learned one should not have to learn the other. Three
/// levels, one shared child menu each, and category management only at the top.
/// </remarks>
[VMenu(
    TitleKey = Loc.SavedPeds.Title,
    SubtitleKey = Loc.SavedPeds.Subtitle,
    DescriptionKey = Loc.SavedPeds.LinkDescription,
    Permission = SavedPedsPermissions.Menu)]
public sealed class SavedPedsMenu : MenuDefinition
{
    private const int NameLength = 30;

    private const int DescriptionLength = 100;

    private MenuBuilder? _root;

    private DetachedMenu? _pedMenu;

    private DetachedMenu? _detailMenu;

    /// <summary>Empty is the uncategorised group, which is a real place rather than a missing one.</summary>
    private string _category = string.Empty;

    private SavedPedEntry? _selected;

    protected override void Build(MenuBuilder menu)
    {
        _root = menu;

        _pedMenu = menu.AddDetachedMenu(
            MenuText.From(CategoryTitle),
            MenuText.From(CategoryTitle),
            _ => { });

        _pedMenu.Builder.OnOpened = _ => Fill(_pedMenu, PedRows());

        _detailMenu = menu.AddDetachedMenu(
            MenuText.From(() => _selected?.Ped.Name ?? string.Empty),
            MenuText.Key(Loc.SavedPeds.Title),
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
        var categories = SavedPedStore.Categories();

        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedPeds.SaveCurrent),
                Description = MenuText.Key(Loc.SavedPeds.SaveCurrentDescription),
                Gate = SavedPedsPermissions.Save,
                OnSelectedAsync = _ => SaveCurrentAsync(),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedPeds.CreateCategory),
                Description = MenuText.Key(Loc.SavedPeds.CreateCategoryDescription),
                Gate = SavedPedsPermissions.Manage,
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

        var peds = SavedPedStore.All();
        var groups = GroupNames(peds);

        if (groups.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedPeds.NoPeds),
                Description = MenuText.Key(Loc.SavedPeds.NoPedsDescription),
            });

            return rows;
        }

        foreach (var name in groups)
        {
            var group = name;
            var count = Count(peds, group);

            rows.Add(new ButtonEntry
            {
                Text = group.Length == 0 ? MenuText.Key(Loc.SavedPeds.Uncategorised) : MenuText.Literal(group),
                Label = MenuText.Literal($"({count.ToString(CultureInfo.InvariantCulture)})"),
                Description = CategoryDescription(categories, group, count),
                OnSelected = _ =>
                {
                    _category = group;

                    _pedMenu?.Open();
                },
            });
        }

        return rows;
    }

    /// <summary>What the owner wrote about the group, or a count when they wrote nothing.</summary>
    private static MenuText CategoryDescription(List<SavedPedCategory> categories, string group, int count)
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
            Loc.SavedPeds.CategoryRowDescription,
            ("count", MenuText.Literal(count.ToString(CultureInfo.InvariantCulture))));
    }

    private ListEntry EditCategoryRow(List<SavedPedCategory> categories)
    {
        var options = new List<MenuText>(categories.Count);

        foreach (var category in categories)
        {
            options.Add(MenuText.Literal(category.Name));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.SavedPeds.EditCategory),
            Description = MenuText.Key(Loc.SavedPeds.EditCategoryDescription),
            Options = options,
            Gate = SavedPedsPermissions.Manage,
            OnSelectedAsync = selected => EditCategoryAsync(categories, selected.SelectedIndex),
        };
    }

    private ConfirmListEntry DeleteCategoryRow(List<SavedPedCategory> categories)
    {
        var options = new List<MenuText>(categories.Count);

        foreach (var category in categories)
        {
            options.Add(MenuText.Literal(category.Name));
        }

        var picked = 0;

        return new ConfirmListEntry
        {
            Text = MenuText.Key(Loc.SavedPeds.DeleteCategory),
            Description = MenuText.Key(Loc.SavedPeds.DeleteCategoryDescription),
            ConfirmationDescription = MenuText.Key(
                Loc.SavedPeds.DeleteCategoryConfirm,
                ("name", MenuText.From(() => NameAt(categories, picked)))),
            Options = options,
            Gate = SavedPedsPermissions.Manage,
            OnIndexChanged = changed => picked = changed.NewIndex,
            OnConfirmed = confirmed =>
            {
                var name = NameAt(categories, confirmed.SelectedIndex);

                if (name.Length == 0)
                {
                    return;
                }

                SavedPedStore.DeleteCategory(name);

                Notifications.Success(MenuText.Key(Loc.SavedPeds.CategoryDeleted));

                RebuildEverything();
            },
        };
    }

    private IReadOnlyList<MenuEntry> PedRows()
    {
        var rows = new List<MenuEntry>();

        foreach (var entry in SavedPedStore.All())
        {
            if (!string.Equals(GroupOf(entry), _category, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            rows.Add(PedRow(entry));
        }

        if (rows.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedPeds.NoPeds),
                Description = MenuText.Key(Loc.SavedPeds.NoPedsDescription),
            });
        }

        return rows;
    }

    private ButtonEntry PedRow(SavedPedEntry entry)
    {
        var appearance = entry.Ped.Appearance;
        var available = Native.IsModelInCdimage(appearance.ModelHash);

        return new ButtonEntry
        {
            Text = MenuText.Literal(entry.Ped.Name),
            Label = entry.IsFromNewerBuild
                ? MenuText.Key(Loc.SavedPeds.NewerBuildLabel)
                : ModelName(appearance),
            Description = Describe(entry, available),
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
    private static MenuText Describe(SavedPedEntry entry, bool available)
    {
        if (entry.IsFromNewerBuild)
        {
            return MenuText.Key(Loc.SavedPeds.NewerBuildDescription);
        }

        var model = ModelName(entry.Ped.Appearance);

        if (!available)
        {
            return MenuText.Key(Loc.SavedPeds.ModelUnavailable, ("model", model));
        }

        return entry.Ped.Description.Length > 0
            ? MenuText.Literal(entry.Ped.Description)
            : MenuText.Key(Loc.SavedPeds.PedRowDescription, ("model", model));
    }

    /// <summary>
    /// The model's name, or a stand-in when this client has no name for it.
    /// </summary>
    // The game has no reverse lookup for a ped model, so a ped saved on a server that listed the
    // model and then opened on one that did not has only its hash left. Late bound, because the
    // ped list can arrive after the menu was built.
    private static MenuText ModelName(PedAppearance appearance) => MenuText.From(() =>
    {
        var resolved = PedModelNames.Resolve(appearance.ModelHash, appearance.ModelName);

        return resolved.Length > 0
            ? resolved
            : Localizer.Current.Get(Loc.SavedPeds.UnnamedModel);
    });

    private IReadOnlyList<MenuEntry> DetailRows()
    {
        if (_selected is not { } entry)
        {
            return
            [
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.SavedPeds.NoPeds),
                    Description = MenuText.Key(Loc.SavedPeds.NoPedsDescription),
                },
            ];
        }

        var name = MenuText.Literal(entry.Ped.Name);

        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedPeds.Spawn),
                Description = MenuText.Key(Loc.SavedPeds.SpawnDescription),
                Gate = SavedPedsPermissions.Spawn,
                OnSelectedAsync = _ => SpawnAsync(entry),
            },
            EditRow(entry),
            MoveRow(entry),
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedPeds.Duplicate),
                Description = MenuText.Key(Loc.SavedPeds.DuplicateDescription),
                Gate = SavedPedsPermissions.Save,
                OnSelectedAsync = _ => DuplicateAsync(entry),
            },
        };

        // Overwriting a save this build cannot fully read would silently drop whatever the newer
        // version put in it, so the row becomes an offer to save alongside it instead.
        if (entry.IsFromNewerBuild)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedPeds.SaveAsNew),
                Description = MenuText.Key(Loc.SavedPeds.SaveAsNewDescription),
                Gate = SavedPedsPermissions.Save,
                OnSelectedAsync = _ => SaveCurrentAsync(),
            });
        }
        else
        {
            rows.Add(new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.SavedPeds.Replace),
                Description = MenuText.Key(Loc.SavedPeds.ReplaceDescription),
                ConfirmationDescription = MenuText.Key(Loc.SavedPeds.ReplaceConfirm, ("name", name)),
                Gate = SavedPedsPermissions.Manage,
                OnConfirmed = _ => Replace(entry),
            });
        }

        rows.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.SavedPeds.Delete),
            Description = MenuText.Key(Loc.SavedPeds.DeleteDescription),
            ConfirmationDescription = MenuText.Key(Loc.SavedPeds.DeleteConfirm, ("name", name)),
            Gate = SavedPedsPermissions.Manage,
            OnConfirmed = _ => Delete(entry),
        });

        return rows;
    }

    /// <summary>Renaming and re-describing. Locked for a save this build cannot fully read.</summary>
    // Editing rewrites the whole save, so a newer build's extra fields would be dropped on the way
    // through. Locked rather than hidden, so the reason is on screen.
    private ButtonEntry EditRow(SavedPedEntry entry) => entry.IsFromNewerBuild
        ? new ButtonEntry
        {
            Text = MenuText.Key(Loc.SavedPeds.Edit),
            Description = MenuText.Key(Loc.SavedPeds.NewerBuildDescription),
            Gate = MenuGate.Never,
        }
        : new ButtonEntry
        {
            Text = MenuText.Key(Loc.SavedPeds.Edit),
            Description = MenuText.Key(Loc.SavedPeds.EditDescription),
            Gate = SavedPedsPermissions.Manage,
            OnSelectedAsync = _ => EditAsync(entry),
        };

    private ListEntry MoveRow(SavedPedEntry entry)
    {
        var groups = GroupNames(SavedPedStore.All());

        var options = new List<MenuText>(groups.Count);

        foreach (var group in groups)
        {
            options.Add(group.Length == 0 ? MenuText.Key(Loc.SavedPeds.Uncategorised) : MenuText.Literal(group));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.SavedPeds.MoveToCategory),
            Description = MenuText.Key(Loc.SavedPeds.MoveToCategoryDescription),
            Options = options,
            Gate = SavedPedsPermissions.Manage,
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
        if (RefuseFreemode())
        {
            return;
        }

        // Read before the prompt as well as after it, so a player who changes ped mid-prompt saves
        // nothing rather than saving the wrong one.
        var appearance = PedAppearanceReader.Read(Native.PlayerPedId());

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedPeds.NamePrompt), NameLength),
            new InputPrompt(MenuText.Key(Loc.SavedPeds.DescriptionPrompt), DescriptionLength),
            new InputPrompt(MenuText.Key(Loc.SavedPeds.CategoryPrompt), NameLength)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (RefuseFreemode())
        {
            return;
        }

        Report(
            SavedPedStore.Save(
                new SavedPed
                {
                    Name = name,
                    Description = answers[1].Trim(),
                    Category = answers[2].Trim(),
                    Appearance = appearance,
                    MovementClipset = PedWalkingStyle.Current,
                },
                replacing: false),
            name);
    }

    /// <summary>
    /// Turns away a freemode ped, because only its clothes would be stored.
    /// </summary>
    // Its face, hair colour, overlays and tattoos all live in the character creator, which is not
    // ported yet. Saving one now would look like it worked and come back grey and blank later.
    private static bool RefuseFreemode()
    {
        if (!PedSpawning.IsWearingFreemode())
        {
            return false;
        }

        Notifications.Warning(MenuText.Key(Loc.SavedPeds.FreemodeRefused));

        return true;
    }

    private void Replace(SavedPedEntry entry)
    {
        if (RefuseFreemode())
        {
            return;
        }

        entry.Ped.Appearance = PedAppearanceReader.Read(Native.PlayerPedId());
        entry.Ped.MovementClipset = PedWalkingStyle.Current;

        var outcome = SavedPedStore.Save(entry.Ped, replacing: true);

        if (outcome is SaveOutcome.Saved)
        {
            Notifications.Success(MenuText.Key(
                Loc.SavedPeds.Replaced,
                ("name", MenuText.Literal(entry.Ped.Name))));

            RebuildEverything();

            return;
        }

        Report(outcome, entry.Ped.Name);
    }

    private static async Task SpawnAsync(SavedPedEntry entry)
    {
        var appearance = entry.Ped.Appearance;
        var modelName = PedModelNames.Resolve(appearance.ModelHash, appearance.ModelName);
        var model = MenuText.Literal(modelName);

        if (!Native.IsModelInCdimage(appearance.ModelHash))
        {
            Notifications.Error(MenuText.Key(Loc.SavedPeds.SpawnModelMissing, ("model", model)));

            return;
        }

        // Re-checked here as well as on the row's gate, because a permission refresh can land between
        // the two, and because the saved peds menu is not a way around a restricted ped list.
        if (!PedSpawning.IsPermitted(modelName))
        {
            Notifications.Warning(MenuText.Key(Loc.SavedPeds.SpawnDenied, ("model", model)));

            return;
        }

        // By hash rather than by name, because a model the server owner never listed has no name this
        // client could turn back into one.
        if (!await PedSpawning.SetPlayerModelAsync(appearance.ModelHash))
        {
            Notifications.Error(MenuText.Key(Loc.SavedPeds.SpawnModelMissing, ("model", model)));

            return;
        }

        var name = MenuText.Literal(entry.Ped.Name);
        var differences = await PedAppearanceWriter.ApplyAsync(Native.PlayerPedId(), appearance);

        // After the clothes, and after the model swap put the remembered walk back, so the one this
        // ped was saved with is the one that ends up on it.
        await PedWalkingStyle.ApplyAsync(entry.Ped.MovementClipset);

        if (differences.Count == 0)
        {
            Notifications.Success(
                MenuText.Key(Loc.SavedPeds.RestoredExactly, ("name", name)),
                Notifications.SpawnDurationMs);

            return;
        }

        Notifications.Warning(
            MenuText.Key(
                Loc.SavedPeds.RestoredPartially,
                ("name", name),
                ("count", MenuText.Literal(differences.Count.ToString(CultureInfo.InvariantCulture)))),
            Notifications.SpawnDurationMs);
    }

    private async Task EditAsync(SavedPedEntry entry)
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedPeds.NamePrompt), NameLength, entry.Ped.Name),
            new InputPrompt(
                MenuText.Key(Loc.SavedPeds.DescriptionPrompt),
                DescriptionLength,
                entry.Ped.Description)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (!SavedPedStore.Edit(entry, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.SavedPeds.NameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.SavedPeds.Edited, ("name", MenuText.Literal(name))));

        RebuildEverything();
    }

    private async Task DuplicateAsync(SavedPedEntry entry)
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.SavedPeds.DuplicatePrompt, ("name", MenuText.Literal(entry.Ped.Name))),
            NameLength,
            entry.Ped.Name);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var name = typed.Trim();
        var outcome = SavedPedStore.Duplicate(entry, name);

        if (outcome is not SaveOutcome.Saved)
        {
            Report(outcome, name);

            return;
        }

        Notifications.Success(MenuText.Key(Loc.SavedPeds.Duplicated, ("name", MenuText.Literal(name))));

        RebuildEverything();
    }

    private void Move(SavedPedEntry entry, string category)
    {
        if (!SavedPedStore.MoveToCategory(entry.Ped, category))
        {
            Notifications.Error(MenuText.Key(
                Loc.SavedPeds.OverwriteRefused,
                ("name", MenuText.Literal(entry.Ped.Name))));

            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.SavedPeds.Moved,
            ("name", category.Length == 0
                ? MenuText.Key(Loc.SavedPeds.Uncategorised)
                : MenuText.Literal(category))));

        RebuildEverything();
    }

    private void Delete(SavedPedEntry entry)
    {
        SavedPedStore.Delete(entry.Ped.Name);

        _selected = null;

        Notifications.Success(MenuText.Key(Loc.SavedPeds.Deleted));

        RebuildEverything();
    }

    private async Task CreateCategoryAsync()
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedPeds.CategoryName), NameLength),
            new InputPrompt(MenuText.Key(Loc.SavedPeds.CategoryDescriptionPrompt), DescriptionLength)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (!SavedPedStore.AddCategory(name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.SavedPeds.CategoryNameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.SavedPeds.CategoryCreated, ("name", MenuText.Literal(name))));

        RebuildEverything();
    }

    private async Task EditCategoryAsync(List<SavedPedCategory> categories, int index)
    {
        if (index < 0 || index >= categories.Count)
        {
            return;
        }

        var category = categories[index];

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedPeds.CategoryName), NameLength, category.Name),
            new InputPrompt(
                MenuText.Key(Loc.SavedPeds.CategoryDescriptionPrompt),
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

        if (!SavedPedStore.EditCategory(category.Name, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.SavedPeds.CategoryNameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        // The player may be standing in a category that just changed its name, and the ped menu
        // filters on that name.
        if (string.Equals(_category, category.Name, StringComparison.OrdinalIgnoreCase))
        {
            _category = name;
        }

        Notifications.Success(MenuText.Key(Loc.SavedPeds.CategoryEdited, ("name", MenuText.Literal(name))));

        RebuildEverything();
    }

    private void Report(SaveOutcome outcome, string name)
    {
        var named = MenuText.Literal(name);

        if (outcome is SaveOutcome.Saved)
        {
            Notifications.Success(MenuText.Key(Loc.SavedPeds.Saved, ("name", named)));

            RebuildEverything();

            return;
        }

        Notifications.Error(MenuText.Key(
            outcome switch
            {
                SaveOutcome.NameTaken => Loc.SavedPeds.NameTaken,
                SaveOutcome.Refused => Loc.SavedPeds.OverwriteRefused,
                _ => Loc.SavedPeds.SaveFailed,
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

        Fill(_pedMenu, PedRows());
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
        _category.Length == 0 ? Localizer.Current.Get(Loc.SavedPeds.Uncategorised) : _category;

    /// <summary>
    /// Every group with something in it, plus every category that was declared, so a ped naming a
    /// category nobody made is still reachable.
    /// </summary>
    private static List<string> GroupNames(List<SavedPedEntry> peds)
    {
        var names = new List<string>();

        foreach (var entry in peds)
        {
            Include(names, GroupOf(entry));
        }

        foreach (var category in SavedPedStore.Categories())
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

    /// <summary>A ped's category, with a name nobody declared treated as its own group.</summary>
    private static string GroupOf(SavedPedEntry entry) => entry.Ped.Category.Trim();

    private static int Count(List<SavedPedEntry> peds, string group)
    {
        var count = 0;

        foreach (var entry in peds)
        {
            if (string.Equals(GroupOf(entry), group, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    private static string NameAt(List<SavedPedCategory> categories, int index) =>
        index >= 0 && index < categories.Count ? categories[index].Name : string.Empty;

    #endregion
}
