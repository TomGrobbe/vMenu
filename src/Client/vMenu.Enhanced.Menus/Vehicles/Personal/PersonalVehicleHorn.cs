using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;

namespace vMenu.Enhanced.Menus.Vehicles.Personal;

internal static class PersonalVehicleHorn
{
    private static readonly HashSet<int> Sounding = [];

    public static void Initialize() =>
        API.OnNetEvent(PersonalVehicleEvents.HornTune, new Action<int, int>(OnHornTune), false);

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
                if (!Native.DoesEntityExist(entity))
                {
                    return;
                }

                Native.SetHornPermanentlyOnTime(entity, onMs);

                await API.Delay(onMs + gapMs);
            }
        }
        finally
        {
            Sounding.Remove(networkId);
        }
    }
}
