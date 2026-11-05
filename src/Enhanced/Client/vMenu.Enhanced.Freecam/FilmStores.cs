using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Freecam;

public enum FilmSaveOutcome
{
    Saved,
    NameTaken,
    Refused,
}

public sealed class FilmEntry<T>(T item, bool isFromNewerBuild)
{
    public T Item { get; } = item;

    public bool IsFromNewerBuild { get; } = isFromNewerBuild;
}

public sealed class FilmStore<T>
    where T : class, IFilmItem
{
    private readonly string _prefix;

    private readonly int _schemaVersion;

    internal FilmStore(string prefix, int schemaVersion)
    {
        _prefix = prefix;
        _schemaVersion = schemaVersion;
    }

    public List<FilmEntry<T>> All()
    {
        var entries = new List<FilmEntry<T>>();

        foreach (var key in KvpStore.Keys(_prefix))
        {
            if (Read(key) is { } entry)
            {
                entries.Add(entry);
            }
        }

        entries.Sort(static (left, right) => string.Compare(left.Item.Name, right.Item.Name, StringComparison.OrdinalIgnoreCase));

        return entries;
    }

    public FilmSaveOutcome Save(T item, bool replacing)
    {
        var key = _prefix + item.Name;

        if (!replacing && Read(key) is not null)
        {
            return FilmSaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, _schemaVersion, item)
            ? FilmSaveOutcome.Saved
            : FilmSaveOutcome.Refused;
    }

    public void Delete(string name) => KvpStore.Delete(_prefix + name);

    public FilmSaveOutcome Rename(T item, string name)
    {
        var old = item.Name;

        if (string.Equals(old, name, StringComparison.Ordinal))
        {
            return FilmSaveOutcome.Saved;
        }

        if (Read(_prefix + name) is not null)
        {
            return FilmSaveOutcome.NameTaken;
        }

        item.Name = name;

        if (Save(item, replacing: true) != FilmSaveOutcome.Saved)
        {
            item.Name = old;

            return FilmSaveOutcome.Refused;
        }

        if (old.Length > 0)
        {
            Delete(old);
        }

        return FilmSaveOutcome.Saved;
    }

    private FilmEntry<T>? Read(string key) =>
        KvpStore.TryRead<T>(key, KvpValueType.Json, _schemaVersion, out var item, out var version) && item is not null
            ? new FilmEntry<T>(item, version > _schemaVersion)
            : null;
}

public static class FilmStores
{
    public static FilmStore<FilmShot> Shots { get; } = new("vmenu_filmshot_", FilmShot.SchemaVersion);

    public static FilmStore<FilmPath> Paths { get; } = new("vmenu_filmpath_", FilmPath.SchemaVersion);
}
