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
    public class MpPedCustomization
    {
        // Variables
        private UIMenu menu;
        private readonly CommonFunctions cf = MainMenu.Cf;
        public UIMenu createCharacterMenu = new UIMenu("Create Character", "Create A New Character", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu editCharacterMenu = new UIMenu("vMenu", "Edit Your Saved Character", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu savedCharactersMenu = new UIMenu("vMenu", "Manage Saved Characters", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu inheritanceMenu = new UIMenu("vMenu", "Character Inheritance Options", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu appearanceMenu = new UIMenu("vMenu", "Character Appearance Options", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu faceShapeMenu = new UIMenu("vMenu", "Character Face Shape Options", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu tattoosMenu = new UIMenu("vMenu", "Character Tattoo Options", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu clothesMenu = new UIMenu("vMenu", "Character Clothing Options", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu propsMenu = new UIMenu("vMenu", "Character Props Options", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public static bool DontCloseMenus = false;
        public static bool DisableBackButton = false;

        private MpPedDataManager currentCharacter = new MpPedDataManager();

        private void MakeCreateCharacterMenu(bool male, bool editPed = false)
        {
            currentCharacter = new MpPedDataManager();
            appearanceMenu.Clear();
            faceShapeMenu.Clear();
            clothesMenu.Clear();
            propsMenu.Clear();

            #region appearance menu.
            List<dynamic> hairStylesList = new List<dynamic>();
            for (int i = 0; i < GetNumberOfPedDrawableVariations(PlayerPedId(), 2); i++)
            {
                hairStylesList.Add($"Style #{i}");
            }
            hairStylesList.Add($"Style #{GetNumberOfPedDrawableVariations(PlayerPedId(), 2)}");
            var overlayColorsList = new List<dynamic>();
            for (int i = 0; i < GetNumHairColors(); i++)
            {
                overlayColorsList.Add($"Color #{i}");
            }

            List<dynamic> beardStylesList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(1); i++)
            {
                beardStylesList.Add($"Style #{i}");
            }


            UIMenuListItem hairStyles = new UIMenuListItem("Hair Style", hairStylesList, GetPedDrawableVariation(PlayerPedId(), 2), "Select a hair style.");
            UIMenuListItem hairColors = new UIMenuListItem("Hair Color", overlayColorsList, 0, "Select a hair color.");
            UIMenuListItem hairHighlightColors = new UIMenuListItem("Hair Highlight Color", overlayColorsList, 0, "Select a hair highlight color.");
            UIMenuListItem beardStyles = new UIMenuListItem("Beard Style", beardStylesList, GetPedHeadOverlayValue(PlayerPedId(), 1) != 255 ? GetPedHeadOverlayValue(PlayerPedId(), 1) : 0, "Select a beard/facial hair style.");
            appearanceMenu.AddItem(hairStyles);
            appearanceMenu.AddItem(hairColors);
            appearanceMenu.AddItem(hairHighlightColors);
            appearanceMenu.AddItem(beardStyles);
            if (!male)
            {
                beardStyles.Enabled = false;
                beardStyles.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                beardStyles.Description = "This is not available for female characters.";
            }

            #endregion



            if (male)
            {

            }
            else
            {

            }

            createCharacterMenu.RefreshIndex();
            createCharacterMenu.UpdateScaleform();
        }

        async Task<bool> SavePed()
        {
            string name = await cf.GetUserInput("Enter a save name.", "", 30);
            if (string.IsNullOrEmpty(name) || name == "NULL")
            {
                Notify.Alert("You cancelled the action or you provided an invalid name.");
                return false;
            }
            else
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(currentCharacter);
                if (StorageManager.SaveJsonData("mp_ped_" + name, json, false))
                {
                    Notify.Success($"Your character (~g~{name}~s~) has been saved.");
                    return true;
                }
                else
                {
                    Notify.Error($"Saving failed, most likely because this name (~y~{name}~s~) is already in use.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu("vMenu", "About vMenu", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            UIMenuItem createMale = new UIMenuItem("Create Male Character", "Create a new male character.");
            createMale.SetRightLabel("→→→");

            UIMenuItem createFemale = new UIMenuItem("Create Female Character", "Create a new female character.");
            createFemale.SetRightLabel("→→→");

            UIMenuItem savedCharacters = new UIMenuItem("Saved Characters", "Spawn, edit or delete your existing saved multiplayer characters.");
            savedCharacters.SetRightLabel("→→→");

            MainMenu.Mp.Add(createCharacterMenu);
            MainMenu.Mp.Add(editCharacterMenu);
            MainMenu.Mp.Add(savedCharactersMenu);
            MainMenu.Mp.Add(inheritanceMenu);
            MainMenu.Mp.Add(appearanceMenu);
            MainMenu.Mp.Add(faceShapeMenu);
            MainMenu.Mp.Add(tattoosMenu);
            MainMenu.Mp.Add(clothesMenu);
            MainMenu.Mp.Add(propsMenu);

            CreateSavedPedsMenu();

            menu.AddItem(createMale);
            menu.BindMenuToItem(createCharacterMenu, createMale);
            menu.AddItem(createFemale);
            menu.BindMenuToItem(createCharacterMenu, createFemale);
            menu.AddItem(savedCharacters);
            menu.BindMenuToItem(savedCharactersMenu, savedCharacters);

            menu.RefreshIndex();
            menu.UpdateScaleform();

            menu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            createCharacterMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            editCharacterMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            inheritanceMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            appearanceMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            faceShapeMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            tattoosMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            clothesMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            propsMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));

            UIMenuItem inheritanceButton = new UIMenuItem("Character Inheritance", "Character inheritance options.");
            UIMenuItem appearanceButton = new UIMenuItem("Character Appearance", "Character appearance options.");
            UIMenuItem faceButton = new UIMenuItem("Character Face Shape Options", "Character face shape options.");
            UIMenuItem tattoosButton = new UIMenuItem("Character Tatttoo Options", "Currently unavailable, please be patient. It may come some day if it's ever properly supported. Needs a lot more research.");
            UIMenuItem clothesButton = new UIMenuItem("Character Clothes", "Character clothes.");
            UIMenuItem propsButton = new UIMenuItem("Character Props", "Character props.");
            UIMenuItem saveButton = new UIMenuItem("Save Character", "Save your character.");
            UIMenuItem exitNoSave = new UIMenuItem("Exit Without Saving", "Are you sure? All unsaved work will be lost.");


            //tattoosButton.SetRightBadge(UIMenuItem.BadgeStyle.Tatoo);
            tattoosButton.SetRightBadge(UIMenuItem.BadgeStyle.Lock);
            tattoosButton.Enabled = false;

            inheritanceButton.SetRightLabel("→→→");
            appearanceButton.SetRightLabel("→→→");
            faceButton.SetRightLabel("→→→");
            clothesButton.SetRightLabel("→→→");
            propsButton.SetRightLabel("→→→");

            createCharacterMenu.AddItem(inheritanceButton);
            createCharacterMenu.AddItem(appearanceButton);
            createCharacterMenu.AddItem(faceButton);
            createCharacterMenu.AddItem(tattoosButton);
            createCharacterMenu.AddItem(clothesButton);
            createCharacterMenu.AddItem(propsButton);
            createCharacterMenu.AddItem(saveButton);
            createCharacterMenu.AddItem(exitNoSave);


            createCharacterMenu.BindMenuToItem(inheritanceMenu, inheritanceButton);
            createCharacterMenu.BindMenuToItem(appearanceMenu, appearanceButton);
            createCharacterMenu.BindMenuToItem(faceShapeMenu, faceButton);
            //createCharacterMenu.BindMenuToItem(appearanceMenu, tattoosButton);
            createCharacterMenu.BindMenuToItem(clothesMenu, clothesButton);
            createCharacterMenu.BindMenuToItem(propsMenu, propsButton);

            #region inheritance
            List<dynamic> parents = new List<dynamic>();
            for (int i = 0; i < 46; i++)
            {
                parents.Add($"#{i}");
            }

            var inheritanceDads = new UIMenuListItem("Father", parents, 0, "Select a father.");
            var inheritanceMoms = new UIMenuListItem("Mother", parents, 0, "Select a mother.");
            List<dynamic> mixValues = new List<dynamic>() { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };
            var inheritanceShapeMix = new UIMenuSliderItem("Head Shape Mix", mixValues, 5, "Select how much of your head shape should be inherited from your father or mother. All the way on the left is your dad, all the way on the right is your mom.", true);
            var inheritanceSkinMix = new UIMenuSliderItem("Body Skin Mix", mixValues, 5, "Select how much of your body skin tone should be inherited from your father or mother. All the way on the left is your dad, all the way on the right is your mom.", true);

            inheritanceMenu.AddItem(inheritanceDads);
            inheritanceMenu.AddItem(inheritanceMoms);
            inheritanceMenu.AddItem(inheritanceShapeMix);
            inheritanceMenu.AddItem(inheritanceSkinMix);

            void SetHeadBlend()
            {
                SetPedHeadBlendData(PlayerPedId(), inheritanceDads.Index, inheritanceMoms.Index, 0, inheritanceDads.Index, inheritanceMoms.Index, 0, mixValues[inheritanceShapeMix.Index], mixValues[inheritanceSkinMix.Index], 0f, false);
            }

            inheritanceMenu.OnListChange += (sender, item, index) =>
            {
                SetHeadBlend();
            };

            inheritanceMenu.OnSliderChange += (sender, item, index) =>
            {
                SetHeadBlend();
            };
            #endregion


            Dictionary<int, KeyValuePair<string, string>> hairOverlays = new Dictionary<int, KeyValuePair<string, string>>()
            {
                { 0, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
                { 1, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 2, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 3, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_003_a") },
                { 4, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 5, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 6, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 7, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 8, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_008_a") },
                { 9, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 10, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 11, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 12, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 13, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 14, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
                { 15, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
                { 16, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 17, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
                { 18, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_000_a") },
                { 19, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_001_a") },
                { 20, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_000_a") },
                { 21, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_001_a") },
                { 22, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
            };

            appearanceMenu.OnListChange += (sender, item, index) =>
            {
                int itemIndex = sender.MenuItems.IndexOf(item);
                var tmp = (UIMenuListItem)sender.MenuItems[1];
                int hairColor = tmp.Index;
                tmp = (UIMenuListItem)sender.MenuItems[2];
                int hairHighlightColor = tmp.Index;
                if (itemIndex == 0)
                {
                    ClearPedFacialDecorations(PlayerPedId());
                    if (index >= GetNumberOfPedDrawableVariations(PlayerPedId(), 2))
                    {
                        SetPedComponentVariation(PlayerPedId(), 2, 0, 0, 0);
                    }
                    else
                    {
                        SetPedComponentVariation(PlayerPedId(), 2, index, 0, 0);
                        if (hairOverlays.ContainsKey(index))
                        {
                            SetPedFacialDecoration(PlayerPedId(), (uint)GetHashKey(hairOverlays[index].Key), (uint)GetHashKey(hairOverlays[index].Value));
                        }
                    }

                }
                else if (itemIndex == 1 || itemIndex == 2)
                {
                    SetPedHairColor(PlayerPedId(), hairColor, hairHighlightColor);
                }
            };

            // create character items selected.
            createCharacterMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == saveButton) // save ped
                {
                    if (await SavePed())
                    {
                        createCharacterMenu.GoBack();
                    }
                }
                else if (item == exitNoSave) // exit
                {
                    bool confirm = false;
                    AddTextEntry("vmenu_warning_message_first_line", "Are you sure you want to exit the character creator?");
                    AddTextEntry("vmenu_warning_message_second_line", "You will lose all (unsaved) customization!");
                    createCharacterMenu.Visible = false;
                    while (true)
                    {
                        await BaseScript.Delay(0);
                        int unk = 1;
                        int unk2 = 1;
                        SetWarningMessage("vmenu_warning_message_first_line", 20, "vmenu_warning_message_second_line", true, 0, ref unk, ref unk2, true, 0);
                        if (IsControlJustPressed(2, 201) || IsControlJustPressed(2, 217)) // continue/accept
                        {
                            confirm = true;
                            break;
                        }
                        else if (IsControlJustPressed(2, 202)) // cancel
                        {
                            break;
                        }
                    }
                    if (confirm)
                    {
                        while (IsControlPressed(2, 201) || IsControlPressed(2, 217) || IsDisabledControlPressed(2, 201) || IsDisabledControlPressed(2, 217))
                            await BaseScript.Delay(0);
                        await BaseScript.Delay(100);
                        menu.Visible = true;
                    }
                    else
                    {
                        createCharacterMenu.Visible = true;
                    }
                }
                else if (item == inheritanceButton) // inhertiance menu
                {
                    var data = Game.PlayerPed.GetHeadBlendData();
                    inheritanceDads.Index = data.FirstFaceShape;
                    inheritanceMoms.Index = data.SecondFaceShape;
                    inheritanceShapeMix.Index = (int)(data.ParentFaceShapePercent * 10f);
                    inheritanceSkinMix.Index = (int)(data.ParentSkinTonePercent * 10f);
                }
            };

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == createMale)
                {
                    cf.SetPlayerSkin("mp_m_freemode_01", new CommonFunctions.PedInfo() { version = -1 });
                    SetPedDefaultComponentVariation(PlayerPedId());
                    SetPedHeadBlendData(PlayerPedId(), 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 0f, false);
                    MakeCreateCharacterMenu(male: true);
                }
                else if (item == createFemale)
                {
                    cf.SetPlayerSkin("mp_f_freemode_01", new CommonFunctions.PedInfo() { version = -1 });
                    SetPedDefaultComponentVariation(PlayerPedId());
                    SetPedHeadBlendData(PlayerPedId(), 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 0f, false);
                    MakeCreateCharacterMenu(male: false);
                }
                else if (item == savedCharacters)
                {
                    UpdateSavedPedsMenu();
                }
            };
        }

        void UpdateSavedPedsMenu()
        {
            List<string> names = new List<string>();
            var handle = StartFindKvp("mp_ped_");
            while (true)
            {
                string foundName = FindKvp(handle);
                if (string.IsNullOrEmpty(foundName))
                {
                    break;
                }
                else
                {
                    names.Add(foundName.Substring(7));
                }
            }
            EndFindKvp(handle);
            savedCharactersMenu.Clear();
            if (names.Count > 0)
            {
                foreach (string item in names)
                {
                    UIMenuItem btn = new UIMenuItem(item, "Click to spawn this saved ped.");
                    savedCharactersMenu.AddItem(btn);
                }
            }
        }

        /// <summary>
        /// Creates the saved mp characters menu.
        /// </summary>
        private void CreateSavedPedsMenu()
        {
            UpdateSavedPedsMenu();

            savedCharactersMenu.OnItemSelect += async (sender, item, index) =>
            {
                await LoadMpPed(item.Text);
            };
        }





        async Task LoadMpPed(string name)
        {
            if (name.Contains("mp_ped_"))
            {
                Debug.WriteLine(GetResourceKvpString(name));
            }
            else
            {
                Debug.WriteLine(GetResourceKvpString("mp_ped_" + name));
            }

            await BaseScript.Delay(0);
        }


        #region old
        /*
        async Task<bool> SaveMpPed()
        {
            string name = await cf.GetUserInput("Enter a save name", "", 30);
            if (string.IsNullOrEmpty(name) || name == "NULL")
            {
                Notify.Error("Invalid name");
                return false;
            }
            else
            {
                var data = new MpPedDataManager.MultiplayerPedData();
                data.PedHeadBlendData = Game.PlayerPed.GetHeadBlendData();
                data.PedHairData = new MpPedDataManager.PedHair() { style = hairStyles.Index, color = hairColors.Index, colorHighlight = hairHighlightColors.Index, beardStyle = beards.Index - 1, beardColor = beardColors.Index, eyebrowsStyle = eyebrows.Index - 1, eyebrowsColor = eyebrowsColors.Index };
                data.CharacterComponentsData = new MpPedDataManager.CharacterComponents();
                data.FaceFeaturesData = new MpPedDataManager.FaceFeatures();
                data.IsMale = true;
                data.ModelHash = (uint)GetHashKey("mp_m_freemode_01");
                data.PedMakeupData = new MpPedDataManager.PedMakeup();
                data.SaveName = name;
                data.Version = 1;
                if (StorageManager.SaveJsonData("mp_ped_" + name, Newtonsoft.Json.JsonConvert.SerializeObject(data), false))
                {
                    Notify.Success($"Ped saved as ~y~{name}~s~.");
                    return true;
                }
                else
                {
                    Notify.Error("An error occured while saving ped. Error location: 1.");
                    return false;
                }
            }
        }
        */

        ///// <summary>
        ///// Creates the male creator menu.
        ///// </summary>
        //private void CreateMaleCreatorMenu()
        //{
        //    #region inheritance menu
        //    MainMenu.Mp.Add(maleInheritance);
        //    UIMenuItem inheritanceBtn = new UIMenuItem("Inheritance", "Character inheritance options.");
        //    inheritanceBtn.SetRightLabel("→→→");
        //    createMaleMenu.AddItem(inheritanceBtn);
        //    createMaleMenu.BindMenuToItem(maleInheritance, inheritanceBtn);

        //    List<dynamic> dads = new List<dynamic>();
        //    for (int dad = 0; dad < 45; dad++)
        //    {
        //        dads.Add($"Father #{dad}");
        //    }
        //    List<dynamic> moms = new List<dynamic>();
        //    for (int mom = 0; mom < 45; mom++)
        //    {
        //        moms.Add($"Mother #{mom}");
        //    }
        //    UIMenuListItem dadList = new UIMenuListItem("Father", dads, 0, "Select a dad to inherit from.");
        //    UIMenuListItem momList = new UIMenuListItem("Mother", moms, 0, "Select a dad to inherit from.");
        //    List<dynamic> mixValues = new List<dynamic>() { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
        //    UIMenuSliderItem shapePercent = new UIMenuSliderItem("Shape %", mixValues, 5);
        //    UIMenuSliderItem skinPercent = new UIMenuSliderItem("Skin %", mixValues, 5);

        //    maleInheritance.AddItem(dadList);
        //    maleInheritance.AddItem(momList);
        //    maleInheritance.AddItem(shapePercent);
        //    maleInheritance.AddItem(skinPercent);

        //    void UpdateHeadBlend()
        //    {
        //        SetPedHeadBlendData(PlayerPedId(), dadList.Index, momList.Index, 0, dadList.Index, momList.Index, 0, mixValues[shapePercent.Index], mixValues[skinPercent.Index], 0f, false);
        //    };

        //    maleInheritance.OnListChange += (sender, item, index) =>
        //    {
        //        UpdateHeadBlend();
        //    };

        //    maleInheritance.OnSliderChange += (sender, item, index) =>
        //    {
        //        UpdateHeadBlend();
        //    };
        //    #endregion

        //    #region appearacnce menu
        //    MainMenu.Mp.Add(maleAppearance);
        //    UIMenuItem maleAppearanceBtn = new UIMenuItem("Appearance", "Character appearance options.");
        //    maleAppearanceBtn.SetRightLabel("→→→");
        //    createMaleMenu.AddItem(maleAppearanceBtn);
        //    createMaleMenu.BindMenuToItem(maleAppearance, maleAppearanceBtn);

        //    List<dynamic> hairStylesList = new List<dynamic>();
        //    List<dynamic> overlayColorsList = new List<dynamic>();
        //    List<dynamic> eyebrowsList = new List<dynamic>() { "No eyebrows" };
        //    List<dynamic> beardsList = new List<dynamic>() { "No facial hair" };

        //    for (int i = 0; i < GetNumberOfPedDrawableVariations(PlayerPedId(), 2); i++) // hair styles
        //    {
        //        hairStylesList.Add($"Style #{i}");
        //    }
        //    for (int i = 0; i < GetNumHairColors(); i++) // overlay colors
        //    {
        //        overlayColorsList.Add($"Color #{i}");
        //    }
        //    for (int i = 0; i < GetNumHeadOverlayValues(2); i++) // eyebrows
        //    {
        //        eyebrowsList.Add($"Style #{i}");
        //    }
        //    for (int i = 0; i < GetNumHeadOverlayValues(1); i++) // beards
        //    {
        //        beardsList.Add($"Style #{i}");
        //    }

        //    UIMenuListItem hairStyles = new UIMenuListItem("Hair Style", hairStylesList, 0, "Choose a hair style.");
        //    UIMenuListItem hairColors = new UIMenuListItem("Hair Color", overlayColorsList, 0, "Choose your hair color.");
        //    UIMenuListItem hairHighlightColors = new UIMenuListItem("Hair Highlight Color", overlayColorsList, 0, "Choose your hair highlight color.");
        //    UIMenuListItem eyebrows = new UIMenuListItem("Eyebrows", eyebrowsList, 0, "Eyebrows overlay.");
        //    UIMenuListItem eyebrowsColors = new UIMenuListItem("Eyebrows Color", overlayColorsList, 0, "Eyebrows overlay color.");
        //    UIMenuListItem beards = new UIMenuListItem("Beards / Facial Hair", beardsList, 0, "Beards and other facial hair styles.");
        //    UIMenuListItem beardColors = new UIMenuListItem("Beard Colors", overlayColorsList, 0, "Beards and other facial hair styles color.");


        //    maleAppearance.AddItem(hairStyles);
        //    maleAppearance.AddItem(hairColors);
        //    maleAppearance.AddItem(hairHighlightColors);
        //    maleAppearance.AddItem(eyebrows);
        //    maleAppearance.AddItem(eyebrowsColors);
        //    maleAppearance.AddItem(beards);
        //    maleAppearance.AddItem(beardColors);


        //    maleAppearance.OnListChange += (sender, item, index) =>
        //    {
        //        if (item == hairStyles)
        //        {
        //            SetPedComponentVariation(PlayerPedId(), 2, hairStyles.Index, 0, 0);
        //        }
        //        else if (item == hairColors || item == hairHighlightColors)
        //        {
        //            SetPedHairColor(PlayerPedId(), hairColors.Index, hairHighlightColors.Index);
        //        }
        //        else if (item == eyebrows)
        //        {
        //            if (index == 0)
        //                SetPedHeadOverlay(PlayerPedId(), 2, 255, 1f);
        //            else
        //                SetPedHeadOverlay(PlayerPedId(), 2, eyebrows.Index - 1, 1f);

        //        }
        //        else if (item == eyebrowsColors)
        //        {
        //            SetPedHeadOverlayColor(PlayerPedId(), 2, 1, eyebrowsColors.Index, eyebrowsColors.Index);
        //        }
        //        else if (item == beards)
        //        {
        //            if (index == 0)
        //                SetPedHeadOverlay(PlayerPedId(), 1, 255, 1f);
        //            else
        //                SetPedHeadOverlay(PlayerPedId(), 1, beards.Index - 1, 1f);
        //        }
        //        else if (item == beardColors)
        //        {
        //            SetPedHeadOverlayColor(PlayerPedId(), 1, 1, beardColors.Index, beardColors.Index);
        //        }
        //    };
        //    #endregion




        //    UIMenuItem save = new UIMenuItem("Save", "Save the character, note if you do not save this character before exiting this menu you will lose all customization done to this character.");
        //    createMaleMenu.AddItem(save);
        //    createMaleMenu.OnItemSelect += async (sender, item, index) =>
        //    {
        //        if (item == save)
        //        {
        //            if (await SaveMpPed())
        //            {
        //                while (!createMaleMenu.Visible)
        //                {
        //                    await BaseScript.Delay(0);
        //                }
        //                createMaleMenu.GoBack();
        //            }
        //        }
        //    };
        //}

        /// <summary>
        /// Creates the female creator menu.
        /// </summary>
        //private void CreateFemaleCreatorMenu()
        //{
        //    UIMenuItem save = new UIMenuItem("Save", "Save the character, note if you do not save this character before exiting this menu you will lose all customization done to this character.");
        //    createFemaleMenu.AddItem(save);
        //    createFemaleMenu.OnItemSelect += (sender, item, index) =>
        //    {
        //        if (item == save)
        //        {
        //            cf.SaveMpPed(isMale: false);
        //        }
        //    };
        //}

        #endregion

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
