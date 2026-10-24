using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class HandlingSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ListEntry(
            Loc.VehicleOptions.SpeedLimiter,
            [
                MenuText.Key(Loc.VehicleOptions.SpeedLimiterSet),
                MenuText.Key(Loc.VehicleOptions.SpeedLimiterReset),
                MenuText.Key(Loc.VehicleOptions.SpeedLimiterCustom),
            ])
        {
            Gate = VehicleOptionsPermissions.SpeedLimiter,
            OnSelected = selected => VehicleSpeedLimiter.Apply(selected.SelectedIndex),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.VehicleOptions.Freeze)
        {
            Gate = VehicleOptionsPermissions.Freeze,
            ReadState = () => VehicleFreeze.Enabled,
            OnChanged = changed => VehicleFreeze.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new ButtonEntry(Loc.VehicleOptions.Flip)
        {
            Gate = VehicleOptionsPermissions.Flip,
            OnSelected = _ => VehicleFlip.FlipCurrent(),
        });

        menu.Entries.Add(new ButtonEntry(Loc.VehicleOptions.Visibility)
        {
            Gate = VehicleOptionsPermissions.Invisible,
            OnSelected = _ => VehicleVisibility.Toggle(),
        });

        menu.Entries.Add(new ButtonEntry(Loc.VehicleOptions.Alarm)
        {
            Gate = VehicleOptionsPermissions.Alarm,
            OnSelected = _ => VehicleAlarm.Toggle(),
        });
    }
}
