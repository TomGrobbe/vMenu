using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// A class rather than a record: generated equality routes through
// EqualityComparer<string>.Default, which the client sandbox refuses to load.
public sealed class VehicleColorOption(int id, string gxtKey, MenuText fallback = default)
{
    public int Id { get; } = id;

    public string GxtKey { get; } = gxtKey;

    // The game's name for this colour, or a readable form of the key when it has none.
    public MenuText Text => fallback.IsEmpty
        ? GameLabels.GameOrLiteral(GxtKey, Fallback)
        : MenuText.From(() => GameLabels.Text(GxtKey, fallback.Resolve(Localizer.Current)));

    private string Fallback => VehicleColorTables.FallbackName(GxtKey);
}

public static class VehicleColorTables
{
    // These three are the only holes in the table. Everything else resolves on a stock install.
    private static readonly Dictionary<string, string> Fallbacks = new(StringComparer.Ordinal)
    {
        ["veh_color_off_white"] = "Off White",
        ["veh_color_taxi_yellow"] = "Taxi Yellow",
        ["VERY_DARK_BLUE"] = "Very Dark Blue",
    };

    // The body colours that are not mod shop paints, plus the names the appearance report looks up.
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

    // The mod shop's own lists, in the mod shop's order.
    public static IReadOnlyList<VehicleColorOption> Wheel { get; } =
    [
        new(156, "CMOD_COL5_0", MenuText.Key(Loc.VehicleOptions.ColorDefaultAlloy)),
        WheelColor(0, 1, "BLACK"), WheelColor(1, 2, "GRAPHITE"), WheelColor(11, 3, "ANTHR_BLACK"),
        WheelColor(2, 4, "BLACK_STEEL"), WheelColor(8, 5, "STONE_SILVER"), WheelColor(122, 6, "veh_color_off_white"),
        WheelColor(27, 7, "RED"), WheelColor(30, 8, "BLAZE_RED"), WheelColor(45, 9, "GARNET_RED"),
        WheelColor(35, 10, "CANDY_RED"), WheelColor(33, 11, "SUNSET_RED"), WheelColor(136, 12, "SALMON_PINK"),
        WheelColor(135, 13, "HOT PINK"), WheelColor(36, 14, "SUNRISE_ORANGE"), WheelColor(41, 15, "ORANGE"),
        WheelColor(138, 16, "BRIGHT_ORANGE"), WheelColor(37, 17, "GOLD"), WheelColor(99, 18, "STRAW_BROWN"),
        WheelColor(90, 19, "BRONZE"), WheelColor(95, 20, "CREEK_BROWN"), WheelColor(115, 21, "CREEK_BROWN"),
        WheelColor(109, 22, "MOSS_BROWN"), WheelColor(153, 23, "MATTE_DIRT"), WheelColor(154, 24, "MATTE_DESERT"),
        WheelColor(88, 25, "YELLOW"), WheelColor(89, 26, "RACE_YELLOW"), WheelColor(91, 27, "FLUR_YELLOW"),
        WheelColor(55, 28, "LIME_GREEN"), WheelColor(125, 29, "BRIGHT_GREEN"), WheelColor(53, 30, "BRIGHT_GREEN"),
        WheelColor(56, 31, "DARK_GREEN"), WheelColor(151, 32, "MATTE_FOR"), WheelColor(82, 33, "DARK_BLUE"),
        WheelColor(64, 34, "BLUE"), WheelColor(87, 35, "LIGHT_BLUE"), WheelColor(70, 36, "ULTRA_BLUE"),
        WheelColor(140, 37, "ULTRA_BLUE"), WheelColor(81, 38, "BRIGHT_PURPLE"), WheelColor(145, 39, "BRIGHT_PURPLE"),
        WheelColor(142, 40, "MIGHT_PURPLE"),
    ];

