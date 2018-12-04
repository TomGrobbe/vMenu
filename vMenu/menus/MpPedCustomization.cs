using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using static vMenuClient.MpPedDataManager;

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
        //public UIMenu editCharacterMenu = new UIMenu("vMenu", "Edit Your Saved Character", true)
        //{
        //    ScaleWithSafezone = false,
        //    MouseControlsEnabled = false,
        //    MouseEdgeEnabled = false,
        //    ControlDisablingEnabled = false
        //};
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
        private UIMenu manageSavedCharacterMenu = new UIMenu("vMenu", "Manage MP Character", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public static bool DontCloseMenus = false;
        public static bool DisableBackButton = false;
        string selectedSavedCharacterManageName = "";
        private bool isEdidtingPed = false;

        private MultiplayerPedData currentCharacter = new MultiplayerPedData();

        /// <summary>
        /// Makes or updates the character creator menu. Also has an option to load data from the <see cref="currentCharacter"/> data, to allow for editing an existing ped.
        /// </summary>
        /// <param name="male"></param>
        /// <param name="editPed"></param>
        private void MakeCreateCharacterMenu(bool male, bool editPed = false)
        {
            isEdidtingPed = editPed;
            if (!editPed)
            {
                currentCharacter = new MultiplayerPedData();
                currentCharacter.DrawableVariations.clothes = new Dictionary<int, KeyValuePair<int, int>>();
                currentCharacter.PropVariations.props = new Dictionary<int, KeyValuePair<int, int>>();
                currentCharacter.PedHeadBlendData = Game.PlayerPed.GetHeadBlendData();
                currentCharacter.Version = 1;
                currentCharacter.ModelHash = male ? (uint)GetHashKey("mp_m_freemode_01") : (uint)GetHashKey("mp_f_freemode_01");
                currentCharacter.IsMale = male;

                SetPedComponentVariation(Game.PlayerPed.Handle, 3, 15, 0, 0);
                SetPedComponentVariation(Game.PlayerPed.Handle, 8, 15, 0, 0);
                SetPedComponentVariation(Game.PlayerPed.Handle, 11, 15, 0, 0);
            }
            if (currentCharacter.DrawableVariations.clothes == null)
            {
                currentCharacter.DrawableVariations.clothes = new Dictionary<int, KeyValuePair<int, int>>();
            }
            if (currentCharacter.PropVariations.props == null)
            {
                currentCharacter.PropVariations.props = new Dictionary<int, KeyValuePair<int, int>>();
            }

            appearanceMenu.Clear();
            //faceShapeMenu.Clear();
            clothesMenu.Clear();
            propsMenu.Clear();

            #region appearance menu.


            List<dynamic> opacity = new List<dynamic>() { "0.0", "0.1", "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9", "1.0" };

            List<dynamic> overlayColorsList = new List<dynamic>();
            for (int i = 0; i < GetNumHairColors(); i++)
            {
                overlayColorsList.Add($"Color #{i}");
            }

            List<dynamic> hairStylesList = new List<dynamic>();
            for (int i = 0; i < GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2); i++)
            {
                hairStylesList.Add($"Style #{i}");
            }
            hairStylesList.Add($"Style #{GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2)}");

            List<dynamic> blemishesStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(0); i++)
            {
                blemishesStyleList.Add($"Style #{i}");
            }

            List<dynamic> beardStylesList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(1); i++)
            {
                beardStylesList.Add($"Style #{i}");
            }

            List<dynamic> eyebrowsStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(2); i++)
            {
                eyebrowsStyleList.Add($"Style #{i}");
            }

            List<dynamic> ageingStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(3); i++)
            {
                ageingStyleList.Add($"Style #{i}");
            }

            List<dynamic> makeupStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(4); i++)
            {
                makeupStyleList.Add($"Style #{i}");
            }

            List<dynamic> blushStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(5); i++)
            {
                blushStyleList.Add($"Style #{i}");
            }

            List<dynamic> complexionStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(6); i++)
            {
                complexionStyleList.Add($"Style #{i}");
            }

            List<dynamic> sunDamageStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(7); i++)
            {
                sunDamageStyleList.Add($"Style #{i}");
            }

            List<dynamic> lipstickStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(8); i++)
            {
                lipstickStyleList.Add($"Style #{i}");
            }

            List<dynamic> molesFrecklesStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(9); i++)
            {
                molesFrecklesStyleList.Add($"Style #{i}");
            }

            List<dynamic> chestHairStyleList = new List<dynamic>();
            for (int i = 0; i < GetNumHeadOverlayValues(10); i++)
            {
                chestHairStyleList.Add($"Style #{i}");
            }


            List<dynamic> eyeColorList = new List<dynamic>();
            for (int i = 0; i < 32; i++)
            {
                eyeColorList.Add($"Eye Color #{i}");
            }

            /*

            0               Blemishes             0 - 23,   255  
            1               Facial Hair           0 - 28,   255  
            2               Eyebrows              0 - 33,   255  
            3               Ageing                0 - 14,   255  
            4               Makeup                0 - 74,   255  
            5               Blush                 0 - 6,    255  
            6               Complexion            0 - 11,   255  
            7               Sun Damage            0 - 10,   255  
            8               Lipstick              0 - 9,    255  
            9               Moles/Freckles        0 - 17,   255  
            10              Chest Hair            0 - 16,   255  
            11              Body Blemishes        0 - 11,   255  
            12              Add Body Blemishes    0 - 1,    255  
            
            */


            // hair
            int currentHairStyle = editPed ? currentCharacter.PedAppearance.hairStyle : GetPedDrawableVariation(Game.PlayerPed.Handle, 2);
            int currentHairColor = editPed ? currentCharacter.PedAppearance.hairColor : 0;
            int currentHairHighlightColor = editPed ? currentCharacter.PedAppearance.hairHighlightColor : 0;

            // 0 blemishes
            int currentBlemishesStyle = editPed ? currentCharacter.PedAppearance.blemishesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 0) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 0) : 0;
            float currentBlemishesOpacity = editPed ? currentCharacter.PedAppearance.blemishesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, currentBlemishesStyle, currentBlemishesOpacity);

            // 1 beard
            int currentBeardStyle = editPed ? currentCharacter.PedAppearance.beardStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 1) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 1) : 0;
            float currentBeardOpacity = editPed ? currentCharacter.PedAppearance.beardOpacity : 0f;
            int currentBeardColor = editPed ? currentCharacter.PedAppearance.beardColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, currentBeardStyle, currentBeardOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, currentBeardColor, currentBeardColor);

            // 2 eyebrows
            int currentEyebrowStyle = editPed ? currentCharacter.PedAppearance.eyebrowsStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 2) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 2) : 0;
            float currentEyebrowOpacity = editPed ? currentCharacter.PedAppearance.eyebrowsOpacity : 0f;
            int currentEyebrowColor = editPed ? currentCharacter.PedAppearance.eyebrowsColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, currentEyebrowStyle, currentEyebrowOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, currentEyebrowColor, currentEyebrowColor);

            // 3 ageing
            int currentAgeingStyle = editPed ? currentCharacter.PedAppearance.ageingStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 3) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 3) : 0;
            float currentAgeingOpacity = editPed ? currentCharacter.PedAppearance.ageingOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, currentAgeingStyle, currentAgeingOpacity);

            // 4 makeup
            int currentMakeupStyle = editPed ? currentCharacter.PedAppearance.makeupStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 4) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 4) : 0;
            float currentMakeupOpacity = editPed ? currentCharacter.PedAppearance.makeupOpacity : 0f;
            int currentMakeupColor = editPed ? currentCharacter.PedAppearance.makeupColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, currentMakeupStyle, currentMakeupOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, currentMakeupColor, currentMakeupColor);

            // 5 blush
            int currentBlushStyle = editPed ? currentCharacter.PedAppearance.blushStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 5) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 5) : 0;
            float currentBlushOpacity = editPed ? currentCharacter.PedAppearance.blushOpacity : 0f;
            int currentBlushColor = editPed ? currentCharacter.PedAppearance.blushColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, currentBlushStyle, currentBlushOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, currentBlushColor, currentBlushColor);

            // 6 complexion
            int currentComplexionStyle = editPed ? currentCharacter.PedAppearance.complexionStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 6) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 6) : 0;
            float currentComplexionOpacity = editPed ? currentCharacter.PedAppearance.complexionOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, currentComplexionStyle, currentComplexionOpacity);

            // 7 sun damage
            int currentSunDamageStyle = editPed ? currentCharacter.PedAppearance.sunDamageStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 7) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 7) : 0;
            float currentSunDamageOpacity = editPed ? currentCharacter.PedAppearance.sunDamageOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, currentSunDamageStyle, currentSunDamageOpacity);

            // 8 lipstick
            int currentLipstickStyle = editPed ? currentCharacter.PedAppearance.lipstickStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 8) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 8) : 0;
            float currentLipstickOpacity = editPed ? currentCharacter.PedAppearance.lipstickOpacity : 0f;
            int currentLipstickColor = editPed ? currentCharacter.PedAppearance.lipstickColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, currentLipstickStyle, currentLipstickOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, currentLipstickColor, currentLipstickColor);

            // 9 moles/freckles
            int currentMolesFrecklesStyle = editPed ? currentCharacter.PedAppearance.molesFrecklesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 9) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 9) : 0;
            float currentMolesFrecklesOpacity = editPed ? currentCharacter.PedAppearance.molesFrecklesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, currentMolesFrecklesStyle, currentMolesFrecklesOpacity);

            // 10 chest hair
            int currentChesthairStyle = editPed ? currentCharacter.PedAppearance.chestHairStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 10) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 10) : 0;
            float currentChesthairOpacity = editPed ? currentCharacter.PedAppearance.chestHairOpacity : 0f;
            int currentChesthairColor = editPed ? currentCharacter.PedAppearance.chestHairColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, currentChesthairStyle, currentChesthairOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, currentChesthairColor, currentChesthairColor);

            int currentEyeColor = editPed ? currentCharacter.PedAppearance.eyeColor : 0;
            SetPedEyeColor(Game.PlayerPed.Handle, currentEyeColor);

            UIMenuListItem hairStyles = new UIMenuListItem("Hair Style", hairStylesList, currentHairStyle, "Select a hair style.");
            UIMenuListItem hairColors = new UIMenuListItem("Hair Color", overlayColorsList, currentHairColor, "Select a hair color.");
            UIMenuListItem hairHighlightColors = new UIMenuListItem("Hair Highlight Color", overlayColorsList, currentHairHighlightColor, "Select a hair highlight color.");

            UIMenuListItem blemishesStyle = new UIMenuListItem("Blemishes Style", blemishesStyleList, currentBlemishesStyle, "Select a blemishes style.");
            UIMenuSliderItem blemishesOpacity = new UIMenuSliderItem("Blemishes Opacity", opacity, (int)(currentBlemishesOpacity * 10f), "Select a blemishes opacity.");

            UIMenuListItem beardStyles = new UIMenuListItem("Beard Style", beardStylesList, currentBeardStyle, "Select a beard/facial hair style.");
            UIMenuSliderItem beardOpacity = new UIMenuSliderItem("Beard Opacity", opacity, (int)(currentBeardOpacity * 10f), "Select the opacity for your beard/facial hair.");
            UIMenuListItem beardColor = new UIMenuListItem("Beard Color", overlayColorsList, currentBeardColor, "Select a beard color");

            UIMenuListItem eyebrowStyle = new UIMenuListItem("Eyebrows Style", eyebrowsStyleList, currentEyebrowStyle, "Select an eyebrows style.");
            UIMenuSliderItem eyebrowOpacity = new UIMenuSliderItem("Eyebrows Opacity", opacity, (int)(currentEyebrowOpacity * 10f), "Select the opacity for your eyebrows.");
            UIMenuListItem eyebrowColor = new UIMenuListItem("Eyebrows Color", overlayColorsList, currentEyebrowColor, "Select an eyebrows color.");

            UIMenuListItem ageingStyle = new UIMenuListItem("Ageing Style", ageingStyleList, currentAgeingStyle, "Select an ageing style.");
            UIMenuSliderItem ageingOpacity = new UIMenuSliderItem("Ageing Opacity", opacity, (int)(currentAgeingOpacity * 10f), "Select an ageing opacity.");

            UIMenuListItem makeupStyle = new UIMenuListItem("Makeup Style", makeupStyleList, currentMakeupStyle, "Select a makeup style.");
            UIMenuSliderItem makeupOpacity = new UIMenuSliderItem("Makeup Opacity", opacity, (int)(currentMakeupOpacity * 10f), "Select a makeup opacity.");
            UIMenuListItem makeupColor = new UIMenuListItem("Makeup Color", overlayColorsList, currentMakeupColor, "Select a makeup color.");

            UIMenuListItem blushStyle = new UIMenuListItem("Blush Style", blushStyleList, currentBlushStyle, "Select a blush style.");
            UIMenuSliderItem blushOpacity = new UIMenuSliderItem("Blush Opacity", opacity, (int)(currentBlushOpacity * 10f), "Select a blush opacity.");
            UIMenuListItem blushColor = new UIMenuListItem("Blush Color", overlayColorsList, currentBlushColor, "Select a blush color.");

            UIMenuListItem complexionStyle = new UIMenuListItem("Complexion Style", complexionStyleList, currentComplexionStyle, "Select a complexion style.");
            UIMenuSliderItem complexionOpacity = new UIMenuSliderItem("Complexion Opacity", opacity, (int)(currentComplexionOpacity * 10f), "Select a complexion opacity.");

            UIMenuListItem sunDamageStyle = new UIMenuListItem("Sun Damage Style", sunDamageStyleList, currentSunDamageStyle, "Select a sun damage style.");
            UIMenuSliderItem sunDamageOpacity = new UIMenuSliderItem("Sun Damage Opacity", opacity, (int)(currentSunDamageOpacity * 10f), "Select a sun damage opacity.");

            UIMenuListItem lipstickStyle = new UIMenuListItem("Lipstick Style", lipstickStyleList, currentLipstickStyle, "Select a lipstick style.");
            UIMenuSliderItem lipstickOpacity = new UIMenuSliderItem("Lipstick Opacity", opacity, (int)(currentLipstickOpacity * 10f), "Select a lipstick opacity.");
            UIMenuListItem lipstickColor = new UIMenuListItem("Lipstick Color", overlayColorsList, currentLipstickColor, "Select a lipstick color.");

            UIMenuListItem molesFrecklesStyle = new UIMenuListItem("Moles and Freckles Style", molesFrecklesStyleList, currentMolesFrecklesStyle, "Select a moles and freckles style.");
            UIMenuSliderItem molesFrecklesOpacity = new UIMenuSliderItem("Moles and Freckles Opacity", opacity, (int)(currentMolesFrecklesOpacity * 10f), "Select a moles and freckles opacity.");

            UIMenuListItem chestHairStyle = new UIMenuListItem("Chest Hair Style", chestHairStyleList, currentChesthairStyle, "Select a chest hair style.");
            UIMenuSliderItem chestHairOpacity = new UIMenuSliderItem("Chest Hair Opacity", opacity, (int)(currentChesthairOpacity * 10f), "Select a chest hair opacity.");
            UIMenuListItem chestHairColor = new UIMenuListItem("Chest Hair Color", overlayColorsList, currentChesthairColor, "Select a chest hair color.");

            UIMenuListItem eyeColor = new UIMenuListItem("Eye Colors", eyeColorList, currentEyeColor, "Select an eye/contact lens color.");

            appearanceMenu.AddItem(hairStyles);
            appearanceMenu.AddItem(hairColors);
            appearanceMenu.AddItem(hairHighlightColors);

            appearanceMenu.AddItem(blemishesStyle);
            appearanceMenu.AddItem(blemishesOpacity);

            appearanceMenu.AddItem(beardStyles);
            appearanceMenu.AddItem(beardOpacity);
            appearanceMenu.AddItem(beardColor);

            appearanceMenu.AddItem(eyebrowStyle);
            appearanceMenu.AddItem(eyebrowOpacity);
            appearanceMenu.AddItem(eyebrowColor);

            appearanceMenu.AddItem(ageingStyle);
            appearanceMenu.AddItem(ageingOpacity);

            appearanceMenu.AddItem(makeupStyle);
            appearanceMenu.AddItem(makeupOpacity);
            appearanceMenu.AddItem(makeupColor);

            appearanceMenu.AddItem(blushStyle);
            appearanceMenu.AddItem(blushOpacity);
            appearanceMenu.AddItem(blushColor);

            appearanceMenu.AddItem(complexionStyle);
            appearanceMenu.AddItem(complexionOpacity);

            appearanceMenu.AddItem(sunDamageStyle);
            appearanceMenu.AddItem(sunDamageOpacity);

            appearanceMenu.AddItem(lipstickStyle);
            appearanceMenu.AddItem(lipstickOpacity);
            appearanceMenu.AddItem(lipstickColor);

            appearanceMenu.AddItem(molesFrecklesStyle);
            appearanceMenu.AddItem(molesFrecklesOpacity);

            appearanceMenu.AddItem(chestHairStyle);
            appearanceMenu.AddItem(chestHairOpacity);
            appearanceMenu.AddItem(chestHairColor);

            appearanceMenu.AddItem(eyeColor);

            if (male)
            {
                // There are weird people out there that wanted makeup for male characters
                // so yeah.... here you go I suppose... strange...

                /*
                makeupStyle.Enabled = false;
                makeupStyle.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                makeupStyle.Description = "This is not available for male characters.";

                makeupOpacity.Enabled = false;
                makeupOpacity.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                makeupOpacity.Description = "This is not available for male characters.";

                makeupColor.Enabled = false;
                makeupColor.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                makeupColor.Description = "This is not available for male characters.";


                blushStyle.Enabled = false;
                blushStyle.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                blushStyle.Description = "This is not available for male characters.";

                blushOpacity.Enabled = false;
                blushOpacity.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                blushOpacity.Description = "This is not available for male characters.";

                blushColor.Enabled = false;
                blushColor.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                blushColor.Description = "This is not available for male characters.";


                lipstickStyle.Enabled = false;
                lipstickStyle.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                lipstickStyle.Description = "This is not available for male characters.";

                lipstickOpacity.Enabled = false;
                lipstickOpacity.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                lipstickOpacity.Description = "This is not available for male characters.";

                lipstickColor.Enabled = false;
                lipstickColor.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                lipstickColor.Description = "This is not available for male characters.";
                */
            }
            else
            {
                beardStyles.Enabled = false;
                beardStyles.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                beardStyles.Description = "This is not available for female characters.";

                beardOpacity.Enabled = false;
                beardOpacity.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                beardOpacity.Description = "This is not available for female characters.";

                beardColor.Enabled = false;
                beardColor.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                beardColor.Description = "This is not available for female characters.";


                chestHairStyle.Enabled = false;
                chestHairStyle.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                chestHairStyle.Description = "This is not available for female characters.";

                chestHairOpacity.Enabled = false;
                chestHairOpacity.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                chestHairOpacity.Description = "This is not available for female characters.";

                chestHairColor.Enabled = false;
                chestHairColor.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                chestHairColor.Description = "This is not available for female characters.";
            }

            #endregion

            #region clothing options menu
            string[] clothingCategoryNames = new string[12] { "Unused (head)", "Masks", "Unused (hair)", "Upper Body", "Lower Body", "Bags & Parachutes", "Shoes", "Scarfs & Chains", "Shirt & Accessory", "Body Armor & Accessory 2", "Badges & Logos", "Shirt Overlay & Jackets" };
            for (int i = 0; i < 12; i++)
            {
                if (i != 0 && i != 2)
                {
                    int currentVariationIndex = editPed && currentCharacter.DrawableVariations.clothes.ContainsKey(i) ? currentCharacter.DrawableVariations.clothes[i].Key : GetPedDrawableVariation(Game.PlayerPed.Handle, i);
                    int currentVariationTextureIndex = editPed && currentCharacter.DrawableVariations.clothes.ContainsKey(i) ? currentCharacter.DrawableVariations.clothes[i].Value : GetPedTextureVariation(Game.PlayerPed.Handle, i);

                    List<dynamic> items = new List<dynamic>();
                    for (int x = 0; x < GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, i); x++)
                    {
                        items.Add($"Drawable #{x} (of {GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, i)})");
                    }

                    UIMenuListItem listItem = new UIMenuListItem(clothingCategoryNames[i], items, currentVariationIndex, $"Select a drawable using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{currentVariationTextureIndex}.");
                    clothesMenu.AddItem(listItem);
                }
            }
            #endregion

            #region props options menu
            string[] propNames = new string[5] { "Hats & Helmets", "Glasses", "Misc Props", "Watches", "Bracelets" };
            for (int x = 0; x < 5; x++)
            {
                int propId = x;
                if (x > 2)
                {
                    propId += 3;
                }

                int currentProp = editPed && currentCharacter.PropVariations.props.ContainsKey(propId) ? currentCharacter.PropVariations.props[propId].Key : GetPedPropIndex(Game.PlayerPed.Handle, propId);
                int currentPropTexture = editPed && currentCharacter.PropVariations.props.ContainsKey(propId) ? currentCharacter.PropVariations.props[propId].Value : GetPedPropTextureIndex(Game.PlayerPed.Handle, propId);

                List<dynamic> propsList = new List<dynamic>();
                for (int i = 0; i < GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propId); i++)
                {
                    propsList.Add($"Prop #{i} (of {GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propId)})");
                }
                propsList.Add("No Prop");

                if (GetPedPropIndex(Game.PlayerPed.Handle, propId) != -1)
                {
                    UIMenuListItem propListItem = new UIMenuListItem($"{propNames[x]}", propsList, currentProp, $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{currentPropTexture}.");
                    propsMenu.AddItem(propListItem);
                }
                else
                {
                    UIMenuListItem propListItem = new UIMenuListItem($"{propNames[x]}", propsList, currentProp, "Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.");
                    propsMenu.AddItem(propListItem);
                }


            }
            #endregion

            #region face features menu
            foreach (UIMenuSliderItem item in faceShapeMenu.MenuItems)
            {
                if (editPed)
                {
                    if (currentCharacter.FaceShapeFeatures.features == null)
                    {
                        currentCharacter.FaceShapeFeatures.features = new Dictionary<int, float>();
                    }
                    else
                    {
                        if (currentCharacter.FaceShapeFeatures.features.ContainsKey(faceShapeMenu.MenuItems.IndexOf(item)))
                        {
                            item.Index = (int)(currentCharacter.FaceShapeFeatures.features[faceShapeMenu.MenuItems.IndexOf(item)] * 10f) + 10;
                            SetPedFaceFeature(Game.PlayerPed.Handle, faceShapeMenu.MenuItems.IndexOf(item), currentCharacter.FaceShapeFeatures.features[faceShapeMenu.MenuItems.IndexOf(item)]);
                        }
                        else
                        {
                            item.Index = 10;
                            SetPedFaceFeature(Game.PlayerPed.Handle, faceShapeMenu.MenuItems.IndexOf(item), 0f);
                        }
                    }
                }
                else
                {
                    item.Index = 10;
                    SetPedFaceFeature(PlayerPedId(), faceShapeMenu.MenuItems.IndexOf(item), 0f);
                }
            }
            #endregion

            createCharacterMenu.RefreshIndex();
            createCharacterMenu.UpdateScaleform();
            appearanceMenu.RefreshIndex();
            appearanceMenu.UpdateScaleform();
            inheritanceMenu.RefreshIndex();
            inheritanceMenu.UpdateScaleform();
        }

        /// <summary>
        /// Saves the mp character and quits the editor if successful.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SavePed()
        {
            currentCharacter.PedHeadBlendData = Game.PlayerPed.GetHeadBlendData();
            if (isEdidtingPed)
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(currentCharacter);
                if (StorageManager.SaveJsonData(currentCharacter.SaveName, json, true))
                {
                    Notify.Success("Your character was saved successfully.");
                    return true;
                }
                else
                {
                    Notify.Error("Your character could not be saved. Reason unknown. :(");
                    return false;
                }
            }
            else
            {
                string name = await cf.GetUserInput("Enter a save name.", "", 30);
                if (string.IsNullOrEmpty(name) || name == "NULL")
                {
                    Notify.Alert("You cancelled the action or you provided an invalid name.");
                    return false;
                }
                else
                {
                    currentCharacter.SaveName = "mp_ped_" + name;
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(currentCharacter);

                    if (StorageManager.SaveJsonData("mp_ped_" + name, json, false))
                    {
                        Notify.Success($"Your character (~g~<C>{name}</C>~s~) has been saved.");
                        Debug.WriteLine($"Saved Character {name}. Data: {json}");
                        return true;
                    }
                    else
                    {
                        Notify.Error($"Saving failed, most likely because this name (~y~<C>{name}</C>~s~) is already in use.");
                        return false;
                    }
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
            MainMenu.Mp.Add(savedCharactersMenu);
            MainMenu.Mp.Add(inheritanceMenu);
            MainMenu.Mp.Add(appearanceMenu);
            MainMenu.Mp.Add(faceShapeMenu);
            //MainMenu.Mp.Add(tattoosMenu);
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

            //menu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            //editCharacterMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            createCharacterMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            inheritanceMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            appearanceMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            faceShapeMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            tattoosMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            clothesMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            propsMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            createCharacterMenu.AddInstructionalButton(new InstructionalButton(Control.PhoneExtraOption, "Turn Character"));
            inheritanceMenu.AddInstructionalButton(new InstructionalButton(Control.PhoneExtraOption, "Turn Character"));
            appearanceMenu.AddInstructionalButton(new InstructionalButton(Control.PhoneExtraOption, "Turn Character"));
            faceShapeMenu.AddInstructionalButton(new InstructionalButton(Control.PhoneExtraOption, "Turn Character"));
            tattoosMenu.AddInstructionalButton(new InstructionalButton(Control.PhoneExtraOption, "Turn Character"));
            clothesMenu.AddInstructionalButton(new InstructionalButton(Control.PhoneExtraOption, "Turn Character"));
            propsMenu.AddInstructionalButton(new InstructionalButton(Control.PhoneExtraOption, "Turn Character"));

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
            //faceButton.SetRightBadge(UIMenuItem.BadgeStyle.Lock);
            //faceButton.Enabled = false;
            //faceButton.Description = "This is currently not available yet. But it will be in a future update.";

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
                SetPedHeadBlendData(Game.PlayerPed.Handle, inheritanceDads.Index, inheritanceMoms.Index, 0, inheritanceDads.Index, inheritanceMoms.Index, 0, mixValues[inheritanceShapeMix.Index], mixValues[inheritanceSkinMix.Index], 0f, false);
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

            #region appearance
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

            // manage the list changes for appearance items.
            appearanceMenu.OnListChange += (sender, item, index) =>
            {
                int itemIndex = sender.MenuItems.IndexOf(item);
                if (itemIndex == 0) // hair style
                {
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    currentCharacter.PedAppearance.HairOverlay = new KeyValuePair<string, string>("", "");

                    if (index >= GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2))
                    {
                        SetPedComponentVariation(Game.PlayerPed.Handle, 2, 0, 0, 0);
                        currentCharacter.PedAppearance.hairStyle = 0;
                    }
                    else
                    {
                        SetPedComponentVariation(Game.PlayerPed.Handle, 2, index, 0, 0);
                        currentCharacter.PedAppearance.hairStyle = index;
                        if (hairOverlays.ContainsKey(index))
                        {
                            SetPedFacialDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(hairOverlays[index].Key), (uint)GetHashKey(hairOverlays[index].Value));
                            currentCharacter.PedAppearance.HairOverlay = new KeyValuePair<string, string>(hairOverlays[index].Key, hairOverlays[index].Value);
                        }
                    }
                }
                else if (itemIndex == 1 || itemIndex == 2) // hair colors
                {
                    var tmp = (UIMenuListItem)sender.MenuItems[1];
                    int hairColor = tmp.Index;
                    tmp = (UIMenuListItem)sender.MenuItems[2];
                    int hairHighlightColor = tmp.Index;

                    SetPedHairColor(Game.PlayerPed.Handle, hairColor, hairHighlightColor);

                    currentCharacter.PedAppearance.hairColor = hairColor;
                    currentCharacter.PedAppearance.hairHighlightColor = hairHighlightColor;
                }
                else if (itemIndex == 31) // eye color
                {
                    int selection = ((UIMenuListItem)sender.MenuItems[itemIndex]).Index;
                    SetPedEyeColor(Game.PlayerPed.Handle, selection);
                    currentCharacter.PedAppearance.eyeColor = selection;
                }
                else
                {
                    int selection = ((UIMenuListItem)sender.MenuItems[itemIndex]).Index;
                    float opacity = 0f;
                    if (sender.MenuItems[itemIndex + 1] is UIMenuSliderItem)
                        opacity = (((float)((UIMenuSliderItem)sender.MenuItems[itemIndex + 1]).Index + 1) / 10f) - 0.1f;
                    else if (sender.MenuItems[itemIndex - 1] is UIMenuSliderItem)
                        opacity = (((float)((UIMenuSliderItem)sender.MenuItems[itemIndex - 1]).Index + 1) / 10f) - 0.1f;
                    else if (sender.MenuItems[itemIndex] is UIMenuSliderItem)
                        opacity = (((float)((UIMenuSliderItem)sender.MenuItems[itemIndex]).Index + 1) / 10f) - 0.1f;
                    else
                        opacity = 1f;
                    switch (itemIndex)
                    {
                        case 3: // blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, selection, opacity);
                            currentCharacter.PedAppearance.blemishesStyle = selection;
                            currentCharacter.PedAppearance.blemishesOpacity = opacity;
                            break;
                        case 5: // beards
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, selection, opacity);
                            currentCharacter.PedAppearance.beardStyle = selection;
                            currentCharacter.PedAppearance.beardOpacity = opacity;
                            break;
                        case 7: // beards color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, selection, selection);
                            currentCharacter.PedAppearance.beardColor = selection;
                            break;
                        case 8: // eyebrows
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, selection, opacity);
                            currentCharacter.PedAppearance.eyebrowsStyle = selection;
                            currentCharacter.PedAppearance.eyebrowsOpacity = opacity;
                            break;
                        case 10: // eyebrows color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, selection, selection);
                            currentCharacter.PedAppearance.eyebrowsColor = selection;
                            break;
                        case 11: // ageing
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, selection, opacity);
                            currentCharacter.PedAppearance.ageingStyle = selection;
                            currentCharacter.PedAppearance.ageingOpacity = opacity;
                            break;
                        case 13: // makeup
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, selection, opacity);
                            currentCharacter.PedAppearance.makeupStyle = selection;
                            currentCharacter.PedAppearance.makeupOpacity = opacity;
                            break;
                        case 15: // makeup color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, selection, selection);
                            currentCharacter.PedAppearance.makeupColor = selection;
                            break;
                        case 16: // blush style
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, selection, opacity);
                            currentCharacter.PedAppearance.blushStyle = selection;
                            currentCharacter.PedAppearance.blushOpacity = opacity;
                            break;
                        case 18: // blush color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, selection, selection);
                            currentCharacter.PedAppearance.blushColor = selection;
                            break;
                        case 19: // complexion
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, selection, opacity);
                            currentCharacter.PedAppearance.complexionStyle = selection;
                            currentCharacter.PedAppearance.complexionOpacity = opacity;
                            break;
                        case 21: // sun damage
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, selection, opacity);
                            currentCharacter.PedAppearance.sunDamageStyle = selection;
                            currentCharacter.PedAppearance.sunDamageOpacity = opacity;
                            break;
                        case 23: // lipstick
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, selection, opacity);
                            currentCharacter.PedAppearance.lipstickStyle = selection;
                            currentCharacter.PedAppearance.lipstickOpacity = opacity;
                            break;
                        case 25: // lipstick color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, selection, selection);
                            currentCharacter.PedAppearance.lipstickColor = selection;
                            break;
                        case 26: // moles and freckles
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, selection, opacity);
                            currentCharacter.PedAppearance.molesFrecklesStyle = selection;
                            currentCharacter.PedAppearance.molesFrecklesOpacity = opacity;
                            break;
                        case 28: // chest hair
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, selection, opacity);
                            currentCharacter.PedAppearance.chestHairStyle = selection;
                            currentCharacter.PedAppearance.chestHairOpacity = opacity;
                            break;
                        case 30: // chest hair color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, selection, selection);
                            currentCharacter.PedAppearance.chestHairColor = selection;
                            break;
                    }
                }
            };

            // manage the slider changes for opacity on the appearance items.
            appearanceMenu.OnSliderChange += (sender, item, index) =>
            {
                int itemIndex = sender.MenuItems.IndexOf(item);

                if (itemIndex > 2 && itemIndex < 31)
                {

                    int selection = ((UIMenuListItem)sender.MenuItems[itemIndex - 1]).Index;
                    float opacity = 0f;
                    if (sender.MenuItems[itemIndex] is UIMenuSliderItem)
                        opacity = (((float)((UIMenuSliderItem)sender.MenuItems[itemIndex]).Index + 1) / 10f) - 0.1f;
                    else if (sender.MenuItems[itemIndex + 1] is UIMenuSliderItem)
                        opacity = (((float)((UIMenuSliderItem)sender.MenuItems[itemIndex + 1]).Index + 1) / 10f) - 0.1f;
                    else if (sender.MenuItems[itemIndex - 1] is UIMenuSliderItem)
                        opacity = (((float)((UIMenuSliderItem)sender.MenuItems[itemIndex - 1]).Index + 1) / 10f) - 0.1f;
                    else
                        opacity = 1f;
                    switch (itemIndex)
                    {
                        case 4: // blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, selection, opacity);
                            currentCharacter.PedAppearance.blemishesStyle = selection;
                            currentCharacter.PedAppearance.blemishesOpacity = opacity;
                            break;
                        case 6: // beards
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, selection, opacity);
                            currentCharacter.PedAppearance.beardStyle = selection;
                            currentCharacter.PedAppearance.beardOpacity = opacity;
                            break;
                        case 9: // eyebrows
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, selection, opacity);
                            currentCharacter.PedAppearance.eyebrowsStyle = selection;
                            currentCharacter.PedAppearance.eyebrowsOpacity = opacity;
                            break;
                        case 12: // ageing
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, selection, opacity);
                            currentCharacter.PedAppearance.ageingStyle = selection;
                            currentCharacter.PedAppearance.ageingOpacity = opacity;
                            break;
                        case 14: // makeup
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, selection, opacity);
                            currentCharacter.PedAppearance.makeupStyle = selection;
                            currentCharacter.PedAppearance.makeupOpacity = opacity;
                            break;
                        case 17: // blush style
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, selection, opacity);
                            currentCharacter.PedAppearance.blushStyle = selection;
                            currentCharacter.PedAppearance.blushOpacity = opacity;
                            break;
                        case 20: // complexion
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, selection, opacity);
                            currentCharacter.PedAppearance.complexionStyle = selection;
                            currentCharacter.PedAppearance.complexionOpacity = opacity;
                            break;
                        case 22: // sun damage
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, selection, opacity);
                            currentCharacter.PedAppearance.sunDamageStyle = selection;
                            currentCharacter.PedAppearance.sunDamageOpacity = opacity;
                            break;
                        case 24: // lipstick
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, selection, opacity);
                            currentCharacter.PedAppearance.lipstickStyle = selection;
                            currentCharacter.PedAppearance.lipstickOpacity = opacity;
                            break;
                        case 27: // moles and freckles
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, selection, opacity);
                            currentCharacter.PedAppearance.molesFrecklesStyle = selection;
                            currentCharacter.PedAppearance.molesFrecklesOpacity = opacity;
                            break;
                        case 29: // chest hair
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, selection, opacity);
                            currentCharacter.PedAppearance.chestHairStyle = selection;
                            currentCharacter.PedAppearance.chestHairOpacity = opacity;
                            break;
                    }
                }

            };
            #endregion

            #region clothes
            clothesMenu.OnListChange += (sender, item, index) =>
            {
                int realIndex = sender.MenuItems.IndexOf(item);
                int componentIndex = realIndex + 1;
                if (realIndex > 0)
                {
                    componentIndex += 1;
                }

                int textureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, componentIndex);
                int newTextureIndex = 0;
                SetPedComponentVariation(Game.PlayerPed.Handle, componentIndex, item.Index, newTextureIndex, 0);
                if (currentCharacter.DrawableVariations.clothes == null)
                {
                    currentCharacter.DrawableVariations.clothes = new Dictionary<int, KeyValuePair<int, int>>();
                }
                currentCharacter.DrawableVariations.clothes[componentIndex] = new KeyValuePair<int, int>(item.Index, newTextureIndex);
                item.Description = $"Select a drawable using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{newTextureIndex}.";
                clothesMenu.UpdateScaleform();
            };

            clothesMenu.OnListSelect += (sender, item, index) =>
            {
                int realIndex = sender.MenuItems.IndexOf(item);
                int componentIndex = realIndex + 1;
                if (realIndex > 0)
                {
                    componentIndex += 1;
                }

                int textureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, componentIndex);
                int newTextureIndex = (GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, item.Index) - 1) < (textureIndex + 1) ? 0 : textureIndex + 1;
                SetPedComponentVariation(Game.PlayerPed.Handle, componentIndex, item.Index, newTextureIndex, 0);
                if (currentCharacter.DrawableVariations.clothes == null)
                {
                    currentCharacter.DrawableVariations.clothes = new Dictionary<int, KeyValuePair<int, int>>();
                }
                currentCharacter.DrawableVariations.clothes[componentIndex] = new KeyValuePair<int, int>(item.Index, newTextureIndex);
                item.Description = $"Select a drawable using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{newTextureIndex}.";
                clothesMenu.UpdateScaleform();
            };
            #endregion

            #region props
            propsMenu.OnListChange += (sender, item, index) =>
            {
                int realIndex = sender.MenuItems.IndexOf(item);
                int propIndex = realIndex;
                if (realIndex == 3)
                {
                    propIndex = 6;
                }
                if (realIndex == 4)
                {
                    propIndex = 7;
                }

                int textureIndex = 0;
                if (index >= GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Game.PlayerPed.Handle, propIndex);
                    if (currentCharacter.PropVariations.props == null)
                    {
                        currentCharacter.PropVariations.props = new Dictionary<int, KeyValuePair<int, int>>();
                    }
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    item.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.";
                }
                else
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, item.Index, textureIndex, true);
                    if (currentCharacter.PropVariations.props == null)
                    {
                        currentCharacter.PropVariations.props = new Dictionary<int, KeyValuePair<int, int>>();
                    }
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(item.Index, textureIndex);
                    if (GetPedPropIndex(Game.PlayerPed.Handle, propIndex) == -1)
                    {
                        item.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.";
                    }
                    else
                    {
                        item.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{textureIndex} (of {GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, item.Index) - 1}).";
                    }
                }
                propsMenu.UpdateScaleform();
            };

            propsMenu.OnListSelect += (sender, item, index) =>
            {
                int realIndex = sender.MenuItems.IndexOf(item);
                int propIndex = realIndex;
                if (realIndex == 3)
                {
                    propIndex = 6;
                }
                if (realIndex == 4)
                {
                    propIndex = 7;
                }

                int textureIndex = GetPedPropTextureIndex(Game.PlayerPed.Handle, propIndex);
                int newTextureIndex = (GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, item.Index) - 1) < (textureIndex + 1) ? 0 : textureIndex + 1;
                if (textureIndex >= GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Game.PlayerPed.Handle, propIndex);
                    if (currentCharacter.PropVariations.props == null)
                    {
                        currentCharacter.PropVariations.props = new Dictionary<int, KeyValuePair<int, int>>();
                    }
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    item.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.";
                }
                else
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, item.Index, newTextureIndex, true);
                    if (currentCharacter.PropVariations.props == null)
                    {
                        currentCharacter.PropVariations.props = new Dictionary<int, KeyValuePair<int, int>>();
                    }
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(item.Index, newTextureIndex);
                    if (GetPedPropIndex(Game.PlayerPed.Handle, propIndex) == -1)
                    {
                        item.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.";
                    }
                    else
                    {
                        item.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{newTextureIndex} (of {GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, item.Index) - 1}).";
                    }
                }
                propsMenu.UpdateScaleform();
            };
            #endregion

            #region face shape data
            /*
            Nose_Width  
            Nose_Peak_Hight  
            Nose_Peak_Lenght  
            Nose_Bone_High  
            Nose_Peak_Lowering  
            Nose_Bone_Twist  
            EyeBrown_High  
            EyeBrown_Forward  
            Cheeks_Bone_High  
            Cheeks_Bone_Width  
            Cheeks_Width  
            Eyes_Openning  
            Lips_Thickness  
            Jaw_Bone_Width 'Bone size to sides  
            Jaw_Bone_Back_Lenght 'Bone size to back  
            Chimp_Bone_Lowering 'Go Down  
            Chimp_Bone_Lenght 'Go forward  
            Chimp_Bone_Width  
            Chimp_Hole  
            Neck_Thikness  
            */

            List<dynamic> faceFeaturesValuesList = new List<dynamic>() { -1f, -.9f, -.8f, -.7f, -.6f, -.5f, -.4f, -.3f, -.2f, -.1f, 0f, .1f, .2f, .3f, .4f, .5f, .6f, .7f, .8f, .9f, 1f };
            var faceFeaturesNamesList = new string[20] { "Nose Width", "Noes Peak Height", "Nose Peak Length", "Nose Bone Height", "Nose Peak Lowering", "Nose Bone Twist", "Eyebrows Height", "Eyebrows Depth", "Cheekbones Height", "Cheekbones Width", "Cheeks Width", "Eyes Opening", "Lips Thickness", "Jaw Bone Width", "Jaw Bone Depth/Length", "Chin Height", "Chin Depth/Length", "Chin Width", "Chin Hole Size", "Neck Thickness" };
            for (int i = 0; i < 19; i++)
            {
                UIMenuSliderItem faceFeature = new UIMenuSliderItem(faceFeaturesNamesList[i], faceFeaturesValuesList, 10, $"Set the {faceFeaturesNamesList[i]} face feature value.", true);
                faceShapeMenu.AddItem(faceFeature);
            }

            faceShapeMenu.OnSliderChange += (sender, item, index) =>
            {
                if (currentCharacter.FaceShapeFeatures.features == null)
                {
                    currentCharacter.FaceShapeFeatures.features = new Dictionary<int, float>();
                }
                float value = faceFeaturesValuesList[item.Index];
                currentCharacter.FaceShapeFeatures.features[sender.MenuItems.IndexOf(item)] = value;
                SetPedFaceFeature(Game.PlayerPed.Handle, sender.MenuItems.IndexOf(item), value);
            };

            #endregion

            // handle button presses for the createCharacter menu.
            createCharacterMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == saveButton) // save ped
                {
                    if (await SavePed())
                    {
                        while (!MainMenu.Mp.IsAnyMenuOpen())
                        {
                            await BaseScript.Delay(0);
                        }

                        while (IsControlPressed(2, 201) || IsControlPressed(2, 217) || IsDisabledControlPressed(2, 201) || IsDisabledControlPressed(2, 217))
                            await BaseScript.Delay(0);
                        await BaseScript.Delay(100);

                        createCharacterMenu.GoBack();
                    }
                }
                else if (item == exitNoSave) // exit without saving
                {
                    bool confirm = false;
                    AddTextEntry("vmenu_warning_message_first_line", "Are you sure you want to exit the character creator?");
                    AddTextEntry("vmenu_warning_message_second_line", "You will lose all (unsaved) customization!");
                    createCharacterMenu.Visible = false;

                    // wait for confirmation or cancel input.
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

                    // if confirmed to discard changes quit the editor.
                    if (confirm)
                    {
                        while (IsControlPressed(2, 201) || IsControlPressed(2, 217) || IsDisabledControlPressed(2, 201) || IsDisabledControlPressed(2, 217))
                            await BaseScript.Delay(0);
                        await BaseScript.Delay(100);
                        menu.Visible = true;
                    }
                    else // otherwise cancel and go back to the editor.
                    {
                        createCharacterMenu.Visible = true;
                    }
                }
                else if (item == inheritanceButton) // update the inheritance menu anytime it's opened to prevent some weird glitch where old data is used.
                {
                    var data = Game.PlayerPed.GetHeadBlendData();
                    inheritanceDads.Index = data.FirstFaceShape;
                    inheritanceMoms.Index = data.SecondFaceShape;
                    inheritanceShapeMix.Index = (int)(data.ParentFaceShapePercent * 10f);
                    inheritanceSkinMix.Index = (int)(data.ParentSkinTonePercent * 10f);
                    inheritanceMenu.RefreshIndex();
                    inheritanceMenu.UpdateScaleform();
                }
            };

            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == createMale)
                {
                    await cf.SetPlayerSkin("mp_m_freemode_01", new CommonFunctions.PedInfo() { version = -1 });

                    //SetPlayerModel(Game.PlayerPed.Handle, currentCharacter.ModelHash);
                    ClearPedDecorations(Game.PlayerPed.Handle);
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                    SetPedHairColor(Game.PlayerPed.Handle, 0, 0);
                    SetPedEyeColor(Game.PlayerPed.Handle, 0);
                    ClearAllPedProps(Game.PlayerPed.Handle);

                    MakeCreateCharacterMenu(male: true);
                }
                else if (item == createFemale)
                {
                    await cf.SetPlayerSkin("mp_f_freemode_01", new CommonFunctions.PedInfo() { version = -1 });

                    //SetPlayerModel(Game.PlayerPed.Handle, currentCharacter.ModelHash);
                    ClearPedDecorations(Game.PlayerPed.Handle);
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                    SetPedHairColor(Game.PlayerPed.Handle, 0, 0);
                    SetPedEyeColor(Game.PlayerPed.Handle, 0);
                    ClearAllPedProps(Game.PlayerPed.Handle);

                    MakeCreateCharacterMenu(male: false);
                }
                else if (item == savedCharacters)
                {
                    UpdateSavedPedsMenu();
                }
            };
        }



        /// <summary>
        /// Spawns the ped from the data inside <see cref="currentCharacter"/>.
        /// Character data MUST be set BEFORE calling this function.
        /// </summary>
        /// <returns></returns>
        private async Task SpawnSavedPed()
        {
            if (currentCharacter.Version < 1)
            {
                return;
            }
            if (IsModelInCdimage(currentCharacter.ModelHash))
            {
                if (!HasModelLoaded(currentCharacter.ModelHash))
                {
                    RequestModel(currentCharacter.ModelHash);
                    while (!HasModelLoaded(currentCharacter.ModelHash))
                    {
                        await BaseScript.Delay(0);
                    }
                }

                // for some weird reason, using SetPlayerModel here does not work, it glitches out and makes the player have what seems to be both male
                // and female ped at the same time.. really fucking weird. Only the cf.SetPlayerSkin function seems to work some how. I really have no clue.
                await cf.SetPlayerSkin(currentCharacter.ModelHash, new CommonFunctions.PedInfo() { version = -1 }, true);
                // SetPlayerModel(Game.PlayerPed.Handle, currentCharacter.IsMale ? (uint)GetHashKey("mp_m_freemode_01") : (uint)GetHashKey("mp_f_freemode_01"));
                // SetPlayerModel(Game.PlayerPed.Handle, currentCharacter.ModelHash);

                ClearPedDecorations(Game.PlayerPed.Handle);
                ClearPedFacialDecorations(Game.PlayerPed.Handle);
                SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                SetPedHairColor(Game.PlayerPed.Handle, 0, 0);
                SetPedEyeColor(Game.PlayerPed.Handle, 0);
                ClearAllPedProps(Game.PlayerPed.Handle);

                #region headblend
                var data = currentCharacter.PedHeadBlendData;
                SetPedHeadBlendData(Game.PlayerPed.Handle, data.FirstFaceShape, data.SecondFaceShape, data.ThirdFaceShape, data.FirstSkinTone, data.SecondSkinTone, data.ThirdSkinTone, data.ParentFaceShapePercent, data.ParentSkinTonePercent, data.ParentThirdUnkPercent, data.IsParentInheritance);

                while (!HasPedHeadBlendFinished(Game.PlayerPed.Handle))
                {
                    await BaseScript.Delay(0);
                }
                #endregion

                #region appearance
                var appData = currentCharacter.PedAppearance;
                // hair
                SetPedComponentVariation(Game.PlayerPed.Handle, 2, appData.hairStyle, 0, 0);
                SetPedHairColor(Game.PlayerPed.Handle, appData.hairColor, appData.hairHighlightColor);
                if (!string.IsNullOrEmpty(appData.HairOverlay.Key) && !string.IsNullOrEmpty(appData.HairOverlay.Value))
                {
                    SetPedFacialDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(appData.HairOverlay.Key), (uint)GetHashKey(appData.HairOverlay.Value));
                }
                // blemishes
                SetPedHeadOverlay(Game.PlayerPed.Handle, 0, appData.blemishesStyle, appData.blemishesOpacity);
                // bread
                SetPedHeadOverlay(Game.PlayerPed.Handle, 1, appData.beardStyle, appData.beardOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, appData.beardColor, appData.beardColor);
                // eyebrows
                SetPedHeadOverlay(Game.PlayerPed.Handle, 2, appData.eyebrowsStyle, appData.eyebrowsOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, appData.eyebrowsColor, appData.eyebrowsColor);
                // ageing
                SetPedHeadOverlay(Game.PlayerPed.Handle, 3, appData.ageingStyle, appData.ageingOpacity);
                // makeup
                SetPedHeadOverlay(Game.PlayerPed.Handle, 4, appData.makeupStyle, appData.makeupOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, appData.makeupColor, appData.makeupColor);
                // blush
                SetPedHeadOverlay(Game.PlayerPed.Handle, 5, appData.blushStyle, appData.blushOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, appData.blushColor, appData.blushColor);
                // complexion
                SetPedHeadOverlay(Game.PlayerPed.Handle, 6, appData.complexionStyle, appData.complexionOpacity);
                // sundamage
                SetPedHeadOverlay(Game.PlayerPed.Handle, 7, appData.sunDamageStyle, appData.sunDamageOpacity);
                // lipstick
                SetPedHeadOverlay(Game.PlayerPed.Handle, 8, appData.lipstickStyle, appData.lipstickOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, appData.lipstickColor, appData.lipstickColor);
                // moles and freckles
                SetPedHeadOverlay(Game.PlayerPed.Handle, 9, appData.molesFrecklesStyle, appData.molesFrecklesOpacity);
                // chest hair 
                SetPedHeadOverlay(Game.PlayerPed.Handle, 10, appData.chestHairStyle, appData.chestHairOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, appData.chestHairColor, appData.chestHairColor);
                // eyecolor
                SetPedEyeColor(Game.PlayerPed.Handle, appData.eyeColor);
                #endregion

                #region Face Shape Data
                for (var i = 0; i < 19; i++)
                {
                    SetPedFaceFeature(Game.PlayerPed.Handle, i, 0f);
                }


                if (currentCharacter.FaceShapeFeatures.features != null)
                {
                    foreach (var t in currentCharacter.FaceShapeFeatures.features)
                    {
                        SetPedFaceFeature(Game.PlayerPed.Handle, t.Key, t.Value);
                    }
                }
                else
                {
                    currentCharacter.FaceShapeFeatures.features = new Dictionary<int, float>();
                }

                #endregion

                #region Clothing Data
                if (currentCharacter.DrawableVariations.clothes != null && currentCharacter.DrawableVariations.clothes.Count > 0)
                {
                    foreach (var cd in currentCharacter.DrawableVariations.clothes)
                    {
                        SetPedComponentVariation(Game.PlayerPed.Handle, cd.Key, cd.Value.Key, cd.Value.Value, 0);
                    }
                }
                #endregion

                #region Props Data
                if (currentCharacter.PropVariations.props != null && currentCharacter.PropVariations.props.Count > 0)
                {
                    foreach (var cd in currentCharacter.PropVariations.props)
                    {
                        if (cd.Value.Key > -1)
                        {
                            SetPedPropIndex(Game.PlayerPed.Handle, cd.Key, cd.Value.Key, cd.Value.Value > -1 ? cd.Value.Value : 0, true);
                        }
                    }
                }
                #endregion
            }
        }


        /// <summary>
        /// Creates the saved mp characters menu.
        /// </summary>
        private void CreateSavedPedsMenu()
        {

            UpdateSavedPedsMenu();

            MainMenu.Mp.Add(manageSavedCharacterMenu);

            UIMenuItem spawnPed = new UIMenuItem("Spawn Saved Character", "Spawns the selected saved character.");
            UIMenuItem editPed = new UIMenuItem("Edit Saved Character", "This allows you to edit everything about your saved character. The changes will be saved to this character's save file entry once you hit the save button.");
            UIMenuItem clonePed = new UIMenuItem("Clone Saved Character", "This will make a clone of your saved character. It will ask you to provide a name for that character. If that name is already taken the action will be canceled.");
            UIMenuItem delPed = new UIMenuItem("Delete Saved Character", "Deletes the selected saved character. This can not be undone!");
            delPed.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
            manageSavedCharacterMenu.AddItem(spawnPed);
            manageSavedCharacterMenu.AddItem(editPed);
            manageSavedCharacterMenu.AddItem(clonePed);
            manageSavedCharacterMenu.AddItem(delPed);

            manageSavedCharacterMenu.BindMenuToItem(createCharacterMenu, editPed);
            manageSavedCharacterMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == editPed)
                {
                    currentCharacter = StorageManager.GetSavedMpCharacterData(selectedSavedCharacterManageName);

                    await SpawnSavedPed();

                    MakeCreateCharacterMenu(male: currentCharacter.IsMale, editPed: true);
                }
                else if (item == spawnPed)
                {
                    currentCharacter = StorageManager.GetSavedMpCharacterData(selectedSavedCharacterManageName);

                    await SpawnSavedPed();
                }
                else if (item == clonePed)
                {
                    var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);
                    string name = await cf.GetUserInput("Enter a name for the cloned character", "", 30);
                    if (string.IsNullOrEmpty(name) || name.ToUpper() == "NULL")
                    {
                        Notify.Error(CommonErrors.InvalidSaveName);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GetResourceKvpString("mp_ped_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                        }
                        else
                        {
                            tmpCharacter.SaveName = "mp_ped_" + name;
                            if (StorageManager.SaveJsonData("mp_ped_" + name, Newtonsoft.Json.JsonConvert.SerializeObject(tmpCharacter), false))
                            {
                                Notify.Success($"Your character has been cloned. The name of the cloned character is: ~g~<C>{name}</C>~s~.");
                                UpdateSavedPedsMenu();
                            }
                            else
                            {
                                Notify.Error("The clone could not be created, reason unknown. Does a character already exist with that name? :(");
                            }
                        }
                    }
                }
                else if (item == delPed)
                {
                    if (delPed.RightLabel == "Are you sure?")
                    {
                        delPed.SetRightLabel("");
                        DeleteResourceKvp("mp_ped_" + selectedSavedCharacterManageName);
                        Notify.Success("Your saved character has been deleted.");
                        manageSavedCharacterMenu.GoBack();
                        UpdateSavedPedsMenu();
                        manageSavedCharacterMenu.RefreshIndex();
                        manageSavedCharacterMenu.UpdateScaleform();
                    }
                    else
                    {
                        delPed.SetRightLabel("Are you sure?");
                        manageSavedCharacterMenu.UpdateScaleform();
                    }
                }

                if (item != delPed)
                {
                    if (delPed.RightLabel == "Are you sure?")
                    {
                        delPed.SetRightLabel("");
                    }
                }
            };

            // reset the "are you sure" state.
            manageSavedCharacterMenu.OnMenuClose += (sender) =>
            {
                manageSavedCharacterMenu.MenuItems[2].SetRightLabel("");
            };

            savedCharactersMenu.OnItemSelect += (sender, item, index) =>
            {
                selectedSavedCharacterManageName = item.Text;
                manageSavedCharacterMenu.Subtitle.Caption = item.Text;
                manageSavedCharacterMenu.RefreshIndex();
                manageSavedCharacterMenu.UpdateScaleform();
            };
        }

        /// <summary>
        /// Updates the saved peds menu.
        /// </summary>
        private void UpdateSavedPedsMenu()
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
                    UIMenuItem btn = new UIMenuItem(item, "Click to spawn, edit or delete this saved character.");
                    btn.SetRightLabel("→→→");
                    savedCharactersMenu.AddItem(btn);
                    savedCharactersMenu.BindMenuToItem(manageSavedCharacterMenu, btn);
                }
            }
            savedCharactersMenu.RefreshIndex();
            savedCharactersMenu.UpdateScaleform();
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
