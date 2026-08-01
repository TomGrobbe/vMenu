using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;

namespace vMenu.Enhanced.Menus.Data;

/// <summary>
/// Turns a vehicle's raw handling figures into the 0..1 values the stats panel draws.
/// </summary>
/// <remarks>
/// The panel is relative, not absolute: each figure is scaled against the best in that vehicle's own
/// class, which is why every table below is indexed by class id and why that order is not cosmetic.
/// </remarks>
internal static class VehicleClassStats
{
    private static readonly float[] MaxSpeed =
    [
        44.9374657f,
        50.0000038f,
        48.862133f,
        48.1321335f,
        50.7077942f,
        51.3333359f,
        52.3922348f,
        53.86687f,
        52.03867f,
        49.2241631f,
        39.6176529f,
        37.5559425f,
        42.72843f,
        21.0f,
        45.0f,
        65.1952744f,
        109.764259f,
        42.72843f,
        56.5962219f,
        57.5398865f,
        43.3140678f,
        26.66667f,
        53.0537224f,
    ];

    private static readonly float[] MaxAcceleration =
    [
        0.34f,
        0.29f,
        0.335f,
        0.28f,
        0.395f,
        0.39f,
        0.66f,
        0.42f,
        0.425f,
        0.475f,
        0.21f,
        0.3f,
        0.32f,
        0.17f,
        18.0f,
        5.88f,
        21.0700016f,
        0.33f,
        14.0f,
        6.86f,
        0.32f,
        0.2f,
        0.76f,
    ];

    private static readonly float[] MaxBraking =
    [
        0.72f,
        0.95f,
        0.85f,
        0.9f,
        1.0f,
        1.0f,
        1.3f,
        1.25f,
        1.52f,
        1.1f,
        0.6f,
        0.7f,
        0.8f,
        3.0f,
        0.4f,
        3.5920403f,
        20.58f,
        0.9f,
        2.93960738f,
        3.9472363f,
        0.85f,
        5.0f,
        1.3f,
    ];

    private static readonly float[] MaxTraction =
    [
        2.3f,
        2.55f,
        2.3f,
        2.6f,
        2.625f,
        2.65f,
        2.8f,
        2.782f,
        2.9f,
        2.95f,
        2.0f,
        3.3f,
        2.175f,
        2.05f,
        0.0f,
        1.6f,
        2.15f,
        2.55f,
        2.57f,
        3.7f,
        2.05f,
        2.5f,
        3.2925f,
    ];

    /// <summary>Falls back to a blank panel for a class id the tables do not cover.</summary>
    internal static VehicleStats Normalise(uint modelHash, int vehicleClass)
    {
        if ((uint)vehicleClass >= (uint)MaxSpeed.Length)
        {
            return VehicleStats.None;
        }

        return new VehicleStats(
            Map(Native.GetVehicleModelEstimatedMaxSpeed(modelHash), 0f, MaxSpeed[vehicleClass], 0f, 1f),
            Map(Native.GetVehicleModelAcceleration(modelHash), 0f, MaxAcceleration[vehicleClass], 0f, 1f),
            Map(Native.GetVehicleModelMaxBraking(modelHash), 0f, MaxBraking[vehicleClass], 0f, 1f),
            Map(Native.GetVehicleModelMaxTraction(modelHash), 0f, MaxTraction[vehicleClass], 0f, 1f));
    }

    /// <summary>
    /// Maps <paramref name="value"/>, a value between <paramref name="minIn"/> and
    /// <paramref name="maxIn"/>, onto the range <paramref name="minOut"/>..<paramref name="maxOut"/>.
    /// </summary>
    internal static float Map(float value, float minIn, float maxIn, float minOut, float maxOut) =>
        ((value - minIn) * (maxOut - minOut) / (maxIn - minIn)) + minOut;
}
