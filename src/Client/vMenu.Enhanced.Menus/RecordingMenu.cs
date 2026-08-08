using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.RecordingMenu.Title,
    SubtitleKey = Loc.RecordingMenu.Subtitle,
    DescriptionKey = Loc.RecordingMenu.LinkDescription
    )]
public sealed class RecordingMenu : MenuDefinition
{
    /// <summary>Hash of the game's "Upload To Social Club" gallery label.</summary>
    private const uint UploadLabelHash = 0x86F10CE6;

    private const string UploadWarningEntry = "ERROR_UPLOAD";

    private const int EditorNoticeMs = 2500;

    protected override void Build(MenuBuilder menu)
    {
        ApplyGalleryText();

        Localizer.Changed += ApplyGalleryText;

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.RecordingMenu.TakePic),
            Description = MenuText.Key(Loc.RecordingMenu.TakePicDescription),
            OnSelected = _ =>
            {
                Native.BeginTakeHighQualityPhoto();
                Native.SaveHighQualityPhoto(-1);
                Native.FreeMemoryForHighQualityPhoto();

                Notifications.Success(MenuText.Key(Loc.RecordingMenu.TakePicDone));
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

                    Notifications.Success(MenuText.Key(Loc.RecordingMenu.RecordingStarted));
                }
                else
                {
                    Native.StopRecordingAndSaveClip();

                    Notifications.Success(MenuText.Key(Loc.RecordingMenu.RecordingSaved));
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
                {
                    Notifications.Warning(MenuText.Key(Loc.RecordingMenu.NotRecording));

                    return;
                }

                Native.StopRecordingAndDiscardClip();

                Notifications.Info(MenuText.Key(Loc.RecordingMenu.RecordingCancelled));
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
                Notifications.Info(MenuText.Key(Loc.RecordingMenu.LeftRockstarEditor), EditorNoticeMs);
            },
        });
    }

    private static void ApplyGalleryText()
    {
        var localizer = Localizer.Current;

        Native.AddTextEntryByHash(UploadLabelHash, localizer.Get(Loc.RecordingMenu.GalleryUpload));
        Native.AddTextEntry(UploadWarningEntry, localizer.Get(Loc.RecordingMenu.GalleryUploadWarning));
    }
}
