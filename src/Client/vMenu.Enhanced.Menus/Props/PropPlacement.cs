using System.Numerics;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Props;

public static class PropPlacement
{
    internal const Control ConfirmControl = Control.FrontendAccept;

    internal const Control CancelControl = Control.FrontendCancel;

    internal const Control RotateLeftControl = Control.VehicleFlyRollLeftOnly;

    internal const Control RotateRightControl = Control.VehicleFlyRollRightOnly;

    internal const Control NearerControl = Control.CursorScrollDown;

    internal const Control FurtherControl = Control.CursorScrollUp;

    internal const Control SnapControl = Control.Context;

    private const int GhostAlpha = 200;

    private const float RotateSpeed = 90f;

    private const int ButtonGraceMs = 300;

    private static readonly Control[] Suppressed =
    [
        Control.Attack,
        Control.Attack2,
        Control.Aim,
        Control.MeleeAttack1,
        Control.MeleeAttack2,
        Control.VehicleAim,
        Control.Detonate,
        Control.SelectWeapon,
        Control.Phone,
    ];

    private static TickHandle? _frame;

    private static TickHandle? _buttons;

    private static bool _active;

    private static int _entity;

    private static uint _model;

    private static float _heading;

    private static float _distance;

    private static Action<int>? _onPlaced;

    private static Menu? _cameFrom;

    public static bool IsActive => _active;

    public static void Initialize()
    {
        _frame = TickRegistry.Register(
            "PropSpawner.Placement",
            Frame,
            TickRate.PerFrame,
            () => _active,
            onStarted: EnterGhost,
            onStopped: LeaveGhost,
            autoStart: false);

        _buttons = TickRegistry.Register(
            "PropSpawner.PlacementButtons",
            PropPlacementButtons.DrawAsync,
            TickRate.PerFrame,
            () => _active,
            onStopped: PropPlacementButtons.Release,
            autoStart: false);
    }

    public static async Task BeginAsync(uint hash, Action<int>? onPlaced = null)
    {
        if (_active)
        {
            Notifications.Warning(MenuText.Key(Loc.PropSpawner.AlreadyPlacing));

            return;
        }

        if (!PropSpawning.IsSpawnable(hash))
        {
            Notifications.Error(MenuText.Key(Loc.PropSpawner.InvalidModel));

            return;
        }

        if (!SpawnedProps.TryTakeOrWarn())
        {
            return;
        }

        _distance = PropSpawnOptions.Distance;

        var prop = await PropSpawning.SpawnAsync(
            hash,
            CameraRay.Hit(_distance, Native.PlayerPedId()),
            PropSpawnOptions.Networked,
            frozen: true);

        if (prop is null)
        {
            Notifications.Error(MenuText.Key(Loc.PropSpawner.SpawnFailed));

            return;
        }

        _entity = prop.Handle;
        _model = hash;
        _heading = 0f;
        _onPlaced = onPlaced;
        _active = true;

        _cameFrom = MenuController.GetCurrentMenu();

        MenuController.CloseAllMenus();

        _frame?.Reevaluate();
        _buttons?.Reevaluate();
    }

    // Written once here and undone once on the way out. The frame body only moves the prop.
    private static void EnterGhost()
    {
        Native.FreezeEntityPosition(_entity, true);
        Native.SetEntityInvincible(_entity, true, false);
        Native.SetEntityCollision(_entity, false, false);
        Native.SetEntityAlpha(_entity, GhostAlpha, false);

        PropPlacementButtons.Invalidate();
    }

    private static void LeaveGhost()
    {
        if (_entity == 0 || !Native.DoesEntityExist(_entity))
        {
            return;
        }

        Native.ResetEntityAlpha(_entity);
        Native.SetEntityCollision(_entity, true, true);
        Native.SetEntityInvincible(_entity, false, false);
    }

    private static void Frame()
    {
        if (_entity == 0 || !Native.DoesEntityExist(_entity))
        {
            Finish();

            return;
        }

        foreach (var control in Suppressed)
        {
            Native.DisableControlAction(0, (int)control, true);
        }

        if (ReadInput())
        {
            return;
        }

        var target = CameraRay.Hit(_distance, _entity);

        Native.SetEntityHeading(_entity, Wrap(Native.GetGameplayCamRot(0).Z + _heading));
        Native.SetEntityCoordsNoOffset(_entity, target.X, target.Y, target.Z, false, false, false);
    }

    private static bool ReadInput()
    {
        if (Native.IsDisabledControlJustPressed(0, (int)ConfirmControl))
        {
            Confirm();

            return true;
        }

        if (Native.IsDisabledControlJustPressed(0, (int)CancelControl))
        {
            Cancel();

            return true;
        }

        var turn = RotateSpeed * Native.GetFrameTime();

        if (Native.IsDisabledControlPressed(0, (int)RotateLeftControl))
        {
            _heading += turn;
        }
        else if (Native.IsDisabledControlPressed(0, (int)RotateRightControl))
        {
            _heading -= turn;
        }

        if (Native.IsDisabledControlJustPressed(0, (int)FurtherControl))
        {
            Reach(1);
        }
        else if (Native.IsDisabledControlJustPressed(0, (int)NearerControl))
        {
            Reach(-1);
        }

        if (Native.IsDisabledControlJustPressed(0, (int)SnapControl))
        {
            PropSpawnOptions.SetSnapToGround(!PropSpawnOptions.SnapToGround);

            PropPlacementButtons.Invalidate();
        }

        return false;
    }

    private static void Reach(int by)
    {
        _distance = Math.Clamp(_distance + by, PropSpawnOptions.MinDistance, PropSpawnOptions.MaxDistance);

        PropSpawnOptions.Distance = (int)_distance;
    }

    private static void Confirm()
    {
        var entity = _entity;

        PropSpawning.Settle(entity, PropSpawnOptions.Frozen, PropSpawnOptions.SnapToGround);

        PropRecents.Add(PropModelNames.Of(_model));

        Notifications.Success(MenuText.Key(Loc.PropSpawner.Placed));

        var placed = _onPlaced;

        Finish();

        Reopen();

        placed?.Invoke(entity);
    }

    private static void Cancel()
    {
        SpawnedProps.Delete(_entity);

        Notifications.Info(MenuText.Key(Loc.PropSpawner.Cancelled));

        Finish();

        Reopen();
    }

    private static void Finish()
    {
        _active = false;
        _entity = 0;
        _model = 0;
        _onPlaced = null;

        _frame?.Reevaluate();
        _buttons?.Reevaluate();
    }

    private static void Reopen()
    {
        var menu = _cameFrom;

        _cameFrom = null;

        if (menu is null)
        {
            return;
        }

        menu.OpenMenu();

        _ = HoldMenuButtonsAsync();
    }

    // MenuAPI selects on release, so the key that confirmed a placement would hit a row.
    private static async Task HoldMenuButtonsAsync()
    {
        MenuController.DisableMenuButtons = true;

        await API.Delay(ButtonGraceMs);

        MenuController.DisableMenuButtons = false;
    }

    private static float Wrap(float degrees)
    {
        var wrapped = degrees % 360f;

        return wrapped < 0f ? wrapped + 360f : wrapped;
    }
}
