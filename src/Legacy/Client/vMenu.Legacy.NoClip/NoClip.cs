using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Ticks;
using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.NoClip;

public static class NoClip
{
    private static readonly int[] DisabledControls =
    [
        Controls.RadioWheel,
        Controls.MoveLeftRight,
        Controls.MoveUpDown,
        Controls.MoveForward,
        Controls.MoveBackward,
        Controls.TurnLeft,
        Controls.TurnRight,
        Controls.DecreaseSpeed,
        Controls.MoveUp,
        Controls.MultiplayerInfo,
        Controls.ToggleFollowCam,
    ];

    private static readonly (float Multiplier, string Label)[] MoveSpeeds =
    [
        (0.1f, "1"),
        (0.25f, "2"),
        (0.75f, "3"),
        (1.15f, "4"),
        (2.5f, "5"),
        (5.5f, "6"),
        (8.5f, "7"),
        (12.5f, "8"),
        (18.5f, "9"),
        (25.5f, "10"),
    ];
    private const float ForwardStep = 0.5f;
    private const float VerticalStep = 0.21f;
    private const float TurnStep = 3f;

    private const int NoclipAlpha = 51;

    private const string InstructionalButtonsScaleform = "INSTRUCTIONAL_BUTTONS";
    private static bool NoclipActive { get; set; } = false;
    private static int MovingSpeed { get; set; } = 0;
    private static bool FollowCamMode { get; set; } = true;

    private static int _instructionalButtonsScaleformId = -1;

    private static bool _rebuildInstructionalButtons = true;

    public static event Action<int>? EntityReleased;

    public static bool IsActive => NoclipActive;

    private static bool _lentByStaff;

    private static bool _putThereByStaff;

    private static bool IsAllowed => _lentByStaff || ClientPermissions.IsAllowed(Global.NoClip);

    private static int _noclipEntity;
    private static int _noclipPed;

    private static int _cachedEntity;

    private static TickHandle? _move;
    private static TickHandle? _instructionalButtons;

    public static void Initialize()
    {
        Commands.Register("nc", false, ToggleRequested);

        NoClipKeyBindings.Register(
            isActive: () => NoclipActive,
            onToggle: ToggleRequested,
            onSpeedUp: () => AdjustSpeed(1),
            onSpeedDown: () => AdjustSpeed(-1),
            onFollowCam: () => Script.RunOnMainThread(() =>
            {
                if (API.IsPauseMenuActive())
                {
                    return;
                }

                FollowCamMode = !FollowCamMode;
                _rebuildInstructionalButtons = true;
            }));

        _move = TickRegistry.Register(
            "NoClip.Move",
            NoClipFrame,
            TickRate.PerFrame,
            condition: () => NoclipActive,
            onStarted: BeginNoclip,
            onStopped: EndNoclip);

        _instructionalButtons = TickRegistry.Register(
            "NoClip.InstructionalButtons",
            handler: InstructionalButtonsTick,
            TickRate.PerFrame,
            condition: () => NoclipActive);

        ClientPermissions.PermissionsChanged += OnPermissionsChanged;
    }

    #region Instructional buttons
    private static async Task InstructionalButtonsTick()
    {
        if (API.IsHudHidden())
        {
            return;
        }

        if (!API.HasScaleformMovieLoaded(_instructionalButtonsScaleformId))
        {
            await PrepareInstructionalButtons();
        }

        if (_rebuildInstructionalButtons)
        {
            BuildInstructionalButtons();
            _rebuildInstructionalButtons = false;
        }

        DisplayInstructionalButtons();
    }

    private static async Task PrepareInstructionalButtons()
    {
        if (_instructionalButtonsScaleformId != -1 && API.HasScaleformMovieLoaded(_instructionalButtonsScaleformId))
        {
            return;
        }

        _instructionalButtonsScaleformId = API.RequestScaleformMovie(InstructionalButtonsScaleform);

        while (!API.HasScaleformMovieLoaded(_instructionalButtonsScaleformId))
        {
            await BaseScript.Delay(0);
        }

        _rebuildInstructionalButtons = true;
    }

    private static void ReleaseInstructionalButtons()
    {
        if (_instructionalButtonsScaleformId == -1)
        {
            return;
        }

        NativeFixer.SetScaleformMovieAsNoLongerNeeded(ref _instructionalButtonsScaleformId);

        _instructionalButtonsScaleformId = -1;
        _rebuildInstructionalButtons = true;
    }

    private static readonly InstructionalButton[] InstructionalButtons =
    [
        new(() => $"Change Speed ({MoveSpeeds[MovingSpeed].Label}x)", static () => $"{NativeFixer.GetControlInstructionalButton(0, NoClipKeyBindings.SpeedDownControl, true)}%b_998%{NativeFixer.GetControlInstructionalButton(0, NoClipKeyBindings.SpeedUpControl, true)}"),
        new(() => "Up / Down", static () => $"{NativeFixer.GetControlInstructionalButton(0, NoClipKeyBindings.DownControl, true)}%b_998%{NativeFixer.GetControlInstructionalButton(0, NoClipKeyBindings.UpControl, true)}"),
        new(() => "Forward / Backward", static () => $"{NativeFixer.GetControlInstructionalButton(0, NoClipKeyBindings.BackwardControl, true)}%b_998%{NativeFixer.GetControlInstructionalButton(0, NoClipKeyBindings.ForwardControl, true)}"),
        new(() => FollowCamMode ? "Follow Cam: On" : "Follow Cam: Off", static () => NativeFixer.GetControlInstructionalButton(0, NoClipKeyBindings.FollowCamControl, true))
    ];

