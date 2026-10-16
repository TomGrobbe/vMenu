namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class Cutscenes
    {
        public static string Name(string cutscene) => "cutscenes." + cutscene.ToLowerInvariant();

        public static string Description(string cutscene) => Name(cutscene) + ".desc";

        public const string Title = "cutscenes.title";

        public const string Subtitle = "cutscenes.subtitle";

        public const string LinkDescription = "cutscenes.link.desc";

        public const string HaircutRoast = "cutscenes.haircut";

        public const string HaircutRoastDescription = "cutscenes.haircut.desc";

        public const string BridgePullOver = "cutscenes.pulledover";

        public const string BridgePullOverDescription = "cutscenes.pulledover.desc";

        public const string NeedsOnFoot = "cutscenes.needsonfoot";

        public const string Busy = "cutscenes.busy";

        public const string DidNotWork = "cutscenes.didnotwork";
    }
}
