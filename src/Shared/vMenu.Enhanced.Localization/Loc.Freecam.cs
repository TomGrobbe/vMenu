namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class Freecam
    {
        [English("You are not allowed to use the freecam.")]
        public const string ToggleDenied = "freecam.toggle.denied";

        [English("Start the freecam first.")]
        public const string NotActive = "freecam.notactive";

        [English("Move")]
        public const string ButtonMove = "freecam.button.move";

        [English("Up / Down")]
        public const string ButtonHeight = "freecam.button.height";

        [English("Zoom")]
        public const string ButtonZoom = "freecam.button.zoom";

        [English("Roll")]
        public const string ButtonRoll = "freecam.button.roll";

        [English("Reset")]
        public const string ButtonReset = "freecam.button.reset";

        [English("Fast / Slow")]
        public const string ButtonFastSlow = "freecam.button.fastslow";

        [English("Speed ({speed} m/s)")]
        public const string ButtonSpeed = "freecam.button.speed";

        [English("Add Keyframe")]
        public const string ButtonAddKeyframe = "freecam.button.addkeyframe";

        [English("Play / Stop Path")]
        public const string ButtonPlay = "freecam.button.play";

        [English("Hide HUD")]
        public const string ButtonHideHud = "freecam.button.hidehud";

        [English("Hide Hints")]
        public const string ButtonHints = "freecam.button.hints";

        [English("Keyframe added, the path now has ~b~{count}~s~.")]
        public const string KeyframeAdded = "freecam.keyframe.added";

        [English("Last keyframe removed, the path now has ~b~{count}~s~.")]
        public const string KeyframeRemoved = "freecam.keyframe.removed";

        [English("This path is full.")]
        public const string PathFull = "freecam.path.full";

        [English("This path has no keyframes yet.")]
        public const string PathEmpty = "freecam.path.empty";

        [English("A path needs at least two keyframes to play.")]
        public const string TooFewKeyframes = "freecam.path.toofew";

        [English("This path follows a target. Lock onto one first.")]
        public const string NeedsTarget = "freecam.path.needstarget";

        [English(
            "This path was recorded with a different follow setting. Match the follow and attach mode it was made with, or start a new path.")]
        public const string SpaceMismatch = "freecam.path.spacemismatch";
    }
}
