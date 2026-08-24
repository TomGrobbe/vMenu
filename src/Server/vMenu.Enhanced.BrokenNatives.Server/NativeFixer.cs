using CitizenFX.Base;
using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

using MessagePack;

namespace vMenu.Enhanced.BrokenNatives.Server;

public static class NativeFixer
{
    internal static NativeApi nativeApi = BaseEntrypoint.NativeApi;

    // API.EmitLocal's answer cannot be read back once anything has re-entered this resource. The runtime
    // keeps one native call context per resource per thread, so a handler calling back in resets the
    // context our own emit is still holding its return value in. The event is delivered either way and
    // nothing reads the answer, so this form never asks for it. Primitives only.
    public static void EmitLocal(string eventName, params object?[] args)
    {
        var payload = MessagePackSerializer.Serialize(args, MessagePackSerializerOptions.Standard);

        nativeApi.ResetContext();
        nativeApi.PushArg(eventName);
        nativeApi.PushArg(payload);
        nativeApi.PushArg(payload.Length);
        nativeApi.Invoke(2435909744uL, "TriggerEventInternal");
    }

    // AddConvarChangeListener's func parameter is generated as a raw int with no way to produce one.
    // The reference must come from the shared FuncRefManager, because the runtime dispatches every
    // callback through that table and one registered elsewhere comes back as "Invalid function".
    public static int AddConvarChangeListener(string convar, Action<string, object?> handler)
    {
        // GetCore is flagged as internal API, and is taken anyway because it is the only route to that
        // manager. If a future runtime removes it, this call is the one thing to move.
#pragma warning disable FIVEM001
        var reference = SharedAPI.GetCore().FuncRefManager.Register(handler);
#pragma warning restore FIVEM001

        return Native.AddConvarChangeListener(convar, (int)reference);
    }
}
