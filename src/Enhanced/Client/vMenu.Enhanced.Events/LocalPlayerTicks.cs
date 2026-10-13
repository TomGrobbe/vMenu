using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Events;

public static class LocalPlayerTicks
{
    // A ped only changes on a respawn or a model swap, both of which take about a second.
    private const long IdentityIntervalMs = 250;

    // Fast enough that two shots from an automatic weapon land in separate polls. Anything closer than
    // this arrives as one PlayerPedDamaged carrying the sum.
    private const long HealthIntervalMs = 100;

    // Past this the player did not walk there, so something put them there.
    private const float RespawnDistance = 25f;

    private static TickHandle? _identityTick;
    private static TickHandle? _healthTick;

    private static Action<PlayerPedIdChanged>? _pedChanged;
    private static Func<PlayerPedIdChanged, Task>? _pedChangedAsync;
    private static Action<PlayerPedModelChanged>? _modelChanged;
    private static Func<PlayerPedModelChanged, Task>? _modelChangedAsync;
    private static Action<PlayerPedDamaged>? _damaged;
    private static Func<PlayerPedDamaged, Task>? _damagedAsync;
    private static Action<PlayerPedDied>? _died;
    private static Func<PlayerPedDied, Task>? _diedAsync;
    private static Action<PlayerPedRevived>? _revived;
    private static Func<PlayerPedRevived, Task>? _revivedAsync;

    private static int _ped;
    private static uint _model;

    private static int _healthPed;
    private static int _health;
    private static int _armour;
    private static bool _dead;

    private static int _diedAsPed;
    private static Vector3 _diedAt;

    public static event Action<PlayerPedIdChanged>? PlayerPedIdChanged
    {
        add
        {
            _pedChanged += value;

            // The first subscriber is what starts the loop, and the last one to leave stops it.
            _identityTick?.Reevaluate();
        }
        remove
        {
            _pedChanged -= value;

            _identityTick?.Reevaluate();
        }
    }

    public static event Func<PlayerPedIdChanged, Task>? PlayerPedIdChangedAsync
    {
        add
        {
            _pedChangedAsync += value;

            _identityTick?.Reevaluate();
        }
        remove
        {
            _pedChangedAsync -= value;

            _identityTick?.Reevaluate();
        }
    }

    public static event Action<PlayerPedModelChanged>? PlayerPedModelChanged
    {
        add
        {
            _modelChanged += value;

            _identityTick?.Reevaluate();
        }
        remove
        {
            _modelChanged -= value;

            _identityTick?.Reevaluate();
        }
    }

    public static event Func<PlayerPedModelChanged, Task>? PlayerPedModelChangedAsync
    {
        add
        {
            _modelChangedAsync += value;

            _identityTick?.Reevaluate();
        }
        remove
        {
            _modelChangedAsync -= value;

            _identityTick?.Reevaluate();
        }
    }

    public static event Action<PlayerPedDamaged>? PlayerPedDamaged
    {
        add
        {
            _damaged += value;

            _healthTick?.Reevaluate();
        }
        remove
        {
            _damaged -= value;

            _healthTick?.Reevaluate();
        }
    }

    public static event Func<PlayerPedDamaged, Task>? PlayerPedDamagedAsync
    {
        add
        {
            _damagedAsync += value;

            _healthTick?.Reevaluate();
        }
        remove
        {
            _damagedAsync -= value;

            _healthTick?.Reevaluate();
        }
    }

    public static event Action<PlayerPedDied>? PlayerPedDied
    {
        add
        {
            _died += value;

            _healthTick?.Reevaluate();
        }
        remove
        {
            _died -= value;

            _healthTick?.Reevaluate();
        }
    }

    public static event Func<PlayerPedDied, Task>? PlayerPedDiedAsync
    {
        add
        {
            _diedAsync += value;

            _healthTick?.Reevaluate();
        }
        remove
        {
            _diedAsync -= value;

            _healthTick?.Reevaluate();
        }
    }

    public static event Action<PlayerPedRevived>? PlayerPedRevived
    {
        add
        {
            _revived += value;

            _healthTick?.Reevaluate();
        }
        remove
        {
            _revived -= value;

            _healthTick?.Reevaluate();
        }
    }

    public static event Func<PlayerPedRevived, Task>? PlayerPedRevivedAsync
    {
        add
        {
            _revivedAsync += value;

            _healthTick?.Reevaluate();
        }
        remove
        {
            _revivedAsync -= value;

            _healthTick?.Reevaluate();
        }
    }

    internal static void Initialize()
    {
        _identityTick = TickRegistry.Register(
            "Events.Player.Identity",
            PollIdentityTick,
            TickRate.Every(IdentityIntervalMs),
            IdentityWanted,
            SeedIdentity);

        _healthTick = TickRegistry.Register(
            "Events.Player.Health",
            PollHealthTick,
            TickRate.Every(HealthIntervalMs),
            HealthWanted,
            SeedHealth);
    }

    // The delegate fields rather than a subscriber count: a count drifts the first time somebody removes
    // a handler that was never added, and a delegate cannot.
    private static bool IdentityWanted() =>
        _pedChanged is not null || _pedChangedAsync is not null
        || _modelChanged is not null || _modelChangedAsync is not null;

