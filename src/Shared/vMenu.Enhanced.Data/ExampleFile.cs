using System.Text;

namespace vMenu.Enhanced.Data;

/// <summary>
/// The warning header shared by every generated <c>.example</c> file.
/// </summary>
public static class ExampleFile
{
    public const string Extension = ".example";

    public const string ConfigDirectory = "config";

    /// <summary>Every plugin's templates live here, each one named after the resource it came from.</summary>
    // One shipped folder rather than a folder per plugin: SaveResourceFile writes files and never
    // folders, and no native makes one, so a folder named after a plugin could never be created.
    public const string PluginsDirectory = ConfigDirectory + "/plugins";

    /// <summary>What a plugin's copy of one of these templates should be called.</summary>
    public static string PluginCopyName(string resource, string copyName) => resource + "." + copyName;

    private const string Rule = "###############################################################################";

    public static string Banner(string copyName, params string[] extraNotes) =>
        BannerIn(ConfigDirectory, copyName, extraNotes);

    /// <summary>The same banner, for a file that does not sit directly in the config directory.</summary>
    public static string BannerIn(string directory, string copyName, params string[] extraNotes)
    {
        var banner = new StringBuilder();

        banner.Append(Rule).Append('\n');
        banner.Append("#  THIS FILE IS REGENERATED EVERY TIME vMenu Enhanced STARTS.\n");
        banner.Append("#  ANY CHANGES YOU MAKE TO IT WILL BE LOST.\n");
        banner.Append("#\n");
        banner.Append("#  To use it:\n");
        // Concatenated rather than interpolated: the FiveM sandbox refuses StringBuilder's
        // interpolated string handler, so an interpolated argument here fails to load the assembly.
        banner.Append("#    1. Copy this file and name the copy '" + copyName + "'.\n");
        banner.Append("#    2. Edit that copy, never this one.\n");
        banner.Append("#    3. Exec it from your server.cfg ABOVE the line that starts vMenu:\n");
        banner.Append("#\n");
        banner.Append("#         exec @vMenu.Enhanced/" + directory + "/" + copyName + "\n");
        banner.Append("#         ensure vMenu.Enhanced\n");

        foreach (var note in extraNotes)
        {
            banner.Append("#\n").Append(Comment(note, "# "));
        }

        banner.Append(Rule).Append('\n');

        return banner.ToString();
    }

    /// <summary>Wraps prose into <c>#</c> comment lines.</summary>
    public static string Comment(string text, string prefix = "#", int width = 78)
    {
        var comment = new StringBuilder();
        var line = new StringBuilder(prefix);

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length + 1 + word.Length > width && line.Length > prefix.Length)
            {
                comment.Append(line).Append('\n');
                line.Clear().Append(prefix);
            }

            line.Append(' ').Append(word);
        }

        return comment.Append(line).Append('\n').ToString();
    }
}
