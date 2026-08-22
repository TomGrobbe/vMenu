namespace vMenu.Enhanced.Menus.Players.Character;

public static class PedHairDecorations
{
    private const string Multiplayer = "multiplayer_overlays";

    private const string Business = "mpbusiness_overlays";

    private const string Hipster = "mphipster_overlays";

    private static readonly (string Collection, string Name)[] ByHairStyle =
    [
        (Multiplayer, "FM_M_Hair_001_a"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_003_a"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_008_a"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_long_a"),
        (Multiplayer, "FM_M_Hair_long_a"),
        (Multiplayer, "FM_M_Hair_001_z"),
        (Multiplayer, "FM_M_Hair_001_a"),
        (Business, "FM_Bus_M_Hair_000_a"),
        (Business, "FM_Bus_M_Hair_001_a"),
        (Hipster, "FM_Hip_M_Hair_000_a"),
        (Hipster, "FM_Hip_M_Hair_001_a"),
        (Multiplayer, "FM_M_Hair_001_a"),
    ];

    public static (string Collection, string Name)? For(int hairStyle) =>
        hairStyle >= 0 && hairStyle < ByHairStyle.Length ? ByHairStyle[hairStyle] : null;

    public static bool IsScalpOverlay(string collection, string name)
    {
        foreach (var entry in ByHairStyle)
        {
            if (string.Equals(entry.Collection, collection, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
