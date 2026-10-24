using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

// One row rather than a menu of its own, since there is only ever one thing to say about it. Not the
// same as washing the vehicle, which also takes the scrapes and bullet holes off the paint.
internal static class DirtSection
{
    // The game's scale runs from clean to this.
    private const int MaxDirt = 15;

    public static MenuEntry Row(MenuGate gate)
    {
        var options = new List<MenuText>(MaxDirt + 1)
        {
            MenuText.Key(Loc.VehicleOptions.DirtClean),
        };

        for (var level = 1; level <= MaxDirt; level++)
        {
            var number = level.ToString(CultureInfo.InvariantCulture);

            options.Add(MenuText.Key(Loc.VehicleOptions.DirtValue, ("number", MenuText.Literal(number))));
        }

        return new ListEntry(Loc.VehicleOptions.DirtLevel, options)
        {
            Gate = gate,
            ReadSelectedIndex = () => SectionRows.Driven() is { } handle
                ? Math.Clamp((int)API.GetVehicleDirtLevel(handle), 0, MaxDirt)
                : 0,
            OnIndexChanged = changed =>
            {
                if (SectionRows.Driven() is { } handle)
                {
                    API.SetVehicleDirtLevel(handle, changed.NewIndex);
                }
            },
        };
    }
}
