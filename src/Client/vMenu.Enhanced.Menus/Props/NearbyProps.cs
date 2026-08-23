using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;

using PropSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.PropSpawner;

namespace vMenu.Enhanced.Menus.Props;

internal static class NearbyProps
{
    private const string ObjectPool = "CObject";

    private const float Range = 30f;

    private const int MaxRows = 40;

    private static MenuBuilder? _menu;

    internal static bool CanManage => ClientPermissions.IsAllowed(PropSpawnerPermissions.Manage);

    internal static void Build(MenuBuilder menu)
    {
        _menu = menu;

        menu.OnOpened = _ =>
        {
            Refill(menu);

            // Asked of the menu: the rows were just rebuilt, so the event's item is stale.
            Preview(menu.Menu.GetCurrentMenuItem());
        };

        menu.OnIndexChanged = changed => Preview(changed.NewItem);

        menu.OnClosed = _ => PropPreview.Hide();
    }

    private static void Preview(MenuAPI.MenuItem? item)
    {
        if (item?.ItemData is int entity && entity != 0)
        {
            PropPreview.Highlight(entity);

            return;
        }

        PropPreview.Hide();
    }

    private static void Refill(MenuBuilder menu)
    {
        menu.ClearEntries();
        menu.AddRange(Rows());
    }

    private static IReadOnlyList<MenuEntry> Rows()
    {
        var found = Find();

        if (found.Count == 0)
        {
            return
            [
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.PropSpawner.NearbyEmpty),
                    Description = MenuText.Key(Loc.PropSpawner.NearbyEmptyDescription),
                    ReadEnabled = static () => false,
                },
            ];
        }

        var rows = new List<MenuEntry>(found.Count);

        foreach (var prop in found)
        {
            var entity = prop.Entity;

            rows.Add(new ConfirmButtonEntry
            {
                Text = MenuText.Literal(Name(prop)),
                Description = MenuText.Key(
                    prop.Mine ? Loc.PropSpawner.NearbyRowMine : Loc.PropSpawner.NearbyRowOther,
                    ("distance", MenuText.Literal(Metres(prop.Distance)))),
                ConfirmationDescription = MenuText.Key(Loc.PropSpawner.NearbyRemoveConfirm),

                Configure = item => item.ItemData = entity,
                OnConfirmed = _ => Remove(entity),
            });
        }

        return rows;
    }

    private static void Remove(int entity)
    {
        PropPreview.Hide();

        if (!SpawnedProps.Owns(entity) && !CanManage)
        {
            Notifications.Error(MenuText.Key(Loc.PropSpawner.NearbyDenied));

            return;
        }

        SpawnedProps.Delete(entity);

        Notifications.Success(MenuText.Key(Loc.PropSpawner.NearbyRemoved));

        if (_menu is not { } menu)
        {
            return;
        }

        Refill(menu);

        Preview(menu.Menu.GetCurrentMenuItem());
    }

    private static List<Found> Find()
    {
        var manage = CanManage;
        var origin = Native.GetEntityCoords(Native.PlayerPedId(), true);
        var reach = Range * Range;
        var found = new List<Found>();

        foreach (var entity in NativeFixer.GetGamePool(ObjectPool))
        {
            if (!Native.DoesEntityExist(entity))
            {
                continue;
            }

            var mine = SpawnedProps.Owns(entity);

            if (!mine && !manage)
            {
                continue;
            }

            var apart = Vector3.DistanceSquared(origin, Native.GetEntityCoords(entity, false));

            if (apart > reach)
            {
                continue;
            }

            found.Add(new Found(entity, (uint)Native.GetEntityModel(entity), MathF.Sqrt(apart), mine));
        }

        found.Sort(static (left, right) => left.Distance.CompareTo(right.Distance));

        if (found.Count > MaxRows)
        {
            found.RemoveRange(MaxRows, found.Count - MaxRows);
        }

        return found;
    }

    private static string Name(Found prop) =>
        PropModelNames.Of(prop.Model) is { Length: > 0 } known
            ? known
            : prop.Model.ToString(CultureInfo.InvariantCulture);

    private static string Metres(float distance) =>
        distance.ToString("0.#", CultureInfo.InvariantCulture);

    private readonly record struct Found(int Entity, uint Model, float Distance, bool Mine);
}
