using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Freecam;

public static class Freecam
{
    public const float DefaultFov = 45f;

    public const float MinFov = 5f;

    public const float MaxFov = 100f;

    private const float MinSpeed = 0.25f;

    private const float MaxSpeed = 300f;

    private const float MaxPitch = 89.5f;

    private const float RollSpeed = 60f;

    private const float ZoomRate = 1.5f;

    private const float FastMultiplier = 4f;

    private const float SlowMultiplier = 0.2f;

    private const float SpeedStep = 1.25f;

    private const float StickLookScale = 14f;

    private const float StickDeadzone = 0.15f;

    private const float MouseLookScale = 1.25f;

    private const float SmoothingSeconds = 0.1f;

    private const int FadeMs = 500;

    private const int ExitHereHoldMs = 500;

    private const int ControlGroup = 0;

    private const int LookLeftRight = 1;

    private const int LookUpDown = 2;

    private const int MoveLeftRight = 30;

    private const int MoveUpDown = 31;

    private const string CameraName = "DEFAULT_SCRIPTED_CAMERA";

    private static readonly int[] SuppressedControls =
    [
        0, 14, 15, 16, 17, 19, 20, 21, 22, 23, 24, 25, 26, 27, 30, 31, 32, 33, 34, 35, 36, 37, 44, 45, 47, 58, 74,
        85, 140, 141, 142, 143, 170, 172, 173, 174, 175, 257, 261, 262, 263, 264, 288, 289, 303, 304, 311,
    ];

    private static readonly AcceleratedAxis StrafeAxis = new();

    private static readonly AcceleratedAxis ForwardAxis = new();

    private static readonly AcceleratedAxis LiftAxis = new();

    private static readonly AcceleratedAxis RollAxis = new();

    private static readonly AcceleratedAxis ZoomAxis = new();

    private static readonly GlobalInstructionalButtons Buttons = new();

    private enum Phase
    {
        Off,
        Flying,
        Returning,
    }

    private static Phase _phase;

    private static int _camera;

    private static Vector3 _position;

    private static float _pitch;

    private static float _roll;

    private static float _yaw;

    private static float _fov = DefaultFov;

    private static Vector3 _worldPosition;

    private static Vector3 _worldRotation;

    private static float _worldFov = DefaultFov;

    private static float _worldFocus = 10f;

    private static bool _hudHidden;

    private static bool _faded;

    private static bool _fadingOut;

    private static bool _buttonsKeyboard;

    private static bool _buttonsBuilt;

    private static int _toggleHeldSince;

    private static TickHandle? _tick;

    public static bool IsActive => _phase == Phase.Flying;

    public static Vector3 Direction => Orientation.Forward(Orientation.FromEuler(_worldRotation));

    public static CameraPose Pose => new(_worldPosition, _worldRotation, _worldFov, _worldFocus);

    public static float Fov
    {
        get => _fov;
        set => _fov = Math.Clamp(value, MinFov, MaxFov);
    }

    public static float Roll
    {
        get => _roll;
        set => _roll = Orientation.Wrap(value);
    }

    public static float Speed
    {
        get => Math.Clamp(UserDefaults.FreecamSpeed.Value, MinSpeed, MaxSpeed);
        set
        {
            UserDefaults.FreecamSpeed.Value = Math.Clamp(value, MinSpeed, MaxSpeed);

            Buttons.Refresh();
        }
    }

    public static event Action? StateChanged;

    public static event Action<int>? EntityReleased;

    private static bool IsAllowed => ClientPermissions.IsAllowed(Global.Freecam);

    public static void Initialize()
    {
        FreecamKeyBindings.Reset.Pressed = ResetLens;
        FreecamKeyBindings.SpeedUp.Pressed = () => StepSpeed(SpeedStep);
        FreecamKeyBindings.SpeedDown.Pressed = () => StepSpeed(1f / SpeedStep);
        FreecamKeyBindings.AddKeyframe.Pressed = AddKeyframePressed;
        FreecamKeyBindings.DeleteKeyframe.Pressed = DeleteKeyframePressed;
        FreecamKeyBindings.Play.Pressed = PlayPressed;
        FreecamKeyBindings.HideHud.Pressed = () => _hudHidden = !_hudHidden;
        FreecamKeyBindings.Hints.Pressed = () => UserDefaults.FreecamHideHints.Value = !UserDefaults.FreecamHideHints.Value;
        FreecamKeyBindings.Register(() => IsActive && !Native.IsPauseMenuActive(), TogglePressed, ToggleReleased);

        _tick = TickRegistry.Register(
            "Freecam.Camera",
            Frame,
            TickRate.PerFrame,
            condition: () => _phase != Phase.Off,
            onStopped: Cleanup);

        NoClip.NoClip.IsBlocked = () => _phase != Phase.Off;

        ClientPermissions.PermissionsChanged += OnPermissionsChanged;
        ResourceShutdown.Stopping += Cleanup;
    }

