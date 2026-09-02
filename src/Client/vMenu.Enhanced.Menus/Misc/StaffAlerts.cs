using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.StaffAlerts;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;

using StaffAlertSettings = vMenu.Enhanced.Data.Configuration.Settings.StaffAlerts;

namespace vMenu.Enhanced.Menus.Misc;

public static class StaffAlerts
{
    private const string RespondCommandName = "vmenu-respond";

    private const string DismissCommandName = "dismiss";

    private const int DescriptionLimit = 100;

    private const int ForgetGraceSeconds = 30;

    private const int NoClipSettleMs = 1000;

    private static readonly HashSet<int> Received = [];

    private static readonly HashSet<int> OnScreen = [];

    private static readonly StaffCommand RespondCommand = new(RespondCommandName, Respond);

    private static readonly StaffCommand DismissCommand = new(DismissCommandName, _ => Dismiss());

    private static bool _registered;

    private static bool _busy;

    private static int _nextAllowedAt;

    private static bool Enabled => ClientConfig.Value(StaffAlertSettings.Enabled);

    public static bool Hidden => UserDefaults.MiscHideStaffAlerts.Value;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(StaffAlertEvents.Show, new Action<string, string, string, string>(OnShow), false);
        API.OnNetEvent(StaffAlertEvents.Resolved, new Action<string, string, string>(OnResolved), false);
        API.OnNetEvent(StaffAlertEvents.Expired, new Action<string, string>(OnExpired), false);
        API.OnNetEvent(StaffAlertEvents.Dismissed, new Action<string, string, string>(OnDismissed), false);
        API.OnNetEvent(StaffAlertEvents.DismissedNotice, new Action<string>(OnDismissedNotice), false);

        ClientConfig.AddEventListenerFor([StaffAlertSettings.Enabled], Apply);
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetHidden(bool hidden)
    {
        UserDefaults.MiscHideStaffAlerts.Value = hidden;

        Report();

        if (!hidden)
        {
            return;
        }

        Clear();

        Received.Clear();
    }

