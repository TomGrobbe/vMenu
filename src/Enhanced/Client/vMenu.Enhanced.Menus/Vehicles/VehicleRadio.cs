using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleRadio
{
    private const char Separator = ',';

    private const string OffStation = RadioStations.Off;

    private static readonly List<string> Stations = [];

    private static readonly HashSet<string> Locked = new(StringComparer.Ordinal);

    private static bool _watching;

    public static bool DefaultEnabled => UserDefaults.VehicleDefaultRadioEnabled.Value && IsAllowed;

    public static string DefaultStation => UserDefaults.VehicleDefaultRadioStation.Value;

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.Radio);

    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static IReadOnlyList<string> All()
    {
        if (Stations.Count > 0)
        {
            return Stations;
        }

        var count = Native.MaxRadioStationIndex();

        for (var index = 0; index <= count; index++)
        {
            var name = Native.GetRadioStationName(index);

            if (string.IsNullOrWhiteSpace(name)
                || string.Equals(name, OffStation, StringComparison.OrdinalIgnoreCase)
                || Stations.Contains(name))
            {
                continue;
            }

            Stations.Add(name);
        }

        return Stations;
    }

    public static IReadOnlyList<string> Selectable()
    {
        var stations = new List<string> { OffStation };

        stations.AddRange(All());

        return stations;
    }

    public static bool IsBlocked(string station) => Blocked().Contains(station);

    public static void SetDefaultEnabled(bool enabled)
    {
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.VehicleDefaultRadioEnabled.Value = enabled;

        Apply();
    }

    public static void SetDefaultStation(string station)
    {
        UserDefaults.VehicleDefaultRadioStation.Value = station;

        Apply();
    }

    public static void SetBlocked(string station, bool blocked)
    {
        if (!IsAllowed)
        {
            return;
        }

        var stations = Blocked();

        if (blocked ? !stations.Add(station) : !stations.Remove(station))
        {
            return;
        }

        UserDefaults.VehicleBlockedRadioStations.Value = string.Join(Separator, stations);

        ApplyLocks();
    }

    public static void ClearBlocked()
    {
        UserDefaults.VehicleBlockedRadioStations.Value = string.Empty;

        ApplyLocks();
    }

    private static HashSet<string> Blocked()
    {
        var stored = UserDefaults.VehicleBlockedRadioStations.Value;
        var stations = new HashSet<string>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(stored))
        {
            return stations;
        }

        foreach (var station in stored.Split(Separator, StringSplitOptions.RemoveEmptyEntries))
        {
            stations.Add(station.Trim());
        }

        return stations;
    }

    private static void Apply()
    {
        Watch(true);

        ApplyLocks();

        Tune(OwnVehicle.Driven());
    }

    private static void ApplyLocks()
    {
        var wanted = IsAllowed ? Blocked() : [];

        foreach (var station in Locked)
        {
            if (!wanted.Contains(station))
            {
                Write(station, locked: false);
            }
        }

        Locked.Clear();

        foreach (var station in wanted)
        {
            Write(station, locked: true);

            Locked.Add(station);
        }
    }

    private static void Write(string station, bool locked)
    {
        Native.LockRadioStation(station, locked);
        Native.SetRadioStationIsVisible(station, !locked);
    }

    private static void Tune(int vehicle)
    {
        if (!DefaultEnabled || vehicle == 0)
        {
            return;
        }

        _ = TuneAsync(vehicle);
    }

    private static async Task TuneAsync(int vehicle)
    {
        await API.Delay(1);

        if (!Native.DoesEntityExist(vehicle) || !Native.DoesPlayerVehHaveRadio())
        {
            return;
        }

        while (Native.IsRadioRetuning())
        {
            await API.Delay(10);
        }

        if (!Native.DoesEntityExist(vehicle))
        {
            return;
        }

        Native.SetVehRadioStation(vehicle, DefaultStation);
    }

    private static void Watch(bool watching)
    {
        if (watching == _watching)
        {
            return;
        }

        _watching = watching;

        if (watching)
        {
            LocalVehicleTicks.VehicleChanged += OnChanged;

            return;
        }

        LocalVehicleTicks.VehicleChanged -= OnChanged;
    }

    private static void OnChanged(VehicleChanged changed)
    {
        ApplyLocks();

        if (changed.Vehicle is { } vehicle)
        {
            Tune(vehicle);
        }
    }
}
