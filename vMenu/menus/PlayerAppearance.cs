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
    public class PlayerAppearance
    {
        private Menu menu;

        private Menu pedCustomizationMenu;
        private Menu savedPedsMenu;
        private Menu spawnPedsMenu;
        private Menu addonPedsMenu;
        private Menu mainPedsMenu = new Menu("Main Peds", "Spawn A Ped");
        private Menu animalsPedsMenu = new Menu("Animals", "Spawn A Ped");
        private Menu malePedsMenu = new Menu("Male Peds", "Spawn A Ped");
        private Menu femalePedsMenu = new Menu("Female Peds", "Spawn A Ped");

        public static Dictionary<string, uint> AddonPeds;

        public static int ClothingAnimationType { get; set; } = UserDefaults.PAClothingAnimationType;

        private Dictionary<MenuListItem, int> drawablesMenuListItems = new Dictionary<MenuListItem, int>();
        private Dictionary<MenuListItem, int> propsMenuListItems = new Dictionary<MenuListItem, int>();

        #region create the menu
        /// <summary>
        /// Creates the menu(s).
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Player Appearance");
            savedPedsMenu = new Menu(Game.Player.Name, "Saved Peds");
            pedCustomizationMenu = new Menu(Game.Player.Name, "Customize Saved Ped");
            spawnPedsMenu = new Menu(Game.Player.Name, "Spawn Ped");
            addonPedsMenu = new Menu(Game.Player.Name, "Addon Peds");


            // Add the (submenus) to the menu pool.
            MenuController.AddSubmenu(menu, pedCustomizationMenu);
            MenuController.AddSubmenu(menu, savedPedsMenu);
            MenuController.AddSubmenu(menu, spawnPedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, addonPedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, mainPedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, animalsPedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, malePedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, femalePedsMenu);

            // Create the menu items.
            MenuItem pedCustomization = new MenuItem("Ped Customization", "Modify your ped's appearance.") { Label = "→→→" };
            MenuItem savedPedsBtn = new MenuItem("Saved Peds", "Edit, rename, clone, spawn or delete saved peds.") { Label = "→→→" };
            MenuItem spawnPedsBtn = new MenuItem("Spawn Peds", "Change ped model by selecting one from the list or by selecting an addon ped from the list.") { Label = "→→→" };


            MenuItem spawnByNameBtn = new MenuItem("Spawn By Name", "Spawn a ped by entering it's name manually.");
            MenuItem addonPedsBtn = new MenuItem("Addon Peds", "Spawn a ped from the addon peds list.") { Label = "→→→" };
            MenuItem mainPedsBtn = new MenuItem("Main Peds", "Select a new ped from the main player-peds list.") { Label = "→→→" };
            MenuItem animalPedsBtn = new MenuItem("Animals", "Become an animal. ~r~Note this may crash your own or other players' game if you die as an animal, godmode can NOT prevent this.") { Label = "→→→" };
            MenuItem malePedsBtn = new MenuItem("Male Peds", "Select a male ped.") { Label = "→→→" };
            MenuItem femalePedsBtn = new MenuItem("Female Peds", "Select a female ped.") { Label = "→→→" };

            List<string> walkstyles = new List<string>() { "Normal", "Injured", "Tough Guy", "Femme", "Gangster", "Posh", "Sexy", "Business", "Drunk", "Hipster" };
            MenuListItem walkingStyle = new MenuListItem("Walking Style", walkstyles, 0, "Change the walking style of your current ped. " +
                "You need to re-apply this each time you change player model or load a saved ped.");

            List<string> clothingGlowAnimations = new List<string>() { "On", "Off", "Fade", "Flash" };
            MenuListItem clothingGlowType = new MenuListItem("Illuminated Clothing Style", clothingGlowAnimations, ClothingAnimationType, "Set the style of the animation used on your player's illuminated clothing items.");

            // Add items to the menu.
            menu.AddMenuItem(pedCustomization);
            menu.AddMenuItem(savedPedsBtn);
            menu.AddMenuItem(spawnPedsBtn);

            menu.AddMenuItem(walkingStyle);
            menu.AddMenuItem(clothingGlowType);

            if (IsAllowed(Permission.PACustomize))
            {
                MenuController.BindMenuItem(menu, pedCustomizationMenu, pedCustomization);
            }
            else
            {
                menu.RemoveMenuItem(pedCustomization);
            }

            // always allowed
            MenuController.BindMenuItem(menu, savedPedsMenu, savedPedsBtn);
            MenuController.BindMenuItem(menu, spawnPedsMenu, spawnPedsBtn);

            if (AddonPeds != null && AddonPeds.Count > 0 && IsAllowed(Permission.PAAddonPeds))
            {
                spawnPedsMenu.AddMenuItem(addonPedsBtn);
                MenuController.BindMenuItem(spawnPedsMenu, addonPedsMenu, addonPedsBtn);

                var addons = AddonPeds.ToList();

                addons.Sort((a, b) => a.Key.ToLower().CompareTo(b.Key.ToLower()));

                foreach (var ped in addons)
                {
                    string name = GetLabelText(ped.Key);
                    if (string.IsNullOrEmpty(name) || name == "NULL")
                    {
                        name = ped.Key;
                    }

                    MenuItem pedBtn = new MenuItem(ped.Key, "Click to spawn this model.") { Label = $"({name})" };

                    if (!IsModelInCdimage(ped.Value) || !IsModelAPed(ped.Value))
                    {
                        pedBtn.Enabled = false;
                        pedBtn.LeftIcon = MenuItem.Icon.LOCK;
                        pedBtn.Description = "This ped is not (correctly) streamed. If you are the server owner, please ensure that the ped name and model are valid!";
                    }

                    addonPedsMenu.AddMenuItem(pedBtn);
                }

                addonPedsMenu.OnItemSelect += async (sender, item, index) =>
                {
                    await SetPlayerSkin((uint)GetHashKey(item.Text), new PedInfo() { version = -1 }, true);
                };
            }

            if (IsAllowed(Permission.PASpawnNew))
            {
                spawnPedsMenu.AddMenuItem(spawnByNameBtn);
                spawnPedsMenu.AddMenuItem(mainPedsBtn);
                spawnPedsMenu.AddMenuItem(animalPedsBtn);
                spawnPedsMenu.AddMenuItem(malePedsBtn);
                spawnPedsMenu.AddMenuItem(femalePedsBtn);

                MenuController.BindMenuItem(spawnPedsMenu, mainPedsMenu, mainPedsBtn);
                MenuController.BindMenuItem(spawnPedsMenu, animalsPedsMenu, animalPedsBtn);
                MenuController.BindMenuItem(spawnPedsMenu, malePedsMenu, malePedsBtn);
                MenuController.BindMenuItem(spawnPedsMenu, femalePedsMenu, femalePedsBtn);

                foreach (var animal in animalModels)
                {
                    MenuItem animalBtn = new MenuItem(animal.Key, "Click to spawn this animal.") { Label = $"({animal.Value})" };
                    animalsPedsMenu.AddMenuItem(animalBtn);
                }

                animalsPedsMenu.OnItemSelect += async (sender, item, index) =>
                {
                    uint model = (uint)GetHashKey(item.Text);
                    RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
                    await SetPlayerSkin(model, new PedInfo() { version = -1 }, false); // weapons need to be removed because for animals some 'invalid' weapons need to be forced
                };

                spawnPedsMenu.OnItemSelect += async (sender, item, index) =>
                {
                    if (item == spawnByNameBtn)
                    {
                        string model = await GetUserInput("Ped Model Name", 30);
                        if (!string.IsNullOrEmpty(model))
                        {
                            await SetPlayerSkin(model, new PedInfo() { version = -1 }, true);
                        }
                        else
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }
                    }
                };
            }


            // Handle list selections.
            menu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
            {
                if (item == walkingStyle)
                {
                    //if (MainMenu.DebugMode) Subtitle.Custom("Ped is: " + IsPedMale(Game.PlayerPed.Handle));
                    SetWalkingStyle(walkstyles[listIndex].ToString());
                }
                if (item == clothingGlowType)
                {
                    ClothingAnimationType = item.ListIndex;
                }
            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == pedCustomization)
                {
                    RefreshCustomizationMenu();
                }
            };


            #region ped drawable list changes
            // Manage list changes.
            pedCustomizationMenu.OnListIndexChange += (sender, item, oldListIndex, newListIndex, itemIndex) =>
            {
                if (drawablesMenuListItems.ContainsKey(item))
                {
                    int drawableID = drawablesMenuListItems[item];
                    SetPedComponentVariation(Game.PlayerPed.Handle, drawableID, newListIndex, 0, 0);
                }
                else if (propsMenuListItems.ContainsKey(item))
                {
                    int propID = propsMenuListItems[item];
                    if (newListIndex == 0)
                    {
                        SetPedPropIndex(Game.PlayerPed.Handle, propID, newListIndex - 1, 0, false);
                        ClearPedProp(Game.PlayerPed.Handle, propID);
                    }
                    else
                    {
                        SetPedPropIndex(Game.PlayerPed.Handle, propID, newListIndex - 1, 0, true);
                    }
                    if (propID == 0)
                    {
                        int component = GetPedPropIndex(Game.PlayerPed.Handle, 0);      // helmet index
                        int texture = GetPedPropTextureIndex(Game.PlayerPed.Handle, 0); // texture
                        int compHash = GetHashNameForProp(Game.PlayerPed.Handle, 0, component, texture); // prop combination hash
                        if (N_0xd40aac51e8e4c663((uint)compHash) > 0) // helmet has visor. 
                        {
                            if (!IsHelpMessageBeingDisplayed())
                            {
                                BeginTextCommandDisplayHelp("TWOSTRINGS");
                                AddTextComponentSubstringPlayerName("Hold ~INPUT_SWITCH_VISOR~ to flip your helmet visor open or closed");
                                AddTextComponentSubstringPlayerName("when on foot or on a motorcycle and when vMenu is closed.");
                                EndTextCommandDisplayHelp(0, false, true, 6000);
                            }
                        }
                    }

                }
            };

            // Manage list selections.
            pedCustomizationMenu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
            {
                if (drawablesMenuListItems.ContainsKey(item)) // drawable
                {
                    int currentDrawableID = drawablesMenuListItems[item];
                    int currentTextureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, currentDrawableID);
                    int maxDrawableTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, currentDrawableID, listIndex) - 1;

                    if (currentTextureIndex == -1)
                        currentTextureIndex = 0;

                    int newTexture = currentTextureIndex < maxDrawableTextures ? currentTextureIndex + 1 : 0;

                    SetPedComponentVariation(Game.PlayerPed.Handle, currentDrawableID, listIndex, newTexture, 0);
                }
                else if (propsMenuListItems.ContainsKey(item)) // prop
                {
                    int currentPropIndex = propsMenuListItems[item];
                    int currentPropVariationIndex = GetPedPropIndex(Game.PlayerPed.Handle, currentPropIndex);
                    int currentPropTextureVariation = GetPedPropTextureIndex(Game.PlayerPed.Handle, currentPropIndex);
                    int maxPropTextureVariations = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, currentPropIndex, currentPropVariationIndex) - 1;

                    int newPropTextureVariationIndex = currentPropTextureVariation < maxPropTextureVariations ? currentPropTextureVariation + 1 : 0;
                    SetPedPropIndex(Game.PlayerPed.Handle, currentPropIndex, currentPropVariationIndex, newPropTextureVariationIndex, true);
                }
            };
            #endregion
        }
        #endregion

        #region get the menu
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
        #endregion

        #region Ped Customization Menu
        ///// <summary>
        ///// Refresh/create the ped customization menu.
        ///// </summary>
        private void RefreshCustomizationMenu()
        {
            drawablesMenuListItems.Clear();
            propsMenuListItems.Clear();
            pedCustomizationMenu.ClearMenuItems();

            #region Ped Drawables
            for (int drawable = 0; drawable < 12; drawable++)
            {
                int currentDrawable = GetPedDrawableVariation(Game.PlayerPed.Handle, drawable);
                int maxVariations = GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, drawable);
                int maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, drawable, currentDrawable);

                if (maxVariations > 0)
                {
                    List<string> drawableTexturesList = new List<string>();

                    for (int i = 0; i < maxVariations; i++)
                    {
                        drawableTexturesList.Add($"Drawable #{i + 1} (of {maxVariations})");
                    }

                    MenuListItem drawableTextures = new MenuListItem($"{textureNames[drawable]}", drawableTexturesList, currentDrawable, $"Use ← & → to select a ~o~{textureNames[drawable]} Variation~s~, press ~r~enter~s~ to cycle through the available textures.");
                    drawablesMenuListItems.Add(drawableTextures, drawable);
                    pedCustomizationMenu.AddMenuItem(drawableTextures);
                }
            }
            #endregion

            #region Ped Props
            for (int tmpProp = 0; tmpProp < 5; tmpProp++)
            {
                int realProp = tmpProp > 2 ? tmpProp + 3 : tmpProp;

                int currentProp = GetPedPropIndex(Game.PlayerPed.Handle, realProp);
                int maxPropVariations = GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, realProp);

                if (maxPropVariations > 0)
                {
                    List<string> propTexturesList = new List<string>();

                    propTexturesList.Add($"Prop #1 (of {maxPropVariations + 1})");
                    for (int i = 0; i < maxPropVariations; i++)
                    {
                        propTexturesList.Add($"Prop #{i + 2} (of {maxPropVariations + 1})");
                    }


                    MenuListItem propTextures = new MenuListItem($"{propNames[tmpProp]}", propTexturesList, currentProp + 1, $"Use ← & → to select a ~o~{propNames[tmpProp]} Variation~s~, press ~r~enter~s~ to cycle through the available textures.");
                    propsMenuListItems.Add(propTextures, realProp);
                    pedCustomizationMenu.AddMenuItem(propTextures);

                }
            }
            pedCustomizationMenu.RefreshIndex();
            #endregion
        }

        #region Textures & Props
        private readonly List<string> textureNames = new List<string>()
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

        private readonly List<string> propNames = new List<string>()
        {
            "Hats / Helmets", // id 0
            "Glasses", // id 1
            "Misc", // id 2
            "Watches", // id 6
            "Bracelets", // id 7
        };
        #endregion
        #endregion


        #region saved peds menus
        ///// <summary>
        ///// Refresh the spawn saved peds menu.
        ///// </summary>
        //private void RefreshSpawnSavedPedMenu()
        //{
        //    spawnSavedPedMenu.ClearMenuItems();
        //    int findHandle = StartFindKvp("ped_");
        //    List<string> savesFound = new List<string>();
        //    var i = 0;
        //    while (true)
        //    {
        //        i++;
        //        var saveName = FindKvp(findHandle);
        //        if (saveName != null && saveName != "" && saveName != "NULL")
        //        {
        //            // It's already the new format, so add it.
        //            savesFound.Add(saveName);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    var items = new List<string>();
        //    foreach (var savename in savesFound)
        //    {
        //        if (savename.Length > 4)
        //        {
        //            var title = savename.Substring(4);
        //            if (!items.Contains(title))
        //            {
        //                MenuItem savedPedBtn = new MenuItem(title, "Spawn this saved ped.");
        //                spawnSavedPedMenu.AddMenuItem(savedPedBtn);
        //                items.Add(title);
        //            }
        //        }
        //    }

        //    // Sort the menu items (case IN-sensitive) by name.
        //    spawnSavedPedMenu.SortMenuItems((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));

        //    spawnSavedPedMenu.RefreshIndex();
        //    //spawnSavedPedMenu.UpdateScaleform();
        //}

        ///// <summary>
        ///// Refresh the delete saved peds menu.
        ///// </summary>
        //private void RefreshDeleteSavedPedMenu()
        //{
        //    deleteSavedPedMenu.ClearMenuItems();
        //    int findHandle = StartFindKvp("ped_");
        //    List<string> savesFound = new List<string>();
        //    while (true)
        //    {
        //        var saveName = FindKvp(findHandle);
        //        if (saveName != null && saveName != "" && saveName != "NULL")
        //        {
        //            savesFound.Add(saveName);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    foreach (var savename in savesFound)
        //    {
        //        MenuItem deleteSavedPed = new MenuItem(savename.Substring(4), "~r~Delete ~s~this saved ped, this action can ~r~NOT~s~ be undone!")
        //        {
        //            LeftIcon = MenuItem.Icon.WARNING
        //        };
        //        deleteSavedPedMenu.AddMenuItem(deleteSavedPed);
        //    }

        //    // Sort the menu items (case IN-sensitive) by name.
        //    deleteSavedPedMenu.SortMenuItems((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));

        //    deleteSavedPedMenu.OnItemSelect += (sender, item, idex) =>
        //    {
        //        var name = item.Text.ToString();
        //        StorageManager.DeleteSavedStorageItem("ped_" + name);
        //        Notify.Success("Saved ped deleted.");
        //        deleteSavedPedMenu.GoBack();
        //    };

        //    deleteSavedPedMenu.RefreshIndex();
        //    //deleteSavedPedMenu.UpdateScaleform();
        //}
        #endregion

        #region Model Names
        private Dictionary<string, string> mainModels = new Dictionary<string, string>()
        {
            ["player_zero"] = "Michael",
            ["player_one"] = "Franklin",
            ["player_two"] = "Trevor",
            ["mp_f_freemode_01"] = "FreemodeFemale01",
            ["mp_m_freemode_01"] = "FreemodeMale01"
        };
        private Dictionary<string, string> animalModels = new Dictionary<string, string>()
        {
            ["a_c_boar"] = "Boar",
            ["a_c_cat_01"] = "Cat",
            ["a_c_chickenhawk"] = "ChickenHawk",
            ["a_c_chimp"] = "Chimp",
            ["a_c_chop"] = "Chop",
            ["a_c_cormorant"] = "Cormorant",
            ["a_c_cow"] = "Cow",
            ["a_c_coyote"] = "Coyote",
            ["a_c_crow"] = "Crow",
            ["a_c_deer"] = "Deer",
            ["a_c_dolphin"] = "Dolphin",
            ["a_c_fish"] = "Fish",
            ["a_c_hen"] = "Hen",
            ["a_c_humpback"] = "Humpback",
            ["a_c_husky"] = "Husky",
            ["a_c_killerwhale"] = "KillerWhale",
            ["a_c_mtlion"] = "MountainLion",
            ["a_c_pig"] = "Pig",
            ["a_c_pigeon"] = "Pigeon",
            ["a_c_poodle"] = "Poodle",
            ["a_c_pug"] = "Pug",
            ["a_c_rabbit_01"] = "Rabbit",
            ["a_c_rat"] = "Rat",
            ["a_c_retriever"] = "Retriever",
            ["a_c_rhesus"] = "Rhesus",
            ["a_c_rottweiler"] = "Rottweiler",
            ["a_c_seagull"] = "Seagull",
            ["a_c_sharkhammer"] = "HammerShark",
            ["a_c_sharktiger"] = "TigerShark",
            ["a_c_shepherd"] = "Shepherd",
            ["a_c_westy"] = "Westy"
        };
        private Dictionary<string, string> maleModels = new Dictionary<string, string>()
        {
            ["a_m_m_acult_01"] = "Acult01AMM",
            ["a_m_m_afriamer_01"] = "AfriAmer01AMM",
            ["a_m_m_beach_01"] = "Beach01AMM",
            ["a_m_m_beach_02"] = "Beach02AMM",
            ["a_m_m_bevhills_01"] = "Bevhills01AMM",
            ["a_m_m_bevhills_02"] = "Bevhills02AMM",
            ["a_m_m_business_01"] = "Business01AMM",
            ["a_m_m_eastsa_01"] = "Eastsa01AMM",
            ["a_m_m_eastsa_02"] = "Eastsa02AMM",
            ["a_m_m_farmer_01"] = "Farmer01AMM",
            ["a_m_m_fatlatin_01"] = "Fatlatin01AMM",
            ["a_m_m_genfat_01"] = "Genfat01AMM",
            ["a_m_m_genfat_02"] = "Genfat02AMM",
            ["a_m_m_golfer_01"] = "Golfer01AMM",
            ["a_m_m_hasjew_01"] = "Hasjew01AMM",
            ["a_m_m_hillbilly_01"] = "Hillbilly01AMM",
            ["a_m_m_hillbilly_02"] = "Hillbilly02AMM",
            ["a_m_m_indian_01"] = "Indian01AMM",
            ["a_m_m_ktown_01"] = "Ktown01AMM",
            ["a_m_m_malibu_01"] = "Malibu01AMM",
            ["a_m_m_mexcntry_01"] = "MexCntry01AMM",
            ["a_m_m_mexlabor_01"] = "MexLabor01AMM",
            ["a_m_m_og_boss_01"] = "OgBoss01AMM",
            ["a_m_m_paparazzi_01"] = "Paparazzi01AMM",
            ["a_m_m_polynesian_01"] = "Polynesian01AMM",
            ["a_m_m_prolhost_01"] = "PrologueHostage01AMM",
            ["a_m_m_rurmeth_01"] = "Rurmeth01AMM",
            ["a_m_m_salton_01"] = "Salton01AMM",
            ["a_m_m_salton_02"] = "Salton02AMM",
            ["a_m_m_salton_03"] = "Salton03AMM",
            ["a_m_m_salton_04"] = "Salton04AMM",
            ["a_m_m_skater_01"] = "Skater01AMM",
            ["a_m_m_skidrow_01"] = "Skidrow01AMM",
            ["a_m_m_socenlat_01"] = "Socenlat01AMM",
            ["a_m_m_soucent_01"] = "Soucent01AMM",
            ["a_m_m_soucent_02"] = "Soucent02AMM",
            ["a_m_m_soucent_03"] = "Soucent03AMM",
            ["a_m_m_soucent_04"] = "Soucent04AMM",
            ["a_m_m_stlat_02"] = "Stlat02AMM",
            ["a_m_m_tennis_01"] = "Tennis01AMM",
            ["a_m_m_tourist_01"] = "Tourist01AMM",
            ["a_m_m_trampbeac_01"] = "TrampBeac01AMM",
            ["a_m_m_tramp_01"] = "Tramp01AMM",
            ["a_m_m_tranvest_01"] = "Tranvest01AMM",
            ["a_m_m_tranvest_02"] = "Tranvest02AMM",
            ["a_m_o_acult_01"] = "Acult01AMO",
            ["a_m_o_acult_02"] = "Acult02AMO",
            ["a_m_o_beach_01"] = "Beach01AMO",
            ["a_m_o_genstreet_01"] = "Genstreet01AMO",
            ["a_m_o_ktown_01"] = "Ktown01AMO",
            ["a_m_o_salton_01"] = "Salton01AMO",
            ["a_m_o_soucent_01"] = "Soucent01AMO",
            ["a_m_o_soucent_02"] = "Soucent02AMO",
            ["a_m_o_soucent_03"] = "Soucent03AMO",
            ["a_m_o_tramp_01"] = "Tramp01AMO",
            ["a_m_y_acult_01"] = "Acult01AMY",
            ["a_m_y_acult_02"] = "Acult02AMY",
            ["a_m_y_beachvesp_01"] = "Beachvesp01AMY",
            ["a_m_y_beachvesp_02"] = "Beachvesp02AMY",
            ["a_m_y_beach_01"] = "Beach01AMY",
            ["a_m_y_beach_02"] = "Beach02AMY",
            ["a_m_y_beach_03"] = "Beach03AMY",
            ["a_m_y_bevhills_01"] = "Bevhills01AMY",
            ["a_m_y_bevhills_02"] = "Bevhills02AMY",
            ["a_m_y_breakdance_01"] = "Breakdance01AMY",
            ["a_m_y_busicas_01"] = "Busicas01AMY",
            ["a_m_y_business_01"] = "Business01AMY",
            ["a_m_y_business_02"] = "Business02AMY",
            ["a_m_y_business_03"] = "Business03AMY",
            ["a_m_y_cyclist_01"] = "Cyclist01AMY",
            ["a_m_y_dhill_01"] = "Dhill01AMY",
            ["a_m_y_downtown_01"] = "Downtown01AMY",
            ["a_m_y_eastsa_01"] = "Eastsa01AMY",
            ["a_m_y_eastsa_02"] = "Eastsa02AMY",
            ["a_m_y_epsilon_01"] = "Epsilon01AMY",
            ["a_m_y_epsilon_02"] = "Epsilon02AMY",
            ["a_m_y_gay_01"] = "Gay01AMY",
            ["a_m_y_gay_02"] = "Gay02AMY",
            ["a_m_y_genstreet_01"] = "Genstreet01AMY",
            ["a_m_y_genstreet_02"] = "Genstreet02AMY",
            ["a_m_y_golfer_01"] = "Golfer01AMY",
            ["a_m_y_hasjew_01"] = "Hasjew01AMY",
            ["a_m_y_hiker_01"] = "Hiker01AMY",
            ["a_m_y_hippy_01"] = "Hippy01AMY",
            ["a_m_y_hipster_01"] = "Hipster01AMY",
            ["a_m_y_hipster_02"] = "Hipster02AMY",
            ["a_m_y_hipster_03"] = "Hipster03AMY",
            ["a_m_y_indian_01"] = "Indian01AMY",
            ["a_m_y_jetski_01"] = "Jetski01AMY",
            ["a_m_y_juggalo_01"] = "Juggalo01AMY",
            ["a_m_y_ktown_01"] = "Ktown01AMY",
            ["a_m_y_ktown_02"] = "Ktown02AMY",
            ["a_m_y_latino_01"] = "Latino01AMY",
            ["a_m_y_methhead_01"] = "Methhead01AMY",
            ["a_m_y_mexthug_01"] = "MexThug01AMY",
            ["a_m_y_motox_01"] = "Motox01AMY",
            ["a_m_y_motox_02"] = "Motox02AMY",
            ["a_m_y_musclbeac_01"] = "Musclbeac01AMY",
            ["a_m_y_musclbeac_02"] = "Musclbeac02AMY",
            ["a_m_y_polynesian_01"] = "Polynesian01AMY",
            ["a_m_y_roadcyc_01"] = "Roadcyc01AMY",
            ["a_m_y_runner_01"] = "Runner01AMY",
            ["a_m_y_runner_02"] = "Runner02AMY",
            ["a_m_y_salton_01"] = "Salton01AMY",
            ["a_m_y_skater_01"] = "Skater01AMY",
            ["a_m_y_skater_02"] = "Skater02AMY",
            ["a_m_y_soucent_01"] = "Soucent01AMY",
            ["a_m_y_soucent_02"] = "Soucent02AMY",
            ["a_m_y_soucent_03"] = "Soucent03AMY",
            ["a_m_y_soucent_04"] = "Soucent04AMY",
            ["a_m_y_stbla_01"] = "Stbla01AMY",
            ["a_m_y_stbla_02"] = "Stbla02AMY",
            ["a_m_y_stlat_01"] = "Stlat01AMY",
            ["a_m_y_stwhi_01"] = "Stwhi01AMY",
            ["a_m_y_stwhi_02"] = "Stwhi02AMY",
            ["a_m_y_sunbathe_01"] = "Sunbathe01AMY",
            ["a_m_y_surfer_01"] = "Surfer01AMY",
            ["a_m_y_vindouche_01"] = "Vindouche01AMY",
            ["a_m_y_vinewood_01"] = "Vinewood01AMY",
            ["a_m_y_vinewood_02"] = "Vinewood02AMY",
            ["a_m_y_vinewood_03"] = "Vinewood03AMY",
            ["a_m_y_vinewood_04"] = "Vinewood04AMY",
            ["a_m_y_yoga_01"] = "Yoga01AMY",
        };
        private Dictionary<string, string> femaleModels = new Dictionary<string, string>()
        {
            ["a_f_m_beach_01"] = "Beach01AFM",
            ["a_f_m_bevhills_01"] = "Bevhills01AFM",
            ["a_f_m_bevhills_02"] = "Bevhills02AFM",
            ["a_f_m_bodybuild_01"] = "Bodybuild01AFM",
            ["a_f_m_business_02"] = "Business02AFM",
            ["a_f_m_downtown_01"] = "Downtown01AFM",
            ["a_f_m_eastsa_01"] = "Eastsa01AFM",
            ["a_f_m_eastsa_02"] = "Eastsa02AFM",
            ["a_f_m_fatbla_01"] = "FatBla01AFM",
            ["a_f_m_fatcult_01"] = "FatCult01AFM",
            ["a_f_m_fatwhite_01"] = "FatWhite01AFM",
            ["a_f_m_ktown_01"] = "Ktown01AFM",
            ["a_f_m_ktown_02"] = "Ktown02AFM",
            ["a_f_m_prolhost_01"] = "PrologueHostage01AFM",
            ["a_f_m_salton_01"] = "Salton01AFM",
            ["a_f_m_skidrow_01"] = "Skidrow01AFM",
            ["a_f_m_soucentmc_01"] = "Soucentmc01AFM",
            ["a_f_m_soucent_01"] = "Soucent01AFM",
            ["a_f_m_soucent_02"] = "Soucent02AFM",
            ["a_f_m_tourist_01"] = "Tourist01AFM",
            ["a_f_m_trampbeac_01"] = "TrampBeac01AFM",
            ["a_f_m_tramp_01"] = "Tramp01AFM",
            ["a_f_o_genstreet_01"] = "Genstreet01AFO",
            ["a_f_o_indian_01"] = "Indian01AFO",
            ["a_f_o_ktown_01"] = "Ktown01AFO",
            ["a_f_o_salton_01"] = "Salton01AFO",
            ["a_f_o_soucent_01"] = "Soucent01AFO",
            ["a_f_o_soucent_02"] = "Soucent02AFO",
            ["a_f_y_beach_01"] = "Beach01AFY",
            ["a_f_y_bevhills_01"] = "Bevhills01AFY",
            ["a_f_y_bevhills_02"] = "Bevhills02AFY",
            ["a_f_y_bevhills_03"] = "Bevhills03AFY",
            ["a_f_y_bevhills_04"] = "Bevhills04AFY",
            ["a_f_y_business_01"] = "Business01AFY",
            ["a_f_y_business_02"] = "Business02AFY",
            ["a_f_y_business_03"] = "Business03AFY",
            ["a_f_y_business_04"] = "Business04AFY",
            ["a_f_y_eastsa_01"] = "Eastsa01AFY",
            ["a_f_y_eastsa_02"] = "Eastsa02AFY",
            ["a_f_y_eastsa_03"] = "Eastsa03AFY",
            ["a_f_y_epsilon_01"] = "Epsilon01AFY",
            ["a_f_y_fitness_01"] = "Fitness01AFY",
            ["a_f_y_fitness_02"] = "Fitness02AFY",
            ["a_f_y_genhot_01"] = "Genhot01AFY",
            ["a_f_y_golfer_01"] = "Golfer01AFY",
            ["a_f_y_hiker_01"] = "Hiker01AFY",
            ["a_f_y_hippie_01"] = "Hippie01AFY",
            ["a_f_y_hipster_01"] = "Hipster01AFY",
            ["a_f_y_hipster_02"] = "Hipster02AFY",
            ["a_f_y_hipster_03"] = "Hipster03AFY",
            ["a_f_y_hipster_04"] = "Hipster04AFY",
            ["a_f_y_indian_01"] = "Indian01AFY",
            ["a_f_y_juggalo_01"] = "Juggalo01AFY",
            ["a_f_y_runner_01"] = "Runner01AFY",
            ["a_f_y_rurmeth_01"] = "Rurmeth01AFY",
            ["a_f_y_scdressy_01"] = "Scdressy01AFY",
            ["a_f_y_skater_01"] = "Skater01AFY",
            ["a_f_y_soucent_01"] = "Soucent01AFY",
            ["a_f_y_soucent_02"] = "Soucent02AFY",
            ["a_f_y_soucent_03"] = "Soucent03AFY",
            ["a_f_y_tennis_01"] = "Tennis01AFY",
            ["a_f_y_topless_01"] = "Topless01AFY",
            ["a_f_y_tourist_01"] = "Tourist01AFY",
            ["a_f_y_tourist_02"] = "Tourist02AFY",
            ["a_f_y_vinewood_01"] = "Vinewood01AFY",
            ["a_f_y_vinewood_02"] = "Vinewood02AFY",
            ["a_f_y_vinewood_03"] = "Vinewood03AFY",
            ["a_f_y_vinewood_04"] = "Vinewood04AFY",
            ["a_f_y_yoga_01"] = "Yoga01AFY",
            ["csb_abigail"] = "AbigailCutscene",
            ["csb_anita"] = "AnitaCutscene",
            ["csb_bride"] = "BrideCutscene",
            ["csb_denise_friend"] = "DeniseFriendCutscene",
        };



        #endregion

    }
}
