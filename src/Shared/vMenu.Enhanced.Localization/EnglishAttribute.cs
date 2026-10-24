using System.Diagnostics;

namespace vMenu.Enhanced.Localization;

/// <summary>The English text for a <see cref="Loc"/> key, which is also the fallback for every other language. The
/// build writes the English table from these, so a key without one does not compile.</summary>
/// <remarks>Only the source keeps these. The compiler leaves them out of the assembly, so no string is stored
/// twice.</remarks>
[AttributeUsage(AttributeTargets.Field)]
[Conditional("VMENU_ENGLISH_SOURCE")]
internal sealed class EnglishAttribute : Attribute
{
    /// <param name="description">Stored under the key plus <c>.desc</c>, which is where a menu row built from the key
    /// looks for its description. Leave it out for a key that has none.</param>
    public EnglishAttribute(string text, string? description = null)
    {
        Text = text;
        Description = description;
    }

    public string Text { get; }

    public string? Description { get; }
}