    public static bool Enable()
    {
        if (!IsAllowed || SceneLock.IsActive)
        {
            return false;
        }

        API.RunOnMainThread(() => SetActive(true));

        return true;
    }

    public static void Disable() => API.RunOnMainThread(() => SetActive(false));

    public static void SetPose(CameraPose pose)
    {
        if (_phase != Phase.Flying)
        {
            return;
        }

        PathPlayer.Stop();

        SetWorldPose(pose);
    }

    public static CameraPose PoseIn(PathSpace space, AttachMode mode) =>
        space == PathSpace.World || !TargetLock.IsValid ? Pose : ToLocal(Pose, TargetLock.Basis(mode));

    internal static CameraPose ToWorld(CameraPose pose, PathSpace space, AttachMode mode)
    {
        if (space == PathSpace.World || !TargetLock.IsValid)
        {
            return pose;
        }

        var (origin, basis) = TargetLock.Basis(mode);

        return new CameraPose(
            origin + Vector3.Transform(pose.Position, basis),
            Orientation.ToEuler(basis * Orientation.FromEuler(pose.Rotation)),
            pose.Fov,
            pose.Focus);
    }

    private static CameraPose ToLocal(CameraPose pose, (Vector3 Origin, Quaternion Rotation) basis)
    {
        var inverse = Quaternion.Inverse(basis.Rotation);

        return new CameraPose(
            Vector3.Transform(pose.Position - basis.Origin, inverse),
            Orientation.ToEuler(inverse * Orientation.FromEuler(pose.Rotation)),
            pose.Fov,
            pose.Focus);
    }

    internal static void ChangeBasis(Action change)
    {
        var world = Pose;

        change();

        if (_phase == Phase.Flying)
        {
            SetWorldPose(world);
        }
    }

    private static void TogglePressed()
    {
        API.RunOnMainThread(() =>
        {
            if (_phase == Phase.Returning || Native.IsPauseMenuActive())
            {
                return;
            }

            if (_phase == Phase.Flying)
            {
                _toggleHeldSince = Native.GetGameTimer();

                return;
            }

            if (!IsAllowed)
            {
                Notifications.Warning(MenuText.Key(Loc.Freecam.ToggleDenied));

                return;
            }

            SetActive(true);
        });
    }

    private static void ToggleReleased()
    {
        API.RunOnMainThread(() =>
        {
            if (_toggleHeldSince == 0)
            {
                return;
            }

            _toggleHeldSince = 0;

            SetActive(false);
        });
    }

    private static void OnPermissionsChanged()
    {
        if (!IsAllowed)
        {
            SetActive(false);
        }
    }

    private static void SetActive(bool active)
    {
        if (active)
        {
            if (_phase != Phase.Off || !IsAllowed || SceneLock.IsActive)
            {
                return;
            }

            if (NoClip.NoClip.IsActive)
            {
                NoClip.NoClip.Disable();
            }

            API.EmitServer(PlayerStateEvents.ReportFreecam, true);

            Begin();

            _phase = Phase.Flying;
        }
        else
        {
            if (_phase != Phase.Flying)
            {
                return;
            }

            PathPlayer.Stop();

            _phase = Phase.Returning;
            _faded = FreecamActor.NeedsFade(_worldPosition);
            _fadingOut = _faded;

            if (_faded)
            {
                Buttons.Hide();
                Native.DoScreenFadeOut(FadeMs);
            }
            else
            {
                BeginReturn();
            }

            API.EmitServer(PlayerStateEvents.ReportFreecam, false);
        }

        _tick?.Reevaluate();

        StateChanged?.Invoke();
    }

