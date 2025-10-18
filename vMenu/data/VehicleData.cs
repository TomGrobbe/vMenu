using System.Collections.Generic;

using static CitizenFX.Core.Native.API;

namespace vMenuClient.data
{
    public static class VehicleData
    {
        public readonly struct VehicleColor
        {
            public readonly int id;
            public readonly string label;

            public VehicleColor(int id, string label)
            {
                if (label == "veh_color_taxi_yellow")
                {
                    if (GetLabelText("veh_color_taxi_yellow") == "NULL")
                    {
                        AddTextEntry("veh_color_taxi_yellow", $"Taxi {GetLabelText("IEC_T20_2")}");
                    }
                }
                else if (label == "veh_color_off_white")
                {
                    if (GetLabelText("veh_color_off_white") == "NULL")
                    {
                        AddTextEntry("veh_color_off_white", "Off White");
                    }
                }
                else if (label == "VERY_DARK_BLUE")
                {
                    if (GetLabelText("VERY_DARK_BLUE") == "NULL")
                    {
                        AddTextEntry("VERY_DARK_BLUE", "Very Dark Blue");
                    }
                }
                // Chameleon Colour Labels //
                else if (label == "G9_PAINT01")
                {
                    AddTextEntry("G9_PAINT01", "Monochrome");
                }
                else if (label == "G9_PAINT02")
                {
                    AddTextEntry("G9_PAINT02", "Night & Day");
                }
                else if (label == "G9_PAINT03")
                {
                    AddTextEntry("G9_PAINT03", "The Verlierer");
                }
                else if (label == "G9_PAINT04")
                {
                    AddTextEntry("G9_PAINT04", "Sprunk Extreme");
                }
                else if (label == "G9_PAINT05")
                {
                    AddTextEntry("G9_PAINT05", "Vice City");
                }
                else if (label == "G9_PAINT06")
                {
                    AddTextEntry("G9_PAINT06", "Synthwave Nights");
                }
                else if (label == "G9_PAINT07")
                {
                    AddTextEntry("G9_PAINT07", "Four Seasons");
                }
                else if (label == "G9_PAINT08")
                {
                    AddTextEntry("G9_PAINT08", "Maisonette 9 Throwback");
                }
                else if (label == "G9_PAINT09")
                {
                    AddTextEntry("G9_PAINT09", "Bubblegum");
                }
                else if (label == "G9_PAINT10")
                {
                    AddTextEntry("G9_PAINT10", "Full Rainbow");
                }
                else if (label == "G9_PAINT11")
                {
                    AddTextEntry("G9_PAINT11", "Sunset");
                }
                else if (label == "G9_PAINT12")
                {
                    AddTextEntry("G9_PAINT12", "The Seven");
                }
                else if (label == "G9_PAINT13")
                {
                    AddTextEntry("G9_PAINT13", "Kamen Rider");
                }
                else if (label == "G9_PAINT14")
                {
                    AddTextEntry("G9_PAINT14", "Chromatic Aberration");
                }
                else if (label == "G9_PAINT15")
                {
                    AddTextEntry("G9_PAINT15", "It's Christmas!");
                }
                else if (label == "G9_PAINT16")
                {
                    AddTextEntry("G9_PAINT16", "Temperature");
                }

                this.label = label;
                this.id = id;
            }
        }

        public static readonly List<VehicleColor> ClassicColors = new()
        {
            new VehicleColor(0, "BLACK"),
            new VehicleColor(1, "GRAPHITE"),
            new VehicleColor(2, "BLACK_STEEL"),
            new VehicleColor(3, "DARK_SILVER"),
            new VehicleColor(4, "SILVER"),
            new VehicleColor(5, "BLUE_SILVER"),
            new VehicleColor(6, "ROLLED_STEEL"),
            new VehicleColor(7, "SHADOW_SILVER"),
            new VehicleColor(8, "STONE_SILVER"),
            new VehicleColor(9, "MIDNIGHT_SILVER"),
            new VehicleColor(10, "CAST_IRON_SIL"),
            new VehicleColor(11, "ANTHR_BLACK"),

            new VehicleColor(27, "RED"),
            new VehicleColor(28, "TORINO_RED"),
            new VehicleColor(29, "FORMULA_RED"),
            new VehicleColor(30, "BLAZE_RED"),
            new VehicleColor(31, "GRACE_RED"),
            new VehicleColor(32, "GARNET_RED"),
            new VehicleColor(33, "SUNSET_RED"),
            new VehicleColor(34, "CABERNET_RED"),
            new VehicleColor(35, "CANDY_RED"),
            new VehicleColor(36, "SUNRISE_ORANGE"),
            new VehicleColor(37, "GOLD"),
            new VehicleColor(38, "ORANGE"),

            new VehicleColor(49, "DARK_GREEN"),
            new VehicleColor(50, "RACING_GREEN"),
            new VehicleColor(51, "SEA_GREEN"),
            new VehicleColor(52, "OLIVE_GREEN"),
            new VehicleColor(53, "BRIGHT_GREEN"),
            new VehicleColor(54, "PETROL_GREEN"),

            new VehicleColor(61, "GALAXY_BLUE"),
            new VehicleColor(62, "DARK_BLUE"),
            new VehicleColor(63, "SAXON_BLUE"),
            new VehicleColor(64, "BLUE"),
            new VehicleColor(65, "MARINER_BLUE"),
            new VehicleColor(66, "HARBOR_BLUE"),
            new VehicleColor(67, "DIAMOND_BLUE"),
            new VehicleColor(68, "SURF_BLUE"),
            new VehicleColor(69, "NAUTICAL_BLUE"),
            new VehicleColor(70, "ULTRA_BLUE"),
            new VehicleColor(71, "PURPLE"),
            new VehicleColor(72, "SPIN_PURPLE"),
            new VehicleColor(73, "RACING_BLUE"),
            new VehicleColor(74, "LIGHT_BLUE"),

            new VehicleColor(88, "YELLOW"),
            new VehicleColor(89, "RACE_YELLOW"),
            new VehicleColor(90, "BRONZE"),
            new VehicleColor(91, "FLUR_YELLOW"),
            new VehicleColor(92, "LIME_GREEN"),

            new VehicleColor(94, "UMBER_BROWN"),
            new VehicleColor(95, "CREEK_BROWN"),
            new VehicleColor(96, "CHOCOLATE_BROWN"),
            new VehicleColor(97, "MAPLE_BROWN"),
            new VehicleColor(98, "SADDLE_BROWN"),
            new VehicleColor(99, "STRAW_BROWN"),
            new VehicleColor(100, "MOSS_BROWN"),
            new VehicleColor(101, "BISON_BROWN"),
            new VehicleColor(102, "WOODBEECH_BROWN"),
            new VehicleColor(103, "BEECHWOOD_BROWN"),
            new VehicleColor(104, "SIENNA_BROWN"),
            new VehicleColor(105, "SANDY_BROWN"),
            new VehicleColor(106, "BLEECHED_BROWN"),
            new VehicleColor(107, "CREAM"),

            new VehicleColor(111, "WHITE"),
            new VehicleColor(112, "FROST_WHITE"),

            new VehicleColor(135, "HOT PINK"),
            new VehicleColor(136, "SALMON_PINK"),
            new VehicleColor(137, "PINK"),
            new VehicleColor(138, "BRIGHT_ORANGE"),

            new VehicleColor(141, "MIDNIGHT_BLUE"),
            new VehicleColor(142, "MIGHT_PURPLE"),
            new VehicleColor(143, "WINE_RED"),

            new VehicleColor(145, "BRIGHT_PURPLE"),
            new VehicleColor(146, "VERY_DARK_BLUE"),
            new VehicleColor(147, "BLACK_GRAPHITE"),

            new VehicleColor(150, "LAVA_RED"),
        };

