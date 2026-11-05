using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Storage;

using FreecamCamera = vMenu.Enhanced.Freecam.Freecam;

namespace vMenu.Enhanced.Menus.Filmmaking;

[VMenu(
    TitleKey = Loc.Filmmaker.CameraTitle,
    SubtitleKey = Loc.Filmmaker.CameraSubtitle,
    DescriptionKey = Loc.Filmmaker.CameraLinkDescription)]
public sealed class FilmmakerCameraMenu : MenuDefinition
{
    private const float SpeedStep = 1.25f;

    private const float FovStep = 1f;

    private const float RollStep = 1f;

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new DynamicListEntry(
            Loc.Filmmaker.Speed,
            SpeedText,
            change =>
            {
                FreecamCamera.Speed = change.Left ? FreecamCamera.Speed / SpeedStep : FreecamCamera.Speed * SpeedStep;

                return SpeedText();
            }));

        menu.Entries.Add(new SliderEntry(Loc.Filmmaker.LookSensitivity, 1, 20)
        {
            ReadPosition = () => UserDefaults.FreecamLookSensitivity.Value,
            OnMoved = moved => UserDefaults.FreecamLookSensitivity.Value = moved.NewPosition,
        });

        menu.Entries.Add(new SliderEntry(Loc.Filmmaker.Smoothing, 0, 10)
        {
            ReadPosition = () => UserDefaults.FreecamSmoothing.Value,
            OnMoved = moved => UserDefaults.FreecamSmoothing.Value = moved.NewPosition,
        });

        menu.Entries.Add(new CheckboxEntry(Loc.Filmmaker.InvertLook)
        {
            ReadState = () => UserDefaults.FreecamInvertLook.Value,
            OnChanged = changed => UserDefaults.FreecamInvertLook.Value = changed.Checked,
        });

        menu.Entries.Add(new DynamicListEntry(
            Loc.Filmmaker.Zoom,
            ZoomText,
            change =>
            {
                FreecamCamera.Fov += change.Left ? -FovStep : FovStep;

                return ZoomText();
            })
        {
            ReadEnabled = () => FreecamCamera.IsActive,
        });

        menu.Entries.Add(new DynamicListEntry(
            Loc.Filmmaker.Roll,
            RollText,
            change =>
            {
                FreecamCamera.Roll += change.Left ? -RollStep : RollStep;

                return RollText();
            })
        {
            ReadEnabled = () => FreecamCamera.IsActive,
        });

        menu.Entries.Add(new ButtonEntry(Loc.Filmmaker.ResetLens)
        {
            ReadEnabled = () => FreecamCamera.IsActive,
            OnSelected = _ =>
            {
                FreecamCamera.ResetLens();

                menu.Refresh();
            },
        });

        menu.Entries.Add(new CheckboxEntry(Loc.Filmmaker.HideHints)
        {
            ReadState = () => UserDefaults.FreecamHideHints.Value,
            OnChanged = changed => UserDefaults.FreecamHideHints.Value = changed.Checked,
        });

        FreecamCamera.StateChanged += menu.Refresh;
    }

    private static string SpeedText() => FilmmakerText.Format(FreecamCamera.Speed) + " m/s";

    private static string ZoomText() =>
        $"{FilmmakerText.Format(FreecamCamera.Fov, "0")}° ({FilmmakerText.Format(FreecamCamera.DefaultFov / FreecamCamera.Fov, "0.0")}x)";

    private static string RollText() => FilmmakerText.Format(FreecamCamera.Roll, "0") + "°";
}
