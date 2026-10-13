using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

namespace vMenu.Enhanced.Menus.Misc;

public static class LocationBlips
{
    private const float BaseScale = 0.8f;

    private const float MinScale = 0.2f;

    private const float MaxScale = 2f;

    private static readonly List<int> AlwaysOnHandles = [];

    private static readonly List<int> ToggleableHandles = [];

    public static bool ToggleableShown =>
        UserDefaults.DisplayLocationBlips.Value
        && ClientPermissions.IsAllowed(DisplaySettingsPermissions.LocationBlips);

    public static void Initialize()
    {
        LocationBlipSync.Changed += Apply;
        ClientPermissions.PermissionsChanged += Apply;
        ResourceShutdown.Stopping += RemoveAll;
    }

    public static void SetToggleableShown(bool shown)
    {
        UserDefaults.DisplayLocationBlips.Value = shown;

        Apply();
    }

    public static void Apply()
    {
        Rebuild(AlwaysOnHandles, LocationBlipSync.File.AlwaysOn, wanted: true);
        Rebuild(ToggleableHandles, LocationBlipSync.File.Toggleable, wanted: ToggleableShown);
    }

    public static LocationBlip? Nearest(Vector3 position, out bool alwaysOn)
    {
        var fromAlwaysOn = Nearest(LocationBlipSync.File.AlwaysOn, position, out var alwaysOnDistance);
        var fromToggleable = Nearest(LocationBlipSync.File.Toggleable, position, out var toggleableDistance);

        alwaysOn = fromAlwaysOn is not null && (fromToggleable is null || alwaysOnDistance <= toggleableDistance);

        return alwaysOn ? fromAlwaysOn : fromToggleable ?? fromAlwaysOn;
    }

    private static LocationBlip? Nearest(List<LocationBlip> list, Vector3 position, out float distance)
    {
        LocationBlip? closest = null;

        distance = float.MaxValue;

        foreach (var blip in list)
        {
            var apart = Vector3.DistanceSquared(position, new Vector3(blip.X, blip.Y, blip.Z));

            if (apart >= distance)
            {
                continue;
            }

            distance = apart;
            closest = blip;
        }

        return closest;
    }

    private static void Rebuild(List<int> handles, List<LocationBlip> wantedBlips, bool wanted)
    {
        foreach (var handle in handles)
        {
            Destroy(handle);
        }

        handles.Clear();

        if (!wanted)
        {
            return;
        }

        foreach (var blip in wantedBlips)
        {
            handles.Add(Create(blip));
        }
    }

    private static int Create(LocationBlip blip)
    {
        var handle = Native.AddBlipForCoord(blip.X, blip.Y, blip.Z);

        Native.SetBlipSprite(handle, blip.Sprite);
        Native.SetBlipColour(handle, blip.Colour);
        Native.SetBlipScale(handle, Math.Clamp(BaseScale + blip.ScaleOffset, MinScale, MaxScale));
        Native.SetBlipAsShortRange(handle, blip.ShortRange);

        Native.BeginTextCommandSetBlipName("STRING");
        Native.AddTextComponentSubstringPlayerName(blip.Name);
        Native.EndTextCommandSetBlipName(handle);

        return handle;
    }

    private static void Destroy(int handle)
    {
        if (!Native.DoesBlipExist(handle))
        {
            return;
        }

        // Ref<T> cannot cross an await, and the out overload pushes a literal 0.
        var doomed = handle;

        Native.RemoveBlip(ref doomed);
    }

    private static void RemoveAll()
    {
        Rebuild(AlwaysOnHandles, [], wanted: false);
        Rebuild(ToggleableHandles, [], wanted: false);
    }
}
