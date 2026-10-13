using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// A class rather than a record: generated equality routes through
// EqualityComparer<string>.Default, which the client sandbox refuses to load.
public sealed class VehicleColorOption(int id, string gxtKey)
{
    public int Id { get; } = id;

    public string GxtKey { get; } = gxtKey;

    // The game's name for this colour, or a readable form of the key when it has none.
    public MenuText Text => GameLabels.GameOrLiteral(GxtKey, Fallback);

    private string Fallback => VehicleColorTables.FallbackName(GxtKey);
}

public sealed class VehicleColorGroup(string nameKey, IReadOnlyList<VehicleColorOption> colors)
{
    public string NameKey { get; } = nameKey;

    public IReadOnlyList<VehicleColorOption> Colors { get; } = colors;

    public int IndexOf(int colorId)
    {
        for (var index = 0; index < Colors.Count; index++)
        {
            if (Colors[index].Id == colorId)
            {
                return index;
            }
        }

        return -1;
    }
}

// The paint finish is a separate setting rather than part of these groups, because the game treats
// it that way: the same colour id looks completely different depending on the finish applied over
// it. That is why there is no separate "metallic" table. Metallic is a finish, not a colour.
public static class VehicleColorTables
{
    // These three are the only holes in the table. Everything else resolves on a stock install.
    private static readonly Dictionary<string, string> Fallbacks = new(StringComparer.Ordinal)
    {
        ["veh_color_off_white"] = "Off White",
        ["veh_color_taxi_yellow"] = "Taxi Yellow",
        ["VERY_DARK_BLUE"] = "Very Dark Blue",
    };

    // The main table. Also what pearlescent, dashboard, interior and wheel colours pick from.
    public static IReadOnlyList<VehicleColorOption> Classic { get; } =
    [
        new(0, "BLACK"), new(1, "GRAPHITE"), new(2, "BLACK_STEEL"), new(3, "DARK_SILVER"),
        new(4, "SILVER"), new(5, "BLUE_SILVER"), new(6, "ROLLED_STEEL"), new(7, "SHADOW_SILVER"),
        new(8, "STONE_SILVER"), new(9, "MIDNIGHT_SILVER"), new(10, "CAST_IRON_SIL"), new(11, "ANTHR_BLACK"),

        new(27, "RED"), new(28, "TORINO_RED"), new(29, "FORMULA_RED"), new(30, "BLAZE_RED"),
        new(31, "GRACE_RED"), new(32, "GARNET_RED"), new(33, "SUNSET_RED"), new(34, "CABERNET_RED"),
        new(35, "CANDY_RED"), new(36, "SUNRISE_ORANGE"), new(37, "GOLD"), new(38, "ORANGE"),

        new(49, "DARK_GREEN"), new(50, "RACING_GREEN"), new(51, "SEA_GREEN"), new(52, "OLIVE_GREEN"),
        new(53, "BRIGHT_GREEN"), new(54, "PETROL_GREEN"),

        new(61, "GALAXY_BLUE"), new(62, "DARK_BLUE"), new(63, "SAXON_BLUE"), new(64, "BLUE"),
        new(65, "MARINER_BLUE"), new(66, "HARBOR_BLUE"), new(67, "DIAMOND_BLUE"), new(68, "SURF_BLUE"),
        new(69, "NAUTICAL_BLUE"), new(70, "ULTRA_BLUE"), new(71, "PURPLE"), new(72, "SPIN_PURPLE"),
        new(73, "RACING_BLUE"), new(74, "LIGHT_BLUE"),

        new(88, "YELLOW"), new(89, "RACE_YELLOW"), new(90, "BRONZE"), new(91, "FLUR_YELLOW"),
        new(92, "LIME_GREEN"),

        new(94, "UMBER_BROWN"), new(95, "CREEK_BROWN"), new(96, "CHOCOLATE_BROWN"), new(97, "MAPLE_BROWN"),
        new(98, "SADDLE_BROWN"), new(99, "STRAW_BROWN"), new(100, "MOSS_BROWN"), new(101, "BISON_BROWN"),
        new(102, "WOODBEECH_BROWN"), new(103, "BEECHWOOD_BROWN"), new(104, "SIENNA_BROWN"),
        new(105, "SANDY_BROWN"), new(106, "BLEECHED_BROWN"), new(107, "CREAM"),

        new(111, "WHITE"), new(112, "FROST_WHITE"),

        new(135, "HOT PINK"), new(136, "SALMON_PINK"), new(137, "PINK"), new(138, "BRIGHT_ORANGE"),

        new(141, "MIDNIGHT_BLUE"), new(142, "MIGHT_PURPLE"), new(143, "WINE_RED"),

        new(145, "BRIGHT_PURPLE"), new(146, "VERY_DARK_BLUE"), new(147, "BLACK_GRAPHITE"),

        new(150, "LAVA_RED"),
    ];

    public static IReadOnlyList<VehicleColorOption> Matte { get; } =
    [
        new(12, "BLACK"), new(13, "GREY"), new(14, "LIGHT_GREY"),
        new(39, "RED"), new(40, "DARK_RED"), new(41, "ORANGE"), new(42, "YELLOW"),
        new(55, "LIME_GREEN"),
        new(82, "DARK_BLUE"), new(83, "BLUE"), new(84, "MIDNIGHT_BLUE"),
        new(128, "GREEN"),
        new(148, "PURPLE"), new(149, "MIGHT_PURPLE"),
        new(151, "MATTE_FOR"), new(152, "MATTE_OD"), new(153, "MATTE_DIRT"),
        new(154, "MATTE_DESERT"), new(155, "MATTE_FOIL"),
    ];

