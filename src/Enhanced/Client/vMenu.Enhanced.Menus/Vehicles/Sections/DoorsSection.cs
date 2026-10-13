using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class DoorsSection
{
    // Anything past this is standing open rather than merely unlatched.
    private const float OpenAngle = 0.1f;

    private static readonly (int Index, string TextKey)[] Doors =
    [
        (0, Loc.VehicleOptions.DoorFrontLeft),
        (1, Loc.VehicleOptions.DoorFrontRight),
        (2, Loc.VehicleOptions.DoorRearLeft),
        (3, Loc.VehicleOptions.DoorRearRight),
        (4, Loc.VehicleOptions.DoorHood),
        (5, Loc.VehicleOptions.DoorTrunk),
        (6, Loc.VehicleOptions.DoorExtraLeft),
        (7, Loc.VehicleOptions.DoorExtraRight),
    ];

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

        var present = Present(handle);

        var rows = new List<MenuEntry>();

        foreach (var door in present)
        {
            rows.Add(DoorRow(door.Index, door.TextKey));
        }

        if (rows.Count == 0)
        {
            return SectionRows.BlockedOnly();
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.OpenAllDoors),
            Description = MenuText.Key(Loc.VehicleOptions.OpenAllDoorsDescription),
            OnSelected = _ => SetAll(menu, present, open: true),
        });

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.CloseAllDoors),
            Description = MenuText.Key(Loc.VehicleOptions.CloseAllDoorsDescription),
            OnSelected = _ => SetAll(menu, present, open: false),
        });

        rows.Add(RemoveDoorRow(present));

        rows.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.DeleteRemovedDoors),
            Description = MenuText.Key(Loc.VehicleOptions.DeleteRemovedDoorsDescription),
            ReadState = () => UserDefaults.VehicleDeleteRemovedDoors.Value,
            OnChanged = changed => UserDefaults.VehicleDeleteRemovedDoors.Value = changed.Checked,
        });

        return rows;
    }

    // Asked of the game rather than assumed, because a vehicle that has no seventh door does not simply
    // ignore the seventh slot: some models answer for it with the front left door instead. Listing all
    // eight regardless produced two rows that moved together and disagreed about their own state.
    private static List<(int Index, string TextKey)> Present(int handle)
    {
        var present = new List<(int Index, string TextKey)>();

        foreach (var door in Doors)
        {
            if (Native.GetIsDoorValid(handle, door.Index))
            {
                present.Add(door);
            }
        }

        return present;
    }

    private static CheckboxEntry DoorRow(int index, string textKey) => new()
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(Loc.VehicleOptions.DoorDescription),
        ReadState = () => IsOpen(index),
        OnChanged = changed => Set(index, changed.Checked),
    };

    private static ConfirmListEntry RemoveDoorRow(List<(int Index, string TextKey)> present)
    {
        var options = new List<MenuText>(present.Count);

        foreach (var door in present)
        {
            options.Add(MenuText.Key(door.TextKey));
        }

        var picked = 0;

        return new ConfirmListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.RemoveDoor),
            Description = MenuText.Key(Loc.VehicleOptions.RemoveDoorDescription),
            ConfirmationDescription = MenuText.Key(
                Loc.VehicleOptions.RemoveDoorConfirm,
                ("name", MenuText.From(() => NameAt(present, picked)))),
            Options = options,
            OnIndexChanged = changed => picked = changed.NewIndex,
            OnConfirmed = confirmed =>
            {
                if (SectionRows.Driven() is not { } handle
                    || confirmed.SelectedIndex < 0
                    || confirmed.SelectedIndex >= present.Count)
                {
                    return;
                }

                Native.SetVehicleDoorBroken(
                    handle,
                    present[confirmed.SelectedIndex].Index,
                    UserDefaults.VehicleDeleteRemovedDoors.Value);
            },
        };
    }

    // Resolved through the localizer rather than captured, so the confirmation follows a language change
    // like every other piece of text.
    private static string NameAt(List<(int Index, string TextKey)> present, int index) =>
        index >= 0 && index < present.Count
            ? Localizer.Current.Get(present[index].TextKey)
            : string.Empty;

    internal static void ToggleAll()
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return;
        }

        var present = Present(handle);
        var open = true;

        foreach (var door in present)
        {
            if (IsOpen(door.Index))
            {
                open = false;

                break;
            }
        }

        foreach (var door in present)
        {
            Set(door.Index, open);
        }
    }

    private static bool IsOpen(int index) =>
        SectionRows.Driven() is { } handle && Native.GetVehicleDoorAngleRatio(handle, index) > OpenAngle;

    private static void Set(int index, bool open)
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return;
        }

        if (open)
        {
            Native.SetVehicleDoorOpen(handle, index, false, false);

            return;
        }

        Native.SetVehicleDoorShut(handle, index, false);
    }

    private static void SetAll(MenuBuilder menu, List<(int Index, string TextKey)> present, bool open)
    {
        foreach (var door in present)
        {
            Set(door.Index, open);
        }

        // A tick box only re-reads its state when the menu is refreshed, so without this the rows above
        // would still show whatever they showed before the button was pressed.
        SectionRows.Fill(menu, Rows(menu));
    }
}
