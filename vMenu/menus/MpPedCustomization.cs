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
        public UIMenu createMaleMenu = new UIMenu("Create Character", "Create A Male Character", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu createFemaleMenu = new UIMenu("Create Character", "Create A Female Character", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu savedCharactersMenu = new UIMenu("Saved Characters", "Manage Saved MP Characters", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu maleInheritance = new UIMenu("vMenu", "Character Inheritance Options", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu maleAppearance = new UIMenu("vMenu", "Character Appearance Options", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public static bool DontCloseMenus = false;
        public static bool DisableBackButton = false;

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

            UIMenuItem savedCharacters = new UIMenuItem("Saved Characters", "Spawn, edit or delete your existing saved multiplayer characters.");
            savedCharacters.SetRightLabel("→→→");

            UIMenuItem createFemale = new UIMenuItem("Create Female Character", "Create a new female character.");
            createFemale.SetRightLabel("→→→");

            MainMenu.Mp.Add(createMaleMenu);
            MainMenu.Mp.Add(createFemaleMenu);
            MainMenu.Mp.Add(savedCharactersMenu);

            CreateMaleCreatorMenu();
            CreateFemaleCreatorMenu();
            CreateSavedPedsMenu();

            menu.AddItem(createMale);
            menu.BindMenuToItem(createMaleMenu, createMale);

            menu.AddItem(createFemale);
            menu.BindMenuToItem(createFemaleMenu, createFemale);

            menu.AddItem(savedCharacters);
            menu.BindMenuToItem(savedCharactersMenu, savedCharacters);

            menu.UpdateScaleform();

            menu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            createMaleMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            createFemaleMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));

            createMaleMenu.UpdateScaleform();
            createFemaleMenu.UpdateScaleform();
            savedCharactersMenu.UpdateScaleform();

            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == createMale)
                {
                    cf.SetPlayerSkin("mp_m_freemode_01", new CommonFunctions.PedInfo() { version = -1 });
                    SetPedHeadBlendData(PlayerPedId(), 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 0f, false);
                }
                else if (item == savedCharacters)
                {
                    UpdateSavedPedsMenu();
                }
                else if (item == createFemale)
                {
                    await BaseScript.Delay(100);
                    createFemaleMenu.GoBack();
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

        /// <summary>
        /// Creates the female creator menu.
        /// </summary>
        private void CreateFemaleCreatorMenu()
        {
            UIMenuItem save = new UIMenuItem("Save", "Save the character, note if you do not save this character before exiting this menu you will lose all customization done to this character.");
            createFemaleMenu.AddItem(save);
            createFemaleMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == save)
                {
                    cf.SaveMpPed(isMale: false);
                }
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

        /// <summary>
        /// Creates the male creator menu.
        /// </summary>
        private void CreateMaleCreatorMenu()
        {
            #region inheritance menu
            MainMenu.Mp.Add(maleInheritance);
            UIMenuItem inheritanceBtn = new UIMenuItem("Inheritance", "Character inheritance options.");
            inheritanceBtn.SetRightLabel("→→→");
            createMaleMenu.AddItem(inheritanceBtn);
            createMaleMenu.BindMenuToItem(maleInheritance, inheritanceBtn);

            List<dynamic> dads = new List<dynamic>();
            for (int dad = 0; dad < 45; dad++)
            {
                dads.Add($"Father #{dad}");
            }
            List<dynamic> moms = new List<dynamic>();
            for (int mom = 0; mom < 45; mom++)
            {
                moms.Add($"Mother #{mom}");
            }
            UIMenuListItem dadList = new UIMenuListItem("Father", dads, 0, "Select a dad to inherit from.");
            UIMenuListItem momList = new UIMenuListItem("Mother", moms, 0, "Select a dad to inherit from.");
            List<dynamic> mixValues = new List<dynamic>() { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
            UIMenuSliderItem shapePercent = new UIMenuSliderItem("Shape %", mixValues, 5);
            UIMenuSliderItem skinPercent = new UIMenuSliderItem("Skin %", mixValues, 5);

            maleInheritance.AddItem(dadList);
            maleInheritance.AddItem(momList);
            maleInheritance.AddItem(shapePercent);
            maleInheritance.AddItem(skinPercent);

            void UpdateHeadBlend()
            {
                SetPedHeadBlendData(PlayerPedId(), dadList.Index, momList.Index, 0, dadList.Index, momList.Index, 0, mixValues[shapePercent.Index], mixValues[skinPercent.Index], 0f, false);
            };

            maleInheritance.OnListChange += (sender, item, index) =>
            {
                UpdateHeadBlend();
            };

            maleInheritance.OnSliderChange += (sender, item, index) =>
            {
                UpdateHeadBlend();
            };
            #endregion

            #region appearacnce menu
            MainMenu.Mp.Add(maleAppearance);
            UIMenuItem maleAppearanceBtn = new UIMenuItem("Appearance", "Character appearance options.");
            maleAppearanceBtn.SetRightLabel("→→→");
            createMaleMenu.AddItem(maleAppearanceBtn);
            createMaleMenu.BindMenuToItem(maleAppearance, maleAppearanceBtn);

            List<dynamic> hairStylesList = new List<dynamic>();
            List<dynamic> overlayColorsList = new List<dynamic>();
            List<dynamic> eyebrowsList = new List<dynamic>() { "No eyebrows" };
            List<dynamic> beardsList = new List<dynamic>() { "No facial hair" };

            for (int i = 0; i < GetNumberOfPedDrawableVariations(PlayerPedId(), 2); i++) // hair styles
            {
                hairStylesList.Add($"Style #{i}");
            }
            for (int i = 0; i < GetNumHairColors(); i++) // overlay colors
            {
                overlayColorsList.Add($"Color #{i}");
            }
            for (int i = 0; i < GetNumHeadOverlayValues(2); i++) // eyebrows
            {
                eyebrowsList.Add($"Style #{i}");
            }
            for (int i = 0; i < GetNumHeadOverlayValues(1); i++) // beards
            {
                beardsList.Add($"Style #{i}");
            }

            UIMenuListItem hairStyles = new UIMenuListItem("Hair Style", hairStylesList, 0, "Choose a hair style.");
            UIMenuListItem hairColors = new UIMenuListItem("Hair Color", overlayColorsList, 0, "Choose your hair color.");
            UIMenuListItem hairHighlightColors = new UIMenuListItem("Hair Highlight Color", overlayColorsList, 0, "Choose your hair highlight color.");
            UIMenuListItem eyebrows = new UIMenuListItem("Eyebrows", eyebrowsList, 0, "Eyebrows overlay.");
            UIMenuListItem eyebrowsColors = new UIMenuListItem("Eyebrows Color", overlayColorsList, 0, "Eyebrows overlay color.");
            UIMenuListItem beards = new UIMenuListItem("Beards / Facial Hair", beardsList, 0, "Beards and other facial hair styles.");
            UIMenuListItem beardColors = new UIMenuListItem("Beard Colors", overlayColorsList, 0, "Beards and other facial hair styles color.");


            maleAppearance.AddItem(hairStyles);
            maleAppearance.AddItem(hairColors);
            maleAppearance.AddItem(hairHighlightColors);
            maleAppearance.AddItem(eyebrows);
            maleAppearance.AddItem(eyebrowsColors);
            maleAppearance.AddItem(beards);
            maleAppearance.AddItem(beardColors);


            maleAppearance.OnListChange += (sender, item, index) =>
            {
                if (item == hairStyles)
                {
                    SetPedComponentVariation(PlayerPedId(), 2, hairStyles.Index, 0, 0);
                }
                else if (item == hairColors || item == hairHighlightColors)
                {
                    SetPedHairColor(PlayerPedId(), hairColors.Index, hairHighlightColors.Index);
                }
                else if (item == eyebrows)
                {
                    if (index == 0)
                        SetPedHeadOverlay(PlayerPedId(), 2, 255, 1f);
                    else
                        SetPedHeadOverlay(PlayerPedId(), 2, eyebrows.Index - 1, 1f);

                }
                else if (item == eyebrowsColors)
                {
                    SetPedHeadOverlayColor(PlayerPedId(), 2, 1, eyebrowsColors.Index, eyebrowsColors.Index);
                }
                else if (item == beards)
                {
                    if (index == 0)
                        SetPedHeadOverlay(PlayerPedId(), 1, 255, 1f);
                    else
                        SetPedHeadOverlay(PlayerPedId(), 1, beards.Index - 1, 1f);
                }
                else if (item == beardColors)
                {
                    SetPedHeadOverlayColor(PlayerPedId(), 1, 1, beardColors.Index, beardColors.Index);
                }
            };
            #endregion

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


            UIMenuItem save = new UIMenuItem("Save", "Save the character, note if you do not save this character before exiting this menu you will lose all customization done to this character.");
            createMaleMenu.AddItem(save);
            createMaleMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == save)
                {
                    if (await SaveMpPed())
                    {
                        while (!createMaleMenu.Visible)
                        {
                            await BaseScript.Delay(0);
                        }
                        createMaleMenu.GoBack();
                    }
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