    private static void Begin()
    {
        var position = Native.GetGameplayCamCoord();
        var rotation = Native.GetGameplayCamRot(Orientation.RotationOrder);

        TargetLock.Reset();
        ResetLens();
        StopMoving();

        _position = position;
        _pitch = Math.Clamp(rotation.X, -MaxPitch, MaxPitch);
        _yaw = rotation.Z;
        _fov = Math.Clamp(Native.GetGameplayCamFov(), MinFov, MaxFov);
        _worldPosition = position;
        _worldRotation = new Vector3(_pitch, 0f, _yaw);
        _worldFov = _fov;
        _worldFocus = CameraEffects.FocusDistance;
        _hudHidden = false;
        _toggleHeldSince = 0;

        _camera = Native.CreateCamWithParams(CameraName, position.X, position.Y, position.Z, _pitch, 0f, _yaw, _fov, true, Orientation.RotationOrder);

        Native.RenderScriptCams(true, false, 0, false, false, 0);

        FreecamActor.Leave(Native.PlayerPedId());

        _buttonsBuilt = false;
    }

    private static void BeginReturn()
    {
        DestroyCamera();

        Buttons.Hide();

        FreecamActor.BeginReturn(Native.PlayerPedId());
    }

    private static void Finish()
    {
        var ped = Native.PlayerPedId();

        FreecamActor.Finish(ped);

        Complete(ped);
    }

    // The car is left behind on purpose: it can be far away with nobody near it, so it can't be brought along.
    private static void ExitHere()
    {
        var ped = Native.PlayerPedId();
        var position = _worldPosition;

        _toggleHeldSince = 0;

        PathPlayer.Stop();
        DestroyCamera();
        Buttons.Hide();

        if (Native.GetGroundZFor_3dCoord(position.X, position.Y, position.Z, out var ground, false, false))
        {
            position = new Vector3(position.X, position.Y, ground);
        }

        FreecamActor.FinishAt(ped, position, _worldRotation.Z);

        API.EmitServer(PlayerStateEvents.ReportFreecam, false);

        Complete(ped);
    }

    private static void Complete(int ped)
    {
        _phase = Phase.Off;

        FadeBackIn();

        TargetLock.Reset();

        _tick?.Reevaluate();

        EntityReleased?.Invoke(ped);

        StateChanged?.Invoke();
    }

    private static void Cleanup()
    {
        DestroyCamera();

        Buttons.Hide();

        if (_phase == Phase.Off)
        {
            return;
        }

        PathPlayer.Stop();

        FreecamActor.Finish(Native.PlayerPedId());

        _phase = Phase.Off;

        FadeBackIn();
    }

    private static void FadeBackIn()
    {
        _fadingOut = false;

        if (!_faded)
        {
            return;
        }

        _faded = false;

        Native.DoScreenFadeIn(FadeMs);
    }

    private static void DestroyCamera()
    {
        if (_camera == 0)
        {
            return;
        }

        Native.RenderScriptCams(false, false, 0, true, false, 0);

        if (Native.DoesCamExist(_camera))
        {
            Native.SetCamActive(_camera, false);
            Native.DestroyCam(_camera, false);
        }

        _camera = 0;

        Hud.Hidden = false;

        CameraEffects.Forget();
    }

    private static void Frame()
    {
        var ped = Native.PlayerPedId();

        if (_phase == Phase.Returning)
        {
            if (_fadingOut)
            {
                Render();
                FreecamActor.Follow(ped, _worldPosition);

                if (Native.IsScreenFadedOut())
                {
                    _fadingOut = false;

                    BeginReturn();
                }

                return;
            }

            if (FreecamActor.TickReturn(ped))
            {
                Finish();
            }

            return;
        }

        if (!Native.DoesCamExist(_camera) || SceneLock.IsActive || Native.IsEntityDead(ped, false))
        {
            SetActive(false);

            return;
        }

        if (_toggleHeldSince != 0 && Native.GetGameTimer() - _toggleHeldSince >= ExitHereHoldMs)
        {
            ExitHere();

            return;
        }

        foreach (var control in SuppressedControls)
        {
            Native.DisableControlAction(ControlGroup, control, false);
        }

        var delta = Native.GetFrameTime();

        if (PathPlayer.IsActive)
        {
            Playback(delta);
        }
        else
        {
            Fly(delta);
        }

        Render();

        FreecamActor.Follow(ped, _worldPosition);

        Overlay();
    }

