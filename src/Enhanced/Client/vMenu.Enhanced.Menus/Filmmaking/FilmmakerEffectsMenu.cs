using vMenu.Enhanced.Freecam;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;

using FilmmakerPermissions = vMenu.Enhanced.Data.Permissions.Menus.Filmmaker;

namespace vMenu.Enhanced.Menus.Filmmaking;

[VMenu(
    TitleKey = Loc.Filmmaker.EffectsTitle,
    SubtitleKey = Loc.Filmmaker.EffectsSubtitle,
    DescriptionKey = Loc.Filmmaker.EffectsLinkDescription,
    Permission = FilmmakerPermissions.Effects)]
public sealed class FilmmakerEffectsMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new CheckboxEntry(Loc.Filmmaker.HideHud)
        {
            ReadState = () => CameraEffects.HideHud,
            OnChanged = changed => CameraEffects.HideHud = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry(Loc.Filmmaker.Letterbox)
        {
            ReadState = () => CameraEffects.Letterbox,
            OnChanged = changed => CameraEffects.Letterbox = changed.Checked,
        });

        menu.Entries.Add(new SliderEntry(Loc.Filmmaker.MotionBlur, 0, 10)
        {
            ReadPosition = () => CameraEffects.MotionBlur / 10,
            OnMoved = moved => CameraEffects.MotionBlur = moved.NewPosition * 10,
        });

        menu.Entries.Add(new ListEntry(
            Loc.Filmmaker.Shake,
            FilmmakerText.Keys(
                Loc.Filmmaker.ShakeNone,
                Loc.Filmmaker.ShakeHandheld,
                Loc.Filmmaker.ShakeDrunk,
                Loc.Filmmaker.ShakeRoad,
                Loc.Filmmaker.ShakeAir,
                Loc.Filmmaker.ShakeVibrate))
        {
            ReadSelectedIndex = () => (int)CameraEffects.Shake,
            OnIndexChanged = changed => CameraEffects.Shake = (ShakeType)changed.NewIndex,
        });

        menu.Entries.Add(new SliderEntry(Loc.Filmmaker.ShakeIntensity, 1, 10)
        {
            ReadPosition = () => CameraEffects.ShakeIntensity,
            OnMoved = moved => CameraEffects.ShakeIntensity = moved.NewPosition,
        });
    }
}
