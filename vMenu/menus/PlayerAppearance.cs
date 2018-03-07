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
    public class PlayerAppearance
    {
        // Variables
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.Cf;
        private StorageManager sm = new StorageManager();

        private UIMenu menu;

        private UIMenu pedTextures;
        private UIMenu spawnSavedPedMenu;
        private UIMenu deleteSavedPedMenu;

        /// <summary>
        /// Creates the menu(s).
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Player Appearance", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            //Create the submenus.
            spawnSavedPedMenu = new UIMenu("Saved Peds", "Spawn a saved ped", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            deleteSavedPedMenu = new UIMenu("Saved Peds", "Delete a saved ped", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            pedTextures = new UIMenu("Ped Customization", "Customize your ped", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            // Add the (submenus) to the menu pool.
            MainMenu.Mp.Add(pedTextures);
            MainMenu.Mp.Add(spawnSavedPedMenu);
            MainMenu.Mp.Add(deleteSavedPedMenu);

            // Create the menu items.
            UIMenuItem pedCustomization = new UIMenuItem("Ped Customization", "Modify your ped's appearance.");
            pedCustomization.SetRightLabel("→→→");
            UIMenuItem savePed = new UIMenuItem("Save Current Ped", "Save your current ped and clothes.");
            savePed.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
            UIMenuItem spawnSavedPed = new UIMenuItem("Spawn Saved Ped", "Spawn one of your saved peds.");
            spawnSavedPed.SetRightLabel("→→→");
            UIMenuItem deleteSavedPed = new UIMenuItem("Delete Saved Ped", "Delete one of your saved peds.");
            deleteSavedPed.SetRightLabel("→→→");
            deleteSavedPed.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);

            // Add items to the mneu.
            menu.AddItem(pedCustomization);
            menu.AddItem(savePed);
            menu.AddItem(spawnSavedPed);
            menu.AddItem(deleteSavedPed);

            // Bind items to the submenus.
            if (cf.IsAllowed(Permission.PACustomize))
            {
                menu.BindMenuToItem(pedTextures, pedCustomization);
            }
            else
            {
                pedCustomization.Enabled = false;
                pedCustomization.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                pedCustomization.Description = "This option has been disabled by the server owner.";
            }


            if (cf.IsAllowed(Permission.PASpawnSaved))
            {
                menu.BindMenuToItem(spawnSavedPedMenu, spawnSavedPed);
            }
            else
            {
                spawnSavedPed.Enabled = false;
                spawnSavedPed.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                spawnSavedPed.Description = "This option has been disabled by the server owner.";
            }

            menu.BindMenuToItem(deleteSavedPedMenu, deleteSavedPed);

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == pedCustomization)
                {
                    RefreshCustomizationMenu();
                }
                else if (item == spawnSavedPed)
                {
                    RefreshSpawnSavedPedMenu();
                }
                else if (item == deleteSavedPed)
                {
                    RefreshDeleteSavedPedMenu();
                }
                else if (item == savePed)
                {
                    cf.SavePed();
                }
            };

            // Loop through all the modelNames and create lists of max 50 ped names each.
            for (int i = 0; i < (modelNames.Count / 50) + 1; i++)
            {
                List<dynamic> pedList = new List<dynamic>();
                for (int ii = 0; ii < 50; ii++)
                {
                    int index = ((i * 50) + ii);
                    if (index >= modelNames.Count)
                    {
                        break;
                    }
                    int max = ((modelNames.Count / 50) != i) ? 50 : modelNames.Count % 50;
                    pedList.Add(modelNames[index] + $" ({(ii + 1).ToString()}/{max.ToString()})");
                }
                UIMenuListItem pedl = new UIMenuListItem("Peds #" + (i + 1).ToString(), pedList, 0);

                menu.AddItem(pedl);
                if (!cf.IsAllowed(Permission.PASpawnNew))
                {
                    pedl.Enabled = false;
                    pedl.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    pedl.Description = "This option has been disabled by the server owner.";
                }
            }

            // Handle list selections.
            menu.OnListSelect += (sender, item, index) =>
            {
                int i = ((sender.CurrentSelection - 4) * 50) + index;
                string modelName = modelNames[i];
                if (cf.IsAllowed(Permission.PASpawnNew))
                {
                    cf.SetPlayerSkin(modelName);
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

        #region Ped Customization Menu
        /// <summary>
        /// Refresh/create the ped customization menu.
        /// </summary>
        private void RefreshCustomizationMenu()
        {
            // Remove any old items.
            pedTextures.MenuItems.Clear();

            #region Loop through all ped drawable variations and all ped props.
            for (var i = 0; i < 17; i++)
            {
                #region Ped Drawable Variations
                //if (i < 12)
                if (i < 12)
                {
                    // Get the drawable information.
                    var currentDrawable = GetPedDrawableVariation(PlayerPedId(), i);
                    var variations = GetNumberOfPedDrawableVariations(PlayerPedId(), i);
                    var textures = GetNumberOfPedTextureVariations(PlayerPedId(), i, currentDrawable);
                    // If there are any variations.
                    if (variations > 0)
                    {
                        // Loop through all of them and add them to the list.
                        var textureList = new List<dynamic>();
                        for (var x = 0; x < variations; x++)
                        {
                            textureList.Add("Item #" + x.ToString());
                        }
                        UIMenuListItem listItem = new UIMenuListItem($"{textureNames[i]}", textureList, currentDrawable,
                            $"Use ← & → to select a ~o~{textureNames[i]} Variation~s~, press ~r~enter~s~ to cycle through the available textures.");
                        pedTextures.AddItem(listItem);

                        // Manage list changes.
                        pedTextures.OnListChange += (sender2, item2, index2) =>
                        {
                            if (item2 == listItem)
                            {
                                SetPedComponentVariation(PlayerPedId(), sender2.CurrentSelection, index2, 0, 0);
                            }
                        };

                        // Manage list selections.
                        pedTextures.OnListSelect += (sender2, item2, index2) =>
                        {
                            if (item2 == listItem)
                            {
                                var currentTexture = GetPedTextureVariation(PlayerPedId(), sender2.CurrentSelection);
                                currentTexture = currentTexture == -1 ? 0 : currentTexture;
                                var totalTextures = GetNumberOfPedTextureVariations(PlayerPedId(), sender2.CurrentSelection, index2) - 1;

                                SetPedComponentVariation(PlayerPedId(), sender2.CurrentSelection, index2, (currentTexture < totalTextures ? currentTexture + 1 : 0), 0);
                            }
                        };
                    }
                    else
                    {
                        UIMenuItem placeholder = new UIMenuItem($"{textureNames[i]}");
                        placeholder.SetRightLabel("None");
                        placeholder.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                        placeholder.Enabled = false;
                        pedTextures.AddItem(placeholder);
                    }
                }
                #endregion
                #region Ped Props
                else
                {
                    // Variables setup.
                    var ii = i - 12;// 20;
                    if (ii > 2)
                    {
                        ii += 3;
                    }
                    var currentProp = GetPedPropIndex(PlayerPedId(), ii);
                    var props = GetNumberOfPedPropDrawableVariations(PlayerPedId(), ii);
                    // If there are any props.
                    if (props > 0)
                    {
                        // Loop through all of them and add them to lists.
                        var propsList = new List<dynamic>();
                        for (var x = 0; x < props; x++)
                        {
                            propsList.Add("Item #" + x.ToString());
                        }

                        // Add an "off" item to the list to allow the prop to be turned off.
                        propsList.Add("Off");

                        // Create and add the list item to the menu.
                        UIMenuListItem listItem = new UIMenuListItem($"{propNames[ii > 2 ? ii - 3 : ii]}", propsList, currentProp,
                            $"Use ← & → to select a ~o~{propNames[ii > 2 ? ii - 3 : ii]} Variation~s~, press ~r~enter~s~ to cycle through the available textures.");

                        pedTextures.AddItem(listItem);

                        // Handle list changes.
                        pedTextures.OnListChange += (sender2, item2, index2) =>
                        {
                            if (item2 == listItem)
                            {
                                if (index2 == propsList.Count - 1)
                                {
                                    ClearPedProp(PlayerPedId(), (sender2.CurrentSelection - textureNames.Count) + ((sender2.CurrentSelection - textureNames.Count) > 2 ? 3 : 0));
                                }
                                else
                                {
                                    SetPedPropIndex(PlayerPedId(), (sender2.CurrentSelection - textureNames.Count) + ((sender2.CurrentSelection - textureNames.Count) > 2 ? 3 : 0), index2, 0, true);
                                }
                            }
                        };

                        // Handle list selections.
                        pedTextures.OnListSelect += (sender2, item2, index2) =>
                        {
                            if (item2 == listItem)
                            {
                                if (index2 != propsList.Count - 1)
                                {
                                    var propTextureCount = GetNumberOfPedPropTextureVariations(PlayerPedId(), (sender2.CurrentSelection - textureNames.Count) + ((sender2.CurrentSelection - textureNames.Count) > 2 ? 3 : 0), index2);
                                    var propCurrentTexture = GetPedPropTextureIndex(PlayerPedId(), (sender2.CurrentSelection - textureNames.Count) + ((sender2.CurrentSelection - textureNames.Count) > 2 ? 3 : 0));
                                    SetPedPropIndex(PlayerPedId(), (sender2.CurrentSelection - textureNames.Count) + ((sender2.CurrentSelection - textureNames.Count) > 2 ? 3 : 0), index2, (propCurrentTexture + 1 < propTextureCount ? propCurrentTexture + 1 : 0), true);
                                }
                            }
                        };
                    }
                    // If there's not enough variations available (none at all) then add a placeholder to let them know this option is unavailable.
                    else
                    {
                        UIMenuItem placeholder = new UIMenuItem($"{propNames[ii > 2 ? ii - 3 : ii]}");
                        placeholder.SetRightLabel("None");
                        placeholder.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                        placeholder.Enabled = false;
                        pedTextures.AddItem(placeholder);
                    }
                }
                #endregion
            }
            #endregion

            // Refresh index and update scaleform to make everything pretty.
            pedTextures.RefreshIndex();
            pedTextures.UpdateScaleform();

        }

        #region Textures & Props
        private List<string> textureNames = new List<string>()
        {
            "Head",
            "Mask / Facial Hair",
            "Hair Style / Color",
            "Hands / Upper Body",
            "Legs / Pants",
            "Bags / Parachutes",
            "Shoes",
            "Neck / Scarfs",
            "Shirt / Accessory",
            "Body Armor / Accessory 2",
            "Badges / Logos",
            "Shirt Overlay / Jackets",
        };

        private List<string> propNames = new List<string>()
        {
            "Hats / Helmets", // id 0
            "Glasses", // id 1
            "Misc", // id 2
            "Watches", // id 6
            "Bracelets", // id 7
        };
        #endregion
        #endregion

        /// <summary>
        /// Refresh the spawn saved peds menu.
        /// </summary>
        private void RefreshSpawnSavedPedMenu()
        {
            spawnSavedPedMenu.MenuItems.Clear();
            int findHandle = StartFindKvp("ped_");
            List<string> savesFound = new List<string>();
            while (true)
            {
                var saveName = FindKvp(findHandle);
                if (saveName != null && saveName != "" && saveName != "NULL")
                {
                    savesFound.Add(saveName);
                }
                else
                {
                    break;
                }
            }

            foreach (var savename in savesFound)
            {
                UIMenuItem savedPedBtn = new UIMenuItem(savename.Substring(4), "Spawn this saved ped.");
                spawnSavedPedMenu.AddItem(savedPedBtn);
            }

            spawnSavedPedMenu.OnItemSelect += (sender, item, idex) =>
            {
                var name = item.Text.ToString();
                cf.LoadSavedPed(name);
            };

            spawnSavedPedMenu.RefreshIndex();
            spawnSavedPedMenu.UpdateScaleform();
        }

        /// <summary>
        /// Refresh the delete saved peds menu.
        /// </summary>
        private void RefreshDeleteSavedPedMenu()
        {
            deleteSavedPedMenu.MenuItems.Clear();
            int findHandle = StartFindKvp("ped_");
            List<string> savesFound = new List<string>();
            while (true)
            {
                var saveName = FindKvp(findHandle);
                if (saveName != null && saveName != "" && saveName != "NULL")
                {
                    savesFound.Add(saveName);
                }
                else
                {
                    break;
                }
            }
            foreach (var savename in savesFound)
            {
                UIMenuItem deleteSavedPed = new UIMenuItem(savename.Substring(4), "~r~Delete ~s~this saved ped, this action can ~r~NOT~s~ be undone!");
                deleteSavedPed.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
                deleteSavedPedMenu.AddItem(deleteSavedPed);
            }

            deleteSavedPedMenu.OnItemSelect += (sender, item, idex) =>
            {
                var name = item.Text.ToString();
                sm.DeleteSavedDictionary("ped_" + name);
                Notify.Success("Saved ped deleted.");
                deleteSavedPedMenu.GoBack();
            };
            deleteSavedPedMenu.RefreshIndex();
            deleteSavedPedMenu.UpdateScaleform();
        }


        #region Model Names
        private List<string> modelNames = new List<string>()
        {
            "mp_f_freemode_01",
            "mp_m_freemode_01",
            "a_f_m_beach_01",
            "a_f_m_bevhills_01",
            "a_f_m_bevhills_02",
            "a_f_m_bodybuild_01",
            "a_f_m_business_02",
            "a_f_m_downtown_01",
            "a_f_m_eastsa_01",
            "a_f_m_eastsa_02",
            "a_f_m_fatbla_01",
            "a_f_m_fatcult_01",
            "a_f_m_fatwhite_01",
            "a_f_m_ktown_01",
            "a_f_m_ktown_02",
            "a_f_m_prolhost_01",
            "a_f_m_salton_01",
            "a_f_m_skidrow_01",
            "a_f_m_soucentmc_01",
            "a_f_m_soucent_01",
            "a_f_m_soucent_02",
            "a_f_m_tourist_01",
            "a_f_m_trampbeac_01",
            "a_f_m_tramp_01",
            "a_f_o_genstreet_01",
            "a_f_o_indian_01",
            "a_f_o_ktown_01",
            "a_f_o_salton_01",
            "a_f_o_soucent_01",
            "a_f_o_soucent_02",
            "a_f_y_beach_01",
            "a_f_y_bevhills_01",
            "a_f_y_bevhills_02",
            "a_f_y_bevhills_03",
            "a_f_y_bevhills_04",
            "a_f_y_business_01",
            "a_f_y_business_02",
            "a_f_y_business_03",
            "a_f_y_business_04",
            "a_f_y_eastsa_01",
            "a_f_y_eastsa_02",
            "a_f_y_eastsa_03",
            "a_f_y_epsilon_01",
            "a_f_y_fitness_01",
            "a_f_y_fitness_02",
            "a_f_y_genhot_01",
            "a_f_y_golfer_01",
            "a_f_y_hiker_01",
            "a_f_y_hippie_01",
            "a_f_y_hipster_01",
            "a_f_y_hipster_02",
            "a_f_y_hipster_03",
            "a_f_y_hipster_04",
            "a_f_y_indian_01",
            "a_f_y_juggalo_01",
            "a_f_y_runner_01",
            "a_f_y_rurmeth_01",
            "a_f_y_scdressy_01",
            "a_f_y_skater_01",
            "a_f_y_soucent_01",
            "a_f_y_soucent_02",
            "a_f_y_soucent_03",
            "a_f_y_tennis_01",
            "a_f_y_topless_01",
            "a_f_y_tourist_01",
            "a_f_y_tourist_02",
            "a_f_y_vinewood_01",
            "a_f_y_vinewood_02",
            "a_f_y_vinewood_03",
            "a_f_y_vinewood_04",
            "a_f_y_yoga_01",
            "a_m_m_acult_01",
            "a_m_m_afriamer_01",
            "a_m_m_beach_01",
            "a_m_m_beach_02",
            "a_m_m_bevhills_01",
            "a_m_m_bevhills_02",
            "a_m_m_business_01",
            "a_m_m_eastsa_01",
            "a_m_m_eastsa_02",
            "a_m_m_farmer_01",
            "a_m_m_fatlatin_01",
            "a_m_m_genfat_01",
            "a_m_m_genfat_02",
            "a_m_m_golfer_01",
            "a_m_m_hasjew_01",
            "a_m_m_hillbilly_01",
            "a_m_m_hillbilly_02",
            "a_m_m_indian_01",
            "a_m_m_ktown_01",
            "a_m_m_malibu_01",
            "a_m_m_mexcntry_01",
            "a_m_m_mexlabor_01",
            "a_m_m_og_boss_01",
            "a_m_m_paparazzi_01",
            "a_m_m_polynesian_01",
            "a_m_m_prolhost_01",
            "a_m_m_rurmeth_01",
            "a_m_m_salton_01",
            "a_m_m_salton_02",
            "a_m_m_salton_03",
            "a_m_m_salton_04",
            "a_m_m_skater_01",
            "a_m_m_skidrow_01",
            "a_m_m_socenlat_01",
            "a_m_m_soucent_01",
            "a_m_m_soucent_02",
            "a_m_m_soucent_03",
            "a_m_m_soucent_04",
            "a_m_m_stlat_02",
            "a_m_m_tennis_01",
            "a_m_m_tourist_01",
            "a_m_m_trampbeac_01",
            "a_m_m_tramp_01",
            "a_m_m_tranvest_01",
            "a_m_m_tranvest_02",
            "a_m_o_acult_01",
            "a_m_o_acult_02",
            "a_m_o_beach_01",
            "a_m_o_genstreet_01",
            "a_m_o_ktown_01",
            "a_m_o_salton_01",
            "a_m_o_soucent_01",
            "a_m_o_soucent_02",
            "a_m_o_soucent_03",
            "a_m_o_tramp_01",
            "a_m_y_acult_01",
            "a_m_y_acult_02",
            "a_m_y_beachvesp_01",
            "a_m_y_beachvesp_02",
            "a_m_y_beach_01",
            "a_m_y_beach_02",
            "a_m_y_beach_03",
            "a_m_y_bevhills_01",
            "a_m_y_bevhills_02",
            "a_m_y_breakdance_01",
            "a_m_y_busicas_01",
            "a_m_y_business_01",
            "a_m_y_business_02",
            "a_m_y_business_03",
            "a_m_y_cyclist_01",
            "a_m_y_dhill_01",
            "a_m_y_downtown_01",
            "a_m_y_eastsa_01",
            "a_m_y_eastsa_02",
            "a_m_y_epsilon_01",
            "a_m_y_epsilon_02",
            "a_m_y_gay_01",
            "a_m_y_gay_02",
            "a_m_y_genstreet_01",
            "a_m_y_genstreet_02",
            "a_m_y_golfer_01",
            "a_m_y_hasjew_01",
            "a_m_y_hiker_01",
            "a_m_y_hippy_01",
            "a_m_y_hipster_01",
            "a_m_y_hipster_02",
            "a_m_y_hipster_03",
            "a_m_y_indian_01",
            "a_m_y_jetski_01",
            "a_m_y_juggalo_01",
            "a_m_y_ktown_01",
            "a_m_y_ktown_02",
            "a_m_y_latino_01",
            "a_m_y_methhead_01",
            "a_m_y_mexthug_01",
            "a_m_y_motox_01",
            "a_m_y_motox_02",
            "a_m_y_musclbeac_01",
            "a_m_y_musclbeac_02",
            "a_m_y_polynesian_01",
            "a_m_y_roadcyc_01",
            "a_m_y_runner_01",
            "a_m_y_runner_02",
            "a_m_y_salton_01",
            "a_m_y_skater_01",
            "a_m_y_skater_02",
            "a_m_y_soucent_01",
            "a_m_y_soucent_02",
            "a_m_y_soucent_03",
            "a_m_y_soucent_04",
            "a_m_y_stbla_01",
            "a_m_y_stbla_02",
            "a_m_y_stlat_01",
            "a_m_y_stwhi_01",
            "a_m_y_stwhi_02",
            "a_m_y_sunbathe_01",
            "a_m_y_surfer_01",
            "a_m_y_vindouche_01",
            "a_m_y_vinewood_01",
            "a_m_y_vinewood_02",
            "a_m_y_vinewood_03",
            "a_m_y_vinewood_04",
            "a_m_y_yoga_01",
            "csb_abigail",
            "csb_anita",
            "csb_anton",
            "csb_ballasog",
            "csb_bride",
            "csb_burgerdrug",
            "csb_car3guy1",
            "csb_car3guy2",
            "csb_chef",
            "csb_chin_goon",
            "csb_cletus",
            "csb_cop",
            "csb_customer",
            "csb_denise_friend",
            "csb_fos_rep",
            "csb_groom",
            "csb_grove_str_dlr",
            "csb_g",
            "csb_hao",
            "csb_hugh",
            "csb_imran",
            "csb_janitor",
            "csb_maude",
            "csb_mweather",
            "csb_ortega",
            "csb_oscar",
            "csb_porndudes",
            "csb_prologuedriver",
            "csb_prolsec",
            "csb_ramp_gang",
            "csb_ramp_hic",
            "csb_ramp_hipster",
            "csb_ramp_marine",
            "csb_ramp_mex",
            "csb_reporter",
            "csb_roccopelosi",
            "csb_screen_writer",
            "csb_stripper_01",
            "csb_stripper_02",
            "csb_tonya",
            "csb_trafficwarden",
            "g_f_y_ballas_01",
            "g_f_y_families_01",
            "g_f_y_lost_01",
            "g_f_y_vagos_01",
            "g_m_m_armboss_01",
            "g_m_m_armgoon_01",
            "g_m_m_armlieut_01",
            "g_m_m_chemwork_01",
            "g_m_m_chiboss_01",
            "g_m_m_chicold_01",
            "g_m_m_chigoon_01",
            "g_m_m_chigoon_02",
            "g_m_m_korboss_01",
            "g_m_m_mexboss_01",
            "g_m_m_mexboss_02",
            "g_m_y_armgoon_02",
            "g_m_y_azteca_01",
            "g_m_y_ballaeast_01",
            "g_m_y_ballaorig_01",
            "g_m_y_ballasout_01",
            "g_m_y_famca_01",
            "g_m_y_famdnf_01",
            "g_m_y_famfor_01",
            "g_m_y_korean_01",
            "g_m_y_korean_02",
            "g_m_y_korlieut_01",
            "g_m_y_lost_01",
            "g_m_y_lost_02",
            "g_m_y_lost_03",
            "g_m_y_mexgang_01",
            "g_m_y_mexgoon_01",
            "g_m_y_mexgoon_02",
            "g_m_y_mexgoon_03",
            "g_m_y_pologoon_01",
            "g_m_y_pologoon_02",
            "g_m_y_salvaboss_01",
            "g_m_y_salvagoon_01",
            "g_m_y_salvagoon_02",
            "g_m_y_salvagoon_03",
            "g_m_y_strpunk_01",
            "g_m_y_strpunk_02",
            "hc_driver",
            "hc_gunman",
            "hc_hacker",
            "ig_abigail",
            "ig_amandatownley",
            "ig_andreas",
            "ig_ashley",
            "ig_ballasog",
            "ig_bankman",
            "ig_barry",
            "ig_bestmen",
            "ig_beverly",
            "ig_brad",
            "ig_bride",
            "ig_car3guy1",
            "ig_car3guy2",
            "ig_casey",
            "ig_chef",
            "ig_chengsr",
            "ig_chrisformage",
            "ig_claypain",
            "ig_clay",
            "ig_cletus",
            "ig_dale",
            "ig_davenorton",
            "ig_denise",
            "ig_devin",
            "ig_dom",
            "ig_dreyfuss",
            "ig_drfriedlander",
            "ig_fabien",
            "ig_fbisuit_01",
            "ig_floyd",
            "ig_groom",
            "ig_hao",
            "ig_hunter",
            "ig_janet",
            "ig_jay_norris",
            "ig_jewelass",
            "ig_jimmyboston",
            "ig_jimmydisanto",
            "ig_joeminuteman",
            "ig_johnnyklebitz",
            "ig_josef",
            "ig_josh",
            "ig_kerrymcintosh",
            "ig_lamardavis",
            "ig_lazlow",
            "ig_lestercrest",
            "ig_lifeinvad_01",
            "ig_lifeinvad_02",
            "ig_magenta",
            "ig_manuel",
            "ig_marnie",
            "ig_maryann",
            "ig_maude",
            "ig_michelle",
            "ig_milton",
            "ig_molly",
            "ig_mrk",
            "ig_mrsphillips",
            "ig_mrs_thornhill",
            "ig_natalia",
            "ig_nervousron",
            "ig_nigel",
            "ig_old_man1a",
            "ig_old_man2",
            "ig_omega",
            "ig_oneil",
            "ig_orleans",
            "ig_ortega",
            "ig_paper",
            "ig_patricia",
            "ig_priest",
            "ig_prolsec_02",
            "ig_ramp_gang",
            "ig_ramp_hic",
            "ig_ramp_hipster",
            "ig_ramp_mex",
            "ig_roccopelosi",
            "ig_russiandrunk",
            "ig_screen_writer",
            "ig_siemonyetarian",
            "ig_solomon",
            "ig_stevehains",
            "ig_stretch",
            "ig_talina",
            "ig_tanisha",
            "ig_taocheng",
            "ig_taostranslator",
            "ig_tenniscoach",
            "ig_terry",
            "ig_tomepsilon",
            "ig_tonya",
            "ig_tracydisanto",
            "ig_trafficwarden",
            "ig_tylerdix",
            "ig_wade",
            "ig_zimbor",
            "mp_f_deadhooker",
            "mp_f_misty_01",
            "mp_f_stripperlite",
            "mp_g_m_pros_01",
            "mp_m_claude_01",
            "mp_m_exarmy_01",
            "mp_m_famdd_01",
            "mp_m_fibsec_01",
            "mp_m_marston_01",
            "mp_m_niko_01",
            "mp_m_shopkeep_01",
            "mp_s_m_armoured_01",
            "player_one",
            "player_two",
            "player_zero",
            "s_f_m_fembarber",
            "s_f_m_maid_01",
            "s_f_m_shop_high",
            "s_f_m_sweatshop_01",
            "s_f_y_airhostess_01",
            "s_f_y_bartender_01",
            "s_f_y_baywatch_01",
            "s_f_y_cop_01",
            "s_f_y_factory_01",
            "s_f_y_hooker_01",
            "s_f_y_hooker_02",
            "s_f_y_hooker_03",
            "s_f_y_migrant_01",
            "s_f_y_movprem_01",
            "s_f_y_ranger_01",
            "s_f_y_scrubs_01",
            "s_f_y_sheriff_01",
            "s_f_y_shop_low",
            "s_f_y_shop_mid",
            "s_f_y_stripperlite",
            "s_f_y_stripper_01",
            "s_f_y_stripper_02",
            "s_f_y_sweatshop_01",
            "s_m_m_ammucountry",
            "s_m_m_armoured_01",
            "s_m_m_armoured_02",
            "s_m_m_autoshop_01",
            "s_m_m_autoshop_02",
            "s_m_m_bouncer_01",
            "s_m_m_chemsec_01",
            "s_m_m_ciasec_01",
            "s_m_m_cntrybar_01",
            "s_m_m_dockwork_01",
            "s_m_m_doctor_01",
            "s_m_m_fiboffice_01",
            "s_m_m_fiboffice_02",
            "s_m_m_gaffer_01",
            "s_m_m_gardener_01",
            "s_m_m_gentransport",
            "s_m_m_hairdress_01",
            "s_m_m_highsec_01",
            "s_m_m_highsec_02",
            "s_m_m_janitor",
            "s_m_m_lathandy_01",
            "s_m_m_lifeinvad_01",
            "s_m_m_linecook",
            "s_m_m_lsmetro_01",
            "s_m_m_mariachi_01",
            "s_m_m_marine_01",
            "s_m_m_marine_02",
            "s_m_m_migrant_01",
            "s_m_m_movalien_01",
            "s_m_m_movprem_01",
            "s_m_m_movspace_01",
            "s_m_m_paramedic_01",
            "s_m_m_pilot_01",
            "s_m_m_pilot_02",
            "s_m_m_postal_01",
            "s_m_m_postal_02",
            "s_m_m_prisguard_01",
            "s_m_m_scientist_01",
            "s_m_m_security_01",
            "s_m_m_snowcop_01",
            "s_m_m_strperf_01",
            "s_m_m_strpreach_01",
            "s_m_m_strvend_01",
            "s_m_m_trucker_01",
            "s_m_m_ups_01",
            "s_m_m_ups_02",
            "s_m_o_busker_01",
            "s_m_y_airworker",
            "s_m_y_ammucity_01",
            "s_m_y_armymech_01",
            "s_m_y_autopsy_01",
            "s_m_y_barman_01",
            "s_m_y_baywatch_01",
            "s_m_y_blackops_01",
            "s_m_y_blackops_02",
            "s_m_y_busboy_01",
            "s_m_y_chef_01",
            "s_m_y_clown_01",
            "s_m_y_construct_01",
            "s_m_y_construct_02",
            "s_m_y_cop_01",
            "s_m_y_dealer_01",
            "s_m_y_devinsec_01",
            "s_m_y_dockwork_01",
            "s_m_y_doorman_01",
            "s_m_y_dwservice_01",
            "s_m_y_dwservice_02",
            "s_m_y_factory_01",
            "s_m_y_fireman_01",
            "s_m_y_garbage",
            "s_m_y_grip_01",
            "s_m_y_hwaycop_01",
            "s_m_y_marine_01",
            "s_m_y_marine_02",
            "s_m_y_marine_03",
            "s_m_y_mime",
            "s_m_y_pestcont_01",
            "s_m_y_pilot_01",
            "s_m_y_prismuscl_01",
            "s_m_y_prisoner_01",
            "s_m_y_ranger_01",
            "s_m_y_robber_01",
            "s_m_y_sheriff_01",
            "s_m_y_shop_mask",
            "s_m_y_strvend_01",
            "s_m_y_swat_01",
            "s_m_y_uscg_01",
            "s_m_y_valet_01",
            "s_m_y_waiter_01",
            "s_m_y_winclean_01",
            "s_m_y_xmech_01",
            "s_m_y_xmech_02",
            "u_f_m_corpse_01",
            "u_f_m_miranda",
            "u_f_m_promourn_01",
            "u_f_o_moviestar",
            "u_f_o_prolhost_01",
            "u_f_y_bikerchic",
            "u_f_y_comjane",
            "u_f_y_corpse_01",
            "u_f_y_corpse_02",
            "u_f_y_hotposh_01",
            "u_f_y_jewelass_01",
            "u_f_y_mistress",
            "u_f_y_poppymich",
            "u_f_y_princess",
            "u_f_y_spyactress",
            "u_m_m_aldinapoli",
            "u_m_m_bankman",
            "u_m_m_bikehire_01",
            "u_m_m_fibarchitect",
            "u_m_m_filmdirector",
            "u_m_m_glenstank_01",
            "u_m_m_griff_01",
            "u_m_m_jesus_01",
            "u_m_m_jewelsec_01",
            "u_m_m_jewelthief",
            "u_m_m_markfost",
            "u_m_m_partytarget",
            "u_m_m_prolsec_01",
            "u_m_m_promourn_01",
            "u_m_m_rivalpap",
            "u_m_m_spyactor",
            "u_m_m_willyfist",
            "u_m_o_finguru_01",
            "u_m_o_taphillbilly",
            "u_m_o_tramp_01",
            "u_m_y_abner",
            "u_m_y_antonb",
            "u_m_y_babyd",
            "u_m_y_baygor",
            "u_m_y_burgerdrug_01",
            "u_m_y_chip",
            "u_m_y_cyclist_01",
            "u_m_y_fibmugger_01",
            "u_m_y_guido_01",
            "u_m_y_gunvend_01",
            "u_m_y_hippie_01",
            "u_m_y_imporage",
            "u_m_y_justin",
            "u_m_y_mani",
            "u_m_y_militarybum",
            "u_m_y_paparazzi",
            "u_m_y_party_01",
            "u_m_y_pogo_01",
            "u_m_y_prisoner_01",
            "u_m_y_proldriver_01",
            "u_m_y_rsranger_01",
            "u_m_y_sbike",
            "u_m_y_staggrm_01",
            "u_m_y_tattoo_01",
            "u_m_y_zombie_01"
        };
        #endregion
    }
}
