using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Serialization.Server;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Permissions.Server;

public static partial class WeaponCatalog
{
    private static string _payload = "[]";

    public static void LoadAndRegister()
    {
        Load(API.LoadResourceFile(API.GetCurrentResourceName(), ConfigFile));

        _payload = ServerJson.Serialize(Categories);
    }

    public static void RegisterEventHandlers() =>
        Script.EventHandlers[WeaponEvents.Request] += new Action<Player>(OnRequested);

    private static void OnRequested([FromSource] Player source) =>
        Script.TriggerClientEvent(source.ServerId(), WeaponEvents.Set, _payload, WeaponComponentCatalog.Payload);
}