    public static async Task RaiseAsync()
    {
        if (_busy)
        {
            return;
        }

        var waiting = Remaining();

        if (waiting > 0)
        {
            Notifications.Warning(MenuText.Key(
                Loc.MiscSettings.AlertStaffCooldown,
                ("seconds", MenuText.Literal(waiting.ToString(CultureInfo.InvariantCulture)))));

            return;
        }

        var typed = await UserInput.GetTextAsync(MenuText.Key(Loc.MiscSettings.AlertStaffPrompt), DescriptionLimit);

        if (typed is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(typed))
        {
            Notifications.Warning(MenuText.Key(Loc.MiscSettings.AlertStaffEmpty));

            return;
        }

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(ActionIds.StaffAlerts.Raise, typed.Trim());

            if (result.Status == ActionStatus.Refused && result.Data.Length > 0)
            {
                StartCooldown(result.Data[0]);

                Notifications.Warning(MenuText.Key(
                    Loc.MiscSettings.AlertStaffCooldown,
                    ("seconds", MenuText.Literal(result.Data[0]))));

                return;
            }

            if (result.Status != ActionStatus.Ok || result.Data.Length < 1)
            {
                Notifications.Error(MenuText.Key(Loc.OnlinePlayers.Failed));

                return;
            }

            StartCooldown(ClientConfig.Value(StaffAlertSettings.CooldownSeconds).ToString(CultureInfo.InvariantCulture));

            if (result.Data[0] == "0")
            {
                Notifications.Warning(MenuText.Key(Loc.MiscSettings.AlertStaffNobody));

                return;
            }

            Notifications.Success(MenuText.Key(
                Loc.MiscSettings.AlertStaffSent,
                ("count", MenuText.Literal(result.Data[0]))));
        }
        finally
        {
            _busy = false;
        }
    }

    private static void OnShow(string alertId, string from, string description, string durationMs)
    {
        if (!int.TryParse(alertId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            || !int.TryParse(durationMs, NumberStyles.Integer, CultureInfo.InvariantCulture, out var duration))
        {
            Log.Error($"[StaffAlerts] Ignoring an alert that did not parse: id {alertId}, duration {durationMs}");

            return;
        }

        if (Hidden)
        {
            return;
        }

        Banner(id, from, description, duration);

        _ = ForgetAsync(id);
    }

    public static void ShowAgain(int id, string player, string description) =>
        Banner(id, player, description, DisplayMs());

    private static void Banner(int id, string player, string description, int durationMs)
    {
        var text = MenuText.Key(
            Loc.MiscSettings.AlertStaffBanner,
            ("player", MenuText.Literal(player)),
            ("description", MenuText.Literal(description)),
            ("id", MenuText.Literal(id.ToString(CultureInfo.InvariantCulture)))).Resolve(Localizer.Current);

        Received.Add(id);
        OnScreen.Add(id);

        Native.SendNuiMessage(ClientJson.Serialize(new AlertMessage
        {
            Id = id,
            Text = text,
            Duration = durationMs,
        }));
    }

    private static int DisplayMs() =>
        Math.Max(1, ClientConfig.Value(StaffAlertSettings.DisplaySeconds)) * 1000;

    private static async Task ForgetAsync(int id)
    {
        var window = Math.Max(0, ClientConfig.Value(StaffAlertSettings.ExpireSeconds)) + ForgetGraceSeconds;

        await API.Delay(window * 1000);

        Received.Remove(id);
        OnScreen.Remove(id);
    }

    private static void OnResolved(string alertId, string staff, string responderServerId)
    {
        if (!int.TryParse(alertId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            return;
        }

        if (!Received.Remove(id))
        {
            return;
        }

        Close(id);

        if (int.TryParse(responderServerId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var responder)
            && responder == Native.GetPlayerServerId(Native.PlayerId()))
        {
            return;
        }

        Notifications.Info(MenuText.Key(
            Loc.MiscSettings.AlertStaffTaken,
            ("staff", MenuText.Literal(staff)),
            ("id", MenuText.Literal(alertId))));
    }
    private static void OnExpired(string alertId, string from)
    {
        if (!int.TryParse(alertId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            || !Received.Remove(id))
        {
            return;
        }

        Close(id);

        Notifications.Warning(MenuText.Key(
            Loc.MiscSettings.AlertStaffExpired,
            ("player", MenuText.Literal(from)),
            ("id", MenuText.Literal(alertId))));
    }

    private static void OnDismissed(string alertId, string staff, string closerServerId)
    {
        if (!int.TryParse(alertId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            || !Received.Remove(id))
        {
            return;
        }

        Close(id);

        if (int.TryParse(closerServerId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var closer)
            && closer == Native.GetPlayerServerId(Native.PlayerId()))
        {
            return;
        }

        Notifications.Info(MenuText.Key(
            Loc.StaffAlerts.DismissedByStaff,
            ("staff", MenuText.Literal(staff)),
            ("id", MenuText.Literal(alertId))));
    }

    private static void OnDismissedNotice(string staff) =>
        Notifications.Warning(MenuText.Key(
            Loc.StaffAlerts.YourAlertDismissed,
            ("staff", MenuText.Literal(staff))));

    /// <summary>Takes one banner off this screen, if it is still up.</summary>
    private static void Close(int id)
    {
        if (OnScreen.Remove(id))
        {
            Native.SendNuiMessage(ClientJson.Serialize(new AlertCloseMessage { Id = id }));
        }
    }

    private static void Dismiss()
    {
        API.RunOnMainThread(() =>
        {
            var cleared = OnScreen.Count;

            if (cleared == 0)
            {
                Notifications.Info(MenuText.Key(Loc.MiscSettings.AlertDismissNothing));

                return;
            }

            Clear();

            Notifications.Success(MenuText.Key(
                Loc.MiscSettings.AlertDismissed,
                ("count", MenuText.Literal(cleared.ToString(CultureInfo.InvariantCulture)))));
        });
    }

    private static void Clear()
    {
        if (OnScreen.Count == 0)
        {
            return;
        }

        OnScreen.Clear();

        Native.SendNuiMessage(ClientJson.Serialize(new AlertClearMessage()));
    }

    private static void Report() => API.EmitServer(StaffAlertEvents.ReportHidden, Hidden);

    private static void Apply()
    {
        var wanted = Enabled && ClientPermissions.IsAllowed(Global.Staff);

        RespondCommand.Apply(wanted);
        DismissCommand.Apply(wanted);

        Report();
    }

    private static async void Respond(string raw)
    {
        await API.JumpToMainThread();
        try
        {
            if (Argument(raw) is not { } token)
            {
                Notifications.Warning(MenuText.Key(Loc.MiscSettings.AlertRespondUsage));

                return;
            }

            await RespondToAsync(token);
        }
        catch (Exception exception)
        {
            Log.Error($"[StaffAlerts] /{RespondCommandName} failed: {exception}");
        }
    }

    public static async Task<bool> RespondToAsync(string token)
    {
        var alertId = MenuText.Literal(token);

        var result = await ServerActions.InvokeAsync(ActionIds.StaffAlerts.Respond, token);

        if (result.Status == ActionStatus.Refused && result.Data.Length > 0)
        {
            Notifications.Warning(MenuText.Key(
                Loc.MiscSettings.AlertRespondAlreadyTaken,
                ("staff", MenuText.Literal(result.Data[0])),
                ("id", alertId)));

            return false;
        }

        if (result.Status == ActionStatus.NotReady)
        {
            Notifications.Warning(MenuText.Key(Loc.MiscSettings.AlertRespondExpired, ("id", alertId)));

            return false;
        }

        if (result.Status != ActionStatus.Ok || result.Data.Length < 4)
        {
            Notifications.Error(MenuText.Key(Loc.MiscSettings.AlertRespondGone, ("id", alertId)));

            return false;
        }

        if (!TryParse(result.Data[0], out var x)
            || !TryParse(result.Data[1], out var y)
            || !TryParse(result.Data[2], out var z))
        {
            Notifications.Error(MenuText.Key(Loc.OnlinePlayers.Failed));

            return false;
        }

        if (NoClip.NoClip.Enable())
        {
            await API.Delay(NoClipSettleMs);
        }

        await PlayerTeleport.ToCoordsAsync(new Vector3(x, y, z));

        Notifications.Success(MenuText.Key(
            Loc.MiscSettings.AlertRespondDone,
            ("player", MenuText.Literal(result.Data[3]))));

        return true;
    }

    private static string? Argument(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length >= 2 ? parts[1] : null;
    }

    private static void StartCooldown(string seconds)
    {
        if (int.TryParse(seconds, NumberStyles.Integer, CultureInfo.InvariantCulture, out var wait) && wait > 0)
        {
            _nextAllowedAt = Native.GetGameTimer() + (wait * 1000);
        }
    }

    private static int Remaining()
    {
        var left = _nextAllowedAt - Native.GetGameTimer();

        return left <= 0 ? 0 : (int)Math.Ceiling(left / 1000f);
    }

    private static bool TryParse(string value, out float result) =>
        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

    private sealed class StaffCommand(string name, Action<string> run)
    {
        private readonly Action<int, MessagePackBuffer, string> _handler = (_, _, raw) => run(raw);

        private int? _id;

        public void Apply(bool wanted)
        {
            if (wanted && _id is null)
            {
                _id = NativeFixer.RegisterCommand(name, restricted: false, _handler);

                Log.Debug($"[StaffAlerts] Registered /{name}.");
            }
            else if (!wanted && _id is not null)
            {
                Native.UnregisterCommand(_id.Value);
                _id = null;

                Log.Debug($"[StaffAlerts] Unregistered /{name}.");
            }
        }
    }

    private sealed class AlertMessage
    {
        public string Type { get; } = "staff_alert";

        public required int Id { get; init; }

        public required string Text { get; init; }

        public required int Duration { get; init; }
    }

    private sealed class AlertClearMessage
    {
        public string Type { get; } = "staff_alert_clear";
    }

    private sealed class AlertCloseMessage
    {
        public string Type { get; } = "staff_alert_close";

        public required int Id { get; init; }
    }
}