        public static readonly List<VehicleColor> MatteColors = new()
        {
            new VehicleColor(12, "BLACK"),
            new VehicleColor(13, "GREY"),
            new VehicleColor(14, "LIGHT_GREY"),

            new VehicleColor(39, "RED"),
            new VehicleColor(40, "DARK_RED"),
            new VehicleColor(41, "ORANGE"),
            new VehicleColor(42, "YELLOW"),

            new VehicleColor(55, "LIME_GREEN"),

            new VehicleColor(82, "DARK_BLUE"),
            new VehicleColor(83, "BLUE"),
            new VehicleColor(84, "MIDNIGHT_BLUE"),

            new VehicleColor(128, "GREEN"),

            new VehicleColor(148, "Purple"),
            new VehicleColor(149, "MIGHT_PURPLE"),

            new VehicleColor(151, "MATTE_FOR"),
            new VehicleColor(152, "MATTE_OD"),
            new VehicleColor(153, "MATTE_DIRT"),
            new VehicleColor(154, "MATTE_DESERT"),
            new VehicleColor(155, "MATTE_FOIL"),
        };

        public static readonly List<VehicleColor> MetalColors = new()
        {
            new VehicleColor(117, "BR_STEEL"),
            new VehicleColor(118, "BR BLACK_STEEL"),
            new VehicleColor(119, "BR_ALUMINIUM"),

            new VehicleColor(158, "GOLD_P"),
            new VehicleColor(159, "GOLD_S"),
        };

        public static readonly List<VehicleColor> UtilColors = new()
        {
            new VehicleColor(15, "BLACK"),
            new VehicleColor(16, "FMMC_COL1_1"),
            new VehicleColor(17, "DARK_SILVER"),
            new VehicleColor(18, "SILVER"),
            new VehicleColor(19, "BLACK_STEEL"),
            new VehicleColor(20, "SHADOW_SILVER"),

            new VehicleColor(43, "DARK_RED"),
            new VehicleColor(44, "RED"),
            new VehicleColor(45, "GARNET_RED"),

            new VehicleColor(56, "DARK_GREEN"),
            new VehicleColor(57, "GREEN"),

            new VehicleColor(75, "DARK_BLUE"),
            new VehicleColor(76, "MIDNIGHT_BLUE"),
            new VehicleColor(77, "SAXON_BLUE"),
            new VehicleColor(78, "NAUTICAL_BLUE"),
            new VehicleColor(79, "BLUE"),
            new VehicleColor(80, "FMMC_COL1_13"),
            new VehicleColor(81, "BRIGHT_PURPLE"),

            new VehicleColor(93, "STRAW_BROWN"),

            new VehicleColor(108, "UMBER_BROWN"),
            new VehicleColor(109, "MOSS_BROWN"),
            new VehicleColor(110, "SANDY_BROWN"),

            new VehicleColor(122, "veh_color_off_white"),

            new VehicleColor(125, "BRIGHT_GREEN"),

            new VehicleColor(127, "HARBOR_BLUE"),

            new VehicleColor(134, "FROST_WHITE"),

            new VehicleColor(139, "LIME_GREEN"),
            new VehicleColor(140, "ULTRA_BLUE"),

            new VehicleColor(144, "GREY"),

            new VehicleColor(157, "LIGHT_BLUE"),

            new VehicleColor(160, "YELLOW")
        };

