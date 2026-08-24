using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// The game has a different animation for every riding posture, and no native that says which one a
// bike uses, so the answer is a list of model names. The tables below are legacy's, moved out of the
// handler that used them: it built six lists and hashed forty model names on every single press.
public static class VisorAnimations
{
    public const string OnFoot = "anim@mp_helmets@on_foot";

    private const string OnSportsBike = "anim@mp_helmets@on_bike@sports";

    private const string OnChopper = "anim@mp_helmets@on_bike@chopper";

    private const string OnDirtBike = "anim@mp_helmets@on_bike@dirt";

    private const string OnScooter = "anim@mp_helmets@on_bike@scooter";

    private const string OnPoliceBike = "anim@mp_helmets@on_bike@policeb";

    private const string OnQuad = "anim@mp_helmets@on_bike@quad";

    private const int BikeClass = 8;

    private const int QuadClass = 7;

    private const int BicycleClass = 13;

    private static readonly uint[] SportsBikes = Hashes(
        "akuma", "bati", "bati2", "carbonrs", "defiler", "diablous2", "double", "fcr", "fcr2",
        "hakuchou", "hakuchou2", "lectro", "nemesis", "oppressor", "oppressor2", "pcj", "ruffian",
        "shotaro", "vader", "vortex");

    private static readonly uint[] Choppers = Hashes("sanctus", "zombiea", "zombieb");

    private static readonly uint[] DirtBikes = Hashes(
        "bf400", "enduro", "manchez", "sanchez", "sanchez2", "esskey");

    private static readonly uint[] Scooters = Hashes(
        "faggio", "faggio2", "faggio3", "cliffhanger", "bagger");

    private static readonly uint[] PoliceBikes = Hashes(
        "avarus", "chimera", "policeb", "sovereign", "hexer", "innovation", "nightblade", "ratbike",
        "daemon", "daemon2", "diablous", "gargoyle", "thrust", "vindicator", "wolfsbane");

    // A handful of helmets whose two versions were authored in the opposite order from all the others.
    private static readonly int[] InvertedFemale = [66, 81];

    private static readonly int[] InvertedMale = [67, 82];

    // Headgear the game animates as goggles being pushed up rather than a visor.
    private static readonly int[] GogglesFemale = [115, 116, 117, 118];

    private static readonly int[] GogglesMale = [116, 117, 118, 119];

    private static readonly uint FreemodeFemale = API.Hash("mp_f_freemode_01");

    // Anything that is not something you sit astride uses the on foot animation, which looks close
    // enough sitting in a car and is what legacy settled on too.
    public static string ForVehicle(int vehicle)
    {
        var model = (uint)Native.GetEntityModel(vehicle);

        return Native.GetVehicleClass(vehicle) switch
        {
            QuadClass => OnQuad,
            BicycleClass => OnScooter,
            BikeClass => ForBike(model),
            _ => OnFoot,
        };
    }

    public static bool IsInverted(uint pedModel, int drawable) =>
        Contains(pedModel == FreemodeFemale ? InvertedFemale : InvertedMale, drawable);

    public static bool IsGoggles(uint pedModel, int drawable) =>
        Contains(pedModel == FreemodeFemale ? GogglesFemale : GogglesMale, drawable);

    private static string ForBike(uint model)
    {
        if (Contains(PoliceBikes, model)) { return OnPoliceBike; }

        if (Contains(Choppers, model)) { return OnChopper; }

        if (Contains(DirtBikes, model)) { return OnDirtBike; }

        if (Contains(Scooters, model)) { return OnScooter; }

        // Sports is the fallback as well as a list of its own, because it is the most upright of the riding
        // postures and looks least wrong on a bike nobody thought to categorise.
        return OnSportsBike;
    }

    private static uint[] Hashes(params string[] models)
    {
        var hashes = new uint[models.Length];

        for (var index = 0; index < models.Length; index++)
        {
            hashes[index] = API.Hash(models[index]);
        }

        return hashes;
    }

    // By hand rather than Array.IndexOf or Contains, which reach for EqualityComparer<T>.Default and the
    // client sandbox refuses to load it.
    private static bool Contains(uint[] values, uint value)
    {
        foreach (var candidate in values)
        {
            if (candidate == value)
            {
                return true;
            }
        }

        return false;
    }

    private static bool Contains(int[] values, int value)
    {
        foreach (var candidate in values)
        {
            if (candidate == value)
            {
                return true;
            }
        }

        return false;
    }
}
