using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;
using PedModelsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PedModels;
using PlayerAppearancePermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerAppearance;
using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;
using SavedPedsPermissions = vMenu.Enhanced.Data.Permissions.Menus.SavedPeds;
using WeaponLoadoutsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponLoadouts;
using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.PlayerMenu.Title,
    SubtitleKey = Loc.PlayerMenu.Subtitle,
    DescriptionKey = Loc.PlayerMenu.LinkDescription)]
public sealed class PlayerMenu : MenuDefinition
{
    // Open to anybody who can reach at least one of the menus inside it.
    public override MenuGate Gate { get; } =
        MenuGate.Permission(PlayerOptionsPermissions.Menu)
        | MenuGate.Permission(PedModelsPermissions.Menu)
        | MenuGate.Permission(PlayerAppearancePermissions.Menu)
        | MenuGate.Permission(CharacterCreatorPermissions.Menu)
        | MenuGate.Permission(SavedPedsPermissions.Menu)
        | MenuGate.Permission(WeaponOptionsPermissions.Menu)
        | MenuGate.Permission(WeaponLoadoutsPermissions.Menu);

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(SubmenuEntry.For(new PlayerOptionsMenu()));
        menu.Entries.Add(SubmenuEntry.For(new PedModelsMenu()));

        menu.Entries.Add(SubmenuEntry.For(new PlayerAppearanceMenu()));
        menu.Entries.Add(SubmenuEntry.For(new CharacterCreatorMenu()));
        menu.Entries.Add(SubmenuEntry.For(new SavedPedsMenu()));

        menu.Entries.Add(SubmenuEntry.For(new WeaponOptionsMenu()));
        menu.Entries.Add(SubmenuEntry.For(new WeaponLoadoutsMenu()));
    }
}
