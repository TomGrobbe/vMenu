using System.Globalization;

using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Weapons;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.WeaponOptions.GameLoadout,
    SubtitleKey = Loc.WeaponOptions.GameLoadoutSubtitle,
    DescriptionKey = Loc.WeaponOptions.GameLoadoutDescription,
    Permission = WeaponOptionsPermissions.GameLoadouts)]
public sealed class GameLoadoutsMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        foreach (var (heading, loadouts) in GameLoadouts.Groups)
        {
            menu.Entries.Add(new SeparatorEntry { Text = heading });

            foreach (var (name, label) in loadouts)
            {
                menu.Entries.Add(new ButtonEntry
                {
                    Text = label,
                    Description = MenuText.Literal(name),
                    OnSelected = _ => Equip(name),
                });
            }
        }
    }

    private static void Equip(string name)
    {
        var (given, removed) = GameLoadouts.Apply(name);

        MenuAudit.ReportAction(AuditActions.GameLoadoutEquipped, name);

        if (removed > 0)
        {
            Notifications.Warning(MenuText.Key(
                Loc.WeaponOptions.GameLoadoutRemoved,
                ("count", MenuText.Literal(removed.ToString(CultureInfo.InvariantCulture)))));
        }

        if (given == 0)
        {
            Notifications.Error(MenuText.Key(Loc.WeaponOptions.GameLoadoutEmpty));

            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.WeaponOptions.GameLoadoutDone,
            ("count", MenuText.Literal(given.ToString(CultureInfo.InvariantCulture)))));
    }
}
