using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class RadioStationsSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows(menu));

        menu.OnOpened = _ => SectionRows.Fill(menu, Rows(menu));
    }

    private static IReadOnlyList<MenuEntry> Rows(MenuBuilder menu)
    {
        var stations = VehicleRadio.All();

        if (stations.Count == 0)
        {
            return [SectionRows.Nothing()];
        }

        var rows = new List<MenuEntry>(stations.Count + 1)
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.RadioUnblockAll),
                Description = MenuText.Key(Loc.VehicleOptions.RadioUnblockAllDescription),
                OnSelected = _ =>
                {
                    VehicleRadio.ClearBlocked();

                    SectionRows.Fill(menu, Rows(menu));
                },
            },
        };

        foreach (var station in stations)
        {
            rows.Add(Row(station));
        }

        return rows;
    }

    private static CheckboxEntry Row(string station) => new()
    {
        Text = MenuText.Literal(RadioStations.DisplayName(station)),
        Description = MenuText.Key(Loc.VehicleOptions.RadioStationDescription),
        ReadState = () => !VehicleRadio.IsBlocked(station),
        OnChanged = changed => VehicleRadio.SetBlocked(station, !changed.Checked),
    };
}
