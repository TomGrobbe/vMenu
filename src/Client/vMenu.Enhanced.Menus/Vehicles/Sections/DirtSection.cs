using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

/// <summary>
/// How dirty the vehicle looks.
/// </summary>
/// <remarks>
/// One row rather than a menu of its own, since there is only ever one thing to say about it. Not
/// the same as washing the vehicle, which also takes the scrapes and bullet holes off the paint.
/// </remarks>
internal static class DirtSection
{
    /// <summary>The game's scale runs from clean to this.</summary>
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

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.DirtLevel),
            Description = MenuText.Key(Loc.VehicleOptions.DirtLevelDescription),
            Options = options,
            Gate = gate,
            ReadSelectedIndex = () => SectionRows.Driven() is { } handle
                ? Math.Clamp((int)Native.GetVehicleDirtLevel(handle), 0, MaxDirt)
                : 0,
            OnIndexChanged = changed =>
            {
                if (SectionRows.Driven() is { } handle)
                {
                    Native.SetVehicleDirtLevel(handle, changed.NewIndex);
                }
            },
        };
    }
}
