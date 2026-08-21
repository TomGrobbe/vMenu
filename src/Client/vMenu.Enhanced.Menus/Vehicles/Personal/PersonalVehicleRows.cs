using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Sections;

namespace vMenu.Enhanced.Menus.Vehicles.Personal;

internal static class PersonalVehicleRows
{
    internal static void AutoFill(MenuBuilder builder, Func<IReadOnlyList<MenuEntry>> rows)
    {
        void Refill() => SectionRows.Fill(builder, rows());

        builder.OnOpened = _ =>
        {
            Refill();

            PersonalVehicle.Changed -= Refill;
            PersonalVehicle.Changed += Refill;
        };

        builder.OnClosed = _ => PersonalVehicle.Changed -= Refill;
    }

    internal static MenuEntry NoneMarked() => new ButtonEntry
    {
        Text = MenuText.Key(Loc.PersonalVehicle.StatusNone),
        Description = MenuText.Key(Loc.PersonalVehicle.NoneMarked),
        ReadEnabled = static () => false,
    };
}
