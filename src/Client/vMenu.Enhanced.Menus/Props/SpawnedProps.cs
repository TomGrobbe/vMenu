using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Props;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Props;

public static class SpawnedProps
{
    private const int MaxProps = 100;

    private static readonly List<int> Tracked = [];

    private static readonly Dictionary<string, List<int>> BySet = new(StringComparer.OrdinalIgnoreCase);

    public static int Count => Tracked.Count;

    public static void Initialize() => ResourceShutdown.Stopping += () => DeleteAll();

    public static bool TryTakeOrWarn()
    {
        Forget();

        if (Tracked.Count < MaxProps)
        {
            return true;
        }

        Notifications.Warning(MenuText.Key(
            Loc.PropSpawner.TooMany,
            ("max", MenuText.Literal(MaxProps.ToString()))));

        return false;
    }

    public static void Track(int entity)
    {
        if (entity != 0 && !Tracked.Contains(entity))
        {
            Tracked.Add(entity);
        }
    }

    public static void Delete(int entity)
    {
        Tracked.Remove(entity);

        if (entity == 0 || !Native.DoesEntityExist(entity))
        {
            return;
        }

        Unreport(entity);

        Native.SetEntityAsMissionEntity(entity, true, true);

        // Ref<T> cannot cross an await, and the out overload pushes a literal 0.
        var handle = entity;

        Native.DeleteObject(ref handle);
    }

    public static int DeleteAll()
    {
        var deleted = 0;

        foreach (var entity in Tracked.ToArray())
        {
            if (Native.DoesEntityExist(entity))
            {
                deleted++;
            }

            Delete(entity);
        }

        Tracked.Clear();

        return deleted;
    }

    public static int Newest() => Tracked.Count > 0 ? Tracked[^1] : 0;

    public static void RecordSet(string name, IReadOnlyList<int> entities) =>
        BySet[name] = [.. entities];

    public static int StandingFrom(string name)
    {
        if (!BySet.TryGetValue(name, out var entities))
        {
            return 0;
        }

        entities.RemoveAll(static entity => !Native.DoesEntityExist(entity));

        return entities.Count;
    }

    public static int DeleteSet(string name)
    {
        if (!BySet.TryGetValue(name, out var entities))
        {
            return 0;
        }

        var deleted = 0;

        foreach (var entity in entities.ToArray())
        {
            if (Native.DoesEntityExist(entity))
            {
                deleted++;
            }

            Delete(entity);
        }

        BySet.Remove(name);

        return deleted;
    }

    // Nullable, not 0 for absence: "is { }" matches any int and would treat 0 as a hit.
    public static int? FindNear(uint model, Vector3 position, float radius)
    {
        var reach = radius * radius;

        foreach (var entity in Tracked)
        {
            if (!Native.DoesEntityExist(entity) || (uint)Native.GetEntityModel(entity) != model)
            {
                continue;
            }

            if (Vector3.DistanceSquared(position, Native.GetEntityCoords(entity, false)) <= reach)
            {
                return entity;
            }
        }

        return null;
    }

    public static bool Owns(int entity) => Tracked.Contains(entity);

    public static List<int> Mine()
    {
        Forget();

        return [.. Tracked];
    }

    private static void Unreport(int entity)
    {
        if (!Native.NetworkGetEntityIsNetworked(entity))
        {
            return;
        }

        API.EmitServer(PropEvents.Removed, Native.NetworkGetNetworkIdFromEntity(entity));
    }

    private static void Forget() => Tracked.RemoveAll(static entity => !Native.DoesEntityExist(entity));
}
