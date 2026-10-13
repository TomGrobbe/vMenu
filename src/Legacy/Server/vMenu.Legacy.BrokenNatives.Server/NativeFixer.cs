using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.BrokenNatives.Server;

public static partial class NativeFixer
{
    public static int[] GetAllVehicles() => Payload.Ints((object)API.GetAllVehicles());

    public static int[] GetAllPeds() => Payload.Ints((object)API.GetAllPeds());

    public static bool SaveResourceFile(string resource, string file, byte[] buffer)
    {
        var text = System.Text.Encoding.UTF8.GetString(buffer);

        return API.SaveResourceFile(resource, file, text, System.Text.Encoding.UTF8.GetByteCount(text));
    }

    public static void EmitLocal(string eventName, params object?[] args) => BaseScript.TriggerEvent(eventName, args);

    public static int AddConvarChangeListener(string convar, Action<string, object?> handler) =>
        API.AddConvarChangeListener(convar, new Action<object, object>((name, reserved) => handler(name?.ToString() ?? string.Empty, reserved)));

    public static void SetHttpHandler(Action<IDictionary<string, object>, IDictionary<string, object>> handler) =>
        API.SetHttpHandler(new Action<object, object>((request, response) =>
            handler((IDictionary<string, object>)request, (IDictionary<string, object>)response)));
}
