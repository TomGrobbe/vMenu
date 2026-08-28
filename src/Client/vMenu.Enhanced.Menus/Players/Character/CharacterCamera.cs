using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Players.Character;

public enum CameraFocus
{
    FullBody,
    Head,
    UpperBody,
    LowerBody,
    Shoes,
    LowerArms,
    FullArms,
}

public static class CharacterCamera
{
    private const int ControlGroup = 0;

    private const string ScriptedCamera = "DEFAULT_SCRIPTED_CAMERA";

    private const string WatchDictionary = "anim@random@shop_clothes@watches";

    private const float FieldOfView = 45f;

    private const int TurnMs = 1600;

    private const int LookMs = 10000;

    private const int LookRenewMs = 4000;

    private const int LookThrottleMs = 150;

    private const float LookStep = 0.2f;

    private const float LookArc = 115f;

    private const float MouseSpeed = 8f;

    private const float StickSpeed = 140f;

    private const float TurnSpeed = 100f;

    private const float ZoomSpeed = 1.7f;

    private const float HeightSpeed = 0.85f;

    private const float Deadzone = 0.15f;

    private const float MinPitch = -55f;

    private const float MaxPitch = 78f;

    private const float MinZoom = 0.4f;

    private const float MaxZoom = 3f;

    private const float MinHeight = -0.9f;

    private const float MaxHeight = 1.3f;

    private const float MinDistance = 0.3f;

    private const float MaxDistance = 8f;

    private const float FramingSpeed = 7f;

    private const float Degrees = MathF.PI / 180f;

    private const string IconSeparator = "%b_998%";

    private static readonly (float PivotZ, float Distance, float Pitch)[] Framings =
    [
        (0f, 2.8f, 6f),
        (0.6f, 0.9f, 3f),
        (0.3f, 1.4f, 8f),
        (-0.45f, 1.6f, 5f),
        (-0.9f, 1f, 12f),
        (0f, 0.98f, 6f),
        (0.15f, 1.3f, 9f),
    ];

    private static readonly int[] Suppressed =
    [
        1, 2, 22, 24, 25, 26, 30, 31, 32, 33, 34, 35, 36, 37, 44, 45, 47, 55, 56, 57, 58,
        71, 72, 75, 76, 81, 82, 83, 84, 85, 99, 100, 114, 115, 140, 141, 142, 143, 152, 153,
        257, 263, 264,
    ];

    private static TickHandle? _tick;

    private static int _camera;

    private static CameraFocus _page = CameraFocus.FullBody;

    private static CameraFocus _framing = CameraFocus.FullBody;

    private static float _yaw;

    private static float _pitch;

    private static float _zoom = 1f;

    private static float _height;

    private static float _pivotZ;

    private static float _distance;

    private static float _basePitch;

    private static bool _seeded;

    private static bool _watchesLoaded;

    private static bool _watchPlaying;

    private static Vector3 _lookAt;

    private static int _lookedAt;

    private static Menu? _buttonsMenu;

    private static bool _buttonsKeyboard;

    private static bool _buttonsStale = true;

    public static void Initialize()
    {
        Localizer.Changed += () => _buttonsStale = true;

        CharacterCameraKeyBinding.Register(ToggleAutoCamera);

        UserDefaults.CharacterCreatorDisableAutoCamera.Changed += () =>
        {
            _buttonsStale = true;

            MenuRegistry.RefreshAll();
        };

        _tick = TickRegistry.Register(
            "CharacterCreator.Camera",
            Frame,
            TickRate.PerFrame,
            condition: () => MpCharacterState.IsEditing,
            onStarted: Begin,
            onStopped: End);
    }

    public static void Reevaluate() => _tick?.Reevaluate();

    public static CameraFocus FocusFor(Menu? menu, CameraFocus page)
    {
        if (menu?.GetCurrentMenuItem() is not { } item)
        {
            return page;
        }

        return item.ItemData switch
        {
            CameraFocus focus => focus,
            ICameraFraming framing => framing.Framing,
            PedCustomizationRows.SlotReference slot => FocusForSlot(slot),
            _ => page,
        };
    }

    public static CameraFocus Page
    {
        get => _page;
        set
        {
            if (_page == value)
            {
                return;
            }

            _page = value;

            if (!AutoCameraDisabled)
            {
                Recentre();
            }
        }
    }

    public static bool AutoCameraDisabled
    {
        get => UserDefaults.CharacterCreatorDisableAutoCamera.Value;
        set => UserDefaults.CharacterCreatorDisableAutoCamera.Value = value;
    }

