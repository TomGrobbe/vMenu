using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Permissions.Server;

public static partial class WeaponCatalog
{
    private static string _payload = "[]";

    public static void LoadAndRegister()
    {
        Load(Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile));

        _payload = ServerJson.Serialize(Categories);
    }

    public static void RegisterEventHandlers() =>
        API.OnNetEvent(WeaponEvents.Request, new Action<Player>(OnRequested), false);

    private static void OnRequested([FromSource] Player source) =>
        API.EmitClient(source.Handle, WeaponEvents.Set, _payload, WeaponComponentCatalog.Payload);
}
