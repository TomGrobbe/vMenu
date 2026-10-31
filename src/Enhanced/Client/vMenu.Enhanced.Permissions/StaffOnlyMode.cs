using vMenu.Enhanced.Data.Permissions;

namespace vMenu.Enhanced.Permissions;

// Handed the setting rather than reading it, because this module cannot reference the configuration one.
public static class StaffOnlyMode
{
    private static Func<bool> _enabled = static () => false;

    private static bool _last;

    public static bool Active => _last;

    public static event Action? Changed;

    public static void Source(Func<bool> enabled)
    {
        _enabled = enabled;

        ClientPermissions.PermissionsChanged += Refresh;

        Refresh();
    }

    public static void Refresh()
    {
        var active = _enabled() && !ClientPermissions.IsAllowed(Global.Staff);

        if (active == _last)
        {
            return;
        }

        _last = active;

        Changed?.Invoke();
    }
}
