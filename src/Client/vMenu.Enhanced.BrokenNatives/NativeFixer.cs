using System.Numerics;

using CitizenFX.Base;
using CitizenFX.Base.Data;
using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

namespace vMenu.Enhanced.BrokenNatives;

/// <summary>
/// Natives that are broken in the API get fixed here.
/// </summary>
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
    /// </summary>
    /// <remarks>
    /// The only working way to enumerate the world: there is no <em>World</em> type in Enhanced, the
    /// entity pools are caches of handles this resource already touched, and
    /// <see cref="Native.GetAllVehicles(out int)" /> is generated with the returned array collapsed
    /// to a single <em>int</em>.
    /// </remarks>
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
    /// Replacement call for <c>SharedAPI.Commands.RegisterCommand</c>, which throws away the id
    /// <see cref="Native.UnregisterCommand(int)" /> needs, so a command registered through it can
    /// never be removed.
    /// </summary>
    /// <param name="handler">Invoked with the source, the arguments, and the raw command text.</param>
    /// <returns>The command id.</returns>
    public static int RegisterCommand(string command, bool restricted, Action<int, MessagePackBuffer, string> handler)
    {
        // Same registry as AddConvarChangeListener, for the same reason.
#pragma warning disable FIVEM001
        var reference = SharedAPI.GetCore().FuncRefManager.Register(handler);
#pragma warning restore FIVEM001

        return Native.RegisterCommand(command, unchecked((int)reference), restricted);
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
