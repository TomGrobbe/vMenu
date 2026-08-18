using System.Numerics;

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
    /// Replacement call for <see cref="Native.GetAllVehicleModels()" /> because that return type is <em>byte[]</em>.
    /// </summary>
    /// <returns></returns>
    public static string[] GetAllVehicleModels()
    {
        nativeApi.ResetContext();
        nativeApi.Invoke(3612546629uL, "GetAllVehicleModels");
        return nativeApi.GetResObject(0).DeserializeTo<string[]>();
    }

    /// <summary>
    /// Replacement call for <see cref="Native.GetGamePool(string)" /> because that return type is <em>byte[]</em>.
    /// The only working way to enumerate the world.
    /// </summary>
    /// <param name="poolName"><em>CVehicle</em>, <em>CPed</em>, <em>CObject</em> or <em>CPickup</em>.</param>
    public static int[] GetGamePool(string poolName)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(poolName);
        nativeApi.Invoke(731729744uL, "GetGamePool");
        return nativeApi.GetResObject(0).DeserializeTo<int[]>();
    }

    /// <summary>
    /// Replacement call for <see cref="Native.GetModelDimensions(uint, out Vector3, out Vector3)" /> because <em>nativeApi.PushArg(default(Vector3))</em> is not supported.
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    public static void GetModelDimensions(uint p0, out Vector3 p1, out Vector3 p2)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(p0);
        nativeApi.PushArg(default(ObjectArg));
        nativeApi.PushArg(default(ObjectArg));
        nativeApi.Invoke(14500376258260264975uL, "GetModelDimensions");
        p1 = nativeApi.GetResVector(1).ToVector();
        p2 = nativeApi.GetResVector(2).ToVector();
    }

    /// <summary>
    /// Replacement call for <see cref="Native.GetShapeTestResult(int, out int, out Vector3, out Vector3, out int)" />
    /// because <em>nativeApi.PushArg(default(Vector3))</em> is not supported. The <em>Ref&lt;Vector3&gt;</em>
    /// overload pushes a Vector3 too, so neither generated form is usable.
    /// </summary>
    /// <returns>0 when the handle is not a shape test, 1 while the result is not ready, 2 once it is.</returns>
    public static int GetShapeTestResult(int shapeTestHandle, out int hit, out Vector3 endCoords, out Vector3 surfaceNormal, out int entityHit)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(shapeTestHandle);
        nativeApi.PushArg(0);
        nativeApi.PushArg(default(ObjectArg));
        nativeApi.PushArg(default(ObjectArg));
        nativeApi.PushArg(0);
        nativeApi.Invoke(1044221499265592803uL, "GetShapeTestResult");
        hit = nativeApi.GetResInt(1);
        endCoords = nativeApi.GetResVector(2).ToVector();
        surfaceNormal = nativeApi.GetResVector(3).ToVector();
        entityHit = nativeApi.GetResInt(4);
        return nativeApi.GetResInt(0);
    }

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
    /// Replacement call for <see cref="Native.GetWeaponHudStats(uint, int)" />, whose second
    /// parameter is the forty byte struct the game writes into, generated as a raw <em>int</em>
    /// with no way to produce one.
    /// </summary>
    /// <param name="stats">Not filled in by the time this returns. See the remarks.</param>
    /// <returns>False when the game has no stats for this weapon.</returns>
    /// <remarks>
    /// An int argument reserves a single value slot, so the game's forty byte write runs off the end
    /// of it and takes the client down. <see cref="INativeStruct" /> is the only push that reserves
    /// the real size: the runtime allocates a buffer of <em>Marshal.SizeOf</em>, hands the game that,
    /// and copies it back into <paramref name="stats"/> at the start of its next tick. So the values
    /// are readable from the following frame onwards, not from this call.
    /// </remarks>
    public static bool GetWeaponHudStats(uint weaponHash, WeaponHudStatsData stats)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(weaponHash);
        nativeApi.PushArg(stats);
        nativeApi.Invoke(8675070465420327855uL, "GetWeaponHudStats");

        return nativeApi.GetResBool(0);
    }

    /// <summary>
    /// Replacement call for <see cref="Native.GetWeaponComponentHudStats(uint, int)" />, broken and
    /// fixed the same way <see cref="GetWeaponHudStats" /> is, down to the delayed
    /// <paramref name="stats"/>.
    /// </summary>
    /// <param name="stats">How much this component moves each bar, positive or negative.</param>
    /// <returns>False when the game has no stats for this component.</returns>
    public static bool GetWeaponComponentHudStats(uint componentHash, WeaponHudStatsData stats)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(componentHash);
        nativeApi.PushArg(stats);
        nativeApi.Invoke(17640523594652482560uL, "GetWeaponComponentHudStats");

        return nativeApi.GetResBool(0);
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
    /// Replacement call for <c>Native.RemoveBlip</c>, which is generated only as
    /// <em>RemoveBlip(out int)</em> and <em>RemoveBlip(Ref&lt;int&gt;)</em>.
    /// </summary>
    /// <remarks>
    /// Both generated forms reserve an empty slot and hand the game that instead of the handle being
    /// removed, so neither can delete a blip. Without this, nothing vMenu creates can ever be taken
    /// off the map again.
    /// </remarks>
    public static void RemoveBlip(int blip)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(blip);
        nativeApi.Invoke(18326475465518923026uL, "RemoveBlip");
    }

    /// <summary>
    /// Replacement call for <see cref="Native.GetStateBagValue(string, string)" /> because that
    /// return type is <em>byte[]</em>.
    /// </summary>
    /// <returns>The stored value, or <see langword="default"/> when the key has never been written.</returns>
    public static T? GetStateBagValue<T>(string bagName, string keyName)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(bagName);
        nativeApi.PushArg(keyName);
        nativeApi.Invoke(1669287029uL, "GetStateBagValue");

        return nativeApi.GetResObject(0).DeserializeTo<T?>();
    }

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
