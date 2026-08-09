using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

/// <summary>Underglow: which tubes are lit, and what colour they glow.</summary>
internal static class NeonSection
{
    private const int Left = 0;

    private const int Right = 1;

    private const int Front = 2;

    private const int Back = 3;

    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        SectionRows.AutoFill(menu, Rows);
    }

    private static IReadOnlyList<MenuEntry> Rows()
    {
        if (SectionRows.Driven() is null)
        {
            return SectionRows.BlockedOnly();
        }

        return
        [
            Tube(Loc.VehicleOptions.NeonFront, Front),
            Tube(Loc.VehicleOptions.NeonRear, Back),
            Tube(Loc.VehicleOptions.NeonLeft, Left),
            Tube(Loc.VehicleOptions.NeonRight, Right),
            ColorRow(),
            new SubmenuEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.CustomColor),
                Description = MenuText.Key(Loc.VehicleOptions.CustomColorDescription),
                MenuSubtitle = MenuText.Key(Loc.VehicleOptions.NeonColor),
                Build = rgb => RgbPicker.Build(rgb, Target()),
            },
        ];
    }

    private static CheckboxEntry Tube(string textKey, int side) => new()
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(Loc.VehicleOptions.NeonSideDescription),
        ReadState = () => SectionRows.Driven() is { } handle && Native.IsVehicleNeonLightEnabled(handle, side),
        OnChanged = changed =>
        {
            if (SectionRows.Driven() is not { } handle)
            {
                return;
            }

            if (changed.Checked)
            {
                SettleColor(handle);
            }

            Native.SetVehicleNeonLightEnabled(handle, side, changed.Checked);
        },
    };

    /// <summary>
    /// Makes the colour on the vehicle agree with the colour the list is showing.
    /// </summary>
    // A vehicle that has never had neon fitted reports a magenta the palette does not contain, so the
    // list rests on its first entry and says white while the tubes light up magenta. Rather than let
    // the row lie, the colour it is showing is applied before the first tube comes on.
    private static void SettleColor(int handle)
    {
        Native.GetVehicleNeonLightsColour(handle, out var red, out var green, out var blue);

        if (VehicleLightColors.IndexOfRgb(red, green, blue) >= 0)
        {
            return;
        }

        if (VehicleLightColors.At(0) is { } fallback)
        {
            Native.SetVehicleNeonLightsColour(handle, fallback.Red, fallback.Green, fallback.Blue);
        }
    }

    private static ListEntry ColorRow()
    {
        var options = new List<MenuText>(VehicleLightColors.All.Count);

        foreach (var color in VehicleLightColors.All)
        {
            options.Add(GameLabels.GameOrLiteral(color.GxtKey, GameLabels.Humanise(color.GxtKey)));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.NeonColor),
            Description = MenuText.Key(Loc.VehicleOptions.NeonColorDescription),
            Options = options,
            ReadSelectedIndex = () =>
            {
                if (Target().Read() is not { } current)
                {
                    return 0;
                }

                var index = VehicleLightColors.IndexOfRgb(current.Red, current.Green, current.Blue);

                // A colour mixed by hand matches nothing here, so the row rests on the first entry.
                return index < 0 ? 0 : index;
            },
            OnIndexChanged = changed =>
            {
                if (SectionRows.Driven() is not { } handle || VehicleLightColors.At(changed.NewIndex) is not { } color)
                {
                    return;
                }

                Native.SetVehicleNeonLightsColour(handle, color.Red, color.Green, color.Blue);
            },
        };
    }

    // No Clear: the tubes always have some colour, so there is no "not set" for the picker to
    // return them to.
    private static RgbTarget Target() => new()
    {
        Read = () =>
        {
            if (SectionRows.Driven() is not { } handle)
            {
                return null;
            }

            Native.GetVehicleNeonLightsColour(handle, out var red, out var green, out var blue);

            return new RgbValue(red, green, blue);
        },
        Write = (red, green, blue) =>
        {
            if (SectionRows.Driven() is { } handle)
            {
                Native.SetVehicleNeonLightsColour(handle, red, green, blue);
            }
        },
    };
}
