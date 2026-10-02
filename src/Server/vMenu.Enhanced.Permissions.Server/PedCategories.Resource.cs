using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Permissions.Server;

public static partial class PedCategories
{
    // The list as the clients receive it, built once when the file is read.
    private static string _payload = "[]";

    public static void LoadAndRegister()
    {
        Load(Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile));

        _payload = ServerJson.Serialize(Categories);
    }

    // Call once the permission registry is ready.
    public static void RegisterEventHandlers() =>
        API.OnNetEvent(PedModelEvents.Request, new Action<Player>(OnRequested), false);

    // A named method, not a lambda: the binder reads FromSourceAttribute off the delegate's MethodInfo.
    private static void OnRequested([FromSource] Player source) =>
        API.EmitClient(source.Handle, PedModelEvents.Set, _payload);
}