    private static void Fly(float delta)
    {
        var following = TargetLock.IsFollowing;
        var (origin, basis) = TargetLock.FollowBasis();
        var lookAt = TargetLock.IsValid && TargetLock.LookAt;

        if (TargetLock.IsLocked && !TargetLock.IsValid)
        {
            TargetLock.Release();
        }

        var input = !Native.IsPauseMenuActive();
        var menuOpen = MenuController.IsAnyMenuOpen();
        var keyboard = Native.IsUsingKeyboardAndMouse(2);

        if (input)
        {
            Look(delta, keyboard, menuOpen);
        }

        var rollInput = input && !menuOpen ? Axis(FreecamKeyBindings.RollRight, FreecamKeyBindings.RollLeft) : 0f;
        _roll = Orientation.Wrap(_roll + (RollAxis.Step(rollInput, RollSpeed, SmoothingSeconds, delta) * delta));

        var zoomInput = input ? Axis(FreecamKeyBindings.ZoomIn, FreecamKeyBindings.ZoomOut) : 0f;
        var zoomFactor = MaxFov / _fov;
        zoomFactor = Math.Clamp(zoomFactor + (ZoomAxis.Step(zoomInput, ZoomRate, SmoothingSeconds, delta) * zoomFactor * delta), 1f, MaxFov / MinFov);
        _fov = MaxFov / zoomFactor;
        _worldFov = _fov;

        var worldPosition = origin + Vector3.Transform(_position, basis);
        var orientation = lookAt
            ? LookAtOrientation(worldPosition)
            : basis * Orientation.FromEuler(new Vector3(_pitch, _roll, _yaw));

        var strafe = 0f;
        var forward = 0f;
        var lift = 0f;

        if (input)
        {
            strafe = Axis(FreecamKeyBindings.Right, FreecamKeyBindings.Left);
            forward = Axis(FreecamKeyBindings.Forward, FreecamKeyBindings.Backward);
            lift = Axis(FreecamKeyBindings.Up, FreecamKeyBindings.Down);

            if (!keyboard)
            {
                strafe += Stick(Native.GetDisabledControlNormal(ControlGroup, MoveLeftRight));
                forward -= Stick(Native.GetDisabledControlNormal(ControlGroup, MoveUpDown));
            }
        }

        var maxSpeed = Speed * (FreecamKeyBindings.Fast.Down ? FastMultiplier : FreecamKeyBindings.Slow.Down ? SlowMultiplier : 1f);
        var smoothing = UserDefaults.FreecamSmoothing.Value * SmoothingSeconds;

        var movement =
            (Orientation.Right(orientation) * StrafeAxis.Step(Math.Clamp(strafe, -1f, 1f), maxSpeed, smoothing, delta))
            + (Orientation.Forward(orientation) * ForwardAxis.Step(Math.Clamp(forward, -1f, 1f), maxSpeed, smoothing, delta))
            + (Vector3.UnitZ * LiftAxis.Step(Math.Clamp(lift, -1f, 1f), maxSpeed, smoothing, delta));

        _position += Vector3.Transform(movement * delta, Quaternion.Inverse(basis));

        _worldPosition = origin + Vector3.Transform(_position, basis);

        if (lookAt)
        {
            orientation = LookAtOrientation(_worldPosition);
        }

        _worldRotation = !following && !lookAt
            ? new Vector3(_pitch, _roll, _yaw)
            : Orientation.ToEuler(orientation);

        _worldFocus = CameraEffects.Focus == FocusMode.Target && TargetLock.IsValid
            ? Vector3.Distance(_worldPosition, TargetLock.LookPoint())
            : CameraEffects.FocusDistance;
    }

    private static void Look(float delta, bool keyboard, bool menuOpen)
    {
        var lookX = Native.GetDisabledControlNormal(ControlGroup, LookLeftRight);
        var lookY = Native.GetDisabledControlNormal(ControlGroup, LookUpDown);
        var sensitivity = Math.Clamp(UserDefaults.FreecamLookSensitivity.Value, 1, 20) * (_fov / DefaultFov);
        var invert = UserDefaults.FreecamInvertLook.Value ? -1f : 1f;

        if (keyboard)
        {
            _yaw -= lookX * sensitivity * MouseLookScale;
            _pitch -= lookY * sensitivity * MouseLookScale * invert;
        }
        else if (!menuOpen)
        {
            _yaw -= Stick(lookX) * sensitivity * StickLookScale * delta;
            _pitch -= Stick(lookY) * sensitivity * StickLookScale * delta * invert;
        }

        _yaw = Orientation.Wrap(_yaw);
        _pitch = Math.Clamp(_pitch, -MaxPitch, MaxPitch);
    }

    private static Quaternion LookAtOrientation(Vector3 from)
    {
        var (pitch, yaw) = Orientation.LookAngles(TargetLock.LookPoint() - from);

        return Orientation.FromEuler(new Vector3(pitch, _roll, yaw));
    }

