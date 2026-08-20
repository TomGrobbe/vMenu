using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

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

    private static uint _storedWeapon;

    private static bool _hidWeapon;

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

    public static bool Enabled => UserDefaults.MiscFingerPointing.Value;

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

        SharedAPI.Commands.RegisterCommand("vmenu_pointing_debug", false, new Action(ToggleDebug));

        _tick = TickRegistry.Register("Misc.Pointing", Frame, TickRate.PerFrame, () => _state != State.Off);
    }

    public static void SetEnabled(bool enabled)
    {
        UserDefaults.MiscFingerPointing.Value = enabled;

        if (!enabled && _state != State.Off)
        {
            Cleanup(Native.PlayerPedId());
        }
    }

    public static void SetDebug(bool visible)
    {
        UserDefaults.PointingDebug.Value = visible;

        _debug = visible;
    }

    private static void ToggleDebug()
    {
        SetDebug(!_debug);

        Log.Info($"[vMenu] Pointing probe overlay: {(_debug ? "on" : "off")}");
    }

    private static void OnPressed() => SharedAPI.RunOnMainThread(Toggle);

    private static void Toggle()
    {
        var ped = Native.PlayerPedId();

        if (_state == State.Off)
        {
            if (CanStart(ped))
            {
                Start(ped);
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
        && !Native.IsPauseMenuActive()
        && Native.UpdateOnscreenKeyboard() != 0;

    private static bool IsSafe(int ped) =>
        !Native.IsPedInAnyVehicle(ped, false)
        && !Native.IsPedInjured(ped)
        && !Native.IsPlayerSwitchInProgress();

    private static void Start(int ped)
    {
        Native.RequestAnimDict(AnimDict);

        _stopRequested = false;

        if (IsTwoHanded(Native.GetSelectedPedWeapon(ped)))
        {
            Native.SetPedCurrentWeaponVisible(ped, false, false, false, true);

            _hidWeapon = true;
        }

        Enter(State.Loading);

        _tick?.Reevaluate();
    }

    private static void Frame()
    {
        var ped = Native.PlayerPedId();

        if (!IsSafe(ped))
        {
            Cleanup(ped);

            return;
        }

        Native.DisableControlAction(ControlGroup, LookBehindControl, true);
        Native.DisableControlAction(ControlGroup, NextCameraControl, true);

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

        if (!Native.IsTaskMoveNetworkActive(ped))
        {
            if (_state != State.Intro)
            {
                Cleanup(ped);
            }

            return;
        }

        UpdateStowedWeapon(ped);

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

        if (Native.HasAnimDictLoaded(AnimDict))
        {
            Native.TaskMoveNetwork(ped, MoveNetwork, BlendDuration, false, AnimDict, TaskFlags);

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
        if (Native.IsTaskMoveNetworkActive(ped) && Native.GetTaskMoveNetworkEvent(ped, IntroFinishedEvent))
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

        if (Native.IsTaskMoveNetworkReadyForTransition(ped))
        {
            Native.RequestTaskMoveNetworkStateTransition(ped, StopState);
        }

        Enter(State.Outro);
    }

    private static void Outro(int ped)
    {
        if (Native.GetTaskMoveNetworkEvent(ped, BlendOutEvent) || Elapsed() > AnimationTimeoutMs)
        {
            Cleanup(ped);
        }
    }

    private static void Cleanup(int ped)
    {
        Native.RemoveAnimDict(AnimDict);

        if (Native.IsTaskMoveNetworkActive(ped) && !Native.IsPedInjured(ped))
        {
            Native.ClearPedSecondaryTask(ped);
        }

        RestoreWeapon(ped);

        if (_hidWeapon)
        {
            if (!Native.IsPedInAnyVehicle(ped, true))
            {
                Native.SetPedCurrentWeaponVisible(ped, true, false, false, true);
            }

            _hidWeapon = false;
        }

        if (_heldHelmetOn)
        {
            Native.SetPedConfigFlag(ped, DontTakeOffHelmet, false);

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
        _stateStartedAt = Native.GetGameTimer();
    }

    private static int Elapsed() => Native.GetGameTimer() - _stateStartedAt;

    private static bool QuitPressed()
    {
        foreach (var control in QuitControls)
        {
            if (Native.IsControlPressed(ControlGroup, control))
            {
                return true;
            }
        }

        return false;
    }

    private static void PushSignals(int ped)
    {
        Native.SetTaskMoveNetworkSignalFloat(ped, "Pitch", Pitch());
        Native.SetTaskMoveNetworkSignalFloat(ped, "Heading", Heading());
        Native.SetTaskMoveNetworkSignalBool(ped, "isBlocked", _blocked);
        Native.SetTaskMoveNetworkSignalBool(ped, "isFirstPerson", IsFirstPerson());

        Native.GetPedCurrentMoveBlendRatio(ped, out _, out var forward);

        Native.SetTaskMoveNetworkSignalFloat(ped, "Speed", Math.Abs(forward));
    }

    private static float Pitch()
    {
        var pitch = Math.Clamp(Native.GetGameplayCamRelativePitch(), -70f, 42f);

        return (pitch + 70f) / 112f;
    }

    private static float Heading()
    {
        var heading = Math.Clamp(Native.GetGameplayCamRelativeHeading(), -180f, 180f);

        return 1f - ((heading + 180f) / 360f);
    }

    private static bool IsFirstPerson() => Native.GetFollowPedCamViewMode() == FirstPersonViewMode;

    private static void UpdateProbe(int ped)
    {
        if (_probe != 0)
        {
            var status = Native.GetShapeTestResult(_probe, out var hit, out _, out _, out _);

            if (status != ShapeTestNotReady)
            {
                _blocked = status == ShapeTestReady && hit != 0;
                _probe = 0;
            }
        }

        var heading = Math.Clamp(Native.GetGameplayCamRelativeHeading(), -180f, 180f);
        var forward = ((OffsetMaxY - OffsetMinY) * ((heading + 180f) / 360f)) + OffsetMinY;

        var radians = heading * Math.PI / 180.0;
        var cos = (float)Math.Cos(radians);
        var sin = (float)Math.Sin(radians);

        var offset = Native.GetOffsetFromEntityInWorldCoords(
            ped,
            (cos * OffsetX) - (sin * forward),
            (sin * OffsetX) + (cos * forward),
            OffsetZ);

        _probeX = offset.X;
        _probeY = offset.Y;
        _probeZ = offset.Z;

        if (_probe == 0)
        {
            _probe = Native.StartShapeTestCapsule(
                _probeX, _probeY, _probeZ - CapsuleHeight,
                _probeX, _probeY, _probeZ + CapsuleHeight,
                CapsuleRadius,
                ProbeFlags,
                ped,
                ShapeTestOptions);
        }
    }

    private static void UpdateStowedWeapon(int ped)
    {
        if (!IsFirstPerson())
        {
            RestoreWeapon(ped);

            return;
        }

        if (_storedWeapon != 0)
        {
            return;
        }

        var weapon = Native.GetSelectedPedWeapon(ped);

        if (weapon == API.Hash(Unarmed))
        {
            return;
        }

        _storedWeapon = weapon;

        Native.SetCurrentPedWeapon(ped, API.Hash(Unarmed), true);
    }

    private static void RestoreWeapon(int ped)
    {
        if (_storedWeapon == 0)
        {
            return;
        }

        if (Native.HasPedGotWeapon(ped, _storedWeapon, 0))
        {
            Native.SetCurrentPedWeapon(ped, _storedWeapon, true);
        }

        _storedWeapon = 0;
    }

    private static void HoldHelmetOn(int ped)
    {
        if (Native.GetPedConfigFlag(ped, DontTakeOffHelmet, false))
        {
            return;
        }

        Native.SetPedConfigFlag(ped, DontTakeOffHelmet, true);

        _heldHelmetOn = true;
    }

    private static bool IsTwoHanded(uint weapon)
    {
        var group = Native.GetWeapontypeGroup(weapon);

        return group == API.Hash("GROUP_RIFLE")
            || group == API.Hash("GROUP_SHOTGUN")
            || group == API.Hash("GROUP_SMG")
            || group == API.Hash("GROUP_SNIPER")
            || group == API.Hash("GROUP_HEAVY")
            || group == API.Hash("GROUP_MG");
    }

    private static void DrawDebug()
    {
        if (!_debug)
        {
            return;
        }

        var green = _blocked ? 0 : ClearGreen;
        var blue = _blocked ? 0 : ClearBlue;

        Native.DrawMarkerSphere(_probeX, _probeY, _probeZ, CapsuleRadius, MarkerRed, green, blue, SphereAlpha);

        Native.DrawLine(
            _probeX, _probeY, _probeZ - CapsuleHeight,
            _probeX, _probeY, _probeZ + CapsuleHeight,
            255, 0, 0, 255);

        Hud.DrawText($"Pointing: {_state}, blocked: {_blocked}", 0f, 0f);
    }
}
