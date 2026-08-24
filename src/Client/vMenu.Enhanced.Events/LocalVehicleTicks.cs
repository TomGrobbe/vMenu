using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Events;

public static class LocalVehicleTicks
{
    // The seat the game calls the driver's. Passengers are 0 and up.
    private const int DriverSeat = -1;

    // Below the driver's index, so it can never collide with a real seat.
    private const int NoSeat = -2;

    // Getting out of a car takes about a second and a half of animation, so a quarter second after that
    // lands well under what a player notices as the menu catching up.
    private const long SeatIntervalMs = 250;

    // Damage is the one thing here that arrives in bursts.
    private const long DamageIntervalMs = 100;

    // Dirt builds up over seconds of driving, so it is the one thing here that can be watched slowly.
    private const long DirtIntervalMs = 500;

    // Below this a change is noise rather than damage: ownership changing hands re-syncs the three
    // numbers with a wobble of a fraction of a point.
    private const float DamageThreshold = 1f;

    // Below this a change is float noise rather than dirt, on a scale that runs 0 to 15.
    private const float DirtThreshold = 0.01f;

    private static TickHandle? _seatTick;
    private static TickHandle? _damageTick;
    private static TickHandle? _dirtTick;

    private static Action<VehicleEntered>? _entered;
    private static Func<VehicleEntered, Task>? _enteredAsync;
    private static Action<VehicleExited>? _exited;
    private static Func<VehicleExited, Task>? _exitedAsync;
    private static Action<VehicleSwapped>? _swapped;
    private static Func<VehicleSwapped, Task>? _swappedAsync;
    private static Action<VehicleSeatChanged>? _seatChanged;
    private static Func<VehicleSeatChanged, Task>? _seatChangedAsync;
    private static Action<VehicleChanged>? _changed;
    private static Func<VehicleChanged, Task>? _changedAsync;
    private static Action<VehicleDamaged>? _damaged;
    private static Func<VehicleDamaged, Task>? _damagedAsync;
    private static Action<VehicleDirtied>? _dirtied;
    private static Func<VehicleDirtied, Task>? _dirtiedAsync;

    private static int _vehicle;
    private static int _seat = NoSeat;

    private static int _damagedVehicle;
    private static float _body;
    private static float _engine;
    private static float _tank;

    private static int _dirtyVehicle;
    private static float _dirt;

