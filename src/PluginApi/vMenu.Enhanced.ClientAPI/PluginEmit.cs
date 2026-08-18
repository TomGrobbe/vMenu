using CitizenFX.Base;

using MessagePack;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>
/// Sends an event to vMenu without reading the answer back.
/// </summary>
/// <remarks>
/// <para>
/// <c>API.EmitLocal</c> reads the triggering native's return value once every handler has run.
/// The runtime keeps one native call context per resource per thread, and those handlers run
/// inside the very call that triggered them. vMenu handling the event is harmless on its own, it
/// having a context of its own, but the moment it answers back into this plugin, the native this
/// plugin invokes while serving that answer resets the context the first call is still holding its
/// return value in. Reading it then either throws "Failed to get result" or quietly answers with
/// whatever that other native returned.
/// </para>
/// <para>
/// The event has already been delivered by the time that happens, and nothing here looks at the
/// answer, so this never asks for it.
/// </para>
/// </remarks>
internal static class PluginEmit
{
    private const ulong TriggerEventInternal = 2435909744uL;

    private static readonly NativeApi NativeApi = BaseEntrypoint.NativeApi;

    /// <param name="eventName">The event vMenu listens to.</param>
    /// <param name="args">Strings and numbers only, which serialize the same either way.</param>
    // MessagePack's standard options rather than the runtime's own, which additionally know
    // FiveM's types and are not reachable from here without internal API.
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
