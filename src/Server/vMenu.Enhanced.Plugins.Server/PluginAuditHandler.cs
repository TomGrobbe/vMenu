using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Webhooks.Server;

namespace vMenu.Enhanced.Plugins.Server;

public static class PluginAuditHandler
{
    private const int MaxResourceLength = 64;

    private const int MaxItemLength = 96;

    private const int MaxValueLength = 64;

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(
            AuditEvents.Plugin,
            new Action<Player, string, string, string, string>(OnPluginAction),
            false);
    }

    private static void OnPluginAction(
        [FromSource] Player source,
        string resource,
        string itemId,
        string kind,
        string value)
    {
        if (!WebhookLog.WantsMenuActions || !MenuAuditHandler.TryTakeAuditSlot(source.Handle))
        {
            return;
        }

        var owner = WebhookText.Clean(resource, MaxResourceLength);
        var id = WebhookText.Clean(itemId, MaxItemLength);

        if (owner.Length == 0 || id.Length == 0 || !PluginRegistry.TryLoggedItem(owner, id, out var description))
        {
            return;
        }

        WebhookLog.Action(
            WebhookActor.For(source),
            MenuAuditHandler.PhraseFor(kind, description, WebhookText.Clean(value, MaxValueLength)),
            ("plugin", owner));
    }
}
