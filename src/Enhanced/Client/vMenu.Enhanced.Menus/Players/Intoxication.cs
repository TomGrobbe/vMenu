namespace vMenu.Enhanced.Menus.Players;

internal static class Intoxication
{
    public const string DrunkWalk = "move_m@drunk@verydrunk";

    public const string Shake = "DRUNK_SHAKE";

    public const string DrugsIn = "DrugsMichaelAliensFightIn";

    public const string DrugsOut = "DrugsMichaelAliensFightOut";

    public static string Timecycle(bool drugged) => drugged ? "stoned_aliens" : "DRUNK";

    public static float ShakeAmplitude(bool drugged) => drugged ? 2f : 1.2f;
}
