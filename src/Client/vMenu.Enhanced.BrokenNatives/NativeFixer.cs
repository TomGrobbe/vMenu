using CitizenFX.Base;
using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.BrokenNatives;

/// <summary>
/// Natives that are broken in the API get fixed here.
/// </summary>
public static class NativeFixer
{
    internal static NativeApi nativeApi = BaseEntrypoint.NativeApi;

    /// <summary>
    /// Replacement call for <see cref="Native.GetAllVehicleModels" /> because that return type is <pre>byte[]</pre>
    /// </summary>
    /// <returns></returns>
    public static string[] GetAllVehicleModels()
    {
        nativeApi.ResetContext();
        nativeApi.Invoke(3612546629uL, "GetAllVehicleModels");
        return nativeApi.GetResObject(0).DeserializeTo<string[]>();
    }
}