        public static readonly List<VehicleColor> WornColors = new()
        {
            new VehicleColor(21, "BLACK"),
            new VehicleColor(22, "GRAPHITE"),
            new VehicleColor(23, "LIGHT_GREY"),
            new VehicleColor(24, "SILVER"),
            new VehicleColor(25, "BLUE_SILVER"),
            new VehicleColor(26, "SHADOW_SILVER"),

            new VehicleColor(46, "RED"),
            new VehicleColor(47, "SALMON_PINK"),
            new VehicleColor(48, "DARK_RED"),

            new VehicleColor(58, "DARK_GREEN"),
            new VehicleColor(59, "GREEN"),
            new VehicleColor(60, "SEA_GREEN"),

            new VehicleColor(85, "DARK_BLUE"),
            new VehicleColor(86, "BLUE"),
            new VehicleColor(87, "LIGHT_BLUE"),

            new VehicleColor(113, "SANDY_BROWN"),
            new VehicleColor(114, "BISON_BROWN"),
            new VehicleColor(115, "CREEK_BROWN"),
            new VehicleColor(116, "BLEECHED_BROWN"),

            new VehicleColor(121, "veh_color_off_white"),

            new VehicleColor(123, "ORANGE"),
            new VehicleColor(124, "SUNRISE_ORANGE"),

            new VehicleColor(126, "veh_color_taxi_yellow"),

            new VehicleColor(129, "RACING_GREEN"),
            new VehicleColor(130, "ORANGE"),
            new VehicleColor(131, "WHITE"),
            new VehicleColor(132, "FROST_WHITE"),
            new VehicleColor(133, "OLIVE_GREEN"),
        };

        // Chameleon Colour List //
        public static readonly List<VehicleColor> ChameleonColors = new()
        {
            new VehicleColor(223, "G9_PAINT01"),
            new VehicleColor(224, "G9_PAINT02"),
            new VehicleColor(225, "G9_PAINT03"),
            new VehicleColor(226, "G9_PAINT04"),
            new VehicleColor(227, "G9_PAINT05"),
            new VehicleColor(228, "G9_PAINT06"),
            new VehicleColor(229, "G9_PAINT07"),
            new VehicleColor(230, "G9_PAINT08"),
            new VehicleColor(231, "G9_PAINT09"),
            new VehicleColor(232, "G9_PAINT10"),
            new VehicleColor(233, "G9_PAINT11"),
            new VehicleColor(234, "G9_PAINT12"),
            new VehicleColor(235, "G9_PAINT13"),
            new VehicleColor(236, "G9_PAINT14"),
            new VehicleColor(237, "G9_PAINT15"),
            new VehicleColor(238, "G9_PAINT16"),
        };

        public static class Vehicles
        {
            #region Vehicle List Per Class

