using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;

using FilmmakerPermissions = vMenu.Enhanced.Data.Permissions.Menus.Filmmaker;
using FreecamCamera = vMenu.Enhanced.Freecam.Freecam;

namespace vMenu.Enhanced.Menus.Filmmaking;

[VMenu(
    TitleKey = Loc.Filmmaker.Title,
    SubtitleKey = Loc.Filmmaker.Subtitle,
    DescriptionKey = Loc.Filmmaker.LinkDescription,
    Permission = FilmmakerPermissions.Menu)]
public sealed class FilmmakerMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new CheckboxEntry(Loc.Filmmaker.FreecamToggle)
        {
            Gate = Global.Freecam,
            ReadState = () => FreecamCamera.IsActive,
            OnChanged = changed =>
            {
                if (!changed.Checked)
                {
                    FreecamCamera.Disable();
                }
                else if (!FreecamCamera.Enable())
                {
                    menu.Refresh();
                }
            },
        });

        menu.Entries.Add(SubmenuEntry.For(new FilmmakerCameraMenu()));
        menu.Entries.Add(SubmenuEntry.For(new FilmmakerEffectsMenu()));
        menu.Entries.Add(SubmenuEntry.For(new FilmmakerDepthOfFieldMenu()));
        menu.Entries.Add(SubmenuEntry.For(new FilmmakerTargetMenu()));
        menu.Entries.Add(SubmenuEntry.For(new FilmmakerShotsMenu()));
        menu.Entries.Add(SubmenuEntry.For(new FilmmakerPathMenu()));

        FreecamCamera.StateChanged += menu.Refresh;
    }
}
