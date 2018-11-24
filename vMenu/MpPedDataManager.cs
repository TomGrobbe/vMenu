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
        public HeadBlendData GetHeadBlendData()
        {
            string data = Exports["vMenu"].Export_GetPedHeadBlendData();
            dynamic dataObj = JsonConvert.DeserializeObject(data);
            int shape1 = int.Parse(dataObj["0"].ToString());
            int shape2 = int.Parse(dataObj["2"].ToString());
            int skin1 = int.Parse(dataObj["6"].ToString());
            int skin2 = int.Parse(dataObj["8"].ToString());

            uint parentShapeInt = uint.Parse(dataObj["12"].ToString());
            byte[] bytes = BitConverter.GetBytes(parentShapeInt);
            float parentShape = BitConverter.ToSingle(bytes, 0);

            uint parentSkinInt = uint.Parse(dataObj["14"].ToString());
            bytes = BitConverter.GetBytes(parentSkinInt);
            float parentSkin = BitConverter.ToSingle(bytes, 0);

            return new HeadBlendData(shape1, shape2, 0, skin1, skin2, 0, parentShape, parentSkin, 0f, false);
        }

        public struct HeadBlendData
        {
            public int shape1;
            public int shape2;
            public int shape3;
            public int skin1;
            public int skin2;
            public int skin3;
            public float parentShapePercent;
            public float parentSkinPercent;
            public float parentThirdPercent;
            public bool isParent;

            public HeadBlendData(int shape1, int shape2, int shape3, int skin1, int skin2, int skin3, float parentShapePercent, float parentSkinPercent, float parentThirdPercent, bool isParent)
            {
                this.shape1 = shape1;
                this.shape2 = shape2;
                this.shape3 = shape3;
                this.skin1 = skin1;
                this.skin2 = skin2;
                this.skin3 = skin3;
                this.parentShapePercent = parentShapePercent;
                this.parentSkinPercent = parentSkinPercent;
                this.parentThirdPercent = parentThirdPercent;
                this.isParent = isParent;
            }
        }

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
            public HeadBlendData PedHeadBlendData;
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
