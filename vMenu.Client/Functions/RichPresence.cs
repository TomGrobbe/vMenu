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
    public class RichPresence
    {
    	public struct RichPresenceStruct
        {
            public RichPresenceToggle Enable;
            public RichPresenceTextStruct RichPresence;
            public RichPresenceButton ButtonOne;
			public RichPresenceButton ButtonTwo;
			public RichPresenceImage SmallImage;
			public RichPresenceImage LargeImage;

            public RichPresenceStruct(RichPresenceToggle Enable, RichPresenceTextStruct RichPresence, RichPresenceButton ButtonOne, RichPresenceButton ButtonTwo, RichPresenceImage SmallImage, RichPresenceImage LargeImage)
            {
                this.Enable = Enable;
                this.RichPresence = RichPresence;
				this.ButtonOne = ButtonOne;
				this.ButtonTwo = ButtonTwo;
				this.SmallImage = SmallImage;
				this.LargeImage = LargeImage;
            }
        }

        public struct RichPresenceToggle
        {
            public bool toggle;

            public RichPresenceToggle(bool toggle)
            {
                this.toggle = toggle;
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

        private static readonly object _padlock = new();
        private static RichPresence _instance;

        RichPresenceStruct JsonRichPresence;
        string JsonRichPresenceAppId;

        private RichPresence ()
        {
            string JsonData = LoadResourceFile(GetCurrentResourceName(), "config/RichPresence.jsonc") ?? "{}";
            Dictionary<string, RichPresenceStruct> JsonRichPresenceObj = JsonConvert.DeserializeObject<Dictionary<string, RichPresenceStruct>>(JsonData);
            JsonRichPresence = JsonRichPresenceObj.FirstOrDefault().Value;

            if (JsonRichPresenceObj.FirstOrDefault().Key != "")
            {
                if (JsonRichPresence.Enable.toggle != false)
                {
                    JsonRichPresenceAppId = JsonRichPresenceObj.FirstOrDefault().Key;
                    Main.Instance.AttachTick(DiscordRichPresence);
                }
            }
        }

        internal static RichPresence Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new RichPresence();
                }
            }
        }
        public string Substitutes(string text)
        {
            text = text.Replace("%playercount%", $"{GetActivePlayers().Count}/{GetConvar("sv_maxClients", "48")}");
            text = text.Replace("%playername%", $"{Game.Player.Name}"); 
            text = text.Replace("%playerid%", $"{Game.Player.ServerId}"); 
            text = text.Replace("%version%", $"{MenuFunctions.Version}");
            text = text.Replace("%gamebuild%", $"{GetGameBuildNumber()}");
            text = text.Replace("%nl%", $"\n");
            text = text.Replace("%newline%", $"\n");

            return text;
        }
        public async Task DiscordRichPresence()
        {
            SetDiscordAppId(JsonRichPresenceAppId);

            if (JsonRichPresence.RichPresence.text != "")
            {
                SetRichPresence(Substitutes(JsonRichPresence.RichPresence.text));
            }

            if (JsonRichPresence.ButtonOne.link != "" || JsonRichPresence.ButtonOne.text != "")
            {
                SetDiscordRichPresenceAction(0, Substitutes(JsonRichPresence.ButtonOne.text), JsonRichPresence.ButtonOne.link);
            }

            if (JsonRichPresence.ButtonTwo.link != "" || JsonRichPresence.ButtonTwo.text != "")
            {
                SetDiscordRichPresenceAction(1, Substitutes(JsonRichPresence.ButtonTwo.text), JsonRichPresence.ButtonTwo.link);
            }

            if (JsonRichPresence.LargeImage.image != "")
            {
                SetDiscordRichPresenceAsset(JsonRichPresence.LargeImage.image);
            }

            if (JsonRichPresence.LargeImage.text != "")
            {
                SetDiscordRichPresenceAssetText(Substitutes(JsonRichPresence.LargeImage.text));
            }

            if (JsonRichPresence.SmallImage.image != "")
            {
                SetDiscordRichPresenceAssetSmall(JsonRichPresence.SmallImage.image);
            }

            if (JsonRichPresence.SmallImage.text != "")
            {
                SetDiscordRichPresenceAssetSmallText(Substitutes(JsonRichPresence.SmallImage.text));
            }

            await BaseScript.Delay(5000);
        }

    }
}
