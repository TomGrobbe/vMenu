namespace vMenu.Enhanced.Data.Configuration;

// Rules of the dotted setting naming scheme. Names are convars, so both sides read them with the
// exact string declared here and nothing composes a prefix at runtime.
public static class ConfigPath
{
    public const string Root = "vMenu.Enhanced";

    public const char Separator = '.';

    // Whether a segment survives a convar lookup. Anything else could never be set.
    public static bool IsValidSegment(string segment)
    {
        if (string.IsNullOrEmpty(segment))
        {
            return false;
        }

        foreach (var character in segment)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character != '_')
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name) || !name.StartsWith(Root + Separator, StringComparison.Ordinal))
        {
            return false;
        }

        return name[(Root.Length + 1)..].Split(Separator).All(IsValidSegment);
    }
}
