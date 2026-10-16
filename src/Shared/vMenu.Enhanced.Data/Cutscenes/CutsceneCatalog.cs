namespace vMenu.Enhanced.Data.Cutscenes;

public static class CutsceneCatalog
{
    public const string HaircutRoast = "Haircut";

    public const string BridgePullOver = "PulledOver";

    public static IReadOnlyList<CutsceneEntry> All { get; } =
    [
        new(HaircutRoast, "had Lamar roast the haircut of"),
        new(BridgePullOver, "had Trevor pull over"),
    ];
}

public sealed class CutsceneEntry(string id, string bullyVerb)
{
    public string Id { get; } = id;

    public string BullyVerb { get; } = bullyVerb;
}
