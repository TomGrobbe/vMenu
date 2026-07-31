using CitizenFX.Base;

namespace vMenu.Enhanced.BrokenNatives.Server;

/// <summary>
/// Natives that are broken in the API get fixed here.
/// </summary>
public static class NativeFixer
{
    internal static NativeApi nativeApi = BaseEntrypoint.NativeApi;

    public static bool SaveResourceFile(string resource, string file, string buffer)
    {
        nativeApi.ResetContext();
        nativeApi.PushArg(resource);
        nativeApi.PushArg(file);
        nativeApi.PushArg(buffer);
        nativeApi.PushArg(-1);
        nativeApi.Invoke(2694741627uL, "SaveResourceFile");
        return nativeApi.GetResBool(0);
    }
}
