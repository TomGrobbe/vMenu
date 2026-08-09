using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

/// <summary>
/// The paint jobs printed onto the bodywork.
/// </summary>
/// <remarks>
/// These are the vehicle's own liveries. Liveries that arrived with a mod kit are an upgrade slot
/// like any other, so they show up in the modifications menu instead.
/// </remarks>
internal static class LiveriesSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        SectionRows.AutoFill(menu, Rows);
    }

    private static IReadOnlyList<MenuEntry> Rows()
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return SectionRows.BlockedOnly();
        }

        var rows = new List<MenuEntry>();

        var liveries = Native.GetVehicleLiveryCount(handle);

        if (liveries > 0)
        {
            rows.Add(LiveryRow(
                handle,
                Loc.VehicleOptions.Livery,
                Loc.VehicleOptions.LiveryDescription,
                liveries,
                Native.GetVehicleLivery,
                Native.SetVehicleLivery));
        }

        var roofLiveries = Native.GetVehicleRoofLiveryCount(handle);

        if (roofLiveries > 0)
        {
            rows.Add(LiveryRow(
                handle,
                Loc.VehicleOptions.RoofLivery,
                Loc.VehicleOptions.RoofLiveryDescription,
                roofLiveries,
                Native.GetVehicleRoofLivery,
                Native.SetVehicleRoofLivery));
        }

        if (rows.Count == 0)
        {
            rows.Add(SectionRows.Nothing());
        }

        return rows;
    }

    private static ListEntry LiveryRow(
        int handle,
        string textKey,
        string descriptionKey,
        int count,
        Func<int, int> read,
        Action<int, int> write)
    {
        // The game counts from zero and uses -1 for none, so "None" is prepended and everything
        // shifts by one.
        var options = new List<MenuText>(count + 1)
        {
            MenuText.Key(Loc.VehicleOptions.LiveryNone),
        };

        for (var index = 0; index < count; index++)
        {
            var labelKey = Native.GetLiveryName(handle, index);
            var number = (index + 1).ToString(CultureInfo.InvariantCulture);

            options.Add(GameLabels.Exists(labelKey)
                ? GameLabels.GameOrLiteral(labelKey, string.Empty)
                : MenuText.Key(
                    Loc.VehicleOptions.Numbered,
                    ("name", MenuText.Key(textKey)),
                    ("number", MenuText.Literal(number))));
        }

        return new ListEntry
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(descriptionKey),
            Options = options,
            ReadSelectedIndex = () => SectionRows.Driven() is { } current
                ? Math.Clamp(read(current) + 1, 0, options.Count - 1)
                : 0,
            OnIndexChanged = changed =>
            {
                if (SectionRows.DrivenWithModKit() is { } current)
                {
                    write(current, changed.NewIndex - 1);
                }
            },
        };
    }
}
