using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class RadioStationsSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows(menu));

        menu.OnOpened = _ => menu.ReplaceEntries(Rows(menu));
    }

    private static List<MenuEntry> Rows(MenuBuilder menu)
    {
        var stations = VehicleRadio.All();

        if (stations.Count == 0)
        {
            return [SectionRows.Nothing()];
        }

        var rows = new List<MenuEntry>(stations.Count + 1)
        {
            new ButtonEntry(Loc.VehicleOptions.RadioUnblockAll)
            {
                OnSelected = _ =>
                {
                    VehicleRadio.ClearBlocked();

                    menu.ReplaceEntries(Rows(menu));
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
