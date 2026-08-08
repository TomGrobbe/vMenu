using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Extensions;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Teleport;

using TeleportMenuPermissions = vMenu.Enhanced.Data.Permissions.Menus.TeleportMenu;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.TeleportMenu.Title,
    SubtitleKey = Loc.TeleportMenu.Subtitle,
    DescriptionKey = Loc.TeleportMenu.LinkDescription,
    Permission = TeleportMenuPermissions.Menu)]
public sealed class TeleportMenu : MenuDefinition
{
    private const int TextLength = 50;

    private DetachedMenu? _categoryMenu;

    private DetachedMenu? _locationMenu;

    private TeleportCategory? _selected;

    private TeleportCategory? _builtFor;

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.TeleportMenu.TeleportToWaypoint),
            Description = MenuText.Key(Loc.TeleportMenu.TeleportToWaypointDescription),
            Gate = TeleportMenuPermissions.Waypoint,
            OnSelectedAsync = _ => TeleportTargets.ToWaypointAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.TeleportMenu.TeleportToCoords),
            Description = MenuText.Key(Loc.TeleportMenu.TeleportToCoordsDescription),
            Gate = TeleportMenuPermissions.Coords,
            OnSelectedAsync = _ => TeleportTargets.ToTypedCoordsAsync(),
        });

        // One menu shared by every category row rather than one per row, so nothing is registered
        // per refresh. The row records which category it was before opening it.
        _locationMenu = menu.AddDetachedMenu(
            MenuText.From(() => _selected?.Name ?? string.Empty),
            MenuText.From(() => _selected?.Description ?? string.Empty),
            _ => { });

        // Only when the player picked a different category. Reopening the same one leaves the rows
        // alone, so the highlight stays where they left it.
        _locationMenu.Builder.OnOpened = _ =>
        {
            if (ReferenceEquals(_builtFor, _selected))
            {
                return;
            }

            _builtFor = _selected;

            RebuildLocations(keepIndex: false);
        };

        // The row's description is a sentence and runs off the side of the subtitle bar, so the bar
        // repeats the short title instead, like every menu declared through VMenu does.
        _categoryMenu = menu.AddDetachedMenu(
            MenuText.Key(Loc.TeleportMenu.TeleportCategories),
            MenuText.Key(Loc.TeleportMenu.TeleportCategories),
            _ => { });

        // Nothing is fetched when the menu opens. The rows come from the local copy, which the server
        // refreshes when this client joins and again whenever anybody adds something.
        TeleportSync.Changed += OnCategoriesChanged;

        RebuildCategories();

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.TeleportMenu.TeleportCategories),
            Description = MenuText.Key(Loc.TeleportMenu.TeleportCategoriesDescription),
            // Without this the row opens for anybody, and a player the server withholds the list
            // from just finds an empty menu instead of a locked row.
            Gate = TeleportMenuPermissions.Category,
            OnSelected = _ => _categoryMenu?.Open(),
        });
    }

    // The list is replaced wholesale, so whatever the player had picked is a stale object now and has
    // to be looked up again by name.
    private void OnCategoriesChanged()
    {
        _selected = _selected is { } previous ? Find(previous.Name) : null;
        _builtFor = _selected;

        RebuildCategories();
        RebuildLocations(keepIndex: true);
    }

    private static TeleportCategory? Find(string name)
    {
        foreach (var category in TeleportSync.Categories)
        {
            if (string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return category;
            }
        }

        return null;
    }

    private void RebuildCategories()
    {
        if (_categoryMenu is not { } categoryMenu)
        {
            return;
        }

        var rows = new List<MenuEntry>();

        foreach (var category in TeleportSync.Categories)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.From(() => category.Name),
                Description = MenuText.From(() => category.Description),
                OnSelected = _ =>
                {
                    _selected = category;

                    _locationMenu?.Open();
                },
            });
        }

        if (rows.Count == 0)
        {
            rows.Add(Placeholder(Loc.TeleportMenu.NoCategories, Loc.TeleportMenu.NoCategoriesDescription));
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.TeleportMenu.CreateCategory),
            Description = MenuText.Key(Loc.TeleportMenu.CreateCategoryDescription),
            Gate = TeleportMenuPermissions.Manage,
            OnSelectedAsync = _ => CreateCategoryAsync(),
        });

        Fill(categoryMenu, rows, keepIndex: true);
    }

    private void RebuildLocations(bool keepIndex)
    {
        if (_locationMenu is not { } locationMenu)
        {
            return;
        }

        var rows = new List<MenuEntry>();

        foreach (var location in _selected?.Locations ?? [])
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.From(() => location.Name),
                Description = MenuText.From(() => location.Description),
                OnSelectedAsync = _ => PlayerTeleport.ToCoordsAsync(
                    new Vector3(location.Position.X, location.Position.Y, location.Position.Z),
                    location.Heading),
            });
        }

        if (rows.Count == 0)
        {
            rows.Add(Placeholder(Loc.TeleportMenu.NoLocations, Loc.TeleportMenu.NoLocationsDescription));
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.TeleportMenu.CreatePosition),
            Description = MenuText.Key(Loc.TeleportMenu.CreatePositionDescription),
            Gate = TeleportMenuPermissions.Manage,
            OnSelectedAsync = _ => CreatePositionAsync(),
        });

        Fill(locationMenu, rows, keepIndex);
    }

    // Rebuilding drops every item, and MenuAPI puts the highlight back on the first one, so a rebuild
    // under a menu the player is already looking at moves their selection out from under them.
    private static void Fill(DetachedMenu menu, List<MenuEntry> rows, bool keepIndex)
    {
        var was = menu.Menu.CurrentIndex;
        var offset = menu.Menu.ViewIndexOffset;

        menu.Builder.ClearEntries();
        menu.Builder.AddRange(rows);

        var keep = keepIndex && was < menu.Menu.GetMenuItems().Count;

        // The offset as well as the index, or a player partway down a long list keeps their row but
        // has the list itself scrolled back to the top under them.
        menu.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }

    // Nothing is added locally. The server writes the file and sends the new list to everybody,
    // including this client, and the rows are rebuilt from that.
    private static async Task CreateCategoryAsync()
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.TeleportMenu.CategoryName), TextLength),
            new InputPrompt(MenuText.Key(Loc.TeleportMenu.CategoryDescription), TextLength)) is not { } answers)
        {
            return;
        }

        Report(await ServerActions.InvokeAsync(
            ActionIds.TeleportMenu.AddCategory,
            answers[0],
            answers[1]));
    }

    private async Task CreatePositionAsync()
    {
        if (_selected is not { } category)
        {
            return;
        }

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.TeleportMenu.PositionName), TextLength),
            new InputPrompt(MenuText.Key(Loc.TeleportMenu.PositionDescription), TextLength)) is not { } answers)
        {
            return;
        }

        var position = API.Players.Local.Position;

        // Off the ped rather than the vehicle: a ped sitting in one already reports the vehicle's
        // heading, so this is right either way.
        var heading = API.Players.Local.Ped is { } ped ? Native.GetEntityHeading(ped.Handle) : 0f;

        Report(await ServerActions.InvokeAsync(
            ActionIds.TeleportMenu.AddLocation,
            category.Name,
            answers[0],
            answers[1],
            Coord(position.X),
            Coord(position.Y),
            Coord(position.Z),
            Coord(heading)));
    }

    private static string Coord(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    // A row rather than an empty menu: MenuAPI ignores every direction key while a menu has no items.
    private static ButtonEntry Placeholder(string text, string description) => new()
    {
        Text = MenuText.Key(text),
        Description = MenuText.Key(description),
    };

    private static void Report(ActionResult result)
    {
        if (result.IsOk)
        {
            Notifications.Success(MenuText.Key(Loc.TeleportMenu.Saved));

            return;
        }

        Notifications.Error(MenuText.Key(result.Status switch
        {
            ActionStatus.Denied => Loc.TeleportMenu.SaveDenied,
            ActionStatus.Refused => Loc.TeleportMenu.NameTaken,
            ActionStatus.NotFound => Loc.TeleportMenu.CategoryGone,
            _ => Loc.TeleportMenu.SaveFailed,
        }));
    }

}
