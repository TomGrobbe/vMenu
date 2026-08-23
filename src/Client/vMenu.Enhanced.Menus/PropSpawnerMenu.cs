using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Props;

using PropSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.PropSpawner;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.PropSpawner.Title,
    SubtitleKey = Loc.PropSpawner.Subtitle,
    DescriptionKey = Loc.PropSpawner.LinkDescription,
    Permission = PropSpawnerPermissions.Menu)]
public sealed class PropSpawnerMenu : MenuDefinition
{
    private const int ModelMaxLength = 60;

    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    protected override void Build(MenuBuilder menu)
    {
        menu.OnOpened = _ => MenuRegistry.Refresh(menu.Menu);

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PropSpawner.SpawnByName),
            Description = MenuText.Key(Loc.PropSpawner.SpawnByNameDescription),
            Gate = PropSpawnerPermissions.Spawn,
            OnSelectedAsync = _ => SpawnByNameAsync(),
        });

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.PropSpawner.Recents),
            Description = MenuText.Key(Loc.PropSpawner.RecentsDescription),
            MenuSubtitle = MenuText.Key(Loc.PropSpawner.RecentsSubtitle),
            Gate = PropSpawnerPermissions.Spawn,
            Build = BuildRecents,
        });

        menu.Entries.Add(SubmenuEntry.For(new SavedPropSetsMenu()));

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PropSpawner.Networked),
            Description = MenuText.Key(Loc.PropSpawner.NetworkedDescription),
            LockedDescription = MenuText.Key(Loc.PropSpawner.NetworkedLocked),
            Gate = PropSpawnerPermissions.Networked,
            ReadState = () => PropSpawnOptions.Networked,
            OnChanged = changed => PropSpawnOptions.SetNetworked(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PropSpawner.Frozen),
            Description = MenuText.Key(Loc.PropSpawner.FrozenDescription),
            ReadState = () => PropSpawnOptions.Frozen,
            OnChanged = changed => PropSpawnOptions.SetFrozen(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PropSpawner.SnapToGround),
            Description = MenuText.Key(Loc.PropSpawner.SnapToGroundDescription),
            ReadState = () => PropSpawnOptions.SnapToGround,
            OnChanged = changed => PropSpawnOptions.SetSnapToGround(changed.Checked),
        });

        menu.Entries.Add(new SliderEntry
        {
            Text = MenuText.Key(Loc.PropSpawner.Distance),
            Description = Reach(),
            Min = PropSpawnOptions.MinDistance,
            Max = PropSpawnOptions.MaxDistance,
            ReadPosition = () => PropSpawnOptions.Distance,
            OnMoved = moved =>
            {
                PropSpawnOptions.Distance = moved.NewPosition;

                moved.Item.Description = Reach().Resolve(Localizer.Current);
            },
        });

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.PropSpawner.Nearby),
            Description = MenuText.Key(Loc.PropSpawner.NearbyDescription),
            MenuSubtitle = MenuText.Key(Loc.PropSpawner.NearbySubtitle),
            Gate = MenuGate.Permission(PropSpawnerPermissions.Delete)
                | MenuGate.Permission(PropSpawnerPermissions.Manage),
            Build = NearbyProps.Build,
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.PropSpawner.DeleteAll),
            Description = MenuText.Key(Loc.PropSpawner.DeleteAllDescription),
            ConfirmationDescription = MenuText.Key(Loc.PropSpawner.DeleteAllConfirm),
            Gate = PropSpawnerPermissions.Delete,
            ReadEnabled = () => SpawnedProps.Count > 0,
            OnConfirmed = _ => DeleteAll(),
        });
    }

    private static MenuText Reach() => MenuText.Key(
        Loc.PropSpawner.DistanceDescription,
        ("metres", MenuText.From(() => PropSpawnOptions.Distance.ToString(CultureInfo.InvariantCulture))));

    private static async Task SpawnByNameAsync()
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.PropSpawner.SpawnByNamePrompt),
            ModelMaxLength,
            string.Empty,
            PropRecents.Suggestions(),
            suggestWhenEmpty: true);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        await PropPlacement.BeginAsync(PropModelNames.Remember(typed.Trim()));
    }

    private static void BuildRecents(MenuBuilder menu)
    {
        menu.OnOpened = _ =>
        {
            menu.ClearEntries();
            menu.AddRange(RecentRows());
        };
    }

    private static IReadOnlyList<MenuEntry> RecentRows()
    {
        var models = PropRecents.All;

        if (models.Count == 0)
        {
            return
            [
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.PropSpawner.RecentsEmpty),
                    Description = MenuText.Key(Loc.PropSpawner.RecentsEmptyDescription),
                    ReadEnabled = static () => false,
                },
            ];
        }

        var rows = new List<MenuEntry>(models.Count);

        foreach (var model in models)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(model),
                Description = MenuText.Key(Loc.PropSpawner.RecentRowDescription),
                OnSelectedAsync = _ => PropPlacement.BeginAsync(PropModelNames.Remember(model)),
            });
        }

        return rows;
    }

    private static void DeleteAll()
    {
        var deleted = SpawnedProps.DeleteAll();

        if (deleted == 0)
        {
            Notifications.Info(MenuText.Key(Loc.PropSpawner.NothingToDelete));

            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.PropSpawner.Deleted,
            ("count", MenuText.Literal(deleted.ToString(CultureInfo.InvariantCulture)))));
    }
}
