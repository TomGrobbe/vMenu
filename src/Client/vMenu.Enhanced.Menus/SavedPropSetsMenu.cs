using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Props;
using vMenu.Enhanced.Menus.Props.Saved;
using vMenu.Enhanced.Menus.Saved;

using PropSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.PropSpawner;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.PropSpawner.Sets,
    SubtitleKey = Loc.PropSpawner.SetsSubtitle,
    DescriptionKey = Loc.PropSpawner.SetsDescription,
    Permission = PropSpawnerPermissions.Sets)]
public sealed class SavedPropSetsMenu : MenuDefinition
{
    private const int NameMaxLength = 40;

    private const int DescriptionMaxLength = 80;

    private const string SpawnRowMarker = "spawn";

    private MenuBuilder? _root;

    private DetachedMenu? _detailMenu;

    private DetachedMenu? _removeMenu;

    private SavedPropSetEntry? _selected;

    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    protected override void Build(MenuBuilder menu)
    {
        _root = menu;

        _detailMenu = menu.AddDetachedMenu(
            MenuText.From(() => _selected?.Set.Name ?? string.Empty),
            MenuText.Key(Loc.PropSpawner.SetsSubtitle),
            _ => { });

        _detailMenu.Builder.OnOpened = _ =>
        {
            Fill(_detailMenu, DetailRows());

            PreviewDetail(_detailMenu?.Builder.Menu.GetCurrentMenuItem());
        };

        _detailMenu.Builder.OnIndexChanged = changed => PreviewDetail(changed.NewItem);

        _detailMenu.Builder.OnClosed = _ => PropPreview.Hide();

        _removeMenu = menu.AddDetachedMenu(
            MenuText.Key(Loc.PropSpawner.SetsRemoveProp),
            MenuText.From(() => _selected?.Set.Name ?? string.Empty),
            _ => { },
            PropSpawnerPermissions.SetsManage);

        _removeMenu.Builder.OnOpened = _ =>
        {
            Fill(_removeMenu, RemoveRows());

            PreviewRow(_removeMenu?.Builder.Menu.GetCurrentMenuItem());
        };

        _removeMenu.Builder.OnIndexChanged = changed => PreviewRow(changed.NewItem);

        _removeMenu.Builder.OnClosed = _ => PropPreview.Hide();

        menu.AddRange(RootRows());

        menu.OnOpened = _ => Refill(menu, RootRows());
    }

