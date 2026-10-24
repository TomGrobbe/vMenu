using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class PlateSection
{
    // The game truncates anything longer, so there is no point letting more be typed.
    private const int MaxPlateLength = 8;

    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows(menu));

        SectionRows.AutoFill(menu, () => Rows(menu));
    }

    private static List<MenuEntry> Rows(MenuBuilder menu)
    {
        if (SectionRows.Driven() is null)
        {
            return SectionRows.BlockedOnly();
        }

        return
        [
            new ButtonEntry(Loc.VehicleOptions.PlateText)
            {
                // The plate itself on the right, so the row shows what it currently says without opening anything.
                Label = MenuText.From(() => SectionRows.Driven() is { } current
                    ? API.GetVehicleNumberPlateText(current) ?? string.Empty
                    : string.Empty),

                OnSelectedAsync = _ => ChangeTextAsync(menu),
            },
            new ListEntry(Loc.VehicleOptions.PlateStyle, VehicleOptionTables.PlateStyles)
            {
                ReadSelectedIndex = () => SectionRows.Driven() is { } current
                    ? Math.Clamp(API.GetVehicleNumberPlateTextIndex(current), 0, VehicleOptionTables.PlateStyles.Count - 1)
                    : 0,
                OnIndexChanged = changed =>
                {
                    if (SectionRows.Driven() is { } current)
                    {
                        API.SetVehicleNumberPlateTextIndex(current, changed.NewIndex);
                    }
                },
            },
        ];
    }

    private static async Task ChangeTextAsync(MenuBuilder menu)
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return;
        }

        var current = API.GetVehicleNumberPlateText(handle) ?? string.Empty;

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.VehicleOptions.PlateTextPrompt),
            MaxPlateLength,
            current);

        if (typed is null)
        {
            return;
        }

        if (SectionRows.Driven() is not { } target)
        {
            return;
        }

        API.SetVehicleNumberPlateText(target, typed);

        // The row carries the plate text as its label, and a label is only rewritten on a refresh.
        menu.ReplaceEntries(Rows(menu));
    }
}
