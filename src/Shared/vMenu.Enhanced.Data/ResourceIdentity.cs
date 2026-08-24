using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Permissions;

namespace vMenu.Enhanced.Data;

// The folder name vMenu Enhanced has to be installed under. The filesystem permission, the
// exec @vMenu.Enhanced/... lines and every generated example file name the resource literally, so a
// renamed copy fails later on in ways that are hard to trace back to the name.
public static class ResourceIdentity
{
    public const string RequiredName = "vMenu.Enhanced";

    private const string Rule = "###############################################################################";

    public static bool IsCorrectlyNamed(string? resourceName) =>
        string.Equals(resourceName, RequiredName, StringComparison.Ordinal);

    // The lines to log when the resource is installed under the wrong name.
    public static string[] MismatchReport(string? resourceName, string side)
    {
        var actual = string.IsNullOrWhiteSpace(resourceName) ? "<unknown>" : resourceName;

        return
        [
            Rule,
            $"  vMenu Enhanced did not start ({side} side).",
            "",
            $"  It is installed as '{actual}', but it has to be named '{RequiredName}'.",
            "",
            "  Rename the folder in your resources directory to:",
            $"      {RequiredName}",
            "",
            "  Then make your server.cfg match it:",
            $"      exec @{RequiredName}/{ExampleFile.ConfigDirectory}/{PermissionsExample.CopyName}",
            $"      exec @{RequiredName}/{ExampleFile.ConfigDirectory}/{ConfigurationExample.CopyName}",
            $"      add_filesystem_permission {RequiredName} write {RequiredName}",
            $"      ensure {RequiredName}",
            Rule,
        ];
    }
}
