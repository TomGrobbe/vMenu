using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Appearance;
using vMenu.Enhanced.Data.Clothing;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Players.Appearance.Torso;
using vMenu.Enhanced.Serialization;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Menus.Players.Character;

internal static class OutfitPresetsMenu
{
    private const int NameLength = 50;

    private const int DescriptionLength = 200;

    private static ClothingPresetCategory? _category;

    private static OnlineOutfitPack? _pack;

    private static DetachedMenu? _categoryPage;

    private static DetachedMenu? _packPage;

    private static MenuBuilder? _server;

    private static MenuBuilder? _categoryList;

    internal static void Build(MenuBuilder menu)
    {
        _categoryPage = menu.AddDetachedMenu(
            MenuText.From(() => _category?.Name ?? string.Empty),
            MenuText.From(() => _category?.Name ?? string.Empty),
            BuildCategory,
            CharacterCreatorPermissions.Presets);

        _packPage = menu.AddDetachedMenu(
            MenuText.From(() => _pack?.Name ?? string.Empty),
            MenuText.From(() => _pack?.Name ?? string.Empty),
            BuildPack,
            CharacterCreatorPermissions.OnlineOutfits);

        ClothingPresetSync.Changed += OnPresetsChanged;

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.ServerPresets),
            Description = MenuText.Key(Loc.CharacterCreator.ServerPresetsDescription),
            MenuSubtitle = MenuText.Key(Loc.CharacterCreator.ServerPresetsSubtitle),
            Gate = CharacterCreatorPermissions.Presets,
            Build = BuildServerPresets,
        });

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.OnlineOutfits),
            Description = MenuText.Key(Loc.CharacterCreator.OnlineOutfitsDescription),
            MenuSubtitle = MenuText.Key(Loc.CharacterCreator.OnlineOutfitsSubtitle),
            Gate = CharacterCreatorPermissions.OnlineOutfits,
            Build = BuildOnline,
        });
    }

    #region Server presets

    private static void BuildServerPresets(MenuBuilder menu)
    {
        _server = menu;

        menu.AddRange(ServerRows());

        menu.OnOpened = _ => Fill(menu, ServerRows());
    }

    private static void OnPresetsChanged()
    {
        if (_category is { } open)
        {
            _category = ClothingPresetSync.Find(open.Name);
        }

        if (_server is { } server)
        {
            Fill(server, ServerRows());
        }

        if (_categoryList is { } categories)
        {
            Fill(categories, CategoryRows());
        }
    }

    private static IReadOnlyList<MenuEntry> ServerRows()
    {
        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.CreatePresetCategory),
                Description = MenuText.Key(Loc.CharacterCreator.CreatePresetCategoryDescription),
                Gate = CharacterCreatorPermissions.PresetsManage,
                OnSelectedAsync = _ => CreateCategoryAsync(),
            },
        };

        foreach (var category in ClothingPresetSync.Categories)
        {
            var current = category;
            var name = MenuText.Literal(current.Name);

            rows.Add(new ButtonEntry
            {
                Text = name,
                Label = MenuText.Literal("(" + current.Presets.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")"),
                Description = MenuText.Key(Loc.CharacterCreator.PresetCategoryRowDescription, ("name", name)),
                Gate = CharacterCreatorPermissions.Presets,
                OnSelected = _ =>
                {
                    _category = current;

                    _categoryPage?.Open();
                },
            });
        }

        if (ClothingPresetSync.Categories.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.NoPresets),
                Description = MenuText.Key(Loc.CharacterCreator.NoPresetsDescription),
            });
        }

        return rows;
    }

    private static void BuildCategory(MenuBuilder menu)
    {
        _categoryList = menu;

        menu.AddRange(CategoryRows());

        menu.OnOpened = _ => Fill(menu, CategoryRows());
    }

    private static IReadOnlyList<MenuEntry> CategoryRows()
    {
        if (_category is not { } category)
        {
            return [];
        }

        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.SavePreset),
                Description = MenuText.Key(Loc.CharacterCreator.SavePresetDescription),
                Gate = CharacterCreatorPermissions.PresetsManage,
                OnSelectedAsync = _ => PublishAsync(category),
            },
            new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.DeletePresetCategory),
                Description = MenuText.Key(Loc.CharacterCreator.DeletePresetCategoryDescription),
                ConfirmationDescription = MenuText.Key(
                    Loc.CharacterCreator.DeletePresetCategoryConfirm,
                    ("name", MenuText.Literal(category.Name))),
                Gate = CharacterCreatorPermissions.PresetsManage,
                OnConfirmedAsync = _ => RemoveCategoryAsync(category),
            },
        };

        var male = PedSpawning.IsFreemodeMale((uint)Native.GetEntityModel(Native.PlayerPedId()));

        foreach (var preset in category.Presets)
        {
            var current = preset;
            var name = MenuText.Literal(current.Name);
            var fits = current.Fits(male);

            rows.Add(new SubmenuEntry
            {
                Text = name,
                Description = MenuText.Key(
                    fits ? Loc.CharacterCreator.PresetRowDescription : Loc.CharacterCreator.PresetWrongSex,
                    ("name", name)),
                MenuSubtitle = name,
                Gate = CharacterCreatorPermissions.Presets,
                Build = child => BuildPreset(child, category, current),
            });
        }

        if (category.Presets.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.NoPresets),
                Description = MenuText.Key(Loc.CharacterCreator.NoPresetsDescription),
            });
        }

        return rows;
    }

    private static void BuildPreset(MenuBuilder menu, ClothingPresetCategory category, ClothingPreset preset)
    {
        var name = MenuText.Literal(preset.Name);

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.VariantApply),
            Description = MenuText.Key(Loc.CharacterCreator.PresetRowDescription, ("name", name)),
            Gate = CharacterCreatorPermissions.Presets,
            OnSelected = _ => Wear(preset),
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.DeletePreset),
            Description = MenuText.Key(Loc.CharacterCreator.DeletePresetDescription),
            ConfirmationDescription = MenuText.Key(Loc.CharacterCreator.DeletePresetConfirm, ("name", name)),
            Gate = CharacterCreatorPermissions.PresetsManage,
            OnConfirmedAsync = _ => RemovePresetAsync(category, preset),
        });
    }

    private static void Wear(ClothingPreset preset)
    {
        var ped = Native.PlayerPedId();
        var male = PedSpawning.IsFreemodeMale((uint)Native.GetEntityModel(ped));

        if (!preset.Fits(male))
        {
            Notifications.Warning(
                MenuText.Key(Loc.CharacterCreator.PresetWrongSex, ("name", MenuText.Literal(preset.Name))),
                Notifications.SpawnDurationMs);

            return;
        }

        PedAppearanceWriter.Apply(ped, preset);

        Restore(ped, preset);
    }

    #endregion

    #region GTA Online outfits

    private static void BuildOnline(MenuBuilder menu)
    {
        menu.AddRange(OnlineRows());

        menu.OnOpened = _ =>
        {
            OnlineOutfitCatalogue.Begin(Male());

            Fill(menu, OnlineRows());
        };

        OnlineOutfitCatalogue.Changed += () => Fill(menu, OnlineRows());
    }

    private static IReadOnlyList<MenuEntry> OnlineRows()
    {
        var male = Male();

        if (!OnlineOutfitCatalogue.IsReady(male))
        {
            return
            [
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.CharacterCreator.OnlineLoading),
                    Description = MenuText.Key(Loc.CharacterCreator.OnlineLoadingDescription),
                },
            ];
        }

        var packs = OnlineOutfitCatalogue.Packs(male);
        var rows = new List<MenuEntry>();

        foreach (var pack in packs)
        {
            var current = pack;
            var name = current.Name.Length == 0
                ? MenuText.Key(Loc.CharacterCreator.BaseCollection)
                : MenuText.Literal(current.Name);

            rows.Add(new ButtonEntry
            {
                Text = name,
                Label = MenuText.Literal("(" + current.Outfits.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")"),
                Description = MenuText.Key(Loc.CharacterCreator.OnlinePackRowDescription, ("name", name)),
                Gate = CharacterCreatorPermissions.OnlineOutfits,
                OnSelected = _ =>
                {
                    _pack = current;

                    _packPage?.Open();
                },
            });
        }

        if (rows.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.OnlineEmpty),
                Description = MenuText.Key(Loc.CharacterCreator.OnlineEmptyDescription),
            });
        }

        return rows;
    }

    private static void BuildPack(MenuBuilder menu)
    {
        menu.AddRange(PackRows());

        menu.OnOpened = _ => Fill(menu, PackRows());
    }

    private static IReadOnlyList<MenuEntry> PackRows()
    {
        if (_pack is not { } pack)
        {
            return [];
        }

        var rows = new List<MenuEntry>();

        for (var index = 0; index < pack.Outfits.Count; index++)
        {
            var outfit = pack.Outfits[index];

            var name = outfit.Name.Length == 0
                ? MenuText.Key(
                    Loc.CharacterCreator.OnlineUnnamedOutfit,
                    ("number", MenuText.Literal((index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture))))
                : MenuText.Literal(outfit.Name);

            rows.Add(new ButtonEntry
            {
                Text = name,
                Description = MenuText.Key(Loc.CharacterCreator.OnlineOutfitRowDescription, ("name", name)),
                Gate = CharacterCreatorPermissions.OnlineOutfits,
                OnSelected = _ => WearOnline(outfit),
            });
        }

        if (rows.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.CharacterCreator.OnlineEmpty),
                Description = MenuText.Key(Loc.CharacterCreator.OnlineEmptyDescription),
            });
        }

        return rows;
    }

    private static void WearOnline(OnlineOutfit outfit)
    {
        var ped = Native.PlayerPedId();

        PedAppearanceWriter.Apply(ped, outfit.Outfit);

        if (outfit.Outfit.ComponentAt(PedComponentSlots.Torso) is null)
        {
            TorsoFit.FitWornOutfit(ped);
        }

        Restore(ped, outfit.Outfit);
    }

    #endregion

    #region Publishing

    private static async Task CreateCategoryAsync()
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.PresetCategoryName), NameLength),
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.PresetCategoryDescriptionPrompt), DescriptionLength))
            is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        Report(
            await ServerActions.InvokeAsync(ActionIds.CharacterCreator.AddPresetCategory, name, answers[1].Trim()),
            Loc.CharacterCreator.PresetCategoryCreated,
            name);
    }

    private static async Task PublishAsync(ClothingPresetCategory category)
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.PresetNamePrompt), NameLength),
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.PresetDescriptionPrompt), DescriptionLength))
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
        var outfit = PedAppearanceReader.ReadOutfit(ped);

        Report(
            await ServerActions.InvokeAsync(
                ActionIds.CharacterCreator.AddPreset,
                category.Name,
                name,
                answers[1].Trim(),
                Male() ? "male" : "female",
                ClientJson.Serialize(outfit)),
            Loc.CharacterCreator.PresetSaved,
            name);
    }

    private static async Task RemoveCategoryAsync(ClothingPresetCategory category)
    {
        _category = null;

        Report(
            await ServerActions.InvokeAsync(ActionIds.CharacterCreator.RemovePresetCategory, category.Name),
            Loc.CharacterCreator.PresetCategoryDeleted,
            category.Name);
    }

    private static async Task RemovePresetAsync(ClothingPresetCategory category, ClothingPreset preset) =>
        Report(
            await ServerActions.InvokeAsync(ActionIds.CharacterCreator.RemovePreset, category.Name, preset.Name),
            Loc.CharacterCreator.PresetDeleted,
            preset.Name);

    private static void Report(ActionResult result, string success, string name)
    {
        var named = MenuText.Literal(name);

        if (result.Status is ActionStatus.Ok)
        {
            Notifications.Success(MenuText.Key(success, ("name", named)));

            return;
        }

        var key = result.Status switch
        {
            ActionStatus.Denied => Loc.CharacterCreator.PresetSaveDenied,
            ActionStatus.Refused => Loc.CharacterCreator.PresetNameTaken,
            ActionStatus.NotFound => Loc.CharacterCreator.PresetCategoryGone,
            _ => Loc.CharacterCreator.PresetSaveFailed,
        };

        Notifications.Error(MenuText.Key(key, ("name", named)));
    }

    #endregion

    private static void Restore(int ped, PedOutfit outfit)
    {
        if (MpCharacterState.Style is { } style)
        {
            FreemodeWriter.ApplyHair(ped, style, outfit.ComponentAt(PedComponentSlots.Hair) is null);
        }

        if (MpCharacterState.Worn is { } core)
        {
            FreemodeWriter.ApplyDecorations(ped, core.Tattoos, MpCharacterState.Style);
        }
    }

    private static bool Male() => PedSpawning.IsFreemodeMale((uint)Native.GetEntityModel(Native.PlayerPedId()));

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
