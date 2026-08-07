using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using vMenu.Enhanced.Data.Configuration;

namespace vMenu.Enhanced.Menus;
[VMenu(
    TitleKey = Loc.RecordingMenu.Title,
    SubtitleKey = Loc.RecordingMenu.Subtitle,
    DescriptionKey = Loc.RecordingMenu.LinkDescription
    )]
public sealed class RecordingMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.RecordingMenu.TakePic),
            Description = MenuText.Key(Loc.RecordingMenu.TakePicDescription),
            OnSelected = _ =>
            {
                Native.BeginTakeHighQualityPhoto();
                Native.SaveHighQualityPhoto(-1);
                Native.FreeMemoryForHighQualityPhoto();
            },
        });
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.RecordingMenu.OpenGallery),
            Description = MenuText.Key(Loc.RecordingMenu.OpenGalleryDescription),
            OnSelected = _ =>
            {
                Native.ActivateFrontendMenu(API.Hash("FE_MENU_VERSION_MP_PAUSE"), true, 3);
            },
        });
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.RecordingMenu.StartStopRecording),
            Description = MenuText.Key(Loc.RecordingMenu.StartStopRecordingDescription),
            OnSelected = _ =>
            {
                if (!Native.IsRecording())
                {
                    Native.StartRecording(1);
                }
                else
                {
                    Native.StopRecordingAndSaveClip();
                }
            },
        });
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.RecordingMenu.CancelRecording),
            Description = MenuText.Key(Loc.RecordingMenu.CancelRecordingDescription),
            OnSelected = _ =>
            {
                if (!Native.IsRecording())
                { }
                else
                {
                    Native.StopRecordingAndDiscardClip();
                }
            },
        });
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.RecordingMenu.OpenRockstarEditor),
            Description = MenuText.Key(Loc.RecordingMenu.OpenRockstarEditorDescription),
            OnSelectedAsync = async _ =>  
            {
                Native.NetworkStartSoloTutorialSession();
                Native.ActivateRockstarEditor(3);
                while (Native.IsPauseMenuActive())
                {
                    await API.Delay(0);
                }

                Native.DoScreenFadeIn(1);
                Native.NetworkEndTutorialSession();
                Notifications.Info(MenuText.Key(Loc.RecordingMenu.CanceledRockstarEditor),  2500);
            },
        });
    }
    
}