    private IReadOnlyList<MenuEntry> RootRows()
    {
        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.PropSpawner.SetsCreate),
                Description = MenuText.Key(Loc.PropSpawner.SetsCreateDescription),
                Gate = PropSpawnerPermissions.SetsManage,
                OnSelectedAsync = _ => CreateAsync(),
            },
        };

        var sets = SavedPropSetStore.All();

        if (sets.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.PropSpawner.SetsEmpty),
                Description = MenuText.Key(Loc.PropSpawner.SetsEmptyDescription),
                ReadEnabled = static () => false,
            });

            return rows;
        }

        foreach (var entry in sets)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(entry.Set.Name),
                Description = MenuText.Key(
                    Loc.PropSpawner.SetsRowDescription,
                    ("count", MenuText.Literal(Number(entry.Set.Props.Count)))),
                Label = MenuText.Literal(Number(entry.Set.Props.Count)),
                OnSelected = _ =>
                {
                    _selected = entry;

                    _detailMenu?.Open();
                },
            });
        }

        return rows;
    }

    private IReadOnlyList<MenuEntry> DetailRows()
    {
        if (_selected is not { } entry)
        {
            return [];
        }

        return
        [
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.PropSpawner.SetsSpawn),
                Description = MenuText.Key(Loc.PropSpawner.SetsSpawnDescription),
                ReadEnabled = () => entry.Set.Props.Count > 0,

                Configure = item => item.ItemData = SpawnRowMarker,
                OnSelectedAsync = _ => SpawnAsync(entry),
            },
            new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.PropSpawner.SetsUnspawn),
                Description = MenuText.Key(Loc.PropSpawner.SetsUnspawnDescription),
                ConfirmationDescription = MenuText.Key(Loc.PropSpawner.SetsUnspawnConfirm),
                ReadEnabled = () => SpawnedProps.StandingFrom(entry.Set.Name) > 0,
                OnConfirmed = _ => RemoveSpawned(entry),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.PropSpawner.SetsAddProp),
                Description = MenuText.Key(Loc.PropSpawner.SetsAddPropDescription),
                Gate = PropSpawnerPermissions.SetsManage,
                OnSelectedAsync = _ => AddPropAsync(entry),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.PropSpawner.SetsRemoveList),
                Description = MenuText.Key(Loc.PropSpawner.SetsRemoveListDescription),
                Label = MenuText.Literal("→"),
                Gate = PropSpawnerPermissions.SetsManage,
                ReadEnabled = () => entry.Set.Props.Count > 0,
                OnSelected = _ => _removeMenu?.Open(),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.PropSpawner.SetsRename),
                Description = MenuText.Key(Loc.PropSpawner.SetsRenameDescription),
                Gate = PropSpawnerPermissions.SetsManage,
                OnSelectedAsync = _ => RenameAsync(entry),
            },
            new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.PropSpawner.SetsDelete),
                Description = MenuText.Key(Loc.PropSpawner.SetsDeleteDescription),
                ConfirmationDescription = MenuText.Key(Loc.PropSpawner.SetsDeleteConfirm),
                Gate = PropSpawnerPermissions.SetsManage,
                OnConfirmed = _ => Delete(entry),
            },
        ];
    }

    private IReadOnlyList<MenuEntry> RemoveRows()
    {
        if (_selected is not { } entry)
        {
            return [];
        }

        if (entry.Set.Props.Count == 0)
        {
            return
            [
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.PropSpawner.SetsNoProps),
                    Description = MenuText.Key(Loc.PropSpawner.SetsNoPropsDescription),
                    ReadEnabled = static () => false,
                },
            ];
        }

        var rows = new List<MenuEntry>(entry.Set.Props.Count);

        for (var index = 0; index < entry.Set.Props.Count; index++)
        {
            var prop = entry.Set.Props[index];
            var at = index;

            rows.Add(new ConfirmButtonEntry
            {
                Text = MenuText.Literal(prop.Model),
                Description = MenuText.Key(
                    Loc.PropSpawner.SetsRemovePropDescription,
                    ("where", MenuText.Literal(Where(prop)))),
                ConfirmationDescription = MenuText.Key(Loc.PropSpawner.SetsRemovePropConfirm),

                Configure = item => item.ItemData = prop,
                OnConfirmed = _ => RemoveProp(entry, at),
            });
        }

        return rows;
    }

    private void PreviewDetail(MenuAPI.MenuItem? item)
    {
        if (_selected is { } entry && item?.ItemData as string == SpawnRowMarker)
        {
            PropPreview.ShowSet(entry.Set.Name, entry.Set.Props);

            return;
        }

        PropPreview.Hide();
    }

    private static void PreviewRow(MenuAPI.MenuItem? item)
    {
        if (item?.ItemData is SavedProp prop)
        {
            PropPreview.ShowAt(
                prop.Model,
                new System.Numerics.Vector3(prop.X, prop.Y, prop.Z),
                prop.Heading);

            return;
        }

        PropPreview.Hide();
    }

    private async Task CreateAsync()
    {
        var answers = await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.PropSpawner.SetsName), NameMaxLength),
            new InputPrompt(MenuText.Key(Loc.PropSpawner.SetsDescriptionPrompt), DescriptionMaxLength));

        if (answers is null || string.IsNullOrWhiteSpace(answers[0]))
        {
            return;
        }

        var set = new SavedPropSet { Name = answers[0].Trim(), Description = answers[1].Trim() };

        Report(SavedPropSetStore.Save(set, replacing: false));

        Rebuild();
    }

    private async Task SpawnAsync(SavedPropSetEntry entry)
    {
        PropPreview.Hide();

        var placed = new List<int>(entry.Set.Props.Count);

        foreach (var prop in entry.Set.Props)
        {
            if (!SpawnedProps.TryTakeOrWarn())
            {
                break;
            }

            var spawned = await PropSpawning.SpawnAsync(
                PropModelNames.Remember(prop.Model),
                new System.Numerics.Vector3(prop.X, prop.Y, prop.Z),
                prop.Networked,
                prop.Frozen);

            if (spawned is null)
            {
                continue;
            }

            Native.SetEntityHeading(spawned.Handle, prop.Heading);

            placed.Add(spawned.Handle);
        }

        SpawnedProps.RecordSet(entry.Set.Name, placed);

        Notifications.Success(MenuText.Key(
            Loc.PropSpawner.SetsSpawned,
            ("count", MenuText.Literal(Number(placed.Count)))));

        Rebuild();
    }

    private void RemoveSpawned(SavedPropSetEntry entry)
    {
        var removed = SpawnedProps.DeleteSet(entry.Set.Name);

        if (removed == 0)
        {
            Notifications.Info(MenuText.Key(Loc.PropSpawner.SetsNothingStanding));

            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.PropSpawner.SetsUnspawned,
            ("count", MenuText.Literal(Number(removed)))));

        Rebuild();
    }

    private async Task AddPropAsync(SavedPropSetEntry entry)
    {
        if (entry.IsFromNewerBuild)
        {
            Notifications.Error(MenuText.Key(Loc.PropSpawner.SetsNewerBuild));

            return;
        }

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.PropSpawner.SpawnByNamePrompt),
            NameMaxLength,
            string.Empty,
            PropRecents.Suggestions(),
            suggestWhenEmpty: true);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var model = typed.Trim();

        // Placing is the only way a prop enters a set, which is what leaves no path to edit one.
        await PropPlacement.BeginAsync(PropModelNames.Remember(model), entity => Store(entry, model, entity));
    }

    private void Store(SavedPropSetEntry entry, string model, int entity)
    {
        var position = Native.GetEntityCoords(entity, true);

        var added = SavedPropSetStore.AddProp(entry, new SavedProp
        {
            Model = model,
            X = position.X,
            Y = position.Y,
            Z = position.Z,
            Heading = Native.GetEntityHeading(entity),
            Networked = PropSpawnOptions.Networked,
            Frozen = PropSpawnOptions.Frozen,
        });

        if (!added)
        {
            Notifications.Error(MenuText.Key(Loc.PropSpawner.SetsSaveFailed));

            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.PropSpawner.SetsPropAdded,
            ("name", MenuText.Literal(entry.Set.Name))));

        Rebuild();
    }

    private void RemoveProp(SavedPropSetEntry entry, int index)
    {
        if (!SavedPropSetStore.RemoveProp(entry, index))
        {
            Notifications.Error(MenuText.Key(
                entry.IsFromNewerBuild ? Loc.PropSpawner.SetsNewerBuild : Loc.PropSpawner.SetsSaveFailed));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.PropSpawner.SetsPropRemoved));

        Fill(_removeMenu, RemoveRows());

        PreviewRow(_removeMenu?.Builder.Menu.GetCurrentMenuItem());

        Rebuild();
    }

    private async Task RenameAsync(SavedPropSetEntry entry)
    {
        var answers = await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.PropSpawner.SetsName), NameMaxLength, entry.Set.Name),
            new InputPrompt(MenuText.Key(Loc.PropSpawner.SetsDescriptionPrompt), DescriptionMaxLength, entry.Set.Description));

        if (answers is null || string.IsNullOrWhiteSpace(answers[0]))
        {
            return;
        }

        if (!SavedPropSetStore.Edit(entry, answers[0].Trim(), answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(
                entry.IsFromNewerBuild ? Loc.PropSpawner.SetsNewerBuild : Loc.PropSpawner.SetsNameTaken));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.PropSpawner.SetsRenamed));

        Rebuild();
    }

    private void Delete(SavedPropSetEntry entry)
    {
        SavedPropSetStore.Delete(entry.Set.Name);

        Notifications.Success(MenuText.Key(Loc.PropSpawner.SetsDeleted));

        _selected = null;

        Rebuild();
    }

    private static void Report(SaveOutcome outcome)
    {
        if (outcome == SaveOutcome.Saved)
        {
            Notifications.Success(MenuText.Key(Loc.PropSpawner.SetsSaved));

            return;
        }

        Notifications.Error(MenuText.Key(outcome == SaveOutcome.NameTaken
            ? Loc.PropSpawner.SetsNameTaken
            : Loc.PropSpawner.SetsSaveFailed));
    }

    private void Rebuild()
    {
        if (_root is { } root)
        {
            Refill(root, RootRows());
        }

        Fill(_detailMenu, DetailRows());
    }

    private static void Fill(DetachedMenu? menu, IReadOnlyList<MenuEntry> rows)
    {
        if (menu is { } detached)
        {
            Refill(detached.Builder, rows);
        }
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

    private static string Where(SavedProp prop) => string.Format(
        CultureInfo.InvariantCulture,
        "{0:0.#}, {1:0.#}, {2:0.#}",
        prop.X,
        prop.Y,
        prop.Z);

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