            #region Compacts
            public static List<string> Compacts { get; } = new List<string>()
            {
                "ASBO", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "BLISTA",
                "BRIOSO",
                "BRIOSO2", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "BRIOSO3", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "CLUB", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "DILETTANTE",
                "DILETTANTE2",
                "ISSI2",
                "ISSI3",
                "ISSI4",
                "ISSI5",
                "ISSI6",
                "KANJO", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "PANTO",
                "PRAIRIE",
                "RHAPSODY",
                "WEEVIL", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
            };
            #endregion
            #region Sedans
            public static List<string> Sedans { get; } = new List<string>()
            {
                "ASEA",
                "ASEA2",
                "ASTEROPE",
                "ASTEROPE2", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "CHAVOSV6", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "CINQUEMILA", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "COG55",
                "COG552",
                "COGNOSCENTI",
                "COGNOSCENTI2",
                "DEITY", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "DRIFTCHAVOSV6", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "DRIFTHARDY", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "DRIFTVORSCHLAG", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "EMPEROR",
                "EMPEROR2",
                "EMPEROR3",
                "FUGITIVE",
                "GLENDALE",
                "GLENDALE2", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "HARDY", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "IMPALER5", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "INGOT",
                "INTRUDER",
                "LIMO2",
                "MINIMUS", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "PREMIER",
                "PRIMO",
                "PRIMO2",
                "REGINA",
                "RHINEHART", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "ROMERO",
                "SCHAFTER2",
                "SCHAFTER5",
                "SCHAFTER6",
                "STAFFORD",
                "STANIER",
                "STRATUM",
                "STRETCH",
                "SUPERD",
                "SURGE",
                "TAILGATER",
                "TAILGATER2", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "VORSCHLAGHAMMER", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "WARRENER",
                "WARRENER2", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "WASHINGTON",
            };
            #endregion
            #region SUVs
            public static List<string> SUVs { get; } = new List<string>()
            {
                "ALEUTIAN", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "ASTRON", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "BALLER",
                "BALLER2",
                "BALLER3",
                "BALLER4",
                "BALLER5",
                "BALLER6",
                "BALLER7", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "BALLER8", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "BJXL",
                "CASTIGATOR", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "CAVALCADE",
                "CAVALCADE2",
                "CAVALCADE3", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "CONTENDER",
                "DORADO", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "DUBSTA",
                "DUBSTA2",
                "EVERON3", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "FQ2",
                "GRANGER",
                "GRANGER2", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "GRESLEY",
                "HABANERO",
                "HUNTLEY",
                "ISSI8", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "IWAGEN", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "JUBILEE", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "LANDSTALKER",
                "LANDSTALKER2", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "MESA",
                "MESA2",
                "NOVAK", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "PATRIOT",
                "PATRIOT2",
                "RADI",
                "REBLA", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "ROCOTO",
                "SEMINOLE",
                "SEMINOLE2", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "SERRANO",
                "SQUADDIE", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "TOROS",
                "VIVANITE", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "WOODLANDER", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "XLS",
                "XLS2",
            };
            #endregion
            #region Coupes
            public static List<string> Coupes { get; } = new List<string>()
            {
                "COGCABRIO",
                "DRIFTFR36", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "EUROSX32", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "EXEMPLAR",
                "F620",
                "FELON",
                "FELON2",
                "FR36", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "JACKAL",
                "KANJOSJ", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "ORACLE",
                "ORACLE2",
                "POSTLUDE", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "PREVION", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "SENTINEL",
                "SENTINEL2",
                "WINDSOR",
                "WINDSOR2",
                "ZION",
                "ZION2",
            };
            #endregion
            #region Muscle
            public static List<string> Muscle { get; } = new List<string>()
            {
                "BLADE",
                "BRIGHAM", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "BROADWAY", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "BUCCANEER",
                "BUCCANEER2",
                "BUFFALO4", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "BUFFALO5", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "CHINO",
                "CHINO2",
                "CLIQUE",
                "CLIQUE2", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "COQUETTE3",
                "DEVIANT",
                "DOMINATOR",
                "DOMINATOR10", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "DOMINATOR2",
                "DOMINATOR3",
                "DOMINATOR4",
                "DOMINATOR5",
                "DOMINATOR6",
                "DOMINATOR7", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "DOMINATOR8", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "DOMINATOR9", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "DRIFTDOMINATOR10", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "DRIFTGAUNTLET4", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "DRIFTYOSEMITE", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "DUKES",
                "DUKES2",
                "DUKES3", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "ELLIE",
                "EUDORA", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "FACTION",
                "FACTION2",
                "FACTION3",
                "GAUNTLET",
                "GAUNTLET2",
                "GAUNTLET3", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "GAUNTLET4", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "GAUNTLET5", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "GREENWOOD", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "HERMES",
                "HOTKNIFE",
                "HUSTLER",
                "IMPALER",
                "IMPALER2",
                "IMPALER3",
                "IMPALER4",
                "IMPALER6", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "IMPERATOR",
                "IMPERATOR2",
                "IMPERATOR3",
                "LURCHER",
                "MANANA2", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "MOONBEAM",
                "MOONBEAM2",
                "NIGHTSHADE",
                "PEYOTE2", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "PHOENIX",
                "PICADOR",
                "RATLOADER",
                "RATLOADER2",
                "RUINER",
                "RUINER2",
                "RUINER3",
                "RUINER4", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "SABREGT",
                "SABREGT2",
                "SLAMVAN",
                "SLAMVAN2",
                "SLAMVAN3",
                "SLAMVAN4",
                "SLAMVAN5",
                "SLAMVAN6",
                "STALION",
                "STALION2",
                "TAHOMA", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "TAMPA",
                "TAMPA3",
                "TAMPA4", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "TULIP",
                "TULIP2", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "VAMOS",
                "VIGERO",
                "VIGERO2", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "VIGERO3", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "VIRGO",
                "VIRGO2",
                "VIRGO3",
                "VOODOO",
                "VOODOO2",
                "WEEVIL2", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "YOSEMITE",
                "YOSEMITE2", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
            };
            #endregion
            #region SportsClassics
            public static List<string> SportsClassics { get; } = new List<string>()
            {
                "ARDENT",
                "BTYPE",
                "BTYPE2",
                "BTYPE3",
                "CASCO",
                "CHEBUREK",
                "CHEETAH2",
                "CHEETAH3", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "COQUETTE2",
                "COQUETTE5", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "DELUXO",
                "DRIFTCHEBUREK", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "DRIFTJESTER3", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "DRIFTNEBULA", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "DYNASTY", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "FAGALOA",
                "FELTZER3", // Stirling GT
                "GT500",
                "INFERNUS2",
                "JB700",
                "JB7002", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "JESTER3",
                "MAMBA",
                "MANANA",
                "MICHELLI",
                "MONROE",
                "NEBULA", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "PEYOTE",
                "PEYOTE3", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "PIGALLE",
                "RAPIDGT3",
                "RETINUE",
                "RETINUE2", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "SAVESTRA",
                "STINGER",
                "STINGERGT",
                "STROMBERG",
                "SWINGER",
                "TOREADOR", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "TORERO",
                "TORNADO",
                "TORNADO2",
                "TORNADO3",
                "TORNADO4",
                "TORNADO5",
                "TORNADO6",
                "TURISMO2",
                "URANUS", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "VISERIS",
                "Z190",
                "ZION3", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "ZTYPE",
            };
            #endregion
            #region Sports
            public static List<string> Sports { get; } = new List<string>()
            {
                "ALPHA",
                "BANSHEE",
                "BANSHEE3", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "BESTIAGTS",
                "BLISTA2",
                "BLISTA3",
                "BUFFALO",
                "BUFFALO2",
                "BUFFALO3",
                "CALICO", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "CARBONIZZARE",
                "COMET2",
                "COMET3",
                "COMET4",
                "COMET5",
                "COMET6", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "COMET7", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "COQUETTE",
                "COQUETTE4", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "COQUETTE6", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "CORSITA", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "COUREUR", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "CYPHER", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "DRIFTCYPHER", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "DRAFTER", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "DRIFTEUROS", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "DRIFTFUTO", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "DRIFTFUTO2", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "DRIFTJESTER", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "DRIFTREMUS", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "DRIFTSENTINEL", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "DRIFTTAMPA", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "DRIFTZR350", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "ELEGY",
                "ELEGY2",
                "ENVISAGE", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "EUROS", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "EVERON2", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "FELTZER2",
                "FLASHGT",
                "FUROREGT",
                "FUSILADE",
                "FUTO",
                "FUTO2", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "GAUNTLET6", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "GB200",
                "GROWLER", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "HOTRING",
                "IMORGON", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "ISSI7", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "ITALIGTO",
                "ITALIRSX", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "JESTER",
                "JESTER2",
                "JESTER4", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "JESTER5", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "JUGULAR", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "KHAMELION",
                "KOMODA", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "KURUMA",
                "KURUMA2",
                "LOCUST", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "LYNX",
                "MASSACRO",
                "MASSACRO2",
                "NEO", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "NEON",
                "NINEF",
                "NINEF2",
                "NIOBE", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "OMNIS",
                "OMNISEGT", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "PANTHERE", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "PARAGON", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "PARAGON2", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "PARAGON3", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "PARIAH",
                "PENUMBRA",
                "PENUMBRA2", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "R300", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "RAIDEN",
                "RAPIDGT",
                "RAPIDGT2",
                "RAPIDGT4", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "RAPTOR",
                "REMUS", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "REVOLTER",
                "RT3000", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "RUSTON",
                "SCHAFTER2",
                "SCHAFTER3",
                "SCHAFTER4",
                "SCHAFTER5",
                "SCHLAGEN",
                "SCHWARZER",
                "SENTINEL3",
                "SENTINEL4", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "SENTINEL5", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "SEVEN70",
                "SM722", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "SPECTER",
                "SPECTER2",
                "STINGERTT", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "SUGOI", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "SULTAN",
                "SULTAN2", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "SULTAN3", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "SURANO",
                "TAMPA2",
                "TENF", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "TENF2", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "TROPOS",
                "VERLIERER2",
                "VECTRE", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "VETO", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "VETO2", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "VSTR", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "ZR350", // LS TUNERS (MPTUNER) DLC - Requires b2372
                "ZR380",
                "ZR3802",
                "ZR3803",
            };
            #endregion
            #region Super
            public static List<string> Super { get; } = new List<string>()
            {
                "ADDER",
                "AUTARCH",
                "BANSHEE2",
                "BULLET",
                "CHAMPION", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "CHEETAH",
                "CYCLONE",
                "DEVESTE",
                "EMERUS", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "ENTITYXF",
                "ENTITY2",
                "ENTITY3", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "FMJ",
                "FURIA", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "GP1",
                "IGNUS", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "INFERNUS",
                "ITALIGTB",
                "ITALIGTB2",
                "KRIEGER", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "LE7B",
                "LM87", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "NERO",
                "NERO2",
                "OSIRIS",
                "PENETRATOR",
                "PFISTER811",
                "PIPISTRELLO", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "PROTOTIPO",
                "REAPER",
                "S80", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "SC1",
                "SCRAMJET",
                "SHEAVA", // ETR1
                "SULTANRS",
                "SUZUME", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "T20",
                "TAIPAN",
                "TEMPESTA",
                "TEZERACT",
                "THRAX", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "TIGON", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "TORERO2", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "TURISMOR",
                "TURISMO3", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "TYRANT",
                "TYRUS",
                "VACCA",
                "VAGNER",
                "VIGILANTE",
                "VIRTUE", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "VISIONE",
                "VOLTIC",
                "VOLTIC2",
                "XA21",
                "ZENO", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "ZENTORNO",
                "ZORRUSSO", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
            };
            #endregion
            #region Motorcycles
            public static List<string> Motorcycles { get; } = new List<string>()
            {
                "AKUMA",
                "AVARUS",
                "BAGGER",
                "BATI",
                "BATI2",
                "BF400",
                "CARBONRS",
                "CHIMERA",
                "CLIFFHANGER",
                "DAEMON",
                "DAEMON2",
                "DEFILER",
                "DEATHBIKE",
                "DEATHBIKE2",
                "DEATHBIKE3",
                "DIABLOUS",
                "DIABLOUS2",
                "DOUBLE",
                "ENDURO",
                "ESSKEY",
                "FAGGIO",
                "FAGGIO2",
                "FAGGIO3",
                "FCR",
                "FCR2",
                "GARGOYLE",
                "HAKUCHOU",
                "HAKUCHOU2",
                "HEXER",
                "INNOVATION",
                "LECTRO",
                "MANCHEZ",
                "MANCHEZ2", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "MANCHEZ3", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "NEMESIS",
                "NIGHTBLADE",
                "OPPRESSOR",
                "OPPRESSOR2",
                "PCJ",
                "PIZZABOY", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "POWERSURGE", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "RATBIKE",
                "REEVER", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "RROCKET", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "RUFFIAN",
                "SANCHEZ",
                "SANCHEZ2",
                "SANCTUS",
                "SHINOBI", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "SHOTARO",
                "SOVEREIGN",
                "STRYDER", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "THRUST",
                "VADER",
                "VINDICATOR",
                "VORTEX",
                "WOLFSBANE",
                "ZOMBIEA",
                "ZOMBIEB",
            };
            #endregion
            #region OffRoad
            public static List<string> OffRoad { get; } = new List<string>()
            {
                "BFINJECTION",
                "BIFTA",
                "BLAZER",
                "BLAZER2",
                "BLAZER3",
                "BLAZER4",
                "BLAZER5",
                "BODHI2",
                "BOOR", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "BRAWLER",
                "BRUISER",
                "BRUISER2",
                "BRUISER3",
                "BRUTUS",
                "BRUTUS2",
                "BRUTUS3",
                "CARACARA",
                "CARACARA2", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "DLOADER",
                "DRAUGUR", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "DRIFTL352", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "DUBSTA3",
                "DUNE",
                "DUNE2",
                "DUNE3",
                "DUNE4",
                "DUNE5",
                "EVERON", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "FIREBOLT", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "FREECRAWLER",
                "HELLION", // CASINO AND RESORT (MPVINEWOOD) DLC - Requires b2060
                "L35", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "INSURGENT",
                "INSURGENT2",
                "INSURGENT3",
                "KALAHARI",
                "KAMACHO",
                "L352", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "MARSHALL",
                "MENACER",
                "MESA3",
                "MONSTER",
                "MONSTER3",
                "MONSTER4",
                "MONSTER5",
                "MONSTROCITI", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "NIGHTSHARK",
                "OUTLAW", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "PATRIOT3", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "RANCHERXL",
                "RANCHERXL2",
                "RATEL", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "RCBANDITO",
                "REBEL",
                "REBEL2",
                "RIATA",
                "SANDKING",
                "SANDKING2",
                "TECHNICAL",
                "TECHNICAL2",
                "TECHNICAL3",
                "TERMINUS", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "TROPHYTRUCK",
                "TROPHYTRUCK2",
                "VAGRANT", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "VERUS", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "WINKY", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "YOSEMITE1500", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "YOSEMITE3", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "ZHABA", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
            };
            #endregion
            #region Industrial
            public static List<string> Industrial { get; } = new List<string>()
            {
                "BULLDOZER",
                "CUTTER",
                "DUMP",
                "FLATBED",
                "FLATBED2", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "GUARDIAN",
                "HANDLER",
                "MIXER",
                "MIXER2",
                "RUBBLE",
                "TIPTRUCK",
                "TIPTRUCK2",
            };
            #endregion
            #region Utility
            public static List<string> Utility { get; } = new List<string>()
            {
                "AIRTUG",
                "CADDY",
                "CADDY2",
                "CADDY3",
                "DOCKTUG",
                "FORKLIFT",
                "MOWER", // Lawnmower
                "RIPLEY",
                "SADLER",
                "SADLER2",
                "SCRAP",
                "SLAMTRUCK", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "TOWTRUCK",
                "TOWTRUCK2",
                "TOWTRUCK3", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "TOWTRUCK4", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "TRACTOR", // Tractor (rusted/old)
                "TRACTOR2", // Fieldmaster
                "TRACTOR3", // Fieldmaster
                "UTILLITRUCK",
                "UTILLITRUCK2",
                "UTILLITRUCK3",

                /// Trailers

                /// Army Trailers
                "ARMYTRAILER", // Military
                "ARMYTRAILER2", // Civillian
                "FREIGHTTRAILER", // Extended
                "ARMYTANKER", // Army Tanker
                "TRAILERLARGE", // Mobile Operations Center
            
                /// Large Trailers
                "DOCKTRAILER", // Shipping Container Trailer
                "TR3", // Large Boat Trailer (Sailboat)
                "TR2", // Large Vehicle Trailer
                "TR4", // Large Vehicle Trailer (Mission Cars)
                "TRFLAT", // Large Flatbed Empty Trailer
                "TRAILERS", // Container/Curtain Trailer
                "TRAILERS4", // White Container Trailer
                "TRAILERS2", // Box Trailer
                "TRAILERS3", // Ramp Box Trailer
                "TRAILERS5", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "TVTRAILER", // Fame or Shame Trailer
                "TVTRAILER2", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "TRAILERLOGS", // Logs Trailer
                "TANKER", // Ron Oil Tanker Trailer
                "TANKER2", // Ron Oil Tanker Trailer (Heist Version)
            
                /// Medium Trailers
                "BALETRAILER", // (Tractor Hay Bale Trailer)
                "GRAINTRAILER", // (Tractor Grain Trailer)
            
                // Ortega's trailer, we don't want this one because you can't drive them.
                //"PROPTRAILER",

                /// Small Trailers
                "BOATTRAILER", // Small Boat Trailer
                "BOATTRAILER2", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "BOATTRAILER3", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "RAKETRAILER", // Tractor Tow Plow/Rake
                "TRAILERSMALL", // Small Utility Trailer
            };
            #endregion
            #region Vans
            public static List<string> Vans { get; } = new List<string>()
            {
                "BISON",
                "BISON2",
                "BISON3",
                "BOBCATXL",
                "BOXVILLE",
                "BOXVILLE2",
                "BOXVILLE3",
                "BOXVILLE4",
                "BOXVILLE5",
                "BOXVILLE6", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "BURRITO",
                "BURRITO2",
                "BURRITO3",
                "BURRITO4",
                "BURRITO5",
                "CAMPER",
                "GBURRITO",
                "GBURRITO2",
                "JOURNEY",
                "JOURNEY2", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "MINIVAN",
                "MINIVAN2",
                "PARADISE",
                "PONY",
                "PONY2",
                "RUMPO",
                "RUMPO2",
                "RUMPO3",
                "SPEEDO",
                "SPEEDO2",
                "SPEEDO4",
                "SPEEDO5", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "SURFER",
                "SURFER2",
                "SURFER3", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "TACO",
                "YOUGA",
                "YOUGA2",
                "YOUGA3", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "YOUGA4", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "YOUGA5", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
            };
            #endregion
            #region Cycles
            public static List<string> Cycles { get; } = new List<string>()
            {
                "BMX",
                "CRUISER",
                "FIXTER",
                "INDUCTOR", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "INDUCTOR2", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "SCORCHER",
                "TRIBIKE",
                "TRIBIKE2",
                "TRIBIKE3",
            };
            #endregion
            #region Boats
            public static List<string> Boats { get; } = new List<string>()
            {
                "AVISA", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "DINGHY",
                "DINGHY2",
                "DINGHY3",
                "DINGHY4",
                "DINGHY5", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "JETMAX",
                "KOSATKA", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "LONGFIN", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "MARQUIS",
                "PATROLBOAT", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "PREDATOR",
                "SEASHARK",
                "SEASHARK2",
                "SEASHARK3",
                "SPEEDER",
                "SPEEDER2",
                "SQUALO",
                "SUBMERSIBLE",
                "SUBMERSIBLE2",
                "SUNTRAP",
                "TORO",
                "TORO2",
                "TROPIC",
                "TROPIC2",
                "TUG",
            };
            #endregion
            #region Helicopters
            public static List<string> Helicopters { get; } = new List<string>()
            {
                "AKULA",
                "ANNIHILATOR",
                "ANNIHILATOR2", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "BUZZARD",
                "BUZZARD2",
                "CARGOBOB",
                "CARGOBOB2",
                "CARGOBOB3",
                "CARGOBOB4",
                "CARGOBOB5", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "CONADA", // CRIMINAL ENTERPRISES (MPSUM2) DLC - Requires b2699
                "CONADA2", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "FROGGER",
                "FROGGER2",
                "HAVOK",
                "HUNTER",
                "MAVERICK",
                "MAVERICK2", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "POLMAV",
                "SAVAGE",
                "SEASPARROW",
                "SEASPARROW2", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "SEASPARROW3", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "SKYLIFT",
                "SUPERVOLITO",
                "SUPERVOLITO2",
                "SWIFT",
                "SWIFT2",
                "VALKYRIE",
                "VALKYRIE2",
                "VOLATUS",
            };
            #endregion
            #region Planes
            public static List<string> Planes { get; } = new List<string>()
            {
                "ALKONOST", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
                "ALPHAZ1",
                "AVENGER",
                "AVENGER2",
                "AVENGER3", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "AVENGER4", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "BESRA",
                "BLIMP",
                "BLIMP2",
                "BLIMP3",
                "BOMBUSHKA",
                "CARGOPLANE",
                "CARGOPLANE2", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "CUBAN800",
                "DODO",
                "DUSTER",
                "DUSTER2", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "HOWARD",
                "HYDRA",
                "JET",
                "LAZER",
                "LUXOR",
                "LUXOR2",
                "MAMMATUS",
                "MICROLIGHT",
                "MILJET",
                "MOGUL",
                "MOLOTOK",
                "NIMBUS",
                "NOKOTA",
                "PYRO",
                "RAIJU", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "ROGUE",
                "SEABREEZE",
                "SHAMAL",
                "STARLING",
                "STREAMER216", // SA MERCENARIES (MP2023_01) DLC - Requires b2944
                "STRIKEFORCE",
                "STUNT",
                "TITAN",
                "TITAN2", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "TULA",
                "VELUM",
                "VELUM2",
                "VESTRA",
                "VOLATOL",
            };
            #endregion
            #region Service
            public static List<string> Service { get; } = new List<string>()
            {
                "AIRBUS",
                "BRICKADE",
                "BRICKADE2", // DRUG WARS (MPCHRISTMAS3) DLC - Requires b2802
                "BUS",
                "COACH",
                "PBUS2",
                "RALLYTRUCK",
                "RENTALBUS",
                "TAXI",
                "TOURBUS",
                "TRASH",
                "TRASH2",
                "WASTELANDER",
            };
            #endregion
            #region Emergency
            public static List<string> Emergency { get; } = new List<string>()
            {
                "AMBULANCE",
                "FBI",
                "FBI2",
                "FIRETRUK",
                "LGUARD",
                "PBUS",
                "POLCARACARA", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "POLCOQUETTE4", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "POLDOMINATOR10", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "POLDORADO", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "POLFACTION2", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "POLGAUNTLET", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "POLGREENWOOD", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "POLICE",
                "POLICE2",
                "POLICE3",
                "POLICE4",
                "POLICE5", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "POLICEB",
                "POLICEB2", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "POLICEOLD1",
                "POLICEOLD2",
                "POLICET",
                "POLICET3", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "POLIMPALER5", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "POLIMPALER6", // BOTTOM DOLLAR BOUNTIES (MP2024_01) DLC - Requires b3258
                "POLMAV",
                "POLTERMINUS", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "PRANGER",
                "PREDATOR",
                "RIOT",
                "RIOT2",
                "SHERIFF",
                "SHERIFF2",
            };
            #endregion
            #region Military
            public static List<string> Military { get; } = new List<string>()
            {
                "APC",
                "BARRACKS",
                "BARRACKS2",
                "BARRACKS3",
                "BARRAGE",
                "CHERNOBOG",
                "CRUSADER",
                "HALFTRACK",
                "KHANJALI",
                "MINITANK", // CASINO HEIST (MPHEIST3) DLC - Requires b2060
                "RHINO",
                "SCARAB",
                "SCARAB2",
                "SCARAB3",
                "THRUSTER", // Jetpack
                "TRAILERSMALL2", // Anti Aircraft Trailer
                "VETIR", // CAYO PERICO (MPHEIST4) DLC - Requires b2189
            };
            #endregion
            #region Commercial
            public static List<string> Commercial { get; } = new List<string>()
            {
                "BENSON",
                "BENSON2", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "BIFF",
                "CERBERUS",
                "CERBERUS2",
                "CERBERUS3",
                "HAULER",
                "HAULER2",
                "MULE",
                "MULE2",
                "MULE3",
                "MULE4",
                "MULE5", // THE CONTRACT (MPSECURITY) DLC - Requires b2545
                "PACKER",
                "PHANTOM",
                "PHANTOM2",
                "PHANTOM3",
                "PHANTOM4", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "POUNDER",
                "POUNDER2",
                "STOCKADE",
                "STOCKADE3",
                "STOCKADE4", // MONEY FRONTS (MP2025_01) DLC - Requires b3570
                "TERBYTE",
            };
            #endregion
            #region Trains
            public static List<string> Trains { get; } = new List<string>()
            {
                "CABLECAR",
                "FREIGHT",
                "FREIGHT2", // CHOP SHOP (MP2023_02) DLC - Requires b3095
                "FREIGHTCAR",
                "FREIGHTCAR3", // AGENTS OF SABOTAGE (MP2024_02) DLC - Requires b3407
                "FREIGHTCONT1",
                "FREIGHTCONT2",
                "FREIGHTGRAIN",
                "METROTRAIN",
                "TANKERCAR",
            };
            #endregion
            #region OpenWheel
            public static List<string> OpenWheel { get; } = new List<string>()
            {
                "FORMULA",
                "FORMULA2",
                "OPENWHEEL1", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
                "OPENWHEEL2", // SUMMER SPECIAL (MPSUM) DLC - Requires b2060
            };
            #endregion


