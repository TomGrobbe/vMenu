using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

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

        var handle = API.GetVehiclePedIsIn(API.PlayerPedId(), false);

        if (handle == 0)
        {
            return;
        }

        if (API.IsVehicleAlarmActivated(handle))
        {
            API.SetVehicleAlarmTimeLeft(handle, 0);
            API.SetVehicleAlarm(handle, false);

            Notifications.Info(MenuText.Key(Loc.VehicleOptions.AlarmStopped));

            return;
        }

        API.SetVehicleAlarm(handle, true);
        NativeFixer.StartVehicleAlarm(handle);
        API.SetVehicleAlarmTimeLeft(handle, Duration.Next(MinDurationMs, MaxDurationMs));

        Notifications.Info(MenuText.Key(Loc.VehicleOptions.AlarmStarted));
    }
}
