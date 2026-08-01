using System.Diagnostics.CodeAnalysis;

using vMenu.Enhanced.Localization.Languages;

namespace vMenu.Enhanced.Localization;

/// <summary>
/// Every language vMenu can display.
/// </summary>
/// <remarks>
/// Tables are compiled C# rather than files on disk because the CitizenFX Enhanced sandbox cannot
/// currently parse JSON on the client. Do not "fix" this by adding a file loader here: it is the
/// runtime that is missing, not the code. When client side JSON works, replace
/// <see cref="CompiledLocalizer"/> with a file backed <see cref="ILocalizer"/> — no menu declaration
/// has to change.
/// <para>
/// Keyed by the raw code with an explicit comparer, so no lookup ever reaches
/// <c>EqualityComparer&lt;T&gt;.Default</c>. The sandbox blocks the internal string comparer it
/// resolves to.
/// </para>
/// </remarks>
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
