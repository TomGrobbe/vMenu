using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Menus.Vehicles.Personal;

internal static class RemoteVehicleExecutor
{
    private const float OpenAngle = 0.1f;

    public static void Initialize() =>
        API.OnNetEvent(
            PersonalVehicleEvents.Perform,
            new Action<string, string, string, string[]>(OnPerform),
            false);

    private static async void OnPerform(string requestId, string networkId, string action, string[] args)
    {
        var carriedOut = false;

        try
        {
            carriedOut = await CarryOutAsync(networkId, action, args ?? []);
        }
        catch (Exception exception)
        {
            Log.Error($"[PersonalVehicle] Carrying out '{action}' failed: {exception}");
        }

        await API.Delay(0);

        API.EmitServer(PersonalVehicleEvents.Performed, requestId, carriedOut);
    }

    private static async Task<bool> CarryOutAsync(string networkId, string action, string[] args)
    {
        if (!int.TryParse(networkId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            return false;
        }

        var entity = await NetworkEntity.ResolveAsync(id);

        if (entity == 0 || !await NetworkEntity.TakeControlAsync(entity))
        {
            return false;
        }

        await API.Delay(0);

        return Apply(entity, action, args);
    }

    private static bool Apply(int entity, string action, string[] args)
    {
        switch (action)
        {
            case RemoteVehicleAction.Lock:
                return Lock(entity, Flag(args, 0));

            case RemoteVehicleAction.Engine:
                Native.SetVehicleEngineOn(entity, Flag(args, 0), true, true);

                return true;

            case RemoteVehicleAction.Lights:
                return Lights(entity, args);

            case RemoteVehicleAction.Door:
                return Door(entity, args);

            case RemoteVehicleAction.AllDoors:
                return AllDoors(entity, Mode(args, 0));

            case RemoteVehicleAction.Window:
                return Window(entity, args);

            case RemoteVehicleAction.AllWindows:
                return AllWindows(entity, Mode(args, 0));

            case RemoteVehicleAction.Explode:
                return Explode(entity);

            default:
                Log.Error($"[PersonalVehicle] Asked to carry out '{action}', which is not something this client knows.");

                return false;
        }
    }

    private static bool Lock(int entity, bool locked)
    {
        Native.SetVehicleDoorsLocked(
            entity,
            locked ? RemoteVehicleAction.LockLocked : RemoteVehicleAction.LockUnlocked);

        Native.SetVehicleDoorsLockedForAllPlayers(entity, locked);

        return true;
    }

    private static bool Lights(int entity, string[] args)
    {
        if (!Index(args, 0, out var state))
        {
            return false;
        }

        Native.SetVehicleLights(entity, state);

        if (state != RemoteVehicleAction.LightsAutomatic)
        {
            Native.SetVehicleFullbeam(entity, false);
        }

        return true;
    }

    private static bool Door(int entity, string[] args)
    {
        if (!Index(args, 0, out var door) || !Native.GetIsDoorValid(entity, door))
        {
            return false;
        }

        SetDoor(entity, door, Wanted(Mode(args, 1), IsOpen(entity, door)));

        return true;
    }

    private static bool AllDoors(int entity, string mode)
    {
        var open = string.Equals(mode, RemoteVehicleAction.Open, StringComparison.Ordinal);

        for (var door = 0; door < RemoteVehicleAction.DoorCount; door++)
        {
            if (Native.GetIsDoorValid(entity, door))
            {
                SetDoor(entity, door, open);
            }
        }

        return true;
    }

    private static bool Window(int entity, string[] args)
    {
        if (!Index(args, 0, out var window) || window < 0 || window >= RemoteVehicleAction.WindowCount)
        {
            return false;
        }

        SetWindow(entity, window, string.Equals(Mode(args, 1), RemoteVehicleAction.Up, StringComparison.Ordinal));

        return true;
    }

    private static bool AllWindows(int entity, string mode)
    {
        var up = string.Equals(mode, RemoteVehicleAction.Up, StringComparison.Ordinal);

        for (var window = 0; window < RemoteVehicleAction.WindowCount; window++)
        {
            SetWindow(entity, window, up);
        }

        return true;
    }

    private static bool Explode(int entity)
    {
        Native.NetworkExplodeVehicle(entity, true, false, 0);

        var handle = entity;

        Native.SetEntityAsNoLongerNeeded(ref handle);

        return true;
    }

    private static void SetDoor(int entity, int door, bool open)
    {
        if (open)
        {
            Native.SetVehicleDoorOpen(entity, door, false, false);

            return;
        }

        Native.SetVehicleDoorShut(entity, door, false);
    }

    private static void SetWindow(int entity, int window, bool up)
    {
        if (up)
        {
            Native.RollUpWindow(entity, window);

            return;
        }

        Native.RollDownWindow(entity, window);
    }

    private static bool IsOpen(int entity, int door) => Native.GetVehicleDoorAngleRatio(entity, door) > OpenAngle;

    private static bool Wanted(string mode, bool open) => mode switch
    {
        RemoteVehicleAction.Open => true,
        RemoteVehicleAction.Shut => false,
        _ => !open,
    };

    private static bool Flag(string[] args, int index) =>
        args.Length > index && string.Equals(args[index], RemoteVehicleAction.On, StringComparison.Ordinal);

    private static string Mode(string[] args, int index) => args.Length > index ? args[index] : string.Empty;

    private static bool Index(string[] args, int index, out int value)
    {
        value = 0;

        return args.Length > index
            && int.TryParse(args[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
}
