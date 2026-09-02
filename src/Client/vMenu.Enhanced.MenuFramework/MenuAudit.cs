using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

public static class MenuAudit
{
    private const int BurstLimit = 20;

    private const int BurstWindowMs = 5000;

    private static readonly List<int> Sent = [];

    private static bool _wanted;

    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        ClientConfig.Track(AuditStateConvars.All);
        ClientConfig.AddEventListenerFor(AuditStateConvars.All, Read);

        Read();
    }

    internal static void Report(string menu, MenuText text, string kind, string value)
    {
        if (!_wanted)
        {
            return;
        }

        var item = text.TranslationKey;

        if (string.IsNullOrEmpty(item) || !AuditedMenuItems.Includes(item!) || !Allowed())
        {
            return;
        }

        API.EmitServer(AuditEvents.Menu, menu, item!, kind, value);
    }

    public static void ReportAction(string action, string value = "", string detail = "")
    {
        if (!_wanted || !Allowed())
        {
            return;
        }

        API.EmitServer(AuditEvents.Action, action, value, detail);
    }

    public static void ReportPluginItem(string resource, string itemId, string kind, string value)
    {
        if (!_wanted || !Allowed())
        {
            return;
        }

        API.EmitServer(AuditEvents.Plugin, resource, itemId, kind, value);
    }

    internal static void ReportTheme(string resource, string id, string name)
    {
        if (!_wanted)
        {
            return;
        }

        API.EmitServer(AuditEvents.Theme, resource, id, name);
    }

    private static void Read()
    {
        var raw = ClientConfig.GetString(AuditStateConvars.MenuReporting);
        var wanted = string.Equals(raw, AuditStateConvars.On, StringComparison.Ordinal);

        if (wanted == _wanted)
        {
            return;
        }

        _wanted = wanted;

        Sent.Clear();

        Log.Debug($"[Audit] The server {(wanted ? "wants" : "does not want")} menu actions reported.");
    }

    private static bool Allowed()
    {
        var now = Native.GetGameTimer();

        while (Sent.Count > 0 && now - Sent[0] >= BurstWindowMs)
        {
            Sent.RemoveAt(0);
        }

        if (Sent.Count >= BurstLimit)
        {
            return false;
        }

        Sent.Add(now);

        return true;
    }
}
