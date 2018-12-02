using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace vMenuClient
{
    public class MpPedDataManager : BaseScript
    {
        public struct DrawableVariations
        {
            public Dictionary<int, KeyValuePair<int, int>> clothes;
        }

        public struct PropVariations
        {
            public Dictionary<int, KeyValuePair<int, int>> props;
        }

        public struct FaceShapeFeatures
        {

        }

        public struct PedTatttoos { } // added the structure for it, but will not be implemented until it's actually possible. Just future-proofing my setup here.

        public struct PedFacePaints { } // also added to make it future proof, but not actually used (yet).

        public struct PedAppearance
        {
            public int hairStyle;
            public int hairColor;
            public int hairHighlightColor;
            public KeyValuePair<string, string> HairOverlay;

            // 0 blemishes
            public int blemishesStyle;
            public float blemishesOpacity;

            // 1 beard
            public int beardStyle;
            public int beardColor;
            public float beardOpacity;

            // 2 eyebrows
            public int eyebrowsStyle;
            public int eyebrowsColor;
            public float eyebrowsOpacity;

            // 3 ageing
            public int ageingStyle;
            public float ageingOpacity;

            // 4 makeup
            public int makeupStyle;
            public int makeupColor;
            public float makeupOpacity;

            // 5 blush
            public int blushStyle;
            public int blushColor;
            public float blushOpacity;

            // 6 complexion
            public int complexionStyle;
            public float complexionOpacity;

            // 7 sun damage
            public int sunDamageStyle;
            public float sunDamageOpacity;

            // 8 lipstick
            public int lipstickStyle;
            public int lipstickColor;
            public float lipstickOpacity;

            // 9 moles / freckles
            public int molesFrecklesStyle;
            public float molesFrecklesOpacity;

            // 10 chest hair
            public int chestHairStyle;
            public int chestHairColor;
            public float chestHairOpacity;

            public int eyeColor;
        }

        public struct MultiplayerPedData
        {
            public PedHeadBlendData PedHeadBlendData;
            public DrawableVariations DrawableVariations;
            public PropVariations PropVariations;
            public FaceShapeFeatures FaceShapeFeatures;
            public PedAppearance PedAppearance;
            public PedTatttoos PedTatttoos;
            public PedFacePaints PedFacePaints;
            public bool IsMale;
            public uint ModelHash;
            public string SaveName;
            public int Version;
        }
    }
}
