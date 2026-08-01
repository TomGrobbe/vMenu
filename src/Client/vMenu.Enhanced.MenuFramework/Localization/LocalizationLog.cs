using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>
/// Deduplicated reporting for localization gaps.
/// </summary>
/// <remarks>
/// Every relabel pass walks every entry, so an unguarded log would repeat the same line on every
/// permission resync and language switch. Reporting each gap once keeps it findable.
/// </remarks>
internal static class LocalizationLog
{
    private static readonly HashSet<string> Reported = new(StringComparer.Ordinal);

    internal static void MissingKey(string key)
    {
        if (Reported.Add($"key:{key}"))
        {
            API.Log.Error($"[i18n] No '{LanguageId.English}' text for key '{key}'. Add it to the English table.");
        }
    }

    internal static void UnknownPlaceholder(string name, string template)
    {
        if (Reported.Add($"arg:{name}:{template}"))
        {
            API.Log.Error($"[i18n] No argument named '{name}' was supplied for \"{template}\".");
        }
    }

    /// <summary>Lets a reload surface the same gaps again.</summary>
    internal static void Reset() => Reported.Clear();
}