    public static IReadOnlyList<VehicleColorOption> Metal { get; } =
    [
        new(117, "BR_STEEL"), new(118, "BR BLACK_STEEL"), new(119, "BR_ALUMINIUM"),
        new(158, "GOLD_P"), new(159, "GOLD_S"),
    ];

    public static IReadOnlyList<VehicleColorOption> Utility { get; } =
    [
        new(15, "BLACK"), new(16, "FMMC_COL1_1"), new(17, "DARK_SILVER"), new(18, "SILVER"),
        new(19, "BLACK_STEEL"), new(20, "SHADOW_SILVER"),
        new(43, "DARK_RED"), new(44, "RED"), new(45, "GARNET_RED"),
        new(56, "DARK_GREEN"), new(57, "GREEN"),
        new(75, "DARK_BLUE"), new(76, "MIDNIGHT_BLUE"), new(77, "SAXON_BLUE"), new(78, "NAUTICAL_BLUE"),
        new(79, "BLUE"), new(80, "FMMC_COL1_13"), new(81, "BRIGHT_PURPLE"),
        new(93, "STRAW_BROWN"),
        new(108, "UMBER_BROWN"), new(109, "MOSS_BROWN"), new(110, "SANDY_BROWN"),
        new(122, "veh_color_off_white"),
        new(125, "BRIGHT_GREEN"),
        new(127, "HARBOR_BLUE"),
        new(134, "FROST_WHITE"),
        new(139, "LIME_GREEN"), new(140, "ULTRA_BLUE"),
        new(144, "GREY"),
        new(157, "LIGHT_BLUE"),
        new(160, "YELLOW"),
    ];

    public static IReadOnlyList<VehicleColorOption> Worn { get; } =
    [
        new(21, "BLACK"), new(22, "GRAPHITE"), new(23, "LIGHT_GREY"), new(24, "SILVER"),
        new(25, "BLUE_SILVER"), new(26, "SHADOW_SILVER"),
        new(46, "RED"), new(47, "SALMON_PINK"), new(48, "DARK_RED"),
        new(58, "DARK_GREEN"), new(59, "GREEN"), new(60, "SEA_GREEN"),
        new(85, "DARK_BLUE"), new(86, "BLUE"), new(87, "LIGHT_BLUE"),
        new(113, "SANDY_BROWN"), new(114, "BISON_BROWN"), new(115, "CREEK_BROWN"), new(116, "BLEECHED_BROWN"),
        new(121, "veh_color_off_white"),
        new(123, "ORANGE"), new(124, "SUNRISE_ORANGE"),
        new(126, "veh_color_taxi_yellow"),
        new(129, "RACING_GREEN"), new(130, "ORANGE"), new(131, "WHITE"), new(132, "FROST_WHITE"),
        new(133, "OLIVE_GREEN"),
    ];

    // The paints that shift colour with the viewing angle, added in a later game build.
    public static IReadOnlyList<VehicleColorOption> Chameleon { get; } =
    [
        new(223, "G9_PAINT01"), new(224, "G9_PAINT02"), new(225, "G9_PAINT03"), new(226, "G9_PAINT04"),
        new(227, "G9_PAINT05"), new(228, "G9_PAINT06"), new(229, "G9_PAINT07"), new(230, "G9_PAINT08"),
        new(231, "G9_PAINT09"), new(232, "G9_PAINT10"), new(233, "G9_PAINT11"), new(234, "G9_PAINT12"),
        new(235, "G9_PAINT13"), new(236, "G9_PAINT14"), new(237, "G9_PAINT15"), new(238, "G9_PAINT16"),
    ];

    // Asked of the game rather than of a build number, so it also answers correctly on a server that
    // added the text itself.
    public static bool HasChameleonPaints => GameLabels.Exists("G9_PAINT01");

    // Chameleon is not among them. It is a paint finish as much as a colour, and mixing it in with the
    // rest meant picking one changed the colour without changing the finish, which looked like the menu
    // doing something different in each direction. It has a row of its own instead.
    public static IReadOnlyList<VehicleColorGroup> BodyGroups { get; } =
    [
        new(Loc.VehicleOptions.ColorGroupClassic, Classic),
        new(Loc.VehicleOptions.ColorGroupMatte, Matte),
        new(Loc.VehicleOptions.ColorGroupMetal, Metal),
        new(Loc.VehicleOptions.ColorGroupUtility, Utility),
        new(Loc.VehicleOptions.ColorGroupWorn, Worn),
    ];

    public static VehicleColorOption? Find(int colorId)
    {
        foreach (var group in BodyGroups)
        {
            foreach (var color in group.Colors)
            {
                if (color.Id == colorId)
                {
                    return color;
                }
            }
        }

        foreach (var color in Chameleon)
        {
            if (color.Id == colorId)
            {
                return color;
            }
        }

        return null;
    }

    // Minus one when the colour is not in the table.
    public static int IndexOf(IReadOnlyList<VehicleColorOption> colors, int colorId)
    {
        for (var index = 0; index < colors.Count; index++)
        {
            if (colors[index].Id == colorId)
            {
                return index;
            }
        }

        return -1;
    }

    internal static string FallbackName(string gxtKey) =>
        Fallbacks.TryGetValue(gxtKey, out var name) ? name : GameLabels.Humanise(gxtKey);
}
