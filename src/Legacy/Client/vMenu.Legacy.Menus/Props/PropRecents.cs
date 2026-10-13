using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Props;

public static class PropRecents
{
    private const string StoreKey = "vmenu_props_recent";

    private const int SchemaVersion = 1;

    private const int MaxRecents = 20;

    public static IReadOnlyList<string> All => Read().Models;

    public static void Add(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return;
        }

        var stored = Read();

        stored.Models.RemoveAll(held => string.Equals(held, model, StringComparison.OrdinalIgnoreCase));
        stored.Models.Insert(0, model);

        if (stored.Models.Count > MaxRecents)
        {
            stored.Models.RemoveRange(MaxRecents, stored.Models.Count - MaxRecents);
        }

        KvpStore.TryWrite(StoreKey, KvpValueType.Json, SchemaVersion, stored);
    }

    public static void Clear() => KvpStore.Delete(StoreKey);

    public static IReadOnlyList<InputSuggestion> Suggestions()
    {
        var models = All;
        var rows = new InputSuggestion[models.Count];

        for (var index = 0; index < rows.Length; index++)
        {
            rows[index] = new InputSuggestion { Value = models[index], Label = models[index] };
        }

        return rows;
    }

    private static Stored Read() =>
        KvpStore.TryRead<Stored>(StoreKey, KvpValueType.Json, SchemaVersion, out var stored, out _)
        && stored is not null
            ? stored
            : new Stored();

    private sealed class Stored
    {
        public List<string> Models { get; set; } = [];
    }
}
