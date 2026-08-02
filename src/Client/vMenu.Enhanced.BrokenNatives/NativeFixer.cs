using System.Numerics;

using CitizenFX.Base;
using CitizenFX.Base.Data;
using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

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
