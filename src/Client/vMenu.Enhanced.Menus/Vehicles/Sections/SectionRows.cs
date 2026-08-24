using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

// Sections are filled from the vehicle the player is in, so their rows are runtime data rather than a
// fixed declaration. Each one refills itself when it is opened, which is also what keeps it honest
// after the player has driven a different car.
internal static class SectionRows
{
    internal static int? Driven()
    {
        var handle = OwnVehicle.Driven();

        return handle == 0 ? null : handle;
    }

    // With its mod kit installed, so the game will answer questions about its upgrades.
    internal static int? DrivenWithModKit()
    {
        if (Driven() is not { } handle)
        {
            return null;
        }

        Native.SetVehicleModKit(handle, 0);

        return handle;
    }

    // Rebuilding drops every item and MenuAPI puts the highlight back on the first one, which moves the
    // selection out from under a player who is looking at the menu while it happens.
    internal static void Fill(MenuBuilder builder, IReadOnlyList<MenuEntry> rows)
    {
        var was = builder.Menu.CurrentIndex;
        var offset = builder.Menu.ViewIndexOffset;

        builder.ClearEntries();
        builder.AddRange(rows);

        var keep = was < builder.Menu.GetMenuItems().Count;

        // The scroll offset as well as the index, or a player partway down a long list keeps their row but
        // has the list scrolled back to the top under them.
        builder.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }

    internal static void AutoFill(MenuBuilder builder, Func<IReadOnlyList<MenuEntry>> rows) =>
        AutoRefresh(builder, () => Fill(builder, rows()));

    internal static void AutoRefresh(MenuBuilder builder, Action refresh)
    {
        void OnChanged(VehicleChanged _) => refresh();

        builder.OnOpened = _ =>
        {
            refresh();

            // Dropped before it is added, because MenuAPI raises the open callback again when a deferred filter
            // puts the menu back up, and a section subscribed twice refills twice.
            LocalVehicleTicks.VehicleChanged -= OnChanged;
            LocalVehicleTicks.VehicleChanged += OnChanged;
        };

        // MenuAPI closes a menu in order to open its child, so walking into a submenu unsubscribes here and
        // the child subscribes for itself.
        builder.OnClosed = _ => LocalVehicleTicks.VehicleChanged -= OnChanged;
    }

    // Shown instead of an empty menu, which MenuAPI will not let the player leave with the arrow keys.
    internal static MenuEntry Placeholder(string textKey, string descriptionKey) => new ButtonEntry
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(descriptionKey),
    };

    internal static IReadOnlyList<MenuEntry> BlockedOnly() => [Blocked()];

    internal static MenuEntry Blocked()
    {
        var ped = API.Players.Local.Ped;

        // Sitting in somebody else's passenger seat is a different problem from standing in the street, and
        // being told the right one is the difference between fixable and baffling.
        if (ped is not null && VehicleTargeting.Current(ped) is { Found: true, Kind: VehicleTargetKind.Passenger })
        {
            return Placeholder(Loc.VehicleOptions.NotDriverTitle, Loc.VehicleOptions.NotDriver);
        }

        return Placeholder(Loc.VehicleOptions.NoVehicle, Loc.VehicleOptions.NoVehicleDescription);
    }

    internal static MenuEntry Nothing() =>
        Placeholder(Loc.VehicleOptions.Nothing, Loc.VehicleOptions.NothingDescription);
}
