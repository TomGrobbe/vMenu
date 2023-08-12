using CitizenFX.Core;

using MenuAPI;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;

namespace vMenuClient.menus
{
    public class Recording
    {
        // Variables
        private Menu menu;

        private void CreateMenu()
        {
            AddTextEntryByHash(0x86F10CE6, "Upload To Cfx.re Forum"); // Replace the "Upload To Social Club" button in gallery
            AddTextEntry("ERROR_UPLOAD", "Are you sure you want to upload this photo to Cfx.re forum?"); // Replace the warning message text for uploading

            // Create the menu.
            menu = new Menu("Recording", "Recording Options");

            var takePic = new MenuItem("Take Photo", "Takes a photo and saves it to the Pause Menu gallery.");
            var openPmGallery = new MenuItem("Open Gallery", "Opens the Pause Menu gallery.");
            var startRec = new MenuItem("Start Recording", "Start a new game recording using GTA V's built in recording.");
            var stopRec = new MenuItem("Stop Recording", "Stop and save your current recording.");
            var openEditor = new MenuItem("Rockstar Editor", "Open the rockstar editor, note you might want to quit the session first before doing this to prevent some issues.");

            menu.AddMenuItem(takePic);
            menu.AddMenuItem(openPmGallery);
            menu.AddMenuItem(startRec);
            menu.AddMenuItem(stopRec);
            menu.AddMenuItem(openEditor);

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
                else if (item == openPmGallery)
                {
                    ActivateFrontendMenu((uint)GetHashKey("FE_MENU_VERSION_MP_PAUSE"), true, 3);
                }
                else if (item == takePic)
                {
                    BeginTakeHighQualityPhoto();
                    SaveHighQualityPhoto(-1);
                    FreeMemoryForHighQualityPhoto();
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
                        QuitSession();
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
