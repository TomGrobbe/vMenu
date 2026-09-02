using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles;

namespace vMenu.Enhanced.Menus.Players;

// The action layer only ever answers whoever asked, so being killed, summoned or messaged arrives on
// its own events. Registered imperatively, because attribute discovery only scans the assembly named
// as the client_script and this one is a project reference.
public static class PlayerPushEvents
{
    private const string On = "1";

    // Nearly twice the usual, because a message from another player is something to read rather than
    // something to glance at.
    public const int MessageDurationMs = 15000;

    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(PlayerEvents.Kill, new Action<string>(OnKilled), false);
        API.OnNetEvent(PlayerEvents.Message, new Action<string, string, string>(OnMessage), false);
        API.OnNetEvent(PlayerEvents.Teleport, new Action<string, string, string, string>(OnSummoned), false);

        API.OnNetEvent(
            PlayerEvents.TeleportIntoVehicle,
            new Action<string, string, string, string, string, string>(OnSummonedIntoVehicle),
            false);
        API.OnNetEvent(PlayerEvents.SetWantedLevel, new Action<string, string>(OnWantedLevelRequested), false);
        API.OnNetEvent(PlayerEvents.GetGodMode, new Action<string>(OnGodModeRequested), false);
        API.OnNetEvent(PlayerEvents.SetNoClip, new Action<string>(OnNoClipSet), false);
        API.OnNetEvent(PlayerEvents.SetNoClipAccess, new Action<string>(OnNoClipAccessSet), false);
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
        API.EmitServer(PlayerEvents.GodModeAck, requestId, PlayerGodMode.Enabled, VehicleGodMode.Enabled);

    private static async void OnWantedLevelRequested(string requestId, string stars)
    {
        if (!int.TryParse(stars, NumberStyles.Integer, CultureInfo.InvariantCulture, out var wanted))
        {
            Log.Error($"[OnlinePlayers] Ignoring a wanted level request that did not parse: {stars}");

            return;
        }

        PlayerActions.SetWantedLevel(wanted);

        await API.Delay(0);

        API.EmitServer(
            PlayerEvents.WantedLevelAck,
            requestId,
            PlayerActions.WantedLevel().ToString(CultureInfo.InvariantCulture));
    }

    private static void OnKilled(string by)
    {
        if (API.Players.Local.Ped is not { } ped)
        {
            return;
        }

        Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.Killed, ("player", MenuText.Literal(by))));

        Native.SetEntityHealth(ped.Handle, 0, 0, 0);
    }

    private static void OnMessage(string messageId, string from, string message)
    {
        Notifications.Info(
            MenuText.Key(
                Loc.OnlinePlayers.MessageReceived,
                ("player", MenuText.Literal(from)),
                ("message", MenuText.Literal(message))),
            MessageDurationMs);

        // After it is on screen, not before: the sender is waiting on this to hear that their message
        // arrived, so it has to mean the message was actually shown.
        API.EmitServer(PlayerEvents.MessageAck, messageId);
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
