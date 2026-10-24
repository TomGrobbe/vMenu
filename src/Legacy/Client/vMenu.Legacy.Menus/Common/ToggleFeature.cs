using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Common;

/// <summary>The game events after which a toggle writes its answer again, because the game reset it.</summary>
[Flags]
public enum ToggleWatch
{
    None = 0,
    PedChanged = 1,
    PedRevived = 2,
    VehicleChanged = 4,
    VehicleDirtied = 8,
}

/// <summary>The saved preference, permission gate and event watching every simple on/off toggle shares, so a
/// feature only supplies what to do with the answer.</summary>
public sealed class ToggleFeature
{
    private readonly BoolDefault? _saved;

    private readonly string _permission;

    private readonly Action<bool> _apply;

    private readonly ToggleWatch _watch;

    private bool _wanted;

    private bool _watching;

    public ToggleFeature(BoolDefault saved, string permission, Action<bool> apply, ToggleWatch watch = ToggleWatch.None)
        : this(permission, apply, watch)
    {
        _saved = saved;
    }

    /// <summary>Kept in memory only, for a toggle that must not survive a rejoin.</summary>
    public ToggleFeature(string permission, Action<bool> apply, ToggleWatch watch = ToggleWatch.None)
    {
        _permission = permission;
        _apply = apply;
        _watch = watch;
    }

    public bool Enabled => (_saved?.Value ?? _wanted) && IsAllowed;

    public bool IsAllowed => ClientPermissions.IsAllowed(_permission);

    /// <summary>Call once at startup, before permissions have arrived.</summary>
    public void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    /// <summary>Ignores switching on without the permission: the checkbox follows the permission, but a revoke
    /// can land between the two.</summary>
    public void SetEnabled(bool enabled)
    {
        if (enabled && !IsAllowed)
        {
            return;
        }

        if (_saved is null)
        {
            _wanted = enabled;
        }
        else
        {
            _saved.Value = enabled;
        }

        Apply();
    }

    public void Apply()
    {
        var on = Enabled;

        Watch(on);

        _apply(on);
    }

    /// <summary>Only subscribed while on, because the first subscriber is what starts the polling behind each
    /// event.</summary>
    private void Watch(bool watching)
    {
        if (watching == _watching)
        {
            return;
        }

        _watching = watching;

        if (Watches(ToggleWatch.PedChanged))
        {
            if (watching)
            {
                LocalPlayerTicks.PlayerPedIdChanged += OnPedChanged;
            }
            else
            {
                LocalPlayerTicks.PlayerPedIdChanged -= OnPedChanged;
            }
        }

        if (Watches(ToggleWatch.PedRevived))
        {
            if (watching)
            {
                LocalPlayerTicks.PlayerPedRevived += OnPedRevived;
            }
            else
            {
                LocalPlayerTicks.PlayerPedRevived -= OnPedRevived;
            }
        }

        if (Watches(ToggleWatch.VehicleChanged))
        {
            if (watching)
            {
                LocalVehicleTicks.VehicleChanged += OnVehicleChanged;
            }
            else
            {
                LocalVehicleTicks.VehicleChanged -= OnVehicleChanged;
            }
        }

        if (Watches(ToggleWatch.VehicleDirtied))
        {
            if (watching)
            {
                LocalVehicleTicks.VehicleDirtied += OnVehicleDirtied;
            }
            else
            {
                LocalVehicleTicks.VehicleDirtied -= OnVehicleDirtied;
            }
        }
    }

    private bool Watches(ToggleWatch watch) => (_watch & watch) != 0;

    private void OnPedChanged(PlayerPedIdChanged _) => Apply();

    private void OnPedRevived(PlayerPedRevived _) => Apply();

    private void OnVehicleChanged(VehicleChanged _) => Apply();

    private void OnVehicleDirtied(VehicleDirtied _) => Apply();
}
