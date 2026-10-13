using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Dice
{
    public static int Next(int maxExclusive) => API.GetRandomIntInRange(0, maxExclusive);

    public static int Next(int min, int maxExclusive) => API.GetRandomIntInRange(min, maxExclusive);

    public static float Float(float min, float max) => API.GetRandomFloatInRange(min, max);
}
