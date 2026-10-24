using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Localization;

/// <summary>Reports translation gaps once at startup, so the first sign of one is not a marker in a menu nobody
/// opened yet. English needs no check: the build refuses a key without it.</summary>
public static class LocalizationSelfCheck
{
    public static void Run()
    {
        var english = LanguageCatalog.English;

        foreach (var language in LanguageCatalog.Available)
        {
            if (!LanguageCatalog.TryGet(language, out var table))
            {
                continue;
            }

            // An entry English does not have is dead weight at best, and a mistyped key at worst: one that silently
            // never resolves, because the lookup asks for the key, not this string.
            var orphans = table.Keys.Where(key => !english.ContainsKey(key)).ToArray();

            if (orphans.Length > 0)
            {
                Log.Warning($"[Localization] '{language}' has {orphans.Length} entrie(s) English does not have: {string.Join(", ", orphans)}");
            }

            // Partial translations are by design, so coverage is reported rather than warned about.
            Log.Debug($"[Localization] '{language}' ({table.NativeName}): {english.Keys.Count(table.ContainsKey)}/{english.Keys.Count} key(s).");
        }
    }
}
