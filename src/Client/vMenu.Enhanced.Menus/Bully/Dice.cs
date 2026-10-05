using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Dice
{
    public static int Next(int maxExclusive) => Native.GetRandomIntInRange(0, maxExclusive);

    public static int Next(int min, int maxExclusive) => Native.GetRandomIntInRange(min, maxExclusive);

    public static float Float(float min, float max) => Native.GetRandomFloatInRange(min, max);
}
