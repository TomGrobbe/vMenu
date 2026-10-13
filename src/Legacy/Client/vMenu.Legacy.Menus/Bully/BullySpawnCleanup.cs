using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Vehicles;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Bully;

internal static class BullySpawnCleanup
{
    private static readonly HashSet<int> Tracked = [];

    public static void Initialize() => ResourceShutdown.Stopping += DeleteTracked;

    public static async Task DeleteAsync(int entity)
    {
        Tracked.Remove(entity);

        if (entity == 0 || !API.DoesEntityExist(entity))
        {
            return;
        }

        if (API.NetworkGetEntityIsNetworked(entity))
        {
            await NetworkEntity.TakeControlAsync(entity);
        }

        API.SetEntityAsMissionEntity(entity, true, true);
        NativeFixer.DeleteEntity(ref entity);
    }

    public static void TrackForCleanup(int entity, bool reportToServer = true)
    {
        Tracked.Add(entity);

        if (reportToServer && API.NetworkGetEntityIsNetworked(entity))
        {
            BaseScript.TriggerServerEvent(
                BullyEvents.Spawned,
                API.NetworkGetNetworkIdFromEntity(entity).ToString(CultureInfo.InvariantCulture));
        }
    }

    public static void StopTrackingForCleanup(int entity) => Tracked.Remove(entity);

    private static void DeleteTracked()
    {
        foreach (var entity in Tracked.Where(API.DoesEntityExist).ToList())
        {
            var doomed = entity;

            API.SetEntityAsMissionEntity(doomed, true, true);
            NativeFixer.DeleteEntity(ref doomed);
        }

        Tracked.Clear();
    }
}
