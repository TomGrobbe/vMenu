using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMenuClient
{
    public static class VehicleData
    {
        #region Vehicle Colors
        #region Metallic and classic colors
        /// <summary>
        /// All Metallic vehicle colors (and classic, because those are the same).
        /// </summary>
        public static readonly Dictionary<string, int> MetallicColors = new Dictionary<string, int>()
        {
            ["Black"] = 0,
            ["Graphite Black"] = 1,
            ["Black Steel"] = 2,
            ["Dark Silver"] = 3,
            ["Silver"] = 4,
            ["Blue Silver"] = 5,
            ["Steel Gray"] = 6,
            ["Shadow Silver"] = 7,
            ["Stone Silver"] = 8,
            ["Midnight Silver"] = 9,
            ["Gun Metal"] = 10,
            ["Anthracite Gray"] = 11,
            ["Red"] = 27,
            ["Torino Red"] = 28,
            ["Formula Red"] = 29,
            ["Blaze Red"] = 30,
            ["Graceful Red"] = 31,
            ["Garnet Red"] = 32,
            ["Desert Red"] = 33,
            ["Cabernet Red"] = 34,
            ["Candy Red"] = 35,
            ["Sunrise Orange"] = 36,
            ["Classic Gold"] = 37,
            ["Orange"] = 38,
            ["Dark Green"] = 49,
            ["Racing Green"] = 50,
            ["Sea Green"] = 51,
            ["Olive Green"] = 52,
            ["Green"] = 53,
            ["Gasoline Blue Green"] = 54,
            ["Midnight Blue"] = 61,
            ["Dark Blue"] = 62,
            ["Saxony Blue"] = 63,
            ["Blue"] = 64,
            ["Mariner Blue"] = 65,
            ["Harbor Blue"] = 66,
            ["Diamond Blue"] = 67,
            ["Surf Blue"] = 68,
            ["Nautical Blue"] = 69,
            ["Bright Blue"] = 70,
            ["Purple Blue"] = 71,
            ["Spinnaker Blue"] = 72,
            ["Ultra Blue"] = 73,
            ["Taxi Yellow"] = 88,
            ["Race Yellow"] = 89,
            ["Bronze"] = 90,
            ["Yellow Bird"] = 91,
            ["Lime"] = 92,
            ["Champagne"] = 93,
            ["Pueblo Beige"] = 94,
            ["Dark Ivory"] = 95,
            ["Choco Brown"] = 96,
            ["Golden Brown"] = 97,
            ["Light Brown"] = 98,
            ["Straw Beige"] = 99,
            ["Moss Brown"] = 100,
            ["Biston Brown"] = 101,
            ["Beechwood"] = 102,
            ["Dark Beechwood"] = 103,
            ["Choco Orange"] = 104,
            ["Beach Sand"] = 105,
            ["Sun Bleeched Sand"] = 106,
            ["Cream"] = 107,
            ["White"] = 111,
            ["Frost White"] = 112,
            ["Securicor Green"] = 125,
            ["Vermillion Pink"] = 137,
            ["Black Purple"] = 142,
            ["Black Red"] = 143,
            ["Purple"] = 145,
            ["Lava Red"] = 150,
        };
        #endregion
        #region Worn colors
        /// <summary>
        /// All Worn vehicle colors.
        /// </summary>
        public static readonly Dictionary<string, int> WornColors = new Dictionary<string, int>()
        {
            ["Black"] = 21,
            ["Graphite"] = 22,
            ["Silver Gray"] = 23,
            ["Silver"] = 24,
            ["Blue Silver"] = 25,
            ["Shadow Silver"] = 26,
            ["Red"] = 46,
            ["Golden Red"] = 47,
            ["Dark Red"] = 48,
            ["Dark Green"] = 58,
            ["Green"] = 59,
            ["Sea Wash"] = 60,
            ["Dark Blue"] = 85,
            ["Blue"] = 86,
            ["Light Blue"] = 87,
            ["Honey Beige"] = 113,
            ["Brown"] = 114,
            ["Dark Brown"] = 115,
            ["Straw Beige"] = 116,
            ["Off White"] = 121,
            ["Orange"] = 123,
            ["Light Orange"] = 124,
            ["Taxi Yellow"] = 126,
            ["White"] = 132,
            ["Olive Army Green"] = 133,
        };
        #endregion
        #region Chrome
        /// <summary>
        /// Chrome color.
        /// </summary>
        public static readonly Dictionary<string, int> Chrome = new Dictionary<string, int>()
        {
            ["Chrome"] = 120
        };
        #endregion
        #region Matte colors
        /// <summary>
        /// All Matte vehicle colors.
        /// </summary>
        public static readonly Dictionary<string, int> MatteColors = new Dictionary<string, int>()
        {
            ["Matte Black"] = 12,
            ["Matte Gray"] = 13,
            ["Matte Light Gray"] = 14,
            ["Matte Red"] = 39,
            ["Matte Dark Red"] = 40,
            ["Matte Orange"] = 41,
            ["Matte Yellow"] = 42,
            ["Matte Lime Green"] = 55,
            ["Matte Dark Blue"] = 82,
            ["Matte Blue"] = 83,
            ["Matte Midnight Blue"] = 84,
            ["Matte Green"] = 128,
            ["Matte Brown"] = 129,
            ["Matte White"] = 131,
            ["Matte Purple"] = 148,
            ["Matte Dark Purple"] = 149,
            ["Matte Forest Green"] = 151,
            ["Matte Olive Drab"] = 152,
            ["Matte Desert Brown"] = 153,
            ["Matte Desert Tan"] = 154,
            ["Matte Foliage Green"] = 155,
        };
        #endregion
        #region Util Colors
        /// <summary>
        /// All Util vehicle colors.
        /// </summary>
        public static readonly Dictionary<string, int> UtilColors = new Dictionary<string, int>()
        {
            ["Black"] = 15,
            ["Black Poly"] = 16,
            ["Darksilver"] = 17,
            ["Silver"] = 18,
            ["Gun Metal"] = 19,
            ["Shadow Silver"] = 20,
            ["Red"] = 43,
            ["Bright Red"] = 44,
            ["Garnet Red"] = 45,
            ["Dark Green"] = 56,
            ["Green"] = 57,
            ["Dark Blue"] = 75,
            ["Midnight Blue"] = 76,
            ["Blue"] = 77,
            ["Sea Foam Blue"] = 78,
            ["Lightning Blue"] = 79,
            ["Maui Blue Poly"] = 80,
            ["Bright Blue"] = 81,
            ["Brown"] = 108,
            ["Medium Brown"] = 109,
            ["Light Brown"] = 110,
            ["Off White"] = 122,
        };
        #endregion
        #region Metals
        /// <summary>
        /// All Metal vehicle colors.
        /// </summary>
        public static readonly Dictionary<string, int> MetalColors = new Dictionary<string, int>()
        {
            ["Brushed Steel"] = 117,
            ["Brushed Black Steel"] = 118,
            ["Brushed Aluminium"] = 119,
            ["Pure Gold"] = 158,
            ["Brushed Gold"] = 159,
        };
        #endregion
        #region Unknown Colors
        /// <summary>
        /// Not sure in which category these are supposed to go...
        /// </summary>
        public static readonly Dictionary<string, int> UnknownColors = new Dictionary<string, int>()
        {
            ["Police Car Blue"] = 127,
            ["Pure White"] = 134,
            ["Hot Pink"] = 135,
            ["Salmonpink"] = 136,
            ["Orange"] = 138,
            ["Green"] = 139,
            ["Blue"] = 140,
            ["Hunter Green"] = 144,
            ["Modshop Black 1"] = 147,
            ["Default Alloy Color"] = 156,
            ["Epsilon Blue"] = 157,
        };
        #endregion
        #endregion
    }
}