            /*
            Compacts = 0,
            Sedans = 1,
            SUVs = 2,
            Coupes = 3,
            Muscle = 4,
            SportsClassics = 5,
            Sports = 6,
            Super = 7,
            Motorcycles = 8,
            OffRoad = 9,
            Industrial = 10,
            Utility = 11,
            Vans = 12,
            Cycles = 13,
            Boats = 14,
            Helicopters = 15,
            Planes = 16,
            Service = 17,
            Emergency = 18,
            Military = 19,
            Commercial = 20,
            Trains = 21
            OpenWheel = 22
            */

            public static Dictionary<string, List<string>> VehicleClasses { get; } = new Dictionary<string, List<string>>()
            {
                [GetLabelText("VEH_CLASS_0")] = Compacts,
                [GetLabelText("VEH_CLASS_1")] = Sedans,
                [GetLabelText("VEH_CLASS_2")] = SUVs,
                [GetLabelText("VEH_CLASS_3")] = Coupes,
                [GetLabelText("VEH_CLASS_4")] = Muscle,
                [GetLabelText("VEH_CLASS_5")] = SportsClassics,
                [GetLabelText("VEH_CLASS_6")] = Sports,
                [GetLabelText("VEH_CLASS_7")] = Super,
                [GetLabelText("VEH_CLASS_8")] = Motorcycles,
                [GetLabelText("VEH_CLASS_9")] = OffRoad,
                [GetLabelText("VEH_CLASS_10")] = Industrial,
                [GetLabelText("VEH_CLASS_11")] = Utility,
                [GetLabelText("VEH_CLASS_12")] = Vans,
                [GetLabelText("VEH_CLASS_13")] = Cycles,
                [GetLabelText("VEH_CLASS_14")] = Boats,
                [GetLabelText("VEH_CLASS_15")] = Helicopters,
                [GetLabelText("VEH_CLASS_16")] = Planes,
                [GetLabelText("VEH_CLASS_17")] = Service,
                [GetLabelText("VEH_CLASS_18")] = Emergency,
                [GetLabelText("VEH_CLASS_19")] = Military,
                [GetLabelText("VEH_CLASS_20")] = Commercial,
                [GetLabelText("VEH_CLASS_21")] = Trains,
                [GetLabelText("VEH_CLASS_22")] = OpenWheel,
            };
            #endregion

