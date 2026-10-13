using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

// Each window is a list of the two things that can be done to it rather than a tick box, because the
// game will not say whether a window is currently up or down. A tick box would have to guess, and
// would then be wrong for anyone who wound a window down before opening this menu.
internal static class WindowsSection
{
    private const int RollUp = 0;

    private static readonly (int Index, string TextKey)[] Windows =
    [
        (0, Loc.VehicleOptions.WindowFrontLeft),
        (1, Loc.VehicleOptions.WindowFrontRight),
        (2, Loc.VehicleOptions.WindowRearLeft),
        (3, Loc.VehicleOptions.WindowRearRight),
    ];

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

        var rows = new List<MenuEntry>();

        foreach (var window in Windows)
        {
            rows.Add(WindowRow(window.Index, window.TextKey));
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.RollDownAllWindows),
            Description = MenuText.Key(Loc.VehicleOptions.RollDownAllWindowsDescription),
            OnSelected = _ => SetAll(up: false),
        });

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.RollUpAllWindows),
            Description = MenuText.Key(Loc.VehicleOptions.RollUpAllWindowsDescription),
            OnSelected = _ => SetAll(up: true),
        });

        rows.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.SmashWindows),
            Description = MenuText.Key(Loc.VehicleOptions.SmashWindowsDescription),
            ConfirmationDescription = MenuText.Key(Loc.VehicleOptions.SmashWindowsConfirm),
            OnConfirmed = _ =>
            {
                if (SectionRows.Driven() is not { } handle)
                {
                    return;
                }

                foreach (var window in Windows)
                {
                    Native.SmashVehicleWindow(handle, window.Index);
                }
            },
        });

        return rows;
    }

    private static ListEntry WindowRow(int index, string textKey) => new()
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(Loc.VehicleOptions.WindowDescription),
        Options =
        [
            MenuText.Key(Loc.VehicleOptions.WindowRollUp),
            MenuText.Key(Loc.VehicleOptions.WindowRollDown),
        ],

        // On enter rather than on scroll: these are actions, and scrolling past one should not perform it.
        OnSelected = selected => Set(index, selected.SelectedIndex == RollUp),
    };

    private static void Set(int index, bool up)
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return;
        }

        if (up)
        {
            Native.RollUpWindow(handle, index);

            return;
        }

        Native.RollDownWindow(handle, index);
    }

    private static void SetAll(bool up)
    {
        foreach (var window in Windows)
        {
            Set(window.Index, up);
        }
    }
}
