using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.MenuFramework.Localization;

// Deduplicated reporting for localization gaps. Every relabel pass walks every entry, so an
// unguarded log would repeat the same line on every permission resync and language switch.
internal static class LocalizationLog
{
    private static readonly HashSet<string> Reported = new(StringComparer.Ordinal);

    internal static void MissingKey(string key)
    {
        if (Reported.Add($"key:{key}"))
        {
            Log.Error($"[i18n] No '{LanguageId.English}' text for key '{key}'. Add it to the English table.");
        }
    }

    internal static void UnknownPlaceholder(string name, string template)
    {
        if (Reported.Add($"arg:{name}:{template}"))
        {
            Log.Error($"[i18n] No argument named '{name}' was supplied for \"{template}\".");
        }
    }

    // Lets a reload surface the same gaps again.
    internal static void Reset() => Reported.Clear();
}
