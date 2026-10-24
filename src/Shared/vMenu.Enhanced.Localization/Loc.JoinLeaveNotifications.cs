namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class JoinLeaveNotifications
    {
        [English("~g~{player}~s~ joined the server.")]
        public const string Joined = "joinleave.joined";

        [English("~r~{player}~s~ left the server.")]
        public const string Left = "joinleave.left";

        [English("~r~{player}~s~ left the server. ~c~({reason})")]
        public const string LeftWithReason = "joinleave.left.reason";
    }
}
