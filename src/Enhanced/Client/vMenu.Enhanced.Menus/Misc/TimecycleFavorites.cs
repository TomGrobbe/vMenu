using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Misc;

public static class TimecycleFavorites
{
    private const string StoreKey = "vmenu_timecycle_favorites";

    private const int SchemaVersion = 1;

    private static HashSet<string>? _names;

    public static event Action? Changed;

    public static int Count => Load().Count;

    public static bool IsFavorite(string name) => Load().Contains(name);

    public static void Toggle(string name)
    {
        var names = Load();

        if (!names.Remove(name))
        {
            names.Add(name);
        }

        KvpStore.TryWrite(StoreKey, KvpValueType.Json, SchemaVersion, new Stored { Names = [.. names] });

        Changed?.Invoke();
    }

    private static HashSet<string> Load()
    {
        if (_names is not null)
        {
            return _names;
        }

        _names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (KvpStore.TryRead<Stored>(StoreKey, KvpValueType.Json, SchemaVersion, out var stored, out _)
            && stored?.Names is { } names)
        {
            foreach (var name in names)
            {
                if (TimecycleCatalog.Find(name) is { } known)
                {
                    _names.Add(known);
                }
            }
        }

        return _names;
    }

    private sealed class Stored
    {
        public List<string> Names { get; set; } = [];
    }
}
