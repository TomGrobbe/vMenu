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
