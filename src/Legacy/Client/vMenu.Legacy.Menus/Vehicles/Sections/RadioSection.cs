using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class RadioSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        menu.OnOpened = _ => menu.ReplaceEntries(Rows());
    }

    private static List<MenuEntry> Rows()
    {
        var stations = VehicleRadio.Selectable();
        var options = new List<MenuText>(stations.Count);

        foreach (var station in stations)
        {
            options.Add(station == RadioStations.Off
                ? MenuText.Key(Loc.VehicleOptions.RadioOff)
                : MenuText.Literal(RadioStations.DisplayName(station)));
        }

        return
        [
            new CheckboxEntry(Loc.VehicleOptions.RadioDefaultEnabled)
            {
                ReadState = () => VehicleRadio.DefaultEnabled,
                OnChanged = changed =>
                {
                    VehicleRadio.SetDefaultEnabled(changed.Checked);

                    MenuRegistry.Refresh(changed.Menu);
                },
            },
            new ListEntry(Loc.VehicleOptions.RadioDefaultStation, options)
            {
                LockedDescription = MenuText.Key(Loc.VehicleOptions.RadioDefaultStationLocked),
                Gate = MenuGate.When(static () => VehicleRadio.DefaultEnabled),
                ReadSelectedIndex = () => Math.Max(IndexOf(stations, VehicleRadio.DefaultStation), 0),
                OnIndexChanged = changed => VehicleRadio.SetDefaultStation(stations[changed.NewIndex]),
            },
            new SubmenuEntry(Loc.VehicleOptions.RadioBlocked)
            {
                MenuTitle = MenuText.Key(Loc.VehicleOptions.Title),
                MenuSubtitle = MenuText.Key(Loc.VehicleOptions.RadioBlockedSubtitle),
                Build = RadioStationsSection.Build,
            },
        ];
    }

    private static int IndexOf(IReadOnlyList<string> stations, string station)
    {
        for (var index = 0; index < stations.Count; index++)
        {
            if (string.Equals(stations[index], station, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }
}
