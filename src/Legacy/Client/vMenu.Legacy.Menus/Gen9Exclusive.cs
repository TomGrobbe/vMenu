using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Logging;
using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus;

// Legacy runs the old game, which lists the clothing and upgrades made for the newer one but refuses to use them.
internal static class Gen9Exclusive
{
    private static bool _missing;

    // FiveM registers these two on every game build, they only answer false below 2699.
    internal static bool Component(int ped, int slot, string? collection, int drawable) =>
        drawable >= 0
        && (collection is string name
            ? API.IsPedCollectionComponentVariationGen9Exclusive(ped, slot, name, drawable)
            : API.IsPedComponentVariationGen9Exclusive(ped, slot, drawable));

    internal static bool VehicleMod(int handle, int slot, int index)
    {
        if (_missing || index < 0)
        {
            return false;
        }

        try
        {
            return API.IsVehicleModGen9Exclusive(handle, slot, index);
        }
        catch (Exception exception)
        {
            // Older game builds do not have this native at all.
            _missing = true;

            Log.Warning("[Gen9] Checking for newer game upgrades is not possible on this game build: " + exception.Message);

            return false;
        }
    }
}
