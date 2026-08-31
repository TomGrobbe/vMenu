using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Saved;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Menus.Players.Character;

internal sealed class CharacterBuilder
{
    private const int NameLength = 50;

    private const int DescriptionLength = 200;

    private DetachedMenu? _page;

    private DetachedMenu? _collectionPage;

    private PedCollection? _collection;

    internal Action<string?>? Finished { get; set; }

    internal void Attach(MenuBuilder parent)
    {
        _collectionPage = parent.AddDetachedMenu(
            MenuText.From(CollectionTitle),
            MenuText.Key(Loc.CharacterCreator.CollectionsSubtitle),
            child =>
            {
                PedCustomizationRows.Attach(
                    child,
                    () => PedVariationScope.ForCollection(CollectionName()),
                    components: PedComponentSlots.Clothing,
                    fitTorso: true);

                child.OnOpened = _ => CharacterCamera.Page = CameraFocus.FullBody;
            },
            CharacterCreatorPermissions.Create);

        _page = parent.AddDetachedMenu(
            MenuText.Key(Loc.CharacterCreator.CreateTitle),
            MenuText.From(() => CharacterEdit.Resolve(MenuText.Key(
                MpCharacterState.From is null
                    ? Loc.CharacterCreator.CreateSubtitle
                    : Loc.CharacterCreator.EditSubtitle))),
            Build,
            CharacterCreatorPermissions.Create);
    }

    internal async Task OpenAsync(MpCharacter character, MpCharacterEntry? from, bool restore)
    {
        var model = PedSpawning.FreemodeModel(character.Core.IsMale);

        var rebuild = restore || (uint)Native.GetEntityModel(Native.PlayerPedId()) != model;

        if (rebuild && !await PedSpawning.SetPlayerModelAsync(model))
        {
            Notifications.Error(MenuText.Key(Loc.CharacterCreator.SaveFailed, ("name", MenuText.Literal(character.Name))));

            return;
        }

        var draft = character.Copy();

        MpCharacterState.BeginEditing(draft, from);
        CharacterCamera.Reevaluate();

        if (rebuild)
        {
            await FreemodeWriter.ApplyAsync(
                Native.PlayerPedId(),
                draft,
                MpCharacterState.Style,
                MpCharacterState.Outfit);
        }
        else
        {
            CaptureWorn();
        }

        _page?.Open();
    }

    private static void CaptureWorn()
    {
        var ped = CharacterEdit.Ped;

        Remember();

        if (MpCharacterState.Style is not { } style)
        {
            return;
        }

        var worn = FreemodeReader.ReadStyle(ped);

        style.HairStyle = worn.HairStyle;
        style.HairColour = worn.HairColour;
        style.HairHighlight = worn.HairHighlight;
        style.HairDecorationCollection = worn.HairDecorationCollection;
        style.HairDecorationName = worn.HairDecorationName;
        style.Overlays = worn.Overlays;
    }

