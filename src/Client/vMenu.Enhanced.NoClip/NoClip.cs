using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.NoClip;

public class NoClip
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
        Controls.MoveUp,
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

    public NoClip()
    {
        NoClipper();
        NoClipperKeyer();
    }

    private static async void NoClipper()
    {
        while (true)
        {
            await NoClipHandler();
            await API.Yield();
        }
    }

    private static async void NoClipperKeyer()
    {
        while (true)
        {
            NoClipControls();
            await API.Yield();
        }
    }

    private static void NoClipControls()
    {
        if (IsF8ConsoleLikelyOpen || !Native.IsUsingKeyboardAndMouse(0))
        {
            return;
        }

        if (Native.IsControlJustPressed(Controls.Group, Controls.ToggleNoclip)
            || Native.IsDisabledControlJustPressed(Controls.Group, Controls.ToggleNoclip))
        {
            SetNoclipActive(!NoclipActive);
        }
    }

    internal static void SetNoclipActive(bool active)
    {
        if (active == NoclipActive)
        {
            return;
        }

        NoclipActive = active;

        if (!active)
        {
            ReleaseInstructionalButtons();
        }
    }

    internal static bool IsNoclipActive()
    {
        return NoclipActive;
    }

    private static async Task NoClipHandler()
    {
        if (!NoclipActive)
        {
            return;
        }

        await PrepareInstructionalButtons();

        var playerPed = Native.PlayerPedId();
        var noclipEntity = GetNoclipEntity(playerPed, out _);
        var cachedEntity = noclipEntity;
        var noclipApplied = false;

        while (NoclipActive)
        {
            noclipApplied = true;

            if (!Native.IsHudHidden())
            {
                DisplayInstructionalButtons();
            }

            playerPed = Native.PlayerPedId();
            noclipEntity = GetNoclipEntity(playerPed, out var inVehicle);

            FreezeEntity(noclipEntity);
            DisableConflictingControls(inVehicle);

            var input = ReadMoveInput();

            MoveEntity(noclipEntity, input);
            ConcealEntity(playerPed, noclipEntity);

            await API.Yield();

            // Deliberately after the yield: the old entity has to be handed back a tick after the
            // switch is noticed, otherwise the game overwrites the reset and it stays invisible.
            if (noclipEntity != cachedEntity)
            {
                ReleaseEntity(cachedEntity);

                cachedEntity = noclipEntity;
            }
        }

        if (noclipApplied)
        {
            ResetEntity(playerPed, noclipEntity);
        }
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
    /// Reads the movement controls, and applies the speed and follow cam toggles. Returns
    /// <c>default</c> while the player can't steer, e.g. on a gamepad, in the pause menu, or with
    /// the on-screen keyboard open.
    /// </summary>
    private static MoveInput ReadMoveInput()
    {
        if (IsF8ConsoleLikelyOpen || !Native.IsUsingKeyboardAndMouse(0) || Native.UpdateOnscreenKeyboard() == 0 || Native.IsPauseMenuActive())
        {
            return default;
        }

        UpdateMovingSpeed();

        var forward = 0.0f;
        var vertical = 0.0f;
        var headingDelta = 0.0f;

        if (Native.IsDisabledControlPressed(Controls.Group, Controls.MoveForward))
        {
            forward = ForwardStep;
        }
        if (Native.IsDisabledControlPressed(Controls.Group, Controls.MoveBackward))
        {
            forward = -ForwardStep;
        }

        // Turning is only available when the entity isn't already following the camera.
        if (!FollowCamMode)
        {
            if (Native.IsDisabledControlPressed(Controls.Group, Controls.TurnLeft))
            {
                headingDelta += TurnStep;
            }
            if (Native.IsDisabledControlPressed(Controls.Group, Controls.TurnRight))
            {
                headingDelta -= TurnStep;
            }
        }

        if (Native.IsDisabledControlPressed(Controls.Group, Controls.MoveUp))
        {
            vertical = VerticalStep;
        }
        if (Native.IsDisabledControlPressed(Controls.Group, Controls.MoveDown))
        {
            vertical = -VerticalStep;
        }

        if (Native.IsDisabledControlJustPressed(Controls.Group, Controls.ToggleFollowCam))
        {
            FollowCamMode = !FollowCamMode;
        }

        return new MoveInput(forward, vertical, headingDelta);
    }

    /// <summary>Steps <see cref="MovingSpeed"/> through <see cref="MoveSpeeds"/>, wrapping around.</summary>
    private static void UpdateMovingSpeed()
    {
        if (Native.IsControlJustPressed(Controls.Group, Controls.IncreaseSpeed))
        {
            MovingSpeed = (MovingSpeed + 1) % MoveSpeeds.Length;
        }
        if (Native.IsDisabledControlJustPressed(Controls.Group, Controls.DecreaseSpeed))
        {
            MovingSpeed = ((MovingSpeed - 1) + MoveSpeeds.Length) % MoveSpeeds.Length;
        }
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
        new(() => $"Speed: {MoveSpeeds[MovingSpeed].Label}x", string.Empty),
        new(() => "Increase speed", "INPUT_SPRINT"),
        new(() => "Decrease speed", "INPUT_DUCK"),
        new(() => "Move Up", "INPUT_COVER"),
        new(() => "Move Down", "INPUT_MULTIPLAYER_INFO"),
        new(() => "Move Backward / Forward", "INPUT_MOVE_UD"),
        new(() => FollowCamMode ? "Follow Cam: On" : "Follow Cam: Off", "INPUT_VEH_HEADLIGHT"),
    ];

    /// <summary>Only shown when the entity isn't following the camera, see <see cref="FollowCamMode"/>.</summary>
    private static readonly InstructionalButton TurnButton = new(() => "Turn Right / Left", "INPUT_MOVE_LR");

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
        Native.ScaleformMovieMethodAddParamTextureNameString(button.ControlName);
        Native.ScaleformMovieMethodAddParamTextureNameString(button.TextGetter());
        Native.EndScaleformMovieMethod();
    }
}
