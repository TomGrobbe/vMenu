using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleSpawnShortcuts
{
    private const int ButtonIntervalMs = 500;

    // A key mapping's icon only comes back from group 0.
    private const int ControlGroup = 0;

    private const int KeyboardGroup = 2;

    private static readonly List<Menu> Menus = [];

    private static TickHandle? _buttons;

    public static void Attach(MenuBuilder menu)
    {
        Menus.Add(menu.Menu);

        VehicleSpawnKeyBindings.Register(
            () => SharedAPI.RunOnMainThread(ToggleSpawnInside),
            () => SharedAPI.RunOnMainThread(ToggleReplacePrevious));

        // The tick only re-reads its condition when something asks it to, and only these menus ever
        // want it running.
        menu.Menu.OnMenuOpen += _ => _buttons?.Reevaluate();
        menu.Menu.OnMenuClose += _ => _buttons?.Reevaluate();

        _buttons ??= TickRegistry.Register(
            "Vehicles.SpawnShortcutButtons",
            SyncButtons,
            TickRate.Every(ButtonIntervalMs),
            () => Tracked() is not null);
    }

    private static void ToggleSpawnInside()
    {
        if (!Active())
        {
            return;
        }

        VehicleSpawnOptions.SetSpawnInside(!VehicleSpawnOptions.SpawnInside);

        Repaint();
    }

    private static void ToggleReplacePrevious()
    {
        if (!Active() || !VehicleSpawnOptions.CanKeepPrevious)
        {
            return;
        }

        VehicleSpawnOptions.SetReplacePrevious(!VehicleSpawnOptions.ReplacePrevious);

        Repaint();
    }

    private static void Repaint()
    {
        MenuRegistry.RefreshAll();

        SyncButtons();
    }

    // The keys are ordinary FiveM bindings, so they fire wherever the player is. Only a spawner menu
    // has anything to do with them.
    private static bool Active() => Tracked() is not null && Keyboard();

    private static bool Keyboard() => Native.IsUsingKeyboardAndMouse(KeyboardGroup);

    private static Menu? Tracked()
    {
        var menu = MenuController.GetCurrentMenu();

        if (menu is null)
        {
            return null;
        }

        foreach (var known in Menus)
        {
            if (ReferenceEquals(known, menu))
            {
                return menu;
            }
        }

        return null;
    }

    private static void SyncButtons()
    {
        if (Tracked() is not { } menu)
        {
            return;
        }

        menu.CustomInstructionalButtons.Clear();

        if (!Keyboard())
        {
            return;
        }

        var localizer = Localizer.Current;

        menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon(VehicleSpawnKeyBindings.SpawnInsideControl),
            localizer.Get(VehicleSpawnOptions.SpawnInside
                ? Loc.VehicleSpawner.SpawnInsideButtonOn
                : Loc.VehicleSpawner.SpawnInsideButtonOff)));

        if (!VehicleSpawnOptions.CanKeepPrevious)
        {
            return;
        }

        menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon(VehicleSpawnKeyBindings.ReplacePreviousControl),
            localizer.Get(VehicleSpawnOptions.ReplacePrevious
                ? Loc.VehicleSpawner.ReplacePreviousButtonOn
                : Loc.VehicleSpawner.ReplacePreviousButtonOff)));
    }

    private static string Icon(int control) =>
        Native.GetControlInstructionalButton(ControlGroup, control, true);
}
