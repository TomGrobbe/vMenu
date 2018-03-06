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
            ValidWeapons vw = new ValidWeapons();


            menu.AddItem(getAllWeapons);
            menu.AddItem(removeAllWeapons);
            menu.AddItem(unlimitedAmmo);
            menu.AddItem(noReload);

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
                    UIMenuItem weaponItem = new UIMenuItem(weapon.Name, $"Open the options for ~y~{weapon.Name.ToString()}~w~.");
                    weaponItem.SetRightLabel("→→→");
                    weaponItem.SetLeftBadge(UIMenuItem.BadgeStyle.Gun);

                    MainMenu.Mp.Add(weaponMenu);

                    weaponInfo.Add(weaponMenu, weapon);

                    UIMenuItem getOrRemoveWeapon = new UIMenuItem("Equip/Remove Weapon");
                    getOrRemoveWeapon.SetLeftBadge(UIMenuItem.BadgeStyle.Gun);
                    weaponMenu.AddItem(getOrRemoveWeapon);
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
                    ped.Weapons.Give(WeaponHash.Unarmed, 0, true, true);
                }
                else if (item == removeAllWeapons)
                {
                    ped.Weapons.RemoveAll();
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