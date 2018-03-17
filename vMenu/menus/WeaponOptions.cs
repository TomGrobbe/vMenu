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
        private CommonFunctions cf = MainMenu.Cf;
        public bool UnlimitedAmmo = UserDefaults.WeaponsUnlimitedAmmo;
        public bool NoReload = UserDefaults.WeaponsNoReload;
        public static Dictionary<string, uint> AddonWeapons = new Dictionary<string, uint>();

        private Dictionary<UIMenu, ValidWeapon> weaponInfo = new Dictionary<UIMenu, ValidWeapon>();
        private Dictionary<UIMenuItem, string> weaponComponents = new Dictionary<UIMenuItem, string>();

        private void CreateMenu()
        {
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
            UIMenuCheckboxItem unlimitedAmmo = new UIMenuCheckboxItem("Unlimited Ammo", UnlimitedAmmo, "Unlimited ammonition supply.");
            UIMenuCheckboxItem noReload = new UIMenuCheckboxItem("No Reload", NoReload, "Never reload.");
            UIMenuItem setAmmo = new UIMenuItem("Set All Ammo Count", "Set the amount of ammo in all your weapons.");
            UIMenuItem refillMaxAmmo = new UIMenuItem("Refill All Ammo", "Give all your weapons max ammo.");
            ValidWeapons vw = new ValidWeapons();
            UIMenuItem addonWeaponsBtn = new UIMenuItem("Addon Weapons", "Equip / remove addon weapons available on this server.");
            UIMenu addonWeaponsMenu = new UIMenu("Addon Weapons", "Equip/Remove Addon Weapons", true)
            {
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenuItem parachuteBtn = new UIMenuItem("Parachute Options", "All parachute related options can be changed here.");
            UIMenu parachuteMenu = new UIMenu("Parachute Options", "Parachute Options", true)
            {
                MouseEdgeEnabled = false,
                MouseControlsEnabled = false,
                ControlDisablingEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenuItem spawnByName = new UIMenuItem("Spawn Weapon By Name", "Enter a weapon mode name to spawn.");

            if (cf.IsAllowed(Permission.WPGetAll))
            {
                menu.AddItem(getAllWeapons);
            }
            if (cf.IsAllowed(Permission.WPRemoveAll))
            {
                menu.AddItem(removeAllWeapons);
            }
            if (cf.IsAllowed(Permission.WPUnlimitedAmmo))
            {
                menu.AddItem(unlimitedAmmo);
            }
            if (cf.IsAllowed(Permission.WPNoReload))
            {
                menu.AddItem(noReload);
            }
            if (cf.IsAllowed(Permission.WPSetAllAmmo))
            {
                menu.AddItem(setAmmo);
                menu.AddItem(refillMaxAmmo);
            }
            if (cf.IsAllowed(Permission.WPSpawn))
            {
                menu.AddItem(spawnByName);
            }

            menu.AddItem(addonWeaponsBtn);

            if (cf.IsAllowed(Permission.WPSpawn) && AddonWeapons != null && AddonWeapons.Count > 0)
            {
                menu.BindMenuToItem(addonWeaponsMenu, addonWeaponsBtn);
                foreach (KeyValuePair<string, uint> weapon in AddonWeapons)
                {
                    string name = weapon.Key.ToString();
                    uint model = weapon.Value;
                    var item = new UIMenuItem(name, $"Click to add/remove this weapon ({name}) to/from your inventory.");
                    addonWeaponsMenu.AddItem(item);
                    if (!IsWeaponValid(model))
                    {
                        item.Enabled = false;
                        item.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                        item.Description = "This model is not available. Please ask the server owner to verify it's being streamed correctly.";
                    }
                }
                addonWeaponsMenu.OnItemSelect += (sender, item, index) =>
                {
                    var weapon = AddonWeapons.ElementAt(index);
                    if (HasPedGotWeapon(PlayerPedId(), weapon.Value, false))
                    {
                        RemoveWeaponFromPed(PlayerPedId(), weapon.Value);
                    }
                    else
                    {
                        var maxAmmo = 200;
                        GetMaxAmmo(PlayerPedId(), weapon.Value, ref maxAmmo);
                        GiveWeaponToPed(PlayerPedId(), weapon.Value, maxAmmo, false, true);
                    }
                };
                addonWeaponsBtn.SetRightLabel("→→→");
            }
            else
            {
                addonWeaponsBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                addonWeaponsBtn.Enabled = false;
                addonWeaponsBtn.Description = "This option is not available on this server because you don't have permission to use it, or it is not setup correctly.";
            }

            parachuteBtn.Enabled = false;
            parachuteBtn.SetRightLabel("WIP");
            parachuteBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
            menu.AddItem(parachuteBtn);
            //menu.BindMenuToItem(parachuteMenu, parachuteBtn);

            addonWeaponsMenu.RefreshIndex();
            addonWeaponsMenu.UpdateScaleform();

            parachuteMenu.RefreshIndex();
            parachuteMenu.UpdateScaleform();

            MainMenu.Mp.Add(addonWeaponsMenu);
            MainMenu.Mp.Add(parachuteMenu);

            foreach (ValidWeapon weapon in vw.WeaponList)
            {
                if (weapon.Name != null)
                {
                    UIMenu weaponMenu = new UIMenu("Weapon Options", weapon.Name, true)
                    {
                        ScaleWithSafezone = false,
                        MouseControlsEnabled = false,
                        MouseEdgeEnabled = false,
                        ControlDisablingEnabled = false
                    };
                    UIMenuItem weaponItem = new UIMenuItem(weapon.Name, $"Open the options for ~y~{weapon.Name.ToString()}~s~.");
                    weaponItem.SetRightLabel("→→→");
                    weaponItem.SetLeftBadge(UIMenuItem.BadgeStyle.Gun);

                    MainMenu.Mp.Add(weaponMenu);

                    weaponInfo.Add(weaponMenu, weapon);

                    UIMenuItem getOrRemoveWeapon = new UIMenuItem("Equip/Remove Weapon", "Add or remove this weapon to/form your inventory.");
                    getOrRemoveWeapon.SetLeftBadge(UIMenuItem.BadgeStyle.Gun);
                    weaponMenu.AddItem(getOrRemoveWeapon);
                    if (!cf.IsAllowed(Permission.WPSpawn))
                    {
                        getOrRemoveWeapon.Enabled = false;
                        getOrRemoveWeapon.Description = "This option has been disabled by the server owner.";
                        getOrRemoveWeapon.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    }

                    UIMenuItem fillAmmo = new UIMenuItem("Re-fill Ammo", "Get max ammo for this weapon.");
                    fillAmmo.SetLeftBadge(UIMenuItem.BadgeStyle.Ammo);
                    weaponMenu.AddItem(fillAmmo);

                    List<dynamic> tints = new List<dynamic>();
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

                    UIMenuListItem weaponTints = new UIMenuListItem("Tints", tints, 0, "Select a tint for your weapon.");
                    weaponMenu.AddItem(weaponTints);

                    weaponMenu.OnListChange += (sender, item, index) =>
                    {
                        if (item == weaponTints)
                        {
                            if (HasPedGotWeapon(PlayerPedId(), weaponInfo[sender].Hash, false))
                            {
                                SetPedWeaponTintIndex(PlayerPedId(), weaponInfo[sender].Hash, index);
                            }
                            else
                            {
                                Notify.Error("You need to get the weapon first!");
                            }
                        }
                    };


                    weaponMenu.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == getOrRemoveWeapon)
                        {
                            var info = weaponInfo[sender];
                            uint hash = info.Hash;
                            if (HasPedGotWeapon(PlayerPedId(), hash, false))
                            {
                                RemoveWeaponFromPed(PlayerPedId(), hash);
                                Subtitle.Custom("Weapon removed.");
                            }
                            else
                            {
                                var ammo = 255;
                                GetMaxAmmo(PlayerPedId(), hash, ref ammo);
                                GiveWeaponToPed(PlayerPedId(), hash, ammo, false, true);
                                Subtitle.Custom("Weapon added.");
                            }
                        }
                        else if (item == fillAmmo)
                        {
                            if (HasPedGotWeapon(PlayerPedId(), weaponInfo[sender].Hash, false))
                            {
                                var ammo = 900;
                                GetMaxAmmo(PlayerPedId(), weaponInfo[sender].Hash, ref ammo);
                                SetAmmoInClip(PlayerPedId(), weaponInfo[sender].Hash, ammo);
                            }
                            else
                            {
                                Notify.Error("You need to get the weapon first before re-filling ammo!");
                            }
                        }
                    };

                    if (weapon.Components != null)
                    {
                        if (weapon.Components.Count > 0)
                        {
                            foreach (var comp in weapon.Components)
                            {
                                UIMenuItem compItem = new UIMenuItem(comp.Key, "Click to equip or remove this component.");
                                weaponComponents.Add(compItem, comp.Key);
                                weaponMenu.AddItem(compItem);
                                weaponMenu.OnItemSelect += (sender, item, index) =>
                                {
                                    if (item == compItem)
                                    {
                                        var Weapon = weaponInfo[sender];
                                        var componentHash = Weapon.Components[weaponComponents[item]];
                                        if (HasPedGotWeapon(PlayerPedId(), Weapon.Hash, false))
                                        {
                                            if (HasPedGotWeaponComponent(PlayerPedId(), Weapon.Hash, componentHash))
                                            {
                                                RemoveWeaponComponentFromPed(PlayerPedId(), Weapon.Hash, componentHash);
                                                Subtitle.Custom("Component removed.");
                                            }
                                            else
                                            {
                                                GiveWeaponComponentToPed(PlayerPedId(), Weapon.Hash, componentHash);
                                                Subtitle.Custom("Component equiped.");
                                            }
                                        }
                                        else
                                        {
                                            Notify.Error("You need to get the weapon first before you can modify it.");
                                        }
                                    }
                                };
                            }
                        }
                    }
                    weaponMenu.RefreshIndex();
                    weaponMenu.UpdateScaleform();

                    menu.AddItem(weaponItem);
                    menu.BindMenuToItem(weaponMenu, weaponItem);
                }
            }

            menu.OnItemSelect += (sender, item, index) =>
            {
                Ped ped = new Ped(PlayerPedId());
                if (item == getAllWeapons)
                {
                    foreach (var weapon in ValidWeapons.Weapons)
                    {
                        var ammo = 255;
                        GetMaxAmmo(PlayerPedId(), weapon.Value, ref ammo);
                        ped.Weapons.Give((WeaponHash)weapon.Value, ammo, weapon.Key == "Unarmed", true);
                    }
                    ped.Weapons.Give(WeaponHash.Unarmed, 0, true, true);
                }
                else if (item == removeAllWeapons)
                {
                    ped.Weapons.RemoveAll();
                }
                else if (item == setAmmo)
                {
                    cf.SetAllWeaponsAmmo();
                }
                else if (item == refillMaxAmmo)
                {
                    foreach (var wp in ValidWeapons.Weapons)
                    {
                        if (ped.Weapons.HasWeapon((WeaponHash)wp.Value))
                        {
                            int maxammo = 200;
                            GetMaxAmmo(ped.Handle, wp.Value, ref maxammo);
                            SetPedAmmo(ped.Handle, wp.Value, maxammo);
                        }
                    }
                }
                else if (item == spawnByName)
                {
                    cf.SpawnCustomWeapon();
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
    }
}