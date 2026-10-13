using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using VehicleSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleSpawner;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleSpawnShortcuts
{
    // Every spawner menu declares both keys, and MenuAPI keys a binding on its name, so all of them
    // end up sharing one entry in the player's FiveM key settings rather than one entry per menu.
    public static void Attach(MenuBuilder menu)
    {
        menu.Keys.Add(new MenuKey
        {
            Name = "spawninside",
            Description = MenuText.Key(Loc.VehicleSpawner.SpawnInsideBinding),
            DefaultKey = "DELETE",
            Text = MenuText.From(SpawnInsideLabel),
            Handler = (_, _) => ToggleSpawnInside(),
        });

        menu.Keys.Add(new MenuKey
        {
            Name = "replaceprevious",
            Description = MenuText.Key(Loc.VehicleSpawner.ReplacePreviousBinding),
            DefaultKey = "END",
            Gate = VehicleSpawnerPermissions.AllowKeepPreviousVehicle,
            Text = MenuText.From(ReplacePreviousLabel),
            Handler = (_, _) => ToggleReplacePrevious(),
        });
    }

    private static void ToggleSpawnInside()
    {
        VehicleSpawnOptions.SetSpawnInside(!VehicleSpawnOptions.SpawnInside);

        Repaint();
    }

    private static void ToggleReplacePrevious()
    {
        if (!VehicleSpawnOptions.CanKeepPrevious)
        {
            return;
        }

        VehicleSpawnOptions.SetReplacePrevious(!VehicleSpawnOptions.ReplacePrevious);

        Repaint();
    }

    // The checkbox rows and the button labels both read the setting, so the toggle has to land on all
    // of them and not just the menu the key was pressed in.
    private static void Repaint() => MenuRegistry.RefreshAll();

    private static string SpawnInsideLabel() => Localizer.Current.Get(
        VehicleSpawnOptions.SpawnInside
            ? Loc.VehicleSpawner.SpawnInsideButtonOn
            : Loc.VehicleSpawner.SpawnInsideButtonOff);

    private static string ReplacePreviousLabel() => Localizer.Current.Get(
        VehicleSpawnOptions.ReplacePrevious
            ? Loc.VehicleSpawner.ReplacePreviousButtonOn
            : Loc.VehicleSpawner.ReplacePreviousButtonOff);
}
