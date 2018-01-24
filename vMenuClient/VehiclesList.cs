using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMenuClient
{
    class VehiclesList
    {
        #region Compacts
        public List<String> Compacts() => new List<String>
        {
            "BLISTA",
            "BRIOSO",
            "DILETTANTE",
            "DILETTANTE2",
            "ISSI2",
            "PANTO",
            "PRAIRIE",
            "RHAPSODY",
        };
        #endregion
        #region Sedans
        public List<String> Sedans() => new List<String>
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
        public List<String> SUVs() => new List<String>
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
            "RADI",
            "ROCOTO",
            "SEMINOLE",
            "SERRANO",
            "XLS",
            "XLS2",
        };
        #endregion
        #region Coupes
        public List<String> Coupes() => new List<String>
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
        public List<String> Muscle() => new List<String>
        {
            "BLADE",
            "BUCCANEER",
            "BUCCANEER2",
            "CHINO",
            "CHINO2",
            "COQUETTE3",
            "DOMINATOR",
            "DOMINATOR2",
            "DUKES",
            "DUKES2",
            "FACTION",
            "FACTION2",
            "FACTION3",
            "GAUNTLET",
            "GAUNTLET2",
            "HERMES",
            "HOTKNIFE",
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
        public List<String> SportsClassics() => new List<String>
        {
            "ARDENT",
            "BTYPE",
            "BTYPE2",
            "BTYPE3",
            "CASCO",
            "CHEETAH2",
            "COQUETTE2",
            "DELUXO",
            "FELTZER3", // Stirling GT
            "GT500",
            "INFERNUS2",
            "JB700",
            "MAMBA",
            "MANANA",
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
            "ZTYPE",
        };
        #endregion
        #region Sports
        public List<String> Sports() => new List<String>
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
            "FUROREGT",
            "FUSILADE",
            "FUTO",
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
        public List<String> Super() => new List<String>
        {
            "ADDER",
            "AUTARCH",
            "BANSHEE2",
            "BULLET",
            "CHEETAH",
            "CYCLONE",
            "ENTITYXF",
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
            "SHEAVA", // ETR1
            "SULTANRS",
            "T20",
            "TEMPESTA",
            "TURISMOR",
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
        public List<String> Motorcycles() => new List<String>
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
        public List<String> OffRoad() => new List<String>
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
            "DLOADER",
            "DUBSTA3",
            "DUNE",
            "DUNE2",
            "DUNE3",
            "DUNE4",
            "DUNE5",
            "INSURGENT",
            "INSURGENT2",
            "INSURGENT3",
            "KALAHARI",
            "MARSHALL",
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
        public List<String> Industrial() => new List<String>
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
        public List<String> Utility()
        {
            return new List<String>
        {
            "AIRTUG",
            "CADDY,Caddy (Golf)",
            "CADDY2,Caddy",
            "CADDY3,Caddy (Bunker)",
            "DOCKTUG",
            "FORKLIFT",
            "TRACTOR2", // Fieldmaster
            "TRACTOR3,Fieldmaster (Snow)", // Fieldmaster
            "MOWER", // Lawnmower
            "RIPLEY",
            "SADLER",
            "SADLER2,Sadler (Snow)",
            "SCRAP",
            "TOWTRUCK",
            "TOWTRUCK2,Towtruck (Old)",
            "TRACTOR", // Tractor (rusted/old)
            "UTILLITRUCK,Utility Truck (Crane)",
            "UTILLITRUCK2,Utility Truck",
            "UTILLITRUCK3,Utility Truck (Pickup)",

            /// Trailers

            /// Army Trailers
            "ARMYTRAILER,Military Flatbed Trailer", // Military
            "ARMYTRAILER2,Military Flatbed Trailer (Cutter)", // Civillian
            "FREIGHTTRAILER,Extended Flatbed Trailer", // Extended
            "ARMYTANKER,Army Gasoline Trailer", // Army Tanker
            
            /// Large Trailers
            "DOCKTRAILER,Large Shipping Container Trailer", // Shipping Container Trailer
            "TR3,Sailboat Trailer", // Large Boat Trailer (Sailboat)
            "TR2,Vehicles Trailer (Empty)", // Large Vehicle Trailer
            "TR4,Vehicles Trailer (SP Mission Cars)", // Large Vehicle Trailer (Mission Cars)
            "TRFLAT,Flatbed Trailer", // Large Flatbed Empty Trailer
            "TRAILERS,Canvas Shipping Container Trailer", // Container/Curtain Trailer
            "TRAILERS4,Metal Shipping COntainer Trailer", // White Container Trailer
            "TRAILERS2,Branded Canvas Trailer", // Box Trailer
            "TRAILERS3,Big Goods Trailer", // Ramp Box Trailer
            "TVTRAILER,Fame Or Shame Trailer", // Fame or Shame Trailer
            "TRAILERLOGS,Logs Trailer", // Logs Trailer
            "TANKER,Ron Oil Tanker Trailer", // Ron Oil Tanker Trailer
            "TANKER2,Oil Tanker Trailer", // Ron Oil Tanker Trailer (Heist Version)
            
            /// Medium Trailers
            "BALETRAILER,Hay Bale Trailer", // (Tractor Hay Bale Trailer)
            "GRAINTRAILER,Grain Trailer", // (Tractor Grain Trailer)
            // Ortega's trailer, we don't want this one because you can't drive them.
            //"PROPTRAILER",

            /// Small Trailers
            "BOATTRAILER", // Small Boat Trailer
            "RAKETRAILER,Tractor Plow/Rake", // Tractor Tow Plow/Rake
            "TRAILERSMALL,Small Utility Trailer", // Small Utility Trailer
        };
        }
        #endregion
        #region Vans
        public List<String> Vans() => new List<String>
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
            "SURFER",
            "SURFER2",
            "TACO",
            "YOUGA",
            "YOUGA2",
        };
        #endregion
        #region Cycles
        public List<String> Cycles() => new List<String>
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
        public List<String> Boats() => new List<String>
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
        public List<String> Helicopters() => new List<String>
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
        public List<String> Planes() => new List<String>
        {
            "ALPHAZ1",
            "AVENGER",
            "AVENGER2",
            "BESRA",
            "BLIMP",
            "BLIMP2",
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
        public List<String> Service() => new List<String>
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
        public List<String> Emergency() => new List<String>
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
        public List<String> Military() => new List<String>
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
            "TRAILERLARGE", // Mobile Operations Center
        };
        #endregion
        #region Commercial
        public List<String> Commercial() => new List<String>
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
            "STOCKADE",
            "STOCKADE3",
        };
        #endregion
        #region Trains
        public List<String> Trains() => new List<String>
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
    }
}
