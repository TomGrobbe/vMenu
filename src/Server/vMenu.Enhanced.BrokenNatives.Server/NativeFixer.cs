using CitizenFX.Base;
using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

using MessagePack;

namespace vMenu.Enhanced.BrokenNatives.Server;

/// <summary>
/// Natives that are broken in the API get fixed here.
/// </summary>
public static class NativeFixer
{
    internal static NativeApi nativeApi = BaseEntrypoint.NativeApi;

    /// <summary>
    /// Replacement call for <c>API.EmitLocal</c>, whose answer cannot be read back once anything
    /// has re-entered this resource. Use it for every event another resource may listen to.
    /// </summary>
    /// <param name="args">Primitives only. See the remarks.</param>
    /// <remarks>
    /// <para>
    /// The runtime keeps one native call context per resource per thread, and the handlers of a
    /// local event run inside the call that triggered it. A handler in another resource is
    /// harmless, that resource having a context of its own, but the moment one of them calls back
    /// into this resource, the native we invoke while serving that call resets the very context our
    /// own emit is still holding its return value in. Reading it afterwards either throws "Failed
    /// to get result" or quietly answers with whatever that other native returned.
    /// </para>
    /// <para>
    /// The plugin protocol does exactly that on every interaction: vMenu emits a callback, the
    /// plugin's handler sends an update straight back, and serving that update starts with
    /// <c>GetInvokingResource</c>. The event is delivered either way, and nothing reads the answer,
    /// so this form never asks for it.
    /// </para>
    /// <para>
    /// Arguments go through MessagePack's standard options rather than the runtime's own, which
    /// additionally know FiveM's types. Strings, numbers and booleans are identical either way.
    /// </para>
    /// </remarks>
    public static void EmitLocal(string eventName, params object?[] args)
    {
        var payload = MessagePackSerializer.Serialize(args, MessagePackSerializerOptions.Standard);

        nativeApi.ResetContext();
        nativeApi.PushArg(eventName);
        nativeApi.PushArg(payload);
        nativeApi.PushArg(payload.Length);
        nativeApi.Invoke(2435909744uL, "TriggerEventInternal");
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

        return Native.AddConvarChangeListener(convar, (int)reference);
    }
}
