using System.Diagnostics.CodeAnalysis;

using vMenu.Enhanced.MenuFramework.Localization.Languages;

namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>Every language vMenu can display.</summary>
// English is compiled in and every other language is a file, loaded by LanguageLoader. That split is
// deliberate: English is the fallback for every key another language leaves out, so it has to be
// complete and present before anything can render, which a file on disk cannot guarantee.
// Keyed with an explicit comparer, so no lookup reaches EqualityComparer<T>.Default, whose internal
// string comparer the sandbox blocks.
public static class LanguageCatalog
{
    private static readonly Dictionary<string, LanguageTable> Tables = new(StringComparer.Ordinal);

    private static readonly List<LanguageId> Order = [];

    static LanguageCatalog()
    {
        English = EnglishStrings.Table;

        // First, so the picker always lists it first whatever order the convar names the rest in.
        Register(English);
    }

    /// <summary>Always present, and the fallback for every key another language has not translated.</summary>
    public static LanguageTable English { get; }

    /// <summary>In registration order, English first, which is the order the language picker shows.</summary>
    public static IReadOnlyList<LanguageId> Available => Order;

    public static void Register(LanguageTable table)
    {
        if (Tables.TryAdd(table.Id.Code, table))
        {
            Order.Add(table.Id);
            return;
        }

        // Replacing in place keeps the picker order stable when a table is swapped at runtime.
        Tables[table.Id.Code] = table;
    }

    public static bool TryGet(LanguageId language, [MaybeNullWhen(false)] out LanguageTable table)
    {
        if (language.Code is { } code)
        {
            return Tables.TryGetValue(code, out table);
        }

        table = null;

        return false;
    }
}
