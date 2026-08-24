using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

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

    private static IReadOnlyList<MenuEntry> Rows(MenuBuilder menu)
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return SectionRows.BlockedOnly();
        }

        return
        [
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.PlateText),
                Description = MenuText.Key(Loc.VehicleOptions.PlateTextDescription),

                // The plate itself on the right, so the row shows what it currently says without opening anything.
                Label = MenuText.From(() => SectionRows.Driven() is { } current
                    ? Native.GetVehicleNumberPlateText(current) ?? string.Empty
                    : string.Empty),

                OnSelectedAsync = _ => ChangeTextAsync(menu),
            },
            new ListEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.PlateStyle),
                Description = MenuText.Key(Loc.VehicleOptions.PlateStyleDescription),
                Options = VehicleOptionTables.PlateStyles,
                ReadSelectedIndex = () => SectionRows.Driven() is { } current
                    ? Math.Clamp(Native.GetVehicleNumberPlateTextIndex(current), 0, VehicleOptionTables.PlateStyles.Count - 1)
                    : 0,
                OnIndexChanged = changed =>
                {
                    if (SectionRows.Driven() is { } current)
                    {
                        Native.SetVehicleNumberPlateTextIndex(current, changed.NewIndex);
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

        var current = Native.GetVehicleNumberPlateText(handle) ?? string.Empty;

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

        Native.SetVehicleNumberPlateText(target, typed);

        // The row carries the plate text as its label, and a label is only rewritten on a refresh.
        SectionRows.Fill(menu, Rows(menu));
    }
}
