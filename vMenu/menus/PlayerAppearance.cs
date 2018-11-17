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
        private CommonFunctions cf = MainMenu.Cf;
        private StorageManager sm = new StorageManager();

        private UIMenu menu;
        //public UIMenu mpCharMenu;

        private UIMenu pedCustomizationMenu;
        private UIMenu spawnSavedPedMenu;
        private UIMenu deleteSavedPedMenu;

        public static Dictionary<string, uint> AddonPeds;

        public static int ClothingAnimationType { get; set; } = UserDefaults.PAClothingAnimationType;

        /*
        
        //public List<UIMenu> mpCharMenus = new List<UIMenu>();

        #region Mp character struct
        public struct MpCharacterStyle
        {
            /// sex
            public bool IsMale { get; set; }

            /// appearance
            // hair
            public int HairStyle { get; set; }
            public int HairColor { get; set; }
            public int HairHighlightColor { get; set; }
            // face
            public int EyeColor { get; set; }


            /// body/head shapes
            public float NoseWidth { get; set; }
            public float NosePeakHeight { get; set; }
            public float NosePeakLength { get; set; }
            public float NosePeakLowering { get; set; }
            public float NoseBoneTwist { get; set; }
            public float EyebrowHeight { get; set; }
            public float EyebrowForward { get; set; }
            public float CheeksBoneHeight { get; set; }
            public float CheeksBoneWidth { get; set; }
            public float CheeksWidth { get; set; }
            public float EyesOpening { get; set; }
            public float LipsThickness { get; set; }
            public float JawBoneWidth { get; set; }
            public float JawBoneBackLength { get; set; }
            public float ChinBoneLowering { get; set; }
            public float ChinBoneLength { get; set; }
            public float ChinBoneWidth { get; set; }
            public float ChinHole { get; set; }
            public float NeckThickness { get; set; }
            public float Resemblance { get; set; }
            public float SkinTone { get; set; }

            public int Mom { get; set; }
            public int Dad { get; set; }
            public bool Valid { get; private set; }


            /// constructor

            public MpCharacterStyle(bool isMale) : this(isMale, true, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, 0f) { }
            public MpCharacterStyle(bool isMale, bool isValid) : this(isMale, isValid, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0f, 0f) { }
            public MpCharacterStyle(bool isMale, bool isValid, int hairStyle, int hairColor, int hairHighlightColor, int eyeColor, float noseWidth, float nosePeakHeight, float nosePeakLength, float nosePeakLowering, float noseBoneTwist, float eyebrowHeight, float eyebrowForward, float cheekboneHeight, float cheekboneWidth, float cheekWidth, float eyesOpening, float lipsThickness, float jawBoneWidth, float jawBoneBackLength, float chinBoneLowering, float chinBoneLength, float chinBoneWidth, float chinHole, float neckThickness, int mom, int dad, float resemblance, float skinTone)
            {
                IsMale = isMale;
                Valid = isValid;
                HairStyle = hairStyle;
                HairColor = hairColor;
                HairHighlightColor = hairHighlightColor;
                EyeColor = eyeColor;
                NoseWidth = noseWidth;
                NosePeakHeight = nosePeakHeight;
                NosePeakLength = nosePeakLength;
                NosePeakLowering = nosePeakLowering;
                NoseBoneTwist = noseBoneTwist;
                EyebrowHeight = eyebrowHeight;
                EyebrowForward = eyebrowForward;
                CheeksBoneHeight = cheekboneHeight;
                CheeksBoneWidth = cheekboneWidth;
                CheeksWidth = cheekWidth;
                EyesOpening = eyesOpening;
                LipsThickness = lipsThickness;
                JawBoneWidth = jawBoneWidth;
                JawBoneBackLength = jawBoneBackLength;
                ChinBoneLength = chinBoneLength;
                ChinBoneWidth = chinBoneWidth;
                ChinBoneLowering = chinBoneLowering;
                ChinHole = chinHole;
                NeckThickness = neckThickness;
                Mom = mom;
                Dad = dad;
                Resemblance = resemblance;
                SkinTone = skinTone;
            }
        }
        #endregion
        
        //MpCharacterStyle currentCharacter = new MpCharacterStyle(false, false);
        */

        private Dictionary<UIMenuListItem, int> drawablesMenuListItems = new Dictionary<UIMenuListItem, int>();
        private Dictionary<UIMenuListItem, int> propsMenuListItems = new Dictionary<UIMenuListItem, int>();

        #region create the menu
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

            ////Create the submenus.
            //mpCharMenu = new UIMenu(GetPlayerName(PlayerId()), "Multiplayer Ped Customization", true)
            //{
            //    ScaleWithSafezone = false,
            //    MouseControlsEnabled = false,
            //    MouseEdgeEnabled = false,
            //    ControlDisablingEnabled = false
            //};
            //mpCharMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right"));
            //mpCharMenus.Add(mpCharMenu);

            spawnSavedPedMenu = new UIMenu("Saved Peds", "Spawn Saved Ped", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            deleteSavedPedMenu = new UIMenu("Saved Peds", "Delete Saved Ped", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            pedCustomizationMenu = new UIMenu("Ped Customization", "Customize Saved Ped", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            // Add the (submenus) to the menu pool.
            //MainMenu.Mp.Add(mpCharMenu);
            MainMenu.Mp.Add(pedCustomizationMenu);
            MainMenu.Mp.Add(spawnSavedPedMenu);
            MainMenu.Mp.Add(deleteSavedPedMenu);

            // Create the menu items.
            //UIMenuItem mpCharMenuBtn = new UIMenuItem("MP Character Customization", "All multiplayer (freemode character) ped customization options.");
            //mpCharMenuBtn.SetRightLabel("→→→");
            UIMenuItem pedCustomization = new UIMenuItem("Ped Customization", "Modify your ped's appearance.");
            pedCustomization.SetRightLabel("→→→");
            UIMenuItem savePed = new UIMenuItem("Save Current Ped", "Save your current ped and clothes.");
            savePed.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
            UIMenuItem spawnSavedPed = new UIMenuItem("Spawn Saved Ped", "Spawn one of your saved peds.");
            spawnSavedPed.SetRightLabel("→→→");
            UIMenuItem deleteSavedPed = new UIMenuItem("Delete Saved Ped", "Delete one of your saved peds.");
            deleteSavedPed.SetRightLabel("→→→");
            deleteSavedPed.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
            UIMenuItem spawnByName = new UIMenuItem("Spawn Ped By Name", "Enter a model name of a custom ped you want to spawn.");
            List<dynamic> walkstyles = new List<dynamic>() { "Normal", "Injured", "Tough Guy", "Femme", "Gangster", "Posh", "Sexy", "Business", "Drunk", "Hipster" };
            UIMenuListItem walkingStyle = new UIMenuListItem("Walking Style", walkstyles, 0, "Change the walking style of your current ped. " +
                "You need to re-apply this each time you change player model or load a saved ped.");
            List<dynamic> clothingGlowAnimations = new List<dynamic>() { "On", "Off", "Fade", "Flash" };
            UIMenuListItem clothingGlowType = new UIMenuListItem("Illuminated Clothing Style", clothingGlowAnimations, ClothingAnimationType, "Set the style of the animation used on your player's illuminated clothing items.");

            // Add items to the mneu.
            //menu.AddItem(mpCharMenuBtn);
            menu.AddItem(pedCustomization);
            menu.AddItem(savePed);
            menu.AddItem(spawnSavedPed);
            menu.AddItem(deleteSavedPed);
            menu.AddItem(walkingStyle);
            menu.AddItem(clothingGlowType);

            // old mp character stuff
            /*
            ////Bind items to the submenus.
            //if (cf.IsAllowed(Permission.PACustomize) && MainMenu.EnableExperimentalFeatures) // only enable it if experimental features are turned on
            //{
            //    CreateMpPedMenu(mpCharMenu); // loads all menu items and adds event listeners.
            //    menu.BindMenuToItem(mpCharMenu, mpCharMenuBtn);
            //}
            //else
            //{
            //    mpCharMenuBtn.Enabled = false;
            //    mpCharMenuBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
            //    mpCharMenuBtn.Description = "This will be added in the near future. It does not work, so don't even try to enable it, it WILL currently delete all your saved peds/vehicles if you do so.";
            //}
            */

            if (cf.IsAllowed(Permission.PACustomize))
            {
                menu.BindMenuToItem(pedCustomizationMenu, pedCustomization);
            }
            else
            {
                pedCustomization.Enabled = false;
                pedCustomization.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                pedCustomization.Description = "~r~This option has been disabled by the server owner.";
            }

            if (cf.IsAllowed(Permission.PASpawnSaved))
            {
                menu.BindMenuToItem(spawnSavedPedMenu, spawnSavedPed);
            }
            else
            {
                spawnSavedPed.Enabled = false;
                spawnSavedPed.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                spawnSavedPed.Description = "~r~This option has been disabled by the server owner.";
            }

            menu.BindMenuToItem(deleteSavedPedMenu, deleteSavedPed);

            UIMenu addonPeds = new UIMenu("Model Spawner", "Spawn Addon Ped", true)
            {
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false,
                ScaleWithSafezone = false
            };

            UIMenuItem addonPedsBtn = new UIMenuItem("Addon Peds", "Choose a player skin from the addons list available on this server.");
            menu.AddItem(addonPedsBtn);
            MainMenu.Mp.Add(addonPeds);

            if (AddonPeds != null)
            {
                if (AddonPeds.Count > 0)
                {
                    addonPedsBtn.SetRightLabel("→→→");
                    foreach (KeyValuePair<string, uint> ped in AddonPeds)
                    {
                        var button = new UIMenuItem(ped.Key, "Click to use this ped.");
                        addonPeds.AddItem(button);
                        if (!IsModelAPed(ped.Value) || !IsModelInCdimage(ped.Value))
                        {
                            button.Enabled = false;
                            button.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            button.Description = "This ped is not available on this server. Are you sure the model is valid?";
                        }
                    }
                    addonPeds.OnItemSelect += (sender, item, index) =>
                    {
                        if (item.Enabled)
                        {
                            cf.SetPlayerSkin(AddonPeds.ElementAt(index).Value, new CommonFunctions.PedInfo() { version = -1 });
                        }
                        else
                        {
                            Notify.Error("This ped is not available. Please ask the server owner to verify this addon ped.");
                        }

                    };
                    menu.BindMenuToItem(addonPeds, addonPedsBtn);
                }
                else
                {
                    addonPedsBtn.Enabled = false;
                    addonPedsBtn.Description = "This server does not have any addon peds available.";
                    addonPedsBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                }
            }
            else
            {
                addonPedsBtn.Enabled = false;
                addonPedsBtn.Description = "This server does not have any addon peds available.";
                addonPedsBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
            }

            addonPeds.RefreshIndex();
            addonPeds.UpdateScaleform();

            // Add the spawn by name button after the addon peds menu item.
            menu.AddItem(spawnByName);



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
                if (item == walkingStyle)
                {
                    Subtitle.Custom("Ped is: " + IsPedMale(PlayerPedId()));
                    cf.SetWalkingStyle(walkstyles[index].ToString());
                }
                else if (item == clothingGlowType)
                {
                    ClothingAnimationType = item.Index;
                }
                else
                {
                    int i = ((sender.CurrentSelection - 8) * 50) + index;
                    string modelName = modelNames[i];
                    if (cf.IsAllowed(Permission.PASpawnNew))
                    {
                        cf.SetPlayerSkin(modelName, new CommonFunctions.PedInfo() { version = -1 });
                    }
                }

            };

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
                else if (item == spawnByName)
                {
                    cf.SpawnPedByName();
                }
            };

            // Handle saved ped spawning.
            spawnSavedPedMenu.OnItemSelect += (sender, item, idex) =>
            {
                var name = item.Text.ToString();
                cf.LoadSavedPed(name, true);
            };

            #region ped drawable list changes
            // Manage list changes.
            pedCustomizationMenu.OnListChange += (sender, item, index) =>
            {
                //SetPedComponentVariation(PlayerPedId(), sender2.CurrentSelection, index2, 0, 0);

                if (drawablesMenuListItems.ContainsKey(item))
                {
                    int drawableID = drawablesMenuListItems[item];
                    SetPedComponentVariation(PlayerPedId(), drawableID, index, 0, 0);
                }
                else if (propsMenuListItems.ContainsKey(item))
                {
                    int propID = propsMenuListItems[item];
                    if (index == 0)
                    {
                        SetPedPropIndex(PlayerPedId(), propID, index - 1, 0, false);
                        ClearPedProp(PlayerPedId(), propID);
                    }
                    else
                    {
                        SetPedPropIndex(PlayerPedId(), propID, index - 1, 0, true);
                    }

                }
            };

            // Manage list selections.
            pedCustomizationMenu.OnListSelect += (sender, item, index) =>
            {
                if (drawablesMenuListItems.ContainsKey(item)) // drawable
                {
                    int currentDrawableID = drawablesMenuListItems[item];
                    int currentTextureIndex = GetPedTextureVariation(PlayerPedId(), currentDrawableID);
                    int maxDrawableTextures = GetNumberOfPedTextureVariations(PlayerPedId(), currentDrawableID, index) - 1;

                    if (currentTextureIndex == -1)
                        currentTextureIndex = 0;

                    int newTexture = currentTextureIndex < maxDrawableTextures ? currentTextureIndex + 1 : 0;

                    SetPedComponentVariation(PlayerPedId(), currentDrawableID, index, newTexture, 0);
                }
                else if (propsMenuListItems.ContainsKey(item)) // prop
                {
                    int currentPropIndex = propsMenuListItems[item];
                    int currentPropVariationIndex = GetPedPropIndex(PlayerPedId(), currentPropIndex);
                    int currentPropTextureVariation = GetPedPropTextureIndex(PlayerPedId(), currentPropIndex);
                    int maxPropTextureVariations = GetNumberOfPedPropTextureVariations(PlayerPedId(), currentPropIndex, currentPropVariationIndex) - 1;

                    int newPropTextureVariationIndex = currentPropTextureVariation < maxPropTextureVariations ? currentPropTextureVariation + 1 : 0;
                    SetPedPropIndex(PlayerPedId(), currentPropIndex, currentPropVariationIndex, newPropTextureVariationIndex, true);
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
        public UIMenu GetMenu()
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
            pedCustomizationMenu.Clear();

            #region Ped Drawables
            for (int drawable = 0; drawable < 12; drawable++)
            {
                int currentDrawable = GetPedDrawableVariation(PlayerPedId(), drawable);
                int maxVariations = GetNumberOfPedDrawableVariations(PlayerPedId(), drawable);
                int maxTextures = GetNumberOfPedTextureVariations(PlayerPedId(), drawable, currentDrawable);

                if (maxVariations > 0)
                {
                    List<dynamic> drawableTexturesList = new List<dynamic>();

                    for (int i = 0; i < maxVariations; i++)
                    {
                        drawableTexturesList.Add($"Drawable #{i + 1} (of {maxVariations})");
                    }

                    UIMenuListItem drawableTextures = new UIMenuListItem($"{textureNames[drawable]}", drawableTexturesList, currentDrawable, $"Use ← & → to select a ~o~{textureNames[drawable]} Variation~s~, press ~r~enter~s~ to cycle through the available textures.");
                    drawablesMenuListItems.Add(drawableTextures, drawable);
                    pedCustomizationMenu.AddItem(drawableTextures);
                }
            }
            #endregion

            #region Ped Props
            for (int tmpProp = 0; tmpProp < 5; tmpProp++)
            {
                int realProp = tmpProp > 2 ? tmpProp + 3 : tmpProp;

                int currentProp = GetPedPropIndex(PlayerPedId(), realProp);
                int maxPropVariations = GetNumberOfPedPropDrawableVariations(PlayerPedId(), realProp);

                if (maxPropVariations > 0)
                {
                    List<dynamic> propTexturesList = new List<dynamic>();

                    propTexturesList.Add($"Prop #1 (of {maxPropVariations + 1})");
                    for (int i = 0; i < maxPropVariations; i++)
                    {
                        propTexturesList.Add($"Prop #{i + 2} (of {maxPropVariations + 1})");
                    }


                    UIMenuListItem propTextures = new UIMenuListItem($"{propNames[tmpProp]}", propTexturesList, currentProp + 1, $"Use ← & → to select a ~o~{propNames[tmpProp]} Variation~s~, press ~r~enter~s~ to cycle through the available textures.");
                    propsMenuListItems.Add(propTextures, realProp);
                    pedCustomizationMenu.AddItem(propTextures);

                }
            }
            pedCustomizationMenu.RefreshIndex();
            pedCustomizationMenu.UpdateScaleform();
            menu.UpdateScaleform();
            #endregion
        }
        #endregion

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

        #region saved peds menus
        /// <summary>
        /// Refresh the spawn saved peds menu.
        /// </summary>
        private void RefreshSpawnSavedPedMenu()
        {
            spawnSavedPedMenu.MenuItems.Clear();
            int findHandle = StartFindKvp("ped_");
            List<string> savesFound = new List<string>();
            var i = 0;
            while (true)
            {
                i++;
                var saveName = FindKvp(findHandle);
                if (saveName != null && saveName != "" && saveName != "NULL")
                {
                    // It's already the new format, so add it.
                    savesFound.Add(saveName);
                }
                else
                {
                    break;
                }
            }

            var items = new List<string>();
            foreach (var savename in savesFound)
            {
                if (savename.Length > 4)
                {
                    var title = savename.Substring(4);
                    if (!items.Contains(title))
                    {
                        UIMenuItem savedPedBtn = new UIMenuItem(title, "Spawn this saved ped.");
                        spawnSavedPedMenu.AddItem(savedPedBtn);
                        items.Add(title);
                    }
                }
            }

            // Sort the menu items (case IN-sensitive) by name.
            spawnSavedPedMenu.MenuItems.Sort((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));

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

            // Sort the menu items (case IN-sensitive) by name.
            deleteSavedPedMenu.MenuItems.Sort((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));

            deleteSavedPedMenu.OnItemSelect += (sender, item, idex) =>
            {
                var name = item.Text.ToString();
                sm.DeleteSavedStorageItem("ped_" + name);
                Notify.Success("Saved ped deleted.");
                deleteSavedPedMenu.GoBack();
            };

            deleteSavedPedMenu.RefreshIndex();
            deleteSavedPedMenu.UpdateScaleform();
        }
        #endregion

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

        #region old mp character stuff
        /*
        #region Multiplayer ped customization
        /// <summary>
        /// Creates the multiplayer ped customization submenu.
        /// </summary>
        /// <param name="mpMenu"></param>
        public void CreateMpPedMenu(UIMenu mpMenu)
        {
            #region create submenus
            // create new model
            UIMenu newCharacterMenu = new UIMenu("New Character", "Create a new character", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // load existing model
            UIMenu loadCharacterMenu = new UIMenu("Load Character", "Load an existing character", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // create new male model
            UIMenu maleMenu = new UIMenu("New Character", "Create a new male character", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // create new female model
            UIMenu femaleMenu = new UIMenu("New Character", "Create a new female character", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // male appearance
            UIMenu maleAppearanceMenu = new UIMenu("Male Appearance", "Appearance", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // female appearance
            UIMenu femaleAppearanceMenu = new UIMenu("Female Appearance", "Appearance", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // male features
            UIMenu maleFeaturesMenu = new UIMenu("Male Features", "Features", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // female features
            UIMenu femaleFeaturesMenu = new UIMenu("Female Features", "Features", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // male heritage
            UIMenu maleHeritageMenu = new UIMenu("Male Heritage", "Heritage", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // female heritage
            UIMenu femaleHeritageMenu = new UIMenu("Female Heritage", "Features", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // male style
            UIMenu maleStyleMenu = new UIMenu("Male Style", "Facial and Body Style", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // female style
            UIMenu femaleStyleMenu = new UIMenu("Female Style", "Facial and Body Style", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            #endregion


            #region add submenus to menu pool
            MainMenu.Mp.Add(newCharacterMenu); // new character menu
            MainMenu.Mp.Add(loadCharacterMenu); // load character menu
            MainMenu.Mp.Add(maleMenu); // new male character
            MainMenu.Mp.Add(femaleMenu); // new female character
            MainMenu.Mp.Add(maleAppearanceMenu); // male appearance
            MainMenu.Mp.Add(femaleAppearanceMenu); // female appearance
            MainMenu.Mp.Add(maleFeaturesMenu); // female features
            MainMenu.Mp.Add(femaleFeaturesMenu); // female features
            MainMenu.Mp.Add(femaleHeritageMenu); // female heritage
            MainMenu.Mp.Add(maleHeritageMenu); // male heritage
            MainMenu.Mp.Add(femaleStyleMenu); // female style
            MainMenu.Mp.Add(maleStyleMenu); // male style
            #endregion


            #region add instructional buttons
            newCharacterMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // new character
            loadCharacterMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // load character
            maleMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // new male character
            femaleMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // new female character
            maleAppearanceMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // male appearance
            femaleAppearanceMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // female appearance
            femaleFeaturesMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // female features
            maleFeaturesMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // male features
            maleHeritageMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // male heritage
            femaleHeritageMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // female heritage
            maleStyleMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // male style
            femaleStyleMenu.AddInstructionalButton(new InstructionalButton(Control.LookLeftRight, "Turn Head Left/Right")); // female style
            #endregion


            #region add menus to list
            mpCharMenus.Add(newCharacterMenu); // new char
            mpCharMenus.Add(loadCharacterMenu); // load char
            mpCharMenus.Add(maleMenu); // new male char
            mpCharMenus.Add(femaleMenu); // new female char
            mpCharMenus.Add(maleAppearanceMenu); // male appearance
            mpCharMenus.Add(femaleAppearanceMenu); // female appearance
            mpCharMenus.Add(maleFeaturesMenu); // male features
            mpCharMenus.Add(femaleFeaturesMenu); // female features
            mpCharMenus.Add(maleHeritageMenu); // male heritage
            mpCharMenus.Add(femaleHeritageMenu); // female heritage
            mpCharMenus.Add(maleStyleMenu); // male style
            mpCharMenus.Add(femaleStyleMenu); // female style
            #endregion


            #region create menu items
            // character customization menu
            UIMenuItem newCharBtn = new UIMenuItem("New Character", "Create a new multiplayer character.");
            UIMenuItem loadCharBtn = new UIMenuItem("Load Existing Character", "Load an existing (saved) multiplayer character.");

            // new character menu
            UIMenuItem male = new UIMenuItem("Create Male Character", "Create a new male multiplayer character.");
            UIMenuItem female = new UIMenuItem("Create Female Character", "Create a new female multiplayer character.");

            // new male menu (TODO)

            // new female menu
            UIMenuItem f_Appearance = new UIMenuItem("Appearance", "Make changes to your Appearance.");
            UIMenuItem f_Features = new UIMenuItem("Features", "Make changes to your Features.");
            UIMenuItem f_Heritage = new UIMenuItem("Heritage", "Make changes to your Heritage.");
            UIMenuItem f_Styles = new UIMenuItem("Styles", "Make changes to your Face and Body Style.");


            #region female appearance menu.
            // hair styles 
            List<dynamic> f_hair_styles = new List<dynamic>() { };
            for (int i = 0; i < 24; i++)
            {
                f_hair_styles.Add(GetLabelText($"CC_F_HS_{i}"));
            }
            UIMenuListItem f_hair_style = new UIMenuListItem("Hair Style", f_hair_styles, 0);

            List<dynamic> hair_colors = new List<dynamic>();
            for (int i = 0; i < 64; i++)
            {
                hair_colors.Add($"Hair Color #{i + 1}/64");
            }
            UIMenuListItem f_hair_colors = new UIMenuListItem("Hair Color", hair_colors, 0);
            UIMenuListItem f_hair_hi_colors = new UIMenuListItem("Hair Highlight Color", hair_colors, 0);


            // features

            List<dynamic> features_range = new List<dynamic>() { -1f, -0.9f, -0.8f, -0.7f, -0.6f, -0.5f, -0.4f, -0.3f, -0.2f, -0.1f, 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };
            UIMenuSliderItem f_noseWidth = new UIMenuSliderItem("Nose Width", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_noseHeight = new UIMenuSliderItem("Nose Height", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_noseLength = new UIMenuSliderItem("Nose Length", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_noseLowering = new UIMenuSliderItem("Nose Lowering", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_noseBoneTwist = new UIMenuSliderItem("Nose Bone Twist", features_range, features_range.Count / 2, "Make changes to your Features.", true);

            UIMenuSliderItem f_eyebrowHeight = new UIMenuSliderItem("Eybebrows Height", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_eyebrowForward = new UIMenuSliderItem("Eyebrows Depth", features_range, features_range.Count / 2, "Make changes to your Features.", true);

            UIMenuSliderItem f_cheeksboneHeight = new UIMenuSliderItem("Cheekbones Height", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_cheeksboneWidth = new UIMenuSliderItem("Cheekbones Width", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_cheeksWidth = new UIMenuSliderItem("Cheeks Width", features_range, features_range.Count / 2, "Make changes to your Features.", true);

            UIMenuSliderItem f_eyesOpening = new UIMenuSliderItem("Eyes Opening", features_range, features_range.Count / 2, "Make changes to your Features.", true);

            UIMenuSliderItem f_lipsThickness = new UIMenuSliderItem("Lips Thickness", features_range, features_range.Count / 2, "Make changes to your Features.", true);

            UIMenuSliderItem f_jawBoneWidth = new UIMenuSliderItem("Jaw Bone Width", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_jawBoneBackLength = new UIMenuSliderItem("Jaw Bone Back Length", features_range, features_range.Count / 2, "Make changes to your Features.", true);

            UIMenuSliderItem f_chinBoneLowering = new UIMenuSliderItem("Chin Bone Lowering", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_chinBoneLength = new UIMenuSliderItem("Chin Bone Length", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_chinBoneWidth = new UIMenuSliderItem("Chin Bone Width", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            UIMenuSliderItem f_chinHole = new UIMenuSliderItem("Chin Hole", features_range, features_range.Count / 2, "Make changes to your Features.", true);

            UIMenuSliderItem f_neckThickness = new UIMenuSliderItem("Neck Thickness", features_range, features_range.Count / 2, "Make changes to your Features.", true);
            #endregion

            #region female heritage menu
            //UIMenuSliderItem 
            UIMenuHeritageCardItem heritageCard = new UIMenuHeritageCardItem(0, 0);


            // mom
            List<dynamic> moms = new List<dynamic>();
            for (var i = 0; i < 22; i++)
            {
                moms.Add($"Mom #{i}");
            }
            UIMenuListItem mom = new UIMenuListItem("Mom", moms, currentCharacter.Mom);


            // dad
            List<dynamic> dads = new List<dynamic>();
            for (var i = 0; i < 24; i++)
            {
                dads.Add($"Dad #{i}");
            }
            UIMenuListItem dad = new UIMenuListItem("Dad", dads, currentCharacter.Dad);


            // resemblance
            UIMenuSliderItem resemblance = new UIMenuSliderItem("Resemblance", features_range, features_range.Count / 2, "Select if your features are influenced more by your Mother or Father.", true);
            UIMenuSliderItem skinTone = new UIMenuSliderItem("Skin Tone", features_range, features_range.Count / 2, "Select if your skin tone is influenced more by your Mother or Father.", true);

            #endregion

            #region female style menu buttons
            List<dynamic> blemishes = new List<dynamic>() { "1", "2" };
            UIMenuListItem f_blemishes = new UIMenuListItem("Blemishes", blemishes, 0); // id == 0

            List<dynamic> eyebrows = new List<dynamic>() { "1", "2" };
            UIMenuListItem f_eyebrows = new UIMenuListItem("Eyebrows", eyebrows, 0); // id == 2

            List<dynamic> ageing = new List<dynamic>() { "1", "2" };
            UIMenuListItem f_ageing = new UIMenuListItem("Ageing", ageing, 0); // id == 3

            List<dynamic> blush = new List<dynamic>() { "1", "2" };
            //UIMenuListItem

            List<dynamic> makeup = new List<dynamic>();
            for (int i = 0; i < 22; i++)
            {
                makeup.Add(GetLabelText("CC_MKUP_11"));
            }
            UIMenuListItem f_makeup = new UIMenuListItem("Makeup", makeup, 0); // id == 4
            #endregion

            // save character buttons
            UIMenuItem f_saveChar = new UIMenuItem("Save Character", "Save your character, if you don't save your character now, you won't be able to load or edit it later. You also won't be able to save this ped in the 'Saved Peds' menu.");
            UIMenuItem m_saveChar = new UIMenuItem("Save Character", "Save your character, if you don't save your character now, you won't be able to load or edit it later. You also won't be able to save this ped in the 'Saved Peds' menu.");
            #endregion



            #region add items to menus
            // character customization menu
            mpMenu.AddItem(newCharBtn);
            mpMenu.AddItem(loadCharBtn);

            // new character menu
            newCharacterMenu.AddItem(male);
            newCharacterMenu.AddItem(female);

            // new female char menu
            femaleMenu.AddItem(f_Heritage);
            femaleMenu.AddItem(f_Features);
            femaleMenu.AddItem(f_Appearance);
            femaleMenu.AddItem(f_Styles);
            femaleMenu.AddItem(f_saveChar);

            // female appearance menu
            femaleAppearanceMenu.AddItem(f_hair_style);
            f_hair_style.Index = 4;
            femaleAppearanceMenu.AddItem(f_hair_colors);
            f_hair_colors.Index = 59;
            femaleAppearanceMenu.AddItem(f_hair_hi_colors);
            f_hair_hi_colors.Index = 3;

            // female features menu
            femaleFeaturesMenu.AddItem(f_noseWidth);
            femaleFeaturesMenu.AddItem(f_noseHeight);
            femaleFeaturesMenu.AddItem(f_noseLength);
            femaleFeaturesMenu.AddItem(f_noseLowering);
            femaleFeaturesMenu.AddItem(f_noseBoneTwist);

            femaleFeaturesMenu.AddItem(f_eyebrowHeight);
            femaleFeaturesMenu.AddItem(f_eyebrowForward);

            femaleFeaturesMenu.AddItem(f_cheeksboneHeight);
            femaleFeaturesMenu.AddItem(f_cheeksboneWidth);
            femaleFeaturesMenu.AddItem(f_cheeksWidth);

            femaleFeaturesMenu.AddItem(f_eyesOpening);

            femaleFeaturesMenu.AddItem(f_lipsThickness);

            femaleFeaturesMenu.AddItem(f_jawBoneWidth);
            femaleFeaturesMenu.AddItem(f_jawBoneBackLength);

            femaleFeaturesMenu.AddItem(f_chinBoneLowering);
            femaleFeaturesMenu.AddItem(f_chinBoneLength);
            femaleFeaturesMenu.AddItem(f_chinBoneWidth);
            femaleFeaturesMenu.AddItem(f_chinHole);

            femaleFeaturesMenu.AddItem(f_neckThickness);

            // female heritage menu
            femaleHeritageMenu.AddItem(heritageCard);
            femaleHeritageMenu.AddItem(mom);
            femaleHeritageMenu.AddItem(dad);
            femaleHeritageMenu.AddItem(resemblance);
            femaleHeritageMenu.AddItem(skinTone);
            femaleHeritageMenu.UpdateScaleform();
            femaleHeritageMenu.RefreshIndex();

            #endregion


            #region bind menus to menu items
            // character customization menu
            mpMenu.BindMenuToItem(newCharacterMenu, newCharBtn);
            mpMenu.BindMenuToItem(loadCharacterMenu, loadCharBtn);

            // new character menu
            newCharacterMenu.BindMenuToItem(maleMenu, male);
            newCharacterMenu.BindMenuToItem(femaleMenu, female);

            // female char menu
            femaleMenu.BindMenuToItem(femaleAppearanceMenu, f_Appearance);
            femaleMenu.BindMenuToItem(femaleFeaturesMenu, f_Features);
            femaleMenu.BindMenuToItem(femaleHeritageMenu, f_Heritage);

            femaleMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == f_Heritage)
                {
                    if (femaleHeritageMenu.CurrentSelection == 0)
                    {
                        femaleHeritageMenu.CurrentSelection++;
                    }
                }
            };

            #endregion

            //DeleteResourceKvp("mp_char_some cool name");

            #region item select events
            mpCharMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == loadCharBtn)
                {
                    loadCharacterMenu.Clear();
                }

                int searchHandle = StartFindKvp("mp_char_");
                List<string> characterNames = new List<string>();
                while (true)
                {
                    var foundItem = FindKvp(searchHandle);
                    if (string.IsNullOrEmpty(foundItem))
                    {
                        break;
                    }
                    else
                    {
                        characterNames.Add(foundItem);
                    }
                    await BaseScript.Delay(0);
                }
                foreach (var name in characterNames)
                {
                    var Character = cf.LoadMultiplayerCharacter(name.Substring(8, name.Length - 8));
                    if (Character.Valid)
                    {
                        UIMenuItem nameItem = new UIMenuItem(name.Substring(8, name.Length - 8), "Press select to load this character.");
                        loadCharacterMenu.AddItem(nameItem);
                        nameItem.SetRightLabel($"{(Character.IsMale ? "(male)" : "(female)")}");
                    }
                    else
                    {
                        UIMenuItem nameItem = new UIMenuItem(name.Substring(8, name.Length - 8), "Press select to load this character.");
                        loadCharacterMenu.AddItem(nameItem);
                        nameItem.Enabled = false;
                        nameItem.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                        nameItem.Description = "This model is invalid or corrupted, sorry.";

                    }


                }
                loadCharacterMenu.RefreshIndex();
                loadCharacterMenu.UpdateScaleform();
            };

            loadCharacterMenu.OnItemSelect += (sender, item, index) =>
            {
                var Character = cf.LoadMultiplayerCharacter(item.Text.ToString());
                if (Character.Valid)
                {
                    Notify.Success("Multiplayer character has been loaded.");
                    SetModel(Character.IsMale);
                    currentCharacter = Character;

                    // heritage features.
                    SetPedHeritage();
                    // hair style.
                    SetPedComponentVariation(PlayerPedId(), 2, currentCharacter.HairStyle, 0, 0);
                    // hair colors.
                    SetPedHairColor(PlayerPedId(), currentCharacter.HairColor, currentCharacter.HairHighlightColor);
                    // eye color.
                    SetPedEyeColor(PlayerPedId(), currentCharacter.EyeColor);

                    // facial features.
                    SetPedFaceFeature(PlayerPedId(), 0, currentCharacter.NoseWidth);
                    SetPedFaceFeature(PlayerPedId(), 1, currentCharacter.NosePeakHeight);
                    SetPedFaceFeature(PlayerPedId(), 2, currentCharacter.NosePeakLength);
                    SetPedFaceFeature(PlayerPedId(), 3, currentCharacter.NosePeakLowering);
                    SetPedFaceFeature(PlayerPedId(), 4, currentCharacter.NoseBoneTwist);
                    SetPedFaceFeature(PlayerPedId(), 5, currentCharacter.EyebrowHeight);
                    SetPedFaceFeature(PlayerPedId(), 6, currentCharacter.EyebrowForward);
                    SetPedFaceFeature(PlayerPedId(), 7, currentCharacter.CheeksBoneHeight);
                    SetPedFaceFeature(PlayerPedId(), 8, currentCharacter.CheeksBoneWidth);
                    SetPedFaceFeature(PlayerPedId(), 9, currentCharacter.CheeksWidth);
                    SetPedFaceFeature(PlayerPedId(), 10, currentCharacter.EyesOpening);
                    SetPedFaceFeature(PlayerPedId(), 11, currentCharacter.LipsThickness);
                    SetPedFaceFeature(PlayerPedId(), 12, currentCharacter.JawBoneWidth);
                    SetPedFaceFeature(PlayerPedId(), 13, currentCharacter.JawBoneBackLength);
                    SetPedFaceFeature(PlayerPedId(), 14, currentCharacter.ChinBoneLowering);
                    SetPedFaceFeature(PlayerPedId(), 15, currentCharacter.ChinBoneLength);
                    SetPedFaceFeature(PlayerPedId(), 16, currentCharacter.ChinBoneWidth);
                    SetPedFaceFeature(PlayerPedId(), 17, currentCharacter.ChinHole);
                    SetPedFaceFeature(PlayerPedId(), 18, currentCharacter.NeckThickness);

                    // facial appearance


                    //SetPedFaceFeature(PlayerPedId(), 0, )
                }
                else
                {
                    Notify.Error("Failed to load.", true, true);
                }
            };

            newCharacterMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == male)
                {
                    currentCharacter = new MpCharacterStyle(true, true);
                    SetModel(true);
                }
                if (item == female)
                {
                    currentCharacter = new MpCharacterStyle(false, true);
                    SetModel(false);
                }
            };

            femaleMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == f_saveChar)
                {
                    cf.SaveMultiplayerCharacter(currentCharacter);
                }
            };
            #endregion


            #region checkbox events

            #endregion


            #region list events
            femaleHeritageMenu.OnListChange += (sender, item, index) =>
            {
                if (item == mom)
                {
                    heritageCard.Mom = index;
                    currentCharacter.Mom = index;
                    SetPedHeritage();
                }
                else if (item == dad)
                {
                    heritageCard.Dad = index;
                    currentCharacter.Dad = index;
                    SetPedHeritage();
                }
            };

            femaleAppearanceMenu.OnListChange += (sender, item, index) =>
            {
                if (item == f_hair_style)
                {
                    if (index == 0)
                    {
                        SetPedComponentVariation(PlayerPedId(), 2, 0, 0, 0);
                        currentCharacter.HairStyle = 0;
                    }
                    else
                    {
                        SetPedComponentVariation(PlayerPedId(), 2, index + 25 + 13, 0, 0);
                        currentCharacter.HairStyle = index + 25 + 13;
                    }
                }
                else if (item == f_hair_colors)
                {
                    SetPedHairColor(PlayerPedId(), index, currentCharacter.HairHighlightColor);
                    currentCharacter.HairColor = index;
                }
                else if (item == f_hair_hi_colors)
                {
                    SetPedHairColor(PlayerPedId(), currentCharacter.HairColor, index);
                    currentCharacter.HairHighlightColor = index;
                    
                }
            };
            #endregion


            #region slider events
            femaleFeaturesMenu.OnSliderChange += (sender, item, index) =>
            {
                SetPedFaceFeature(PlayerPedId(), sender.MenuItems.IndexOf(item), features_range[index]);
                //currentCharacter.
            };

            femaleHeritageMenu.OnSliderChange += (sender, item, index) =>
            {
                if (item == resemblance)
                {
                    currentCharacter.Resemblance = features_range[index];
                    SetPedHeritage();
                }
                else if (item == skinTone)
                {
                    currentCharacter.SkinTone = features_range[index];
                    SetPedHeritage();
                }
            };
            #endregion



            #region (working, but unused)
            //var nose = new List<dynamic>() { -1.0f, -0.9f, -0.8f, -0.7f, -0.6f, -0.5f, -0.4f, -0.3f, -0.2f, -0.1f, 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
            //UIMenuSliderItem testItem = new UIMenuSliderItem("Nose", nose, 0, "Nose feature", true);
            //mpMenu.AddItem(testItem);
            //menu.OnMenuChange += (sender, newMenu, forward) =>
            //{
            //    if (newMenu == mpMenu)
            //    {

            //    }
            //};
            //mpMenu.OnSliderChange += (sender, item, index) =>
            //{
            //    //Debug.WriteLine(index.ToString());
            //    Debug.WriteLine(nose[index].ToString());


            //    SetPedFaceFeature(PlayerPedId(), 0, nose[index]);
            //    //var data = 0;
            //    //GetPedHeadBlendData(PlayerPedId(), ref data);
            //    //var dat = CitizenFX.Core.Native.Function.Call<dynamic>((CitizenFX.Core.Native.Hash)0x2746BD9D88C5C5D0, PlayerPedId());
            //    //Debug.WriteLine(dat.ToString());
            //    //GetPedHeadBlendData()
            //};

            /*
            #region tattoo stuff
            // create submenu.
            UIMenu tattooMenu = new UIMenu(GetPlayerName(PlayerId()), "MP Character Tattoo Options", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            // create tattoo items. + tattoo menu bind item
            UIMenuItem tattooMenuBtn = new UIMenuItem("Tattoo Options", "Add or remove a tattoo.");
            mpMenu.AddItem(tattooMenuBtn);
            mpMenu.BindMenuToItem(tattooMenu, tattooMenuBtn);

            // add submenu to menu pool.
            MainMenu.Mp.Add(tattooMenu);




            // create tattoo-submenu submenus.

            #region male tattoos
            UIMenu maleTattooMenu = new UIMenu("Male Tattoos", "Male Tattoo Options", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            UIMenu maleHead = new UIMenu("Male Tattoos", "Male Head Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu maleLeftArm = new UIMenu("Male Tattoos", "Male Left Arm Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu maleRightArm = new UIMenu("Male Tattoos", "Male Right Arm Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu maleLeftLeg = new UIMenu("Male Tattoos", "Male Left Leg Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu maleRightLeg = new UIMenu("Male Tattoos", "Male Right Leg Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu maleTorso = new UIMenu("Male Tattoos", "Male Torso Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            #endregion

            #region female tattoos
            UIMenu femaleTattooMenu = new UIMenu("Female Tattoos", "Female Tattoo Options", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };

            UIMenu femaleHead = new UIMenu("Female Tattoos", "Female Head Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu femaleLeftArm = new UIMenu("Female Tattoos", "Female Left Arm Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu femaleRightArm = new UIMenu("Female Tattoos", "Female Right Arm Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu femaleLeftLeg = new UIMenu("Female Tattoos", "Female Left Leg Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu femaleRightLeg = new UIMenu("Female Tattoos", "Female Right Leg Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            UIMenu femaleTorso = new UIMenu("Female Tattoos", "Female Torso Tattoos", true)
            {
                ControlDisablingEnabled = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ScaleWithSafezone = false
            };
            #endregion



            MainMenu.Mp.Add(maleTattooMenu);

            MainMenu.Mp.Add(maleHead);
            MainMenu.Mp.Add(maleLeftArm);
            MainMenu.Mp.Add(maleRightArm);
            MainMenu.Mp.Add(maleLeftLeg);
            MainMenu.Mp.Add(maleRightLeg);
            MainMenu.Mp.Add(maleTorso);


            MainMenu.Mp.Add(femaleTattooMenu);

            MainMenu.Mp.Add(femaleHead);
            MainMenu.Mp.Add(femaleLeftArm);
            MainMenu.Mp.Add(femaleRightArm);
            MainMenu.Mp.Add(femaleLeftLeg);
            MainMenu.Mp.Add(femaleRightLeg);
            MainMenu.Mp.Add(femaleTorso);



            UIMenuItem maleTattooMenuBtn = new UIMenuItem("Male Tattoos", "Male tattoo options.");
            tattooMenu.AddItem(maleTattooMenuBtn);
            tattooMenu.BindMenuToItem(maleTattooMenu, maleTattooMenuBtn);


            UIMenuItem femaleTattooMenuBtn = new UIMenuItem("Female Tattoos", "Female tattoo options.");
            tattooMenu.AddItem(femaleTattooMenuBtn);
            tattooMenu.BindMenuToItem(femaleTattooMenu, femaleTattooMenuBtn);

            // male tattoo categories
            UIMenuItem maleHeadBtn = new UIMenuItem("Head");
            maleTattooMenu.AddItem(maleHeadBtn);
            maleTattooMenu.BindMenuToItem(maleHead, maleHeadBtn);
            UIMenuItem maleLeftArmBtn = new UIMenuItem("Left Arm");
            maleTattooMenu.AddItem(maleLeftArmBtn);
            maleTattooMenu.BindMenuToItem(maleLeftArm, maleLeftArmBtn);
            UIMenuItem maleRightArmBtn = new UIMenuItem("Right Arm");
            maleTattooMenu.AddItem(maleRightArmBtn);
            maleTattooMenu.BindMenuToItem(maleRightArm, maleRightArmBtn);
            UIMenuItem maleLeftLegBtn = new UIMenuItem("Left Leg");
            maleTattooMenu.AddItem(maleLeftLegBtn);
            maleTattooMenu.BindMenuToItem(maleLeftLeg, maleLeftLegBtn);
            UIMenuItem maleRightLegBtn = new UIMenuItem("Right Leg");
            maleTattooMenu.AddItem(maleRightLegBtn);
            maleTattooMenu.BindMenuToItem(maleRightLeg, maleRightLegBtn);
            UIMenuItem maleTorsoBtn = new UIMenuItem("Torso");
            maleTattooMenu.AddItem(maleTorsoBtn);
            maleTattooMenu.BindMenuToItem(maleTorso, maleTorsoBtn);


            // female tattoo categories
            UIMenuItem femaleHeadBtn = new UIMenuItem("Head");
            femaleTattooMenu.AddItem(femaleHeadBtn);
            femaleTattooMenu.BindMenuToItem(femaleHead, femaleHeadBtn);
            UIMenuItem femaleLeftArmBtn = new UIMenuItem("Left Arm");
            femaleTattooMenu.AddItem(femaleLeftArmBtn);
            femaleTattooMenu.BindMenuToItem(femaleLeftArm, femaleLeftArmBtn);
            UIMenuItem femaleRightArmBtn = new UIMenuItem("Right Arm");
            femaleTattooMenu.AddItem(femaleRightArmBtn);
            femaleTattooMenu.BindMenuToItem(femaleRightArm, femaleRightArmBtn);
            UIMenuItem femaleLeftLegBtn = new UIMenuItem("Left Leg");
            femaleTattooMenu.AddItem(femaleLeftLegBtn);
            femaleTattooMenu.BindMenuToItem(femaleLeftLeg, femaleLeftLegBtn);
            UIMenuItem femaleRightLegBtn = new UIMenuItem("Right Leg");
            femaleTattooMenu.AddItem(femaleRightLegBtn);
            femaleTattooMenu.BindMenuToItem(femaleRightLeg, femaleRightLegBtn);
            UIMenuItem femaleTorsoBtn = new UIMenuItem("Torso");
            femaleTattooMenu.AddItem(femaleTorsoBtn);
            femaleTattooMenu.BindMenuToItem(femaleTorso, femaleTorsoBtn);


            foreach (var tattoo in tattoosList)
            {
                UIMenuItem item = new UIMenuItem($"{GetLabelText(tattoo.displayName)}", tattoo.name);
                switch (tattoo.gender)
                {
                    case PedGender.FEMALE:
                        switch (tattoo.zone)
                        {
                            case "HEAD":
                                femaleHead.AddItem(item);
                                break;
                            case "LEFT_ARM":
                                femaleLeftArm.AddItem(item);
                                break;
                            case "RIGHT_ARM":
                                femaleRightArm.AddItem(item);
                                break;
                            case "LEFT_LEG":
                                femaleLeftLeg.AddItem(item);
                                break;
                            case "RIGHT_LEG":
                                femaleRightLeg.AddItem(item);
                                break;
                            case "TORSO":
                                femaleTorso.AddItem(item);
                                break;
                            default: break;
                        }
                        break;
                    case PedGender.MALE:
                        switch (tattoo.zone)
                        {
                            case "HEAD":
                                maleHead.AddItem(item);
                                break;
                            case "LEFT_ARM":
                                maleLeftArm.AddItem(item);
                                break;
                            case "RIGHT_ARM":
                                maleRightArm.AddItem(item);
                                break;
                            case "LEFT_LEG":
                                maleLeftLeg.AddItem(item);
                                break;
                            case "RIGHT_LEG":
                                maleRightLeg.AddItem(item);
                                break;
                            case "TORSO":
                                maleTorso.AddItem(item);
                                break;
                            default: break;
                        }
                        break;
                    default: break;
                }
            }

            maleTorso.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            maleLeftArm.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            maleRightArm.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            maleLeftLeg.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            maleRightLeg.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            maleHead.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);

            femaleTorso.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            femaleLeftArm.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            femaleRightArm.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            femaleLeftLeg.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            femaleRightLeg.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);
            femaleHead.OnItemSelect += (sender, item, index) => EnableTattoo(sender, item, index);

            maleTorso.RefreshIndex();
            maleTorso.UpdateScaleform();
            maleLeftArm.RefreshIndex();
            maleLeftArm.UpdateScaleform();
            maleRightArm.RefreshIndex();
            maleRightArm.UpdateScaleform();
            maleLeftLeg.RefreshIndex();
            maleLeftLeg.UpdateScaleform();
            maleRightLeg.RefreshIndex();
            maleRightLeg.UpdateScaleform();
            maleHead.RefreshIndex();
            maleHead.UpdateScaleform();

            femaleTorso.RefreshIndex();
            femaleTorso.UpdateScaleform();
            femaleLeftArm.RefreshIndex();
            femaleLeftArm.UpdateScaleform();
            femaleRightArm.RefreshIndex();
            femaleRightArm.UpdateScaleform();
            femaleLeftLeg.RefreshIndex();
            femaleLeftLeg.UpdateScaleform();
            femaleRightLeg.RefreshIndex();
            femaleRightLeg.UpdateScaleform();
            femaleHead.RefreshIndex();
            femaleHead.UpdateScaleform();

            //update the menu.
            tattooMenu.RefreshIndex();
            tattooMenu.UpdateScaleform();
            #endregion

            // create items.
            UIMenuItem backBtn = new UIMenuItem("Back", "Go back to the previous menu.");


            // add items to the submenu.

            mpMenu.AddItem(backBtn);


            // add event listener for slider changes.

            // add event listener for checkbox changes.

            // add event listener for list changes.

            // add event listener for list selection events.





            // add event listener for item select events.
            mpMenu.OnItemSelect += (sender, item, index) =>
            {
                // go back.
                if (item == backBtn)
                {
                    mpMenu.GoBack();
                }
            };

            // update stuff.
            mpMenu.RefreshIndex();
            mpMenu.UpdateScaleform();
            
            

            #endregion
        }



        private void SetPedHeritage()
        {
            float Resemblance = (currentCharacter.Resemblance + 1f) / 2f;
            float SkinTone = (currentCharacter.SkinTone + 1f) / 2f;
            //float thirdF = 0f;
            //float thirdF = (Resemblance + SkinTone) / 2f;

            int dad = currentCharacter.Dad;
            int mom = currentCharacter.Mom;
            //int num = dad + mom;
            //dad = (num < 20) ? (20 + dad) : (dad + mom + 2);
            //SetPedHeadBlendData(PlayerPedId(), mom, dad, 0, mom, dad, 0, Resemblance, SkinTone, thirdF, false);
            if (GetEntityModel(PlayerPedId()) == (uint)GetHashKey("mp_f_freemode_01"))
            {
                SetPedHeadBlendData(PlayerPedId(), mom + 21, dad, 0, mom + 21, dad, 0, Resemblance, SkinTone, 0f, false);
            }
            else
            {
                SetPedHeadBlendData(PlayerPedId(), mom, dad, 0, mom, dad, 0, Resemblance, SkinTone, 0f, false);
            }

        }

        private async void SetModel(bool male)
        {
            uint model = male ? (uint)GetHashKey("mp_m_freemode_01") : (uint)GetHashKey("mp_f_freemode_01");

            if (IsModelInCdimage(model))
            {
                if (!HasModelLoaded(model))
                {
                    RequestModel(model);
                }
                while (!HasModelLoaded(model))
                {
                    await BaseScript.Delay(0);
                }
                SetPlayerModel(PlayerId(), model);
                SetModelAsNoLongerNeeded((uint)model);
                SetPedDefaultComponentVariation(PlayerPedId());
                SetPedHeadBlendData(PlayerPedId(), male ? 0 : 20, 0, 0, male ? 0 : 20, 0, 0, 0f, 0f, 0f, false);
                if (!male)
                {
                    currentCharacter.HairStyle = 4 + 25 + 13;
                    SetPedComponentVariation(PlayerPedId(), 2, currentCharacter.HairStyle, 0, 0);
                    currentCharacter.HairColor = 59;
                    currentCharacter.HairHighlightColor = 3;
                    SetPedHairColor(PlayerPedId(), currentCharacter.HairColor, currentCharacter.HairHighlightColor);
                }
                else
                {
                    currentCharacter.HairStyle = 4 + 0 + 13;
                    SetPedComponentVariation(PlayerPedId(), 2, currentCharacter.HairStyle, 0, 0);
                    currentCharacter.HairColor = 59;
                    currentCharacter.HairHighlightColor = 3;
                    SetPedHairColor(PlayerPedId(), currentCharacter.HairColor, currentCharacter.HairHighlightColor);
                }

            }

        }

        #region Enable Tattoo function
        /// <summary>
        /// Enables the tattoo overlay.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        private void EnableTattoo(UIMenu sender, UIMenuItem item, int index)
        {
            if (GetEntityModel(PlayerPedId()) == (uint)GetHashKey("mp_m_freemode_01")) // ped is male.
            {
                Tattoo tat = tattoosList.Find((th) => GetLabelText(th.displayName) == item.Text);
                if (tat.gender == PedGender.MALE || tat.gender == PedGender.UNISEX)
                {
                    ClearPedDecorations(PlayerPedId());
                    SetPedDecoration(PlayerPedId(), (uint)GetHashKey(tat.collection), (uint)GetHashKey(tat.name));
                }
                else
                {
                    Notify.Error("This tattoo is not available for male peds.");
                }
            }
            else if (GetEntityModel(PlayerPedId()) == (uint)GetHashKey("mp_f_freemode_01")) // ped is female.
            {
                Tattoo tat = tattoosList.Find((th) => GetLabelText(th.displayName) == item.Text);
                if (tat.gender == PedGender.FEMALE || tat.gender == PedGender.UNISEX)
                {
                    ClearPedDecorations(PlayerPedId());
                    SetPedDecoration(PlayerPedId(), (uint)GetHashKey(tat.collection), (uint)GetHashKey(tat.name));
                }
                else
                {
                    Notify.Error("This tattoo is not available for female peds.");
                }

            }
        }
        #endregion


        #region tattoos
        private enum PedGender { MALE, FEMALE, UNISEX };

        private struct Tattoo
        {
            public string collection;
            public string name;
            public string displayName;
            public PedGender gender;
            public string zone;
        }

        private readonly List<Tattoo> tattoosList = new List<Tattoo>()
        {
            #region mpChristmas2_overlays
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_000", displayName = "TAT_X2_000", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_001", displayName = "TAT_X2_001", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_002", displayName = "TAT_X2_002", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_003", displayName = "TAT_X2_003", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_004", displayName = "TAT_X2_004", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_005", displayName = "TAT_X2_005", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_006", displayName = "TAT_X2_006", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_007", displayName = "TAT_X2_007", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_008", displayName = "TAT_X2_008", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_009", displayName = "TAT_X2_009", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_010", displayName = "TAT_X2_010", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_011", displayName = "TAT_X2_011", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_012", displayName = "TAT_X2_012", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_013", displayName = "TAT_X2_013", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_014", displayName = "TAT_X2_014", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_015", displayName = "TAT_X2_015", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_016", displayName = "TAT_X2_016", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_017", displayName = "TAT_X2_017", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_018", displayName = "TAT_X2_018", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_019", displayName = "TAT_X2_019", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_020", displayName = "TAT_X2_020", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_021", displayName = "TAT_X2_021", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_022", displayName = "TAT_X2_022", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_023", displayName = "TAT_X2_023", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_024", displayName = "TAT_X2_024", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_025", displayName = "TAT_X2_025", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_026", displayName = "TAT_X2_026", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_027", displayName = "TAT_X2_027", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_028", displayName = "TAT_X2_028", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_M_Tat_029", displayName = "TAT_X2_029", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_000", displayName = "TAT_X2_000", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_001", displayName = "TAT_X2_001", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_002", displayName = "TAT_X2_002", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_003", displayName = "TAT_X2_003", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_004", displayName = "TAT_X2_004", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_005", displayName = "TAT_X2_005", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_006", displayName = "TAT_X2_006", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_007", displayName = "TAT_X2_007", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_008", displayName = "TAT_X2_008", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_009", displayName = "TAT_X2_009", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_010", displayName = "TAT_X2_010", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_011", displayName = "TAT_X2_011", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_012", displayName = "TAT_X2_012", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_013", displayName = "TAT_X2_013", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_014", displayName = "TAT_X2_014", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_015", displayName = "TAT_X2_015", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_016", displayName = "TAT_X2_016", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_017", displayName = "TAT_X2_017", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_018", displayName = "TAT_X2_018", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_019", displayName = "TAT_X2_019", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_020", displayName = "TAT_X2_020", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_021", displayName = "TAT_X2_021", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_022", displayName = "TAT_X2_022", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_023", displayName = "TAT_X2_023", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_024", displayName = "TAT_X2_024", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_025", displayName = "TAT_X2_025", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_026", displayName = "TAT_X2_026", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_027", displayName = "TAT_X2_027", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_028", displayName = "TAT_X2_028", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2_overlays", name = "MP_Xmas2_F_Tat_029", displayName = "TAT_X2_029", gender = PedGender.FEMALE, zone = "HEAD"},
            #endregion

            #region mpLowrider_overlays
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_002_M", displayName = "TAT_S1_002", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_004_M", displayName = "TAT_S1_004", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_005_M", displayName = "TAT_S1_005", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_007_M", displayName = "TAT_S1_007", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_009_M", displayName = "TAT_S1_009", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_010_M", displayName = "TAT_S1_010", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_013_M", displayName = "TAT_S1_013", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_014_M", displayName = "TAT_S1_014", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_015_M", displayName = "TAT_S1_015", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_017_M", displayName = "TAT_S1_017", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_020_M", displayName = "TAT_S1_020", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_021_M", displayName = "TAT_S1_021", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_023_M", displayName = "TAT_S1_023", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_026_M", displayName = "TAT_S1_026", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_027_M", displayName = "TAT_S1_027", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_033_M", displayName = "TAT_S1_033", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_001_F", displayName = "TAT_S1_001", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_002_F", displayName = "TAT_S1_002", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_004_F", displayName = "TAT_S1_004", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_005_F", displayName = "TAT_S1_005", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_007_F", displayName = "TAT_S1_007", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_009_F", displayName = "TAT_S1_009", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_010_F", displayName = "TAT_S1_010", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_013_F", displayName = "TAT_S1_013", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_014_F", displayName = "TAT_S1_014", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_015_F", displayName = "TAT_S1_015", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_017_F", displayName = "TAT_S1_017", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_020_F", displayName = "TAT_S1_020", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_021_F", displayName = "TAT_S1_021", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_023_F", displayName = "TAT_S1_023", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_026_F", displayName = "TAT_S1_026", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_027_F", displayName = "TAT_S1_027", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider_overlays", name = "MP_LR_Tat_033_F", displayName = "TAT_S1_033", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            #endregion

            #region mpLuxe_overlays
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_000_M", displayName = "TAT_LX_000", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_001_M", displayName = "TAT_LX_001", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_003_M", displayName = "TAT_LX_003", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_004_M", displayName = "TAT_LX_004", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_006_M", displayName = "TAT_LX_006", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_007_M", displayName = "TAT_LX_007", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_008_M", displayName = "TAT_LX_008", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_009_M", displayName = "TAT_LX_009", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_013_M", displayName = "TAT_LX_013", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_014_M", displayName = "TAT_LX_014", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_015_M", displayName = "TAT_LX_015", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_019_M", displayName = "TAT_LX_019", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_020_M", displayName = "TAT_LX_020", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_021_M", displayName = "TAT_LX_021", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_024_M", displayName = "TAT_LX_024", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_000_F", displayName = "TAT_LX_000", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_001_F", displayName = "TAT_LX_001", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_003_F", displayName = "TAT_LX_003", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_004_F", displayName = "TAT_LX_004", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_006_F", displayName = "TAT_LX_006", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_007_F", displayName = "TAT_LX_007", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_008_F", displayName = "TAT_LX_008", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_009_F", displayName = "TAT_LX_009", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_013_F", displayName = "TAT_LX_013", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_014_F", displayName = "TAT_LX_014", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_015_F", displayName = "TAT_LX_015", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_019_F", displayName = "TAT_LX_019", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_020_F", displayName = "TAT_LX_020", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_021_F", displayName = "TAT_LX_021", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe_overlays", name = "MP_Luxe_Tat_024_F", displayName = "TAT_LX_024", gender = PedGender.FEMALE, zone = "TORSO"},
            #endregion

            #region mpAirRaces_overlays
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_000_M", displayName = "TAT_AR_000", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_001_M", displayName = "TAT_AR_001", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_002_M", displayName = "TAT_AR_002", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_003_M", displayName = "TAT_AR_003", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_004_M", displayName = "TAT_AR_004", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_005_M", displayName = "TAT_AR_005", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_006_M", displayName = "TAT_AR_006", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_007_M", displayName = "TAT_AR_007", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_000_F", displayName = "TAT_AR_000", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_001_F", displayName = "TAT_AR_001", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_002_F", displayName = "TAT_AR_002", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_003_F", displayName = "TAT_AR_003", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_004_F", displayName = "TAT_AR_004", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_005_F", displayName = "TAT_AR_005", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_006_F", displayName = "TAT_AR_006", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpAirRaces_overlays", name = "MP_AirRaces_Tattoo_007_F", displayName = "TAT_AR_007", gender = PedGender.FEMALE, zone = "TORSO"},
            #endregion

            #region mpBiker_overlays
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_000_F", displayName = "TAT_BI_000", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_000_M", displayName = "TAT_BI_000", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_001_F", displayName = "TAT_BI_001", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_001_M", displayName = "TAT_BI_001", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_002_F", displayName = "TAT_BI_002", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_002_M", displayName = "TAT_BI_002", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_003_F", displayName = "TAT_BI_003", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_003_M", displayName = "TAT_BI_003", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_004_F", displayName = "TAT_BI_004", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_004_M", displayName = "TAT_BI_004", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_005_F", displayName = "TAT_BI_005", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_005_M", displayName = "TAT_BI_005", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_006_F", displayName = "TAT_BI_006", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_006_M", displayName = "TAT_BI_006", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_007_F", displayName = "TAT_BI_007", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_007_M", displayName = "TAT_BI_007", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_008_F", displayName = "TAT_BI_008", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_008_M", displayName = "TAT_BI_008", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_009_F", displayName = "TAT_BI_009", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_009_M", displayName = "TAT_BI_009", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_010_F", displayName = "TAT_BI_010", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_010_M", displayName = "TAT_BI_010", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_011_F", displayName = "TAT_BI_011", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_011_M", displayName = "TAT_BI_011", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_012_F", displayName = "TAT_BI_012", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_012_M", displayName = "TAT_BI_012", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_013_F", displayName = "TAT_BI_013", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_013_M", displayName = "TAT_BI_013", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_014_F", displayName = "TAT_BI_014", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_014_M", displayName = "TAT_BI_014", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_015_F", displayName = "TAT_BI_015", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_015_M", displayName = "TAT_BI_015", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_016_F", displayName = "TAT_BI_016", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_016_M", displayName = "TAT_BI_016", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_017_F", displayName = "TAT_BI_017", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_017_M", displayName = "TAT_BI_017", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_018_F", displayName = "TAT_BI_018", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_018_M", displayName = "TAT_BI_018", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_019_F", displayName = "TAT_BI_019", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_019_M", displayName = "TAT_BI_019", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_020_F", displayName = "TAT_BI_020", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_020_M", displayName = "TAT_BI_020", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_021_F", displayName = "TAT_BI_021", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_021_M", displayName = "TAT_BI_021", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_022_F", displayName = "TAT_BI_022", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_022_M", displayName = "TAT_BI_022", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_023_F", displayName = "TAT_BI_023", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_023_M", displayName = "TAT_BI_023", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_024_F", displayName = "TAT_BI_024", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_024_M", displayName = "TAT_BI_024", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_025_F", displayName = "TAT_BI_025", gender = PedGender.FEMALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_025_M", displayName = "TAT_BI_025", gender = PedGender.MALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_026_F", displayName = "TAT_BI_026", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_026_M", displayName = "TAT_BI_026", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_027_F", displayName = "TAT_BI_027", gender = PedGender.FEMALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_027_M", displayName = "TAT_BI_027", gender = PedGender.MALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_028_F", displayName = "TAT_BI_028", gender = PedGender.FEMALE, zone = "RIGHT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_028_M", displayName = "TAT_BI_028", gender = PedGender.MALE, zone = "RIGHT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_029_F", displayName = "TAT_BI_029", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_029_M", displayName = "TAT_BI_029", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_030_F", displayName = "TAT_BI_030", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_030_M", displayName = "TAT_BI_030", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_031_F", displayName = "TAT_BI_031", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_031_M", displayName = "TAT_BI_031", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_032_F", displayName = "TAT_BI_032", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_032_M", displayName = "TAT_BI_032", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_033_F", displayName = "TAT_BI_033", gender = PedGender.FEMALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_033_M", displayName = "TAT_BI_033", gender = PedGender.MALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_034_F", displayName = "TAT_BI_034", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_034_M", displayName = "TAT_BI_034", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_035_F", displayName = "TAT_BI_035", gender = PedGender.FEMALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_035_M", displayName = "TAT_BI_035", gender = PedGender.MALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_036_F", displayName = "TAT_BI_036", gender = PedGender.FEMALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_036_M", displayName = "TAT_BI_036", gender = PedGender.MALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_037_F", displayName = "TAT_BI_037", gender = PedGender.FEMALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_037_M", displayName = "TAT_BI_037", gender = PedGender.MALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_038_F", displayName = "TAT_BI_038", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_038_M", displayName = "TAT_BI_038", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_039_F", displayName = "TAT_BI_039", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_039_M", displayName = "TAT_BI_039", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_040_F", displayName = "TAT_BI_040", gender = PedGender.FEMALE, zone = "RIGHT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_040_M", displayName = "TAT_BI_040", gender = PedGender.MALE, zone = "RIGHT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_041_F", displayName = "TAT_BI_041", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_041_M", displayName = "TAT_BI_041", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_042_F", displayName = "TAT_BI_042", gender = PedGender.FEMALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_042_M", displayName = "TAT_BI_042", gender = PedGender.MALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_043_F", displayName = "TAT_BI_043", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_043_M", displayName = "TAT_BI_043", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_044_F", displayName = "TAT_BI_044", gender = PedGender.FEMALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_044_M", displayName = "TAT_BI_044", gender = PedGender.MALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_045_F", displayName = "TAT_BI_045", gender = PedGender.FEMALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_045_M", displayName = "TAT_BI_045", gender = PedGender.MALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_046_F", displayName = "TAT_BI_046", gender = PedGender.FEMALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_046_M", displayName = "TAT_BI_046", gender = PedGender.MALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_047_F", displayName = "TAT_BI_047", gender = PedGender.FEMALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_047_M", displayName = "TAT_BI_047", gender = PedGender.MALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_048_F", displayName = "TAT_BI_048", gender = PedGender.FEMALE, zone = "RIGHT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_048_M", displayName = "TAT_BI_048", gender = PedGender.MALE, zone = "RIGHT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_049_F", displayName = "TAT_BI_049", gender = PedGender.FEMALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_049_M", displayName = "TAT_BI_049", gender = PedGender.MALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_050_F", displayName = "TAT_BI_050", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_050_M", displayName = "TAT_BI_050", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_051_F", displayName = "TAT_BI_051", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_051_M", displayName = "TAT_BI_051", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_052_F", displayName = "TAT_BI_052", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_052_M", displayName = "TAT_BI_052", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_053_F", displayName = "TAT_BI_053", gender = PedGender.FEMALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_053_M", displayName = "TAT_BI_053", gender = PedGender.MALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_054_F", displayName = "TAT_BI_054", gender = PedGender.FEMALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_054_M", displayName = "TAT_BI_054", gender = PedGender.MALE, zone = "RIGHT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_055_F", displayName = "TAT_BI_055", gender = PedGender.FEMALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_055_M", displayName = "TAT_BI_055", gender = PedGender.MALE, zone = "LEFT ARM"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_056_F", displayName = "TAT_BI_056", gender = PedGender.FEMALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_056_M", displayName = "TAT_BI_056", gender = PedGender.MALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_057_F", displayName = "TAT_BI_057", gender = PedGender.FEMALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_057_M", displayName = "TAT_BI_057", gender = PedGender.MALE, zone = "LEFT LEG"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_058_F", displayName = "TAT_BI_058", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_058_M", displayName = "TAT_BI_058", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_059_F", displayName = "TAT_BI_059", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_059_M", displayName = "TAT_BI_059", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_060_F", displayName = "TAT_BI_060", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpBiker_overlays", name = "MP_MP_Biker_Tat_060_M", displayName = "TAT_BI_060", gender = PedGender.MALE, zone = "TORSO"},
            #endregion

            #region mpChristmas2017_overlays
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_000_M", displayName = "TAT_H27_000", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_001_M", displayName = "TAT_H27_001", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_002_M", displayName = "TAT_H27_002", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_003_M", displayName = "TAT_H27_003", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_004_M", displayName = "TAT_H27_004", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_005_M", displayName = "TAT_H27_005", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_006_M", displayName = "TAT_H27_006", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_007_M", displayName = "TAT_H27_007", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_008_M", displayName = "TAT_H27_008", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_009_M", displayName = "TAT_H27_009", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_010_M", displayName = "TAT_H27_010", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_011_M", displayName = "TAT_H27_011", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_012_M", displayName = "TAT_H27_012", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_013_M", displayName = "TAT_H27_013", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_014_M", displayName = "TAT_H27_014", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_015_M", displayName = "TAT_H27_015", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_016_M", displayName = "TAT_H27_016", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_017_M", displayName = "TAT_H27_017", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_018_M", displayName = "TAT_H27_018", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_019_M", displayName = "TAT_H27_019", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_020_M", displayName = "TAT_H27_020", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_021_M", displayName = "TAT_H27_021", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_022_M", displayName = "TAT_H27_022", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_023_M", displayName = "TAT_H27_023", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_024_M", displayName = "TAT_H27_024", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_025_M", displayName = "TAT_H27_025", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_026_M", displayName = "TAT_H27_026", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_027_M", displayName = "TAT_H27_027", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_028_M", displayName = "TAT_H27_028", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_029_M", displayName = "TAT_H27_029", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_000_F", displayName = "TAT_H27_000", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_001_F", displayName = "TAT_H27_001", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_002_F", displayName = "TAT_H27_002", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_003_F", displayName = "TAT_H27_003", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_004_F", displayName = "TAT_H27_004", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_005_F", displayName = "TAT_H27_005", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_006_F", displayName = "TAT_H27_006", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_007_F", displayName = "TAT_H27_007", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_008_F", displayName = "TAT_H27_008", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_009_F", displayName = "TAT_H27_009", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_010_F", displayName = "TAT_H27_010", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_011_F", displayName = "TAT_H27_011", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_012_F", displayName = "TAT_H27_012", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_013_F", displayName = "TAT_H27_013", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_014_F", displayName = "TAT_H27_014", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_015_F", displayName = "TAT_H27_015", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_016_F", displayName = "TAT_H27_016", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_017_F", displayName = "TAT_H27_017", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_018_F", displayName = "TAT_H27_018", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_019_F", displayName = "TAT_H27_019", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_020_F", displayName = "TAT_H27_020", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_021_F", displayName = "TAT_H27_021", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_022_F", displayName = "TAT_H27_022", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_023_F", displayName = "TAT_H27_023", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_024_F", displayName = "TAT_H27_024", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_025_F", displayName = "TAT_H27_025", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_026_F", displayName = "TAT_H27_026", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_027_F", displayName = "TAT_H27_027", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_028_F", displayName = "TAT_H27_028", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpChristmas2017_overlays", name = "MP_Christmas2017_Tattoo_029_F", displayName = "TAT_H27_029", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            #endregion

            #region mpGunrunning_overlays
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_000_M", displayName = "TAT_GR_000", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_001_M", displayName = "TAT_GR_001", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_002_M", displayName = "TAT_GR_002", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_003_M", displayName = "TAT_GR_003", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_004_M", displayName = "TAT_GR_004", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_005_M", displayName = "TAT_GR_005", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_006_M", displayName = "TAT_GR_006", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_007_M", displayName = "TAT_GR_007", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_000_F", displayName = "TAT_GR_000", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_001_F", displayName = "TAT_GR_001", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_002_F", displayName = "TAT_GR_002", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_003_F", displayName = "TAT_GR_003", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_004_F", displayName = "TAT_GR_004", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_005_F", displayName = "TAT_GR_005", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_006_F", displayName = "TAT_GR_006", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_007_F", displayName = "TAT_GR_007", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_008_M", displayName = "TAT_GR_008", gender = PedGender.MALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_009_M", displayName = "TAT_GR_009", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_010_M", displayName = "TAT_GR_010", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_011_M", displayName = "TAT_GR_011", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_012_M", displayName = "TAT_GR_012", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_013_M", displayName = "TAT_GR_013", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_014_M", displayName = "TAT_GR_014", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_015_M", displayName = "TAT_GR_015", gender = PedGender.MALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_016_M", displayName = "TAT_GR_016", gender = PedGender.MALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_017_M", displayName = "TAT_GR_017", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_018_M", displayName = "TAT_GR_018", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_019_M", displayName = "TAT_GR_019", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_020_M", displayName = "TAT_GR_020", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_021_M", displayName = "TAT_GR_021", gender = PedGender.MALE, zone = "RIGHT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_022_M", displayName = "TAT_GR_022", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_023_M", displayName = "TAT_GR_023", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_024_M", displayName = "TAT_GR_024", gender = PedGender.MALE, zone = "RIGHT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_025_M", displayName = "TAT_GR_025", gender = PedGender.MALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_026_M", displayName = "TAT_GR_026", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_027_M", displayName = "TAT_GR_027", gender = PedGender.MALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_028_M", displayName = "TAT_GR_028", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_029_M", displayName = "TAT_GR_029", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_030_M", displayName = "TAT_GR_030", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_008_F", displayName = "TAT_GR_008", gender = PedGender.FEMALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_009_F", displayName = "TAT_GR_009", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_010_F", displayName = "TAT_GR_010", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_011_F", displayName = "TAT_GR_011", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_012_F", displayName = "TAT_GR_012", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_013_F", displayName = "TAT_GR_013", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_014_F", displayName = "TAT_GR_014", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_015_F", displayName = "TAT_GR_015", gender = PedGender.FEMALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_016_F", displayName = "TAT_GR_016", gender = PedGender.FEMALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_017_F", displayName = "TAT_GR_017", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_018_F", displayName = "TAT_GR_018", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_019_F", displayName = "TAT_GR_019", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_020_F", displayName = "TAT_GR_020", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_021_F", displayName = "TAT_GR_021", gender = PedGender.FEMALE, zone = "RIGHT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_022_F", displayName = "TAT_GR_022", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_023_F", displayName = "TAT_GR_023", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_024_F", displayName = "TAT_GR_024", gender = PedGender.FEMALE, zone = "RIGHT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_025_F", displayName = "TAT_GR_025", gender = PedGender.FEMALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_026_F", displayName = "TAT_GR_026", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_027_F", displayName = "TAT_GR_027", gender = PedGender.FEMALE, zone = "LEFT"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_028_F", displayName = "TAT_GR_028", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_029_F", displayName = "TAT_GR_029", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpGunrunning_overlays", name = "MP_Gunrunning_Tattoo_030_F", displayName = "TAT_GR_030", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            #endregion

            #region mpImportExport_overlays
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_000_M", displayName = "TAT_IE_000", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_001_M", displayName = "TAT_IE_001", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_002_M", displayName = "TAT_IE_002", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_003_M", displayName = "TAT_IE_003", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_004_M", displayName = "TAT_IE_004", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_005_M", displayName = "TAT_IE_005", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_006_M", displayName = "TAT_IE_006", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_007_M", displayName = "TAT_IE_007", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_008_M", displayName = "TAT_IE_008", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_009_M", displayName = "TAT_IE_009", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_010_M", displayName = "TAT_IE_010", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_011_M", displayName = "TAT_IE_011", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_000_F", displayName = "TAT_IE_000", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_001_F", displayName = "TAT_IE_001", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_002_F", displayName = "TAT_IE_002", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_003_F", displayName = "TAT_IE_003", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_004_F", displayName = "TAT_IE_004", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_005_F", displayName = "TAT_IE_005", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_006_F", displayName = "TAT_IE_006", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_007_F", displayName = "TAT_IE_007", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_008_F", displayName = "TAT_IE_008", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_009_F", displayName = "TAT_IE_009", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_010_F", displayName = "TAT_IE_010", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpImportExport_overlays", name = "MP_MP_ImportExport_Tat_011_F", displayName = "TAT_IE_011", gender = PedGender.FEMALE, zone = "TORSO"},
            #endregion

            #region mpLowrider2_overlays
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_000_M", displayName = "TAT_S2_000", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_003_M", displayName = "TAT_S2_003", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_006_M", displayName = "TAT_S2_006", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_008_M", displayName = "TAT_S2_008", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_011_M", displayName = "TAT_S2_011", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_012_M", displayName = "TAT_S2_012", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_016_M", displayName = "TAT_S2_016", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_018_M", displayName = "TAT_S2_018", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_019_M", displayName = "TAT_S2_019", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_022_M", displayName = "TAT_S2_022", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_028_M", displayName = "TAT_S2_028", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_029_M", displayName = "TAT_S2_029", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_030_M", displayName = "TAT_S2_030", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_031_M", displayName = "TAT_S2_031", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_032_M", displayName = "TAT_S2_032", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_035_M", displayName = "TAT_S2_035", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_000_F", displayName = "TAT_S2_000", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_003_F", displayName = "TAT_S2_003", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_006_F", displayName = "TAT_S2_006", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_008_F", displayName = "TAT_S2_008", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_011_F", displayName = "TAT_S2_011", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_012_F", displayName = "TAT_S2_012", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_016_F", displayName = "TAT_S2_016", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_018_F", displayName = "TAT_S2_018", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_019_F", displayName = "TAT_S2_019", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_022_F", displayName = "TAT_S2_022", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_028_F", displayName = "TAT_S2_028", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_029_F", displayName = "TAT_S2_029", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_030_F", displayName = "TAT_S2_030", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_031_F", displayName = "TAT_S2_031", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_032_F", displayName = "TAT_S2_032", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLowrider2_overlays", name = "MP_LR_Tat_035_F", displayName = "TAT_S2_035", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            #endregion

            #region mpLuxe2_overlays
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_002_M", displayName = "TAT_L2_002", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_005_M", displayName = "TAT_L2_005", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_010_M", displayName = "TAT_L2_010", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_011_M", displayName = "TAT_L2_011", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_012_M", displayName = "TAT_L2_012", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_016_M", displayName = "TAT_L2_016", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_017_M", displayName = "TAT_L2_017", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_018_M", displayName = "TAT_L2_018", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_022_M", displayName = "TAT_L2_022", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_023_M", displayName = "TAT_L2_023", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_025_M", displayName = "TAT_L2_025", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_026_M", displayName = "TAT_L2_026", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_027_M", displayName = "TAT_L2_027", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_028_M", displayName = "TAT_L2_028", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_029_M", displayName = "TAT_L2_029", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_030_M", displayName = "TAT_L2_030", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_031_M", displayName = "TAT_L2_031", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_002_F", displayName = "TAT_L2_002", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_005_F", displayName = "TAT_L2_005", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_010_F", displayName = "TAT_L2_010", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_011_F", displayName = "TAT_L2_011", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_012_F", displayName = "TAT_L2_012", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_016_F", displayName = "TAT_L2_016", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_017_F", displayName = "TAT_L2_017", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_018_F", displayName = "TAT_L2_018", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_022_F", displayName = "TAT_L2_022", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_023_F", displayName = "TAT_L2_023", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_025_F", displayName = "TAT_L2_025", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_026_F", displayName = "TAT_L2_026", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_027_F", displayName = "TAT_L2_027", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_028_F", displayName = "TAT_L2_028", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_029_F", displayName = "TAT_L2_029", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_030_F", displayName = "TAT_L2_030", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpLuxe2_overlays", name = "MP_Luxe_Tat_031_F", displayName = "TAT_L2_031", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            #endregion

            #region mpSmuggler_overlays
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_000_M", displayName = "TAT_SM_000", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_001_M", displayName = "TAT_SM_001", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_002_M", displayName = "TAT_SM_002", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_003_M", displayName = "TAT_SM_003", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_004_M", displayName = "TAT_SM_004", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_005_M", displayName = "TAT_SM_005", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_006_M", displayName = "TAT_SM_006", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_007_M", displayName = "TAT_SM_007", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_008_M", displayName = "TAT_SM_008", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_009_M", displayName = "TAT_SM_009", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_010_M", displayName = "TAT_SM_010", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_011_M", displayName = "TAT_SM_011", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_012_M", displayName = "TAT_SM_012", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_013_M", displayName = "TAT_SM_013", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_014_M", displayName = "TAT_SM_014", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_015_M", displayName = "TAT_SM_015", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_016_M", displayName = "TAT_SM_016", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_017_M", displayName = "TAT_SM_017", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_018_M", displayName = "TAT_SM_018", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_019_M", displayName = "TAT_SM_019", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_020_M", displayName = "TAT_SM_020", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_021_M", displayName = "TAT_SM_021", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_022_M", displayName = "TAT_SM_022", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_023_M", displayName = "TAT_SM_023", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_024_M", displayName = "TAT_SM_024", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_025_M", displayName = "TAT_SM_025", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_000_F", displayName = "TAT_SM_000", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_001_F", displayName = "TAT_SM_001", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_002_F", displayName = "TAT_SM_002", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_003_F", displayName = "TAT_SM_003", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_004_F", displayName = "TAT_SM_004", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_005_F", displayName = "TAT_SM_005", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_006_F", displayName = "TAT_SM_006", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_007_F", displayName = "TAT_SM_007", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_008_F", displayName = "TAT_SM_008", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_009_F", displayName = "TAT_SM_009", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_010_F", displayName = "TAT_SM_010", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_011_F", displayName = "TAT_SM_011", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_012_F", displayName = "TAT_SM_012", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_013_F", displayName = "TAT_SM_013", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_014_F", displayName = "TAT_SM_014", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_015_F", displayName = "TAT_SM_015", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_016_F", displayName = "TAT_SM_016", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_017_F", displayName = "TAT_SM_017", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_018_F", displayName = "TAT_SM_018", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_019_F", displayName = "TAT_SM_019", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_020_F", displayName = "TAT_SM_020", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_021_F", displayName = "TAT_SM_021", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_022_F", displayName = "TAT_SM_022", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_023_F", displayName = "TAT_SM_023", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_024_F", displayName = "TAT_SM_024", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpSmuggler_overlays", name = "MP_Smuggler_Tattoo_025_F", displayName = "TAT_SM_025", gender = PedGender.FEMALE, zone = "TORSO"},
	        #endregion

            #region mpStunt_overlays
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_000_M", displayName = "TAT_ST_000", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_001_M", displayName = "TAT_ST_001", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_002_M", displayName = "TAT_ST_002", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_003_M", displayName = "TAT_ST_003", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_004_M", displayName = "TAT_ST_004", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_005_M", displayName = "TAT_ST_005", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_006_M", displayName = "TAT_ST_006", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_007_M", displayName = "TAT_ST_007", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_008_M", displayName = "TAT_ST_008", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_009_M", displayName = "TAT_ST_009", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_010_M", displayName = "TAT_ST_010", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_011_M", displayName = "TAT_ST_011", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_012_M", displayName = "TAT_ST_012", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_013_M", displayName = "TAT_ST_013", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_014_M", displayName = "TAT_ST_014", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_015_M", displayName = "TAT_ST_015", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_016_M", displayName = "TAT_ST_016", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_017_M", displayName = "TAT_ST_017", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_018_M", displayName = "TAT_ST_018", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_019_M", displayName = "TAT_ST_019", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_020_M", displayName = "TAT_ST_020", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_021_M", displayName = "TAT_ST_021", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_022_M", displayName = "TAT_ST_022", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_023_M", displayName = "TAT_ST_023", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_024_M", displayName = "TAT_ST_024", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_025_M", displayName = "TAT_ST_025", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_026_M", displayName = "TAT_ST_026", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_027_M", displayName = "TAT_ST_027", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_028_M", displayName = "TAT_ST_028", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_029_M", displayName = "TAT_ST_029", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_030_M", displayName = "TAT_ST_030", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_031_M", displayName = "TAT_ST_031", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_032_M", displayName = "TAT_ST_032", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_033_M", displayName = "TAT_ST_033", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_034_M", displayName = "TAT_ST_034", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_035_M", displayName = "TAT_ST_035", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_036_M", displayName = "TAT_ST_036", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_037_M", displayName = "TAT_ST_037", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_038_M", displayName = "TAT_ST_038", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_039_M", displayName = "TAT_ST_039", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_040_M", displayName = "TAT_ST_040", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_041_M", displayName = "TAT_ST_041", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_042_M", displayName = "TAT_ST_042", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_043_M", displayName = "TAT_ST_043", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_044_M", displayName = "TAT_ST_044", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_045_M", displayName = "TAT_ST_045", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_046_M", displayName = "TAT_ST_046", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_047_M", displayName = "TAT_ST_047", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_048_M", displayName = "TAT_ST_048", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_049_M", displayName = "TAT_ST_049", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_000_F", displayName = "TAT_ST_000", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_001_F", displayName = "TAT_ST_001", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_002_F", displayName = "TAT_ST_002", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_003_F", displayName = "TAT_ST_003", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_004_F", displayName = "TAT_ST_004", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_005_F", displayName = "TAT_ST_005", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_006_F", displayName = "TAT_ST_006", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_007_F", displayName = "TAT_ST_007", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_008_F", displayName = "TAT_ST_008", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_009_F", displayName = "TAT_ST_009", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_010_F", displayName = "TAT_ST_010", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_011_F", displayName = "TAT_ST_011", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_012_F", displayName = "TAT_ST_012", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_013_F", displayName = "TAT_ST_013", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_014_F", displayName = "TAT_ST_014", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_015_F", displayName = "TAT_ST_015", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_016_F", displayName = "TAT_ST_016", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_017_F", displayName = "TAT_ST_017", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_018_F", displayName = "TAT_ST_018", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_019_F", displayName = "TAT_ST_019", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_020_F", displayName = "TAT_ST_020", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_021_F", displayName = "TAT_ST_021", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_022_F", displayName = "TAT_ST_022", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_023_F", displayName = "TAT_ST_023", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_024_F", displayName = "TAT_ST_024", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_025_F", displayName = "TAT_ST_025", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_026_F", displayName = "TAT_ST_026", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_027_F", displayName = "TAT_ST_027", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_028_F", displayName = "TAT_ST_028", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_029_F", displayName = "TAT_ST_029", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_030_F", displayName = "TAT_ST_030", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_031_F", displayName = "TAT_ST_031", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_032_F", displayName = "TAT_ST_032", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_033_F", displayName = "TAT_ST_033", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_034_F", displayName = "TAT_ST_034", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_035_F", displayName = "TAT_ST_035", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_036_F", displayName = "TAT_ST_036", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_037_F", displayName = "TAT_ST_037", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_038_F", displayName = "TAT_ST_038", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_039_F", displayName = "TAT_ST_039", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_040_F", displayName = "TAT_ST_040", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_041_F", displayName = "TAT_ST_041", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_042_F", displayName = "TAT_ST_042", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_043_F", displayName = "TAT_ST_043", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_044_F", displayName = "TAT_ST_044", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_045_F", displayName = "TAT_ST_045", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_046_F", displayName = "TAT_ST_046", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_047_F", displayName = "TAT_ST_047", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_048_F", displayName = "TAT_ST_048", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpStunt_overlays", name = "MP_MP_Stunt_Tat_049_F", displayName = "TAT_ST_049", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},

	        #endregion

            #region mpHipster_overlays
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_000", displayName = "TAT_HP_000", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_001", displayName = "TAT_HP_001", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_002", displayName = "TAT_HP_002", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_003", displayName = "TAT_HP_003", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_004", displayName = "TAT_HP_004", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_005", displayName = "TAT_HP_005", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_006", displayName = "TAT_HP_006", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_007", displayName = "TAT_HP_007", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_008", displayName = "TAT_HP_008", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_009", displayName = "TAT_HP_009", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_010", displayName = "TAT_HP_010", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_011", displayName = "TAT_HP_011", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_012", displayName = "TAT_HP_012", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_013", displayName = "TAT_HP_013", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_014", displayName = "TAT_HP_014", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_015", displayName = "TAT_HP_015", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_016", displayName = "TAT_HP_016", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_017", displayName = "TAT_HP_017", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_018", displayName = "TAT_HP_018", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_019", displayName = "TAT_HP_019", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_020", displayName = "TAT_HP_020", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_021", displayName = "TAT_HP_021", gender = PedGender.MALE, zone = "HEAD"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_022", displayName = "TAT_HP_022", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_023", displayName = "TAT_HP_023", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_024", displayName = "TAT_HP_024", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_025", displayName = "TAT_HP_025", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_026", displayName = "TAT_HP_026", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_027", displayName = "TAT_HP_027", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_028", displayName = "TAT_HP_028", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_029", displayName = "TAT_HP_029", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_030", displayName = "TAT_HP_030", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_031", displayName = "TAT_HP_031", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_032", displayName = "TAT_HP_032", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_033", displayName = "TAT_HP_033", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_034", displayName = "TAT_HP_034", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_035", displayName = "TAT_HP_035", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_036", displayName = "TAT_HP_036", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_037", displayName = "TAT_HP_037", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_038", displayName = "TAT_HP_038", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_039", displayName = "TAT_HP_039", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_040", displayName = "TAT_HP_040", gender = PedGender.MALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_041", displayName = "TAT_HP_041", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_042", displayName = "TAT_HP_042", gender = PedGender.MALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_043", displayName = "TAT_HP_043", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_044", displayName = "TAT_HP_044", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_045", displayName = "TAT_HP_045", gender = PedGender.MALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_046", displayName = "TAT_HP_046", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_047", displayName = "TAT_HP_047", gender = PedGender.MALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_M_Tat_048", displayName = "TAT_HP_048", gender = PedGender.MALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_000", displayName = "TAT_HP_000", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_001", displayName = "TAT_HP_001", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_002", displayName = "TAT_HP_002", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_003", displayName = "TAT_HP_003", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_004", displayName = "TAT_HP_004", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_005", displayName = "TAT_HP_005", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_006", displayName = "TAT_HP_006", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_007", displayName = "TAT_HP_007", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_008", displayName = "TAT_HP_008", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_009", displayName = "TAT_HP_009", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_010", displayName = "TAT_HP_010", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_011", displayName = "TAT_HP_011", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_012", displayName = "TAT_HP_012", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_013", displayName = "TAT_HP_013", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_014", displayName = "TAT_HP_014", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_015", displayName = "TAT_HP_015", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_016", displayName = "TAT_HP_016", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_017", displayName = "TAT_HP_017", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_018", displayName = "TAT_HP_018", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_019", displayName = "TAT_HP_019", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_020", displayName = "TAT_HP_020", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_021", displayName = "TAT_HP_021", gender = PedGender.FEMALE, zone = "HEAD"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_022", displayName = "TAT_HP_022", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_023", displayName = "TAT_HP_023", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_024", displayName = "TAT_HP_024", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_025", displayName = "TAT_HP_025", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_026", displayName = "TAT_HP_026", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_027", displayName = "TAT_HP_027", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_028", displayName = "TAT_HP_028", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_029", displayName = "TAT_HP_029", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_030", displayName = "TAT_HP_030", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_031", displayName = "TAT_HP_031", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_032", displayName = "TAT_HP_032", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_033", displayName = "TAT_HP_033", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_034", displayName = "TAT_HP_034", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_035", displayName = "TAT_HP_035", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_036", displayName = "TAT_HP_036", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_037", displayName = "TAT_HP_037", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_038", displayName = "TAT_HP_038", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_039", displayName = "TAT_HP_039", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_040", displayName = "TAT_HP_040", gender = PedGender.FEMALE, zone = "LEFT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_041", displayName = "TAT_HP_041", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_042", displayName = "TAT_HP_042", gender = PedGender.FEMALE, zone = "RIGHT_LEG"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_043", displayName = "TAT_HP_043", gender = PedGender.FEMALE, zone = "LEFT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_044", displayName = "TAT_HP_044", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_045", displayName = "TAT_HP_045", gender = PedGender.FEMALE, zone = "RIGHT_ARM"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_046", displayName = "TAT_HP_046", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_047", displayName = "TAT_HP_047", gender = PedGender.FEMALE, zone = "TORSO"},
            new Tattoo(){collection = "mpHipster_overlays", name = "FM_Hip_F_Tat_048", displayName = "TAT_HP_048", gender = PedGender.FEMALE, zone = "LEFT_ARM"},

	        #endregion
        };

        #region old unused tattoo lists
        //private readonly Dictionary<string, string> maleTattoos = new Dictionary<string, string>()
        //{
        //    ["FM_Tat_Award_M_000"] = "ZONE_HEAD",
        //    ["FM_Tat_Award_M_001"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_Award_M_002"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_Award_M_003"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_004"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_005"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_006"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_Award_M_007"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_Award_M_008"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_009"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_Award_M_010"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_Award_M_011"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_012"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_013"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_014"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_015"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_Award_M_016"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_017"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_018"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_M_019"] = "ZONE_TORSO",
        //    ["FM_Tat_M_001"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_M_002"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_M_003"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_M_004"] = "ZONE_TORSO",
        //    ["FM_Tat_M_005"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_M_006"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_M_007"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_M_008"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_M_009"] = "ZONE_TORSO",
        //    ["FM_Tat_M_010"] = "ZONE_TORSO",
        //    ["FM_Tat_M_011"] = "ZONE_TORSO",
        //    ["FM_Tat_M_012"] = "ZONE_TORSO",
        //    ["FM_Tat_M_013"] = "ZONE_TORSO",
        //    ["FM_Tat_M_014"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_M_015"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_M_016"] = "ZONE_TORSO",
        //    ["FM_Tat_M_017"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_M_018"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_M_019"] = "ZONE_TORSO",
        //    ["FM_Tat_M_020"] = "ZONE_TORSO",
        //    ["FM_Tat_M_021"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_M_022"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_M_023"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_M_024"] = "ZONE_TORSO",
        //    ["FM_Tat_M_025"] = "ZONE_TORSO",
        //    ["FM_Tat_M_026"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_M_027"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_M_028"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_M_029"] = "ZONE_TORSO",
        //    ["FM_Tat_M_030"] = "ZONE_TORSO",
        //    ["FM_Tat_M_031"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_M_032"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_M_033"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_M_034"] = "ZONE_TORSO",
        //    ["FM_Tat_M_035"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_M_036"] = "ZONE_TORSO",
        //    ["FM_Tat_M_037"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_M_038"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_M_039"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_M_040"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_M_041"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_M_042"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_M_043"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_M_044"] = "ZONE_TORSO",
        //    ["FM_Tat_M_045"] = "ZONE_TORSO",
        //    ["FM_Tat_M_046"] = "ZONE_TORSO",
        //    ["FM_Tat_M_047"] = "ZONE_RIGHT_ARM",
        //    ["FM_M_Hair_001_a"] = "ZONE_HEAD",
        //    ["FM_M_Hair_001_b"] = "ZONE_HEAD",
        //    ["FM_M_Hair_001_c"] = "ZONE_HEAD",
        //    ["FM_M_Hair_001_d"] = "ZONE_HEAD",
        //    ["FM_M_Hair_001_e"] = "ZONE_HEAD",
        //    ["FM_M_Hair_003_a"] = "ZONE_HEAD",
        //    ["FM_M_Hair_003_b"] = "ZONE_HEAD",
        //    ["FM_M_Hair_003_c"] = "ZONE_HEAD",
        //    ["FM_M_Hair_003_d"] = "ZONE_HEAD",
        //    ["FM_M_Hair_003_e"] = "ZONE_HEAD",
        //    ["FM_M_Hair_006_a"] = "ZONE_HEAD",
        //    ["FM_M_Hair_006_b"] = "ZONE_HEAD",
        //    ["FM_M_Hair_006_c"] = "ZONE_HEAD",
        //    ["FM_M_Hair_006_d"] = "ZONE_HEAD",
        //    ["FM_M_Hair_006_e"] = "ZONE_HEAD",
        //    ["FM_M_Hair_008_a"] = "ZONE_HEAD",
        //    ["FM_M_Hair_008_b"] = "ZONE_HEAD",
        //    ["FM_M_Hair_008_c"] = "ZONE_HEAD",
        //    ["FM_M_Hair_008_d"] = "ZONE_HEAD",
        //    ["FM_M_Hair_008_e"] = "ZONE_HEAD",
        //    ["FM_M_Hair_long_a"] = "ZONE_HEAD",
        //    ["FM_M_Hair_long_b"] = "ZONE_HEAD",
        //    ["FM_M_Hair_long_c"] = "ZONE_HEAD",
        //    ["FM_M_Hair_long_d"] = "ZONE_HEAD",
        //    ["FM_M_Hair_long_e"] = "ZONE_HEAD",
        //};
        //private readonly Dictionary<string, string> femaleTattoos = new Dictionary<string, string>()
        //{
        //    ["FM_Tat_Award_F_000"] = "ZONE_HEAD",
        //    ["FM_Tat_Award_F_001"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_Award_F_002"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_Award_F_003"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_004"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_005"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_006"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_Award_F_007"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_Award_F_008"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_009"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_Award_F_010"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_Award_F_011"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_012"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_013"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_014"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_015"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_Award_F_016"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_017"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_018"] = "ZONE_TORSO",
        //    ["FM_Tat_Award_F_019"] = "ZONE_TORSO",
        //    ["FM_Tat_F_001"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_F_002"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_F_003"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_F_004"] = "ZONE_TORSO",
        //    ["FM_Tat_F_005"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_F_006"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_F_007"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_F_008"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_F_009"] = "ZONE_TORSO",
        //    ["FM_Tat_F_010"] = "ZONE_TORSO",
        //    ["FM_Tat_F_011"] = "ZONE_TORSO",
        //    ["FM_Tat_F_012"] = "ZONE_TORSO",
        //    ["FM_Tat_F_013"] = "ZONE_TORSO",
        //    ["FM_Tat_F_014"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_F_015"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_F_016"] = "ZONE_TORSO",
        //    ["FM_Tat_F_017"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_F_018"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_F_019"] = "ZONE_TORSO",
        //    ["FM_Tat_F_020"] = "ZONE_TORSO",
        //    ["FM_Tat_F_021"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_F_022"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_F_023"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_F_024"] = "ZONE_TORSO",
        //    ["FM_Tat_F_025"] = "ZONE_TORSO",
        //    ["FM_Tat_F_026"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_F_027"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_F_028"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_F_029"] = "ZONE_TORSO",
        //    ["FM_Tat_F_030"] = "ZONE_TORSO",
        //    ["FM_Tat_F_031"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_F_032"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_F_033"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_F_034"] = "ZONE_TORSO",
        //    ["FM_Tat_F_035"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_F_036"] = "ZONE_TORSO",
        //    ["FM_Tat_F_037"] = "ZONE_LEFT_LEG",
        //    ["FM_Tat_F_038"] = "ZONE_RIGHT_ARM",
        //    ["FM_Tat_F_039"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_F_040"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_F_041"] = "ZONE_LEFT_ARM",
        //    ["FM_Tat_F_042"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_F_043"] = "ZONE_RIGHT_LEG",
        //    ["FM_Tat_F_044"] = "ZONE_TORSO",
        //    ["FM_Tat_F_045"] = "ZONE_TORSO",
        //    ["FM_Tat_F_046"] = "ZONE_TORSO",
        //    ["FM_Tat_F_047"] = "ZONE_RIGHT_ARM",
        //    ["FM_F_Hair_005_a"] = "ZONE_HEAD",
        //    ["FM_F_Hair_005_b"] = "ZONE_HEAD",
        //    ["FM_F_Hair_005_c"] = "ZONE_HEAD",
        //    ["FM_F_Hair_005_d"] = "ZONE_HEAD",
        //    ["FM_F_Hair_005_e"] = "ZONE_HEAD",
        //    ["FM_F_Hair_006_a"] = "ZONE_HEAD",
        //    ["FM_F_Hair_006_b"] = "ZONE_HEAD",
        //    ["FM_F_Hair_006_c"] = "ZONE_HEAD",
        //    ["FM_F_Hair_006_d"] = "ZONE_HEAD",
        //    ["FM_F_Hair_006_e"] = "ZONE_HEAD",
        //    ["FM_F_Hair_013_a"] = "ZONE_HEAD",
        //    ["FM_F_Hair_013_b"] = "ZONE_HEAD",
        //    ["FM_F_Hair_013_c"] = "ZONE_HEAD",
        //    ["FM_F_Hair_013_d"] = "ZONE_HEAD",
        //    ["FM_F_Hair_013_e"] = "ZONE_HEAD",
        //    ["FM_F_Hair_014_a"] = "ZONE_HEAD",
        //    ["FM_F_Hair_014_b"] = "ZONE_HEAD",
        //    ["FM_F_Hair_014_c"] = "ZONE_HEAD",
        //    ["FM_F_Hair_014_d"] = "ZONE_HEAD",
        //    ["FM_F_Hair_014_e"] = "ZONE_HEAD",
        //    ["FM_F_Hair_long_a"] = "ZONE_HEAD",
        //    ["FM_F_Hair_long_b"] = "ZONE_HEAD",
        //    ["FM_F_Hair_long_c"] = "ZONE_HEAD",
        //    ["FM_F_Hair_long_d"] = "ZONE_HEAD",
        //    ["FM_F_Hair_long_e"] = "ZONE_HEAD",
        //};
        //private readonly Dictionary<string, string> unisexTattoos = new Dictionary<string, string>()
        //{
        //    ["FM_Tat_M_000"] = "ZONE_RIGHT_ARM",
        //    ["FM_F_Hair_003_a"] = "ZONE_HEAD",
        //    ["FM_F_Hair_003_b"] = "ZONE_HEAD",
        //    ["FM_F_Hair_003_c"] = "ZONE_HEAD",
        //    ["FM_F_Hair_003_d"] = "ZONE_HEAD",
        //    ["FM_F_Hair_003_e"] = "ZONE_HEAD",
        //};
        #endregion
        #endregion
        #endregion
        */
        #endregion

    }
}
