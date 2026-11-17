namespace vMenu.Enhanced.Data.VehicleData;

public enum VehicleMineGroup
{
    Proximity,
    Arena,
    RcBandito,
    Jb7002,
    Fixer,
}

public static class VehicleMineProfiles
{
    public const int MineModSlot = 9;

    public const int SawBladeModSlot = 44;

    public const string DetonatedEvent = "vMenu.Enhanced:VehicleMines:Detonated";

    public const float DetonatedClearRadius = 1.5f;

    private const int VehicleMineExplosion = 44;

    private const int FirstArenaMineExplosion = 64;

    private const int LastArenaMineExplosion = 68;

    private static readonly uint Mine = VehicleBlipSprites.Hash("vehicle_weapon_mine");

    private static readonly uint[] ArenaMines =
    [
        VehicleBlipSprites.Hash("vehicle_weapon_mine_kinetic"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_spike"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_emp"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_slick"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_tar"),
    ];

    private static readonly uint[] RcBanditoMines =
    [
        VehicleBlipSprites.Hash("vehicle_weapon_mine_kinetic_rc"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_emp_rc"),
    ];

    private static readonly uint[] Jb7002Mines =
    [
        VehicleBlipSprites.Hash("vehicle_weapon_mine_spike"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_slick"),
    ];

    private static readonly uint SlickMine = VehicleBlipSprites.Hash("vehicle_weapon_mine_slick");

    public static readonly uint[] AllMineWeapons =
    [
        VehicleBlipSprites.Hash("vehicle_weapon_mine"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_kinetic"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_spike"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_emp"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_slick"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_tar"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_kinetic_rc"),
        VehicleBlipSprites.Hash("vehicle_weapon_mine_emp_rc"),
    ];

    public static readonly uint[] AllMineModels =
    [
        VehicleBlipSprites.Hash("w_ex_vehiclemine"),
        VehicleBlipSprites.Hash("w_ex_arena_landmine_01b"),
    ];

    public static readonly uint DetonateShotWeapon = VehicleBlipSprites.Hash("weapon_pistol");

    private static readonly uint MineModel = VehicleBlipSprites.Hash("w_ex_vehiclemine");

    private static readonly uint RcBanditoMineModel = VehicleBlipSprites.Hash("w_ex_arena_landmine_01b");

    private static readonly uint Speedo4 = VehicleBlipSprites.Hash("speedo4");

    private static readonly Dictionary<uint, VehicleMineGroup> ByModel = Build();

    private static readonly HashSet<uint> SawBladeModels =
    [
        VehicleBlipSprites.Hash("zr380"),
        VehicleBlipSprites.Hash("zr3802"),
        VehicleBlipSprites.Hash("zr3803"),
        VehicleBlipSprites.Hash("imperator"),
        VehicleBlipSprites.Hash("imperator2"),
        VehicleBlipSprites.Hash("imperator3"),
        VehicleBlipSprites.Hash("slamvan4"),
        VehicleBlipSprites.Hash("slamvan5"),
        VehicleBlipSprites.Hash("slamvan6"),
    ];

    public static VehicleMineGroup? ForModel(uint model) =>
        ByModel.TryGetValue(model, out var group) ? group : null;

    public static uint Weapon(VehicleMineGroup group, int mod) => group switch
    {
        VehicleMineGroup.Fixer => SlickMine,
        VehicleMineGroup.Arena => Pick(ArenaMines, mod),
        VehicleMineGroup.RcBandito => Pick(RcBanditoMines, mod),
        VehicleMineGroup.Jb7002 => Pick(Jb7002Mines, mod),
        _ => Mine,
    };

    public static uint Model(VehicleMineGroup group) =>
        group == VehicleMineGroup.RcBandito ? RcBanditoMineModel : MineModel;

    public static bool UsesHorn(VehicleMineGroup group) =>
        group == VehicleMineGroup.Proximity || group == VehicleMineGroup.Fixer;

    public static bool HasSawBladeSlot(uint model) => SawBladeModels.Contains(model);

    public static bool DropsFurtherBack(uint model) => model == Speedo4;

    public static bool IsMineExplosion(int type) =>
        type == VehicleMineExplosion || (type >= FirstArenaMineExplosion && type <= LastArenaMineExplosion);

    private static uint Pick(uint[] mines, int mod) => mod >= 0 && mod < mines.Length ? mines[mod] : Mine;

    private static Dictionary<uint, VehicleMineGroup> Build()
    {
        var groups = new Dictionary<uint, VehicleMineGroup>();

        void Add(VehicleMineGroup group, params string[] models)
        {
            foreach (var model in models)
            {
                groups[VehicleBlipSprites.Hash(model)] = group;
            }
        }

        Add(
            VehicleMineGroup.Proximity,
            "apc", "dune3", "halftrack", "tampa3", "insurgent3", "technical3", "khanjali", "speedo4", "mule4", "pounder2");

        Add(
            VehicleMineGroup.Arena,
            "issi4", "issi5", "issi6",
            "scarab", "scarab2", "scarab3",
            "cerberus", "cerberus2", "cerberus3",
            "impaler2", "impaler3", "impaler4",
            "zr380", "zr3802", "zr3803",
            "dominator4", "dominator5", "dominator6",
            "imperator", "imperator2", "imperator3",
            "bruiser", "bruiser2", "bruiser3",
            "slamvan4", "slamvan5", "slamvan6",
            "monster3", "monster4", "monster5",
            "brutus", "brutus2", "brutus3");

        Add(VehicleMineGroup.RcBandito, "rcbandito");

        Add(VehicleMineGroup.Jb7002, "jb7002");

        Add(
            VehicleMineGroup.Fixer,
            "champion", "deity", "granger2", "buffalo4", "patriot3", "jubilee", "greenwood", "omnisegt");

        return groups;
    }
}
