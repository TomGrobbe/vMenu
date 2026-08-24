namespace vMenu.Enhanced.NoClip;

// Game controls noclip has to suppress while it is active, see
// https://docs.fivem.net/docs/game-references/controls/. Noclip's own keys are FiveM key mappings,
// see NoClipKeyBindings; these are the game's reactions to the same keys.
internal static class Controls
{
    // Standard control group, used for every control below.
    internal const int Group = 0;

    internal const int MoveDown = 20;          // Control.MultiplayerInfo (Z)
    internal const int IncreaseSpeed = 21;     // Control.Sprint (Left Shift)
    internal const int MoveLeftRight = 30;
    internal const int MoveUpDown = 31;
    internal const int MoveForward = 32;       // Control.MoveUp (W)
    internal const int MoveBackward = 33;      // Control.MoveDownOnly (S)
    internal const int TurnLeft = 34;          // Control.MoveLeftOnly (A)
    internal const int TurnRight = 35;         // Control.MoveRightOnly (D)
    internal const int DecreaseSpeed = 36;     // Control.Duck (Left Ctrl)
    internal const int MoveUp = 44;            // Control.Cover (Q)
    internal const int ToggleFollowCam = 74;   // Control.VehicleHeadlight (H)
    internal const int VehicleRadioWheel = 81; // Move up
    internal const int RadioWheel = 85;        // Move up
    internal const int MultiplayerInfo = 244;  // Move Down

    // Disabled by the game whenever the F8 console has focus, so it doubles as a console check.
    internal const int ConsoleProbe = 360;
}