    private static void Playback(float delta)
    {
        var path = Filmmaker.ActivePath;
        var targeted = path.Space == PathSpace.Target;

        if (path.Keyframes.Count == 0 || (targeted && !TargetLock.IsValid))
        {
            PathPlayer.Stop();

            Fly(delta);

            return;
        }

        var sample = PathPlayer.Advance(path, delta, out var finished);

        sample = ToWorld(sample, path.Space, path.Attach);

        _worldPosition = sample.Position;
        _worldRotation = sample.Rotation;
        _worldFov = Math.Clamp(sample.Fov, MinFov, MaxFov);
        _worldFocus = CameraEffects.Focus == FocusMode.Target && TargetLock.IsValid
            ? Vector3.Distance(_worldPosition, TargetLock.LookPoint())
            : sample.Focus;

        if (!finished)
        {
            return;
        }

        PathPlayer.Stop();

        if (PathPlayer.StayOnLastFrame)
        {
            SetWorldPose(sample);
        }
    }

    private static void SetWorldPose(CameraPose pose)
    {
        var local = TargetLock.IsFollowing ? ToLocal(pose, TargetLock.Basis(TargetLock.Mode)) : pose;

        _position = local.Position;
        _pitch = Math.Clamp(local.Rotation.X, -MaxPitch, MaxPitch);
        _roll = Orientation.Wrap(local.Rotation.Y);
        _yaw = Orientation.Wrap(local.Rotation.Z);
        _fov = Math.Clamp(pose.Fov, MinFov, MaxFov);
        _worldFov = _fov;
        _worldPosition = pose.Position;
        _worldRotation = pose.Rotation;
        _worldFocus = pose.Focus;

        if (CameraEffects.Focus == FocusMode.Manual && pose.Focus > 0f)
        {
            CameraEffects.FocusDistance = pose.Focus;
        }

        StopMoving();
    }

    private static void Render()
    {
        Native.SetCamCoord(_camera, _worldPosition.X, _worldPosition.Y, _worldPosition.Z);
        Native.SetCamRot(_camera, _worldRotation.X, _worldRotation.Y, _worldRotation.Z, Orientation.RotationOrder);
        Native.SetCamFov(_camera, _worldFov);

        CameraEffects.Apply(_camera, _worldFocus);
    }

    private static void Overlay()
    {
        var playing = PathPlayer.IsActive;
        var hideHud = _hudHidden || CameraEffects.HideHud || (playing && PathPlayer.HideHud);

        Hud.Hidden = hideHud;

        if (hideHud)
        {
            Native.HideHudAndRadarThisFrame();
            Native.ThefeedHideThisFrame();
        }

        if (CameraEffects.Letterbox || (playing && PathPlayer.Letterbox))
        {
            CameraEffects.DrawLetterbox();
        }

        var showButtons = !hideHud && !playing && !UserDefaults.FreecamHideHints.Value;

        if (!showButtons)
        {
            if (Buttons.IsShown)
            {
                Buttons.Hide();
            }

            return;
        }

        var keyboard = Native.IsUsingKeyboardAndMouse(2);

        if (!_buttonsBuilt || keyboard != _buttonsKeyboard)
        {
            BuildButtons(keyboard);
        }

        if (!Buttons.IsShown)
        {
            Buttons.Show();
        }
    }

