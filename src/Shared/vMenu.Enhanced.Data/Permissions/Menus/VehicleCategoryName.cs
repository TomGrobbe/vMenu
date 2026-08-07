using System.Text;

namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Turns a category name a server owner typed into something usable as a permission segment.
/// </summary>
/// <remarks>
/// Shared by both sides on purpose: the server decides which permission to register from a name,
/// and the client decides which permission to check from that same name, so the two can never
/// disagree about what "Police Cars" resolves to.
/// </remarks>
public static class VehicleCategoryName
{
    /// <summary>
    /// Lowercased, with every run of unusable characters collapsed into one underscore. Empty when
    /// the name had nothing usable in it at all.
    /// </summary>
    public static string ToPermissionSegment(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var segment = new StringBuilder(name.Length);

        foreach (var character in name)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                segment.Append(char.ToLowerInvariant(character));
                continue;
            }

            if (segment.Length > 0 && segment[^1] != '_')
            {
                segment.Append('_');
            }
        }

        while (segment.Length > 0 && segment[^1] == '_')
        {
            segment.Length--;
        }

        return segment.ToString();
    }
}
