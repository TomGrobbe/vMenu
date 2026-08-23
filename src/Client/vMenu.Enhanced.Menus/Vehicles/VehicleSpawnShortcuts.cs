using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleSpawnShortcuts
{
    private const Control SpawnInsideControl = Control.PhoneCameraSelfie;

    private const Control ReplacePreviousControl = Control.PhoneCameraExpression;

    public static void Attach(MenuBuilder menu)
    {
        menu.InstructionalButtons.Add(new ButtonHint
        {
            Control = SpawnInsideControl,
            Text = MenuText.From(() => Localizer.Current.Get(VehicleSpawnOptions.SpawnInside
                ? Loc.VehicleSpawner.SpawnInsideButtonOn
                : Loc.VehicleSpawner.SpawnInsideButtonOff)),
        });

        menu.InstructionalButtons.Add(new ButtonHint
        {
            Control = ReplacePreviousControl,
            Text = MenuText.From(() => Localizer.Current.Get(VehicleSpawnOptions.ReplacePrevious
                ? Loc.VehicleSpawner.ReplacePreviousButtonOn
                : Loc.VehicleSpawner.ReplacePreviousButtonOff)),
            Gate = MenuGate.When(() => VehicleSpawnOptions.CanKeepPrevious),
        });

        menu.Menu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(
            SpawnInsideControl,
            Menu.ControlPressCheckType.JUST_PRESSED,
            (_, _) => ToggleSpawnInside(),
            true));

        menu.Menu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(
            ReplacePreviousControl,
            Menu.ControlPressCheckType.JUST_PRESSED,
            (_, _) => ToggleReplacePrevious(),
            true));
    }

    private static void ToggleSpawnInside()
    {
        VehicleSpawnOptions.SetSpawnInside(!VehicleSpawnOptions.SpawnInside);

        MenuRegistry.RefreshAll();
    }

    private static void ToggleReplacePrevious()
    {
        if (!VehicleSpawnOptions.CanKeepPrevious)
        {
            return;
        }

        VehicleSpawnOptions.SetReplacePrevious(!VehicleSpawnOptions.ReplacePrevious);

        MenuRegistry.RefreshAll();
    }
}
