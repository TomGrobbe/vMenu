using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMenuClient
{
    public class Vehicles
    {
        #region Vehicle List Per Class

        #region Compacts
        public static List<string> Compacts { get; } = new List<string>()
        {
            "BLISTA",
            "BRIOSO",
            "DILETTANTE",
            "DILETTANTE2",
            "ISSI2",
            "ISSI3",
            "PANTO",
            "PRAIRIE",
            "RHAPSODY",
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
            "STAFFORD",
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
            "MESA",
            "MESA2",
            "PATRIOT",
            "PATRIOT2",
            "RADI",
            "ROCOTO",
            "SEMINOLE",
            "SERRANO",
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
            "COQUETTE3",
            "DOMINATOR",
            "DOMINATOR2",
            "DOMINATOR3",
            "DUKES",
            "DUKES2",
            "ELLIE",
            "FACTION",
            "FACTION2",
            "FACTION3",
            "GAUNTLET",
            "GAUNTLET2",
            "HERMES",
            "HOTKNIFE",
            "HUSTLER",
            "LURCHER",
            "MOONBEAM",
            "MOONBEAM2",
            "NIGHTSHADE",
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
            "STALION",
            "STALION2",
            "TAMPA",
            "TAMPA3",
            "VIGERO",
            "VIRGO",
            "VIRGO2",
            "VIRGO3",
            "VOODOO",
            "VOODOO2",
            "YOSEMITE",
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
            "FAGALOA",
            "FELTZER3", // Stirling GT
            "GT500",
            "INFERNUS2",
            "JB700",
            "JESTER3",
            "MAMBA",
            "MANANA",
            "MICHELLI",
            "MONROE",
            "PEYOTE",
            "PIGALLE",
            "RAPIDGT3",
            "RETINUE",
            "SAVESTRA",
            "STINGER",
            "STINGERGT",
            "STROMBERG",
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
            "ELEGY",
            "ELEGY2",
            "FELTZER2",
            "FLASHGT",
            "FUROREGT",
            "FUSILADE",
            "FUTO",
            "GB200",
            "HOTRING",
            "JESTER",
            "JESTER2",
            "KHAMELION",
            "KURUMA",
            "KURUMA2",
            "LYNX",
            "MASSACRO",
            "MASSACRO2",
            "NINEF",
            "NINEF2",
            "OMNIS",
            "PARIAH",
            "PENUMBRA",
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
            "SCHWARZER",
            "SENTINEL3",
            "SEVEN70",
            "SPECTER",
            "SPECTER2",
            "SULTAN",
            "SURANO",
            "TAMPA2",
            "TROPOS",
            "VERLIERER2",
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
            "ENTITYXF",
            "ENTITY2",
            "FMJ",
            "GP1",
            "INFERNUS",
            "ITALIGTB",
            "ITALIGTB2",
            "LE7B",
            "NERO",
            "NERO2",
            "OSIRIS",
            "PENETRATOR",
            "PFISTER811",
            "PROTOTIPO",
            "REAPER",
            "SC1",
            "SCRAMJET",
            "SHEAVA", // ETR1
            "SULTANRS",
            "T20",
            "TAIPAN",
            "TEMPESTA",
            "TEZERACT",
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
            "NEMESIS",
            "NIGHTBLADE",
            "OPPRESSOR",
            "PCJ",
            "RATBIKE",
            "RUFFIAN",
            "SANCHEZ",
            "SANCHEZ2",
            "SANCTUS",
            "SHOTARO",
            "SOVEREIGN",
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
            "CARACARA",
            "DLOADER",
            "DUBSTA3",
            "DUNE",
            "DUNE2",
            "DUNE3",
            "DUNE4",
            "DUNE5",
            "FREECRAWLER",
            "INSURGENT",
            "INSURGENT2",
            "INSURGENT3",
            "KALAHARI",
            "KAMACHO",
            "MARSHALL",
            "MENACER",
            "MESA3",
            "MONSTER",
            "NIGHTSHARK",
            "RANCHERXL",
            "RANCHERXL2",
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
            "SPEEDO4",
            "SURFER",
            "SURFER2",
            "TACO",
            "YOUGA",
            "YOUGA2",
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
            "DINGHY",
            "DINGHY2",
            "DINGHY3",
            "DINGHY4",
            "JETMAX",
            "MARQUIS",
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
            "ALPHAZ1",
            "AVENGER",
            "AVENGER2",
            "BESRA",
            "BLIMP",
            "BLIMP2",
            "BLIMP3",
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
            "STRIKEFORCE",
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
            "RHINO",
            "THRUSTER", // Jetpack
            "TRAILERSMALL2", // Anti Aircraft Trailer
        };
        #endregion
        #region Commercial
        public static List<string> Commercial { get; } = new List<string>()
        {
            "BENSON",
            "BIFF",
            "HAULER",
            "HAULER2",
            "MULE",
            "MULE2",
            "MULE3",
            "PACKER",
            "PHANTOM",
            "PHANTOM2",
            "PHANTOM3",
            "POUNDER",
            "POUNDER2",
            "STOCKADE",
            "STOCKADE3",
            "TERBYTE",
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

        public Dictionary<string, List<string>> VehicleClasses { get; } = new Dictionary<string, List<string>>()
        {
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_0")] = Compacts,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_1")] = Sedans,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_2")] = SUVs,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_3")] = Coupes,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_4")] = Muscle,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_5")] = SportsClassics,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_6")] = Sports,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_7")] = Super,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_8")] = Motorcycles,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_9")] = OffRoad,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_10")] = Industrial,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_11")] = Utility,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_12")] = Vans,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_13")] = Cycles,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_14")] = Boats,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_15")] = Helicopters,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_16")] = Planes,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_17")] = Service,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_18")] = Emergency,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_19")] = Military,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_20")] = Commercial,
            [MainMenu.Cf.GetLocalizedName("VEH_CLASS_21")] = Trains,
        };
        #endregion
    }
}
