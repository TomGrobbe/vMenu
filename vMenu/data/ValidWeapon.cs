using System.Collections.Generic;

using CitizenFX.Core;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.data
{
    public struct ValidWeapon
    {
        public uint Hash;
        public string Name;
        public Dictionary<string, uint> Components;
        public Permission Perm;
        public string SpawnName;
        public readonly int GetMaxAmmo
        {
            get
            {
                var ammo = 0; GetMaxAmmo(Game.PlayerPed.Handle, Hash, ref ammo); return ammo;
            }
        }
        public int CurrentAmmo;
        public int CurrentTint;
        public readonly float Accuracy
        {
            get
            {
                var stats = new Game.WeaponHudStats(); Game.GetWeaponHudStats(Hash, ref stats); return stats.hudAccuracy;
            }
        }
        public readonly float Damage
        {
            get
            {
                var stats = new Game.WeaponHudStats(); Game.GetWeaponHudStats(Hash, ref stats); return stats.hudDamage;
            }
        }
        public readonly float Range
        {
            get
            {
                var stats = new Game.WeaponHudStats(); Game.GetWeaponHudStats(Hash, ref stats); return stats.hudRange;
            }
        }
        public readonly float Speed
        {
            get
            {
                var stats = new Game.WeaponHudStats(); Game.GetWeaponHudStats(Hash, ref stats); return stats.hudSpeed;
            }
        }
    }

    public static class ValidWeapons
    {
        private static readonly List<ValidWeapon> _weaponsList = new();

        public static List<ValidWeapon> WeaponList
        {
            get
            {
                if (_weaponsList.Count == weaponNames.Count - 1)
                {
                    return _weaponsList;
                }
                CreateWeaponsList();
                return _weaponsList;
            }
        }


        private static Dictionary<string, string> _components = new();
        public static Dictionary<string, string> GetWeaponComponents()
        {
            if (_components.Count == 0)
            {
                var addons = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
                _components = weaponComponentNames;
                try
                {
                    var addonsFile = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(addons);
                    if (addonsFile.ContainsKey("weapon_components"))
                    {
                        foreach (var item in addonsFile["weapon_components"])
                        {
                            var name = item;
                            var displayName = GetLabelText(name) ?? name;
                            var unused = 0;
                            if (GetWeaponComponentHudStats((uint)GetHashKey(name), ref unused))
                            {
                                _components.Add(name, displayName);
                            }
                        }
                    }
                }
                catch
                {
                    Log("[WARNING] The addons.json contains invalid JSON.");
                }
            }
            return _components;
        }

        private static void CreateWeaponsList()
        {
            _weaponsList.Clear();
            foreach (var weapon in weaponNames)
            {
                var realName = weapon.Key;
                var localizedName = weapon.Value;
                if (realName != "weapon_unarmed")
                {
                    var hash = (uint)GetHashKey(weapon.Key);
                    var componentHashes = new Dictionary<string, uint>();
                    foreach (var comp in GetWeaponComponents().Keys)
                    {
                        if (DoesWeaponTakeWeaponComponent(hash, (uint)GetHashKey(comp)))
                        {
                            if (!componentHashes.ContainsKey(GetWeaponComponents()[comp]))
                            {
                                componentHashes.Add(GetWeaponComponents()[comp], (uint)GetHashKey(comp));
                            }
                        }
                    }
                    var vw = new ValidWeapon()
                    {
                        Hash = hash,
                        SpawnName = realName,
                        Name = localizedName,
                        Components = componentHashes,
                        Perm = weaponPermissions[realName]
                    };
                    if (!_weaponsList.Contains(vw))
                    {
                        _weaponsList.Add(vw);
                    }
                }
            }
        }

        #region Weapon names, hashes and localized names (+ all components & tints).
        #region weapon descriptions & names
        public static readonly Dictionary<string, string> weaponDescriptions = new()
        {
            { "weapon_advancedrifle", GetLabelText("WTD_RIFLE_ADV") },
            { "weapon_appistol", GetLabelText("WTD_PIST_AP") },
            { "weapon_assaultrifle", GetLabelText("WTD_RIFLE_ASL") },
            { "weapon_assaultrifle_mk2", GetLabelText("WTD_RIFLE_ASL2") },
            { "weapon_assaultshotgun", GetLabelText("WTD_SG_ASL") },
            { "weapon_assaultsmg", GetLabelText("WTD_SMG_ASL") },
            { "weapon_autoshotgun", GetLabelText("WTD_AUTOSHGN") },
            { "weapon_bat", GetLabelText("WTD_BAT") },
            { "weapon_ball", GetLabelText("WT_BALL") + "."},
            { "weapon_battleaxe", GetLabelText("WTD_BATTLEAXE") },
            { "weapon_bottle", GetLabelText("WTD_BOTTLE") },
            { "weapon_bullpuprifle", GetLabelText("WTD_BULLRIFLE") },
            { "weapon_bullpuprifle_mk2", GetLabelText("WTD_BULLRIFLE2") },
            { "weapon_bullpupshotgun", GetLabelText("WTD_SG_BLP") },
            { "weapon_bzgas", GetLabelText("WT_BZGAS") + "."},
            { "weapon_carbinerifle", GetLabelText("WTD_RIFLE_CBN") },
            { "weapon_carbinerifle_mk2", GetLabelText("WTD_RIFLE_CBN2") },
            { "weapon_combatmg", GetLabelText("WTD_MG_CBT") },
            { "weapon_combatmg_mk2", GetLabelText("WTD_MG_CBT2") },
            { "weapon_combatpdw", GetLabelText("WTD_COMBATPDW") },
            { "weapon_combatpistol", GetLabelText("WTD_PIST_CBT") },
            { "weapon_compactlauncher", GetLabelText("WTD_CMPGL") },
            { "weapon_compactrifle", GetLabelText("WTD_CMPRIFLE") + "~n~" }, // this one is just too short to line-break properly, so we force another empty line
            { "weapon_crowbar", GetLabelText("WTD_CROWBAR") },
            { "weapon_dagger", GetLabelText("WTD_DAGGER") },
            { "weapon_dbshotgun", GetLabelText("WTD_DBSHGN") },
            { "weapon_doubleaction", GetLabelText("WTD_REV_DA") },
            { "weapon_fireextinguisher", GetLabelText("WT_FIRE") + "." },
            { "weapon_firework", GetLabelText("WT_FWRKLNCHR") + "." },
            { "weapon_flare", GetLabelText("WT_FLARE") + "." },
            { "weapon_flaregun", GetLabelText("WTD_FLAREGUN") },
            { "weapon_flashlight", GetLabelText("WTD_FLASHLIGHT") },
            { "weapon_golfclub", GetLabelText("WTD_GOLFCLUB") },
            { "weapon_grenade", GetLabelText("WTD_GNADE") },
            { "weapon_grenadelauncher", GetLabelText("WTD_GL") },
            { "weapon_gusenberg", GetLabelText("WTD2_GUSNBRG") },
            { "weapon_hammer", GetLabelText("WTD_HAMMER") },
            { "weapon_hatchet", GetLabelText("WTD_HATCHET") },
            { "weapon_heavypistol", GetLabelText("WTD2_HVYPISTOL") },
            { "weapon_heavyshotgun", GetLabelText("WTD2_HVYSHGN") },
            { "weapon_heavysniper", GetLabelText("WTD_SNIP_HVY") },
            { "weapon_heavysniper_mk2", GetLabelText("WTD_SNIP_HVY2") },
            { "weapon_hominglauncher", GetLabelText("WTD_HOMLNCH") },
            { "weapon_knife", GetLabelText("WTD_KNIFE") },
            { "weapon_knuckle", GetLabelText("WTD_KNUCKLE") },
            { "weapon_machete", GetLabelText("WTD_MACHETE") },
            { "weapon_machinepistol", GetLabelText("WTD_MCHPIST") },
            { "weapon_marksmanpistol", GetLabelText("WTD_MKPISTOL") },
            { "weapon_marksmanrifle", GetLabelText("WTD_MKRIFLE") },
            { "weapon_marksmanrifle_mk2", GetLabelText("WTD_MKRIFLE2") },
            { "weapon_mg", GetLabelText("WTD_MG") },
            { "weapon_microsmg", GetLabelText("WTD_SMG_MCR") },
            { "weapon_minigun", GetLabelText("WTD_MINIGUN") },
            { "weapon_minismg", GetLabelText("WTD_MINISMG") },
            { "weapon_molotov", GetLabelText("WTD_MOLOTOV") },
            { "weapon_musket", GetLabelText("WTD_MUSKET") },
            { "weapon_nightstick", GetLabelText("WTD_NGTSTK") },
            { "weapon_petrolcan", GetLabelText("WTD_PETROL") },
            { "weapon_pipebomb", GetLabelText("WTD_PIPEBOMB") },
            { "weapon_pistol", GetLabelText("WT_PIST_DESC") },
            { "weapon_pistol50", GetLabelText("WTD_PIST_50") },
            { "weapon_pistol_mk2", GetLabelText("WTD_PIST2") },
            { "weapon_poolcue", GetLabelText("WTD_POOLCUE") },
            { "weapon_proxmine", GetLabelText("WTD_PRXMINE") },
            { "weapon_pumpshotgun", GetLabelText("WTD_SG_PMP") },
            { "weapon_pumpshotgun_mk2", GetLabelText("WTD_SG_PMP2") },
            { "weapon_railgun", GetLabelText("WTD_RAILGUN") },
            { "weapon_revolver", GetLabelText("WTD_REVOLVER") },
            { "weapon_revolver_mk2", GetLabelText("WTD_REVOLVER2") },
            { "weapon_rpg", GetLabelText("WTD_RPG") },
            { "weapon_sawnoffshotgun", GetLabelText("WTD_SG_SOF") },
            { "weapon_smg", GetLabelText("WTD_SMG") },
            { "weapon_smg_mk2", GetLabelText("WTD_SMG2") },
            { "weapon_smokegrenade", GetLabelText("WTD_GNADE_SMK") },
            { "weapon_sniperrifle", GetLabelText("WTD_SNIP_RIF") },
            { "weapon_snowball", GetLabelText("WT_SNWBALL") + "." },
            { "weapon_snspistol", GetLabelText("WTD_SNSPISTOL") },
            { "weapon_snspistol_mk2", GetLabelText("WTD_SNSPISTOL2") },
            { "weapon_specialcarbine", GetLabelText("WTD_SPCARBINE") },
            { "weapon_specialcarbine_mk2", GetLabelText("WTD_SPCARBINE2") },
            { "weapon_stickybomb", GetLabelText("WTD_GNADE_STK") },
            { "weapon_stungun", GetLabelText("WTD_STUN") },
            { "weapon_switchblade", GetLabelText("WTD_SWBLADE") },
            { "weapon_unarmed", GetLabelText("WTD_UNARMED") },
            { "weapon_vintagepistol", GetLabelText("WTD_VPISTOL") },
            { "weapon_wrench", GetLabelText("WTD_WRENCH") },
            { "weapon_raypistol", GetLabelText("WTD_RAYPISTOL") },
            { "weapon_raycarbine", GetLabelText("WTD_RAYCARBINE") },
            { "weapon_rayminigun", GetLabelText("WTD_RAYMINIGUN") },
            { "weapon_stone_hatchet", GetLabelText("WTD_SHATCHET") },
            // MPHEIST3 DLC (v 1868)
            { "weapon_ceramicpistol", GetLabelText("WTD_CERPST") },
            { "weapon_navyrevolver", GetLabelText("WTD_REV_NV") },
            { "weapon_hazardcan", GetLabelText("WTD_HAZARDCAN") },
            // MPHEIST4 DLC (v 2189)
            { "weapon_gadgetpistol", GetLabelText("WTD_GDGTPST") },
            { "weapon_militaryrifle", GetLabelText("WTD_MLTRYRFL") },
            { "weapon_combatshotgun", GetLabelText("WTD_CMBSHGN") },
            // MPSECURITY DLC (v 2545)
            { "weapon_emplauncher", GetLabelText("WTD_EMPL") },
            { "weapon_heavyrifle", GetLabelText("WTD_HEAVYRIFLE") },
            { "weapon_fertilizercan", GetLabelText("WTD_FERTILIZERCAN") },
            { "weapon_stungun_mp", GetLabelText("WTD_STNGUNMP") },
            // MPSUM2 DLC (V 2699)
            { "weapon_tacticalrifle", GetLabelText("WTD_TACRIFLE") },
            { "weapon_precisionrifle", GetLabelText("WTD_PRCSRIFLE") },
            // MPCHRISTMAS3 DLC (V 2802)
            { "weapon_pistolxm3", GetLabelText("WTD_PISTOLXM3") },
            { "weapon_candycane", GetLabelText("WTD_CANDYCANE") },
            { "weapon_railgunxm3", GetLabelText("WTD_RAILGUNXM3") },
            // MP2023_01 DLC (V 2944)
            { "weapon_tecpistol", GetLabelText("WTD_TECPISTOL") },
        };

        public static readonly Dictionary<string, string> weaponNames = new()
        {
            { "weapon_advancedrifle", GetLabelText("WT_RIFLE_ADV") },
            { "weapon_appistol", GetLabelText("WT_PIST_AP") },
            { "weapon_assaultrifle", GetLabelText("WT_RIFLE_ASL") },
            { "weapon_assaultrifle_mk2", GetLabelText("WT_RIFLE_ASL2") },
            { "weapon_assaultshotgun", GetLabelText("WT_SG_ASL") },
            { "weapon_assaultsmg", GetLabelText("WT_SMG_ASL") },
            { "weapon_autoshotgun", GetLabelText("WT_AUTOSHGN") },
            { "weapon_bat", GetLabelText("WT_BAT") },
            { "weapon_ball", GetLabelText("WT_BALL") },
            { "weapon_battleaxe", GetLabelText("WT_BATTLEAXE") },
            { "weapon_bottle", GetLabelText("WT_BOTTLE") },
            { "weapon_bullpuprifle", GetLabelText("WT_BULLRIFLE") },
            { "weapon_bullpuprifle_mk2", GetLabelText("WT_BULLRIFLE2") },
            { "weapon_bullpupshotgun", GetLabelText("WT_SG_BLP") },
            { "weapon_bzgas", GetLabelText("WT_BZGAS") },
            { "weapon_carbinerifle", GetLabelText("WT_RIFLE_CBN") },
            { "weapon_carbinerifle_mk2", GetLabelText("WT_RIFLE_CBN2") },
            { "weapon_combatmg", GetLabelText("WT_MG_CBT") },
            { "weapon_combatmg_mk2", GetLabelText("WT_MG_CBT2") },
            { "weapon_combatpdw", GetLabelText("WT_COMBATPDW") },
            { "weapon_combatpistol", GetLabelText("WT_PIST_CBT") },
            { "weapon_compactlauncher", GetLabelText("WT_CMPGL") },
            { "weapon_compactrifle", GetLabelText("WT_CMPRIFLE")},
            { "weapon_crowbar", GetLabelText("WT_CROWBAR") },
            { "weapon_dagger", GetLabelText("WT_DAGGER") },
            { "weapon_dbshotgun", GetLabelText("WT_DBSHGN") },
            { "weapon_doubleaction", GetLabelText("WT_REV_DA") },
            { "weapon_fireextinguisher", GetLabelText("WT_FIRE") },
            { "weapon_firework", GetLabelText("WT_FWRKLNCHR") },
            { "weapon_flare", GetLabelText("WT_FLARE") },
            { "weapon_flaregun", GetLabelText("WT_FLAREGUN") },
            { "weapon_flashlight", GetLabelText("WT_FLASHLIGHT") },
            { "weapon_golfclub", GetLabelText("WT_GOLFCLUB") },
            { "weapon_grenade", GetLabelText("WT_GNADE") },
            { "weapon_grenadelauncher", GetLabelText("WT_GL") },
            { "weapon_gusenberg", GetLabelText("WT_GUSENBERG") },
            { "weapon_hammer", GetLabelText("WT_HAMMER") },
            { "weapon_hatchet", GetLabelText("WT_HATCHET") },
            { "weapon_heavypistol", GetLabelText("WT_HEAVYPSTL") },
            { "weapon_heavyshotgun", GetLabelText("WT_HVYSHOT") },
            { "weapon_heavysniper", GetLabelText("WT_SNIP_HVY") },
            { "weapon_heavysniper_mk2", GetLabelText("WT_SNIP_HVY2") },
            { "weapon_hominglauncher", GetLabelText("WT_HOMLNCH") },
            { "weapon_knife", GetLabelText("WT_KNIFE") },
            { "weapon_knuckle", GetLabelText("WT_KNUCKLE") },
            { "weapon_machete", GetLabelText("WT_MACHETE") },
            { "weapon_machinepistol", GetLabelText("WT_MCHPIST") },
            { "weapon_marksmanpistol", GetLabelText("WT_MKPISTOL") },
            { "weapon_marksmanrifle", GetLabelText("WT_MKRIFLE") },
            { "weapon_marksmanrifle_mk2", GetLabelText("WT_MKRIFLE2") },
            { "weapon_mg", GetLabelText("WT_MG") },
            { "weapon_microsmg", GetLabelText("WT_SMG_MCR") },
            { "weapon_minigun", GetLabelText("WT_MINIGUN") },
            { "weapon_minismg", GetLabelText("WT_MINISMG") },
            { "weapon_molotov", GetLabelText("WT_MOLOTOV") },
            { "weapon_musket", GetLabelText("WT_MUSKET") },
            { "weapon_nightstick", GetLabelText("WT_NGTSTK") },
            { "weapon_petrolcan", GetLabelText("WT_PETROL") },
            { "weapon_pipebomb", GetLabelText("WT_PIPEBOMB") },
            { "weapon_pistol", GetLabelText("WT_PIST") },
            { "weapon_pistol50", GetLabelText("WT_PIST_50") },
            { "weapon_pistol_mk2", GetLabelText("WT_PIST2") },
            { "weapon_poolcue", GetLabelText("WT_POOLCUE") },
            { "weapon_proxmine", GetLabelText("WT_PRXMINE") },
            { "weapon_pumpshotgun", GetLabelText("WT_SG_PMP") },
            { "weapon_pumpshotgun_mk2", GetLabelText("WT_SG_PMP2") },
            { "weapon_railgun", GetLabelText("WT_RAILGUN") },
            { "weapon_revolver", GetLabelText("WT_REVOLVER") },
            { "weapon_revolver_mk2", GetLabelText("WT_REVOLVER2") },
            { "weapon_rpg", GetLabelText("WT_RPG") },
            { "weapon_sawnoffshotgun", GetLabelText("WT_SG_SOF") },
            { "weapon_smg", GetLabelText("WT_SMG") },
            { "weapon_smg_mk2", GetLabelText("WT_SMG2") },
            { "weapon_smokegrenade", GetLabelText("WT_GNADE_SMK") },
            { "weapon_sniperrifle", GetLabelText("WT_SNIP_RIF") },
            { "weapon_snowball", GetLabelText("WT_SNWBALL") },
            { "weapon_snspistol", GetLabelText("WT_SNSPISTOL") },
            { "weapon_snspistol_mk2", GetLabelText("WT_SNSPISTOL2") },
            { "weapon_specialcarbine", GetLabelText("WT_RIFLE_SCBN") },
            { "weapon_specialcarbine_mk2", GetLabelText("WT_SPCARBINE2") },
            { "weapon_stickybomb", GetLabelText("WT_GNADE_STK") },
            { "weapon_stungun", GetLabelText("WT_STUN") },
            { "weapon_switchblade", GetLabelText("WT_SWBLADE") },
            { "weapon_unarmed", GetLabelText("WT_UNARMED") },
            { "weapon_vintagepistol", GetLabelText("WT_VPISTOL") },
            { "weapon_wrench", GetLabelText("WT_WRENCH") },
            { "weapon_raypistol", GetLabelText("WT_RAYPISTOL") },
            { "weapon_raycarbine", GetLabelText("WT_RAYCARBINE") },
            { "weapon_rayminigun", GetLabelText("WT_RAYMINIGUN") },
            { "weapon_stone_hatchet", GetLabelText("WT_SHATCHET") },
            // MPHEIST3 DLC (v 1868)
            { "weapon_ceramicpistol", GetLabelText("WT_CERPST") },
            { "weapon_navyrevolver", GetLabelText("WT_REV_NV") },
            { "weapon_hazardcan", GetLabelText("WT_HAZARDCAN") },
            // MPHEIST4 DLC (v 2189)
            { "weapon_gadgetpistol", GetLabelText("WT_GDGTPST") },
            { "weapon_militaryrifle", GetLabelText("WT_MLTRYRFL") },
            { "weapon_combatshotgun", GetLabelText("WT_CMBSHGN") },
            // MPSECURITY DLC (v 2545)
            { "weapon_emplauncher", GetLabelText("WT_EMPL") },
            { "weapon_heavyrifle", GetLabelText("WT_HEAVYRIFLE") },
            { "weapon_fertilizercan", GetLabelText("WT_FERTILIZERCAN") },
            { "weapon_stungun_mp", GetLabelText("WT_STNGUNMP") },
            // MPSUM2 DLC (V 2699)
            { "weapon_tacticalrifle", GetLabelText("WT_TACRIFLE") },
            { "weapon_precisionrifle", GetLabelText("WT_PRCSRIFLE") },
            // MPCHRISTMAS3 DLC (V 2802)
            { "weapon_pistolxm3", GetLabelText("WT_PISTOLXM3") },
            { "weapon_candycane", GetLabelText("WT_CANDYCANE") },
            { "weapon_railgunxm3", GetLabelText("WT_RAILGUNXM3") },
            { "weapon_acidpackage", GetLabelText("WT_ACIDPACKAGE") },
            // MP2023_01 DLC (V 2944)
            { "weapon_tecpistol", GetLabelText("WT_TECPISTOL") },
        };
        #endregion

        #region weapon permissions
        public static readonly Dictionary<string, Permission> weaponPermissions = new()
        {
            ["weapon_advancedrifle"] = Permission.WPAdvancedRifle,
            ["weapon_appistol"] = Permission.WPAPPistol,
            ["weapon_assaultrifle"] = Permission.WPAssaultRifle,
            ["weapon_assaultrifle_mk2"] = Permission.WPAssaultRifleMk2,
            ["weapon_assaultshotgun"] = Permission.WPAssaultShotgun,
            ["weapon_assaultsmg"] = Permission.WPAssaultSMG,
            ["weapon_autoshotgun"] = Permission.WPSweeperShotgun,
            ["weapon_ball"] = Permission.WPBall,
            ["weapon_bat"] = Permission.WPBat,
            ["weapon_battleaxe"] = Permission.WPBattleAxe,
            ["weapon_bottle"] = Permission.WPBottle,
            ["weapon_bullpuprifle"] = Permission.WPBullpupRifle,
            ["weapon_bullpuprifle_mk2"] = Permission.WPBullpupRifleMk2,
            ["weapon_bullpupshotgun"] = Permission.WPBullpupShotgun,
            ["weapon_bzgas"] = Permission.WPBZGas,
            ["weapon_carbinerifle"] = Permission.WPCarbineRifle,
            ["weapon_carbinerifle_mk2"] = Permission.WPCarbineRifleMk2,
            ["weapon_combatmg"] = Permission.WPCombatMG,
            ["weapon_combatmg_mk2"] = Permission.WPCombatMGMk2,
            ["weapon_combatpdw"] = Permission.WPCombatPDW,
            ["weapon_combatpistol"] = Permission.WPCombatPistol,
            ["weapon_compactlauncher"] = Permission.WPCompactGrenadeLauncher,
            ["weapon_compactrifle"] = Permission.WPCompactRifle,
            ["weapon_crowbar"] = Permission.WPCrowbar,
            ["weapon_dagger"] = Permission.WPDagger,
            ["weapon_dbshotgun"] = Permission.WPDoubleBarrelShotgun,
            ["weapon_doubleaction"] = Permission.WPDoubleAction,
            ["weapon_fireextinguisher"] = Permission.WPFireExtinguisher,
            ["weapon_firework"] = Permission.WPFirework,
            ["weapon_flare"] = Permission.WPFlare,
            ["weapon_flaregun"] = Permission.WPFlareGun,
            ["weapon_flashlight"] = Permission.WPFlashlight,
            ["weapon_golfclub"] = Permission.WPGolfClub,
            ["weapon_grenade"] = Permission.WPGrenade,
            ["weapon_grenadelauncher"] = Permission.WPGrenadeLauncher,
            ["weapon_gusenberg"] = Permission.WPGusenberg,
            ["weapon_hammer"] = Permission.WPHammer,
            ["weapon_hatchet"] = Permission.WPHatchet,
            ["weapon_heavypistol"] = Permission.WPHeavyPistol,
            ["weapon_heavyshotgun"] = Permission.WPHeavyShotgun,
            ["weapon_heavysniper"] = Permission.WPHeavySniper,
            ["weapon_heavysniper_mk2"] = Permission.WPHeavySniperMk2,
            ["weapon_hominglauncher"] = Permission.WPHomingLauncher,
            ["weapon_knife"] = Permission.WPKnife,
            ["weapon_knuckle"] = Permission.WPKnuckleDuster,
            ["weapon_machete"] = Permission.WPMachete,
            ["weapon_machinepistol"] = Permission.WPMachinePistol,
            ["weapon_marksmanpistol"] = Permission.WPMarksmanPistol,
            ["weapon_marksmanrifle"] = Permission.WPMarksmanRifle,
            ["weapon_marksmanrifle_mk2"] = Permission.WPMarksmanRifleMk2,
            ["weapon_mg"] = Permission.WPMG,
            ["weapon_microsmg"] = Permission.WPMicroSMG,
            ["weapon_minigun"] = Permission.WPMinigun,
            ["weapon_minismg"] = Permission.WPMiniSMG,
            ["weapon_molotov"] = Permission.WPMolotov,
            ["weapon_musket"] = Permission.WPMusket,
            ["weapon_nightstick"] = Permission.WPNightstick,
            ["weapon_petrolcan"] = Permission.WPPetrolCan,
            ["weapon_pipebomb"] = Permission.WPPipeBomb,
            ["weapon_pistol"] = Permission.WPPistol,
            ["weapon_pistol50"] = Permission.WPPistol50,
            ["weapon_pistol_mk2"] = Permission.WPPistolMk2,
            ["weapon_poolcue"] = Permission.WPPoolCue,
            ["weapon_proxmine"] = Permission.WPProximityMine,
            ["weapon_pumpshotgun"] = Permission.WPPumpShotgun,
            ["weapon_pumpshotgun_mk2"] = Permission.WPPumpShotgunMk2,
            ["weapon_railgun"] = Permission.WPRailgun,
            ["weapon_revolver"] = Permission.WPRevolver,
            ["weapon_revolver_mk2"] = Permission.WPRevolverMk2,
            ["weapon_rpg"] = Permission.WPRPG,
            ["weapon_sawnoffshotgun"] = Permission.WPSawnOffShotgun,
            ["weapon_smg"] = Permission.WPSMG,
            ["weapon_smg_mk2"] = Permission.WPSMGMk2,
            ["weapon_smokegrenade"] = Permission.WPSmokeGrenade,
            ["weapon_sniperrifle"] = Permission.WPSniperRifle,
            ["weapon_snowball"] = Permission.WPSnowball,
            ["weapon_snspistol"] = Permission.WPSNSPistol,
            ["weapon_snspistol_mk2"] = Permission.WPSNSPistolMk2,
            ["weapon_specialcarbine"] = Permission.WPSpecialCarbine,
            ["weapon_specialcarbine_mk2"] = Permission.WPSpecialCarbineMk2,
            ["weapon_stickybomb"] = Permission.WPStickyBomb,
            ["weapon_stungun"] = Permission.WPStunGun,
            ["weapon_switchblade"] = Permission.WPSwitchBlade,
            ["weapon_unarmed"] = Permission.WPUnarmed,
            ["weapon_vintagepistol"] = Permission.WPVintagePistol,
            ["weapon_wrench"] = Permission.WPWrench,
            ["weapon_raypistol"] = Permission.WPPlasmaPistol,
            ["weapon_raycarbine"] = Permission.WPPlasmaCarbine,
            ["weapon_rayminigun"] = Permission.WPPlasmaMinigun,
            ["weapon_stone_hatchet"] = Permission.WPStoneHatchet,
            // MPHEIST3 DLC (v 1868)
            ["weapon_ceramicpistol"] = Permission.WPCeramicPistol,
            ["weapon_navyrevolver"] = Permission.WPNavyRevolver,
            ["weapon_hazardcan"] = Permission.WPHazardCan,
            // MPHEIST4 DLC (v 2189)
            ["weapon_gadgetpistol"] = Permission.WPPericoPistol,
            ["weapon_militaryrifle"] = Permission.WPMilitaryRifle,
            ["weapon_combatshotgun"] = Permission.WPCombatShotgun,
            // MPSECURITY DLC (v 2545)
            ["weapon_emplauncher"] = Permission.WPEMPLauncher,
            ["weapon_heavyrifle"] = Permission.WPHeavyRifle,
            ["weapon_fertilizercan"] = Permission.WPFertilizerCan,
            ["weapon_stungun_mp"] = Permission.WPStunGunMP,
            // MPSUM2 DLC (V 2699)
            ["weapon_tacticalrifle"] = Permission.WPTacticalRifle,
            ["weapon_precisionrifle"] = Permission.WPPrecisionRifle,
            // MPCHRISTMAS3 DLC (V 2802)
            ["weapon_pistolxm3"] = Permission.WPPistolXM3,
            ["weapon_candycane"] = Permission.WPCandyCane,
            ["weapon_railgunxm3"] = Permission.WPRailgunXM3,
            ["weapon_acidpackage"] = Permission.WPAcidPackage,
            // MP2023_01 DLC (V 2944)
            ["weapon_tecpistol"] = Permission.WPTecPistol,
        };
        #endregion

        #region weapon component names
        private static readonly Dictionary<string, string> weaponComponentNames = new()
        {
            ["COMPONENT_ADVANCEDRIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_ADVANCEDRIFLE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_ADVANCEDRIFLE_VARMOD_LUXE"] = GetLabelText("WCT_VAR_METAL"),
            ["COMPONENT_APPISTOL_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_APPISTOL_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_APPISTOL_VARMOD_LUXE"] = GetLabelText("WCT_VAR_METAL"),
            ["COMPONENT_ASSAULTRIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_ASSAULTRIFLE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_ASSAULTRIFLE_CLIP_03"] = GetLabelText("WCT_CLIP_DRM"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CLIP_ARMORPIERCING"] = GetLabelText("WCT_CLIP_AP"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_ASSAULTRIFLE_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_ASSAULTRIFLE_VARMOD_LUXE"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_ASSAULTSHOTGUN_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_ASSAULTSHOTGUN_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_ASSAULTSMG_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_ASSAULTSMG_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_ASSAULTSMG_VARMOD_LOWRIDER"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_AT_AR_AFGRIP_02"] = GetLabelText("WCT_GRIP"),
            ["COMPONENT_AT_AR_AFGRIP"] = GetLabelText("WCT_GRIP"),
            ["COMPONENT_AT_AR_BARREL_01"] = GetLabelText("WCT_BARR"),
            ["COMPONENT_AT_AR_BARREL_02"] = GetLabelText("WCT_BARR2"),
            ["COMPONENT_AT_AR_FLSH"] = GetLabelText("WCT_FLASH"),
            ["COMPONENT_AT_AR_SUPP_02"] = GetLabelText("WCT_SUPP"),
            ["COMPONENT_AT_AR_SUPP"] = GetLabelText("WCT_SUPP"),
            ["COMPONENT_AT_BP_BARREL_01"] = GetLabelText("WCT_BARR"),
            ["COMPONENT_AT_BP_BARREL_02"] = GetLabelText("WCT_BARR2"),
            ["COMPONENT_AT_CR_BARREL_01"] = GetLabelText("WCT_BARR"),
            ["COMPONENT_AT_CR_BARREL_02"] = GetLabelText("WCT_BARR2"),
            ["COMPONENT_AT_MG_BARREL_01"] = GetLabelText("WCT_BARR"),
            ["COMPONENT_AT_MG_BARREL_02"] = GetLabelText("WCT_BARR2"),
            ["COMPONENT_AT_MRFL_BARREL_01"] = GetLabelText("WCT_BARR"),
            ["COMPONENT_AT_MRFL_BARREL_02"] = GetLabelText("WCT_BARR2"),
            ["COMPONENT_AT_MUZZLE_01"] = GetLabelText("WCT_MUZZ1"),
            ["COMPONENT_AT_MUZZLE_02"] = GetLabelText("WCT_MUZZ2"),
            ["COMPONENT_AT_MUZZLE_03"] = GetLabelText("WCT_MUZZ3"),
            ["COMPONENT_AT_MUZZLE_04"] = GetLabelText("WCT_MUZZ4"),
            ["COMPONENT_AT_MUZZLE_05"] = GetLabelText("WCT_MUZZ5"),
            ["COMPONENT_AT_MUZZLE_06"] = GetLabelText("WCT_MUZZ6"),
            ["COMPONENT_AT_MUZZLE_07"] = GetLabelText("WCT_MUZZ7"),
            ["COMPONENT_AT_MUZZLE_08"] = GetLabelText("WCT_MUZZ"),
            ["COMPONENT_AT_MUZZLE_09"] = GetLabelText("WCT_MUZZ9"),
            ["COMPONENT_AT_PI_COMP_02"] = GetLabelText("WCT_COMP"),
            ["COMPONENT_AT_PI_COMP_03"] = GetLabelText("WCT_COMP"),
            ["COMPONENT_AT_PI_COMP"] = GetLabelText("WCT_COMP"),
            ["COMPONENT_AT_PI_FLSH_02"] = GetLabelText("WCT_FLASH"),
            ["COMPONENT_AT_PI_FLSH_03"] = GetLabelText("WCT_FLASH"),
            ["COMPONENT_AT_PI_FLSH"] = GetLabelText("WCT_FLASH"),
            ["COMPONENT_AT_PI_RAIL_02"] = GetLabelText("WCT_SCOPE_PI"),
            ["COMPONENT_AT_PI_RAIL"] = GetLabelText("WCT_SCOPE_PI"),
            ["COMPONENT_AT_PI_SUPP_02"] = GetLabelText("WCT_SUPP"),
            ["COMPONENT_AT_PI_SUPP"] = GetLabelText("WCT_SUPP"),
            ["COMPONENT_AT_SB_BARREL_01"] = GetLabelText("WCT_BARR"),
            ["COMPONENT_AT_SB_BARREL_02"] = GetLabelText("WCT_BARR2"),
            ["COMPONENT_AT_SC_BARREL_01"] = GetLabelText("WCT_BARR"),
            ["COMPONENT_AT_SC_BARREL_02"] = GetLabelText("WCT_BARR2"),
            ["COMPONENT_AT_SCOPE_LARGE_FIXED_ZOOM_MK2"] = GetLabelText("WCT_SCOPE_LRG2"),
            ["COMPONENT_AT_SCOPE_LARGE_FIXED_ZOOM"] = GetLabelText("WCT_SCOPE_LRG"),
            ["COMPONENT_AT_SCOPE_LARGE_MK2"] = GetLabelText("WCT_SCOPE_LRG2"),
            ["COMPONENT_AT_SCOPE_LARGE"] = GetLabelText("WCT_SCOPE_LRG"),
            ["COMPONENT_AT_SCOPE_MACRO_02_MK2"] = GetLabelText("WCT_SCOPE_MAC2"),
            ["COMPONENT_AT_SCOPE_MACRO_02_SMG_MK2"] = GetLabelText("WCT_SCOPE_MAC2"),
            ["COMPONENT_AT_SCOPE_MACRO_02"] = GetLabelText("WCT_SCOPE_MAC"),
            ["COMPONENT_AT_SCOPE_MACRO_MK2"] = GetLabelText("WCT_SCOPE_MAC2"),
            ["COMPONENT_AT_SCOPE_MACRO"] = GetLabelText("WCT_SCOPE_MAC"),
            ["COMPONENT_AT_SCOPE_MAX"] = GetLabelText("WCT_SCOPE_MAX"),
            ["COMPONENT_AT_SCOPE_MEDIUM_MK2"] = GetLabelText("WCT_SCOPE_MED2"),
            ["COMPONENT_AT_SCOPE_MEDIUM"] = GetLabelText("WCT_SCOPE_LRG"),
            ["COMPONENT_AT_SCOPE_NV"] = GetLabelText("WCT_SCOPE_NV"),
            ["COMPONENT_AT_SCOPE_SMALL_02"] = GetLabelText("WCT_SCOPE_SML"),
            ["COMPONENT_AT_SCOPE_SMALL_MK2"] = GetLabelText("WCT_SCOPE_SML2"),
            ["COMPONENT_AT_SCOPE_SMALL_SMG_MK2"] = GetLabelText("WCT_SCOPE_SML2"),
            ["COMPONENT_AT_SCOPE_SMALL"] = GetLabelText("WCT_SCOPE_SML"),
            ["COMPONENT_AT_SCOPE_THERMAL"] = GetLabelText("WCT_SCOPE_TH"),
            ["COMPONENT_AT_SIGHTS_SMG"] = GetLabelText("WCT_HOLO"),
            ["COMPONENT_AT_SIGHTS"] = GetLabelText("WCT_HOLO"),
            ["COMPONENT_AT_SR_BARREL_01"] = GetLabelText("WCT_BARR"),
            ["COMPONENT_AT_SR_BARREL_02"] = GetLabelText("WCT_BARR2"),
            ["COMPONENT_AT_SR_SUPP_03"] = GetLabelText("WCT_SUPP"),
            ["COMPONENT_AT_SR_SUPP"] = GetLabelText("WCT_SUPP"),
            ["COMPONENT_BULLPUPRIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_BULLPUPRIFLE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CLIP_ARMORPIERCING"] = GetLabelText("WCT_CLIP_AP"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_BULLPUPRIFLE_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_BULLPUPRIFLE_VARMOD_LOW"] = GetLabelText("WCT_VAR_METAL"),
            ["COMPONENT_CARBINERIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_CARBINERIFLE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_CARBINERIFLE_CLIP_03"] = GetLabelText("WCT_CLIP_DRM"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_CARBINERIFLE_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_CARBINERIFLE_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_CARBINERIFLE_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_CARBINERIFLE_MK2_CLIP_ARMORPIERCING"] = GetLabelText("WCT_CLIP_AP"),
            ["COMPONENT_CARBINERIFLE_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_CARBINERIFLE_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_CARBINERIFLE_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_CARBINERIFLE_VARMOD_LUXE"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_COMBATMG_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_COMBATMG_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_COMBATMG_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_COMBATMG_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_COMBATMG_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_COMBATMG_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_COMBATMG_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_COMBATMG_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_COMBATMG_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_COMBATMG_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_COMBATMG_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_COMBATMG_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_COMBATMG_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_COMBATMG_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_COMBATMG_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_COMBATMG_MK2_CLIP_ARMORPIERCING"] = GetLabelText("WCT_CLIP_AP"),
            ["COMPONENT_COMBATMG_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_COMBATMG_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_COMBATMG_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_COMBATPDW_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_COMBATPDW_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_COMBATPDW_CLIP_03"] = GetLabelText("WCT_CLIP_DRM"),
            ["COMPONENT_COMBATPISTOL_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_COMBATPISTOL_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_COMBATPISTOL_VARMOD_LOWRIDER"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_COMPACTRIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_COMPACTRIFLE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_COMPACTRIFLE_CLIP_03"] = GetLabelText("WCT_CLIP_DRM"),
            ["COMPONENT_GUSENBERG_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_GUSENBERG_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_HEAVYPISTOL_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_HEAVYPISTOL_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_HEAVYPISTOL_VARMOD_LUXE"] = GetLabelText("WCT_VAR_WOOD"),
            ["COMPONENT_HEAVYSHOTGUN_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_HEAVYSHOTGUN_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_HEAVYSHOTGUN_CLIP_03"] = GetLabelText("WCT_CLIP_DRM"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_HEAVYSNIPER_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_HEAVYSNIPER_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_HEAVYSNIPER_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_HEAVYSNIPER_MK2_CLIP_ARMORPIERCING"] = GetLabelText("WCT_CLIP_AP"),
            ["COMPONENT_HEAVYSNIPER_MK2_CLIP_EXPLOSIVE"] = GetLabelText("WCT_CLIP_EX"),
            ["COMPONENT_HEAVYSNIPER_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_HEAVYSNIPER_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_KNUCKLE_VARMOD_BALLAS"] = GetLabelText("WCT_KNUCK_BG"),
            ["COMPONENT_KNUCKLE_VARMOD_BASE"] = GetLabelText("WCT_KNUCK_01"),
            ["COMPONENT_KNUCKLE_VARMOD_DIAMOND"] = GetLabelText("WCT_KNUCK_DMD"),
            ["COMPONENT_KNUCKLE_VARMOD_DOLLAR"] = GetLabelText("WCT_KNUCK_DLR"),
            ["COMPONENT_KNUCKLE_VARMOD_HATE"] = GetLabelText("WCT_KNUCK_HT"),
            ["COMPONENT_KNUCKLE_VARMOD_KING"] = GetLabelText("WCT_KNUCK_SLG"),
            ["COMPONENT_KNUCKLE_VARMOD_LOVE"] = GetLabelText("WCT_KNUCK_LV"),
            ["COMPONENT_KNUCKLE_VARMOD_PIMP"] = GetLabelText("WCT_KNUCK_02"),
            ["COMPONENT_KNUCKLE_VARMOD_PLAYER"] = GetLabelText("WCT_KNUCK_PC"),
            ["COMPONENT_KNUCKLE_VARMOD_VAGOS"] = GetLabelText("WCT_KNUCK_VG"),
            ["COMPONENT_MACHINEPISTOL_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_MACHINEPISTOL_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_MACHINEPISTOL_CLIP_03"] = GetLabelText("WCT_CLIP_DRM"),
            ["COMPONENT_MARKSMANRIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_MARKSMANRIFLE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CLIP_ARMORPIERCING"] = GetLabelText("WCT_CLIP_AP"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_MARKSMANRIFLE_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_MARKSMANRIFLE_VARMOD_LUXE"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_MG_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_MG_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_MG_VARMOD_LOWRIDER"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_MICROSMG_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_MICROSMG_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_MICROSMG_VARMOD_LUXE"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_MINISMG_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_MINISMG_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_PISTOL_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_PISTOL_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_PISTOL_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_PISTOL_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_PISTOL_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_PISTOL_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_PISTOL_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_PISTOL_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_PISTOL_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_PISTOL_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_PISTOL_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_PISTOL_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_PISTOL_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_PISTOL_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_PISTOL_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_PISTOL_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_PISTOL_MK2_CLIP_HOLLOWPOINT"] = GetLabelText("WCT_CLIP_HP"),
            ["COMPONENT_PISTOL_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_PISTOL_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_PISTOL_VARMOD_LUXE"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_PISTOL50_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_PISTOL50_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_PISTOL50_VARMOD_LUXE"] = GetLabelText("WCT_VAR_SIL"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CLIP_01"] = GetLabelText("WCT_SHELL"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CLIP_ARMORPIERCING"] = GetLabelText("WCT_SHELL_AP"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CLIP_EXPLOSIVE"] = GetLabelText("WCT_SHELL_EX"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CLIP_HOLLOWPOINT"] = GetLabelText("WCT_SHELL_HP"),
            ["COMPONENT_PUMPSHOTGUN_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_SHELL_INC"),
            ["COMPONENT_PUMPSHOTGUN_VARMOD_LOWRIDER"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_REVOLVER_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_REVOLVER_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_REVOLVER_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_REVOLVER_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_REVOLVER_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_REVOLVER_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_REVOLVER_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_REVOLVER_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_REVOLVER_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_REVOLVER_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_REVOLVER_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_REVOLVER_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1_RV"),
            ["COMPONENT_REVOLVER_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_REVOLVER_MK2_CLIP_HOLLOWPOINT"] = GetLabelText("WCT_CLIP_HP"),
            ["COMPONENT_REVOLVER_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_REVOLVER_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_REVOLVER_VARMOD_BOSS"] = GetLabelText("WCT_REV_VARB"),
            ["COMPONENT_REVOLVER_VARMOD_GOON"] = GetLabelText("WCT_REV_VARG"),
            ["COMPONENT_SAWNOFFSHOTGUN_VARMOD_LUXE"] = GetLabelText("WCT_VAR_METAL"),
            ["COMPONENT_SMG_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_SMG_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_SMG_CLIP_03"] = GetLabelText("WCT_CLIP_DRM"),
            ["COMPONENT_SMG_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_SMG_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_SMG_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_SMG_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_SMG_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_SMG_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_SMG_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_SMG_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_SMG_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_SMG_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_SMG_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_SMG_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_SMG_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_SMG_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_SMG_MK2_CLIP_HOLLOWPOINT"] = GetLabelText("WCT_CLIP_HP"),
            ["COMPONENT_SMG_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_SMG_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_SMG_VARMOD_LUXE"] = GetLabelText("WCT_VAR_GOLD"),
            ["COMPONENT_SNIPERRIFLE_VARMOD_LUXE"] = GetLabelText("WCT_VAR_WOOD"),
            ["COMPONENT_SNSPISTOL_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_SNSPISTOL_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_SNSPISTOL_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_SNSPISTOL_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_SNSPISTOL_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_SNSPISTOL_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_SNSPISTOL_MK2_CLIP_HOLLOWPOINT"] = GetLabelText("WCT_CLIP_HP"),
            ["COMPONENT_SNSPISTOL_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_SNSPISTOL_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_SNSPISTOL_VARMOD_LOWRIDER"] = GetLabelText("WCT_VAR_WOOD"),
            ["COMPONENT_SPECIALCARBINE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_SPECIALCARBINE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_SPECIALCARBINE_CLIP_03"] = GetLabelText("WCT_CLIP_DRM"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_02"] = GetLabelText("WCT_CAMO_2"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_03"] = GetLabelText("WCT_CAMO_3"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_04"] = GetLabelText("WCT_CAMO_4"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_05"] = GetLabelText("WCT_CAMO_5"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_06"] = GetLabelText("WCT_CAMO_6"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_07"] = GetLabelText("WCT_CAMO_7"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_08"] = GetLabelText("WCT_CAMO_8"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_09"] = GetLabelText("WCT_CAMO_9"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_10"] = GetLabelText("WCT_CAMO_10"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO_IND_01"] = GetLabelText("WCT_CAMO_IND"),
            ["COMPONENT_SPECIALCARBINE_MK2_CAMO"] = GetLabelText("WCT_CAMO_1"),
            ["COMPONENT_SPECIALCARBINE_MK2_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_SPECIALCARBINE_MK2_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_SPECIALCARBINE_MK2_CLIP_ARMORPIERCING"] = GetLabelText("WCT_CLIP_AP"),
            ["COMPONENT_SPECIALCARBINE_MK2_CLIP_FMJ"] = GetLabelText("WCT_CLIP_FMJ"),
            ["COMPONENT_SPECIALCARBINE_MK2_CLIP_INCENDIARY"] = GetLabelText("WCT_CLIP_INC"),
            ["COMPONENT_SPECIALCARBINE_MK2_CLIP_TRACER"] = GetLabelText("WCT_CLIP_TR"),
            ["COMPONENT_SPECIALCARBINE_VARMOD_LOWRIDER"] = GetLabelText("WCT_VAR_ETCHM"),
            ["COMPONENT_SWITCHBLADE_VARMOD_BASE"] = GetLabelText("WCT_SB_BASE"),
            ["COMPONENT_SWITCHBLADE_VARMOD_VAR1"] = GetLabelText("WCT_SB_VAR1"),
            ["COMPONENT_SWITCHBLADE_VARMOD_VAR2"] = GetLabelText("WCT_SB_VAR2"),
            ["COMPONENT_VINTAGEPISTOL_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_VINTAGEPISTOL_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_RAYPISTOL_VARMOD_XMAS18"] = GetLabelText("WCT_VAR_RAY18"),
            // MPHEIST3 DLC (v 1868)
            ["COMPONENT_CERAMICPISTOL_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_CERAMICPISTOL_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_CERAMICPISTOL_SUPP"] = GetLabelText("WCT_SUPP"),
            // MPHEIST4 DLC (v 2189)
            ["COMPONENT_MILITARYRIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_MILITARYRIFLE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_MILITARYRIFLE_SIGHT_01"] = GetLabelText("WCT_MRFL_SIGHT"),
            // MPSECURITY DLC (v 2545)
            ["COMPONENT_APPISTOL_VARMOD_SECURITY"] = GetLabelText("WCT_VAR_STUD"),
            ["COMPONENT_MICROSMG_VARMOD_SECURITY"] = GetLabelText("WCT_VAR_WEED"),
            ["COMPONENT_PUMPSHOTGUN_VARMOD_SECURITY"] = GetLabelText("WCT_VAR_BONE"),
            ["COMPONENT_HEAVYRIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_HEAVYRIFLE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_HEAVYRIFLE_SIGHT_01"] = GetLabelText("WCT_HVYRFLE_SIG"),
            ["COMPONENT_HEAVYRIFLE_CAMO1"] = GetLabelText("WCT_VAR_FAM"),
            // MPSUM2 DLC (V 2699)
            ["COMPONENT_TACTICALRIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_TACTICALRIFLE_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_PRECISIONRIFLE_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_AT_AR_FLSH_REH"] = GetLabelText("WCT_FLASH"),
            // MPCHRISTMAS3 DLC (V 2802)
            ["COMPONENT_PISTOLXM3_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_PISTOLXM3_SUPP"] = GetLabelText("WCT_SUPP"),
            ["COMPONENT_MICROSMG_VARMOD_XM3"] = GetLabelText("WCT_MSMG_XM3"),
            ["COMPONENT_PUMPSHOTGUN_VARMOD_XM3"] = GetLabelText("WCT_PUMPSHT_XM3"),
            ["COMPONENT_PISTOL_MK2_VARMOD_XM3"] = GetLabelText("WCT_PISTMK2_XM3"),
            ["COMPONENT_PISTOL_MK2_VARMOD_XM3_SLIDE"] = GetLabelText("WCT_PISTMK2_XM3"),
            ["COMPONENT_BAT_VARMOD_XM3"] = GetLabelText("WCT_BAT_XM3"),
            ["COMPONENT_BAT_VARMOD_XM3_01"] = GetLabelText("WCT_BAT_XM301"),
            ["COMPONENT_BAT_VARMOD_XM3_02"] = GetLabelText("WCT_BAT_XM302"),
            ["COMPONENT_BAT_VARMOD_XM3_03"] = GetLabelText("WCT_BAT_XM303"),
            ["COMPONENT_BAT_VARMOD_XM3_04"] = GetLabelText("WCT_BAT_XM304"),
            ["COMPONENT_BAT_VARMOD_XM3_05"] = GetLabelText("WCT_BAT_XM305"),
            ["COMPONENT_BAT_VARMOD_XM3_06"] = GetLabelText("WCT_BAT_XM306"),
            ["COMPONENT_BAT_VARMOD_XM3_07"] = GetLabelText("WCT_BAT_XM307"),
            ["COMPONENT_BAT_VARMOD_XM3_08"] = GetLabelText("WCT_BAT_XM308"),
            ["COMPONENT_BAT_VARMOD_XM3_09"] = GetLabelText("WCT_BAT_XM309"),
            ["COMPONENT_KNIFE_VARMOD_XM3"] = GetLabelText("WCT_KNIFE_XM3"),
            ["COMPONENT_KNIFE_VARMOD_XM3_01"] = GetLabelText("WCT_KNIFE_XM301"),
            ["COMPONENT_KNIFE_VARMOD_XM3_02"] = GetLabelText("WCT_KNIFE_XM302"),
            ["COMPONENT_KNIFE_VARMOD_XM3_03"] = GetLabelText("WCT_KNIFE_XM303"),
            ["COMPONENT_KNIFE_VARMOD_XM3_04"] = GetLabelText("WCT_KNIFE_XM304"),
            ["COMPONENT_KNIFE_VARMOD_XM3_05"] = GetLabelText("WCT_KNIFE_XM305"),
            ["COMPONENT_KNIFE_VARMOD_XM3_06"] = GetLabelText("WCT_KNIFE_XM306"),
            ["COMPONENT_KNIFE_VARMOD_XM3_07"] = GetLabelText("WCT_KNIFE_XM307"),
            ["COMPONENT_KNIFE_VARMOD_XM3_08"] = GetLabelText("WCT_KNIFE_XM308"),
            ["COMPONENT_KNIFE_VARMOD_XM3_09"] = GetLabelText("WCT_KNIFE_XM309"),
            // MP2023_01 DLC (V 2944)
            ["COMPONENT_TECPISTOL_CLIP_01"] = GetLabelText("WCT_CLIP1"),
            ["COMPONENT_TECPISTOL_CLIP_02"] = GetLabelText("WCT_CLIP2"),
            ["COMPONENT_MICROSMG_VARMOD_FRN"] = GetLabelText("WCT_MSMGFRN_VAR"),
            ["COMPONENT_CARBINERIFLE_VARMOD_MICH"] = GetLabelText("WCT_CRBNMIC_VAR"),
            ["COMPONENT_RPG_VARMOD_TVR"] = GetLabelText("WCT_RPGTVR_VAR"),
        };
        #endregion

        #region weapon tints
        public static readonly Dictionary<string, int> WeaponTints = new()
        {
            ["Black"] = 0,
            ["Green"] = 1,
            ["Gold"] = 2,
            ["Pink"] = 3,
            ["Army"] = 4,
            ["LSPD"] = 5,
            ["Orange"] = 6,
            ["Platinum"] = 7,
        };
        #endregion

        #region weapon mk2 tints
        public static readonly Dictionary<string, int> WeaponTintsMkII = new()
        {
            ["Classic Black"] = 0,
            ["Classic Gray"] = 1,
            ["Classic Two Tone"] = 2,
            ["Classic White"] = 3,
            ["Classic Beige"] = 4,
            ["Classic Green"] = 5,
            ["Classic Blue"] = 6,
            ["Classic Earth"] = 7,
            ["Classic Brown & Black"] = 8,
            ["Red Contrast"] = 9,
            ["Blue Contrast"] = 10,
            ["Yellow Contrast"] = 11,
            ["Orange Contrast"] = 12,
            ["Bold Pink"] = 13,
            ["Bold Purple & Yellow"] = 14,
            ["Bold Orange"] = 15,
            ["Bold Green & Purple"] = 16,
            ["Bold Red Features"] = 17,
            ["Bold Green Features"] = 18,
            ["Bold Cyan Features"] = 19,
            ["Bold Yellow Features"] = 20,
            ["Bold Red & White"] = 21,
            ["Bold Blue & White"] = 22,
            ["Metallic Gold"] = 23,
            ["Metallic Platinum"] = 24,
            ["Metallic Gray & Lilac"] = 25,
            ["Metallic Purple & Lime"] = 26,
            ["Metallic Red"] = 27,
            ["Metallic Green"] = 28,
            ["Metallic Blue"] = 29,
            ["Metallic White & Aqua"] = 30,
            ["Metallic Red & Yellow"] = 31
        };
        #endregion
        #endregion
    }
}
