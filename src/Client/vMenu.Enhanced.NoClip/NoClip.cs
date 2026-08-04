using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.NoClip;

public static class NoClip
{
    /// <summary>
    /// Controls that have to be disabled every frame while noclip is active, otherwise the ped
    /// or vehicle would still act on them.
    /// </summary>
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
        Controls.IncreaseSpeed,
        Controls.MoveUp,
        Controls.MoveDown,
        Controls.MultiplayerInfo,
        Controls.ToggleFollowCam,
    ];

    /// <summary>Selectable movement speeds, paired with the label shown on the instructional buttons.</summary>
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

    /// <summary>20% opacity, so the entity stays faintly visible to the noclipping player.</summary>
    private const int NoclipAlpha = 51;

    private const string InstructionalButtonsScaleform = "INSTRUCTIONAL_BUTTONS";

    private static bool NoclipActive { get; set; } = false;
    private static int MovingSpeed { get; set; } = 0;
    private static bool FollowCamMode { get; set; } = true;

    private static int _instructionalButtonsScaleformId = -1;

    // State the instructional buttons were last built for. The scaleform keeps its data slots
    // between frames, so the buttons only have to be rebuilt when one of these changes.
    private static int _renderedSpeed = -1;
    private static bool _renderedFollowCamMode;

    private static bool IsF8ConsoleLikelyOpen => !Native.IsControlEnabled(Controls.Group, Controls.ConsoleProbe);

    private static bool IsAllowed => ClientPermissions.IsAllowed(MiscSettings.NoClip);

    /// <summary>The entity being moved, and the player ped it was read from. Both survive the frame.</summary>
    private static int _noclipEntity;

    private static int _noclipPed;

    /// <summary>The entity <see cref="_noclipEntity"/> was on last frame, see <see cref="NoClipFrame"/>.</summary>
    private static int _cachedEntity;

    private static TickHandle? _move;

    public static void Initialize()
    {
        // The keys arrive as commands now, so there is no per frame tick reading them: the only
        // loop left is the one that moves the entity, and that runs solely while noclip is on.
        NoClipKeyBindings.Register(
            onToggle: ToggleNoclip,
            onSpeedUp: () => StepSpeed(1),
            onSpeedDown: () => StepSpeed(-1),
            onFollowCam: ToggleFollowCam);

        _move = TickRegistry.Register(
            "NoClip.Move",
            NoClipFrame,
            TickRate.PerFrame,
            condition: () => NoclipActive,
            onStarted: BeginNoclip,
            onStopped: EndNoclip);

        ClientPermissions.PermissionsChanged += OnPermissionsChanged;
    }

    /// <summary>
    /// Routes a revoke through the normal teardown instead of leaving the flag set, which would drop
    /// the player straight back into noclip the moment the permission came back.
    /// </summary>
    private static void OnPermissionsChanged()
    {
        if (!IsAllowed)
        {
            SetNoclipActive(false);
        }
    }

    /// <summary>
    /// Whether a key binding should be acted on at all. Key mappings fire whatever the game is doing,
    /// so the checks the old per frame tick did have to happen here instead.
    /// </summary>
    private static bool CanUseKeys =>
        !IsF8ConsoleLikelyOpen && Native.UpdateOnscreenKeyboard() != 0 && !Native.IsPauseMenuActive();

    /// <summary>As <see cref="CanUseKeys"/>, for the keys that only mean anything while noclip is on.</summary>
    private static bool CanSteer => NoclipActive && CanUseKeys;

    private static void ToggleNoclip()
    {
        if (!CanUseKeys)
        {
            return;
        }

        SetNoclipActive(!NoclipActive);
    }

    /// <summary>Steps <see cref="MovingSpeed"/> through <see cref="MoveSpeeds"/>, wrapping around.</summary>
    private static void StepSpeed(int direction)
    {
        if (!CanSteer)
        {
            return;
        }

        MovingSpeed = ((MovingSpeed + direction) + MoveSpeeds.Length) % MoveSpeeds.Length;
    }

    private static void ToggleFollowCam()
    {
        if (!CanSteer)
        {
            return;
        }

        FollowCamMode = !FollowCamMode;
    }

    internal static void SetNoclipActive(bool active)
    {
        // The toggle key fires regardless of permission, so this is the only thing gating it.
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
    }

    internal static bool IsNoclipActive()
    {
        return NoclipActive;
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

        // A direction still held when noclip switches off would otherwise be waiting for its release
        // that never gets read, and the entity would drift the moment noclip came back on.
        NoClipKeyBindings.ClearHeld();
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

        await PrepareInstructionalButtons();

        if (!Native.IsHudHidden())
        {
            DisplayInstructionalButtons();
        }

        _noclipPed = Native.PlayerPedId();
        _noclipEntity = GetNoclipEntity(_noclipPed, out var inVehicle);

        FreezeEntity(_noclipEntity);
        DisableConflictingControls(inVehicle);

        var input = ReadMoveInput();

        MoveEntity(_noclipEntity, input);
        ConcealEntity(_noclipPed, _noclipEntity);
    }

    /// <summary>
    /// The entity noclip moves around: the vehicle the player is in, or the player ped itself.
    /// </summary>
    private static int GetNoclipEntity(int playerPed, out bool inVehicle)
    {
        inVehicle = Native.IsPedInAnyVehicle(playerPed, false);

        return inVehicle ? Native.GetVehiclePedIsIn(playerPed, false) : playerPed;
    }

    /// <summary>
    /// Hands an entity noclip is no longer moving back to the game, which happens whenever the
    /// player leaves a vehicle or switches to another one. Has to be called a tick after the
    /// switch, see the call site.
    /// </summary>
    private static void ReleaseEntity(int entity)
    {
        if (Native.IsEntityAVehicle(entity))
        {
            ResetEntity(entity, entity);
        }
    }

    /// <summary>
    /// Disables the controls the ped or vehicle would otherwise still act on this frame.
    /// </summary>
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

    /// <summary>
    /// Reads the held movement bindings. Returns <c>default</c> while the player can't steer, e.g.
    /// in the pause menu or with the on-screen keyboard open.
    /// </summary>
    private static MoveInput ReadMoveInput()
    {
        if (!CanSteer)
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

    /// <summary>Pins the entity in place, so the game stops simulating it.</summary>
    private static void FreezeEntity(int noclipEntity)
    {
        Native.FreezeEntityPosition(noclipEntity, true);
        Native.SetEntityInvincible(noclipEntity, true, false);
    }

    /// <summary>Teleports the entity to where the player is steering it.</summary>
    private static void MoveEntity(int noclipEntity, MoveInput input)
    {
        var moveSpeed = MoveSpeeds[MovingSpeed].Multiplier;
        var newPos = Native.GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, input.Forward * moveSpeed, input.Vertical * moveSpeed);

        // Has to be read before the rotation below zeroes it out. Unused in follow cam mode.
        var heading = FollowCamMode ? 0f : Native.GetEntityHeading(noclipEntity) + input.HeadingDelta;

        Native.SetEntityVelocity(noclipEntity, 0f, 0f, 0f);
        Native.SetEntityRotation(noclipEntity, 0f, 0f, 0f, 0, false);

        // The gameplay cam heading is relative to the entity, so it has to be read *after* the
        // rotation reset above. Reading it while the entity still has last frame's heading makes
        // it compound every frame.
        Native.SetEntityHeading(noclipEntity, FollowCamMode ? Native.GetGameplayCamRelativeHeading() : heading);
        Native.SetEntityCollision(noclipEntity, false, false);
        Native.SetEntityCoordsNoOffset(noclipEntity, newPos.X, newPos.Y, newPos.Z, true, true, true);
    }

    /// <summary>Hides the entity from other players, and from anyone who would react to it.</summary>
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

        // Always reset the alpha.
        Native.ResetEntityAlpha(noclipEntity);

        if (Native.IsEntityAPed(playerPed))
        {
            Native.SetEveryoneIgnorePlayer(playerPed, false);
            Native.SetPoliceIgnorePlayer(playerPed, false);
        }
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

        // A freshly loaded movie has no data slots yet, so force a rebuild.
        _renderedSpeed = -1;
    }

    private static void ReleaseInstructionalButtons()
    {
        if (_instructionalButtonsScaleformId == -1)
        {
            return;
        }

        Native.SetScaleformMovieAsNoLongerNeeded(out _instructionalButtonsScaleformId);

        _instructionalButtonsScaleformId = -1;
        _renderedSpeed = -1;
    }

    private static readonly InstructionalButton[] InstructionalButtons =
    [
        new(() => $"Speed: {MoveSpeeds[MovingSpeed].Label}x", static () => string.Empty),
        new(() => "Increase speed", static () => NoClipKeyBindings.Button(NoClipKeyBindings.SpeedUpControl)),
        new(() => "Decrease speed", static () => NoClipKeyBindings.Button(NoClipKeyBindings.SpeedDownControl)),
        new(() => "Move Up", static () => NoClipKeyBindings.Button(NoClipKeyBindings.UpControl)),
        new(() => "Move Down", static () => NoClipKeyBindings.Button(NoClipKeyBindings.DownControl)),
        new(() => "Move Backward / Forward",
            static () => NoClipKeyBindings.Button(NoClipKeyBindings.BackwardControl) + NoClipKeyBindings.Button(NoClipKeyBindings.ForwardControl)),
        new(() => FollowCamMode ? "Follow Cam: On" : "Follow Cam: Off", static () => NoClipKeyBindings.Button(NoClipKeyBindings.FollowCamControl)),
    ];

    /// <summary>Only shown when the entity isn't following the camera, see <see cref="FollowCamMode"/>.</summary>
    private static readonly InstructionalButton TurnButton = new(
        () => "Turn Right / Left",
        static () => NoClipKeyBindings.Button(NoClipKeyBindings.TurnRightControl) + NoClipKeyBindings.Button(NoClipKeyBindings.TurnLeftControl));

    private static void DisplayInstructionalButtons()
    {
        if (_instructionalButtonsScaleformId == -1)
        {
            return;
        }

        // The scaleform keeps its data slots between frames, so the buttons only have to be
        // rebuilt whenever their contents actually change. Every other frame just draws it.
        if (_renderedSpeed != MovingSpeed || _renderedFollowCamMode != FollowCamMode)
        {
            BuildInstructionalButtons();

            _renderedSpeed = MovingSpeed;
            _renderedFollowCamMode = FollowCamMode;
        }

        Native.DrawScaleformMovieFullscreen(_instructionalButtonsScaleformId, 255, 255, 255, 255, 0);
    }

    private static void BuildInstructionalButtons()
    {
        Native.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "CLEAR_ALL");
        Native.EndScaleformMovieMethod();

        for (var i = 0; i < InstructionalButtons.Length; i++)
        {
            SetDataSlot(i, InstructionalButtons[i]);
        }

        if (!FollowCamMode)
        {
            SetDataSlot(InstructionalButtons.Length, TurnButton);
        }

        Native.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "DRAW_INSTRUCTIONAL_BUTTONS");
        Native.ScaleformMovieMethodAddParamInt(0);
        Native.EndScaleformMovieMethod();
    }

    private static void SetDataSlot(int slot, InstructionalButton button)
    {
        Native.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "SET_DATA_SLOT");
        Native.ScaleformMovieMethodAddParamInt(slot);
        Native.ScaleformMovieMethodAddParamTextureNameString(button.ButtonGetter());
        Native.ScaleformMovieMethodAddParamTextureNameString(button.TextGetter());
        Native.EndScaleformMovieMethod();
    }
}
