using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleAlarm
{
    private const int MinDurationMs = 8000;

    private const int MaxDurationMs = 45000;

    private static readonly Random Duration = new();

    public static void Toggle()
    {
        if (OwnVehicle.RequireDriven(Loc.VehicleOptions.AlarmNoVehicle, Loc.VehicleOptions.AlarmNotDriver) is null)
        {
            return;
        }

        var handle = Native.GetVehiclePedIsIn(Native.PlayerPedId(), false);

        if (handle == 0)
        {
            return;
        }

        if (Native.IsVehicleAlarmActivated(handle))
        {
            Native.SetVehicleAlarmTimeLeft(handle, 0);
            Native.SetVehicleAlarm(handle, false);

            Notifications.Info(MenuText.Key(Loc.VehicleOptions.AlarmStopped));

            return;
        }

        Native.SetVehicleAlarm(handle, true);
        Native.StartVehicleAlarm(handle);
        Native.SetVehicleAlarmTimeLeft(handle, Duration.Next(MinDurationMs, MaxDurationMs));

        Notifications.Info(MenuText.Key(Loc.VehicleOptions.AlarmStarted));
    }
}
