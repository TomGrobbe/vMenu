using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

public static class PathRecorder
{
    public const int MaxPoints = 500;

    private static TickHandle? _tick;

    private static readonly List<AutoPilotPathPoint> Points = [];

    private static bool _recording;

    public static event Action? Changed;

    public static bool IsRecording => _recording;

    public static int Count => Points.Count;

    public static bool IsFull => Points.Count >= MaxPoints;

    public static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.AutoPilot);

    public static void Initialize() =>
        _tick = TickRegistry.Register(
            "Vehicle.AutoPilot.Recorder",
            Sample,
            TickRate.Every(250),
            () => _recording && UserDefaults.AutoPilotAutoRecord.Value && IsAllowed);

    public static void Start()
    {
        _recording = true;

        if (Points.Count == 0)
        {
            Drop();
        }

        Settled();
    }

    public static void Stop()
    {
        _recording = false;

        Settled();
    }

    public static float Length()
    {
        var total = 0f;

        for (var i = 1; i < Points.Count; i++)
        {
            total += Vector3.Distance(At(i - 1), At(i));
        }

        return total;
    }

    public static bool Drop()
    {
        if (IsFull || Here() is not { } here)
        {
            return false;
        }

        Points.Add(new AutoPilotPathPoint { X = here.X, Y = here.Y, Z = here.Z });

        Settled();

        return true;
    }

    public static bool Undo()
    {
        if (Points.Count == 0)
        {
            return false;
        }

        Points.RemoveAt(Points.Count - 1);

        Settled();

        return true;
    }

    public static void Discard()
    {
        _recording = false;

        Points.Clear();

        Settled();
    }

    public static SavedAutoPilotPath Build(string name, string description) =>
        new()
        {
            Name = name,
            Description = description,
            Points = new List<AutoPilotPathPoint>(Points),
        };

    private static void Sample()
    {
        if (IsFull)
        {
            Stop();

            return;
        }

        if (Here() is not { } here)
        {
            return;
        }

        var spacing = Math.Max(1, UserDefaults.AutoPilotPathSpacing.Value);

        if (Points.Count > 0 && Vector3.Distance(At(Points.Count - 1), here) < spacing)
        {
            return;
        }

        Points.Add(new AutoPilotPathPoint { X = here.X, Y = here.Y, Z = here.Z });

        Changed?.Invoke();
    }

    private static Vector3? Here()
    {
        var vehicle = OwnVehicle.Driven();

        if (vehicle != 0)
        {
            return Native.GetEntityCoords(vehicle, false);
        }

        return API.Players.Local.Ped is { } ped ? Native.GetEntityCoords(ped.Handle, false) : null;
    }

    private static Vector3 At(int index) => new(Points[index].X, Points[index].Y, Points[index].Z);

    private static void Settled()
    {
        _tick?.Reevaluate();

        Changed?.Invoke();
    }
}
