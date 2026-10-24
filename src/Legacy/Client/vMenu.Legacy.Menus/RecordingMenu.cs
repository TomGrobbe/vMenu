using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.RecordingMenu.Title,
    SubtitleKey = Loc.RecordingMenu.Subtitle,
    DescriptionKey = Loc.RecordingMenu.LinkDescription
    )]
public sealed class RecordingMenu : MenuDefinition
{
    // Hash of the game's "Upload To Social Club" gallery label.
    private const uint UploadLabelHash = 0x86F10CE6;

    private const string UploadWarningEntry = "ERROR_UPLOAD";

    private const int EditorNoticeMs = 2500;

    protected override void Build(MenuBuilder menu)
    {
        ApplyGalleryText();

        Localizer.Changed += ApplyGalleryText;

        menu.Entries.Add(new ButtonEntry(Loc.RecordingMenu.TakePic)
        {
            OnSelected = _ =>
            {
                API.BeginTakeHighQualityPhoto();
                API.SaveHighQualityPhoto(-1);
                API.FreeMemoryForHighQualityPhoto();

                Notifications.Success(MenuText.Key(Loc.RecordingMenu.TakePicDone));
            },
        });
        menu.Entries.Add(new ButtonEntry(Loc.RecordingMenu.OpenGallery)
        {
            OnSelected = _ =>
            {
                API.ActivateFrontendMenu(Hashing.Joaat("FE_MENU_VERSION_MP_PAUSE"), true, 3);
            },
        });
        menu.Entries.Add(new ButtonEntry(Loc.RecordingMenu.StartStopRecording)
        {
            OnSelected = _ =>
            {
                if (!API.IsRecording())
                {
                    API.StartRecording(1);

                    Notifications.Success(MenuText.Key(Loc.RecordingMenu.RecordingStarted));
                }
                else
                {
                    API.StopRecordingAndSaveClip();

                    Notifications.Success(MenuText.Key(Loc.RecordingMenu.RecordingSaved));
                }
            },
        });
        menu.Entries.Add(new ButtonEntry(Loc.RecordingMenu.CancelRecording)
        {
            OnSelected = _ =>
            {
                if (!API.IsRecording())
                {
                    Notifications.Warning(MenuText.Key(Loc.RecordingMenu.NotRecording));

                    return;
                }

                API.StopRecordingAndDiscardClip();

                Notifications.Info(MenuText.Key(Loc.RecordingMenu.RecordingCancelled));
            },
        });
        menu.Entries.Add(new ButtonEntry(Loc.RecordingMenu.OpenRockstarEditor)
        {
            OnSelectedAsync = async _ =>
            {
                API.NetworkStartSoloTutorialSession();
                NativeFixer.ActivateRockstarEditor(3);
                while (API.IsPauseMenuActive())
                {
                    await BaseScript.Delay(0);
                }

                API.DoScreenFadeIn(1);
                API.NetworkEndTutorialSession();
                Notifications.Info(MenuText.Key(Loc.RecordingMenu.LeftRockstarEditor), EditorNoticeMs);
            },
        });
    }

    private static void ApplyGalleryText()
    {
        var localizer = Localizer.Current;

        NativeFixer.AddTextEntryByHash(UploadLabelHash, localizer.Get(Loc.RecordingMenu.GalleryUpload));
        NativeFixer.AddTextEntry(UploadWarningEntry, localizer.Get(Loc.RecordingMenu.GalleryUploadWarning));
    }
}
