using vMenu.Enhanced.Freecam;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;

using FilmmakerPermissions = vMenu.Enhanced.Data.Permissions.Menus.Filmmaker;
using FreecamCamera = vMenu.Enhanced.Freecam.Freecam;

namespace vMenu.Enhanced.Menus.Filmmaking;

[VMenu(
    TitleKey = Loc.Filmmaker.ShotsTitle,
    SubtitleKey = Loc.Filmmaker.ShotsSubtitle,
    DescriptionKey = Loc.Filmmaker.ShotsLinkDescription,
    Permission = FilmmakerPermissions.Shots)]
public sealed class FilmmakerShotsMenu : MenuDefinition
{
    private DetachedMenu? _shotMenu;

    private FilmEntry<FilmShot>? _shot;

    private MenuBuilder? _root;

    protected override void Build(MenuBuilder menu)
    {
        _root = menu;

        _shotMenu = menu.AddDetachedMenu(
            MenuText.From(() => _shot?.Item.Name ?? string.Empty),
            MenuText.Key(Loc.Filmmaker.ShotsTitle),
            _ => { },
            MenuGate.When(() => _shot is not null));

        _shotMenu.Builder.OnOpened = _ => _shotMenu.Builder.ReplaceEntries(ShotRows());

        menu.AddRange(RootRows());

        menu.OnOpened = _ => menu.ReplaceEntries(RootRows());
    }

    private List<MenuEntry> RootRows()
    {
        var rows = new List<MenuEntry>
        {
            new ButtonEntry(Loc.Filmmaker.ShotSave)
            {
                ReadEnabled = () => FreecamCamera.IsActive,
                OnSelectedAsync = _ => SaveAsync(),
            },
            new SeparatorEntry(Loc.Filmmaker.ShotsGroupSaved),
        };

        var shots = FilmStores.Shots.All();

        if (shots.Count == 0)
        {
            rows.Add(FilmmakerText.Placeholder(Loc.Filmmaker.ShotsEmpty));
        }

        foreach (var entry in shots)
        {
            var picked = entry;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(entry.Item.Name),
                Label = MenuText.Literal("→"),
                OnSelected = _ =>
                {
                    _shot = picked;

                    _shotMenu?.Open();
                },
            });
        }

        return rows;
    }

    private List<MenuEntry> ShotRows()
    {
        if (_shot is not { } entry)
        {
            return [];
        }

        return
        [
            new ButtonEntry(Loc.Filmmaker.ShotGoTo)
            {
                ReadEnabled = () => FreecamCamera.IsActive,
                OnSelected = _ => FreecamCamera.SetPose(entry.Item.ToPose()),
            },
            new ConfirmButtonEntry(Loc.Filmmaker.ShotOverwrite)
            {
                ReadEnabled = () => FreecamCamera.IsActive && !entry.IsFromNewerBuild,
                OnConfirmed = _ =>
                {
                    var replacement = FilmShot.From(entry.Item.Name, FreecamCamera.Pose);

                    if (!FilmmakerText.Report(FilmStores.Shots.Save(replacement, replacing: true), Loc.Filmmaker.ShotSaved, replacement.Name))
                    {
                        return;
                    }

                    _shot = new FilmEntry<FilmShot>(replacement, false);

                    Rebuild();
                },
            },
            new ButtonEntry(Loc.Filmmaker.Rename)
            {
                ReadEnabled = () => !entry.IsFromNewerBuild,
                OnSelectedAsync = _ => RenameAsync(entry),
            },
            new ConfirmButtonEntry(Loc.Filmmaker.Delete)
            {
                OnConfirmed = _ =>
                {
                    FilmStores.Shots.Delete(entry.Item.Name);

                    Notifications.Success(MenuText.Key(Loc.Filmmaker.Deleted, ("name", MenuText.Literal(entry.Item.Name))));

                    _shot = null;

                    Rebuild();

                    _shotMenu?.Menu.GoBack();
                },
            },
        ];
    }

    private async Task SaveAsync()
    {
        if (FilmmakerText.NeedsFreecam())
        {
            return;
        }

        var pose = FreecamCamera.Pose;

        if (await FilmmakerText.PromptNameAsync() is { } name
            && FilmmakerText.Report(FilmStores.Shots.Save(FilmShot.From(name, pose), replacing: false), Loc.Filmmaker.ShotSaved, name))
        {
            Rebuild();
        }
    }

    private async Task RenameAsync(FilmEntry<FilmShot> entry)
    {
        if (await FilmmakerText.PromptNameAsync(entry.Item.Name) is { } name
            && FilmmakerText.Report(FilmStores.Shots.Rename(entry.Item, name), Loc.Filmmaker.Renamed, name))
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        _root?.ReplaceEntries(RootRows());
        _shotMenu?.Builder.ReplaceEntries(ShotRows());
    }
}
