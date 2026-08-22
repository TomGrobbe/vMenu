using System.Globalization;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Menus.Players.Character;

internal sealed class SavedCharacters
{
    private const int NameLength = 50;

    private const int DescriptionLength = 200;

    private CharacterBuilder? _builder;

    private DetachedMenu? _listPage;

    private DetachedMenu? _detailPage;

    private DetachedMenu? _variantPage;

    private DetachedMenu? _variantDetailPage;

    private DetachedMenu? _editChoicePage;

    private MenuBuilder? _root;

    private MenuBuilder? _list;

    private MenuBuilder? _detail;

    private MenuBuilder? _variants;

    private MenuBuilder? _variantDetail;

    private string _category = string.Empty;

    private MpCharacterEntry? _selected;

    private bool _showingOutfits;

    private int _variant;

    internal void Attach(MenuBuilder parent, CharacterBuilder builder)
    {
        _builder = builder;

        _editChoicePage = parent.AddDetachedMenu(
            MenuText.Key(Loc.CharacterCreator.EditCharacter),
            MenuText.From(() => _selected?.Character.Name ?? string.Empty),
            BuildEditChoice,
            CharacterCreatorPermissions.Create);

        _variantDetailPage = parent.AddDetachedMenu(
            MenuText.From(() => VariantName(_selected, _variant)),
            MenuText.From(() => VariantName(_selected, _variant)),
            BuildVariantDetail,
            CharacterCreatorPermissions.Spawn);

        _variantPage = parent.AddDetachedMenu(
            MenuText.From(() => CharacterEdit.Resolve(MenuText.Key(
                _showingOutfits ? Loc.CharacterCreator.Outfits : Loc.CharacterCreator.Styles))),
            MenuText.From(() => _selected?.Character.Name ?? string.Empty),
            BuildVariants,
            CharacterCreatorPermissions.Spawn);

        _detailPage = parent.AddDetachedMenu(
            MenuText.From(() => _selected?.Character.Name ?? string.Empty),
            MenuText.From(() => _selected?.Character.Name ?? string.Empty),
            BuildDetail,
            CharacterCreatorPermissions.Spawn);

        _listPage = parent.AddDetachedMenu(
            MenuText.From(CategoryTitle),
            MenuText.From(CategoryTitle),
            BuildList,
            CharacterCreatorPermissions.Spawn);
    }

    internal void ShowAfterEditing(string? name)
    {
        if (name is null || MpCharacterStore.Load(name) is not { } entry)
        {
            Rebuild();

            MenuController.CloseAllMenus();

            return;
        }

        _selected = entry;
        _category = entry.Character.Category;

        Rebuild();

        _detailPage?.Open();

        if (_detailPage is { } detail && _listPage is { } list)
        {
            MenuController.AddSubmenu(list.Menu, detail.Menu);

            if (_root is { } root)
            {
                MenuController.AddSubmenu(root.Menu, list.Menu);
            }
        }
    }

    internal void Rebuild()
    {
        if (_root is { } root)
        {
            Fill(root, RootRows());
        }

        if (_list is { } list)
        {
            Fill(list, ListRows());
        }

        if (_detail is { } detail)
        {
            Fill(detail, DetailRows());
        }

        if (_variants is { } variants)
        {
            Fill(variants, VariantRows());
        }

        if (_variantDetail is { } variantDetail)
        {
            Fill(variantDetail, VariantDetailRows());
        }
    }

    #region Root

    internal void BuildRoot(MenuBuilder menu)
    {
        _root = menu;

        menu.AddRange(RootRows());

        menu.OnOpened = _ => Fill(menu, RootRows());
    }