    private void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.DisableAutoCamera),
            Description = MenuText.Key(Loc.CharacterCreator.DisableAutoCameraDescription),
            Gate = CharacterCreatorPermissions.Create,
            ReadState = () => CharacterCamera.AutoCameraDisabled,
            OnChanged = changed => CharacterCamera.AutoCameraDisabled = changed.Checked,
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.Randomize),
            Description = MenuText.Key(Loc.CharacterCreator.RandomizeDescription),
            Gate = CharacterCreatorPermissions.Create,
            OnSelected = _ => Randomise(),
        });

        menu.Entries.Add(Child(
            Loc.CharacterCreator.Inheritance,
            Loc.CharacterCreator.InheritanceDescription,
            Loc.CharacterCreator.InheritanceSubtitle,
            CharacterInheritanceRows.Attach));

        menu.Entries.Add(Child(
            Loc.CharacterCreator.Appearance,
            Loc.CharacterCreator.AppearanceDescription,
            Loc.CharacterCreator.AppearanceSubtitle,
            CharacterAppearanceRows.Attach));

        menu.Entries.Add(Child(
            Loc.CharacterCreator.FaceShape,
            Loc.CharacterCreator.FaceShapeDescription,
            Loc.CharacterCreator.FaceShapeSubtitle,
            CharacterFaceRows.Attach));

        menu.Entries.Add(Child(
            Loc.CharacterCreator.Tattoos,
            Loc.CharacterCreator.TattoosDescription,
            Loc.CharacterCreator.TattoosSubtitle,
            CharacterTattooRows.Attach));

        menu.Entries.Add(Child(
            Loc.CharacterCreator.Clothes,
            Loc.CharacterCreator.ClothesDescription,
            Loc.CharacterCreator.ClothesSubtitle,
            child => Wardrobe(child, PedCustomizationRows.PedRows.Components)));

        menu.Entries.Add(Child(
            Loc.CharacterCreator.Props,
            Loc.CharacterCreator.PropsDescription,
            Loc.CharacterCreator.PropsSubtitle,
            child => Wardrobe(child, PedCustomizationRows.PedRows.Props)));

        menu.Entries.Add(Child(
            Loc.CharacterCreator.Collections,
            Loc.CharacterCreator.CollectionsDescription,
            Loc.CharacterCreator.CollectionsSubtitle,
            BuildCollectionList));

        menu.Entries.Add(ExpressionRow());
        menu.Entries.Add(CategoryRow());

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.Save),
            Description = MenuText.Key(Loc.CharacterCreator.SaveDescription),
            Gate = CharacterCreatorPermissions.Save,
            OnSelectedAsync = _ => SaveAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.Exit),
            Description = MenuText.Key(Loc.CharacterCreator.ExitDescription),
            ConfirmationDescription = MenuText.Key(Loc.CharacterCreator.ExitConfirm),
            OnConfirmedAsync = _ => LeaveAsync(),
        });

        menu.Keys.Add(new MenuKey
        {
            Name = "creatorback",
            Description = MenuText.Key(Loc.CharacterCreator.BackBlockedBinding),
            DefaultKey = "BACK",
            DefaultButton = "B_INDEX",
            Text = MenuText.Empty,
            Handler = (_, _) => Notifications.Warning(MenuText.Key(Loc.CharacterCreator.BackBlocked)),
        });

        menu.OnOpened = _ =>
        {
            CharacterCamera.Page = CameraFocus.FullBody;

            MenuController.DisableBackButton = true;

            MenuRegistry.Refresh(menu.Menu);
        };

        menu.OnClosed = _ => MenuController.DisableBackButton = false;
    }

    private static SubmenuEntry Child(string name, string description, string subtitle, Action<MenuBuilder> build) =>
        new()
        {
            Text = MenuText.Key(name),
            Description = MenuText.Key(description),
            MenuSubtitle = MenuText.Key(subtitle),
            Gate = CharacterCreatorPermissions.Create,
            Build = build,
        };

    private static void Wardrobe(MenuBuilder menu, PedCustomizationRows.PedRows rows)
    {
        PedCustomizationRows.Attach(
            menu,
            () => PedVariationScope.Global,
            rows,
            PedComponentSlots.Clothing,
            fitTorso: rows == PedCustomizationRows.PedRows.Components);

        menu.OnOpened = _ =>
        {
            CharacterCamera.Page = rows == PedCustomizationRows.PedRows.Props
                ? CameraFocus.Head
                : CameraFocus.FullBody;

            Remember();
        };

        menu.OnClosed = _ => Remember();
    }

    private static void Remember()
    {
        if (MpCharacterState.Outfit is { } outfit)
        {
            outfit.Outfit = FreemodeReader.ReadOutfit(CharacterEdit.Ped);
        }
    }

    private ListEntry ExpressionRow()
    {
        var options = new List<MenuText>
        {
            MenuText.Key(Loc.CharacterCreator.ExpressionNormal),
            MenuText.Key(Loc.CharacterCreator.ExpressionHappy),
            MenuText.Key(Loc.CharacterCreator.ExpressionAngry),
            MenuText.Key(Loc.CharacterCreator.ExpressionAiming),
            MenuText.Key(Loc.CharacterCreator.ExpressionInjured),
            MenuText.Key(Loc.CharacterCreator.ExpressionStressed),
            MenuText.Key(Loc.CharacterCreator.ExpressionSmug),
            MenuText.Key(Loc.CharacterCreator.ExpressionSulk),
        };

        return new ListEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.Expression),
            Description = MenuText.Key(Loc.CharacterCreator.ExpressionDescription),
            Options = options,
            Gate = CharacterCreatorPermissions.Create,
            Configure = item => item.ItemData = CameraFocus.Head,
            ReadSelectedIndex = () => ExpressionIndex(),

            OnIndexChanged = changed =>
            {
                if (CharacterEdit.Draft is { } draft && changed.NewIndex < CharacterDraft.Expressions.Length)
                {
                    draft.FacialExpression = CharacterDraft.Expressions[changed.NewIndex];

                    CharacterEdit.ApplyExpression();
                }
            },
        };
    }

    private static int ExpressionIndex()
    {
        if (CharacterEdit.Draft is not { } draft)
        {
            return 0;
        }

        for (var index = 0; index < CharacterDraft.Expressions.Length; index++)
        {
            if (string.Equals(CharacterDraft.Expressions[index], draft.FacialExpression, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return 0;
    }

    private ListEntry CategoryRow()
    {
        return new ListEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.Category),
            Description = MenuText.Key(Loc.CharacterCreator.CategoryDescription),
            Options = CategoryOptions(),
            Gate = CharacterCreatorPermissions.Save,

            ReadSelectedIndex = CategoryIndex,

            OnIndexChanged = changed =>
            {
                var names = CategoryNames();

                if (CharacterEdit.Draft is { } draft && changed.NewIndex < names.Count)
                {
                    draft.Category = names[changed.NewIndex];
                }
            },
        };
    }

    private static List<MenuText> CategoryOptions()
    {
        var options = new List<MenuText> { MenuText.Key(Loc.CharacterCreator.Uncategorised) };

        foreach (var name in CategoryNames())
        {
            if (name.Length > 0)
            {
                options.Add(MenuText.Literal(name));
            }
        }

        return options;
    }

    private static List<string> CategoryNames()
    {
        var names = new List<string> { string.Empty };

        foreach (var category in MpCharacterStore.Categories())
        {
            names.Add(category.Name);
        }

        return names;
    }

    private static int CategoryIndex()
    {
        if (CharacterEdit.Draft is not { } draft)
        {
            return 0;
        }

        var names = CategoryNames();

        for (var index = 0; index < names.Count; index++)
        {
            if (string.Equals(names[index], draft.Category, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return 0;
    }

    #region Collections

    private void BuildCollectionList(MenuBuilder menu)
    {
        menu.AddRange(CollectionRows());

        menu.OnOpened = _ =>
        {
            CharacterCamera.Page = CameraFocus.FullBody;

            Refill(menu, CollectionRows());
        };
    }

    private IReadOnlyList<MenuEntry> CollectionRows()
    {
        var rows = new List<MenuEntry>();

        foreach (var collection in PedCollections.All(Native.PlayerPedId()))
        {
            var current = collection;

            rows.Add(new ButtonEntry
            {
                Text = Name(current),
                Description = MenuText.Key(Loc.CharacterCreator.CollectionRowDescription, ("name", Name(current))),
                Gate = CharacterCreatorPermissions.Create,
                OnSelected = _ =>
                {
                    _collection = current;

                    _collectionPage?.Open();
                },
            });
        }

        return rows;
    }

    private static MenuText Name(PedCollection collection) => collection.IsBaseGame
        ? MenuText.Key(Loc.CharacterCreator.BaseCollection)
        : MenuText.Literal(collection.Name);

    private string CollectionName() => _collection?.Name ?? string.Empty;

    private string CollectionTitle() => _collection is { IsBaseGame: false } collection
        ? collection.Name
        : CharacterEdit.Resolve(MenuText.Key(Loc.CharacterCreator.BaseCollection));

    #endregion

    private void Randomise()
    {
        if (CharacterEdit.Draft is not { } draft || CharacterEdit.Style is not { } style)
        {
            return;
        }

        var ped = CharacterEdit.Ped;

        CharacterDraft.Randomise(draft, style, ped);

        CharacterEdit.ApplyBlend();

        foreach (var overlay in draft.Core.Overlays)
        {
            CharacterEdit.ApplyOverlay(overlay);
        }

        foreach (var overlay in style.Overlays)
        {
            CharacterEdit.ApplyOverlay(overlay);
        }

        CharacterEdit.ApplyEyes();
        CharacterEdit.ApplyHair();

        Native.SetPedRandomComponentVariation(ped, 0);
        Native.SetPedRandomProps(ped);

        Remember();

        Notifications.Success(
            MenuText.Key(Loc.CharacterCreator.RandomizeDone),
            Notifications.SpawnDurationMs);
    }

    private async Task SaveAsync()
    {
        if (CharacterEdit.Draft is not { } draft)
        {
            return;
        }

        Remember();

        var editing = MpCharacterState.From;

        if (editing is not null && !editing.IsFromNewerBuild)
        {
            Finish(MpCharacterStore.Save(draft, replacing: true), draft.Name);

            return;
        }

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.NamePrompt), NameLength, draft.Name),
            new InputPrompt(MenuText.Key(Loc.CharacterCreator.DescriptionPrompt), DescriptionLength, draft.Description))
            is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        draft.Name = name;
        draft.Description = answers[1].Trim();

        Finish(MpCharacterStore.Save(draft, replacing: false), name);
    }

    private void Finish(SaveOutcome outcome, string name)
    {
        var named = MenuText.Literal(name);

        if (outcome is not SaveOutcome.Saved)
        {
            Notifications.Error(MenuText.Key(
                outcome is SaveOutcome.NameTaken ? Loc.CharacterCreator.NameTaken : Loc.CharacterCreator.SaveFailed,
                ("name", named)));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.CharacterCreator.Saved, ("name", named)));

        Close(name);
    }

    private async Task LeaveAsync()
    {
        if (MpCharacterState.From is { } saved)
        {
            await FreemodeWriter.ApplyAsync(
                CharacterEdit.Ped,
                saved.Character,
                saved.Character.CurrentStyle,
                saved.Character.CurrentOutfit);
        }

        Close(MpCharacterState.From?.Character.Name);
    }

    private void Close(string? returnTo)
    {
        var draft = MpCharacterState.Draft;
        var style = MpCharacterState.Style;
        var outfit = MpCharacterState.Outfit;

        MpCharacterState.StopEditing();
        CharacterCamera.Reevaluate();

        if (draft is not null)
        {
            MpCharacterState.Wearing(draft, style, outfit, MpCharacterState.From);
        }

        if (returnTo is not null && Finished is { } finished)
        {
            finished(returnTo);

            return;
        }

        MenuController.CloseAllMenus();
    }

    private static void Refill(MenuBuilder builder, IReadOnlyList<MenuEntry> rows)
    {
        var was = builder.Menu.CurrentIndex;
        var offset = builder.Menu.ViewIndexOffset;

        builder.ClearEntries();
        builder.AddRange(rows);

        var keep = was < builder.Menu.GetMenuItems().Count;

        builder.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }
}
