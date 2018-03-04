using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    public class WeaponOptions
    {
        // Variables
        private UIMenu menu;
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.cf;
        public bool UnlimitedAmmo = UserDefaults.WeaponsUnlimitedAmmo;
        public bool NoReload = UserDefaults.WeaponsNoReload;
        public static Dictionary<string, uint> ValidWeapons = new Dictionary<string, uint>();

        private void CreateMenu()
        {
            var num = 0;
            foreach (WeaponHash weaponHash in Enum.GetValues(typeof(WeaponHash)))
            {
                //string name = Weapon.GetDisplayNameFromHash(weaponHash);
                string name = weaponNames[num];
                if (name != "Invalid" && name != "Parachute" & name != "Unarmed")
                {
                    ValidWeapons.Add(name, (uint)weaponHash);
                }
                num++;
            }

            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Weapon Options", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            UIMenuItem getAllWeapons = new UIMenuItem("Get All Weapons", "Get all weapons.");
            UIMenuItem removeAllWeapons = new UIMenuItem("Remove All Weapons", "Removes all weapons in your inventory.");
            UIMenuItem weaponComponents = new UIMenuItem("Weapon Components", "Toggle weapon components on/off for each weapon.");
            UIMenuCheckboxItem unlimitedAmmo = new UIMenuCheckboxItem("Unlimited Ammo", UnlimitedAmmo, "Unlimited ammonition supply.");
            UIMenuCheckboxItem noReload = new UIMenuCheckboxItem("No Reload", NoReload, "Never reload.");


            menu.AddItem(getAllWeapons);
            menu.AddItem(removeAllWeapons);
            menu.AddItem(weaponComponents);
            menu.AddItem(unlimitedAmmo);
            menu.AddItem(noReload);

            foreach (var weapon in ValidWeapons)
            {
                menu.AddItem(new UIMenuItem($"~o~{weapon.Key}"));
            }

            menu.OnItemSelect += (sender, item, index) =>
            {
                Ped ped = new Ped(PlayerPedId());
                if (item == getAllWeapons)
                {
                    foreach (var weapon in ValidWeapons)
                    {
                        ped.Weapons.Give((WeaponHash)weapon.Value, 200, false, true);
                    }
                    // "Give" the player "unarmed" to make sure they don't auto equip any of the weapons.
                    ped.Weapons.Give(WeaponHash.Unarmed, 0, true, true);
                }
                else if (item == removeAllWeapons)
                {
                    ped.Weapons.RemoveAll();
                }
                else if (item == weaponComponents)
                {
                    // Todo
                }
                else
                {
                    uint hash = ValidWeapons[item.Text.Substring(3)];
                    if (HasPedGotWeapon(Game.PlayerPed.Handle, hash, false))
                    {
                        RemoveWeaponFromPed(Game.PlayerPed.Handle, hash);
                        GiveWeaponToPed(Game.PlayerPed.Handle, (uint)GetHashKey("WEAPON_UNARMED"), 0, false, true);
                    }
                    else
                    {
                        int ammo = 900;
                        GetMaxAmmo(Game.PlayerPed.Handle, hash, ref ammo);
                        GiveWeaponToPed(Game.PlayerPed.Handle, hash, ammo, false, true);
                    }
                }
            };


            menu.OnCheckboxChange += (sender, item, _checked) =>
            {
                if (item == noReload)
                {
                    NoReload = _checked;
                }
                else if (item == unlimitedAmmo)
                {
                    UnlimitedAmmo = _checked;
                }
            };
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }

        #region Weapon names, hashes and localized names.
        /// <summary>
        /// List of all localized names.
        /// </summary>
        public static List<string> weaponNames = new List<string>()
        {
            "Sniper Rifle",
            "Fire Extinguisher",
            "Compact Grenade Launcher",
            "Snowball",
            "Vintage Pistol",
            "Combat PDW",
            "Heavy Sniper Mk II",
            "Heavy Sniper",
            "Sweeper Shotgun",
            "Micro SMG",
            "Pipe Wrench",
            "Pistol",
            "Pump Shotgun",
            "AP Pistol",
            "Ball",
            "Molotov",
            "SMG",
            "Sticky Bomb",
            "Jerry Can",
            "Stun Gun",
            "Assault Rifle Mk II",
            "Heavy Shotgun",
            "Minigun",
            "Golf Club",
            "Flare Gun",
            "Flare",
            "Invalid",
            "Hammer",
            "Combat Pistol",
            "Gusenberg Sweeper",
            "Compact Rifle",
            "Homing Launcher",
            "Nightstick",
            "Railgun",
            "Sawed-Off Shotgun",
            "SMG Mk II",
            "Bullpup Rifle",
            "Firework Launcher",
            "Combat MG",
            "Carbine Rifle",
            "Crowbar",
            "Flashlight",
            "Antique Cavalry Dagger",
            "Grenade",
            "Pool Cue",
            "Baseball Bat",
            "Pistol .50",
            "Knife",
            "MG",
            "Bullpup Shotgun",
            "BZ Gas",
            "Invalid",
            "Grenade Launcher",
            "Unarmed",
            "Musket",
            "Proximity Mine",
            "Advanced Rifle",
            "RPG",
            "Pipe Bomb",
            "Mini SMG",
            "SNS Pistol",
            "Pistol Mk II",
            "Assault Rifle",
            "Special Carbine",
            "Heavy Revolver",
            "Marksman Rifle",
            "Battle Axe",
            "Heavy Pistol",
            "Knuckle Duster",
            "Machine Pistol",
            "Combat MG Mk II",
            "Marksman Pistol",
            "Machete",
            "Switchblade",
            "Assault Shotgun",
            "Double Barrel Shotgun",
            "Assault SMG",
            "Hatchet",
            "Bottle",
            "Carbine Rifle Mk II",
            "Parachute",
            "Tear Gas",
        };

        /// <summary>
        /// List of all weapon names + hashes.
        /// </summary>
        public static Dictionary<string, Int64> Weapons = new Dictionary<string, Int64>()
        {
            ["SniperRifle"] = 100416529,
            ["FireExtinguisher"] = 101631238,
            ["CompactGrenadeLauncher"] = 125959754,
            ["Snowball"] = 126349499,
            ["VintagePistol"] = 137902532,
            ["CombatPDW"] = 171789620,
            ["HeavySniperMk2"] = 177293209,
            ["HeavySniper"] = 205991906,
            ["SweeperShotgun"] = 317205821,
            ["MicroSMG"] = 324215364,
            ["Wrench"] = 419712736,
            ["Pistol"] = 453432689,
            ["PumpShotgun"] = 487013001,
            ["APPistol"] = 584646201,
            ["Ball"] = 600439132,
            ["Molotov"] = 615608432,
            ["SMG"] = 736523883,
            ["StickyBomb"] = 741814745,
            ["PetrolCan"] = 883325847,
            ["StunGun"] = 911657153,
            ["AssaultRifleMk2"] = 961495388,
            ["HeavyShotgun"] = 984333226,
            ["Minigun"] = 1119849093,
            ["GolfClub"] = 1141786504,
            ["FlareGun"] = 1198879012,
            ["Flare"] = 1233104067,
            ["GrenadeLauncherSmoke"] = 1305664598,
            ["Hammer"] = 1317494643,
            ["CombatPistol"] = 1593441988,
            ["Gusenberg"] = 1627465347,
            ["CompactRifle"] = 1649403952,
            ["HomingLauncher"] = 1672152130,
            ["Nightstick"] = 1737195953,
            ["Railgun"] = 1834241177,
            ["SawnOffShotgun"] = 2017895192,
            ["SMGMk2"] = 2024373456,
            ["BullpupRifle"] = 2132975508,
            ["Firework"] = 2138347493,
            ["CombatMG"] = 2144741730,
            ["CarbineRifle"] = 2210333304,
            ["Crowbar"] = 2227010557,
            ["Flashlight"] = 2343591895,
            ["Dagger"] = 2460120199,
            ["Grenade"] = 2481070269,
            ["PoolCue"] = 2484171525,
            ["Bat"] = 2508868239,
            ["Pistol50"] = 2578377531,
            ["Knife"] = 2578778090,
            ["MG"] = 2634544996,
            ["BullpupShotgun"] = 2640438543,
            ["BZGas"] = 2694266206,
            ["Unarmed"] = 2725352035,
            ["GrenadeLauncher"] = 2726580491,
            ["NightVision"] = 2803906140,
            ["Musket"] = 2828843422,
            ["ProximityMine"] = 2874559379,
            ["AdvancedRifle"] = 2937143193,
            ["RPG"] = 2982836145,
            ["PipeBomb"] = 3125143736,
            ["MiniSMG"] = 3173288789,
            ["SNSPistol"] = 3218215474,
            ["PistolMk2"] = 3219281620,
            ["AssaultRifle"] = 3220176749,
            ["SpecialCarbine"] = 3231910285,
            ["Revolver"] = 3249783761,
            ["MarksmanRifle"] = 3342088282,
            ["BattleAxe"] = 3441901897,
            ["HeavyPistol"] = 3523564046,
            ["KnuckleDuster"] = 3638508604,
            ["MachinePistol"] = 3675956304,
            ["CombatMGMk2"] = 3686625920,
            ["MarksmanPistol"] = 3696079510,
            ["Machete"] = 3713923289,
            ["SwitchBlade"] = 3756226112,
            ["AssaultShotgun"] = 3800352039,
            ["DoubleBarrelShotgun"] = 4019527611,
            ["AssaultSMG"] = 4024951519,
            ["Hatchet"] = 4191993645,
            ["Bottle"] = 4192643659,
            ["CarbineRifleMk2"] = 4208062921,
            ["Parachute"] = 4222310262,
            ["SmokeGrenade"] = 4256991824
        };
        #endregion
    }
}
