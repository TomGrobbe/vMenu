using System.Text;

namespace vMenu.Enhanced.Data.Permissions;

public sealed class PermissionExampleEntry(
    string name,
    int depth,
    string? source,
    bool isStaffOnly,
    IReadOnlyList<string> extraParents)
{
    public string Name { get; } = name;

    public int Depth { get; } = depth;

    // Null when vMenu declares the permission itself.
    public string? Source { get; } = source;

    public bool IsStaffOnly { get; } = isStaffOnly;

    public IReadOnlyList<string> ExtraParents { get; } = extraParents;
}

public static class PermissionsExample
{
    public const string CopyName = "permissions.cfg";

    private const string EveryoneGroup = "builtin.everyone";

    private const string StaffGroup = "group.admin";

    // Entries at or above this depth get a blank line before them.
    private const int SpacedDepth = 1;

    public static string ResourcePath => $"{ExampleFile.ConfigDirectory}/{CopyName}{ExampleFile.Extension}";

    public static string PluginResourcePath(string resource) =>
        $"{ExampleFile.PluginsDirectory}/{ExampleFile.PluginCopyName(resource, CopyName)}{ExampleFile.Extension}";

    public static string Render(IEnumerable<PermissionExampleEntry> entries)
    {
        var ordered = entries.ToList();

        var file = new StringBuilder();

        file.Append(ExampleFile.Banner(
            CopyName,
            "Permissions are checked live. HOWEVER, the permissions.cfg does not re-execute itself. You can either " +
            "execute it manually again, but I do not recommend this if you made big changes, because conflicting aces " +
            "and principals will cause issues. Instead, if all you did was add somebody to a group, simply execute that " +
            "one command in the server console manually. Then execute `vmenu_refresh_permissions` in the server console " +
            "and every person should have their menu permissions refreshed automatically. " +
            "For big permissions.cfg changes I still recommend to restart your server!"
            ));

        file.Append('\n');
        file.Append(ExampleFile.Comment(
            "vMenu needs these two lines so an integration's role sync can add and remove permission groups "
            + "for players. They do nothing if you do not use role sync."));
        file.Append("add_ace resource.vMenu.Enhanced command.add_principal allow\n");
        file.Append("add_ace resource.vMenu.Enhanced command.remove_principal allow\n");

        file.Append('\n');
        file.Append(ExampleFile.Comment(
            "Give a player a group by one of their identifiers. "
            + "Use whichever you can look up most easily. Note, Steam "
            + "identifiers no longer work in FiveM Enhanced, so use either "
            + "license, license2, discord or fivem identifiers. (You can also use "
            + "the IP identifier, but I don't really recommended that one). "
            + "!!! Make sure you replace these with your own identifiers and remove the # !!! "
            + "If you're using another system to add people to principal groups during runtime "
            + "(for example TxAdmin for your admins), then you do not need to manually add them here."));
        file.Append($"# add_principal identifier.discord:223799456162775043 {StaffGroup}\n");
        file.Append($"# add_principal identifier.fivem:25104 {StaffGroup}\n");

        file.Append('\n');
        file.Append(ExampleFile.Comment(
            $"Groups inherit from each other (if you set it up correctly). This one gives everybody in {StaffGroup} everything "
            + $"group.mod may do, on top of whatever {StaffGroup} is granted below."));
        file.Append($"add_principal {StaffGroup} group.mod\n");

        file.Append('\n');
        file.Append(ExampleFile.Comment(
            "Every permission vMenu Enhanced knows about is listed below, ready to run as it is. "
            + "Each line already suggests who gets it: " + EveryoneGroup + " for the permissions "
            + "that are fine for any player, and " + StaffGroup + " for the few that should stay "
            + "with your staff. Change that principal if somebody else should have them."));

        file.Append("#\n");
        file.Append(ExampleFile.Comment(
            "A line that has other lines indented under it hands out every one of them, so it is "
            + "suggested to " + StaffGroup + " as soon as anything below it is. That is why a .All "
            + "can say " + StaffGroup + " while most of what sits under it says " + EveryoneGroup
            + ". Those lines are still there on their own, so your players keep them."));

        file.Append("#\n");
        file.Append(ExampleFile.Comment(
            "A permission ending in .All grants everything nested underneath it, so keeping only the "
            + ".All line is usually all you need. When you want to restrict some features, delete "
            + "the .All and keep only the specific lines below it instead, or put a # in front of "
            + "the ones you do not want. "
            + "Checkout https://docs.vespura.com/vMenu/Enhanced/ for more permissions information."));

        file.Append("#\n");
        file.Append(ExampleFile.Comment(
            "Permissions that a plugin brings along are not listed here. Every plugin gets its own "
            + "pair of templates in " + ExampleFile.PluginsDirectory + "/, named after the resource "
            + "they came from. The " + Plugins.All + " line below still grants all of them at once, "
            + "so you only need those files to hand out a plugin's permissions one by one."));

        file.Append('\n');
        file.Append(ExampleFile.Comment(
            "Note: While some of the permissions below are indented, that's only to show you to " +
            "which parent they belong. You do not need to have them indented inside the " +
            "permissions.cfg like this to function correctly while executing the file. " +
            "It doesn't make a difference when executing the permissions.cfg from your server.cfg."));

        AppendEntries(file, ordered, SpacedDepth, annotate: true);

        return file.ToString();
    }

