using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    public class VoiceChat
    {
        // Variables
        private UIMenu menu;
        private CommonFunctions cf = MainMenu.Cf;
        public bool EnableVoicechat = UserDefaults.VoiceChatEnabled;
        public bool ShowCurrentSpeaker = UserDefaults.ShowCurrentSpeaker;
        public bool ShowVoiceStatus = UserDefaults.ShowVoiceStatus;
        public float currentProximity = UserDefaults.VoiceChatProximity;
        public List<dynamic> channels = new List<dynamic>()
        {
            "Channel 1 (Default)",
            "Channel 2",
            "Channel 3",
            "Channel 4",
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
            if (cf.IsAllowed(Permission.VCStaffChannel))
            {
                channels.Add("Staff Channel");
            }

            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Voice Chat Settings", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            UIMenuCheckboxItem voiceChatEnabled = new UIMenuCheckboxItem("Enable Voice Chat", EnableVoicechat, "Enable or disable voice chat.");
            UIMenuCheckboxItem showCurrentSpeaker = new UIMenuCheckboxItem("Show Current Speaker", ShowCurrentSpeaker, "Shows who is currently talking.");
            UIMenuCheckboxItem showVoiceStatus = new UIMenuCheckboxItem("Show Microphone Status", ShowVoiceStatus, "Shows whether your microphone is open or muted.");

            List<dynamic> proximity = new List<dynamic>()
            {
                "5 m",
                "10 m",
                "15 m",
                "20 m",
                "100 m",
                "300 m",
                "1 km",
                "2 km",
                "Global",
            };
            UIMenuListItem voiceChatProximity = new UIMenuListItem("Voice Chat Proximity", proximity, proximityRange.IndexOf(currentProximity), "Set the voice chat receiving proximity in meters.");
            UIMenuListItem voiceChatChannel = new UIMenuListItem("Voice Chat Channel", channels, channels.IndexOf(currentChannel), "Set the voice chat channel.");

            if (cf.IsAllowed(Permission.VCEnable))
            {
                menu.AddItem(voiceChatEnabled);

                // Nested permissions because without voice chat enabled, you wouldn't be able to use these settings anyway.
                if (cf.IsAllowed(Permission.VCShowSpeaker))
                {
                    menu.AddItem(showCurrentSpeaker);
                }

                menu.AddItem(voiceChatProximity);
                menu.AddItem(voiceChatChannel);
                menu.AddItem(showVoiceStatus);
            }

            menu.OnCheckboxChange += (sender, item, _checked) =>
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

            menu.OnListChange += (sender, item, index) =>
            {
                if (item == voiceChatProximity)
                {
                    currentProximity = proximityRange[index];
                    Subtitle.Custom($"New voice chat proximity set to: ~b~{proximity[index]}~s~.");
                }
                else if (item == voiceChatChannel)
                {
                    currentChannel = channels[index];
                    Subtitle.Custom($"New voice chat channel set to: ~b~{channels[index]}~s~.");
                }
            };

        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
