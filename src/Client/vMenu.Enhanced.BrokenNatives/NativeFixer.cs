using CitizenFX.Base;
using CitizenFX.Base.Data;
using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

using MessagePack;

namespace vMenu.Enhanced.BrokenNatives;

/// <summary>Natives that are broken in the API get fixed here.</summary>
public static class NativeFixer
{
    internal static NativeApi nativeApi = BaseEntrypoint.NativeApi;

    /// <summary>
    /// Replacement call for <see cref="Native.GetAllVehicleModels()" /> because that return type is
    /// still a raw <em>byte[]</em> with no typed API over it.
    /// </summary>
    // Standard options match how StateBags and EmitLocal talk to the runtime, which speaks plain
    // MessagePack for primitive arrays like this one.
    public static string[] GetAllVehicleModels() =>
        MessagePackSerializer.Deserialize<string[]>(
            Native.GetAllVehicleModels(), MessagePackSerializerOptions.Standard);

    /// <summary>
    /// Replacement call for <see cref="Native.GetGamePool(string)" /> because that return type is
    /// still a raw <em>byte[]</em>. The only working way to enumerate the world.
    /// </summary>
    /// <param name="poolName"><em>CVehicle</em>, <em>CPed</em>, <em>CObject</em> or <em>CPickup</em>.</param>
    public static int[] GetGamePool(string poolName) =>
        MessagePackSerializer.Deserialize<int[]>(
            Native.GetGamePool(poolName), MessagePackSerializerOptions.Standard);

    /// <summary>
    /// Replacement call for <see cref="Native.TestVerticalProbeAgainstAllWater(float, float, float, int, out float)" />
    /// because both generated forms read the height back before checking whether there is one.
    /// </summary>
    /// <param name="blockingFlags">What is allowed to block the probe before it reaches water.</param>
    /// <param name="height">Where the probe stopped, whether that was water or the thing that blocked it.</param>
    /// <returns>0 nothing found, 1 reached water, 2 blocked short of it. <paramref name="height"/> is good for 1 and 2.</returns>
    // Fires straight down from the given point, and unlike the ground Z natives it answers for land
    // and water in one call. The game only fills the height when it found something, and reading an
    // unfilled slot throws "Failed to get result", which is what both generated forms do first.
    public static int TestVerticalProbeAgainstAllWater(float x, float y, float z, int blockingFlags, out float height)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(x);
        nativeApi.PushArg(y);
        nativeApi.PushArg(z);
        nativeApi.PushArg(blockingFlags);
        nativeApi.PushArg(0f);
        nativeApi.Invoke(6018883233978920895uL, "TestVerticalProbeAgainstAllWater");

        var result = nativeApi.GetResInt(0);

        height = result == 0 ? default : nativeApi.GetResFloat(1);

        return result;
    }

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
    /// Replacement call for <c>SharedAPI.Commands.RegisterCommand</c>, which throws away the id
    /// <see cref="Native.UnregisterCommand(int)" /> needs, so a command registered through it can
    /// never be removed. Use the normal one unless the command has to come and go at runtime.
    /// </summary>
    /// <param name="handler">Invoked with the source, the arguments, and the raw command text.</param>
    /// <returns>The command id.</returns>
    public static int RegisterCommand(string command, bool restricted, Action<int, MessagePackBuffer, string> handler)
    {
        // Same registry as AddConvarChangeListener, for the same reason.
#pragma warning disable FIVEM001
        var reference = SharedAPI.GetCore().FuncRefManager.Register(handler);
#pragma warning restore FIVEM001

        return Native.RegisterCommand(command, (int)reference, restricted);
    }

    /// <summary>
    /// Replacement call for <see cref="Native.AddConvarChangeListener(string, int)" />, whose
    /// <em>func</em> parameter is generated as a raw <em>int</em> with no way to produce one.
    /// </summary>
    /// <param name="handler">Called with the convar name. The second argument is reserved by FiveM.</param>
    // The reference must come from the shared FuncRefManager. The runtime dispatches every callback
    // through that table, so one registered anywhere else comes back as "Invalid function".
    public static int AddConvarChangeListener(string convar, Action<string, object?> handler)
    {
        // GetCore is internal API, taken because it is the only route to that manager. If a future
        // runtime removes it, this call is the one thing to move.
#pragma warning disable FIVEM001
        var reference = SharedAPI.GetCore().FuncRefManager.Register(handler);
#pragma warning restore FIVEM001

        return Native.AddConvarChangeListener(convar, (int)reference);
    }

    /// <summary>
    /// Replacement call for <see cref="Native.GetStateBagValue(string, string)" /> because that
    /// return type is still a raw <em>byte[]</em>.
    /// </summary>
    /// <returns>The stored value, or <see langword="default"/> when the key has never been written.</returns>
    // Standard options are the matching pair to StateBags.Set, which writes with the same ones.
    public static T? GetStateBagValue<T>(string bagName, string keyName) =>
        MessagePackSerializer.Deserialize<T?>(
            Native.GetStateBagValue(bagName, keyName), MessagePackSerializerOptions.Standard);

    /// <summary>
    /// Replacement call for <see cref="Native.AddStateBagChangeHandler(string, string, int)" />,
    /// which can neither produce the <em>func</em> reference nor pass the nulls this native expects.
    /// </summary>
    /// <param name="keyName">The key to watch, or <see langword="null"/> for every key.</param>
    /// <param name="bagName">The bag to watch, or <see langword="null"/> for every bag.</param>
    /// <param name="handler">Called with the bag name, the key, the new value, a reserved slot, and whether the write was replicated.</param>
    /// <returns>A cookie for <see cref="Native.RemoveStateBagChangeHandler(int)" />.</returns>
    /// <remarks>
    /// Null is how this native spells "anything", and it is the normal way to use it, but
    /// <c>NativeApi.PushArg</c> matches on <em>string</em> before anything else and throws
    /// "Unsupported type" on a null one. <see cref="StringArg" /> is the only push that carries a
    /// null pointer through.
    /// </remarks>
    public static int AddStateBagChangeHandler(string? keyName, string? bagName, Delegate handler)
    {
        // Same registry as AddConvarChangeListener, for the same reason.
#pragma warning disable FIVEM001
        var reference = SharedAPI.GetCore().FuncRefManager.Register(handler);
#pragma warning restore FIVEM001

        // Freed only after the call: the game reads through these pointers during Invoke.
        using var key = new StringArg(keyName);
        using var bag = new StringArg(bagName);

        nativeApi.ResetContext();
        nativeApi.PushArg(key);
        nativeApi.PushArg(bag);
        nativeApi.PushArg((int)reference);
        nativeApi.Invoke(1537432239uL, "AddStateBagChangeHandler");

        return nativeApi.GetResInt(0);
    }
}
