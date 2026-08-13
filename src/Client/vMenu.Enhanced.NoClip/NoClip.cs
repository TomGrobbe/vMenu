using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Ticks;
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
        (0.5f, "2"),
        (1.0f, "3"),
        (1.5f, "4"),
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

    private static bool IsAllowed => ClientPermissions.IsAllowed(MiscSettings.NoClip);

    private static int _noclipEntity;
    private static int _noclipPed;

    private static int _cachedEntity;

    private static TickHandle? _move;
    private static TickHandle? _instructionalButtons;

    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand("nc", false, ToggleRequested);

        NoClipKeyBindings.Register(
            isActive: () => NoclipActive,
            onToggle: new Action(() =>
            {
                // We need to run this on the main thread, if we try to request a scaleform
                // (which happens down the line of SetNoclipActive(true) somewhere)
                // inside a thread executed by a command handler/keybind handler, it will fail
                SharedAPI.RunOnMainThread(ToggleRequested);
            }),
            onSpeedUp: () => AdjustSpeed(1),
            onSpeedDown: () => AdjustSpeed(-1),
            onFollowCam: () =>
            {
                if (Native.IsPauseMenuActive())
                {
                    return;
                }

                FollowCamMode = !FollowCamMode;
                _rebuildInstructionalButtons = true;
            });

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
        if (Native.IsHudHidden())
        {
            return;
        }

        if (!Native.HasScaleformMovieLoaded(_instructionalButtonsScaleformId))
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
        if (_instructionalButtonsScaleformId != -1 && Native.HasScaleformMovieLoaded(_instructionalButtonsScaleformId))
        {
            return;
        }

        _instructionalButtonsScaleformId = Native.RequestScaleformMovie(InstructionalButtonsScaleform);

        while (!Native.HasScaleformMovieLoaded(_instructionalButtonsScaleformId))
        {
            await API.Delay(0);
        }

        _rebuildInstructionalButtons = true;
    }

    private static void ReleaseInstructionalButtons()
    {
        if (_instructionalButtonsScaleformId == -1)
        {
            return;
        }

        Native.SetScaleformMovieAsNoLongerNeeded(new CitizenFX.FiveM.Shared.Data.Ref<int>(ref _instructionalButtonsScaleformId));

        _instructionalButtonsScaleformId = -1;
        _rebuildInstructionalButtons = true;
    }

    private static readonly InstructionalButton[] InstructionalButtons =
    [
        new(() => $"Change Speed ({MoveSpeeds[MovingSpeed].Label}x)", static () => $"{Native.GetControlInstructionalButton(0, NoClipKeyBindings.SpeedDownControl, true)}%b_998%{Native.GetControlInstructionalButton(0, NoClipKeyBindings.SpeedUpControl, true)}"),
        new(() => "Up / Down", static () => $"{Native.GetControlInstructionalButton(0, NoClipKeyBindings.DownControl, true)}%b_998%{Native.GetControlInstructionalButton(0, NoClipKeyBindings.UpControl, true)}"),
        new(() => "Forward / Backward", static () => $"{Native.GetControlInstructionalButton(0, NoClipKeyBindings.BackwardControl, true)}%b_998%{Native.GetControlInstructionalButton(0, NoClipKeyBindings.ForwardControl, true)}"),
        new(() => FollowCamMode ? "Follow Cam: On" : "Follow Cam: Off", static () => Native.GetControlInstructionalButton(0, NoClipKeyBindings.FollowCamControl, true))
    ];

    /// <summary>Only shown when the entity isn't following the camera, see <see cref="FollowCamMode"/>.</summary>
    private static readonly InstructionalButton[] TurnButtons = [
        new (() => "Turn Left / Right", static () => $"{Native.GetControlInstructionalButton(0, NoClipKeyBindings.TurnLeftControl, true)}%b_998%{Native.GetControlInstructionalButton(0, NoClipKeyBindings.TurnRightControl, true)}"),
    ];

    private static void DisplayInstructionalButtons()
    {
        if (_instructionalButtonsScaleformId == -1)
        {
            return;
        }

        Native.DrawScaleformMovieFullscreen(_instructionalButtonsScaleformId, 255, 255, 255, 255, 0);
    }

    private static void BuildInstructionalButtons()
    {
        Native.CallScaleformMovieMethod(_instructionalButtonsScaleformId, "CLEAR_ALL");

        Native.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "TOGGLE_MOUSE_BUTTONS");
        Native.PushScaleformMovieFunctionParameterInt(0);
        Native.EndScaleformMovieMethod();

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

        Native.CallScaleformMovieMethod(_instructionalButtonsScaleformId, "DRAW_INSTRUCTIONAL_BUTTONS");
    }

    private static void SetDataSlot(int slot, InstructionalButton button)
    {
        Native.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "SET_DATA_SLOT");
        Native.ScaleformMovieMethodAddParamInt(slot);
        Native.ScaleformMovieMethodAddParamTextureNameString(button.ButtonGetter());
        Native.ScaleformMovieMethodAddParamTextureNameString(button.TextGetter());
        Native.EndScaleformMovieMethod();
    }
    #endregion

    /// <summary>Shared by the /nc command and the toggle key, so both answer a refusal the same way.</summary>
    private static void ToggleRequested()
    {
        if (!IsAllowed)
        {
            Notifications.Warning(MenuText.Key(Loc.NoClip.ToggleDenied));

            return;
        }

        if (!Native.IsPauseMenuActive())
        {
            SetNoclipActive(!NoclipActive);
        }
    }

    private static void AdjustSpeed(int steps)
    {
        if (Native.IsPauseMenuActive())
        {
            return;
        }

        MovingSpeed = (MovingSpeed + steps + MoveSpeeds.Length) % MoveSpeeds.Length;
        _rebuildInstructionalButtons = true;
    }

    private static void OnPermissionsChanged()
    {
        if (!IsAllowed)
        {
            SetNoclipActive(false);
        }
    }


    internal static void SetNoclipActive(bool active)
    {
        // The key tick follows the permission, but a revoke can land between the two.
        if (active && !IsAllowed)
        {
            return;
        }

        if (active == NoclipActive)
        {
            return;
        }

        NoclipActive = active;

        _move?.Reevaluate();
        _instructionalButtons?.Reevaluate();
    }

    private static void BeginNoclip()
    {
        _noclipPed = Native.PlayerPedId();
        _noclipEntity = GetNoclipEntity(_noclipPed, out _);
        _cachedEntity = _noclipEntity;
    }

    private static void EndNoclip()
    {
        // A switch on the last frame leaves a hand-back owing that the next frame would have done,
        // and there is no next frame now.
        if (_cachedEntity != _noclipEntity)
        {
            ReleaseEntity(_cachedEntity);
        }

        ResetEntity(_noclipPed, _noclipEntity);
        ReleaseInstructionalButtons();
    }

    private static async Task NoClipFrame()
    {
        // Deliberately a frame behind the switch that set it: handing the old entity back on the
        // same frame lets the game overwrite the reset, and it stays invisible.
        if (_noclipEntity != _cachedEntity)
        {
            ReleaseEntity(_cachedEntity);
            _cachedEntity = _noclipEntity;
        }

        //await PrepareInstructionalButtons();

        _noclipPed = Native.PlayerPedId();
        _noclipEntity = GetNoclipEntity(_noclipPed, out var inVehicle);
        FreezeEntity(_noclipEntity);
        DisableConflictingControls(inVehicle);

        MoveEntity(_noclipEntity, ReadMoveInput());
        ConcealEntity(_noclipPed, _noclipEntity);
    }

    private static int GetNoclipEntity(int playerPed, out bool inVehicle)
    {
        inVehicle = Native.IsPedInAnyVehicle(playerPed, false);
        return inVehicle ? Native.GetVehiclePedIsIn(playerPed, false) : playerPed;
    }

    private static void ReleaseEntity(int entity)
    {
        if (Native.IsEntityAVehicle(entity))
        {
            ResetEntity(entity, entity);
        }
    }

    private static void DisableConflictingControls(bool inVehicle)
    {
        foreach (var control in DisabledControls)
        {
            Native.DisableControlAction(Controls.Group, control, false);
        }
        if (inVehicle)
        {
            Native.DisableControlAction(Controls.Group, Controls.VehicleRadioWheel, false);
        }
    }

    private static MoveInput ReadMoveInput()
    {
        if (Native.IsPauseMenuActive())
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

        // Turning is only available when the entity isn't already following the camera.
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
        Native.FreezeEntityPosition(noclipEntity, true);
        Native.SetEntityInvincible(noclipEntity, true, false);
    }

    private static void MoveEntity(int noclipEntity, MoveInput input)
    {
        var moveSpeed = MoveSpeeds[MovingSpeed].Multiplier;
        var newPos = Native.GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, input.Forward * moveSpeed, input.Vertical * moveSpeed);

        var heading = FollowCamMode ? 0f : Native.GetEntityHeading(noclipEntity) + input.HeadingDelta;
        Native.SetEntityVelocity(noclipEntity, 0f, 0f, 0f);
        Native.SetEntityRotation(noclipEntity, 0f, 0f, 0f, 0, false);

        Native.SetEntityHeading(noclipEntity, FollowCamMode ? Native.GetGameplayCamRelativeHeading() : heading);
        Native.SetEntityCollision(noclipEntity, false, false);
        Native.SetEntityCoordsNoOffset(noclipEntity, newPos.X, newPos.Y, newPos.Z, true, true, true);
    }

    private static void ConcealEntity(int playerPed, int noclipEntity)
    {
        Native.SetEntityVisible(noclipEntity, false, false);
        Native.SetLocalPlayerVisibleLocally(true);
        Native.SetEntityAlpha(noclipEntity, NoclipAlpha, false);
        Native.SetEveryoneIgnorePlayer(playerPed, true);
        Native.SetPoliceIgnorePlayer(playerPed, true);
    }

    private static void ResetEntity(int playerPed, int noclipEntity)
    {
        Native.FreezeEntityPosition(noclipEntity, false);
        Native.SetEntityInvincible(noclipEntity, false, false);
        Native.SetEntityCollision(noclipEntity, true, true);
        Native.SetEntityVisible(playerPed, true, false);
        Native.SetLocalPlayerVisibleLocally(true);

        if (noclipEntity != playerPed)
        {
            Native.SetEntityVisible(noclipEntity, true, false);
        }

        Native.ActivatePhysics(noclipEntity);

        Native.ResetEntityAlpha(noclipEntity);

        if (Native.IsEntityAPed(playerPed))
        {
            Native.SetEveryoneIgnorePlayer(playerPed, false);
            Native.SetPoliceIgnorePlayer(playerPed, false);
        }

        EntityReleased?.Invoke(noclipEntity);
    }
}