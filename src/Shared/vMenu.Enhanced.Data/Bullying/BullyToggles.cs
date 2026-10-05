namespace vMenu.Enhanced.Data.Bullying;

public static class BullyToggles
{
    public const string ShockOnEntry = "ShockOnEntry";

    public const string Riot = "Riot";

    public const string InvertControls = "InvertControls";

    public const string RandomThrottle = "RandomThrottle";

    public const string LowGrip = "LowGrip";

    public const string Lod = "Lod";

    public static IReadOnlyList<string> All { get; } = [ShockOnEntry, Riot, InvertControls, RandomThrottle, LowGrip, Lod];

    public static IReadOnlyList<string> ServerWide { get; } = [Riot, InvertControls, RandomThrottle, LowGrip];

    public static bool IsKnown(string id) => All.Any(toggle => toggle == id);

    public static string LogName(string id) => id switch
    {
        ShockOnEntry => "electric door handles",
        Riot => "gang attack",
        InvertControls => "inverted vehicle controls",
        RandomThrottle => "possessed pedals",
        LowGrip => "slippery tyres",
        Lod => "glitched body",
        _ => id,
    };
}