    private static void ToggleAutoCamera() => SharedAPI.RunOnMainThread(() =>
    {
        if (!MpCharacterState.IsEditing)
        {
            return;
        }

        var disabled = !AutoCameraDisabled;

        AutoCameraDisabled = disabled;

        Notifications.Info(MenuText.Key(disabled
            ? Loc.CharacterCreator.AutoCameraOff
            : Loc.CharacterCreator.AutoCameraOn));
    });

    private static CameraFocus FocusForSlot(PedCustomizationRows.SlotReference slot)
    {
        if (slot.IsComponent)
        {
            return slot.Slot switch
            {
                PedComponentSlots.Head or PedComponentSlots.Hair or PedComponentSlots.Mask => CameraFocus.Head,
                PedComponentSlots.Legs => CameraFocus.LowerBody,
                PedComponentSlots.Shoes => CameraFocus.Shoes,
                PedComponentSlots.Decals => CameraFocus.FullBody,
                _ => CameraFocus.UpperBody,
            };
        }

        return slot.Slot switch
        {
            PedPropSlots.Hats or PedPropSlots.Glasses or PedPropSlots.Ears => CameraFocus.Head,
            PedPropSlots.Watches => CameraFocus.FullArms,
            _ => CameraFocus.LowerArms,
        };
    }

    private static void Begin() => SharedAPI.RunOnMainThread(() =>
    {
        _seeded = false;
        _watchPlaying = false;
        _lookAt = default;
        _lookedAt = 0;

        var ped = Native.PlayerPedId();

        Native.ClearPedTasksImmediately(ped);

        Native.SetPedDesiredHeading(ped, Native.GetEntityHeading(ped));

        Native.RequestAnimDict(WatchDictionary);

        Native.DisplayHud(false);
        Native.DisplayRadar(false);

        Recentre(ped);
    });

    private static void End() => SharedAPI.RunOnMainThread(() =>
    {
        var ped = Native.PlayerPedId();

        Native.ClearPedTasks(ped);
        Native.SetEntityCollision(ped, true, true);
        Native.FreezeEntityPosition(ped, false);

        Native.SetPedCanPlayGestureAnims(ped, true);
        Native.SetPedCanPlayAmbientIdles(ped, false, false);

        _watchPlaying = false;

        Native.DisplayHud(true);
        Native.DisplayRadar(true);

        if (_watchesLoaded)
        {
            Native.RemoveAnimDict(WatchDictionary);
            _watchesLoaded = false;
        }

        Destroy();

        MenuController.DisableBackButton = false;
    });

    private static async Task Frame()
    {
        var ped = Native.PlayerPedId();

        Suppress();
        StandStill(ped);

        CharacterEdit.KeepExpression();

        Native.SetEntityCollision(ped, false, false);
        Native.FreezeEntityPosition(ped, true);

        _watchesLoaded = Native.HasAnimDictLoaded(WatchDictionary);

        var current = MenuController.GetCurrentMenu();
        var focus = FocusFor(current, _page);

        SyncButtons(current);
        PlayWatchIdle(ped, focus);

        if (Native.IsDisabledControlJustReleased(ControlGroup, (int)Control.Jump))
        {
            await TurnAroundAsync(ped);

            return;
        }

        ReadInput(ped);

        LookAt(ped, Place(ped, focus));
    }

