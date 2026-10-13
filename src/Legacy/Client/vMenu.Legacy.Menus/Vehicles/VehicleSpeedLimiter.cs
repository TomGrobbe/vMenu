using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleSpeedLimiter
{
    public const int Set = 0;

    public const int Reset = 1;

    public const int Custom = 2;

    private const float MetersPerSecondToKmh = 3.6f;

    private const float MetersPerSecondToMph = 2.23694f;

    private const float NoLimit = 500.01f;

    private const int InputMaxLength = 6;

    private const float StandstillMetersPerSecond = 0.5f;

    public static void Apply(int action)
    {
        var vehicle = OwnVehicle.RequireDriven(
            Loc.VehicleOptions.SpeedLimiterNoVehicle,
            Loc.VehicleOptions.SpeedLimiterNotDriver);

        if (vehicle is null)
        {
            return;
        }

        switch (action)
        {
            case Set:
                var speed = API.GetEntitySpeed(vehicle.Handle);

                if (speed < StandstillMetersPerSecond)
                {
                    Notifications.Error(MenuText.Key(Loc.VehicleOptions.SpeedLimiterStandstill));

                    return;
                }

                Limit(vehicle.Handle, speed);

                break;

            case Reset:
                API.SetEntityMaxSpeed(vehicle.Handle, NoLimit);

                Notifications.Success(MenuText.Key(Loc.VehicleOptions.SpeedLimiterCleared));

                break;

            default:
                _ = AskAsync(vehicle.Handle);

                break;
        }
    }

    private static async Task AskAsync(int vehicle)
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.VehicleOptions.SpeedLimiterPrompt),
            InputMaxLength);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        if (!float.TryParse(typed, NumberStyles.Float, CultureInfo.InvariantCulture, out var typedSpeed) || typedSpeed <= 0f)
        {
            Notifications.Error(MenuText.Key(Loc.VehicleOptions.SpeedLimiterBadNumber));

            return;
        }

        if (!API.DoesEntityExist(vehicle))
        {
            return;
        }

        var metric = API.ShouldUseMetricMeasurements();

        Limit(vehicle, typedSpeed / (metric ? MetersPerSecondToKmh : MetersPerSecondToMph));
    }

    private static void Limit(int vehicle, float metersPerSecond)
    {
        API.SetEntityMaxSpeed(vehicle, NoLimit);
        API.SetEntityMaxSpeed(vehicle, metersPerSecond);

        var metric = API.ShouldUseMetricMeasurements();
        var shown = MathF.Round(metersPerSecond * (metric ? MetersPerSecondToKmh : MetersPerSecondToMph), 1);

        Notifications.Success(MenuText.Key(
            metric ? Loc.VehicleOptions.SpeedLimiterSetKmh : Loc.VehicleOptions.SpeedLimiterSetMph,
            ("speed", MenuText.Literal(shown.ToString("0.#", CultureInfo.InvariantCulture)))));
    }
}