    public static string RenderForPlugin(string resource, string displayName, IEnumerable<PermissionExampleEntry> entries)
    {
        var ordered = entries.ToList();

        var file = new StringBuilder();

        file.Append(ExampleFile.BannerIn(
            ExampleFile.PluginsDirectory,
            ExampleFile.PluginCopyName(resource, CopyName),
            "These permissions belong to the plugin '" + displayName + "', which the resource '"
            + resource + "' provides. They are listed here and not in vMenu's own permissions.cfg, "
            + "so removing the plugin means removing the files named after it rather than hunting "
            + "through that one.",
            "Nothing here is written while the plugin is not running, so start it before you read "
            + "this. If you have removed the plugin for good, delete every file in this folder whose "
            + "name starts with '" + resource + ".'.",
            "The same rules as vMenu's own permissions apply: " + EveryoneGroup + " is suggested for "
            + "what any player may have and " + StaffGroup + " for what your staff should keep, a "
            + "line ending in .All hands out everything indented under it, and the indentation is "
            + "only there to show you what belongs to what."));

        if (ordered.Count == 0)
        {
            file.Append('\n');
            file.Append(ExampleFile.Comment("This plugin declares no permissions, so there is nothing to hand out here."));

            return file.ToString();
        }

        // Only the plugin's own container is spaced; everything else sits under it.
        AppendEntries(file, ordered, spacedDepth: 0, annotate: false);

        return file.ToString();
    }

    private static void AppendEntries(
        StringBuilder file,
        List<PermissionExampleEntry> ordered,
        int spacedDepth,
        bool annotate)
    {
        var staffOnly = ResolveStaffOnly(ordered);

        for (var index = 0; index < ordered.Count; index++)
        {
            var entry = ordered[index];

            if (entry.Depth <= spacedDepth)
            {
                file.Append('\n');
            }

            // Above the command, not trailing it: the console does not treat a mid-line # as a comment.
            if (annotate && Annotation(entry) is { Length: > 0 } note)
            {
                file.Append(' ', entry.Depth * 2).Append("# ").Append(note).Append('\n');
            }

            file.Append(' ', entry.Depth * 2);
            file.Append("add_ace " + Principal(staffOnly[index]) + " \"" + entry.Name + "\" allow");
            file.Append('\n');
        }
    }

    // A container grants everything under it, so any staff-only permission below it must make it staff-only
    // too, else the container would hand out that restricted permission to everybody.
    private static bool[] ResolveStaffOnly(List<PermissionExampleEntry> ordered)
    {
        var staffOnly = new bool[ordered.Count];

        // Pre-order flatten walked backwards, so each entry's children are already resolved in one pass.
        for (var index = ordered.Count - 1; index >= 0; index--)
        {
            staffOnly[index] = ordered[index].IsStaffOnly;

            for (var below = index + 1; below < ordered.Count && ordered[below].Depth > ordered[index].Depth; below++)
            {
                if (ordered[below].Depth == ordered[index].Depth + 1 && staffOnly[below])
                {
                    staffOnly[index] = true;
                    break;
                }
            }
        }

        return staffOnly;
    }

    private static string Principal(bool isStaffOnly) =>
        isStaffOnly ? StaffGroup : EveryoneGroup;

    private static string Annotation(PermissionExampleEntry entry)
    {
        List<string> notes = [];

        if (entry.Source is not null)
        {
            notes.Add("from " + entry.Source);
        }

        if (entry.ExtraParents.Count > 0)
        {
            notes.Add($"also granted by {string.Join(", ", entry.ExtraParents)}");
        }

        return notes.Count > 0 ? string.Join(", ", notes) : string.Empty;
    }
}
