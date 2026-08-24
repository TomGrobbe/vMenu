namespace vMenu.Enhanced.Data.Permissions;

// Rules of the dotted permission naming scheme, shared by both sides so inheritance is derived
// identically. A permission whose last segment is All grants everything inside its container, which
// is what lets the client resolve inheritance from a name alone.
public static class PermissionPath
{
    public const string Root = "vMenu.Enhanced";

    public const string All = "All";

    public const char Separator = '.';

    public const string AllSuffix = ".All";

    public static bool IsContainerGrant(string permission) =>
        permission.EndsWith(AllSuffix, StringComparison.OrdinalIgnoreCase);

    // Null when the permission sits directly under Root.
    public static string? GetContainer(string permission)
    {
        var index = permission.LastIndexOf(Separator);

        return index > Root.Length ? permission[..index] : null;
    }

    public static IEnumerable<string> EnumerateContainers(string permission)
    {
        var container = GetContainer(permission);

        while (container is not null)
        {
            yield return container;

            container = GetContainer(container);
        }
    }

    // The .All permissions that grant this one, nearest first.
    public static IEnumerable<string> EnumerateContainerGrants(string permission)
    {
        foreach (var container in EnumerateContainers(permission))
        {
            var grant = container + AllSuffix;

            // Skipped so a container grant is never listed as its own parent.
            if (!grant.Equals(permission, StringComparison.OrdinalIgnoreCase))
            {
                yield return grant;
            }
        }
    }

    // Whether a segment is usable in an ACE name. Anything else could never be granted.
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

    public static bool IsValidPermission(string permission)
    {
        if (string.IsNullOrEmpty(permission))
        {
            return false;
        }

        if (!permission.StartsWith(Root + Separator, StringComparison.Ordinal))
        {
            return false;
        }

        var segments = permission[(Root.Length + 1)..].Split(Separator);

        return segments.Length > 0 && segments.All(IsValidSegment);
    }
}
