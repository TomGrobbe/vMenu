namespace vMenu.Enhanced.Menus.Players.Character;

public static class PedHeadOverlays
{
    public const int NoColour = 0;

    public const int HairPalette = 1;

    public const int MakeupPalette = 2;

    public const int Unset = 255;

    public const int Blemishes = 0;

    public const int Beard = 1;

    public const int Eyebrows = 2;

    public const int Ageing = 3;

    public const int Makeup = 4;

    public const int Blush = 5;

    public const int Complexion = 6;

    public const int SunDamage = 7;

    public const int Lipstick = 8;

    public const int MolesFreckles = 9;

    public const int ChestHair = 10;

    public const int BodyBlemishes = 11;

    public static readonly int[] Core =
        [Blemishes, Ageing, Complexion, SunDamage, MolesFreckles, BodyBlemishes];

    public static readonly int[] Style =
        [Beard, Eyebrows, ChestHair, Makeup, Blush, Lipstick];

    public static readonly int[] All =
    [
        Blemishes, Beard, Eyebrows, Ageing, Makeup, Blush,
        Complexion, SunDamage, Lipstick, MolesFreckles, ChestHair, BodyBlemishes,
    ];

    public static int ColourType(int overlay) => overlay switch
    {
        Beard or Eyebrows or ChestHair => HairPalette,
        Makeup or Blush or Lipstick => MakeupPalette,
        _ => NoColour,
    };

    public static bool IsMaleOnly(int overlay) => overlay is Beard or ChestHair;

    public static string TechnicalName(int overlay) => overlay switch
    {
        Blemishes => "blemishes",
        Beard => "beard",
        Eyebrows => "eyebrows",
        Ageing => "ageing",
        Makeup => "makeup",
        Blush => "blush",
        Complexion => "complexion",
        SunDamage => "sun damage",
        Lipstick => "lipstick",
        MolesFreckles => "moles and freckles",
        ChestHair => "chest hair",
        BodyBlemishes => "body blemishes",
        _ => "unknown",
    };
}
