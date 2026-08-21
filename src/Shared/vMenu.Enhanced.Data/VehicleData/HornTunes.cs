namespace vMenu.Enhanced.Data.VehicleData;

public static class HornTunes
{
    private static readonly (int OnMs, int GapMs)[] Shave =
    [
        (150, 60),
        (75, 45),
        (75, 45),
        (150, 60),
        (160, 320),
        (150, 60),
        (220, 0),
    ];

    private static readonly (int OnMs, int GapMs)[] Charge =
    [
        (90, 55),
        (90, 55),
        (90, 55),
        (90, 55),
        (430, 0),
    ];

    private static readonly (int OnMs, int GapMs)[] Laugh =
    [
        (70, 70),
        (70, 70),
        (70, 70),
        (70, 160),
        (70, 70),
        (70, 70),
        (260, 0),
    ];

    private static readonly (int OnMs, int GapMs)[][] Tunes = [Shave, Charge, Laugh];

    public static int Count => Tunes.Length;

    public static IReadOnlyList<(int OnMs, int GapMs)>? Notes(int index) =>
        index < 0 || index >= Tunes.Length ? null : Tunes[index];
}
