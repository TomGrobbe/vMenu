using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Extensions;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

/// <summary>
/// The bits every modification section needs: finding the vehicle, and swapping its rows out.
/// </summary>
/// <remarks>
/// Sections are filled from the vehicle the player is in, so their rows are runtime data rather than
/// a fixed declaration. Each one refills itself when it is opened, which is also what keeps it
/// honest after the player has driven a different car.
/// </remarks>
internal static class SectionRows
{
    /// <summary>The vehicle the player is driving, or null. Silent, unlike <see cref="OwnVehicle"/>.</summary>
    // A menu that opens on a row explaining the problem beats one that fires a notification the
    // moment it appears, so nothing is said here.
    internal static int? Driven()
    {
        var ped = API.Players.Local.Ped;

        if (ped is null || ped.IsDeadOrDying)
        {
            return null;
        }

        var target = VehicleTargeting.Current(ped);

        if (!target.Found || target.Kind is VehicleTargetKind.Passenger)
        {
            return null;
        }

        return target.Handle;
    }

    /// <summary>
    /// The vehicle, with its mod kit installed so the game will answer questions about its upgrades.
    /// </summary>
    internal static int? DrivenWithModKit()
    {
        if (Driven() is not { } handle)
        {
            return null;
        }

        Native.SetVehicleModKit(handle, 0);

        return handle;
    }

    /// <summary>Replaces a section's rows, leaving the player's highlight where they left it.</summary>
    // Rebuilding drops every item and MenuAPI puts the highlight back on the first one, which moves
    // the selection out from under a player who is looking at the menu while it happens.
    internal static void Fill(MenuBuilder builder, IReadOnlyList<MenuEntry> rows)
    {
        var was = builder.Menu.CurrentIndex;
        var offset = builder.Menu.ViewIndexOffset;

        builder.ClearEntries();
        builder.AddRange(rows);

        var keep = was < builder.Menu.GetMenuItems().Count;

        // The scroll offset as well as the index, or a player partway down a long list keeps their
        // row but has the list scrolled back to the top under them.
        builder.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }

    /// <summary>
    /// Refills a section when it opens, and again whenever the player changes what they are in.
    /// </summary>
    internal static void AutoFill(MenuBuilder builder, Func<IReadOnlyList<MenuEntry>> rows) =>
        AutoRefresh(builder, () => Fill(builder, rows()));

    /// <summary>As <see cref="AutoFill"/>, for a menu whose rows are declared rather than generated.</summary>
    internal static void AutoRefresh(MenuBuilder builder, Action refresh)
    {
        void OnChanged(VehicleChanged _) => refresh();

        builder.OnOpened = _ =>
        {
            refresh();

            // Dropped before it is added, because MenuAPI raises the open callback again when a
            // deferred filter puts the menu back up, and a section subscribed twice refills twice.
            LocalVehicleTicks.VehicleChanged -= OnChanged;
            LocalVehicleTicks.VehicleChanged += OnChanged;
        };

        // MenuAPI closes a menu in order to open its child, so walking into a submenu unsubscribes
        // here and the child subscribes for itself.
        builder.OnClosed = _ => LocalVehicleTicks.VehicleChanged -= OnChanged;
    }

    /// <summary>Shown instead of an empty menu, which MenuAPI will not let the player leave with the arrow keys.</summary>
    internal static MenuEntry Placeholder(string textKey, string descriptionKey) => new ButtonEntry
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(descriptionKey),
    };

    /// <summary>A whole section replaced by the one row saying why there is nothing to change.</summary>
    internal static IReadOnlyList<MenuEntry> BlockedOnly() => [Blocked()];

    /// <summary>The row shown instead of a section, saying why there is nothing to change.</summary>
    internal static MenuEntry Blocked()
    {
        var ped = API.Players.Local.Ped;

        // Sitting in somebody else's passenger seat is a different problem from standing in the
        // street, and being told the right one is the difference between fixable and baffling.
        if (ped is not null && VehicleTargeting.Current(ped) is { Found: true, Kind: VehicleTargetKind.Passenger })
        {
            return Placeholder(Loc.VehicleOptions.NotDriverTitle, Loc.VehicleOptions.NotDriver);
        }

        return Placeholder(Loc.VehicleOptions.NoVehicle, Loc.VehicleOptions.NoVehicleDescription);
    }

    internal static MenuEntry Nothing() =>
        Placeholder(Loc.VehicleOptions.Nothing, Loc.VehicleOptions.NothingDescription);
}
