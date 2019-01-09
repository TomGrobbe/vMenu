using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class WeaponOptions
    {
        // Variables
        private Menu menu;

        public bool UnlimitedAmmo { get; private set; } = UserDefaults.WeaponsUnlimitedAmmo;
        public bool NoReload { get; private set; } = UserDefaults.WeaponsNoReload;
        public bool AutoEquipChute { get; private set; } = UserDefaults.AutoEquipChute;

        public static Dictionary<string, uint> AddonWeapons = new Dictionary<string, uint>();

        private Dictionary<Menu, ValidWeapon> weaponInfo = new Dictionary<Menu, ValidWeapon>();
        private Dictionary<MenuItem, string> weaponComponents = new Dictionary<MenuItem, string>();

        #region Create Menu
        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create main weapon options menu and add items
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Weapon Options");

            MenuItem getAllWeapons = new MenuItem("Get All Weapons", "Get all weapons.");
            MenuItem removeAllWeapons = new MenuItem("Remove All Weapons", "Removes all weapons in your inventory.");
            MenuCheckboxItem unlimitedAmmo = new MenuCheckboxItem("Unlimited Ammo", "Unlimited ammonition supply.", UnlimitedAmmo);
            MenuCheckboxItem noReload = new MenuCheckboxItem("No Reload", "Never reload.", NoReload);
            MenuItem setAmmo = new MenuItem("Set All Ammo Count", "Set the amount of ammo in all your weapons.");
            MenuItem refillMaxAmmo = new MenuItem("Refill All Ammo", "Give all your weapons max ammo.");
            MenuItem spawnByName = new MenuItem("Spawn Weapon By Name", "Enter a weapon mode name to spawn.");

            // Add items based on permissions
            if (IsAllowed(Permission.WPGetAll))
            {
                menu.AddMenuItem(getAllWeapons);
            }
            if (IsAllowed(Permission.WPRemoveAll))
            {
                menu.AddMenuItem(removeAllWeapons);
            }
            if (IsAllowed(Permission.WPUnlimitedAmmo))
            {
                menu.AddMenuItem(unlimitedAmmo);
            }
            if (IsAllowed(Permission.WPNoReload))
            {
                menu.AddMenuItem(noReload);
            }
            if (IsAllowed(Permission.WPSetAllAmmo))
            {
                menu.AddMenuItem(setAmmo);
                menu.AddMenuItem(refillMaxAmmo);
            }
            if (IsAllowed(Permission.WPSpawnByName))
            {
                menu.AddMenuItem(spawnByName);
            }
            #endregion

            #region addonweapons submenu
            MenuItem addonWeaponsBtn = new MenuItem("Addon Weapons", "Equip / remove addon weapons available on this server.");
            Menu addonWeaponsMenu = new Menu("Addon Weapons", "Equip/Remove Addon Weapons");
            menu.AddMenuItem(addonWeaponsBtn);

            #region manage creating and accessing addon weapons menu
            if (IsAllowed(Permission.WPSpawn) && AddonWeapons != null && AddonWeapons.Count > 0)
            {
                MenuController.BindMenuItem(menu, addonWeaponsMenu, addonWeaponsBtn);
                foreach (KeyValuePair<string, uint> weapon in AddonWeapons)
                {
                    string name = weapon.Key.ToString();
                    uint model = weapon.Value;
                    var item = new MenuItem(name, $"Click to add/remove this weapon ({name}) to/from your inventory.");
                    addonWeaponsMenu.AddMenuItem(item);
                    if (!IsWeaponValid(model))
                    {
                        item.Enabled = false;
                        item.LeftIcon = MenuItem.Icon.LOCK;
                        item.Description = "This model is not available. Please ask the server owner to verify it's being streamed correctly.";
                    }
                }
                addonWeaponsMenu.OnItemSelect += (sender, item, index) =>
                {
                    var weapon = AddonWeapons.ElementAt(index);
                    if (HasPedGotWeapon(Game.PlayerPed.Handle, weapon.Value, false))
                    {
                        RemoveWeaponFromPed(Game.PlayerPed.Handle, weapon.Value);
                    }
                    else
                    {
                        var maxAmmo = 200;
                        GetMaxAmmo(Game.PlayerPed.Handle, weapon.Value, ref maxAmmo);
                        GiveWeaponToPed(Game.PlayerPed.Handle, weapon.Value, maxAmmo, false, true);
                    }
                };
                addonWeaponsBtn.Label = "→→→";
            }
            else
            {
                addonWeaponsBtn.LeftIcon = MenuItem.Icon.LOCK;
                addonWeaponsBtn.Enabled = false;
                addonWeaponsBtn.Description = "This option is not available on this server because you don't have permission to use it, or it is not setup correctly.";
            }
            #endregion

            //addonWeaponsMenu.
            addonWeaponsMenu.RefreshIndex();
            //addonWeaponsMenu.UpdateScaleform();
            #endregion

            #region parachute options menu
            #region parachute buttons and submenus
            MenuItem parachuteBtn = new MenuItem("Parachute Options", "All parachute related options can be changed here.");
            Menu parachuteMenu = new Menu("Parachute Options", "Parachute Options");

            Menu primaryChute = new Menu("Parachute Options", "Select A Primary Parachute");

            Menu secondaryChute = new Menu("Parachute Options", "Select A Reserve Parachute");

            MenuItem chute = new MenuItem("No Style", "Default parachute.");
            MenuItem chute0 = new MenuItem(GetLabelText("PM_TINT0"), GetLabelText("PD_TINT0"));             // Rainbow Chute
            MenuItem chute1 = new MenuItem(GetLabelText("PM_TINT1"), GetLabelText("PD_TINT1"));             // Red Chute
            MenuItem chute2 = new MenuItem(GetLabelText("PM_TINT2"), GetLabelText("PD_TINT2"));             // Seaside Stripes Chute
            MenuItem chute3 = new MenuItem(GetLabelText("PM_TINT3"), GetLabelText("PD_TINT3"));             // Window Maker Chute
            MenuItem chute4 = new MenuItem(GetLabelText("PM_TINT4"), GetLabelText("PD_TINT4"));             // Patriot Chute
            MenuItem chute5 = new MenuItem(GetLabelText("PM_TINT5"), GetLabelText("PD_TINT5"));             // Blue Chute
            MenuItem chute6 = new MenuItem(GetLabelText("PM_TINT6"), GetLabelText("PD_TINT6"));             // Black Chute
            MenuItem chute7 = new MenuItem(GetLabelText("PM_TINT7"), GetLabelText("PD_TINT7"));             // Hornet Chute
            MenuItem chute8 = new MenuItem(GetLabelText("PS_CAN_0"), "Air Force parachute.");               // Air Force Chute
            MenuItem chute9 = new MenuItem(GetLabelText("PM_TINT0"), "Desert parachute.");                  // Desert Chute
            MenuItem chute10 = new MenuItem("Shadow Chute", "Shadow parachute.");                           // Shadow Chute
            MenuItem chute11 = new MenuItem(GetLabelText("UNLOCK_NAME_PSRWD"), "High altitude parachute."); // High Altitude Chute
            MenuItem chute12 = new MenuItem("Airborne Chute", "Airborne parachute.");                       // Airborne Chute
            MenuItem chute13 = new MenuItem("Sunrise Chute", "Sunrise parachute.");                         // Sunrise Chute
            MenuItem rchute = new MenuItem("No Style", "Default parachute.");
            MenuItem rchute0 = new MenuItem(GetLabelText("PM_TINT0"), GetLabelText("PD_TINT0"));             // Rainbow Chute
            MenuItem rchute1 = new MenuItem(GetLabelText("PM_TINT1"), GetLabelText("PD_TINT1"));             // Red Chute
            MenuItem rchute2 = new MenuItem(GetLabelText("PM_TINT2"), GetLabelText("PD_TINT2"));             // Seaside Stripes Chute
            MenuItem rchute3 = new MenuItem(GetLabelText("PM_TINT3"), GetLabelText("PD_TINT3"));             // Window Maker Chute
            MenuItem rchute4 = new MenuItem(GetLabelText("PM_TINT4"), GetLabelText("PD_TINT4"));             // Patriot Chute
            MenuItem rchute5 = new MenuItem(GetLabelText("PM_TINT5"), GetLabelText("PD_TINT5"));             // Blue Chute
            MenuItem rchute6 = new MenuItem(GetLabelText("PM_TINT6"), GetLabelText("PD_TINT6"));             // Black Chute
            MenuItem rchute7 = new MenuItem(GetLabelText("PM_TINT7"), GetLabelText("PD_TINT7"));             // Hornet Chute
            MenuItem rchute8 = new MenuItem(GetLabelText("PS_CAN_0"), "Air Force parachute.");               // Air Force Chute
            MenuItem rchute9 = new MenuItem(GetLabelText("PM_TINT0"), "Desert parachute.");                  // Desert Chute
            MenuItem rchute10 = new MenuItem("Shadow Chute", "Shadow parachute.");                           // Shadow Chute
            MenuItem rchute11 = new MenuItem(GetLabelText("UNLOCK_NAME_PSRWD"), "High altitude parachute."); // High Altitude Chute
            MenuItem rchute12 = new MenuItem("Airborne Chute", "Airborne parachute.");                       // Airborne Chute
            MenuItem rchute13 = new MenuItem("Sunrise Chute", "Sunrise parachute.");                         // Sunrise Chute

            primaryChute.AddMenuItem(chute);
            primaryChute.AddMenuItem(chute0);
            primaryChute.AddMenuItem(chute1);
            primaryChute.AddMenuItem(chute2);
            primaryChute.AddMenuItem(chute3);
            primaryChute.AddMenuItem(chute4);
            primaryChute.AddMenuItem(chute5);
            primaryChute.AddMenuItem(chute6);
            primaryChute.AddMenuItem(chute7);
            primaryChute.AddMenuItem(chute8);
            primaryChute.AddMenuItem(chute9);
            primaryChute.AddMenuItem(chute10);
            primaryChute.AddMenuItem(chute11);
            primaryChute.AddMenuItem(chute12);
            primaryChute.AddMenuItem(chute13);

            secondaryChute.AddMenuItem(rchute);
            secondaryChute.AddMenuItem(rchute0);
            secondaryChute.AddMenuItem(rchute1);
            secondaryChute.AddMenuItem(rchute2);
            secondaryChute.AddMenuItem(rchute3);
            secondaryChute.AddMenuItem(rchute4);
            secondaryChute.AddMenuItem(rchute5);
            secondaryChute.AddMenuItem(rchute6);
            secondaryChute.AddMenuItem(rchute7);
            secondaryChute.AddMenuItem(rchute8);
            secondaryChute.AddMenuItem(rchute9);
            secondaryChute.AddMenuItem(rchute10);
            secondaryChute.AddMenuItem(rchute11);
            secondaryChute.AddMenuItem(rchute12);
            secondaryChute.AddMenuItem(rchute13);
            #endregion

            #region handle events
            primaryChute.OnItemSelect += (sender, item, index) =>
            {
                SetPedParachuteTintIndex(Game.PlayerPed.Handle, index - 1);
                Subtitle.Custom($"Primary parachute style selected: ~r~{item.Text}~s~.");
            };

            secondaryChute.OnItemSelect += (sender, item, index) =>
            {
                SetPlayerReserveParachuteTintIndex(Game.Player.Handle, index - 1);
                Subtitle.Custom($"Reserve parachute style selected: ~r~{item.Text}~s~.");
            };
            #endregion

            #region create more buttons
            MenuItem primaryChuteBtn = new MenuItem("Primary Parachute Style", "Select a primary parachute.");
            MenuItem secondaryChuteBtn = new MenuItem("Reserve Parachute Style", "Select a reserve parachute.");

            parachuteMenu.AddMenuItem(primaryChuteBtn);
            primaryChuteBtn.Label = "→→→";
            parachuteMenu.AddMenuItem(secondaryChuteBtn);
            secondaryChuteBtn.Label = "→→→";

            MenuController.BindMenuItem(parachuteMenu, primaryChute, primaryChuteBtn);
            MenuController.BindMenuItem(parachuteMenu, secondaryChute, secondaryChuteBtn);

            MenuCheckboxItem autoEquipParachute = new MenuCheckboxItem("Auto Equip Parachute", "Automatically equip a parachute whenever you enter a plane/helicopter.", AutoEquipChute);
            parachuteMenu.AddMenuItem(autoEquipParachute);

            MenuItem togglePrimary = new MenuItem("Get / Remove Primary Parachute", "Equip a primary parachute.");
            MenuItem toggleSecondary = new MenuItem("Get Reserve Parachute", "Equip a reserve parachute, you need to get a primary parachute first before equipping a reserve parachute.");

            parachuteMenu.AddMenuItem(togglePrimary);
            parachuteMenu.AddMenuItem(toggleSecondary);
            #endregion

            #region handle parachute menu events
            parachuteMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == togglePrimary)
                {
                    if (HasPedGotWeapon(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, false))
                    {
                        RemoveWeaponFromPed(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute);
                        Notify.Success("Primary parachute ~r~removed~s~.", true);
                    }
                    else
                    {
                        GiveWeaponToPed(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, 1, false, false);
                        Notify.Success("Primary parachute ~g~equippped~s~.", true);
                    }
                }
                else if (item == toggleSecondary)
                {
                    SetPlayerHasReserveParachute(Game.Player.Handle);
                    Notify.Success("Reserve parachute ~g~equippped~s~.", true);
                }
            };

            parachuteMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == autoEquipParachute)
                {
                    AutoEquipChute = _checked;
                }
            };
            #endregion

            #region parachute smoke trail colors
            List<string> smokeColor = new List<string>()
            {
                "White",
                "Yellow",
                "Red",
                "Green",
                "Blue",
                "Dark Gray",
            };

            MenuListItem smokeColors = new MenuListItem("Smoke Trail Color", smokeColor, 0, "Select a parachute smoke trail color.");
            parachuteMenu.AddMenuItem(smokeColors);
            parachuteMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == smokeColors)
                {
                    SetPlayerCanLeaveParachuteSmokeTrail(Game.Player.Handle, false);
                    if (newIndex == 0)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 255, 255, 255);
                    }
                    else if (newIndex == 1)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 255, 255, 0);
                    }
                    else if (newIndex == 2)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 255, 0, 0);
                    }
                    else if (newIndex == 3)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 0, 255, 0);
                    }
                    else if (newIndex == 4)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 0, 0, 255);
                    }
                    else if (newIndex == 5)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 1, 1, 1);
                    }

                    SetPlayerCanLeaveParachuteSmokeTrail(Game.Player.Handle, true);
                }
            };
            #endregion

            #region misc parachute menu setup
            menu.AddMenuItem(parachuteBtn);
            parachuteBtn.Label = "→→→";
            MenuController.BindMenuItem(menu, parachuteMenu, parachuteBtn);

            parachuteMenu.RefreshIndex();
            //parachuteMenu.UpdateScaleform();

            primaryChute.RefreshIndex();
            //primaryChute.UpdateScaleform();

            secondaryChute.RefreshIndex();
            //secondaryChute.UpdateScaleform();

            MenuController.AddSubmenu(menu, addonWeaponsMenu);
            MenuController.AddSubmenu(menu, parachuteMenu);
            MenuController.AddSubmenu(menu, primaryChute);
            MenuController.AddSubmenu(menu, secondaryChute);
            #endregion
            #endregion

            #region Create Weapon Category Submenus
            MenuItem spacer = GetSpacerMenuItem("↓ Weapon Categories ↓");
            menu.AddMenuItem(spacer);

            Menu handGuns = new Menu("Weapons", "Handguns");
            MenuItem handGunsBtn = new MenuItem("Handguns");

            Menu rifles = new Menu("Weapons", "Assault Rifles");
            MenuItem riflesBtn = new MenuItem("Assault Rifles");

            Menu shotguns = new Menu("Weapons", "Shotguns");
            MenuItem shotgunsBtn = new MenuItem("Shotguns");

            Menu smgs = new Menu("Weapons", "Sub-/Light Machine Guns");
            MenuItem smgsBtn = new MenuItem("Sub-/Light Machine Guns");

            Menu throwables = new Menu("Weapons", "Throwables");
            MenuItem throwablesBtn = new MenuItem("Throwables");

            Menu melee = new Menu("Weapons", "Melee");
            MenuItem meleeBtn = new MenuItem("Melee");

            Menu heavy = new Menu("Weapons", "Heavy Weapons");
            MenuItem heavyBtn = new MenuItem("Heavy Weapons");

            Menu snipers = new Menu("Weapons", "Sniper Rifles");
            MenuItem snipersBtn = new MenuItem("Sniper Rifles");

            MenuController.AddSubmenu(menu, handGuns);
            MenuController.AddSubmenu(menu, rifles);
            MenuController.AddSubmenu(menu, shotguns);
            MenuController.AddSubmenu(menu, smgs);
            MenuController.AddSubmenu(menu, throwables);
            MenuController.AddSubmenu(menu, melee);
            MenuController.AddSubmenu(menu, heavy);
            MenuController.AddSubmenu(menu, snipers);
            #endregion

            #region Setup weapon category buttons and submenus.
            handGunsBtn.Label = "→→→";
            menu.AddMenuItem(handGunsBtn);
            MenuController.BindMenuItem(menu, handGuns, handGunsBtn);

            riflesBtn.Label = "→→→";
            menu.AddMenuItem(riflesBtn);
            MenuController.BindMenuItem(menu, rifles, riflesBtn);

            shotgunsBtn.Label = "→→→";
            menu.AddMenuItem(shotgunsBtn);
            MenuController.BindMenuItem(menu, shotguns, shotgunsBtn);

            smgsBtn.Label = "→→→";
            menu.AddMenuItem(smgsBtn);
            MenuController.BindMenuItem(menu, smgs, smgsBtn);

            throwablesBtn.Label = "→→→";
            menu.AddMenuItem(throwablesBtn);
            MenuController.BindMenuItem(menu, throwables, throwablesBtn);

            meleeBtn.Label = "→→→";
            menu.AddMenuItem(meleeBtn);
            MenuController.BindMenuItem(menu, melee, meleeBtn);

            heavyBtn.Label = "→→→";
            menu.AddMenuItem(heavyBtn);
            MenuController.BindMenuItem(menu, heavy, heavyBtn);

            snipersBtn.Label = "→→→";
            menu.AddMenuItem(snipersBtn);
            MenuController.BindMenuItem(menu, snipers, snipersBtn);
            #endregion

            #region Loop through all weapons, create menus for them and add all menu items and handle events.
            foreach (ValidWeapon weapon in ValidWeapons.WeaponList)
            {
                uint cat = (uint)GetWeapontypeGroup(weapon.Hash);
                if (!string.IsNullOrEmpty(weapon.Name) && (IsAllowed(weapon.Perm) || IsAllowed(Permission.WPGetAll)))
                {
                    //Log($"[DEBUG LOG] [WEAPON-BUG] {weapon.Name} - {weapon.Perm} = {IsAllowed(weapon.Perm)} & All = {IsAllowed(Permission.WPGetAll)}");
                    #region Create menu for this weapon and add buttons
                    Menu weaponMenu = new Menu("Weapon Options", weapon.Name);
                    MenuItem weaponItem = new MenuItem(weapon.Name, $"Open the options for ~y~{weapon.Name.ToString()}~s~.");
                    weaponItem.Label = "→→→";
                    weaponItem.LeftIcon = MenuItem.Icon.GUN;

                    weaponInfo.Add(weaponMenu, weapon);

                    MenuItem getOrRemoveWeapon = new MenuItem("Equip/Remove Weapon", "Add or remove this weapon to/form your inventory.");
                    getOrRemoveWeapon.LeftIcon = MenuItem.Icon.GUN;
                    weaponMenu.AddMenuItem(getOrRemoveWeapon);
                    if (!IsAllowed(Permission.WPSpawn))
                    {
                        getOrRemoveWeapon.Enabled = false;
                        getOrRemoveWeapon.Description = "You do not have permission to use this option.";
                        getOrRemoveWeapon.LeftIcon = MenuItem.Icon.LOCK;
                    }

                    MenuItem fillAmmo = new MenuItem("Re-fill Ammo", "Get max ammo for this weapon.");
                    fillAmmo.LeftIcon = MenuItem.Icon.AMMO;
                    weaponMenu.AddMenuItem(fillAmmo);

                    List<string> tints = new List<string>();
                    if (weapon.Name.Contains(" Mk II"))
                    {
                        foreach (var tint in ValidWeapons.WeaponTintsMkII)
                        {
                            tints.Add(tint.Key);
                        }
                    }
                    else
                    {
                        foreach (var tint in ValidWeapons.WeaponTints)
                        {
                            tints.Add(tint.Key);
                        }
                    }

                    MenuListItem weaponTints = new MenuListItem("Tints", tints, 0, "Select a tint for your weapon.");
                    weaponMenu.AddMenuItem(weaponTints);
                    #endregion

                    #region Handle weapon specific list changes
                    weaponMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
                    {
                        if (item == weaponTints)
                        {
                            if (HasPedGotWeapon(Game.PlayerPed.Handle, weaponInfo[sender].Hash, false))
                            {
                                SetPedWeaponTintIndex(Game.PlayerPed.Handle, weaponInfo[sender].Hash, newIndex);
                            }
                            else
                            {
                                Notify.Error("You need to get the weapon first!");
                            }
                        }
                    };
                    #endregion

                    #region Handle weapon specific button presses
                    weaponMenu.OnItemSelect += (sender, item, index) =>
                    {
                        var info = weaponInfo[sender];
                        uint hash = info.Hash;

                        SetCurrentPedWeapon(Game.PlayerPed.Handle, hash, true);

                        if (item == getOrRemoveWeapon)
                        {
                            if (HasPedGotWeapon(Game.PlayerPed.Handle, hash, false))
                            {
                                RemoveWeaponFromPed(Game.PlayerPed.Handle, hash);
                                Subtitle.Custom("Weapon removed.");
                            }
                            else
                            {
                                var ammo = 255;
                                GetMaxAmmo(Game.PlayerPed.Handle, hash, ref ammo);
                                GiveWeaponToPed(Game.PlayerPed.Handle, hash, ammo, false, true);
                                Subtitle.Custom("Weapon added.");
                            }
                        }
                        else if (item == fillAmmo)
                        {
                            if (HasPedGotWeapon(Game.PlayerPed.Handle, hash, false))
                            {
                                var ammo = 900;
                                GetMaxAmmo(Game.PlayerPed.Handle, hash, ref ammo);
                                SetPedAmmo(Game.PlayerPed.Handle, hash, ammo);
                            }
                            else
                            {
                                Notify.Error("You need to get the weapon first before re-filling ammo!");
                            }
                        }
                    };
                    #endregion

                    #region load components
                    if (weapon.Components != null)
                    {
                        if (weapon.Components.Count > 0)
                        {
                            foreach (var comp in weapon.Components)
                            {
                                //Log($"{weapon.Name} : {comp.Key}");
                                MenuItem compItem = new MenuItem(comp.Key, "Click to equip or remove this component.");
                                weaponComponents.Add(compItem, comp.Key);
                                weaponMenu.AddMenuItem(compItem);

                                #region Handle component button presses
                                weaponMenu.OnItemSelect += (sender, item, index) =>
                                {
                                    if (item == compItem)
                                    {
                                        var Weapon = weaponInfo[sender];
                                        var componentHash = Weapon.Components[weaponComponents[item]];
                                        if (HasPedGotWeapon(Game.PlayerPed.Handle, Weapon.Hash, false))
                                        {
                                            SetCurrentPedWeapon(Game.PlayerPed.Handle, Weapon.Hash, true);
                                            if (HasPedGotWeaponComponent(Game.PlayerPed.Handle, Weapon.Hash, componentHash))
                                            {
                                                RemoveWeaponComponentFromPed(Game.PlayerPed.Handle, Weapon.Hash, componentHash);

                                                Subtitle.Custom("Component removed.");
                                            }
                                            else
                                            {
                                                int ammo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, Weapon.Hash);

                                                int clipAmmo = GetMaxAmmoInClip(Game.PlayerPed.Handle, Weapon.Hash, false);
                                                GetAmmoInClip(Game.PlayerPed.Handle, Weapon.Hash, ref clipAmmo);

                                                GiveWeaponComponentToPed(Game.PlayerPed.Handle, Weapon.Hash, componentHash);

                                                SetAmmoInClip(Game.PlayerPed.Handle, Weapon.Hash, clipAmmo);

                                                SetPedAmmo(Game.PlayerPed.Handle, Weapon.Hash, ammo);
                                                Subtitle.Custom("Component equiped.");
                                            }
                                        }
                                        else
                                        {
                                            Notify.Error("You need to get the weapon first before you can modify it.");
                                        }
                                    }
                                };
                                #endregion
                            }
                        }
                    }
                    #endregion

                    // refresh and add to menu.
                    weaponMenu.RefreshIndex();

                    if (cat == 970310034) // 970310034 rifles
                    {
                        MenuController.AddSubmenu(rifles, weaponMenu);
                        MenuController.BindMenuItem(rifles, weaponMenu, weaponItem);
                        rifles.AddMenuItem(weaponItem);
                    }
                    else if (cat == 416676503 || cat == 690389602) // 416676503 hand guns // 690389602 stun gun
                    {
                        MenuController.AddSubmenu(handGuns, weaponMenu);
                        MenuController.BindMenuItem(handGuns, weaponMenu, weaponItem);
                        handGuns.AddMenuItem(weaponItem);
                    }
                    else if (cat == 860033945) // 860033945 shotguns
                    {
                        MenuController.AddSubmenu(shotguns, weaponMenu);
                        MenuController.BindMenuItem(shotguns, weaponMenu, weaponItem);
                        shotguns.AddMenuItem(weaponItem);
                    }
                    else if (cat == 3337201093 || cat == 1159398588) // 3337201093 sub machine guns // 1159398588 light machine guns
                    {
                        MenuController.AddSubmenu(smgs, weaponMenu);
                        MenuController.BindMenuItem(smgs, weaponMenu, weaponItem);
                        smgs.AddMenuItem(weaponItem);
                    }
                    else if (cat == 1548507267 || cat == 4257178988 || cat == 1595662460) // 1548507267 throwables // 4257178988 fire extinghuiser // jerry can
                    {
                        MenuController.AddSubmenu(throwables, weaponMenu);
                        MenuController.BindMenuItem(throwables, weaponMenu, weaponItem);
                        throwables.AddMenuItem(weaponItem);
                    }
                    else if (cat == 3566412244 || cat == 2685387236) // 3566412244 melee weapons // 2685387236 knuckle duster
                    {
                        MenuController.AddSubmenu(melee, weaponMenu);
                        MenuController.BindMenuItem(melee, weaponMenu, weaponItem);
                        melee.AddMenuItem(weaponItem);
                    }
                    else if (cat == 2725924767) // 2725924767 heavy weapons
                    {
                        MenuController.AddSubmenu(heavy, weaponMenu);
                        MenuController.BindMenuItem(heavy, weaponMenu, weaponItem);
                        heavy.AddMenuItem(weaponItem);
                    }
                    else if (cat == 3082541095) // 3082541095 sniper rifles
                    {
                        MenuController.AddSubmenu(snipers, weaponMenu);
                        MenuController.BindMenuItem(snipers, weaponMenu, weaponItem);
                        snipers.AddMenuItem(weaponItem);
                    }
                }
            }
            #endregion

            #region Disable submenus if no weapons in that category are allowed.
            if (handGuns.Size == 0)
            {
                handGunsBtn.LeftIcon = MenuItem.Icon.LOCK;
                handGunsBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                handGunsBtn.Enabled = false;
            }
            if (rifles.Size == 0)
            {
                riflesBtn.LeftIcon = MenuItem.Icon.LOCK;
                riflesBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                riflesBtn.Enabled = false;
            }
            if (shotguns.Size == 0)
            {
                shotgunsBtn.LeftIcon = MenuItem.Icon.LOCK;
                shotgunsBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                shotgunsBtn.Enabled = false;
            }
            if (smgs.Size == 0)
            {
                smgsBtn.LeftIcon = MenuItem.Icon.LOCK;
                smgsBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                smgsBtn.Enabled = false;
            }
            if (throwables.Size == 0)
            {
                throwablesBtn.LeftIcon = MenuItem.Icon.LOCK;
                throwablesBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                throwablesBtn.Enabled = false;
            }
            if (melee.Size == 0)
            {
                meleeBtn.LeftIcon = MenuItem.Icon.LOCK;
                meleeBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                meleeBtn.Enabled = false;
            }
            if (heavy.Size == 0)
            {
                heavyBtn.LeftIcon = MenuItem.Icon.LOCK;
                heavyBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                heavyBtn.Enabled = false;
            }
            if (snipers.Size == 0)
            {
                snipersBtn.LeftIcon = MenuItem.Icon.LOCK;
                snipersBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                snipersBtn.Enabled = false;
            }
            #endregion

            #region Handle button presses
            menu.OnItemSelect += (sender, item, index) =>
            {
                Ped ped = new Ped(Game.PlayerPed.Handle);
                if (item == getAllWeapons)
                {
                    foreach (ValidWeapon vw in ValidWeapons.WeaponList)
                    {
                        if (IsAllowed(vw.Perm))
                        {
                            GiveWeaponToPed(Game.PlayerPed.Handle, vw.Hash, vw.GetMaxAmmo, false, true);

                            int ammoInClip = GetMaxAmmoInClip(Game.PlayerPed.Handle, vw.Hash, false);
                            SetAmmoInClip(Game.PlayerPed.Handle, vw.Hash, ammoInClip);
                            int ammo = 0;
                            GetMaxAmmo(Game.PlayerPed.Handle, vw.Hash, ref ammo);
                            SetPedAmmo(Game.PlayerPed.Handle, vw.Hash, ammo);
                        }
                    }

                    SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_unarmed"), true);
                }
                else if (item == removeAllWeapons)
                {
                    ped.Weapons.RemoveAll();
                }
                else if (item == setAmmo)
                {
                    SetAllWeaponsAmmo();
                }
                else if (item == refillMaxAmmo)
                {
                    foreach (ValidWeapon vw in ValidWeapons.WeaponList)
                    {
                        if (HasPedGotWeapon(Game.PlayerPed.Handle, vw.Hash, false))
                        {
                            int ammoInClip = GetMaxAmmoInClip(Game.PlayerPed.Handle, vw.Hash, false);
                            SetAmmoInClip(Game.PlayerPed.Handle, vw.Hash, ammoInClip);
                            int ammo = 0;
                            GetMaxAmmo(Game.PlayerPed.Handle, vw.Hash, ref ammo);
                            SetPedAmmo(Game.PlayerPed.Handle, vw.Hash, ammo);
                        }
                    }
                }
                else if (item == spawnByName)
                {
                    SpawnCustomWeapon();
                }
            };
            #endregion

            #region Handle checkbox changes
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == noReload)
                {
                    NoReload = _checked;
                    Subtitle.Custom($"No reload is now {(_checked ? "enabled" : "disabled")}.");
                }
                else if (item == unlimitedAmmo)
                {
                    UnlimitedAmmo = _checked;
                    Subtitle.Custom($"Unlimited ammo is now {(_checked ? "enabled" : "disabled")}.");
                }
            };
            #endregion
        }
        #endregion

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}