    public static IReadOnlyList<VehicleColorOption> Interior { get; } =
    [
        new(0, "BLACK"), new(1, "GRAPHITE"), new(11, "ANTHR_BLACK"), new(2, "BLACK_STEEL"),
        new(3, "DARK_SILVER"), new(5, "BLUE_SILVER"), new(6, "ROLLED_STEEL"), new(7, "SHADOW_SILVER"),
        new(8, "STONE_SILVER"), new(9, "MIDNIGHT_SILVER"), new(10, "CAST_IRON_SIL"), new(27, "RED"),
        new(28, "TORINO_RED"), new(150, "LAVA_RED"), new(30, "BLAZE_RED"), new(31, "GRACE_RED"),
        new(32, "GARNET_RED"), new(33, "SUNSET_RED"), new(34, "CABERNET_RED"), new(143, "WINE_RED"),
        new(35, "CANDY_RED"), new(137, "PINK"), new(136, "SALMON_PINK"), new(36, "SUNRISE_ORANGE"),
        new(38, "ORANGE"), new(138, "BRIGHT_ORANGE"), new(90, "BRONZE"), new(88, "YELLOW"),
        new(89, "RACE_YELLOW"), new(91, "FLUR_YELLOW"), new(49, "DARK_GREEN"), new(50, "RACING_GREEN"),
        new(51, "SEA_GREEN"), new(52, "OLIVE_GREEN"), new(53, "BRIGHT_GREEN"), new(54, "PETROL_GREEN"),
        new(92, "LIME_GREEN"), new(141, "MIDNIGHT_BLUE"), new(61, "GALAXY_BLUE"), new(62, "DARK_BLUE"),
        new(63, "SAXON_BLUE"), new(65, "MARINER_BLUE"), new(66, "HARBOR_BLUE"), new(67, "DIAMOND_BLUE"),
        new(68, "SURF_BLUE"), new(69, "NAUTICAL_BLUE"), new(73, "RACING_BLUE"), new(70, "ULTRA_BLUE"),
        new(74, "LIGHT_BLUE"), new(96, "CHOCOLATE_BROWN"), new(101, "BISON_BROWN"), new(95, "CREEK_BROWN"),
        new(94, "UMBER_BROWN"), new(97, "MAPLE_BROWN"), new(103, "BEECHWOOD_BROWN"), new(104, "SIENNA_BROWN"),
        new(98, "SADDLE_BROWN"), new(100, "MOSS_BROWN"), new(102, "WOODBEECH_BROWN"), new(99, "STRAW_BROWN"),
        new(105, "SANDY_BROWN"), new(106, "BLEECHED_BROWN"), new(72, "SPIN_PURPLE"), new(146, "MIGHT_PURPLE"),
        new(145, "BRIGHT_PURPLE"), new(107, "CREAM"), new(111, "WHITE"), new(112, "FROST_WHITE"),
    ];

    public static IReadOnlyList<VehicleColorOption> Dashboard { get; } =
    [
        new(4, "SILVER"), new(5, "BLUE_SILVER"), new(6, "ROLLED_STEEL"), new(7, "SHADOW_SILVER"),
        new(111, "WHITE"), new(112, "FROST_WHITE"), new(107, "CREAM"), new(104, "SIENNA_BROWN"),
        new(98, "SADDLE_BROWN"), new(100, "MOSS_BROWN"), new(102, "WOODBEECH_BROWN"), new(99, "STRAW_BROWN"),
        new(105, "SANDY_BROWN"), new(106, "BLEECHED_BROWN"), new(37, "GOLD"), new(90, "BRONZE"),
        new(88, "YELLOW"), new(89, "RACE_YELLOW"), new(91, "FLUR_YELLOW"), new(38, "ORANGE"),
        new(138, "BRIGHT_ORANGE"), new(36, "SUNRISE_ORANGE"), new(27, "RED"), new(28, "TORINO_RED"),
        new(29, "FORMULA_RED"), new(150, "LAVA_RED"), new(30, "BLAZE_RED"), new(31, "GRACE_RED"),
        new(32, "GARNET_RED"), new(35, "CANDY_RED"), new(135, "HOT PINK"), new(137, "PINK"),
        new(136, "SALMON_PINK"), new(71, "PURPLE"), new(145, "BRIGHT_PURPLE"), new(63, "SAXON_BLUE"),
        new(64, "BLUE"), new(65, "MARINER_BLUE"), new(66, "HARBOR_BLUE"), new(67, "DIAMOND_BLUE"),
        new(68, "SURF_BLUE"), new(69, "NAUTICAL_BLUE"), new(73, "RACING_BLUE"), new(70, "ULTRA_BLUE"),
        new(74, "LIGHT_BLUE"), new(51, "SEA_GREEN"), new(53, "BRIGHT_GREEN"), new(54, "PETROL_GREEN"),
        new(92, "LIME_GREEN"),
    ];

    private static IReadOnlyList<IReadOnlyList<VehicleColorOption>> Named { get; } = [Classic, Matte, Metal, Utility, Worn];

    public static VehicleColorOption? Find(int colorId)
    {
        foreach (var colors in Named)
        {
            foreach (var color in colors)
            {
                if (color.Id == colorId)
                {
                    return color;
                }
            }
        }

        return ModPaintCatalog.FindCached(colorId) is { Label.Length: > 0 } paint
            ? new VehicleColorOption(paint.Paint, paint.Label)
            : null;
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

    private static VehicleColorOption WheelColor(int id, int slot, string colorKey) =>
        new(id, "CMOD_COL5_" + slot, GameLabels.GameOrLiteral(colorKey, FallbackName(colorKey)));

    internal static string FallbackName(string gxtKey) =>
        Fallbacks.TryGetValue(gxtKey, out var name) ? name : GameLabels.Humanise(gxtKey);
}
