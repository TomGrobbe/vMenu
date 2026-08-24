using CitizenFX.Base;

using MessagePack;

namespace vMenu.Enhanced.ServerAPI;

// Sends an event to vMenu without reading the answer back. API.EmitLocal reads the triggering
// native's return value once every handler has run, and the runtime keeps one native call context
// per resource per thread, so the moment vMenu answers back into this plugin the native invoked
// while serving that answer resets the context the first call is still holding its return value in.
// The event has already been delivered by then and nothing here looks at the answer, so this never
// asks for it.
internal static class PluginEmit
{
    private const ulong TriggerEventInternal = 2435909744uL;

    private static readonly NativeApi NativeApi = BaseEntrypoint.NativeApi;

    // Strings and numbers only. MessagePack's standard options rather than the runtime's own, which
    // additionally know FiveM's types and are not reachable from here without internal API.
    internal static void Local(string eventName, params object?[] args)
    {
        var payload = MessagePackSerializer.Serialize(args, MessagePackSerializerOptions.Standard);

        NativeApi.ResetContext();
        NativeApi.PushArg(eventName);
        NativeApi.PushArg(payload);
        NativeApi.PushArg(payload.Length);
        NativeApi.Invoke(TriggerEventInternal, nameof(TriggerEventInternal));
    }
}
