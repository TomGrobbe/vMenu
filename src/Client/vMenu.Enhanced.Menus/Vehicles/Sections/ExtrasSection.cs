using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

/// <summary>
/// The optional parts a vehicle was built with: push bars, roof racks, light bars and the like.
/// </summary>
/// <remarks>
/// The game has no name for any of them, only a number, so that is what the rows show. Which part a
/// number turns out to be is different on every vehicle.
/// </remarks>
internal static class ExtrasSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        menu.OnOpened = _ => SectionRows.Fill(menu, Rows());
    }

    private static List<MenuEntry> Rows()
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return SectionRows.BlockedOnly();
        }

        var rows = new List<MenuEntry>();

        for (var id = 0; id < VehicleAppearanceReader.ExtraCount; id++)
        {
            if (!Native.DoesExtraExist(handle, id))
            {
                continue;
            }

            rows.Add(ExtraRow(id));
        }

        if (rows.Count == 0)
        {
            rows.Add(SectionRows.Nothing());
        }

        return rows;
    }

    private static CheckboxEntry ExtraRow(int id)
    {
        var number = MenuText.Literal(id.ToString(CultureInfo.InvariantCulture));

        return new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.ExtraName, ("number", number)),
            Description = MenuText.Key(Loc.VehicleOptions.ExtraDescription, ("number", number)),
            ReadState = () => SectionRows.Driven() is { } handle && Native.IsVehicleExtraTurnedOn(handle, id),
            OnChanged = changed =>
            {
                if (SectionRows.Driven() is not { } handle)
                {
                    return;
                }

                // The flag says whether to turn the extra off, not on.
                Native.SetVehicleExtra(handle, id, !changed.Checked);
            },
        };
    }
}
