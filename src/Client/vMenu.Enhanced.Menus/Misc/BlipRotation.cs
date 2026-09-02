using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Misc;

public static class BlipRotation
{
    private static readonly Dictionary<uint, bool> ByModel = [];

    public static bool WantedForModel(uint model)
    {
        if (model == 0)
        {
            return true;
        }

        if (ByModel.TryGetValue(model, out var wanted))
        {
            return wanted;
        }

        wanted = !Native.IsThisModelAHeli(model)
            && !Native.IsThisModelABicycle(model)
            && !Native.IsThisModelABike(model)
            && !Native.IsThisModelAQuadbike(model)
            && !Native.IsThisModelAnAmphibiousQuadbike(model);

        ByModel[model] = wanted;

        return wanted;
    }
}
