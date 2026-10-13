using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class HandlingSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.SpeedLimiter),
            Description = MenuText.Key(Loc.VehicleOptions.SpeedLimiterDescription),
            Gate = VehicleOptionsPermissions.SpeedLimiter,
            Options =
            [
                MenuText.Key(Loc.VehicleOptions.SpeedLimiterSet),
                MenuText.Key(Loc.VehicleOptions.SpeedLimiterReset),
                MenuText.Key(Loc.VehicleOptions.SpeedLimiterCustom),
            ],
            OnSelected = selected => VehicleSpeedLimiter.Apply(selected.SelectedIndex),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.Freeze),
            Description = MenuText.Key(Loc.VehicleOptions.FreezeDescription),
            Gate = VehicleOptionsPermissions.Freeze,
            ReadState = () => VehicleFreeze.Enabled,
            OnChanged = changed => VehicleFreeze.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.Flip),
            Description = MenuText.Key(Loc.VehicleOptions.FlipDescription),
            Gate = VehicleOptionsPermissions.Flip,
            OnSelected = _ => VehicleFlip.FlipCurrent(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.Visibility),
            Description = MenuText.Key(Loc.VehicleOptions.VisibilityDescription),
            Gate = VehicleOptionsPermissions.Invisible,
            OnSelected = _ => VehicleVisibility.Toggle(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.Alarm),
            Description = MenuText.Key(Loc.VehicleOptions.AlarmDescription),
            Gate = VehicleOptionsPermissions.Alarm,
            OnSelected = _ => VehicleAlarm.Toggle(),
        });
    }
}
