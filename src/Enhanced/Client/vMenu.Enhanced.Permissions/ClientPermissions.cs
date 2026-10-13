using vMenu.Enhanced.Data.Permissions;

namespace vMenu.Enhanced.Permissions;

// The client's cached view of what the local player may do. Advisory only: the server sends the
// smallest set describing the player, so a permission ending in .All is stored as a subtree and
// questions inside it are answered by walking up the asked for name. The server re-checks anything
// that matters.
public static class ClientPermissions
{
    private static readonly HashSet<string> GrantedExact = new(StringComparer.OrdinalIgnoreCase);

    // Container paths granted in full, stored without their trailing .All.
    private static readonly HashSet<string> GrantedSubtrees = new(StringComparer.OrdinalIgnoreCase);

    // So the prefix walk runs once per distinct permission rather than every frame a menu asks.
    private static readonly Dictionary<string, bool> ResolvedCache = new(StringComparer.OrdinalIgnoreCase);

    private static bool _grantsEverything;

    // Until a set arrives every check fails, so menus start locked rather than briefly open.
    public static bool HasReceivedPermissions { get; private set; }

    public static bool HasAnyPermission =>
        HasReceivedPermissions && (_grantsEverything || GrantedExact.Count > 0 || GrantedSubtrees.Count > 0);

    // Menus should build once and re-evaluate here.
    public static event Action? PermissionsChanged;

    public static void ApplyPermissions(string[] permissions)
    {
        GrantedExact.Clear();
        GrantedSubtrees.Clear();
        ResolvedCache.Clear();
        _grantsEverything = false;

        foreach (var permission in permissions)
        {
            if (permission.Equals(Global.Everything, StringComparison.OrdinalIgnoreCase))
            {
                _grantsEverything = true;
                continue;
            }

            if (PermissionPath.IsContainerGrant(permission))
            {
                GrantedSubtrees.Add(permission[..^PermissionPath.AllSuffix.Length]);
                continue;
            }

            GrantedExact.Add(permission);
        }

        HasReceivedPermissions = true;

        PermissionsChanged?.Invoke();
    }

    // Inheritance is applied here, so callers never name a parent themselves.
    public static bool IsAllowed(string permission)
    {
        if (_grantsEverything)
        {
            return true;
        }

        if (!HasReceivedPermissions)
        {
            return false;
        }

        if (ResolvedCache.TryGetValue(permission, out var cached))
        {
            return cached;
        }

        var allowed = Evaluate(permission);
        ResolvedCache[permission] = allowed;

        return allowed;
    }

    // Called after a kvp import, to refresh permission checks once settings are restored.
    public static void Reevaluate() => PermissionsChanged?.Invoke();

    // Puts the client back into its pre-sync state.
    public static void Clear()
    {
        GrantedExact.Clear();
        GrantedSubtrees.Clear();
        ResolvedCache.Clear();
        _grantsEverything = false;
        HasReceivedPermissions = false;

        PermissionsChanged?.Invoke();
    }

    private static bool Evaluate(string permission)
    {
        if (GrantedExact.Contains(permission))
        {
            return true;
        }

        foreach (var container in PermissionPath.EnumerateContainers(permission))
        {
            if (GrantedSubtrees.Contains(container))
            {
                return true;
            }
        }

        return false;
    }
}
