using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>
/// What the server tells this client to do because somebody else asked for it.
/// </summary>
/// <remarks>
/// The action layer only ever answers whoever asked, so being killed, summoned or messaged arrives on
/// its own events. Registered imperatively, because attribute discovery only scans the assembly named
/// as the <c>client_script</c> and this one is a project reference.
/// </remarks>
public static class PlayerPushEvents
{
    /// <summary>
    /// Nearly twice the usual, because a message from another player is something to read rather than
    /// something to glance at.
    /// </summary>
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
            API.Log.Error($"[OnlinePlayers] Ignoring a summon to coordinates that did not parse: {x}, {y}, {z}");

            return;
        }

        Notifications.Info(MenuText.Key(Loc.OnlinePlayers.SummonedBy, ("player", MenuText.Literal(by))));

        _ = PlayerTeleport.ToCoordsAsync(new Vector3(px, py, pz));
    }

    private static bool TryParse(string value, out float result) =>
        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
}
