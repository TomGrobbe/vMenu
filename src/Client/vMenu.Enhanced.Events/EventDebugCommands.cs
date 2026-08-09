using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;

namespace vMenu.Enhanced.Events;

/// <summary>
/// A throwaway handler per event that prints the event and everything it carried, so a developer can
/// watch what actually fires before writing anything that depends on it.
/// </summary>
/// <remarks>
/// Switching one on subscribes for real, so the watcher's tick starts up the same way it would for
/// any other subscriber. Switching every one back off lets those ticks stop again.
/// </remarks>
// Every payload is a record struct, so its own ToString names the type and lists every member. That
// is why one shared logger covers all of them and nothing here needs touching when a payload changes.
public static class EventDebugCommands
{
    private const string Command = "vmenu_events";

    private static readonly Hook[] Hooks =
    [
        Hook.For<PlayerPedIdChanged>(
            "Player.PedId",
            handler => LocalPlayerTicks.PlayerPedIdChanged += handler,
            handler => LocalPlayerTicks.PlayerPedIdChanged -= handler),
        Hook.For<PlayerPedModelChanged>(
            "Player.Model",
            handler => LocalPlayerTicks.PlayerPedModelChanged += handler,
            handler => LocalPlayerTicks.PlayerPedModelChanged -= handler),
        Hook.For<PlayerPedDamaged>(
            "Player.Damaged",
            handler => LocalPlayerTicks.PlayerPedDamaged += handler,
            handler => LocalPlayerTicks.PlayerPedDamaged -= handler),
        Hook.For<PlayerPedDied>(
            "Player.Died",
            handler => LocalPlayerTicks.PlayerPedDied += handler,
            handler => LocalPlayerTicks.PlayerPedDied -= handler),
        Hook.For<PlayerPedRevived>(
            "Player.Revived",
            handler => LocalPlayerTicks.PlayerPedRevived += handler,
            handler => LocalPlayerTicks.PlayerPedRevived -= handler),
        Hook.For<VehicleEntered>(
            "Vehicle.Entered",
            handler => LocalVehicleTicks.VehicleEntered += handler,
            handler => LocalVehicleTicks.VehicleEntered -= handler),
        Hook.For<VehicleExited>(
            "Vehicle.Exited",
            handler => LocalVehicleTicks.VehicleExited += handler,
            handler => LocalVehicleTicks.VehicleExited -= handler),
        Hook.For<VehicleSwapped>(
            "Vehicle.Swapped",
            handler => LocalVehicleTicks.VehicleSwapped += handler,
            handler => LocalVehicleTicks.VehicleSwapped -= handler),
        Hook.For<VehicleSeatChanged>(
            "Vehicle.SeatChanged",
            handler => LocalVehicleTicks.VehicleSeatChanged += handler,
            handler => LocalVehicleTicks.VehicleSeatChanged -= handler),
        Hook.For<VehicleChanged>(
            "Vehicle.Changed",
            handler => LocalVehicleTicks.VehicleChanged += handler,
            handler => LocalVehicleTicks.VehicleChanged -= handler),
        Hook.For<VehicleDamaged>(
            "Vehicle.Damaged",
            handler => LocalVehicleTicks.VehicleDamaged += handler,
            handler => LocalVehicleTicks.VehicleDamaged -= handler),
    ];

    internal static void Initialize() =>
        SharedAPI.Commands.RegisterCommand(Command, false, DebugCommands.Gate<string?>(Toggle));

    private static void Toggle(string? argument)
    {
        var target = argument?.Trim() ?? string.Empty;

        if (target.Length == 0)
        {
            Report();

            return;
        }

        // Prefix rather than exact, so "vehicle" switches the whole group and "vehicle.seat" one of it.
        var matches = target.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? Hooks
            : Array.FindAll(Hooks, hook => hook.Name.StartsWith(target, StringComparison.OrdinalIgnoreCase));

        if (matches.Length == 0)
        {
            API.Log.Info($"[Events] Nothing here is called '{target}'.");

            Report();

            return;
        }

        // On unless every match is already on, so toggling a group can never leave half of it running.
        var on = Array.Exists(matches, hook => !hook.Attached);

        foreach (var hook in matches)
        {
            hook.Set(on);
        }

        API.Log.Info($"[Events] Logging {(on ? "on" : "off")} for {matches.Length} event(s).");
    }

    private static void Report()
    {
        API.Log.Info($"[Events] Usage: {Command} <all | name | name prefix>, which switches logging on or off.");

        foreach (var hook in Hooks)
        {
            API.Log.Info($"[Events]   [{(hook.Attached ? "on" : "  ")}] {hook.Name}");
        }
    }

    private sealed class Hook
    {
        public required string Name { get; init; }

        public bool Attached { get; private set; }

        private Action Attach { get; init; } = static () => { };

        private Action Detach { get; init; } = static () => { };

        public static Hook For<TPayload>(string name, Action<Action<TPayload>> add, Action<Action<TPayload>> remove)
            where TPayload : struct
        {
            // Kept in a variable so detaching hands back the very delegate that was attached.
            Action<TPayload> log = payload => API.Log.Info($"[Events] {payload}");

            return new Hook
            {
                Name = name,
                Attach = () => add(log),
                Detach = () => remove(log),
            };
        }

        public void Set(bool attached)
        {
            if (Attached == attached)
            {
                return;
            }

            Attached = attached;

            if (attached)
            {
                Attach();

                return;
            }

            Detach();
        }
    }
}