    // Only shown when the entity is not following the camera.
    private static readonly InstructionalButton[] TurnButtons = [
        new (() => "Turn Left / Right", static () => $"{NativeFixer.GetControlInstructionalButton(0, NoClipKeyBindings.TurnLeftControl, true)}%b_998%{NativeFixer.GetControlInstructionalButton(0, NoClipKeyBindings.TurnRightControl, true)}"),
    ];

    private static void DisplayInstructionalButtons()
    {
        if (_instructionalButtonsScaleformId == -1)
        {
            return;
        }

        API.DrawScaleformMovieFullscreen(_instructionalButtonsScaleformId, 255, 255, 255, 255, 0);
    }

    private static void BuildInstructionalButtons()
    {
        API.CallScaleformMovieMethod(_instructionalButtonsScaleformId, "CLEAR_ALL");

        API.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "TOGGLE_MOUSE_BUTTONS");
        API.PushScaleformMovieFunctionParameterInt(0);
        API.EndScaleformMovieMethod();

        var i = 0;
        foreach (var item in InstructionalButtons)
        {
            SetDataSlot(i, item);
            i++;
        }

        if (!FollowCamMode)
        {
            foreach (var item in TurnButtons)
            {
                SetDataSlot(i, item);
                i++;
            }
        }

        API.CallScaleformMovieMethod(_instructionalButtonsScaleformId, "DRAW_INSTRUCTIONAL_BUTTONS");
    }

    private static void SetDataSlot(int slot, InstructionalButton button)
    {
        API.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "SET_DATA_SLOT");
        API.ScaleformMovieMethodAddParamInt(slot);
        API.ScaleformMovieMethodAddParamTextureNameString(button.ButtonGetter());
        API.ScaleformMovieMethodAddParamTextureNameString(button.TextGetter());
        API.EndScaleformMovieMethod();
    }
    #endregion

    public static void SetLentByStaff(bool lent)
    {
        if (lent == _lentByStaff)
        {
            return;
        }

        _lentByStaff = lent;

        if (lent || IsAllowed)
        {
            return;
        }

        Script.RunOnMainThread(() => SetNoclipActive(false));
    }

    public static void SetActiveByStaff(bool active) =>
        Script.RunOnMainThread(() => SetNoclipActive(active, byStaff: true));

    public static void Disable() => SetNoclipActive(false);

    public static bool Enable()
    {
        if (!IsAllowed || SceneLock.IsActive)
        {
            return false;
        }

        Script.RunOnMainThread(() => SetNoclipActive(true));

        return true;
    }

    // Requesting a scaleform inside a thread run by a command or keybind handler fails, and one is
    // requested somewhere down the line of SetNoclipActive(true), so this goes through the main thread.
    private static void ToggleRequested()
    {
        Script.RunOnMainThread(() =>
        {
            if (!IsAllowed && !NoclipActive)
            {
                Notifications.Warning(MenuText.Key(Loc.NoClip.ToggleDenied));

                return;
            }

            if (!API.IsPauseMenuActive())
            {
                SetNoclipActive(!NoclipActive);
            }
        });
    }

    private static void AdjustSpeed(int steps)
    {
        Script.RunOnMainThread(() =>
        {
            if (API.IsPauseMenuActive())
            {
                return;
            }

            MovingSpeed = (MovingSpeed + steps + MoveSpeeds.Length) % MoveSpeeds.Length;
            _rebuildInstructionalButtons = true;
        });
    }

    private static void OnPermissionsChanged()
    {
        if (!IsAllowed && !_putThereByStaff)
        {
            SetNoclipActive(false);
        }
    }

    internal static void SetNoclipActive(bool active, bool byStaff = false)
    {
        // The key tick follows the permission, but a revoke can land between the two.
        if (active && ((!byStaff && !IsAllowed) || SceneLock.IsActive))
        {
            return;
        }

        if (active == NoclipActive)
        {
            return;
        }

        NoclipActive = active;
        _putThereByStaff = active && byStaff;

        BaseScript.TriggerServerEvent(PlayerStateEvents.ReportNoClip, active);

        _move?.Reevaluate();
        _instructionalButtons?.Reevaluate();
    }

    private static void BeginNoclip()
    {
        _noclipPed = API.PlayerPedId();
        _noclipEntity = GetNoclipEntity(_noclipPed, out _);
        _cachedEntity = _noclipEntity;
    }

    private static void EndNoclip()
    {
        // A switch on the last frame leaves a hand-back owing that the next frame would have done, and there
        // is no next frame now.
        if (_cachedEntity != _noclipEntity)
        {
            ReleaseEntity(_cachedEntity);
        }

        ResetEntity(_noclipPed, _noclipEntity);
        ReleaseInstructionalButtons();
    }

    private static async Task NoClipFrame()
    {
        // Deliberately a frame behind the switch that set it: handing the old entity back on the same frame
        // lets the game overwrite the reset, and it stays invisible.
        if (_noclipEntity != _cachedEntity)
        {
            ReleaseEntity(_cachedEntity);
            _cachedEntity = _noclipEntity;
        }

        _noclipPed = API.PlayerPedId();
        _noclipEntity = GetNoclipEntity(_noclipPed, out var inVehicle);
        FreezeEntity(_noclipEntity);
        DisableConflictingControls(inVehicle);

        MoveEntity(_noclipEntity, ReadMoveInput());
        ConcealEntity(_noclipPed, _noclipEntity);
    }

    private static int GetNoclipEntity(int playerPed, out bool inVehicle)
    {
        inVehicle = API.IsPedInAnyVehicle(playerPed, false);
        return inVehicle ? API.GetVehiclePedIsIn(playerPed, false) : playerPed;
    }

    private static void ReleaseEntity(int entity)
    {
        if (API.IsEntityAVehicle(entity))
        {
            ResetEntity(entity, entity);
        }
    }

    private static void DisableConflictingControls(bool inVehicle)
    {
        foreach (var control in DisabledControls)
        {
            API.DisableControlAction(Controls.Group, control, false);
        }
        if (inVehicle)
        {
            API.DisableControlAction(Controls.Group, Controls.VehicleRadioWheel, false);
        }
    }

    private static MoveInput ReadMoveInput()
    {
        if (API.IsPauseMenuActive())
        {
            return default;
        }

        var forward = 0.0f;
        var vertical = 0.0f;
        var headingDelta = 0.0f;

        if (NoClipKeyBindings.ForwardHeld)
        {
            forward = ForwardStep;
        }
        if (NoClipKeyBindings.BackwardHeld)
        {
            forward = -ForwardStep;
        }

        // Turning is only available when the entity is not already following the camera.
        if (!FollowCamMode)
        {
            if (NoClipKeyBindings.TurnLeftHeld)
            {
                headingDelta += TurnStep;
            }
            if (NoClipKeyBindings.TurnRightHeld)
            {
                headingDelta -= TurnStep;
            }
        }

        if (NoClipKeyBindings.UpHeld)
        {
            vertical = VerticalStep;
        }
        if (NoClipKeyBindings.DownHeld)
        {
            vertical = -VerticalStep;
        }

        return new MoveInput(forward, vertical, headingDelta);
    }

    private static void FreezeEntity(int noclipEntity)
    {
        API.FreezeEntityPosition(noclipEntity, true);
        NativeFixer.SetEntityInvincible(noclipEntity, true, false);
    }

    private static void MoveEntity(int noclipEntity, MoveInput input)
    {
        var moveSpeed = MoveSpeeds[MovingSpeed].Multiplier;
        var newPos = API.GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, input.Forward * moveSpeed, input.Vertical * moveSpeed);

        var heading = FollowCamMode ? 0f : API.GetEntityHeading(noclipEntity) + input.HeadingDelta;
        API.SetEntityVelocity(noclipEntity, 0f, 0f, 0f);
        API.SetEntityRotation(noclipEntity, 0f, 0f, 0f, 0, false);

        API.SetEntityHeading(noclipEntity, FollowCamMode ? API.GetGameplayCamRelativeHeading() : heading);
        API.SetEntityCollision(noclipEntity, false, false);
        API.SetEntityCoordsNoOffset(noclipEntity, newPos.X, newPos.Y, newPos.Z, true, true, true);
    }

    private static void ConcealEntity(int playerPed, int noclipEntity)
    {
        API.SetEntityVisible(noclipEntity, false, false);
        API.SetLocalPlayerVisibleLocally(true);
        NativeFixer.SetEntityAlpha(noclipEntity, NoclipAlpha, false);
        API.SetEveryoneIgnorePlayer(playerPed, true);
        API.SetPoliceIgnorePlayer(playerPed, true);
    }

    private static void ResetEntity(int playerPed, int noclipEntity)
    {
        API.FreezeEntityPosition(noclipEntity, false);
        NativeFixer.SetEntityInvincible(noclipEntity, false, false);
        API.SetEntityCollision(noclipEntity, true, true);
        API.SetEntityVisible(playerPed, true, false);
        API.SetLocalPlayerVisibleLocally(true);

        if (noclipEntity != playerPed)
        {
            API.SetEntityVisible(noclipEntity, true, false);
        }

        API.ActivatePhysics(noclipEntity);

        NativeFixer.ResetEntityAlpha(noclipEntity);

        if (API.IsEntityAPed(playerPed))
        {
            API.SetEveryoneIgnorePlayer(playerPed, false);
            API.SetPoliceIgnorePlayer(playerPed, false);
        }

        EntityReleased?.Invoke(noclipEntity);
    }
}
