using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Vehicles;

namespace vMenu.Enhanced.Menus.Bully;

internal static class BullySpawnCleanup
{
    private static readonly HashSet<int> Tracked = [];

    public static void Initialize() => ResourceShutdown.Stopping += DeleteTracked;

    public static async Task DeleteAsync(int entity)
    {
        Tracked.Remove(entity);

        if (entity == 0 || !Native.DoesEntityExist(entity))
        {
            return;
        }

        if (Native.NetworkGetEntityIsNetworked(entity))
        {
            await NetworkEntity.TakeControlAsync(entity);
        }

        Native.SetEntityAsMissionEntity(entity, true, true);
        Native.DeleteEntity(ref entity);
    }

    public static void TrackForCleanup(int entity, bool reportToServer = true)
    {
        Tracked.Add(entity);

        if (reportToServer && Native.NetworkGetEntityIsNetworked(entity))
        {
            API.EmitServer(
                BullyEvents.Spawned,
                Native.NetworkGetNetworkIdFromEntity(entity).ToString(CultureInfo.InvariantCulture));
        }
    }

    public static void StopTrackingForCleanup(int entity) => Tracked.Remove(entity);

    private static void DeleteTracked()
    {
        foreach (var entity in Tracked.Where(Native.DoesEntityExist).ToList())
        {
            var doomed = entity;

            Native.SetEntityAsMissionEntity(doomed, true, true);
            Native.DeleteEntity(ref doomed);
        }

        Tracked.Clear();
    }
}