    private static void BuildButtons(bool keyboard)
    {
        _buttonsBuilt = true;
        _buttonsKeyboard = keyboard;

        Buttons.Clear();

        if (keyboard)
        {
            Buttons.Add(
                Text(Loc.Freecam.ButtonMove),
                FreecamKeyBindings.Forward.KeyboardControl,
                FreecamKeyBindings.Left.KeyboardControl,
                FreecamKeyBindings.Backward.KeyboardControl,
                FreecamKeyBindings.Right.KeyboardControl);
        }
        else
        {
            Buttons.Add(Text(Loc.Freecam.ButtonMove), MoveLeftRight);
        }

        Buttons
            .Add(Text(Loc.Freecam.ButtonHeight), FreecamKeyBindings.Up.Control(keyboard), FreecamKeyBindings.Down.Control(keyboard))
            .Add(Text(Loc.Freecam.ButtonZoom), FreecamKeyBindings.ZoomIn.Control(keyboard), FreecamKeyBindings.ZoomOut.Control(keyboard))
            .Add(Text(Loc.Freecam.ButtonRoll), FreecamKeyBindings.RollLeft.Control(keyboard), FreecamKeyBindings.RollRight.Control(keyboard))
            .Add(Text(Loc.Freecam.ButtonReset), FreecamKeyBindings.Reset.Control(keyboard))
            .Add(Text(Loc.Freecam.ButtonFastSlow), FreecamKeyBindings.Fast.Control(keyboard), FreecamKeyBindings.Slow.Control(keyboard));

        if (!keyboard)
        {
            return;
        }

        Buttons
            .Add(Text(Loc.Freecam.ButtonExit), FreecamKeyBindings.Toggle.KeyboardControl)
            .Add(
                () => MenuText.Key(Loc.Freecam.ButtonSpeed, ("speed", MenuText.Literal(Speed.ToString("0.##", CultureInfo.InvariantCulture)))).Resolve(Localizer.Current),
                FreecamKeyBindings.SpeedUp.KeyboardControl,
                FreecamKeyBindings.SpeedDown.KeyboardControl)
            .Add(Text(Loc.Freecam.ButtonAddKeyframe), FreecamKeyBindings.AddKeyframe.KeyboardControl)
            .Add(Text(Loc.Freecam.ButtonPlay), FreecamKeyBindings.Play.KeyboardControl)
            .Add(Text(Loc.Freecam.ButtonHideHud), FreecamKeyBindings.HideHud.KeyboardControl)
            .Add(Text(Loc.Freecam.ButtonHints), FreecamKeyBindings.Hints.KeyboardControl);
    }

    private static Func<string> Text(string key) => () => Localizer.Current.Get(key);

    public static void ResetLens()
    {
        _roll = 0f;
        _fov = DefaultFov;

        RollAxis.Reset();
        ZoomAxis.Reset();
    }

    private static void StopMoving()
    {
        StrafeAxis.Reset();
        ForwardAxis.Reset();
        LiftAxis.Reset();
    }

    private static void StepSpeed(float factor)
    {
        if (MenuController.IsAnyMenuOpen())
        {
            return;
        }

        Speed *= factor;
    }

    private static void AddKeyframePressed()
    {
        var outcome = Filmmaker.AddKeyframe();

        if (outcome == KeyframeOutcome.Done)
        {
            Notifications.Success(MenuText.Key(Loc.Freecam.KeyframeAdded, ("count", Count())));

            return;
        }

        Report(outcome);
    }

    private static void DeleteKeyframePressed()
    {
        if (MenuController.IsAnyMenuOpen())
        {
            return;
        }

        var outcome = Filmmaker.RemoveLastKeyframe();

        if (outcome == KeyframeOutcome.Done)
        {
            Notifications.Info(MenuText.Key(Loc.Freecam.KeyframeRemoved, ("count", Count())));

            return;
        }

        Report(outcome);
    }

    private static void PlayPressed()
    {
        if (PathPlayer.IsActive)
        {
            PathPlayer.Stop();

            return;
        }

        Report(Filmmaker.Play());
    }

    private static MenuText Count() =>
        MenuText.Literal(Filmmaker.ActivePath.Keyframes.Count.ToString(CultureInfo.InvariantCulture));

    public static void Report(KeyframeOutcome outcome) => ShowError(outcome switch
    {
        KeyframeOutcome.NotInFreecam => Loc.Freecam.NotActive,
        KeyframeOutcome.Full => Loc.Freecam.PathFull,
        KeyframeOutcome.SpaceMismatch => Loc.Freecam.SpaceMismatch,
        KeyframeOutcome.Empty => Loc.Freecam.PathEmpty,
        _ => null,
    });

    public static void Report(PlayOutcome outcome) => ShowError(outcome switch
    {
        PlayOutcome.NotInFreecam => Loc.Freecam.NotActive,
        PlayOutcome.TooFewKeyframes => Loc.Freecam.TooFewKeyframes,
        PlayOutcome.NeedsTarget => Loc.Freecam.NeedsTarget,
        _ => null,
    });

    private static void ShowError(string? key)
    {
        if (key is not null)
        {
            Notifications.Error(MenuText.Key(key));
        }
    }

    private static float Axis(HeldKey positive, HeldKey negative) =>
        (positive.Down ? 1f : 0f) - (negative.Down ? 1f : 0f);

    private static float Stick(float value)
    {
        var size = MathF.Abs(value);

        if (size < StickDeadzone)
        {
            return 0f;
        }

        return MathF.Sign(value) * ((size - StickDeadzone) / (1f - StickDeadzone));
    }
}
