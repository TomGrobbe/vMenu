using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Weapons;

public static class GameLoadouts
{
    private const string Unarmed = "weapon_unarmed";

    public static IReadOnlyList<(MenuText Heading, (string Name, MenuText Label)[] Loadouts)> Groups { get; } =
    [
        (Plain(Loc.WeaponOptions.GameLoadoutServices),
        [
            ("LOADOUT_COP", Plain(Loc.WeaponOptions.GameLoadoutCop)),
            ("LOADOUT_COP_L1", Level(Loc.WeaponOptions.GameLoadoutCop, 1)),
            ("LOADOUT_COP_L2", Level(Loc.WeaponOptions.GameLoadoutCop, 2)),
            ("LOADOUT_COP_L3", Level(Loc.WeaponOptions.GameLoadoutCop, 3)),
            ("LOADOUT_COP_SHOTGUN", Plain(Loc.WeaponOptions.GameLoadoutCopShotgun)),
            ("LOADOUT_COP_HELI", Plain(Loc.WeaponOptions.GameLoadoutCopHeli)),
            ("LOADOUT_COP_BOAT", Plain(Loc.WeaponOptions.GameLoadoutCopBoat)),
            ("LOADOUT_SWAT", Plain(Loc.WeaponOptions.GameLoadoutSwat)),
            ("LOADOUT_SWAT_NO_LASER", Plain(Loc.WeaponOptions.GameLoadoutSwatNoLaser)),
            ("LOADOUT_ARMY", Plain(Loc.WeaponOptions.GameLoadoutArmy)),
            ("LOADOUT_FIREMAN", Plain(Loc.WeaponOptions.GameLoadoutFireman)),
            ("LOADOUT_POLICE_GUARD", Plain(Loc.WeaponOptions.GameLoadoutPoliceGuard)),
            ("LOADOUT_SECUR", Plain(Loc.WeaponOptions.GameLoadoutSecurity)),
            ("LOADOUT_GUARD", Plain(Loc.WeaponOptions.GameLoadoutGuard)),
        ]),
        (Plain(Loc.WeaponOptions.GameLoadoutGangs),
        [
            ("LOADOUT_LOST", Plain(Loc.WeaponOptions.GameLoadoutLost)),
            ("LOADOUT_LOST_L1", Level(Loc.WeaponOptions.GameLoadoutLost, 1)),
            ("LOADOUT_LOST_L2", Level(Loc.WeaponOptions.GameLoadoutLost, 2)),
            ("LOADOUT_LOST_L3", Level(Loc.WeaponOptions.GameLoadoutLost, 3)),
            ("LOADOUT_MEXICAN", Plain(Loc.WeaponOptions.GameLoadoutMexican)),
            ("LOADOUT_MEXICAN_L1", Level(Loc.WeaponOptions.GameLoadoutMexican, 1)),
            ("LOADOUT_MEXICAN_L2", Level(Loc.WeaponOptions.GameLoadoutMexican, 2)),
            ("LOADOUT_MEXICAN_L3", Level(Loc.WeaponOptions.GameLoadoutMexican, 3)),
            ("LOADOUT_FAMILY", Plain(Loc.WeaponOptions.GameLoadoutFamily)),
            ("LOADOUT_ASIAN", Plain(Loc.WeaponOptions.GameLoadoutAsian)),
            ("LOADOUT_HILLBILLY", Plain(Loc.WeaponOptions.GameLoadoutHillbilly)),
            ("LOADOUT_CULT", Plain(Loc.WeaponOptions.GameLoadoutCult)),
        ]),
        (Plain(Loc.WeaponOptions.GameLoadoutCheats),
        [
            ("LOADOUT_CHEAT_0", Cheat(1)),
            ("LOADOUT_CHEAT_1", Cheat(2)),
        ]),
        (Plain(Loc.WeaponOptions.GameLoadoutAnimals),
        [
            ("LOADOUT_ANIMAL", Plain(Loc.WeaponOptions.GameLoadoutAnimal)),
            ("LOADOUT_COUGAR", Plain(Loc.WeaponOptions.GameLoadoutCougar)),
            ("LOADOUT_ANIMAL_RETRIEVER", Plain(Loc.WeaponOptions.GameLoadoutRetriever)),
            ("LOADOUT_SMALL_DOG", Plain(Loc.WeaponOptions.GameLoadoutSmallDog)),
            ("LOADOUT_CAT", Plain(Loc.WeaponOptions.GameLoadoutCat)),
            ("LOADOUT_COYOTE", Plain(Loc.WeaponOptions.GameLoadoutCoyote)),
            ("LOADOUT_BOAR", Plain(Loc.WeaponOptions.GameLoadoutBoar)),
            ("LOADOUT_PIG", Plain(Loc.WeaponOptions.GameLoadoutPig)),
            ("LOADOUT_COW", Plain(Loc.WeaponOptions.GameLoadoutCow)),
            ("LOADOUT_DEER", Plain(Loc.WeaponOptions.GameLoadoutDeer)),
            ("LOADOUT_RABBIT", Plain(Loc.WeaponOptions.GameLoadoutRabbit)),
            ("LOADOUT_HEN", Plain(Loc.WeaponOptions.GameLoadoutHen)),
            ("LOADOUT_TIGER_SHARK", Plain(Loc.WeaponOptions.GameLoadoutTigerShark)),
            ("LOADOUT_HAMMERHEAD_SHARK", Plain(Loc.WeaponOptions.GameLoadoutHammerheadShark)),
            ("LOADOUT_KILLER_WHALE", Plain(Loc.WeaponOptions.GameLoadoutKillerWhale)),
        ]),
    ];

