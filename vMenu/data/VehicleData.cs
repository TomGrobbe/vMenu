using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public static class VehicleData
    {
        public struct VehicleColor
        {
            public readonly int id;
            public readonly string label;

            public VehicleColor(int id, string label)
            {
                if (label == "veh_color_taxi_yellow")
                {
                    if (CitizenFX.Core.Native.API.GetLabelText("veh_color_taxi_yellow") == "NULL")
                    {
                        CitizenFX.Core.Native.API.AddTextEntry("veh_color_taxi_yellow", $"Taxi {CitizenFX.Core.Native.API.GetLabelText("IEC_T20_2")}");
                    }
                }
                else if (label == "veh_color_off_white")
                {
                    if (CitizenFX.Core.Native.API.GetLabelText("veh_color_off_white") == "NULL")
                    {
                        CitizenFX.Core.Native.API.AddTextEntry("veh_color_off_white", "Off White");
                    }
                }
                else if (label == "VERY_DARK_BLUE")
                {
                    if (CitizenFX.Core.Native.API.GetLabelText("VERY_DARK_BLUE") == "NULL")
                    {
                        CitizenFX.Core.Native.API.AddTextEntry("VERY_DARK_BLUE", "Very Dark Blue");
                    }
                }

                this.label = label;
                this.id = id;
            }
        }

        public static readonly List<VehicleColor> ClassicColors = new List<VehicleColor>()
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

        public static readonly List<VehicleColor> MatteColors = new List<VehicleColor>()
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

        public static readonly List<VehicleColor> MetalColors = new List<VehicleColor>()
        {
            new VehicleColor(117, "BR_STEEL"),
            new VehicleColor(118, "BR BLACK_STEEL"),
            new VehicleColor(119, "BR_ALUMINIUM"),

            new VehicleColor(158, "GOLD_P"),
            new VehicleColor(159, "GOLD_S"),
        };

        public static readonly List<VehicleColor> UtilColors = new List<VehicleColor>()
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

        public static readonly List<VehicleColor> WornColors = new List<VehicleColor>()
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

        public static class Vehicles
        {
            #region Vehicle List Per Class

            #region Compacts
            public static List<string> Compacts { get; } = new List<string>()
            {
                "ASBO", // CASINO (MPHEIST3) DLC
                "BLISTA",
                "BRIOSO",
                "BRIOSO2", // MPCAYOPERICO DLC
                "CLUB", // MPSUM2020
                "DILETTANTE",
                "DILETTANTE2",
                "ISSI2",
                "ISSI3",
                "ISSI4", // MPCHRISTMAS2018 DLC
                "ISSI5", // MPCHRISTMAS2018 DLC
                "ISSI6", // MPCHRISTMAS2018 DLC
                "KANJO", // CASINO (MPHEIST3) DLC
                "PANTO",
                "PRAIRIE",
                "RHAPSODY",
                "WEEVIL", // MPCAYOPERICO DLC
            };
            #endregion
            #region Sedans
            public static List<string> Sedans { get; } = new List<string>()
            {
                "ASEA",
                "ASEA2",
                "ASTEROPE",
                "COG55",
                "COG552",
                "COGNOSCENTI",
                "COGNOSCENTI2",
                "EMPEROR",
                "EMPEROR2",
                "EMPEROR3",
                "FUGITIVE",
                "GLENDALE",
                "GLENDALE2", // MPSUM2020
                "INGOT",
                "INTRUDER",
                "LIMO2",
                "PREMIER",
                "PRIMO",
                "PRIMO2",
                "REGINA",
                "ROMERO",
                "SCHAFTER2",
                "SCHAFTER5",
                "SCHAFTER6",
                "STAFFORD", // dlc (mpbattle)
                "STANIER",
                "STRATUM",
                "STRETCH",
                "SUPERD",
                "SURGE",
                "TAILGATER",
                "WARRENER",
                "WASHINGTON",
            };
            #endregion
            #region SUVs
            public static List<string> SUVs { get; } = new List<string>()
            {
                "BALLER",
                "BALLER2",
                "BALLER3",
                "BALLER4",
                "BALLER5",
                "BALLER6",
                "BJXL",
                "CAVALCADE",
                "CAVALCADE2",
                "CONTENDER",
                "DUBSTA",
                "DUBSTA2",
                "FQ2",
                "GRANGER",
                "GRESLEY",
                "HABANERO",
                "HUNTLEY",
                "LANDSTALKER",
                "LANDSTALKER2", // MPSUM2020
                "MESA",
                "MESA2",
                "NOVAK", // MPVINEWOOD DLC
                "PATRIOT",
                "PATRIOT2", // dlc (mpbattle)
                "RADI",
                "REBLA", // CASINO (MPHEIST3) DLC
                "ROCOTO",
                "SEMINOLE",
                "SEMINOLE2", // MPSUM2020
                "SERRANO",
                "SQUADDIE", // MPCAYOPERICO DLC
                "TOROS", // MPCHRISTMAS2018 DLC
                "XLS",
                "XLS2",
            };
            #endregion
            #region Coupes
            public static List<string> Coupes { get; } = new List<string>()
            {
                "COGCABRIO",
                "EXEMPLAR",
                "F620",
                "FELON",
                "FELON2",
                "JACKAL",
                "ORACLE",
                "ORACLE2",
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
                "BUCCANEER",
                "BUCCANEER2",
                "CHINO",
                "CHINO2",
                "CLIQUE", // MPCHRISTMAS2018 DLC
                "COQUETTE3",
                "DEVIANT", // MPCHRISTMAS2018 DLC
                "DOMINATOR",
                "DOMINATOR2",
                "DOMINATOR3",
                "DOMINATOR4", // MPCHRISTMAS2018 DLC
                "DOMINATOR5", // MPCHRISTMAS2018 DLC
                "DOMINATOR6", // MPCHRISTMAS2018 DLC
                "DUKES",
                "DUKES2",
                "DUKES3", // MPSUM2020
                "ELLIE",
                "FACTION",
                "FACTION2",
                "FACTION3",
                "GAUNTLET",
                "GAUNTLET2",
                "GAUNTLET3", // MPVINEWOOD DLC
                "GAUNTLET4", // MPVINEWOOD DLC
                "GAUNTLET5", // MPSUM2020
                "HERMES",
                "HOTKNIFE",
                "HUSTLER",
                "IMPALER", // MPCHRISTMAS2018 DLC
                "IMPALER2", // MPCHRISTMAS2018 DLC
                "IMPALER3", // MPCHRISTMAS2018 DLC
                "IMPALER4", // MPCHRISTMAS2018 DLC
                "IMPERATOR", // MPCHRISTMAS2018 DLC
                "IMPERATOR2", // MPCHRISTMAS2018 DLC
                "IMPERATOR3", // MPCHRISTMAS2018 DLC
                "LURCHER",
                "MANANA2", // MPSUM2020
                "MOONBEAM",
                "MOONBEAM2",
                "NIGHTSHADE",
                "PEYOTE2", // MPVINEWOOD DLC
                "PHOENIX",
                "PICADOR",
                "RATLOADER",
                "RATLOADER2",
                "RUINER",
                "RUINER2",
                "RUINER3",
                "SABREGT",
                "SABREGT2",
                "SLAMVAN",
                "SLAMVAN2",
                "SLAMVAN3",
                "SLAMVAN4", // MPCHRISTMAS2018 DLC
                "SLAMVAN5", // MPCHRISTMAS2018 DLC
                "SLAMVAN6", // MPCHRISTMAS2018 DLC
                "STALION",
                "STALION2",
                "TAMPA",
                "TAMPA3",
                "TULIP", // MPCHRISTMAS2018 DLC
                "VAMOS", // MPCHRISTMAS2018 DLC
                "VIGERO",
                "VIRGO",
                "VIRGO2",
                "VIRGO3",
                "VOODOO",
                "VOODOO2",
                "YOSEMITE",
                "YOSEMITE2", // CASINO (MPHEIST3) DLC
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
                "COQUETTE2",
                "DELUXO",
                "DYNASTY", // MPVINEWOOD DLC
                "FAGALOA",
                "FELTZER3", // Stirling GT
                "GT500",
                "INFERNUS2",
                "JB700",
                "JB7002", // CASINO (MPHEIST3) DLC
                "JESTER3",
                "MAMBA",
                "MANANA",
                "MICHELLI",
                "MONROE",
                "NEBULA", // MPVINEWOOD DLC
                "PEYOTE",
                "PEYOTE3", // MPSUM2020
                "PIGALLE",
                "RAPIDGT3",
                "RETINUE",
                "RETINUE2", // CASINO (MPHEIST3) DLC
                "SAVESTRA",
                "STINGER",
                "STINGERGT",
                "STROMBERG",
                "SWINGER", // dlc (mpbattle)
                "TOREADOR", // MPCAYOPERICO DLC
                "TORERO",
                "TORNADO",
                "TORNADO2",
                "TORNADO3",
                "TORNADO4",
                "TORNADO5",
                "TORNADO6",
                "TURISMO2",
                "VISERIS",
                "Z190",
                "ZION3", // MPVINEWOOD DLC
                "ZTYPE",
            };
            #endregion
            #region Sports
            public static List<string> Sports { get; } = new List<string>()
            {
                "ALPHA",
                "BANSHEE",
                "BESTIAGTS",
                "BLISTA2",
                "BLISTA3",
                "BUFFALO",
                "BUFFALO2",
                "BUFFALO3",
                "CARBONIZZARE",
                "COMET2",
                "COMET3",
                "COMET4",
                "COMET5",
                "COQUETTE",
                "COQUETTE4", // MPSUM2020
                "DRAFTER", // MPVINEWOOD DLC
                "ELEGY",
                "ELEGY2",
                "FELTZER2",
                "FLASHGT",
                "FUROREGT",
                "FUSILADE",
                "FUTO",
                "GB200",
                "HOTRING",
                "IMORGON", // CASINO (MPHEIST3) DLC
                "ISSI7", // MPVINEWOOD DLC
                "ITALIGTO", // MPCHRISTMAS2018 DLC
                "ITALIRSX", // MPCAYOPERICO DLC
                "JESTER",
                "JESTER2",
                "JUGULAR", // MPVINEWOOD DLC
                "KHAMELION",
                "KOMODA", // CASINO (MPHEIST3) DLC
                "KURUMA",
                "KURUMA2",
                "LOCUST", // MPVINEWOOD DLC
                "LYNX",
                "MASSACRO",
                "MASSACRO2",
                "NEO", // MPVINEWOOD DLC
                "NEON", // Doomsday Heist Update
                "NINEF",
                "NINEF2",
                "OMNIS",
                "PARAGON", // MPVINEWOOD DLC
                "PARAGON2", // MPVINEWOOD DLC
                "PARIAH",
                "PENUMBRA",
                "PENUMBRA2", // MPSUM2020
                "RAIDEN",
                "RAPIDGT",
                "RAPIDGT2",
                "RAPTOR",
                "REVOLTER",
                "RUSTON",
                "SCHAFTER2",
                "SCHAFTER3",
                "SCHAFTER4",
                "SCHAFTER5",
                "SCHLAGEN", // MPCHRISTMAS2018 DLC
                "SCHWARZER",
                "SENTINEL3",
                "SEVEN70",
                "SPECTER",
                "SPECTER2",
                "SUGOI", // CASINO (MPHEIST3) DLC
                "SULTAN",
                "SULTAN2", // CASINO (MPHEIST3) DLC
                "SURANO",
                "TAMPA2",
                "TROPOS",
                "VERLIERER2",
                "VETO", // MPCAYOPERICO DLC
                "VETO2", // MPCAYOPERICO DLC
                "VSTR", // CASINO (MPHEIST3) DLC
                "ZR380", // MPCHRISTMAS2018 DLC
                "ZR3802", // MPCHRISTMAS2018 DLC
                "ZR3803", // MPCHRISTMAS2018 DLC
            };
            #endregion
            #region Super
            public static List<string> Super { get; } = new List<string>()
            {
                "ADDER",
                "AUTARCH",
                "BANSHEE2",
                "BULLET",
                "CHEETAH",
                "CYCLONE",
                "DEVESTE", // MPCHRISTMAS2018 DLC
                "EMERUS", // MPVINEWOOD DLC
                "ENTITYXF",
                "ENTITY2",
                "FMJ",
                "FURIA", // CASINO (MPHEIST3) DLC
                "GP1",
                "INFERNUS",
                "ITALIGTB",
                "ITALIGTB2",
                "KRIEGER", // MPVINEWOOD DLC
                "LE7B",
                "NERO",
                "NERO2",
                "OSIRIS",
                "PENETRATOR",
                "PFISTER811",
                "PROTOTIPO",
                "REAPER",
                "S80", // MPVINEWOOD DLC
                "SC1",
                "SCRAMJET", // dlc (mpbattle)
                "SHEAVA", // ETR1
                "SULTANRS",
                "T20",
                "TAIPAN",
                "TEMPESTA",
                "TEZERACT",
                "THRAX", // MPVINEWOOD DLC
                "TIGON", // MPSUM2020
                "TURISMOR",
                "TYRANT",
                "TYRUS",
                "VACCA",
                "VAGNER",
                "VIGILANTE",
                "VISIONE",
                "VOLTIC",
                "VOLTIC2",
                "XA21",
                "ZENTORNO",
                "ZORRUSSO", // MPVINEWOOD DLC
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
                "DEATHBIKE", // MPCHRISTMAS2018 DLC
                "DEATHBIKE2", // MPCHRISTMAS2018 DLC
                "DEATHBIKE3", // MPCHRISTMAS2018 DLC
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
                "MANCHEZ2", // MPCAYOPERICO DLC
                "NEMESIS",
                "NIGHTBLADE",
                "OPPRESSOR",
                "OPPRESSOR2", // dlc (mpbattle)
                "PCJ",
                "RATBIKE",
                "RROCKET", // MPVINEWOOD DLC
                "RUFFIAN",
                "SANCHEZ",
                "SANCHEZ2",
                "SANCTUS",
                "SHOTARO",
                "SOVEREIGN",
                "STRYDER", // CASINO (MPHEIST3) DLC
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
                "BRAWLER",
                "BRUISER", // MPCHRISTMAS2018 DLC
                "BRUISER2", // MPCHRISTMAS2018 DLC
                "BRUISER3", // MPCHRISTMAS2018 DLC
                "BRUTUS", // MPCHRISTMAS2018 DLC
                "BRUTUS2", // MPCHRISTMAS2018 DLC
                "BRUTUS3", // MPCHRISTMAS2018 DLC
                "CARACARA",
                "CARACARA2", // MPVINEWOOD DLC
                "DLOADER",
                "DUBSTA3",
                "DUNE",
                "DUNE2",
                "DUNE3",
                "DUNE4",
                "DUNE5",
                "EVERON", // CASINO (MPHEIST3) DLC
                "FREECRAWLER", // dlc (mpbattle)
                "HELLION", // MPVINEWOOD DLC
                "INSURGENT",
                "INSURGENT2",
                "INSURGENT3",
                "KALAHARI",
                "KAMACHO",
                "MARSHALL",
                "MENACER", // dlc (mpbattle)
                "MESA3",
                "MONSTER",
                "MONSTER3", // MPCHRISTMAS2018 DLC
                "MONSTER4", // MPCHRISTMAS2018 DLC
                "MONSTER5", // MPCHRISTMAS2018 DLC
                "NIGHTSHARK",
                "OUTLAW", // CASINO (MPHEIST3) DLC
                "RANCHERXL",
                "RANCHERXL2",
                "RCBANDITO", // MPCHRISTMAS2018 DLC
                "REBEL",
                "REBEL2",
                "RIATA",
                "SANDKING",
                "SANDKING2",
                "TECHNICAL",
                "TECHNICAL2",
                "TECHNICAL3",
                "TROPHYTRUCK",
                "TROPHYTRUCK2",
                "VAGRANT", // CASINO (MPHEIST3) DLC
                "VERUS", // MPCAYOPERICO DLC
                "WINKY", // MPCAYOPERICO DLC
                "YOSEMITE3", // MPSUM2020
                "ZHABA", // CASINO (MPHEIST3) DLC
            };
            #endregion
            #region Industrial
            public static List<string> Industrial { get; } = new List<string>()
            {
                "BULLDOZER",
                "CUTTER",
                "DUMP",
                "FLATBED",
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
                "TRACTOR2", // Fieldmaster
                "TRACTOR3", // Fieldmaster
                "MOWER", // Lawnmower
                "RIPLEY",
                "SADLER",
                "SADLER2",
                "SCRAP",
                "SLAMTRUCK", // MPCAYOPERICO DLC
                "TOWTRUCK",
                "TOWTRUCK2",
                "TRACTOR", // Tractor (rusted/old)
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
                "TVTRAILER", // Fame or Shame Trailer
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
                "BURRITO",
                "BURRITO2",
                "BURRITO3",
                "BURRITO4",
                "BURRITO5",
                "CAMPER",
                "GBURRITO",
                "GBURRITO2",
                "JOURNEY",
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
                "SPEEDO4", // dlc (mpbattle)
                "SURFER",
                "SURFER2",
                "TACO",
                "YOUGA",
                "YOUGA2",
                "YOUGA3", // MPSUM2020
            };
            #endregion
            #region Cycles
            public static List<string> Cycles { get; } = new List<string>()
            {
                "BMX",
                "CRUISER",
                "FIXTER",
                "SCORCHER",
                "TRIBIKE",
                "TRIBIKE2",
                "TRIBIKE3",
            };
            #endregion
            #region Boats
            public static List<string> Boats { get; } = new List<string>()
            {
                "AVISA", // MPCAYOPERICO DLC
                "DINGHY",
                "DINGHY2",
                "DINGHY3",
                "DINGHY4",
                "DINGHY5", // MPCAYOPERICO DLC
                "JETMAX",
                "KOSATKA", // MPCAYOPERICO DLC
                "LONGFIN", // MPCAYOPERICO DLC
                "MARQUIS",
                "PATROLBOAT", // MPCAYOPERICO DLC
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
                "ANNIHILATOR2", // MPCAYOPERICO DLC
                "BUZZARD",
                "BUZZARD2",
                "CARGOBOB",
                "CARGOBOB2",
                "CARGOBOB3",
                "CARGOBOB4",
                "FROGGER",
                "FROGGER2",
                "HAVOK",
                "HUNTER",
                "MAVERICK",
                "POLMAV",
                "SAVAGE",
                "SEASPARROW",
                "SEASPARROW2", // MPCAYOPERICO DLC
                "SEASPARROW3", // MPCAYOPERICO DLC
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
                "ALKONOST", // MPCAYOPERICO DLC
                "ALPHAZ1",
                "AVENGER",
                "AVENGER2",
                "BESRA",
                "BLIMP",
                "BLIMP2",
                "BLIMP3", // dlc (mpbattle)
                "BOMBUSHKA",
                "CARGOPLANE",
                "CUBAN800",
                "DODO",
                "DUSTER",
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
                "ROGUE",
                "SEABREEZE",
                "SHAMAL",
                "STARLING",
                "STRIKEFORCE", // dlc (mpbattle)
                "STUNT",
                "TITAN",
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
                "BUS",
                "COACH",
                "PBUS2", // dlc (mpbattle)
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
                "POLICE",
                "POLICE2",
                "POLICE3",
                "POLICE4",
                "POLICEB",
                "POLICEOLD1",
                "POLICEOLD2",
                "POLICET",
                "POLMAV",
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
                "MINITANK", // CASINO (MPHEIST3) DLC
                "RHINO",
                "SCARAB", // MPCHRISTMAS2018 DLC
                "SCARAB2", // MPCHRISTMAS2018 DLC
                "SCARAB3", // MPCHRISTMAS2018 DLC
                "THRUSTER", // Jetpack
                "TRAILERSMALL2", // Anti Aircraft Trailer
                "VETIR", // MPCAYOPERICO DLC
            };
            #endregion
            #region Commercial
            public static List<string> Commercial { get; } = new List<string>()
            {
                "BENSON",
                "BIFF",
                "CERBERUS", // MPCHRISTMAS2018 DLC
                "CERBERUS2", // MPCHRISTMAS2018 DLC
                "CERBERUS3", // MPCHRISTMAS2018 DLC
                "HAULER",
                "HAULER2",
                "MULE",
                "MULE2",
                "MULE3",
                "MULE4", // dlc (mpbattle)
                "PACKER",
                "PHANTOM",
                "PHANTOM2",
                "PHANTOM3",
                "POUNDER",
                "POUNDER2", // dlc (mpbattle)
                "STOCKADE",
                "STOCKADE3",
                "TERBYTE", // dlc (mpbattle)
            };
            #endregion
            #region Trains
            public static List<string> Trains { get; } = new List<string>()
            {
                "CABLECAR",
                "FREIGHT",
                "FREIGHTCAR",
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
                "OPENWHEEL1", // MPSUM2020
                "OPENWHEEL2", // MPSUM2020
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
                List<string> vehs = new List<string>();
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
    }
}
