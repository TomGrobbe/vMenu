using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Ticks;

using BullySettings = vMenu.Enhanced.Data.Configuration.Settings.Bully;

namespace vMenu.Enhanced.Menus.Bully;

internal static class ToggleEffects
{
    private const int ControlGroup = 0;

    private const int VehicleAccelerate = 71;

    private const int VehicleBrake = 72;

    private const int ReducedGripLevel = 1;

    private const int ShockCooldownMs = 3000;

    private const float NormalLod = 1f;

    private const float BrokenLod = -1f;

    private static readonly uint HandContactWithDoor = Streaming.Hash("HandContactWithDoor");

    private static readonly HashSet<int> LodApplied = [];

    private static readonly List<TickHandle> Ticks = [];

    private static readonly VehicleFlag Inverted = new(Native.SetInvertVehicleControls);

    private static readonly VehicleFlag Slippery = new(SetSlippery);

    private static bool _riot;

    private static int _nextBurstAt;

    private static int _burstEndsAt;

    private static bool _burstAccelerates;

    private static bool _entering;

    private static int _shockCooldownUntil;

    private static bool Enabled => ClientConfig.Value(BullySettings.Enabled);

    private static bool LodActive => Enabled && Glitched().Count > 0;

    public static void Initialize()
    {
        Ticks.Add(TickRegistry.Register(
            "Bully.InvertControls",
            Inverted.Follow,
            TickRate.Every(250),
            () => BullyState.IsOn(BullyToggles.InvertControls),
            onStopped: Inverted.Release));

        Ticks.Add(TickRegistry.Register(
            "Bully.RandomThrottle",
            Throttle,
            TickRate.Varying(() => Native.GetGameTimer() < _burstEndsAt ? TickRate.PerFrame : TickRate.Every(250)),
            () => BullyState.IsOn(BullyToggles.RandomThrottle),
            onStarted: ScheduleBurst));

        Ticks.Add(TickRegistry.Register(
            "Bully.LowGrip",
            Slippery.Follow,
            TickRate.Every(500),
            () => BullyState.IsOn(BullyToggles.LowGrip),
            onStopped: Slippery.Release));

        Ticks.Add(TickRegistry.Register(
            "Bully.ElectrocuteDoor",
            WatchDoor,
            TickRate.Varying(() => _entering ? TickRate.PerFrame : TickRate.Every(250)),
            () => Enabled));

        Ticks.Add(TickRegistry.Register("Bully.Lod", ScanLod, TickRate.Every(500), () => LodActive || LodApplied.Count > 0));

        BullyState.Changed += OnChanged;
        ResourceShutdown.Stopping += OnShutdown;

        ClientConfig.Track([BullyEvents.LodConvar]);
        ClientConfig.AddEventListenerFor([BullyEvents.LodConvar], Reevaluate);
        ClientConfig.AddEventListenerFor([BullySettings.Enabled], Reevaluate);
    }

    private static void OnChanged()
    {
        var riot = BullyState.IsOn(BullyToggles.Riot);

        if (riot != _riot)
        {
            _riot = riot;

            Native.SetRiotModeEnabled(riot);
        }

        Reevaluate();
    }

    private static void Reevaluate() => Ticks.ForEach(static tick => tick.Reevaluate());

    private static void OnShutdown()
    {
        if (_riot)
        {
            Native.SetRiotModeEnabled(false);
        }

        Slippery.Release();
        Inverted.Release();
    }

    private static void SetSlippery(int vehicle, bool on)
    {
        Native.SetVehicleReduceGrip(vehicle, on);

        if (on)
        {
            Native.SetVehicleReduceGripLevel(vehicle, ReducedGripLevel);
        }
    }

    private static void ScheduleBurst()
    {
        _nextBurstAt = Native.GetGameTimer() + Dice.Next(5000, 15000);
        _burstEndsAt = 0;
    }

    private static void Throttle()
    {
        var now = Native.GetGameTimer();
        var vehicle = VehiclePranks.DrivenVehicle();

        if (now < _burstEndsAt)
        {
            if (vehicle != 0)
            {
                Surge();
            }

            return;
        }

        if (_burstEndsAt != 0)
        {
            ScheduleBurst();

            return;
        }

        if (now < _nextBurstAt || vehicle == 0)
        {
            return;
        }

        _burstAccelerates = Dice.Next(2) == 0;
        _burstEndsAt = now + Dice.Next(1000, 2000);
    }

    // Pressed for the driver every frame of the burst, so the car's own engine and brakes do the work.
    private static void Surge() =>
        Native.SetControlNormal(ControlGroup, _burstAccelerates ? VehicleAccelerate : VehicleBrake, 1f);

    private static void WatchDoor()
    {
        var ped = Native.PlayerPedId();
        var vehicle = Native.GetVehiclePedIsTryingToEnter(ped);

        _entering = vehicle != 0;

        if (!_entering
            || Native.GetGameTimer() < _shockCooldownUntil
            || !Native.HasAnimEventFired(ped, HandContactWithDoor)
            || !Electrifies(vehicle))
        {
            return;
        }

        _shockCooldownUntil = Native.GetGameTimer() + ShockCooldownMs;

        PedPranks.ShockFromDoor(ped);
    }

    private static bool Electrifies(int vehicle) =>
        BullyState.IsOn(BullyToggles.ShockOnEntry)
        || (Electrified.OwnerOf(vehicle) is var owner and not 0 && owner != Native.GetPlayerServerId(Native.PlayerId()));

    private static HashSet<int> Glitched()
    {
        var glitched = new HashSet<int>();

        foreach (var part in (ClientConfig.GetString(BullyEvents.LodConvar) ?? string.Empty).Split(BullyEvents.ListSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId))
            {
                glitched.Add(serverId);
            }
        }

        return glitched;
    }

    private static void ScanLod()
    {
        PlayerRoster.Refresh();

        var active = LodActive;
        var ids = active ? Glitched() : new HashSet<int>();
        var glitched = PlayerRoster.All.Where(player => ids.Contains(player.ServerId)).ToList();

        glitched.ForEach(static player => Native.SetPedLodMultiplier(player.Ped, BrokenLod));

        foreach (var serverId in LodApplied.Except(glitched.Select(player => player.ServerId)))
        {
            if (PlayerRoster.TryGet(serverId, out var player))
            {
                Native.SetPedLodMultiplier(player.Ped, NormalLod);
            }
        }

        LodApplied.Clear();
        LodApplied.UnionWith(glitched.Select(player => player.ServerId));

        if (!active)
        {
            Reevaluate();
        }
    }

    private sealed class VehicleFlag
    {
        private readonly Action<int, bool> _apply;

        private int _vehicle;

        public VehicleFlag(Action<int, bool> apply) => _apply = apply;

        public void Follow()
        {
            var vehicle = VehiclePranks.DrivenVehicle();

            if (vehicle == _vehicle)
            {
                return;
            }

            Release();

            if (vehicle != 0)
            {
                _vehicle = vehicle;

                _apply(vehicle, true);
            }
        }

        public void Release()
        {
            if (_vehicle != 0 && Native.DoesEntityExist(_vehicle))
            {
                _apply(_vehicle, false);
            }

            _vehicle = 0;
        }
    }
}