    private static void ReadInput(int ped)
    {
        if (MenuController.DisableMenuButtons || Native.IsPauseMenuActive())
        {
            return;
        }

        if (Native.IsDisabledControlJustPressed(ControlGroup, (int)Control.LookBehind))
        {
            Recentre(ped);

            return;
        }

        var delta = Native.GetFrameTime();
        var keyboard = Native.IsUsingKeyboardAndMouse(2);

        var lookX = Native.GetDisabledControlNormal(ControlGroup, (int)Control.LookLeftRight);
        var lookY = Native.GetDisabledControlNormal(ControlGroup, (int)Control.LookUpDown);

        if (keyboard)
        {
            _yaw -= lookX * MouseSpeed;
            _pitch += lookY * MouseSpeed;
        }
        else
        {
            _yaw -= Curve(lookX) * StickSpeed * delta;
            _pitch += Curve(lookY) * StickSpeed * delta;
        }

        var swing = Curve(Native.GetDisabledControlNormal(ControlGroup, (int)Control.MoveLeftRight));
        var stick = Curve(Native.GetDisabledControlNormal(ControlGroup, (int)Control.MoveUpDown));

        _yaw += swing * TurnSpeed * delta;

        if (keyboard)
        {
            _zoom *= 1f + (stick * ZoomSpeed * delta);

            var down = Native.IsDisabledControlPressed(ControlGroup, (int)Control.ParachuteBrakeLeft);
            var up = Native.IsDisabledControlPressed(ControlGroup, (int)Control.ParachuteBrakeRight);

            if (down != up)
            {
                _height += (up ? HeightSpeed : -HeightSpeed) * delta;
            }
        }
        else
        {
            _height -= stick * HeightSpeed * delta;

            var closer = Curve(Native.GetDisabledControlNormal(ControlGroup, (int)Control.Attack));
            var further = Curve(Native.GetDisabledControlNormal(ControlGroup, (int)Control.Aim));

            _zoom *= 1f + ((further - closer) * ZoomSpeed * delta);
        }

        _yaw = Wrap(_yaw);
        _zoom = Math.Clamp(_zoom, MinZoom, MaxZoom);
        _height = Math.Clamp(_height, MinHeight, MaxHeight);
    }

    private static Vector3 Place(int ped, CameraFocus focus)
    {
        if (!AutoCameraDisabled)
        {
            _framing = focus;
        }

        var framing = Framings[(int)_framing];

        if (!_seeded)
        {
            _pivotZ = framing.PivotZ;
            _distance = framing.Distance;
            _basePitch = framing.Pitch;
            _seeded = true;
        }
        else
        {
            var step = 1f - MathF.Exp(-FramingSpeed * Native.GetFrameTime());

            _pivotZ += (framing.PivotZ - _pivotZ) * step;
            _distance += (framing.Distance - _distance) * step;
            _basePitch += (framing.Pitch - _basePitch) * step;
        }

        _pitch = Math.Clamp(_pitch, MinPitch - _basePitch, MaxPitch - _basePitch);

        var pitch = (_basePitch + _pitch) * Degrees;
        var yaw = _yaw * Degrees;
        var distance = Math.Clamp(_distance * _zoom, MinDistance, MaxDistance);
        var reach = MathF.Cos(pitch) * distance;

        var pivot = Native.GetOffsetFromEntityInWorldCoords(ped, 0f, 0f, _pivotZ + _height);

        var position = new Vector3(
            pivot.X - (MathF.Sin(yaw) * reach),
            pivot.Y + (MathF.Cos(yaw) * reach),
            pivot.Z + (MathF.Sin(pitch) * distance));

        if (!Native.DoesCamExist(_camera))
        {
            _camera = Native.CreateCam(ScriptedCamera, false);

            Native.SetCamFov(_camera, FieldOfView);
            Native.SetCamActive(_camera, true);
            Native.RenderScriptCams(true, false, 0, false, false, 0);
        }

        Native.SetCamCoord(_camera, position.X, position.Y, position.Z);
        Native.PointCamAtCoord(_camera, pivot.X, pivot.Y, pivot.Z);

        return position;
    }

    private static void Destroy()
    {
        Native.RenderScriptCams(false, false, 0, true, true, 0);

        if (Native.DoesCamExist(_camera))
        {
            Native.SetCamActive(_camera, false);
            Native.DestroyCam(_camera, false);
        }

        _camera = 0;
        _seeded = false;
    }

