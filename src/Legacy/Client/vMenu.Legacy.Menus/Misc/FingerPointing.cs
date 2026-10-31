using CitizenFX.Core;
using CitizenFX.Core.Native;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Misc;

public static class FingerPointing
{
    private const string AnimDict = "anim@mp_point";

    private const string MoveNetwork = "task_mp_pointing";

    private const string IntroFinishedEvent = "IntroFinished";

    private const string BlendOutEvent = "BLEND_OUT";

    private const string StopState = "Stop";

    private const float BlendDuration = 0.5f;

    private const int TaskFlags = 24;

    private const int LoadTimeoutMs = 10_000;

    private const int AnimationTimeoutMs = 2_000;

    private const int FirstPersonViewMode = 4;

    private const string Unarmed = "weapon_unarmed";

    private const int DontTakeOffHelmet = 36;

    private const int ControlGroup = 0;

    private const int NextCameraControl = 0;

    private const int LookBehindControl = 26;

    #region Rockstar's pointing tunables

    private const float OffsetX = -0.2f;

    private const float OffsetMinY = 0.3f;

    private const float OffsetMaxY = 0.7f;

    private const float OffsetZ = 0.6f;

    private const float CapsuleRadius = 0.4f;

    private const float CapsuleHeight = 0.2f;

    private const int ProbeFlags = 1 | 2 | 4 | 8 | 16 | 64;

    #endregion

    private const int ShapeTestOptions = 7;

    private const int ShapeTestNotReady = 1;

    private const int ShapeTestReady = 2;

    #region Probe overlay colours

    private const int MarkerRed = 123;

    private const int ClearGreen = 53;

    private const int ClearBlue = 200;

    private const float SphereAlpha = 0.5f;

    #endregion

    private static readonly int[] QuitControls =
    [
        21,  // Sprint
        22,  // Jump
        23,  // Enter a vehicle
        24,  // Attack
        25,  // Aim
        27,  // Phone
        36,  // Duck
        37,  // Weapon wheel
        44,  // Cover
        45,  // Reload
        47,  // Detonate
        14,  // Weapon wheel next
        15,  // Weapon wheel previous
        16,  // Select next weapon
        17,  // Select previous weapon
        140, // Melee light
        141, // Melee heavy
        142, // Melee alternate
        143, // Melee block
        257, // Attack 2
    ];

    private static TickHandle? _tick;

    private static bool _registered;

    private static State _state;

    private static int _stateStartedAt;

    private static bool _stopRequested;

    private static int _probe;

    private static bool _blocked;

    private static float _probeX;

    private static float _probeY;

    private static float _probeZ;

    private static bool _heldHelmetOn;

    private static bool _debug;

    private enum State
    {
        Off,
        Loading,
        Intro,
        Loop,
        Outro,
    }

    public static bool Enabled => !StaffOnlyMode.Active && UserDefaults.MiscFingerPointing.Value;

    public static bool DebugVisible => _debug;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        _debug = UserDefaults.PointingDebug.Value;

        FingerPointingKeyBinding.Register(OnPressed);

        StaffOnlyMode.Changed += StopIfDisabled;

        API.RegisterCommand("vmenu_pointing_debug", new Action<int, List<object>, string>((_, _, _) => ToggleDebug()), false);