    private IReadOnlyList<MenuEntry> RootRows()
    {
        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.CreateCategory),
                Description = MenuText.Key(Loc.CharacterCreator.CreateCategoryDescription),
                Gate = CharacterCreatorPermissions.Manage,
                OnSelectedAsync = _ => CreateCategoryAsync(),
            },
        };

        var categories = MpCharacterStore.Categories();
        var characters = MpCharacterStore.All();

        if (categories.Count > 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.EditCategory),
                Description = MenuText.Key(Loc.CharacterCreator.EditCategoryDescription),
                Gate = CharacterCreatorPermissions.Manage,
                OnSelectedAsync = _ => EditCategoryAsync(categories),
            });

            rows.Add(DeleteCategoryRow(categories));
        }

        if (characters.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.NoCharacters),
                Description = MenuText.Key(Loc.CharacterCreator.NoCharactersDescription),
            });

            return rows;
        }

        AddGroup(rows, string.Empty, characters);

        foreach (var category in categories)
        {
            AddGroup(rows, category.Name, characters);
        }

        return rows;
    }

    private ConfirmListEntry DeleteCategoryRow(List<MpCharacterCategory> categories)
    {
        var picked = 0;

        return new ConfirmListEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.DeleteCategory),
            Description = MenuText.Key(Loc.CharacterCreator.DeleteCategoryDescription),
            ConfirmationDescription = MenuText.Key(
                Loc.CharacterCreator.DeleteCategoryConfirm,
                ("name", MenuText.From(() => NameAt(categories, picked)))),
            Options = Names(categories),
            Gate = CharacterCreatorPermissions.Manage,
            OnIndexChanged = changed => picked = changed.NewIndex,
            OnConfirmed = confirmed => DeleteCategory(categories, confirmed.SelectedIndex),
        };
    }

    private static string NameAt(List<MpCharacterCategory> categories, int index) =>
        index >= 0 && index < categories.Count ? categories[index].Name : string.Empty;

    private void AddGroup(List<MenuEntry> rows, string category, List<MpCharacterEntry> characters)
    {
        var count = Count(characters, category);

        if (count == 0 && category.Length == 0)
        {
            return;
        }

        var name = category.Length == 0
            ? MenuText.Key(Loc.CharacterCreator.Uncategorised)
            : MenuText.Literal(category);

        var group = category;

        rows.Add(new ButtonEntry
        {
            Text = name,
            Label = MenuText.Literal("(" + count.ToString(CultureInfo.InvariantCulture) + ")"),
            Description = MenuText.Key(Loc.CharacterCreator.CategoryRowDescription, ("name", name)),
            Gate = CharacterCreatorPermissions.Spawn,
            OnSelected = _ =>
            {
                _category = group;

                _listPage?.Open();
            },
        });
    }

    #endregion

    #region One category

    private void BuildList(MenuBuilder menu)
    {
        _list = menu;

        menu.AddRange(ListRows());

        menu.OnOpened = _ => Fill(menu, ListRows());
    }

    private IReadOnlyList<MenuEntry> ListRows()
    {
        var rows = new List<MenuEntry>();
        var defaultName = UserDefaults.DefaultCharacterName.Value;

        foreach (var entry in MpCharacterStore.All())
        {
            if (!string.Equals(entry.Character.Category, _category, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var character = entry;
            var name = MenuText.Literal(character.Character.Name);

            rows.Add(new ButtonEntry
            {
                Text = name,

                LeftIcon = character.Character.Core.IsMale ? MenuItem.Icon.MALE : MenuItem.Icon.FEMALE,
                ReadRightIcon = () => IsDefault(character) ? MenuItem.Icon.TICK : MenuItem.Icon.NONE,
                Description = Describe(character, defaultName),
                Gate = CharacterCreatorPermissions.Spawn,
                OnSelected = _ =>
                {
                    _selected = character;

                    _detailPage?.Open();
                },
            });
        }

        if (rows.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.NoCharacters),
                Description = MenuText.Key(Loc.CharacterCreator.NoCharactersDescription),
            });
        }

        return rows;
    }

    private static bool IsDefault(MpCharacterEntry entry) =>
        string.Equals(UserDefaults.DefaultCharacterName.Value, entry.Character.Name, StringComparison.Ordinal);

    private static MenuText Describe(MpCharacterEntry entry, string defaultName)
    {
        var name = MenuText.Literal(entry.Character.Name);

        if (entry.IsFromNewerBuild)
        {
            return MenuText.Key(Loc.CharacterCreator.FromNewerBuild);
        }

        return string.Equals(entry.Character.Name, defaultName, StringComparison.Ordinal)
            ? MenuText.Key(Loc.CharacterCreator.IsDefault)
            : MenuText.Key(Loc.CharacterCreator.CharacterRowDescription, ("name", name));
    }

    #endregion

    #region One character

    private void BuildDetail(MenuBuilder menu)
    {
        _detail = menu;

        menu.AddRange(DetailRows());

        menu.OnOpened = _ => Fill(menu, DetailRows());
    }

    private IReadOnlyList<MenuEntry> DetailRows()
    {
        if (_selected is not { } entry)
        {
            return [];
        }

        var name = MenuText.Literal(entry.Character.Name);

        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.Spawn),
                Description = MenuText.Key(Loc.CharacterCreator.SpawnDescription),
                Gate = CharacterCreatorPermissions.Spawn,
                OnSelectedAsync = _ => SpawnAsync(entry),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.EditCharacter),
                Description = MenuText.Key(Loc.CharacterCreator.EditCharacterDescription),
                LockedDescription = MenuText.Key(Loc.CharacterCreator.FromNewerBuild),
                Gate = MenuGate.Permission(CharacterCreatorPermissions.Create)
                    & MenuGate.When(() => !entry.IsFromNewerBuild),
                Behaviour = GateBehaviour.Lock,
                OnSelectedAsync = _ => EditAsync(entry),
            },
            VariantsRow(Loc.CharacterCreator.Outfits, Loc.CharacterCreator.OutfitsDescription, outfits: true),
            VariantsRow(Loc.CharacterCreator.Styles, Loc.CharacterCreator.StylesDescription, outfits: false),
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.Clone),
                Description = MenuText.Key(Loc.CharacterCreator.CloneDescription),
                Gate = CharacterCreatorPermissions.Manage,
                OnSelectedAsync = _ => CloneAsync(entry),
            },
            CategoryRow(entry),
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.SetDefault),
                Description = MenuText.Key(Loc.CharacterCreator.SetDefaultDescription),
                ReadRightIcon = () => IsDefault(entry) ? MenuItem.Icon.TICK : MenuItem.Icon.NONE,
                Gate = CharacterCreatorPermissions.SetDefault,
                OnSelected = _ => ToggleDefault(entry),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.Rename),
                Description = MenuText.Key(Loc.CharacterCreator.RenameDescription),
                LockedDescription = MenuText.Key(Loc.CharacterCreator.FromNewerBuild),
                Gate = MenuGate.Permission(CharacterCreatorPermissions.Manage)
                    & MenuGate.When(() => !entry.IsFromNewerBuild),
                Behaviour = GateBehaviour.Lock,
                OnSelectedAsync = _ => RenameAsync(entry),
            },
            new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.Delete),
                Description = MenuText.Key(Loc.CharacterCreator.DeleteDescription),
                ConfirmationDescription = MenuText.Key(Loc.CharacterCreator.DeleteConfirm, ("name", name)),
                Gate = CharacterCreatorPermissions.Manage,
                OnConfirmed = _ => Delete(entry),
            },
        };

        return rows;
    }

    private ButtonEntry VariantsRow(string name, string description, bool outfits) => new()
    {
        Text = MenuText.Key(name),
        Description = MenuText.Key(description),
        Label = MenuText.Literal("→"),
        Gate = CharacterCreatorPermissions.Spawn,
        OnSelected = _ =>
        {
            _showingOutfits = outfits;

            _variantPage?.Open();
        },
    };

    private ListEntry CategoryRow(MpCharacterEntry entry)
    {
        var names = new List<string> { string.Empty };
        var options = new List<MenuText> { MenuText.Key(Loc.CharacterCreator.Uncategorised) };

        foreach (var category in MpCharacterStore.Categories())
        {
            names.Add(category.Name);
            options.Add(MenuText.Literal(category.Name));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.SetCategory),
            Description = MenuText.Key(Loc.CharacterCreator.SetCategoryDescription),
            Options = options,
            Gate = CharacterCreatorPermissions.Manage,
            SelectedIndex = Math.Max(0, IndexOf(names, entry.Character.Category)),

            OnSelected = selected =>
            {
                if (selected.SelectedIndex >= names.Count)
                {
                    return;
                }

                var category = names[selected.SelectedIndex];

                if (!MpCharacterStore.MoveToCategory(entry.Character, category))
                {
                    Notifications.Error(MenuText.Key(Loc.CharacterCreator.SaveFailed, ("name", MenuText.Literal(entry.Character.Name))));

                    return;
                }

                Notifications.Success(MenuText.Key(
                    Loc.CharacterCreator.CategoryChanged,
                    ("name", category.Length == 0
                        ? MenuText.Key(Loc.CharacterCreator.Uncategorised)
                        : MenuText.Literal(category))));

                Rebuild();
            },
        };
    }

    #endregion

    #region Outfits and styles

    private void BuildVariants(MenuBuilder menu)
    {
        _variants = menu;

        menu.AddRange(VariantRows());

        menu.OnOpened = _ => Fill(menu, VariantRows());
    }

    private IReadOnlyList<MenuEntry> VariantRows()
    {
        if (_selected is not { } entry)
        {
            return [];
        }

        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(_showingOutfits ? Loc.CharacterCreator.SaveOutfit : Loc.CharacterCreator.SaveStyle),
                Description = MenuText.Key(_showingOutfits
                    ? Loc.CharacterCreator.SaveOutfitDescription
                    : Loc.CharacterCreator.SaveStyleDescription),
                Gate = CharacterCreatorPermissions.Save,
                OnSelectedAsync = _ => SaveVariantAsync(entry),
            },
        };

        var count = _showingOutfits ? entry.Character.Outfits.Count : entry.Character.Styles.Count;

        if (count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.NoVariants),
                Description = MenuText.Key(Loc.CharacterCreator.NoVariantsDescription),
            });

            return rows;
        }

        for (var index = 0; index < count; index++)
        {
            rows.Add(VariantRow(entry, index));
        }

        return rows;
    }

    private ButtonEntry VariantRow(MpCharacterEntry entry, int index)
    {
        var name = VariantName(entry, index);

        return new ButtonEntry
        {
            Text = MenuText.Literal(name),
            Label = MenuText.Literal("→"),
            Description = MenuText.Key(Loc.CharacterCreator.VariantRowDescription, ("name", MenuText.Literal(name))),
            Gate = CharacterCreatorPermissions.Spawn,
            OnSelected = _ =>
            {
                _variant = index;

                _variantDetailPage?.Open();
            },
        };
    }

    private void BuildVariantDetail(MenuBuilder menu)
    {
        _variantDetail = menu;

        menu.AddRange(VariantDetailRows());

        menu.OnOpened = _ => Fill(menu, VariantDetailRows());
    }

    private IReadOnlyList<MenuEntry> VariantDetailRows()
    {
        if (_selected is not { } entry || Named() is not { } name)
        {
            return [];
        }

        var outfits = _showingOutfits;
        var index = _variant;
        var last = (outfits ? entry.Character.Outfits.Count : entry.Character.Styles.Count) <= 1;

        return
        [
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.VariantApply),
                Description = MenuText.Key(Loc.CharacterCreator.VariantRowDescription, ("name", name)),
                Gate = CharacterCreatorPermissions.Spawn,
                OnSelectedAsync = _ => ApplyVariantAsync(entry, index, outfits),
            },
            new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.VariantReplace),
                Description = MenuText.Key(Loc.CharacterCreator.VariantRowDescription, ("name", name)),
                ConfirmationDescription = MenuText.Key(Loc.CharacterCreator.VariantDeleteConfirm, ("name", name)),
                Gate = CharacterCreatorPermissions.Save,
                OnConfirmed = _ => ReplaceVariant(entry, index, outfits),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.VariantRename),
                Description = MenuText.Key(Loc.CharacterCreator.VariantRowDescription, ("name", name)),
                Gate = CharacterCreatorPermissions.Manage,
                OnSelectedAsync = _ => RenameVariantAsync(entry, index, outfits),
            },
            new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.VariantDelete),
                Description = MenuText.Key(Loc.CharacterCreator.VariantRowDescription, ("name", name)),
                LockedDescription = MenuText.Key(Loc.CharacterCreator.LastVariant),
                ConfirmationDescription = MenuText.Key(Loc.CharacterCreator.VariantDeleteConfirm, ("name", name)),
                Gate = MenuGate.Permission(CharacterCreatorPermissions.Manage) & MenuGate.When(() => !last),
                Behaviour = GateBehaviour.Lock,
                OnConfirmed = _ => DeleteVariant(entry, index, outfits),
            },
        ];
    }

    private MenuText? Named()
    {
        if (_selected is not { } entry)
        {
            return null;
        }

        var name = VariantName(entry, _variant);

        return name.Length == 0 ? null : (MenuText?)MenuText.Literal(name);
    }

    private string VariantName(MpCharacterEntry? entry, int index)
    {
        if (entry is null)
        {
            return string.Empty;
        }

        if (_showingOutfits)
        {
            return index < entry.Character.Outfits.Count ? entry.Character.Outfits[index].Name : string.Empty;
        }

        return index < entry.Character.Styles.Count ? entry.Character.Styles[index].Name : string.Empty;
    }

    private async Task ApplyVariantAsync(MpCharacterEntry entry, int index, bool outfits)
    {
        var ped = Native.PlayerPedId();

        if (outfits)
        {
            if (index >= entry.Character.Outfits.Count)
            {
                return;
            }

            var outfit = entry.Character.Outfits[index];

            Appearance.PedAppearanceWriter.Apply(ped, outfit.Outfit);

            if (MpCharacterState.Style is { } worn)
            {
                FreemodeWriter.ApplyHair(
                    ped, worn, outfit.Outfit.ComponentAt(Appearance.PedComponentSlots.Hair) is null);
            }

            FreemodeWriter.ApplyDecorations(ped, entry.Character.Core.Tattoos, MpCharacterState.Style);

            entry.Character.LastOutfit = outfit.Name;

            MpCharacterState.Wearing(entry.Character, MpCharacterState.Style, outfit, entry);

        }
        else
        {
            if (index >= entry.Character.Styles.Count)
            {
                return;
            }

            var style = entry.Character.Styles[index];

            FreemodeWriter.ApplyStyle(ped, style);
            FreemodeWriter.ApplyDecorations(ped, entry.Character.Core.Tattoos, style);

            entry.Character.LastStyle = style.Name;

            MpCharacterState.Wearing(entry.Character, style, MpCharacterState.Outfit, entry);

        }

        MpCharacterStore.Save(entry.Character, replacing: true);

        await API.Delay(0);
    }

    private async Task SaveVariantAsync(MpCharacterEntry entry)
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.VariantNamePrompt), NameLength),
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.VariantDescriptionPrompt), DescriptionLength))
            is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        var ped = Native.PlayerPedId();

        var outcome = _showingOutfits
            ? MpCharacterStore.SaveOutfit(entry, name, answers[1].Trim(), FreemodeReader.ReadOutfit(ped), replacing: false)
            : MpCharacterStore.SaveStyle(entry, name, answers[1].Trim(), FreemodeReader.ReadStyle(ped), replacing: false);

        Report(outcome, name);
    }

    private void ReplaceVariant(MpCharacterEntry entry, int index, bool outfits)
    {
        var name = VariantName(entry, index);

        if (name.Length == 0)
        {
            return;
        }

        var ped = Native.PlayerPedId();

        var outcome = outfits
            ? MpCharacterStore.SaveOutfit(entry, name, entry.Character.Outfits[index].Description, FreemodeReader.ReadOutfit(ped), replacing: true)
            : MpCharacterStore.SaveStyle(entry, name, entry.Character.Styles[index].Description, FreemodeReader.ReadStyle(ped), replacing: true);

        Report(outcome, name);
    }

    private async Task RenameVariantAsync(MpCharacterEntry entry, int index, bool outfits)
    {
        var current = VariantName(entry, index);

        if (current.Length == 0)
        {
            return;
        }

        var description = outfits
            ? entry.Character.Outfits[index].Description
            : entry.Character.Styles[index].Description;

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.VariantNamePrompt), NameLength, current),
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.VariantDescriptionPrompt), DescriptionLength, description))
            is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        var renamed = outfits
            ? MpCharacterStore.RenameOutfit(entry, entry.Character.Outfits[index], name, answers[1].Trim())
            : MpCharacterStore.RenameStyle(entry, entry.Character.Styles[index], name, answers[1].Trim());

        if (!renamed)
        {
            Notifications.Error(MenuText.Key(Loc.CharacterCreator.VariantNameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.VariantRenamed, ("name", MenuText.Literal(name))));

        Rebuild();
    }

    private void DeleteVariant(MpCharacterEntry entry, int index, bool outfits)
    {
        var name = VariantName(entry, index);

        if (name.Length == 0)
        {
            return;
        }

        var deleted = outfits
            ? MpCharacterStore.DeleteOutfit(entry, entry.Character.Outfits[index])
            : MpCharacterStore.DeleteStyle(entry, entry.Character.Styles[index]);

        if (!deleted)
        {
            Notifications.Error(MenuText.Key(Loc.CharacterCreator.VariantFailed));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.VariantDeleted, ("name", MenuText.Literal(name))));

        Rebuild();
    }

    private void Report(SaveOutcome outcome, string name)
    {
        var named = MenuText.Literal(name);

        if (outcome is SaveOutcome.Saved)
        {
            Notifications.Success(MenuText.Key(Loc.CharacterCreator.VariantSaved, ("name", named)));

            Rebuild();

            return;
        }

        Notifications.Error(MenuText.Key(
            outcome is SaveOutcome.NameTaken ? Loc.CharacterCreator.VariantNameTaken : Loc.CharacterCreator.VariantFailed,
            ("name", named)));
    }

    #endregion

    #region Actions on a character

    private async Task SpawnAsync(MpCharacterEntry entry)
    {
        var character = entry.Character;
        var model = PedSpawning.FreemodeModel(character.Core.IsMale);

        if (!await PedSpawning.SetPlayerModelAsync(model))
        {
            Notifications.Error(MenuText.Key(Loc.CharacterCreator.SaveFailed, ("name", MenuText.Literal(character.Name))));

            return;
        }

        var style = character.StyleNamed(character.LastStyle)
            ?? (character.Styles.Count > 0 ? character.Styles[0] : null);

        var outfit = character.OutfitNamed(character.LastOutfit)
            ?? (character.Outfits.Count > 0 ? character.Outfits[0] : null);

        var ped = Native.PlayerPedId();

        await FreemodeWriter.ApplyAsync(ped, character, style, outfit);

        MpCharacterState.Wearing(character, style, outfit, entry);

        var differences = outfit is null
            ? []
            : Appearance.PedAppearanceDiff.Compare(
                new Appearance.PedAppearance
                {
                    ModelHash = model,
                    Components = outfit.Outfit.Components,
                    Props = outfit.Outfit.Props,
                },
                Appearance.PedAppearanceReader.Read(ped));

        if (differences.Count == 0)
        {
            Notifications.Success(
                MenuText.Key(Loc.CharacterCreator.Spawned, ("name", MenuText.Literal(character.Name))),
                Notifications.SpawnDurationMs);

            return;
        }

        Notifications.Warning(
            MenuText.Key(
                Loc.CharacterCreator.SpawnedWithDifferences,
                ("name", MenuText.Literal(character.Name)),
                ("count", MenuText.Literal(differences.Count.ToString(CultureInfo.InvariantCulture)))),
            Notifications.SpawnDurationMs);
    }

    private async Task EditAsync(MpCharacterEntry entry)
    {
        if (_builder is not { } builder)
        {
            return;
        }

        if (!Differs(entry))
        {
            await builder.OpenAsync(entry.Character, entry, restore: false);

            return;
        }

        _editChoicePage?.Open();
    }

    private void BuildEditChoice(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.EditAsWorn),
            Description = MenuText.Key(Loc.CharacterCreator.EditAsWornDescription),
            Gate = CharacterCreatorPermissions.Create,
            OnSelectedAsync = _ => Edit(restore: false),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.EditRestored),
            Description = MenuText.Key(Loc.CharacterCreator.EditRestoredDescription),
            Gate = CharacterCreatorPermissions.Create,
            OnSelectedAsync = _ => Edit(restore: true),
        });
    }

    private async Task Edit(bool restore)
    {
        if (_builder is { } builder && _selected is { } entry)
        {
            await builder.OpenAsync(entry.Character, entry, restore);
        }
    }

    private static bool Differs(MpCharacterEntry entry)
    {
        var character = entry.Character;
        var model = PedSpawning.FreemodeModel(character.Core.IsMale);
        var ped = Native.PlayerPedId();

        if ((uint)Native.GetEntityModel(ped) != model)
        {
            return false;
        }

        var outfit = character.OutfitNamed(character.LastOutfit)
            ?? (character.Outfits.Count > 0 ? character.Outfits[0] : null);

        if (outfit is not null)
        {
            var saved = new Appearance.PedAppearance
            {
                ModelHash = model,
                Components = outfit.Outfit.Components,
                Props = outfit.Outfit.Props,
            };

            if (Appearance.PedAppearanceDiff.Compare(saved, Appearance.PedAppearanceReader.Read(ped)).Count > 0)
            {
                return true;
            }
        }

        var style = character.StyleNamed(character.LastStyle)
            ?? (character.Styles.Count > 0 ? character.Styles[0] : null);

        if (style is null)
        {
            return false;
        }

        var worn = FreemodeReader.ReadStyle(ped);

        return worn.HairStyle != style.HairStyle
            || worn.HairColour != style.HairColour
            || worn.HairHighlight != style.HairHighlight;
    }

    private async Task CloneAsync(MpCharacterEntry entry)
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.CharacterCreator.NamePrompt),
            NameLength,
            entry.Character.Name);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var name = typed.Trim();
        var outcome = MpCharacterStore.Duplicate(entry, name);

        if (outcome is not SaveOutcome.Saved)
        {
            Notifications.Error(MenuText.Key(
                outcome is SaveOutcome.NameTaken ? Loc.CharacterCreator.NameTaken : Loc.CharacterCreator.SaveFailed,
                ("name", MenuText.Literal(name))));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.Cloned, ("name", MenuText.Literal(name))));

        Rebuild();
    }

    private async Task RenameAsync(MpCharacterEntry entry)
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.NamePrompt), NameLength, entry.Character.Name),
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.DescriptionPrompt), DescriptionLength, entry.Character.Description))
            is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        var wasDefault = string.Equals(UserDefaults.DefaultCharacterName.Value, entry.Character.Name, StringComparison.Ordinal);

        if (!MpCharacterStore.Edit(entry, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.CharacterCreator.NameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        if (wasDefault)
        {
            UserDefaults.DefaultCharacterName.Value = name;
        }

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.Renamed, ("name", MenuText.Literal(name))));

        Rebuild();
    }

    private void ToggleDefault(MpCharacterEntry entry)
    {
        var name = entry.Character.Name;

        if (string.Equals(UserDefaults.DefaultCharacterName.Value, name, StringComparison.Ordinal))
        {
            UserDefaults.DefaultCharacterName.Value = string.Empty;

            Notifications.Success(MenuText.Key(Loc.CharacterCreator.DefaultCleared));

            Rebuild();

            return;
        }

        UserDefaults.DefaultCharacterName.Value = name;

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.DefaultSet, ("name", MenuText.Literal(name))));

        Rebuild();
    }

    private void Delete(MpCharacterEntry entry)
    {
        var name = entry.Character.Name;

        MpCharacterStore.Delete(name);

        if (string.Equals(UserDefaults.DefaultCharacterName.Value, name, StringComparison.Ordinal))
        {
            UserDefaults.DefaultCharacterName.Value = string.Empty;
        }

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.Deleted, ("name", MenuText.Literal(name))));

        _selected = null;

        Rebuild();
    }

    #endregion

    #region Categories

    private async Task CreateCategoryAsync()
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.CategoryName), NameLength),
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.CategoryDescriptionPrompt), DescriptionLength))
            is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (!MpCharacterStore.AddCategory(name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.CharacterCreator.CategoryNameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.CategoryCreated, ("name", MenuText.Literal(name))));

        Rebuild();
    }

    private async Task EditCategoryAsync(List<MpCharacterCategory> categories)
    {
        if (categories.Count == 0)
        {
            return;
        }

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.CharacterCreator.CategoryName),
            NameLength,
            categories[0].Name);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var existing = Find(categories, typed.Trim());

        if (existing is null)
        {
            Notifications.Error(MenuText.Key(Loc.CharacterCreator.CategoryNameTaken, ("name", MenuText.Literal(typed.Trim()))));

            return;
        }

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.CategoryName), NameLength, existing.Name),
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.CategoryDescriptionPrompt), DescriptionLength, existing.Description))
            is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (!MpCharacterStore.EditCategory(existing.Name, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.CharacterCreator.CategoryNameTaken, ("name", MenuText.Literal(name))));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.CategoryEdited, ("name", MenuText.Literal(name))));

        Rebuild();
    }

    private void DeleteCategory(List<MpCharacterCategory> categories, int index)
    {
        if (index < 0 || index >= categories.Count)
        {
            return;
        }

        var name = categories[index].Name;

        MpCharacterStore.DeleteCategory(name);

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.CategoryDeleted, ("name", MenuText.Literal(name))));

        Rebuild();
    }

    private static MpCharacterCategory? Find(List<MpCharacterCategory> categories, string name)
    {
        foreach (var category in categories)
        {
            if (string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return category;
            }
        }

        return null;
    }

    private static List<MenuText> Names(List<MpCharacterCategory> categories)
    {
        var names = new List<MenuText>(categories.Count);

        foreach (var category in categories)
        {
            names.Add(MenuText.Literal(category.Name));
        }

        return names;
    }

    #endregion

    private string CategoryTitle() => _category.Length == 0
        ? CharacterEdit.Resolve(MenuText.Key(Loc.CharacterCreator.Uncategorised))
        : _category;

    private static int Count(List<MpCharacterEntry> characters, string category)
    {
        var count = 0;

        foreach (var entry in characters)
        {
            if (string.Equals(entry.Character.Category, category, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    // By hand: the client sandbox has no default equality comparer.
    private static int IndexOf(List<string> values, string value)
    {
        for (var index = 0; index < values.Count; index++)
        {
            if (string.Equals(values[index], value, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static void Fill(MenuBuilder builder, IReadOnlyList<MenuEntry> rows)
    {
        var was = builder.Menu.CurrentIndex;
        var offset = builder.Menu.ViewIndexOffset;

        builder.ClearEntries();
        builder.AddRange(rows);

        var keep = was < builder.Menu.GetMenuItems().Count;

        builder.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }
}
