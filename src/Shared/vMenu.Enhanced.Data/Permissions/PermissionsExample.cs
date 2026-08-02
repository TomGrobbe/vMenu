using System.Text;

namespace vMenu.Enhanced.Data.Permissions;

/// <summary>One line of the generated permissions example, flattened out of the registry tree.</summary>
public sealed class PermissionExampleEntry(
    string name,
    int depth,
    bool isDynamic,
    bool isStaffOnly,
    IReadOnlyList<string> extraParents)
{
    public string Name { get; } = name;

    public int Depth { get; } = depth;

    public bool IsDynamic { get; } = isDynamic;

    public bool IsStaffOnly { get; } = isStaffOnly;

    public IReadOnlyList<string> ExtraParents { get; } = extraParents;
}

public static class PermissionsExample
{
    public const string CopyName = "permissions.cfg";

    private const string EveryoneGroup = "builtin.everyone";

    private const string StaffGroup = "group.admin";

    public static string ResourcePath => $"{ExampleFile.ConfigDirectory}/{CopyName}{ExampleFile.Extension}";

    public static string Render(IEnumerable<PermissionExampleEntry> entries)
    {
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
            "Give a player a group by one of their identifiers. "
            + "Use whichever you can look up most easily. For example:"));
        file.Append("add_principal identifier.steam:110000105959047 group.admin\n");
        file.Append("add_principal identifier.license:4510587c13e0b645eb8d24bc104601792277ab98 group.admin\n");

        file.Append('\n');
        file.Append(ExampleFile.Comment(
            "Groups inherit from each other (if you set it up correctly). This one gives everybody in group.admin everything "
            + "group.mod may do, on top of whatever group.admin is granted below."));
        file.Append("add_principal group.admin group.mod\n");

        file.Append('\n');
        file.Append(ExampleFile.Comment(
            "Every permission vMenu Enhanced knows about is listed below, commented out. Uncomment "
            + "the ones you want to grant, and change the principal if somebody else should have "
            + "them. Each line already suggests one: " + EveryoneGroup + " for the permissions that "
            + "are fine for any player, and " + StaffGroup + " for the ones that should stay with "
            + "your staff. "
            + "A permission ending in .All grants everything nested underneath it, so granting the "
            + ".All is usually all you need, unless you want to restrict some features (then don't " +
            "use .All, and only give the specific permissions you want)." +
            "Checkout https://docs.vespura.com/vMenu/Enhanced/ for more permissions information."));

        file.Append('\n');
        file.Append(ExampleFile.Comment(
            "Note: While some of the permissions below are indented, that's only to show you to " +
            "which parent they belong. You do not need to have them indented inside the " +
            "permissions.cfg like this to function correctly while executing the file. " +
            "It doesn't make a difference when executing the permissions.cfg from your server.cfg."));

        foreach (var entry in entries)
        {
            if (entry.Depth == 0)
            {
                file.Append('\n');
            }

            file.Append("# ").Append(' ', entry.Depth * 2);
            file.Append("add_ace " + Principal(entry) + " \"" + entry.Name + "\" allow");
            file.Append(Annotation(entry));
            file.Append('\n');
        }

        return file.ToString();
    }

    private static string Principal(PermissionExampleEntry entry) =>
        entry.IsStaffOnly ? StaffGroup : EveryoneGroup;

    private static string Annotation(PermissionExampleEntry entry)
    {
        List<string> notes = [];

        if (entry.IsDynamic)
        {
            notes.Add("from config/model-whitelists.json");
        }

        if (entry.ExtraParents.Count > 0)
        {
            notes.Add($"also granted by {string.Join(", ", entry.ExtraParents)}");
        }

        return notes.Count > 0 ? $"  # {string.Join(", ", notes)}" : string.Empty;
    }
}
