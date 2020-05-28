using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class VoiceChat
    {
        // Variables
        private Menu menu;
        private static LanguageManager LM = new LanguageManager();
        public bool EnableVoicechat = UserDefaults.VoiceChatEnabled;
        public bool ShowCurrentSpeaker = UserDefaults.ShowCurrentSpeaker;
        public bool ShowVoiceStatus = UserDefaults.ShowVoiceStatus;
        public float currentProximity = UserDefaults.VoiceChatProximity;
        public List<string> channels = new List<string>()
        {
            LM.Get("Channel 1 (Default)"),
            LM.Get("Channel 2"),
            LM.Get("Channel 3"),
            LM.Get("Channel 4"),
        };
        public string currentChannel;
        private List<float> proximityRange = new List<float>()
        {
            5f, // 5m
            10f, // 10m
            15f, // 15m
            20f, // 20m
            100f, // 100m
            300f, // 300m
            1000f, // 1.000m
            2000f, // 2.000m
            0f, // global
        };


        private void CreateMenu()
        {
            currentChannel = channels[0];
            if (IsAllowed(Permission.VCStaffChannel))
            {
                channels.Add(LM.Get("Staff Channel"));
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, LM.Get("Voice Chat Settings"));

            MenuCheckboxItem voiceChatEnabled = new MenuCheckboxItem(LM.Get("Enable Voice Chat"), LM.Get("Enable or disable voice chat."), EnableVoicechat);
            MenuCheckboxItem showCurrentSpeaker = new MenuCheckboxItem(LM.Get("Show Current Speaker"), LM.Get("Shows who is currently talking."), ShowCurrentSpeaker);
            MenuCheckboxItem showVoiceStatus = new MenuCheckboxItem(LM.Get("Show Microphone Status"), LM.Get("Shows whether your microphone is open or muted."), ShowVoiceStatus);

            List<string> proximity = new List<string>()
            {
                LM.Get("5 m"),
                LM.Get("10 m"),
                LM.Get("15 m"),
                LM.Get("20 m"),
                LM.Get("100 m"),
                LM.Get("300 m"),
                LM.Get("1 km"),
                LM.Get("2 km"),
                LM.Get("Global"),
            };
            MenuListItem voiceChatProximity = new MenuListItem(LM.Get("Voice Chat Proximity"), proximity, proximityRange.IndexOf(currentProximity), LM.Get("Set the voice chat receiving proximity in meters."));
            MenuListItem voiceChatChannel = new MenuListItem(LM.Get("Voice Chat Channel"), channels, channels.IndexOf(currentChannel), LM.Get("Set the voice chat channel."));

            if (IsAllowed(Permission.VCEnable))
            {
                menu.AddMenuItem(voiceChatEnabled);

                // Nested permissions because without voice chat enabled, you wouldn't be able to use these settings anyway.
                if (IsAllowed(Permission.VCShowSpeaker))
                {
                    menu.AddMenuItem(showCurrentSpeaker);
                }

                menu.AddMenuItem(voiceChatProximity);
                menu.AddMenuItem(voiceChatChannel);
                menu.AddMenuItem(showVoiceStatus);
            }

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == voiceChatEnabled)
                {
                    EnableVoicechat = _checked;
                }
                else if (item == showCurrentSpeaker)
                {
                    ShowCurrentSpeaker = _checked;
                }
                else if (item == showVoiceStatus)
                {
                    ShowVoiceStatus = _checked;
                }
            };

            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == voiceChatProximity)
                {
                    currentProximity = proximityRange[newIndex];
                    Subtitle.Custom($"New voice chat proximity set to: ~b~{proximity[newIndex]}~s~.");
                }
                else if (item == voiceChatChannel)
                {
                    currentChannel = channels[newIndex];
                    Subtitle.Custom($"New voice chat channel set to: ~b~{channels[newIndex]}~s~.");
                }
            };

        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