    private static void SyncButtons(Menu? menu)
    {
        var keyboard = Native.IsUsingKeyboardAndMouse(2);

        if (!_buttonsStale && keyboard == _buttonsKeyboard && ReferenceEquals(menu, _buttonsMenu))
        {
            return;
        }

        _buttonsStale = false;
        _buttonsKeyboard = keyboard;
        _buttonsMenu = menu;

        if (menu is null)
        {
            return;
        }

        var localizer = Localizer.Current;
        var axis = keyboard ? Loc.CharacterCreator.ZoomCamera : Loc.CharacterCreator.CameraHeight;
        var pair = keyboard ? Loc.CharacterCreator.CameraHeight : Loc.CharacterCreator.ZoomCamera;

        var low = keyboard ? Control.ParachuteBrakeLeft : Control.Aim;
        var high = keyboard ? Control.ParachuteBrakeRight : Control.Attack;

        var auto = keyboard
            ? CharacterCameraKeyBinding.KeyboardControl
            : CharacterCameraKeyBinding.ControllerControl;

        menu.CustomInstructionalButtons.Clear();

        menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon((int)Control.LookLeftRight),
            localizer.Get(Loc.CharacterCreator.MoveCamera)));

        menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon((int)Control.MoveUpDown),
            localizer.Get(axis)));

        menu.CustomInstructionalButtons.Add(
            new Menu.InstructionalButton($"{Icon((int)low)}{IconSeparator}{Icon((int)high)}", localizer.Get(pair)));

        menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon((int)Control.LookBehind),
            localizer.Get(Loc.CharacterCreator.ResetCamera)));

        menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon((int)Control.Jump),
            localizer.Get(Loc.CharacterCreator.TurnCharacter)));

        menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon(auto),
            localizer.Get(AutoCameraDisabled
                ? Loc.CharacterCreator.AutoCameraButtonOff
                : Loc.CharacterCreator.AutoCameraButtonOn)));
    }

    private static string Icon(int control) =>
        Native.GetControlInstructionalButton(ControlGroup, control, true);

    private static void Recentre() => Recentre(Native.PlayerPedId());

    private static void Recentre(int ped)
    {
        _yaw = Native.GetEntityHeading(ped);
        _pitch = 0f;
        _zoom = 1f;
        _height = 0f;

        _framing = CameraFocus.FullBody;
    }

    private static void StandStill(int ped)
    {
        Native.SetPedCanPlayGestureAnims(ped, false);

        Native.SetPedCanPlayAmbientIdles(ped, true, true);
    }

    private static void LookAt(int ped, Vector3 camera)
    {
        var behind = MathF.Abs(Wrap(_yaw - Native.GetEntityHeading(ped))) > LookArc;

        var target = behind
            ? Native.GetOffsetFromEntityInWorldCoords(ped, 0f, 2f, 0.6f)
            : camera;

        var now = Native.GetGameTimer();
        var since = now - _lookedAt;

        if (since < LookThrottleMs
            || (since < LookRenewMs && Vector3.DistanceSquared(target, _lookAt) < LookStep * LookStep))
        {
            return;
        }

        _lookAt = target;
        _lookedAt = now;

        Native.TaskLookAtCoord(ped, target.X, target.Y, target.Z, LookMs, 0, 2);
    }

    private static void PlayWatchIdle(int ped, CameraFocus focus)
    {
        var wanted = focus == CameraFocus.FullArms && _watchesLoaded;

        if (wanted)
        {
            if (!Native.IsEntityPlayingAnim(ped, WatchDictionary, "BASE", 3))
            {
                Native.TaskPlayAnim(ped, WatchDictionary, "BASE", 8f, -8f, -1, 1, 0f, false, 0, false);
            }

            _watchPlaying = true;

            return;
        }

        if (!_watchPlaying)
        {
            return;
        }

        Native.StopAnimTask(ped, WatchDictionary, "BASE", -4f);

        _watchPlaying = false;
    }

    private static async Task TurnAroundAsync(int ped)
    {
        var position = Native.GetEntityCoords(ped, true);
        var heading = Native.GetEntityHeading(ped);

        Native.SetEntityCollision(ped, true, true);
        Native.FreezeEntityPosition(ped, false);
        Native.TaskGoStraightToCoord(ped, position.X, position.Y, position.Z, 8f, TurnMs, heading + 180f, 0.1f);

        var started = Native.GetGameTimer();

        while (Native.GetGameTimer() - started < TurnMs)
        {
            Native.DisableAllControlActions(ControlGroup);

            await API.Delay(0);
        }

        Native.ClearPedTasks(ped);
        Native.SetEntityCoordsNoOffset(ped, position.X, position.Y, position.Z, false, false, false);
        Native.FreezeEntityPosition(ped, true);
        Native.SetEntityCollision(ped, false, false);

        _lookAt = default;
        _lookedAt = 0;
    }

    private static void Suppress()
    {
        foreach (var control in Suppressed)
        {
            Native.DisableControlAction(ControlGroup, control, true);
        }
    }

    private static float Curve(float value)
    {
        var size = MathF.Abs(value);

        if (size < Deadzone)
        {
            return 0f;
        }

        var scaled = (size - Deadzone) / (1f - Deadzone);

        return MathF.CopySign(scaled * scaled, value);
    }

    private static float Wrap(float degrees)
    {
        var wrapped = degrees % 360f;

        return wrapped > 180f ? wrapped - 360f : wrapped < -180f ? wrapped + 360f : wrapped;
    }
}
