using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Script;
using CitizenFX.FiveM.Shared.Serialization;

using MessagePack;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>NUI callbacks whose handlers read the request body as the JSON the page posted.</summary>
public static class NuiCallbacks
{
    /// <summary>The handler is handed the request body as JSON text.</summary>
    public static void Register(string callback, Action<string> handler) =>
        API.OnNuiEvent(callback, new Action<MessagePackBuffer?, NuiCallback?>(new Receiver(handler).Receive));

    private sealed class Receiver(Action<string> handler)
    {
        // Answered before the handler runs, so a throwing handler never leaves the page's request hanging.
        public void Receive(MessagePackBuffer? data, NuiCallback? reply)
        {
            reply?.Invoke("ok");

            handler(data is null ? string.Empty : MessagePackSerializer.ConvertToJson(data.Buffer));
        }
    }
}