    // Animal attacks are not in weapons.json, so they are looked up by name to be counted.
    private static readonly string[] AnimalWeapons =
    [
        "weapon_animal", "weapon_cougar", "weapon_animal_retriever", "weapon_small_dog", "weapon_cat", "weapon_coyote",
        "weapon_boar", "weapon_pig", "weapon_cow", "weapon_deer", "weapon_rabbit", "weapon_hen", "weapon_tiger_shark",
        "weapon_hammerhead_shark", "weapon_killer_whale",
    ];

    // The native gives the whole loadout in one go, so weapons this player may not use are taken away afterwards.
    public static (int Given, int Removed) Apply(string name)
    {
        var ped = API.PlayerPedId();
        var unarmed = Hashing.Joaat(Unarmed);
        var allowed = new HashSet<string>(WeaponInventory.Allowed().Select(weapon => weapon.SpawnName), StringComparer.OrdinalIgnoreCase);
        var given = 0;
        var removed = 0;

        // A loadout the game does not know leaves the old weapons alone, so they go first to keep the result honest.
        WeaponInventory.RemoveAll();

        API.GiveLoadoutToPed(ped, (int)Hashing.Joaat(name));

        foreach (var (weapon, _) in WeaponInventory.Listed())
        {
            var hash = Hashing.Joaat(weapon);

            if (hash == unarmed || !WeaponInventory.Has(hash))
            {
                continue;
            }

            if (allowed.Contains(weapon))
            {
                WeaponInventory.Refill(hash);

                given++;

                continue;
            }

            API.RemoveWeaponFromPed(ped, hash);

            removed++;
        }

        var animal = unarmed;

        foreach (var weapon in AnimalWeapons)
        {
            var hash = Hashing.Joaat(weapon);

            if (WeaponInventory.Has(hash))
            {
                animal = hash;

                given++;
            }
        }

        // An animal attack is the whole point of those loadouts, so it stays in hand instead of going unarmed.
        API.SetCurrentPedWeapon(ped, animal, true);

        return (given, removed);
    }

    private static MenuText Plain(string key) => MenuText.Key(key);

    private static MenuText Level(string key, int level) =>
        MenuText.Key(Loc.WeaponOptions.GameLoadoutLevel, ("name", MenuText.Key(key)), ("level", MenuText.Literal(level.ToString(CultureInfo.InvariantCulture))));

    private static MenuText Cheat(int number) =>
        MenuText.Key(Loc.WeaponOptions.GameLoadoutCheat, ("number", MenuText.Literal(number.ToString(CultureInfo.InvariantCulture))));
}
