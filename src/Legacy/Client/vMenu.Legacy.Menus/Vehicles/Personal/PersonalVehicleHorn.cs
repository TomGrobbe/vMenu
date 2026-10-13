using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles.Personal;

internal static class PersonalVehicleHorn
{
    private static readonly HashSet<int> Sounding = [];

    public static void Initialize() =>
        Script.EventHandlers[PersonalVehicleEvents.HornTune] += new Action<int, int>(OnHornTune);

    private static async void OnHornTune(int networkId, int tune)
    {
        if (HornTunes.Notes(tune) is not { } notes || !Sounding.Add(networkId))
        {
            return;
        }

        try
        {
            var entity = NetworkEntity.Find(networkId);

            if (entity == 0)
            {
                return;
            }

            foreach (var (onMs, gapMs) in notes)
            {
                if (!API.DoesEntityExist(entity))
                {
                    return;
                }

                NativeFixer.SetHornPermanentlyOnTime(entity, onMs);

                await BaseScript.Delay(onMs + gapMs);
            }
        }
        finally
        {
            Sounding.Remove(networkId);
        }
    }
}
