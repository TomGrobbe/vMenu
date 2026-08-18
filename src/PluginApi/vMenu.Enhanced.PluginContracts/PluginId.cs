namespace vMenu.Enhanced.PluginContracts;

/// <summary>
/// Turns a resource name into the identifier used inside ACE permission names and convar
/// names, where only ASCII letters, digits and underscores are valid segment characters.
/// Both sides sanitize identically so names always line up.
/// </summary>
public static class PluginId
{
    public static string Sanitize(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            return string.Empty;
        }

        var characters = resourceName.ToCharArray();

        for (var index = 0; index < characters.Length; index++)
        {
            if (!char.IsAsciiLetterOrDigit(characters[index]))
            {
                characters[index] = '_';
            }
        }

        return new string(characters);
    }
}
