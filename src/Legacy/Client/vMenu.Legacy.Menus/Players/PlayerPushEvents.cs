using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Vehicles;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Players;

// Registered imperatively: attribute discovery only scans the client_script assembly, and this is a
// project reference.
public static class PlayerPushEvents
{
    private const string On = "1";

    private const int DriverSeat = -1;

    // Longer than usual, because a player message is meant to be read, not glanced at.
    public const int MessageDurationMs = 15000;

    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        Script.EventHandlers[PlayerEvents.Kill] += new Action<string>(OnKilled);
        Script.EventHandlers[PlayerEvents.Message] += new Action<string, string, string>(OnMessage);
        Script.EventHandlers[PlayerEvents.Teleport] += new Action<string, string, string, string>(OnSummoned);

        Script.EventHandlers[PlayerEvents.TeleportIntoVehicle] += new Action<string, string, string, string, string, string>(OnSummonedIntoVehicle);
        Script.EventHandlers[PlayerEvents.SetWantedLevel] += new Action<string, string>(OnWantedLevelRequested);
        Script.EventHandlers[PlayerEvents.GetGodMode] += new Action<string>(OnGodModeRequested);
        Script.EventHandlers[PlayerEvents.SetNoClip] += new Action<string>(OnNoClipSet);
        Script.EventHandlers[PlayerEvents.SetNoClipAccess] += new Action<string>(OnNoClipAccessSet);
        Script.EventHandlers[PlayerEvents.SetWaypoint] += new Action<string, string>(OnWaypointSet);
        Script.EventHandlers[PlayerEvents.TeleportToGround] += new Action<string, string>(OnTeleportedToGround);
        Script.EventHandlers[PlayerEvents.TeleportToCoords] += new Action<string, string, string, string>(OnTeleportedToCoords);
        Script.EventHandlers[PlayerEvents.GetStatusReport] += new Action<string>(PlayerStatusReport.OnRequested);
        Script.EventHandlers[PlayerEvents.Restore] += new Action<string>(OnRestore);
        Script.EventHandlers[PlayerEvents.SpawnVehicle] += new Action<string>(OnSpawnVehicle);
        Script.EventHandlers[PlayerEvents.Notify] += new Action<string, string, string>(OnNotify);
    }

    private static void OnWaypointSet(string x, string y)
    {
        if (!TryParse(x, out var px) || !TryParse(y, out var py))
        {
            Log.Error($"[OnlinePlayers] Ignoring a waypoint that did not parse: {x}, {y}");

            return;
        }

        API.SetNewWaypoint(px, py);

        Notifications.Info(MenuText.Key(Loc.OnlinePlayers.WaypointByStaff));
    }

    private static async void OnTeleportedToGround(string x, string y)
    {
        if (!TryParse(x, out var px) || !TryParse(y, out var py))
        {
            Log.Error($"[OnlinePlayers] Ignoring a teleport that did not parse: {x}, {y}");

            return;
        }

        Notifications.Info(MenuText.Key(Loc.OnlinePlayers.TeleportedByStaff));

        await PlayerTeleport.ToGroundAsync(px, py);
    }

    // An empty heading keeps the player facing the way they already were.
    private static async void OnTeleportedToCoords(string x, string y, string z, string heading)
    {
        if (!TryParse(x, out var px) || !TryParse(y, out var py) || !TryParse(z, out var pz))
        {
            Log.Error($"[OnlinePlayers] Ignoring a teleport that did not parse: {x}, {y}, {z}");

            return;
        }

        float? facing = TryParse(heading, out var h) ? h : null;

        Notifications.Info(MenuText.Key(Loc.OnlinePlayers.TeleportedByStaff));

        await PlayerTeleport.ToCoordsAsync(new Vector3(px, py, pz), facing);
    }

    private static void OnRestore(string mode)
    {
        if (string.Equals(mode, "armor", StringComparison.OrdinalIgnoreCase))
        {
            PlayerActions.SetArmorTier(PlayerActions.ArmorTiers);

            Notifications.Info(MenuText.Key(Loc.OnlinePlayers.ArmorByStaff));

            return;
        }

        var ped = API.PlayerPedId();
        NativeFixer.SetEntityHealth(ped, API.GetEntityMaxHealth(ped), 0, 0);

        Notifications.Info(MenuText.Key(Loc.OnlinePlayers.HealedByStaff));
    }

    private static async void OnSpawnVehicle(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return;
        }

        var vehicle = await VehicleSpawning.SpawnAsync(model.Trim());

        if (vehicle is null)
        {
            Log.Warning($"[OnlinePlayers] A staff vehicle spawn for '{model}' did not produce a vehicle.");

            return;
        }

        if (Game.PlayerPed is { } ped)
        {
            API.SetPedIntoVehicle(ped.Handle, vehicle.Handle, DriverSeat);
        }

        Notifications.Info(MenuText.Key(Loc.OnlinePlayers.VehicleSpawnedByStaff));
    }

    private static void OnNotify(string style, string text, string footer)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var notificationStyle = style?.ToLowerInvariant() switch
        {
            "success" => NotificationStyle.Success,
            "warning" => NotificationStyle.Warning,
            "error" => NotificationStyle.Error,
            _ => NotificationStyle.Info,
        };

        var source = string.IsNullOrWhiteSpace(footer) ? null : footer;

        Notifications.Show(notificationStyle, MenuText.Literal(text), MessageDurationMs, source);
    }

    private static void OnNoClipSet(string state)
    {
        var active = state == On;

        NoClip.NoClip.SetActiveByStaff(active);

        Notifications.Info(MenuText.Key(active
            ? Loc.OnlinePlayers.NoClipOnByStaff
            : Loc.OnlinePlayers.NoClipOffByStaff));
    }

    private static void OnNoClipAccessSet(string state)
    {
        var lent = state == On;

        NoClip.NoClip.SetLentByStaff(lent);

        Notifications.Info(MenuText.Key(lent
            ? Loc.OnlinePlayers.NoClipAccessLentByStaff
            : Loc.OnlinePlayers.NoClipAccessTakenByStaff));
    }

    private static void OnGodModeRequested(string requestId) =>
        BaseScript.TriggerServerEvent(PlayerEvents.GodModeAck, requestId, PlayerGodMode.Toggle.Enabled, VehicleGodMode.Enabled);

    private static async void OnWantedLevelRequested(string requestId, string stars)
    {
        if (!int.TryParse(stars, NumberStyles.Integer, CultureInfo.InvariantCulture, out var wanted))
        {
            Log.Error($"[OnlinePlayers] Ignoring a wanted level request that did not parse: {stars}");

            return;
        }

        PlayerActions.SetWantedLevel(wanted);

        await BaseScript.Delay(0);

        BaseScript.TriggerServerEvent(
            PlayerEvents.WantedLevelAck,
            requestId,
            PlayerActions.WantedLevel().ToString(CultureInfo.InvariantCulture));
    }

    private static void OnKilled(string by)
    {
        if (Game.PlayerPed is not { } ped)
        {
            return;
        }

        Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.Killed, ("player", MenuText.Literal(by))));

        NativeFixer.SetEntityHealth(ped.Handle, 0, 0, 0);
    }

    private static void OnMessage(string messageId, string from, string message)
    {
        Notifications.Info(
            MenuText.Key(
                Loc.OnlinePlayers.MessageReceived,
                ("player", MenuText.Literal(from)),
                ("message", MenuText.Literal(message))),
            MessageDurationMs);

        // Acked only after it is on screen, since the sender waits on this to know it was shown.
        BaseScript.TriggerServerEvent(PlayerEvents.MessageAck, messageId);
    }

    private static void OnSummoned(string by, string x, string y, string z)
    {
        if (!TryParse(x, out var px) || !TryParse(y, out var py) || !TryParse(z, out var pz))
        {
            Log.Error($"[OnlinePlayers] Ignoring a summon to coordinates that did not parse: {x}, {y}, {z}");

            return;
        }

        Notifications.Info(MenuText.Key(Loc.OnlinePlayers.SummonedBy, ("player", MenuText.Literal(by))));

        _ = PlayerTeleport.ToCoordsAsync(new Vector3(px, py, pz));
    }

    private static void OnSummonedIntoVehicle(string by, string networkId, string seat, string x, string y, string z)
    {
        if (!int.TryParse(networkId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var vehicle)
            || !int.TryParse(seat, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            Log.Error($"[OnlinePlayers] Ignoring a summon into a vehicle that did not parse: {networkId}, {seat}");

            return;
        }

        if (!TryParse(x, out var px) || !TryParse(y, out var py) || !TryParse(z, out var pz))
        {
            Log.Error($"[OnlinePlayers] Ignoring a summon to coordinates that did not parse: {x}, {y}, {z}");

            return;
        }

        Notifications.Info(MenuText.Key(Loc.OnlinePlayers.SummonedIntoVehicleBy, ("player", MenuText.Literal(by))));

        _ = PlayerTeleport.IntoVehicleAsync(vehicle, new Vector3(px, py, pz), index);
    }

    private static bool TryParse(string value, out float result) =>
        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
}