    public static event Action<VehicleEntered>? VehicleEntered
    {
        add
        {
            _entered += value;

            // The first subscriber is what starts the loop, and the last one to leave stops it.
            _seatTick?.Reevaluate();
        }
        remove
        {
            _entered -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Func<VehicleEntered, Task>? VehicleEnteredAsync
    {
        add
        {
            _enteredAsync += value;

            _seatTick?.Reevaluate();
        }
        remove
        {
            _enteredAsync -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Action<VehicleExited>? VehicleExited
    {
        add
        {
            _exited += value;

            _seatTick?.Reevaluate();
        }
        remove
        {
            _exited -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Func<VehicleExited, Task>? VehicleExitedAsync
    {
        add
        {
            _exitedAsync += value;

            _seatTick?.Reevaluate();
        }
        remove
        {
            _exitedAsync -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Action<VehicleSwapped>? VehicleSwapped
    {
        add
        {
            _swapped += value;

            _seatTick?.Reevaluate();
        }
        remove
        {
            _swapped -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Func<VehicleSwapped, Task>? VehicleSwappedAsync
    {
        add
        {
            _swappedAsync += value;

            _seatTick?.Reevaluate();
        }
        remove
        {
            _swappedAsync -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Action<VehicleSeatChanged>? VehicleSeatChanged
    {
        add
        {
            _seatChanged += value;

            _seatTick?.Reevaluate();
        }
        remove
        {
            _seatChanged -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Func<VehicleSeatChanged, Task>? VehicleSeatChangedAsync
    {
        add
        {
            _seatChangedAsync += value;

            _seatTick?.Reevaluate();
        }
        remove
        {
            _seatChangedAsync -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Action<VehicleChanged>? VehicleChanged
    {
        add
        {
            _changed += value;

            _seatTick?.Reevaluate();
        }
        remove
        {
            _changed -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Func<VehicleChanged, Task>? VehicleChangedAsync
    {
        add
        {
            _changedAsync += value;

            _seatTick?.Reevaluate();
        }
        remove
        {
            _changedAsync -= value;

            _seatTick?.Reevaluate();
        }
    }

    public static event Action<VehicleDamaged>? VehicleDamaged
    {
        add
        {
            _damaged += value;

            _damageTick?.Reevaluate();
        }
        remove
        {
            _damaged -= value;

            _damageTick?.Reevaluate();
        }
    }

    public static event Func<VehicleDamaged, Task>? VehicleDamagedAsync
    {
        add
        {
            _damagedAsync += value;

            _damageTick?.Reevaluate();
        }
        remove
        {
            _damagedAsync -= value;

            _damageTick?.Reevaluate();
        }
    }

    public static event Action<VehicleDirtied>? VehicleDirtied
    {
        add
        {
            _dirtied += value;

            _dirtTick?.Reevaluate();
        }
        remove
        {
            _dirtied -= value;

            _dirtTick?.Reevaluate();
        }
    }

    public static event Func<VehicleDirtied, Task>? VehicleDirtiedAsync
    {
        add
        {
            _dirtiedAsync += value;

            _dirtTick?.Reevaluate();
        }
        remove
        {
            _dirtiedAsync -= value;

            _dirtTick?.Reevaluate();
        }
    }

    internal static void Initialize()
    {
        _seatTick = TickRegistry.Register(
            "Events.Vehicle.Seat",
            PollSeat,
            TickRate.Every(SeatIntervalMs),
            SeatWanted,
            SeedSeat);

        _damageTick = TickRegistry.Register(
            "Events.Vehicle.Damage",
            PollDamage,
            TickRate.Every(DamageIntervalMs),
            DamageWanted,
            SeedDamage);

        _dirtTick = TickRegistry.Register(
            "Events.Vehicle.Dirt",
            PollDirt,
            TickRate.Every(DirtIntervalMs),
            DirtWanted,
            SeedDirt);
    }

    private static bool SeatWanted() =>
        _entered is not null || _enteredAsync is not null
        || _exited is not null || _exitedAsync is not null
        || _swapped is not null || _swappedAsync is not null
        || _seatChanged is not null || _seatChangedAsync is not null
        || _changed is not null || _changedAsync is not null;

    private static bool DamageWanted() => _damaged is not null || _damagedAsync is not null;

    private static bool DirtWanted() => _dirtied is not null || _dirtiedAsync is not null;

    // The state the first poll compares against, so subscribing while already in a car is silent.
    private static void SeedSeat() => Read(out _vehicle, out _seat);

    private static void SeedDamage()
    {
        Read(out _damagedVehicle, out _);

        ReadCondition(_damagedVehicle, out _body, out _engine, out _tank);
    }

    private static void SeedDirt()
    {
        Read(out _dirtyVehicle, out _);

        _dirt = ReadDirt(_dirtyVehicle);
    }

    private static void PollSeat()
    {
        Read(out var vehicle, out var seat);

        if (vehicle == _vehicle && seat == _seat)
        {
            return;
        }

        var previous = _vehicle;
        var previousSeat = _seat;

        // Written before the dispatch, never after: a subscriber that throws, or one that calls back in
        // here, must not be able to make the same change fire twice.
        _vehicle = vehicle;
        _seat = seat;

        if (previous == vehicle)
        {
            var moved = new VehicleSeatChanged(vehicle, seat, previousSeat);

            Dispatch.Raise(_seatChanged, moved, nameof(VehicleSeatChanged));
            Dispatch.RaiseAsync(_seatChangedAsync, moved, nameof(VehicleSeatChanged));
        }
        else if (previous == 0)
        {
            var entered = new VehicleEntered(vehicle, seat);

            Dispatch.Raise(_entered, entered, nameof(VehicleEntered));
            Dispatch.RaiseAsync(_enteredAsync, entered, nameof(VehicleEntered));
        }
        else if (vehicle == 0)
        {
            var exited = new VehicleExited(previous, previousSeat);

            Dispatch.Raise(_exited, exited, nameof(VehicleExited));
            Dispatch.RaiseAsync(_exitedAsync, exited, nameof(VehicleExited));
        }
        else
        {
            var swapped = new VehicleSwapped(vehicle, previous, seat);

            Dispatch.Raise(_swapped, swapped, nameof(VehicleSwapped));
            Dispatch.RaiseAsync(_swappedAsync, swapped, nameof(VehicleSwapped));
        }

        var changed = new VehicleChanged(
            Absent(vehicle),
            Absent(previous),
            Absent(seat, NoSeat),
            Absent(previousSeat, NoSeat));

        Dispatch.Raise(_changed, changed, nameof(VehicleChanged));
        Dispatch.RaiseAsync(_changedAsync, changed, nameof(VehicleChanged));
    }

    private static void PollDamage()
    {
        Read(out var vehicle, out _);

        ReadCondition(vehicle, out var body, out var engine, out var tank);

        // A different vehicle is a different set of numbers, so it is seeded rather than compared.
        if (vehicle != _damagedVehicle)
        {
            _damagedVehicle = vehicle;
            _body = body;
            _engine = engine;
            _tank = tank;

            return;
        }

        var bodyLost = _body - body;
        var engineLost = _engine - engine;
        var tankLost = _tank - tank;

        _body = body;
        _engine = engine;
        _tank = tank;

        // Repairing moves the same three numbers upwards, which is not damage.
        if (vehicle == 0
            || (bodyLost < DamageThreshold && engineLost < DamageThreshold && tankLost < DamageThreshold))
        {
            return;
        }

        var damage = new VehicleDamaged(
            vehicle,
            body,
            engine,
            tank,
            Math.Max(bodyLost, 0f),
            Math.Max(engineLost, 0f),
            Math.Max(tankLost, 0f));

        Dispatch.Raise(_damaged, damage, nameof(VehicleDamaged));
        Dispatch.RaiseAsync(_damagedAsync, damage, nameof(VehicleDamaged));
    }

    private static void PollDirt()
    {
        Read(out var vehicle, out _);

        var dirt = ReadDirt(vehicle);

        // A different vehicle is a different number, so it is seeded rather than compared. Getting into an
        // already dirty car is not getting dirty.
        if (vehicle != _dirtyVehicle)
        {
            _dirtyVehicle = vehicle;
            _dirt = dirt;

            return;
        }

        var gained = dirt - _dirt;

        _dirt = dirt;

        // Washing moves the same number downwards, which is not getting dirty.
        if (vehicle == 0 || gained < DirtThreshold)
        {
            return;
        }

        var dirtied = new VehicleDirtied(vehicle, dirt, gained);

        Dispatch.Raise(_dirtied, dirtied, nameof(VehicleDirtied));
        Dispatch.RaiseAsync(_dirtiedAsync, dirtied, nameof(VehicleDirtied));
    }

    private static int? Absent(int value, int none = 0) => value == none ? null : value;

    private static void Read(out int vehicle, out int seat)
    {
        vehicle = 0;
        seat = NoSeat;

        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        // The false is "not while climbing in". GetVehiclePedIsIn on its own answers with the vehicle a ped
        // is still walking towards, and a section that refilled then would refill again on arrival.
        if (!Native.IsPedInAnyVehicle(ped, false))
        {
            return;
        }

        var handle = Native.GetVehiclePedIsIn(ped, false);

        // A vehicle deleted or streamed out from under the player leaves a handle nothing answers to, and a
        // menu needs to hear about that as leaving it.
        if (handle == 0 || !Native.DoesEntityExist(handle))
        {
            return;
        }

        vehicle = handle;
        seat = FindSeat(handle, ped);
    }

    private static int FindSeat(int vehicle, int ped)
    {
        // The same question VehicleTargeting asks, so the watcher and the menus cannot end up disagreeing
        // about who is driving.
        if (Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) == ped)
        {
            return DriverSeat;
        }

        // Counts the driver's seat as one of them, so the last passenger index is two below the total.
        var seats = Native.GetVehicleModelNumberOfSeats((uint)Native.GetEntityModel(vehicle));

        for (var seat = 0; seat <= seats - 2; seat++)
        {
            if (Native.GetPedInVehicleSeat(vehicle, seat, false) == ped)
            {
                return seat;
            }
        }

        return NoSeat;
    }

    // Three separate numbers with three different ranges: body and petrol tank run 0 to 1000, and engine
    // goes down to about minus four thousand once it is dead or on fire.
    private static void ReadCondition(int vehicle, out float body, out float engine, out float tank)
    {
        if (vehicle == 0)
        {
            body = 0f;
            engine = 0f;
            tank = 0f;

            return;
        }

        body = Native.GetVehicleBodyHealth(vehicle);
        engine = Native.GetVehicleEngineHealth(vehicle);
        tank = Native.GetVehiclePetrolTankHealth(vehicle);
    }

    private static float ReadDirt(int vehicle) => vehicle == 0 ? 0f : Native.GetVehicleDirtLevel(vehicle);
}
