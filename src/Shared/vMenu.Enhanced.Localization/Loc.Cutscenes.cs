namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class Cutscenes
    {
        public static string Name(string cutscene) => "cutscenes." + cutscene.ToLowerInvariant();

        public static string Description(string cutscene) => Name(cutscene) + ".desc";

        [English("Cutscenes")]
        public const string Title = "cutscenes.title";

        [English("Star In A Story Scene")]
        public const string Subtitle = "cutscenes.subtitle";

        [English("Pick a scene from the story and play a part in it as your own character.")]
        public const string LinkDescription = "cutscenes.link.desc";

        [English("Lamar's Haircut Roast")]
        public const string HaircutRoast = "cutscenes.haircut";

        [English(
            "Lamar roasts your new haircut in Franklin's driveway, with you playing Franklin. You go back to where you were when it ends.")]
        public const string HaircutRoastDescription = "cutscenes.haircut.desc";

        [English("Pulled Over On The Bridge")]
        public const string BridgePullOver = "cutscenes.pulledover";

        [English(
            "Trevor and Michael, dressed as cops, pull you over on a bridge and Trevor drags you out of your car. You go back to where you were when it ends.")]
        public const string BridgePullOverDescription = "cutscenes.pulledover.desc";

        [English("You have to be on foot to start a cutscene.")]
        public const string NeedsOnFoot = "cutscenes.needsonfoot";

        [English("Something else is already playing for you. Try again when it is done.")]
        public const string Busy = "cutscenes.busy";

        [English("The cutscene could not be started. Try again in a moment.")]
        public const string DidNotWork = "cutscenes.didnotwork";
    }
}
