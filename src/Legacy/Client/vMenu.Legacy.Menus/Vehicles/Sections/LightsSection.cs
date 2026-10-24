using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class LightsSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        SectionRows.AutoFill(menu, Rows);
    }

    private static List<MenuEntry> Rows()
    {
        if (SectionRows.DrivenWithModKit() is null)
        {
            return SectionRows.BlockedOnly();
        }

        return
        [
            new CheckboxEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.XenonLights),
                Description = MenuText.Key(
                    Loc.VehicleOptions.XenonLightsDescription,
                    ("number", VehicleModLabels.SlotNumber(VehicleModSlot.XenonLights))),
                ReadState = () => SectionRows.DrivenWithModKit() is { } handle
                    && API.IsToggleModOn(handle, (int)VehicleModSlot.XenonLights),
                OnChanged = changed =>
                {
                    if (SectionRows.DrivenWithModKit() is { } handle)
                    {
                        API.ToggleVehicleMod(handle, (int)VehicleModSlot.XenonLights, changed.Checked);
                    }
                },
            },
            ColorRow(),
            new SubmenuEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.CustomColor),
                Description = MenuText.Key(Loc.VehicleOptions.CustomColorDescription),
                MenuSubtitle = MenuText.Key(Loc.VehicleOptions.HeadlightColor),
                Build = rgb => RgbPicker.Build(rgb, Target()),
            },
        ];
    }

    private static ListEntry ColorRow()
    {
        // The headlights the vehicle came with are not one of the thirteen, so they are added at the end and
        // mapped to the value the game uses for them.
        var options = new List<MenuText>(VehicleLightColors.All.Count + 1);

        foreach (var color in VehicleLightColors.All)
        {
            options.Add(GameLabels.GameOrLiteral(color.GxtKey, GameLabels.Humanise(color.GxtKey)));
        }

        options.Add(MenuText.Key(Loc.VehicleOptions.HeadlightDefault));

        var defaultIndex = options.Count - 1;

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.HeadlightColor),
            Description = MenuText.Key(Loc.VehicleOptions.HeadlightColorDescription),
            Options = options,
            ReadSelectedIndex = () =>
            {
                if (SectionRows.Driven() is not { } handle)
                {
                    return defaultIndex;
                }

                var current = API.GetVehicleHeadlightsColour(handle);

                return current >= 0 && current < defaultIndex ? current : defaultIndex;
            },
            OnIndexChanged = changed =>
            {
                if (SectionRows.DrivenWithModKit() is not { } handle)
                {
                    return;
                }

                if (changed.NewIndex == defaultIndex)
                {
                    API.SetVehicleHeadlightsColour(handle, VehicleLightColors.DefaultHeadlightColor);

                    return;
                }

                // A colour mixed by hand sits on top of the index and would hide whatever is picked here, so it
                // goes first.
                API.ClearVehicleXenonLightsCustomColor(handle);
                API.SetVehicleHeadlightsColour(handle, changed.NewIndex);
            },
        };
    }

    private static RgbTarget Target() => new()
    {
        Read = () =>
        {
            if (SectionRows.Driven() is not { } handle)
            {
                return null;
            }

            try
            {
                return NativeFixer.GetVehicleXenonLightsCustomColor(handle, out var red, out var green, out var blue)
                    ? new RgbValue(red, green, blue)
                    : null;
            }
            catch (Exception)
            {
                // The generated wrapper reads all three output slots whether or not the game filled them, and having
                // no custom colour is the normal case rather than an error.
                return null;
            }
        },
        Write = (red, green, blue) =>
        {
            if (SectionRows.DrivenWithModKit() is { } handle)
            {
                API.SetVehicleXenonLightsCustomColor(handle, red, green, blue);
            }
        },
        Clear = () =>
        {
            if (SectionRows.DrivenWithModKit() is { } handle)
            {
                API.ClearVehicleXenonLightsCustomColor(handle);
            }
        },
    };
}
