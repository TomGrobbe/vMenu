using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

// Stored preferences rather than facts about a vehicle, so like GodModeSection nothing here refills
// when the player changes what they are driving.
internal static class EngineSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.EngineAlwaysOn),
            Description = MenuText.Key(Loc.VehicleOptions.EngineAlwaysOnDescription),
            Gate = VehicleOptionsPermissions.EngineAlwaysOn,
            ReadState = () => VehicleEngine.AlwaysOn,
            OnChanged = changed => VehicleEngine.SetAlwaysOn(changed.Checked),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.ToggleEngine),
            Description = MenuText.Key(Loc.VehicleOptions.ToggleEngineDescription),
            Gate = VehicleOptionsPermissions.ToggleEngine,
            OnSelected = _ => VehicleEngine.Toggle(),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.PowerMultiplierEnabled),
            Description = MenuText.Key(Loc.VehicleOptions.PowerMultiplierEnabledDescription),
            Gate = VehicleOptionsPermissions.PowerMultiplier,
            ReadState = () => VehiclePerformance.PowerEnabled,
            OnChanged = changed =>
            {
                VehiclePerformance.SetPowerEnabled(changed.Checked);

                MenuRegistry.Refresh(changed.Menu);
            },
        });

        menu.Entries.Add(MultiplierRow(
            Loc.VehicleOptions.PowerMultiplier,
            Loc.VehicleOptions.PowerMultiplierDescription,
            Loc.VehicleOptions.PowerMultiplierLocked,
            VehicleOptionsPermissions.PowerMultiplier,
            () => VehiclePerformance.PowerEnabled,
            () => VehiclePerformance.PowerMultiplier,
            VehiclePerformance.SetPowerMultiplier));

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.TorqueMultiplierEnabled),
            Description = MenuText.Key(Loc.VehicleOptions.TorqueMultiplierEnabledDescription),
            Gate = VehicleOptionsPermissions.TorqueMultiplier,
            ReadState = () => VehiclePerformance.TorqueEnabled,
            OnChanged = changed =>
            {
                VehiclePerformance.SetTorqueEnabled(changed.Checked);

                MenuRegistry.Refresh(changed.Menu);
            },
        });

        menu.Entries.Add(MultiplierRow(
            Loc.VehicleOptions.TorqueMultiplier,
            Loc.VehicleOptions.TorqueMultiplierDescription,
            Loc.VehicleOptions.TorqueMultiplierLocked,
            VehicleOptionsPermissions.TorqueMultiplier,
            () => VehiclePerformance.TorqueEnabled,
            () => VehiclePerformance.TorqueMultiplier,
            VehiclePerformance.SetTorqueMultiplier));
    }

    private static ListEntry MultiplierRow(
        string textKey,
        string descriptionKey,
        string lockedKey,
        string permission,
        Func<bool> enabled,
        Func<int> read,
        Action<int> write)
    {
        var options = new List<MenuText>(VehiclePerformance.Steps.Length);

        foreach (var step in VehiclePerformance.Steps)
        {
            options.Add(MenuText.Key(Loc.VehicleOptions.MultiplierOption, ("amount", MenuText.Literal(step.ToString()))));
        }

        return new ListEntry
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(descriptionKey),
            LockedDescription = MenuText.Key(lockedKey),
            Gate = MenuGate.Permission(permission) & MenuGate.When(enabled),
            Options = options,
            ReadSelectedIndex = () => Math.Max(Array.IndexOf(VehiclePerformance.Steps, read()), 0),
            OnIndexChanged = changed => write(VehiclePerformance.Steps[changed.NewIndex]),
        };
    }
}
