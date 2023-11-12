// System Libraries //
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

// CitizenFX Libraries //
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

// vMenu Namespaces //
using vMenu.Client.Menus;
using ScaleformUI.Menu;
using vMenu.Client.Functions;
using static vMenu.Client.KeyMappings.Commands;

namespace vMenu.Client.Functions
{
    public class RichPresence : BaseScript
    {
    	public struct RichPresenceStruct
        {
            public RichPresenceTextStruct RichPresence;
			public RichPresenceButton ButtonOne;
			public RichPresenceButton ButtonTwo;
			public RichPresenceImage SmallImage;
			public RichPresenceImage LargeImage;
            public RichPresenceStruct(RichPresenceTextStruct RichPresence, RichPresenceButton ButtonOne, RichPresenceButton ButtonTwo, RichPresenceImage SmallImage, RichPresenceImage LargeImage)
            {
                this.RichPresence = RichPresence;
				this.ButtonOne = ButtonOne;
				this.ButtonTwo = ButtonTwo;
				this.SmallImage = SmallImage;
				this.LargeImage = LargeImage;
            }
        }    	
    	public struct RichPresenceTextStruct
        {
            public string text;
            public RichPresenceTextStruct(string text)
            {
                this.text = text;
            }
        }
    	public struct RichPresenceButton
        {
            public string text;
            public string link;
            public RichPresenceButton(string text,string link)
            {
                this.text = text;
                this.link = link;
            }
        }
    	public struct RichPresenceImage
        {
            public string text;
            public string image;
            public RichPresenceImage(string text,string image)
            {
                this.text = text;
                this.image = image;
            }
        }

        public RichPresence ()
        {
            Tick += DiscordRichPresence;
        }
        public async Task DiscordRichPresence()
        {
            string JsonData = LoadResourceFile(GetCurrentResourceName(), "RichPresence.jsonc") ?? "{}";
            var JsonRichPresence = JsonConvert.DeserializeObject<Dictionary<string, RichPresenceStruct>>(JsonData);
            foreach (var RichPresenceData in JsonRichPresence)
            {
            var data = RichPresenceData.Value;
                if (RichPresenceData.Key != "")
                {
                    SetDiscordAppId(RichPresenceData.Key);
                    if (data.RichPresence.text != "")
                    SetRichPresence(data.RichPresence.text);
                    if (data.ButtonOne.link != "" || data.ButtonOne.text != "" )
                    SetDiscordRichPresenceAction(0, data.ButtonOne.text, data.ButtonOne.link);
                    if (data.ButtonTwo.link != "" || data.ButtonTwo.text != "")
                    SetDiscordRichPresenceAction(1, data.ButtonTwo.text, data.ButtonTwo.link);
                    if (data.LargeImage.image != "")
                    SetDiscordRichPresenceAsset(data.LargeImage.image);
                    if (data.LargeImage.text != "")
                    SetDiscordRichPresenceAssetText(data.LargeImage.text);
                    if (data.SmallImage.image != "")
                    SetDiscordRichPresenceAssetSmall(data.SmallImage.image);
                    if (data.SmallImage.text != "")
                    SetDiscordRichPresenceAssetSmallText(data.SmallImage.text);
                }
            }
            await Delay(5000);
        }

    }
}
