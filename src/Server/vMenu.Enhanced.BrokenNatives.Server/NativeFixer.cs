using CitizenFX.Base;
using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.BrokenNatives.Server;

/// <summary>
/// Natives that are broken in the API get fixed here.
/// </summary>
public static class NativeFixer
{
    internal static NativeApi nativeApi = BaseEntrypoint.NativeApi;

    public static bool SaveResourceFile(string resource, string file, string buffer)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(resource);
        nativeApi.PushArg(file);
        nativeApi.PushArg(buffer);
        nativeApi.PushArg(-1);
        nativeApi.Invoke(2694741627uL, "SaveResourceFile");
        return nativeApi.GetResBool(0);
    }

    /// <summary>
    /// Replacement call for <see cref="Native.AddConvarChangeListener(string, int)" />, whose
    /// <em>func</em> parameter is generated as a raw <em>int</em> with no way to produce one.
    /// </summary>
    /// <remarks>
    /// The reference must come from the shared <c>FuncRefManager</c>, not from
    /// <see cref="NativeApi.RegisterFunctionReference(Delegate)" />: the runtime dispatches every
    /// callback through the shared manager's table, so a reference registered anywhere else comes
    /// back as "Invalid function" when it fires.
    /// </remarks>
    /// <param name="handler">Called with the convar name; the second argument is reserved by FiveM.</param>
    public static int AddConvarChangeListener(string convar, Action<string, object?> handler)
    {
        // GetCore is flagged as internal API, and is taken anyway because it is the only route to
        // that manager: SharedAPI exposes Log, Side, Exports and Commands, none of which can
        // register a reference. If a future runtime removes it, this call is the one thing to move.
#pragma warning disable FIVEM001
        var reference = SharedAPI.GetCore().FuncRefManager.Register(handler);
#pragma warning restore FIVEM001

        return Native.AddConvarChangeListener(convar, unchecked((int)reference));
    }
}
