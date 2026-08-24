using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles;

// GetDisplayNameFromVehicleModel is not a spawn name. It returns the model's game name field, which
// is a fixed size buffer, so a longer name comes back cut short: polgreenwood reads as polgreenw, and
// hashing that names a model the game has never heard of. The hash is the only dependable identity a
// vehicle has, so anything that needs the name asks for it by hash here.
public static class VehicleModelNames
{
    private static Dictionary<uint, string>? _byHash;

    // The spawn name for a hash, or the fallback when this game has no such model.
    public static string Resolve(uint hash, string fallback) =>
        Index().TryGetValue(hash, out var name) ? name : fallback;

    public static string Resolve(uint hash) => Resolve(hash, Native.GetDisplayNameFromVehicleModel(hash));

    private static Dictionary<uint, string> Index()
    {
        if (_byHash is not null)
        {
            return _byHash;
        }

        var models = NativeFixer.GetAllVehicleModels();

        // Nothing to build from yet, and caching that would keep every later lookup empty too.
        if (models is null)
        {
            return [];
        }

        var index = new Dictionary<uint, string>();

        foreach (var model in models)
        {
            var name = model?.Trim();

            if (!string.IsNullOrEmpty(name))
            {
                index[API.Hash(name)] = name;
            }
        }

        _byHash = index;

        return index;
    }
}