            public static string[] GetAllVehicles()
            {
                var vehs = new List<string>();
                foreach (var vc in VehicleClasses)
                {
                    foreach (var c in vc.Value)
                    {
                        vehs.Add(c);
                    }
                }
                return vehs.ToArray();
            }
        }

        /// <summary>
        /// Values taken from `SET_VEHICLE_MOD` native
        /// </summary>
        public enum ModType
        {
            VMT_SPOILER = 0,
            VMT_BUMPER_F = 1,
            VMT_BUMPER_R = 2,
            VMT_SKIRT = 3,
            VMT_EXHAUST = 4,
            VMT_CHASSIS = 5,
            VMT_GRILL = 6,
            VMT_BONNET = 7,
            VMT_WING_L = 8,
            VMT_WING_R = 9,
            VMT_ROOF = 10,
            VMT_ENGINE = 11,
            VMT_BRAKES = 12,
            VMT_GEARBOX = 13,
            VMT_HORN = 14,
            VMT_SUSPENSION = 15,
            VMT_ARMOUR = 16,
            VMT_NITROUS = 17,
            VMT_TURBO = 18,
            VMT_SUBWOOFER = 19,
            VMT_TYRE_SMOKE = 20,
            VMT_HYDRAULICS = 21,
            VMT_XENON_LIGHTS = 22,
            VMT_WHEELS = 23,
            VMT_WHEELS_REAR_OR_HYDRAULICS = 24,
            VMT_PLTHOLDER = 25,
            VMT_PLTVANITY = 26,
            VMT_INTERIOR1 = 27,
            VMT_INTERIOR2 = 28,
            VMT_INTERIOR3 = 29,
            VMT_INTERIOR4 = 30,
            VMT_INTERIOR5 = 31,
            VMT_SEATS = 32,
            VMT_STEERING = 33,
            VMT_KNOB = 34,
            VMT_PLAQUE = 35,
            VMT_ICE = 36,
            VMT_TRUNK = 37,
            VMT_HYDRO = 38,
            VMT_ENGINEBAY1 = 39,
            VMT_ENGINEBAY2 = 40,
            VMT_ENGINEBAY3 = 41,
            VMT_CHASSIS2 = 42,
            VMT_CHASSIS3 = 43,
            VMT_CHASSIS4 = 44,
            VMT_CHASSIS5 = 45,
            VMT_DOOR_L = 46,
            VMT_DOOR_R = 47,
            VMT_LIVERY_MOD = 48,
            VMT_LIGHTBAR = 49,
        }
    }
}
