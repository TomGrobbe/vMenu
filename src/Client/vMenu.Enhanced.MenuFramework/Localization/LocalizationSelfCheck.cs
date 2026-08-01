using System.Reflection;

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Localization;

/// <summary>
/// Reports localization gaps once at startup.
/// </summary>
/// <remarks>
/// A <see cref="Loc"/> constant guarantees the key exists in code, not that any table has text for
/// it. Without this the first sign of a gap is a marker appearing in a menu nobody opened yet, so
/// this turns it into one console line at boot.
/// </remarks>
public static class LocalizationSelfCheck
{
    public static void Run()
    {
        var keys = CollectKeys(typeof(Loc));

        var missing = keys.Where(key => !LanguageCatalog.English.TryGet(key, out _)).ToArray();

        if (missing.Length > 0)
        {
            API.Log.Error($"[i18n] The English table is missing {missing.Length} key(s): {string.Join(", ", missing)}");
        }

        foreach (var language in LanguageCatalog.Available)
        {
            if (!LanguageCatalog.TryGet(language, out var table))
            {
                continue;
            }

            // An entry nobody names is dead weight at best, and a mistyped key at worst — one that
            // silently never resolves, because the lookup asks for the constant, not this string.
            var orphans = table.Keys.Where(key => !keys.Contains(key)).ToArray();

            if (orphans.Length > 0)
            {
                API.Log.Warn($"[i18n] '{language}' has {orphans.Length} entrie(s) no Loc constant names: {string.Join(", ", orphans)}");
            }

            // Partial translations are by design, so coverage is reported rather than warned about.
            API.Log.Debug($"[i18n] '{language}' ({table.NativeName}): {keys.Count(table.ContainsKey)}/{keys.Count} key(s).");
        }
    }

    private static HashSet<string> CollectKeys(Type container)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        Walk(container);

        return keys;

        void Walk(Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                // A const string is a literal, non-readonly field.
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string)
                    && field.GetRawConstantValue() is string value)
                {
                    keys.Add(value);
                }
            }

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public))
            {
                Walk(nested);
            }
        }
    }
}
