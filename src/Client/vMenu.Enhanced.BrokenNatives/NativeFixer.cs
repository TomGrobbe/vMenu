using System.Numerics;

using CitizenFX.Base;
using CitizenFX.Base.Data;
using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

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
}
