using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using static vMenuShared.ConfigManager;

namespace vMenuClient
{
    public class Recording
    {
        // Variables
        private UIMenu menu;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu("Recording", "Recording Options", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            UIMenuItem startRec = new UIMenuItem("Start Recording", "Start a new game recording using GTA V's built in recording.");
            UIMenuItem stopRec = new UIMenuItem("Stop Recording", "Stop and save your current recording.");
            UIMenuItem openEditor = new UIMenuItem("Rockstar Editor", "Open the rockstar editor, note you might want to quit the session first before doing this to prevent some issues.");
            menu.AddItem(startRec);
            menu.AddItem(stopRec);
            menu.AddItem(openEditor);

            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == startRec)
                {
                    if (IsRecording())
                    {
                        Notify.Alert("You are already recording a clip, you need to stop recording first before you can start recording again!");
                    }
                    else
                    {
                        StartRecording(1);
                    }
                }
                else if (item == stopRec)
                {
                    if (!IsRecording())
                    {
                        Notify.Alert("You are currently NOT recording a clip, you need to start recording first before you can stop and save a clip.");
                    }
                    else
                    {
                        StopRecordingAndSaveClip();
                    }
                }
                else if (item == openEditor)
                {
                    if (GetSettingsBool(Setting.vmenu_quit_session_in_rockstar_editor))
                    {
                        MainMenu.Cf.QuitSession();
                    }
                    ActivateRockstarEditor();
                    // wait for the editor to be closed again.
                    while (IsPauseMenuActive())
                    {
                        await BaseScript.Delay(0);
                    }
                    // then fade in the screen.
                    DoScreenFadeIn(1);
                    Notify.Alert("You left your previous session before entering the Rockstar Editor. Restart the game to be able to rejoin the server's main session.", true, true);
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
