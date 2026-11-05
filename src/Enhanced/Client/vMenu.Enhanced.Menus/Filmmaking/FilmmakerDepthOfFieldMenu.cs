using vMenu.Enhanced.Freecam;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;

using FilmmakerPermissions = vMenu.Enhanced.Data.Permissions.Menus.Filmmaker;

namespace vMenu.Enhanced.Menus.Filmmaking;

[VMenu(
    TitleKey = Loc.Filmmaker.DofTitle,
    SubtitleKey = Loc.Filmmaker.DofSubtitle,
    DescriptionKey = Loc.Filmmaker.DofLinkDescription,
    Permission = FilmmakerPermissions.DepthOfField)]
public sealed class FilmmakerDepthOfFieldMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new CheckboxEntry(Loc.Filmmaker.DofEnabled)
        {
            ReadState = () => CameraEffects.DofEnabled,
            OnChanged = changed => CameraEffects.DofEnabled = changed.Checked,
        });

        menu.Entries.Add(new ListEntry(
            Loc.Filmmaker.DofFocusMode,
            FilmmakerText.Keys(Loc.Filmmaker.DofFocusManual, Loc.Filmmaker.DofFocusTarget))
        {
            ReadSelectedIndex = () => (int)CameraEffects.Focus,
            OnIndexChanged = changed => CameraEffects.Focus = (FocusMode)changed.NewIndex,
        });

        menu.Entries.Add(new DynamicListEntry(
            Loc.Filmmaker.DofDistance,
            DistanceText,
            change =>
            {
                var step = Step(CameraEffects.FocusDistance);

                CameraEffects.FocusDistance += change.Left ? -step : step;

                return DistanceText();
            })
        {
            ReadEnabled = () => CameraEffects.Focus == FocusMode.Manual,
        });

        menu.Entries.Add(new SliderEntry(Loc.Filmmaker.DofAperture, 0, 20)
        {
            ReadPosition = () => CameraEffects.DofAperture / 5,
            OnMoved = moved => CameraEffects.DofAperture = moved.NewPosition * 5,
        });
    }

    private static float Step(float distance) => distance switch
    {
        < 5f => 0.25f,
        < 20f => 1f,
        < 100f => 5f,
        _ => 25f,
    };

    private static string DistanceText() => FilmmakerText.Format(CameraEffects.FocusDistance) + " m";
}
