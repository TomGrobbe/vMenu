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
        public struct CharacterComponents
        {
        }

        public struct FaceFeatures
        {
        }

        public struct PedHair
        {
            public int style;
            public int color;
            public int colorHighlight;
            public int eyebrowsStyle;
            public int eyebrowsColor;
            public int beardStyle;
            public int beardColor;
            //public int 
        }

        public struct PedMakeup
        {
        }

        public struct MultiplayerPedData
        {
            public PedHeadBlendData PedHeadBlendData;
            public CharacterComponents CharacterComponentsData;
            public FaceFeatures FaceFeaturesData;
            public PedHair PedHairData;
            public PedMakeup PedMakeupData;
            public bool IsMale;
            public uint ModelHash;
            public string SaveName;
            public int Version;
        }
    }
}