        _tick = TickRegistry.Register("Misc.Pointing", Frame, TickRate.PerFrame, () => _state != State.Off);
    }

    public static void SetEnabled(bool enabled)
    {
        UserDefaults.MiscFingerPointing.Value = enabled;

        if (!enabled && _state != State.Off)
        {
            Cleanup(API.PlayerPedId());
        }
    }

    private static void StopIfDisabled()
    {
        if (!Enabled && _state != State.Off)
        {
            Cleanup(API.PlayerPedId());
        }
    }

    public static void SetDebug(bool visible)
    {
        UserDefaults.PointingDebug.Value = visible;

        _debug = visible;
    }

    private static void ToggleDebug()
    {
        Script.RunOnMainThread(() =>
        {
            SetDebug(!_debug);

            Log.Info($"[vMenu] Pointing probe overlay: {(_debug ? "on" : "off")}");
        });
    }

    private static void OnPressed() => Script.RunOnMainThread(Toggle);

    private static void Toggle()
    {
        var ped = API.PlayerPedId();

        if (_state == State.Off)
        {
            if (CanStart(ped))
            {
                Start();
            }

            return;
        }

        if (_state != State.Outro)
        {
            _stopRequested = true;
        }
    }

    private static bool CanStart(int ped) =>
        Enabled
        && IsSafe(ped)
        && !MenuController.IsAnyMenuOpen()
        && !API.IsPauseMenuActive()
        && API.UpdateOnscreenKeyboard() != 0;

    private static bool IsSafe(int ped) =>
        !API.IsPedInAnyVehicle(ped, false)
        && !API.IsPedInjured(ped)
        && !API.IsPlayerSwitchInProgress()
        && !IsArmed(ped);

    // The pointing move network fights the aiming animation, so a drawn weapon is never worth the mess.
    private static bool IsArmed(int ped) => NativeFixer.GetSelectedPedWeapon(ped) != Hashing.Joaat(Unarmed);

    private static void Start()
    {
        API.RequestAnimDict(AnimDict);

        _stopRequested = false;

        Enter(State.Loading);

        _tick?.Reevaluate();
    }

    private static void Frame()
    {
        var ped = API.PlayerPedId();

        if (!IsSafe(ped))
        {
            Cleanup(ped);

            return;
        }

        API.DisableControlAction(ControlGroup, LookBehindControl, true);
        API.DisableControlAction(ControlGroup, NextCameraControl, true);

        switch (_state)
        {
            case State.Loading:
                Loading(ped);
                break;

            case State.Intro:
                Intro(ped);
                break;

            case State.Loop:
                Loop(ped);
                break;

            case State.Outro:
                Outro(ped);
                break;
        }

        if (_state is State.Off or State.Loading)
        {
            return;
        }

        if (!API.IsTaskMoveNetworkActive(ped))
        {
            if (_state != State.Intro)
            {
                Cleanup(ped);
            }

            return;
        }

        UpdateProbe(ped);

        PushSignals(ped);

        DrawDebug();
    }

    private static void Loading(int ped)
    {
        if (_stopRequested)
        {
            Cleanup(ped);

            return;
        }

        if (API.HasAnimDictLoaded(AnimDict))
        {
            API.TaskMoveNetwork(ped, MoveNetwork, BlendDuration, false, AnimDict, TaskFlags);

            Enter(State.Intro);

            return;
        }

        if (Elapsed() > LoadTimeoutMs)
        {
            Cleanup(ped);
        }
    }

    private static void Intro(int ped)
    {
        if (API.IsTaskMoveNetworkActive(ped) && API.GetTaskMoveNetworkEvent(ped, IntroFinishedEvent))
        {
            HoldHelmetOn(ped);

            Enter(State.Loop);

            return;
        }

        if (Elapsed() > AnimationTimeoutMs)
        {
            Cleanup(ped);
        }
    }

    private static void Loop(int ped)
    {
        if (!_stopRequested && !QuitPressed())
        {
            return;
        }

        _stopRequested = false;

        if (API.IsTaskMoveNetworkReadyForTransition(ped))
        {
            API.RequestTaskMoveNetworkStateTransition(ped, StopState);
        }

        Enter(State.Outro);
    }

    private static void Outro(int ped)
    {
        if (API.GetTaskMoveNetworkEvent(ped, BlendOutEvent) || Elapsed() > AnimationTimeoutMs)
        {
            Cleanup(ped);
        }
    }

    private static void Cleanup(int ped)
    {
        API.RemoveAnimDict(AnimDict);

        if (API.IsTaskMoveNetworkActive(ped) && !API.IsPedInjured(ped))
        {
            API.ClearPedSecondaryTask(ped);
        }

        if (_heldHelmetOn)
        {
            API.SetPedConfigFlag(ped, DontTakeOffHelmet, false);

            _heldHelmetOn = false;
        }

        _state = State.Off;
        _stopRequested = false;
        _probe = 0;
        _blocked = false;

        _tick?.Reevaluate();
    }

    private static void Enter(State state)
    {
        _state = state;
        _stateStartedAt = API.GetGameTimer();
    }

    private static int Elapsed() => API.GetGameTimer() - _stateStartedAt;

    private static bool QuitPressed()
    {
        foreach (var control in QuitControls)
        {
            if (API.IsControlPressed(ControlGroup, control))
            {
                return true;
            }
        }

        return false;
    }

    private static void PushSignals(int ped)
    {
        API.SetTaskMoveNetworkSignalFloat(ped, "Pitch", Pitch());
        API.SetTaskMoveNetworkSignalFloat(ped, "Heading", Heading());
        API.SetTaskMoveNetworkSignalBool(ped, "isBlocked", _blocked);
        API.SetTaskMoveNetworkSignalBool(ped, "isFirstPerson", IsFirstPerson());

        NativeFixer.GetPedCurrentMoveBlendRatio(ped, out _, out var forward);

        API.SetTaskMoveNetworkSignalFloat(ped, "Speed", Math.Abs(forward));
    }

    private static float Pitch()
    {
        var pitch = Math.Clamp(API.GetGameplayCamRelativePitch(), -70f, 42f);

        return (pitch + 70f) / 112f;
    }

    private static float Heading()
    {
        var heading = Math.Clamp(API.GetGameplayCamRelativeHeading(), -180f, 180f);

        return 1f - ((heading + 180f) / 360f);
    }

    private static bool IsFirstPerson() => API.GetFollowPedCamViewMode() == FirstPersonViewMode;

    private static void UpdateProbe(int ped)
    {
        if (_probe != 0)
        {
            var status = NativeFixer.GetShapeTestResult(_probe, out var hit, out _, out _, out _);

            if (status != ShapeTestNotReady)
            {
                _blocked = status == ShapeTestReady && hit != 0;
                _probe = 0;
            }
        }

        var heading = Math.Clamp(API.GetGameplayCamRelativeHeading(), -180f, 180f);
        var forward = ((OffsetMaxY - OffsetMinY) * ((heading + 180f) / 360f)) + OffsetMinY;

        var radians = heading * Math.PI / 180.0;
        var cos = (float)Math.Cos(radians);
        var sin = (float)Math.Sin(radians);

        var offset = API.GetOffsetFromEntityInWorldCoords(
            ped,
            (cos * OffsetX) - (sin * forward),
            (sin * OffsetX) + (cos * forward),
            OffsetZ);

        _probeX = offset.X;
        _probeY = offset.Y;
        _probeZ = offset.Z;

        if (_probe == 0)
        {
            _probe = API.StartShapeTestCapsule(
                _probeX, _probeY, _probeZ - CapsuleHeight,
                _probeX, _probeY, _probeZ + CapsuleHeight,
                CapsuleRadius,
                ProbeFlags,
                ped,
                ShapeTestOptions);
        }
    }

    private static void HoldHelmetOn(int ped)
    {
        if (API.GetPedConfigFlag(ped, DontTakeOffHelmet, false))
        {
            return;
        }

        API.SetPedConfigFlag(ped, DontTakeOffHelmet, true);

        _heldHelmetOn = true;
    }

    private static void DrawDebug()
    {
        if (!_debug)
        {
            return;
        }

        var green = _blocked ? 0 : ClearGreen;
        var blue = _blocked ? 0 : ClearBlue;

        NativeFixer.DrawMarkerSphere(_probeX, _probeY, _probeZ, CapsuleRadius, MarkerRed, green, blue, SphereAlpha);

        API.DrawLine(
            _probeX, _probeY, _probeZ - CapsuleHeight,
            _probeX, _probeY, _probeZ + CapsuleHeight,
            255, 0, 0, 255);

        Hud.DrawText($"Pointing: {_state}, blocked: {_blocked}", 0f, 0f);
    }
}
