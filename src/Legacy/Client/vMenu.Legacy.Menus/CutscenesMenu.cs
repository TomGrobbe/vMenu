using vMenu.Enhanced.Data.Cutscenes;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Cutscenes;

using CutscenesPermissions = vMenu.Enhanced.Data.Permissions.Menus.Cutscenes;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.Cutscenes.Title,
    SubtitleKey = Loc.Cutscenes.Subtitle,
    DescriptionKey = Loc.Cutscenes.LinkDescription)]
public sealed class CutscenesMenu : MenuDefinition
{
    public override MenuGate Gate => MenuGate.Permission(CutscenesPermissions.Menu);

    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.AddRange(CutsceneCatalog.All.Select(entry => new ButtonEntry
        {
            Text = MenuText.Key(Loc.Cutscenes.Name(entry.Id)),
            Description = MenuText.Key(Loc.Cutscenes.Description(entry.Id)),
            OnSelectedAsync = _ => CutscenePlayer.FromMenuAsync(entry.Id),
        }));
    }
}
