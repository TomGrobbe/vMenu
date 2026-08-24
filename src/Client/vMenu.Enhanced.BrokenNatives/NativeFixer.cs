using CitizenFX.Base;
using CitizenFX.Base.Data;
using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

using MessagePack;

namespace vMenu.Enhanced.BrokenNatives;

public static class NativeFixer
{
    internal static NativeApi nativeApi = BaseEntrypoint.NativeApi;

    // Native.GetAllVehicleModels still returns a raw byte[]. Standard options match how StateBags and
    // EmitLocal talk to the runtime, which speaks plain MessagePack for primitive arrays like this one.
    public static string[] GetAllVehicleModels() =>
        MessagePackSerializer.Deserialize<string[]>(
            Native.GetAllVehicleModels(), MessagePackSerializerOptions.Standard);

    // Native.GetGamePool still returns a raw byte[]. The only working way to enumerate the world.
    // Pool names: CVehicle, CPed, CObject or CPickup.
    public static int[] GetGamePool(string poolName) =>
        MessagePackSerializer.Deserialize<int[]>(
            Native.GetGamePool(poolName), MessagePackSerializerOptions.Standard);

    public static List<(uint Collection, uint Overlay)> GetPedDecorations(int ped)
    {
        var raw = MessagePackSerializer.Deserialize<long[][]?>(
            Native.GetPedDecorations(ped), MessagePackSerializerOptions.Standard);

        var decorations = new List<(uint, uint)>();

        if (raw is null)
        {
            return decorations;
        }

        foreach (var pair in raw)
        {
            if (pair is { Length: >= 2 })
            {
                decorations.Add(((uint)pair[0], (uint)pair[1]));
            }
        }

        return decorations;
    }

    // Both generated forms of TestVerticalProbeAgainstAllWater read the height back before checking
    // whether there is one, and the game only fills it when it found something, so an unfilled slot
    // throws "Failed to get result". Answers 0 nothing found, 1 reached water, 2 blocked short of it.
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

    public static void SetFacialIdleAnimOverride(int ped, string clipName)
    {
        using var clip = new StringArg(clipName);
        using var dictionary = new StringArg(null);

        nativeApi.ResetContext();
        nativeApi.PushArg(ped);
        nativeApi.PushArg(clip);
        nativeApi.PushArg(dictionary);
        // Hash from the wrapper IL, not the published tables: Enhanced remaps it.
        nativeApi.Invoke(3173285894442253041uL, "SetFacialIdleAnimOverride");
    }

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

    // SharedAPI.Commands.RegisterCommand throws away the id UnregisterCommand needs, so a command
    // registered through it can never be removed. Use the normal one unless it has to come and go.
    public static int RegisterCommand(string command, bool restricted, Action<int, MessagePackBuffer, string> handler)
    {
        // Same registry as AddConvarChangeListener, for the same reason.
#pragma warning disable FIVEM001
        var reference = SharedAPI.GetCore().FuncRefManager.Register(handler);
#pragma warning restore FIVEM001

        return Native.RegisterCommand(command, (int)reference, restricted);
    }

    // AddConvarChangeListener's func parameter is generated as a raw int with no way to produce one.
    // The reference must come from the shared FuncRefManager, because the runtime dispatches every
    // callback through that table and one registered elsewhere comes back as "Invalid function".
    public static int AddConvarChangeListener(string convar, Action<string, object?> handler)
    {
        // GetCore is internal API, taken because it is the only route to that manager. If a future runtime
        // removes it, this call is the one thing to move.
#pragma warning disable FIVEM001
        var reference = SharedAPI.GetCore().FuncRefManager.Register(handler);
#pragma warning restore FIVEM001

        return Native.AddConvarChangeListener(convar, (int)reference);
    }

    // Native.GetStateBagValue still returns a raw byte[]. Standard options are the matching pair to
    // StateBags.Set, which writes with the same ones.
    public static T? GetStateBagValue<T>(string bagName, string keyName) =>
        MessagePackSerializer.Deserialize<T?>(
            Native.GetStateBagValue(bagName, keyName), MessagePackSerializerOptions.Standard);

    // AddStateBagChangeHandler can neither produce the func reference nor pass the nulls this native
    // expects. Null is how it spells "anything", but NativeApi.PushArg matches on string before
    // anything else and throws "Unsupported type" on a null one, so StringArg is the only push that
    // carries a null pointer through. Reached only through StateBags.Watch, which is not used yet.
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
