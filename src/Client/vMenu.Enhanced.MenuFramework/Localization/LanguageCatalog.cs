using System.Diagnostics.CodeAnalysis;

using vMenu.Enhanced.MenuFramework.Localization.Languages;

namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>Every language vMenu can display.</summary>
// Tables are compiled C#. A file backed set is possible through ClientJson, and the way there is to
// replace CompiledLocalizer with a file backed ILocalizer rather than add a loader here.
// Keyed with an explicit comparer, so no lookup reaches EqualityComparer<T>.Default, whose internal
// string comparer the sandbox blocks.
public static class LanguageCatalog
{
    private static readonly Dictionary<string, LanguageTable> Tables = new(StringComparer.Ordinal);

    private static readonly List<LanguageId> Order = [];

    static LanguageCatalog()
    {
        English = EnglishStrings.Table;

        // Registration order is the order the language picker lists them, English first.
        Register(English);
        Register(SpanishStrings.Table);
        Register(GermanStrings.Table);
        Register(FrenchStrings.Table);
        Register(DutchStrings.Table);
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
