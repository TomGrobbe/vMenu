using System.Numerics;

using CitizenFX.Base;
using CitizenFX.Base.Data;
using CitizenFX.FiveM.Client;

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
}
