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
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.Cf;
        public bool EnableVoicechat = UserDefaults.VoiceChatEnabled;
        public bool ShowCurrentSpeaker = UserDefaults.ShowCurrentSpeaker;
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
            20f, // 20m
            50f, // 50m
            100f, // 100m
            150f, // 150m
            300f, // 300m
            500f, // 500m
            2000f, // 2.000m
            5000f, // 5.000m
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

            List<dynamic> proximity = new List<dynamic>()
            {
                "20 m",
                "50 m",
                "100 m",
                "150 m",
                "300 m",
                "500 m",
                "2 km",
                "5 km",
                "Global",
            };
            UIMenuListItem voiceChatProximity = new UIMenuListItem("Voice Chat Proximity", proximity, proximityRange.IndexOf(currentProximity), "Set the voice chat receiving proximity in meters.");
            UIMenuListItem voiceChatChannel = new UIMenuListItem("Voice Chat Channel", channels, channels.IndexOf(currentChannel), "Set the voice chat channel.");

            menu.AddItem(voiceChatEnabled);
            menu.AddItem(showCurrentSpeaker);
            menu.AddItem(voiceChatProximity);
            menu.AddItem(voiceChatChannel);

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