    private static bool HealthWanted() =>
        _damaged is not null || _damagedAsync is not null
        || _died is not null || _diedAsync is not null
        || _revived is not null || _revivedAsync is not null;

    // The state the first poll compares against, so subscribing is never itself an event. TickHandle
    // runs this before it starts driving the loop, which removes the need for a "have I run yet" flag.
    private static void SeedIdentity()
    {
        var ped = Native.PlayerPedId();

        if (!Exists(ped))
        {
            return;
        }

        _ped = ped;
        _model = Native.GetEntityModel(ped);
    }

    private static void SeedHealth()
    {
        var ped = Native.PlayerPedId();

        if (!Exists(ped))
        {
            return;
        }

        _healthPed = ped;
        _health = Native.GetEntityHealth(ped);
        _armour = Native.GetPedArmour(ped);
        _dead = Native.IsEntityDead(ped, false);
        _diedAsPed = 0;
    }

    private static void PollIdentityTick()
    {
        var ped = Native.PlayerPedId();

        if (!Exists(ped))
        {
            return;
        }

        var model = Native.GetEntityModel(ped);

        if (ped != _ped)
        {
            var previous = _ped;

            // Written before the dispatch, never after: a subscriber that throws, or one that calls back in
            // here, must not be able to make the same change fire twice.
            _ped = ped;

            var change = new PlayerPedIdChanged(ped, previous);

            Dispatch.Raise(_pedChanged, change, nameof(PlayerPedIdChanged));
            Dispatch.RaiseAsync(_pedChangedAsync, change, nameof(PlayerPedIdChanged));
        }

        if (model == _model)
        {
            return;
        }

        var previousModel = _model;

        _model = model;

        var modelChange = new PlayerPedModelChanged(ped, model, previousModel);

        Dispatch.Raise(_modelChanged, modelChange, nameof(PlayerPedModelChanged));
        Dispatch.RaiseAsync(_modelChangedAsync, modelChange, nameof(PlayerPedModelChanged));
    }

    private static void PollHealthTick()
    {
        var ped = Native.PlayerPedId();

        if (!Exists(ped))
        {
            return;
        }

        var health = Native.GetEntityHealth(ped);
        var armour = Native.GetPedArmour(ped);
        var dead = Native.IsEntityDead(ped, false);

        // A new ped is a new set of numbers rather than damage: max health travels with the model, so a swap
        // from a two hundred point ped to a hundred point one would read as a big hit.
        if (ped != _healthPed)
        {
            var wasDead = _dead;

            _healthPed = ped;
            _health = health;
            _armour = armour;
            _dead = dead;

            // Except this, which is the whole reason for noticing a new ped while the old one was down.
            if (wasDead && !dead)
            {
                RaiseRevived(ped, health);
            }

            return;
        }

        if (dead != _dead)
        {
            _dead = dead;
            _health = health;
            _armour = armour;

            if (dead)
            {
                RaiseDied(ped);
            }
            else
            {
                RaiseRevived(ped, health);
            }

            return;
        }

        var healthLost = _health - health;
        var armourLost = _armour - armour;

        _health = health;
        _armour = armour;

        // Downwards only. Health climbs on its own from the game's regen and armour climbs from a pickup,
        // and neither of those is something happening to the player.
        if (dead || (healthLost <= 0 && armourLost <= 0))
        {
            return;
        }

        var damage = new PlayerPedDamaged(ped, health, Math.Max(healthLost, 0), armour, Math.Max(armourLost, 0));

        Dispatch.Raise(_damaged, damage, nameof(PlayerPedDamaged));
        Dispatch.RaiseAsync(_damagedAsync, damage, nameof(PlayerPedDamaged));
    }

    private static void RaiseDied(int ped)
    {
        _diedAsPed = ped;
        _diedAt = Native.GetEntityCoords(ped, true);

        var death = new PlayerPedDied(ped, Native.GetPedSourceOfDeath(ped), Native.GetPedCauseOfDeath(ped));

        Dispatch.Raise(_died, death, nameof(PlayerPedDied));
        Dispatch.RaiseAsync(_diedAsync, death, nameof(PlayerPedDied));
    }

    private static void RaiseRevived(int ped, int health)
    {
        // Neither the game nor the runtime says which of the two happened, so this is inferred. A new ped, or
        // the same one a long way from where it fell, is somebody's spawn point; anything else is the same
        // body picked up where it lay. A watcher that started while the player was down calls it a respawn.
        var respawned = ped != _diedAsPed
            || Vector3.DistanceSquared(Native.GetEntityCoords(ped, true), _diedAt) > RespawnDistance * RespawnDistance;

        _diedAsPed = 0;

        var revive = new PlayerPedRevived(ped, health, respawned);

        Dispatch.Raise(_revived, revive, nameof(PlayerPedRevived));
        Dispatch.RaiseAsync(_revivedAsync, revive, nameof(PlayerPedRevived));
    }

    // Between a model swap and the loading screen letting go, PlayerPedId answers with a handle nothing
    // exists behind. Skipping the poll keeps that from arriving as two changes, out and straight back.
    private static bool Exists(int ped) => ped != 0 && Native.DoesEntityExist(ped);
}
