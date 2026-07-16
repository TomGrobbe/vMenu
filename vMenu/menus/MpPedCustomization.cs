using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuClient.MpPedDataManager;
using static vMenuShared.ConfigManager;

namespace vMenuClient.menus
{
    public class MpPedCustomization
    {
        // Variables
        private Menu menu;
        public Menu createCharacterMenu = new("创建角色", "创建一个新角色");
        public Menu savedCharactersMenu = new("vMenu", "管理已保存的角色");
        public Menu savedCharactersCategoryMenu = new("类别", "在运行时更新！");
        public Menu inheritanceMenu = new("vMenu", "角色继承选项");
        public Menu appearanceMenu = new("vMenu", "角色外观选项");
        public Menu faceShapeMenu = new("vMenu", "角色脸型选项");
        public Menu tattoosMenu = new("vMenu", "角色纹身选项");
        public Menu clothesMenu = new("vMenu", "角色服装选项");
        public Menu propsMenu = new("vMenu", "角色配饰选项");
        private readonly Menu manageSavedCharacterMenu = new("vMenu", "管理多人角色");

        public static List<string> ExtraBlendableFaces = [];

        // Need to be able to disable/enable these buttons from another class.
        internal MenuItem createMaleBtn = new("创建男性角色", "创建一个新的男性角色.") { Label = "→→→" };
        internal MenuItem createFemaleBtn = new("创建女性角色", "创建一个新的女性角色.") { Label = "→→→" };
        internal MenuItem editPedBtn = new("编辑已保存角色", "这允许您编辑已保存角色的所有内容.更改将在您点击保存按钮后保存到该角色的保存文件条目.");

        // Need to be editable from other functions
        private readonly MenuListItem setCategoryBtn = new("设置角色类别", new List<string> { }, 0, "设置此角色的类别.选择以保存.");
        private readonly MenuListItem categoryBtn = new("角色类别", new List<string> { }, 0, "设置此角色的类别.");

        public static bool DontCloseMenus { get { return MenuController.PreventExitingMenu; } set { MenuController.PreventExitingMenu = value; } }
        public static bool DisableBackButton { get { return MenuController.DisableBackButton; } set { MenuController.DisableBackButton = value; } }
        string selectedSavedCharacterManageName = "";
        private bool isEdidtingPed = false;
        private readonly List<string> facial_expressions = new() { "mood_Normal_1", "mood_Happy_1", "mood_Angry_1", "mood_Aiming_1", "mood_Injured_1", "mood_stressed_1", "mood_smug_1", "mood_sulk_1", };

        private readonly Dictionary<int, string> blendHeadsNames = new()
        {
            { 0, "Male_" },
            { 2, "Special_Male_" },
            { 1, "Female_" },
            { 3, "Special_Female_" },
        };
        private readonly List<float> mixValues = [0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f];
        private readonly List<float> faceFeaturesValuesList =
        [
            -1.0f,    // 0
            -0.9f,    // 1
            -0.8f,    // 2
            -0.7f,    // 3
            -0.6f,    // 4
            -0.5f,    // 5
            -0.4f,    // 6
            -0.3f,    // 7
            -0.2f,    // 8
            -0.1f,    // 9
            0.0f,    // 10
            0.1f,    // 11
            0.2f,    // 12
            0.3f,    // 13
            0.4f,    // 14
            0.5f,    // 15
            0.6f,    // 16
            0.7f,    // 17
            0.8f,    // 18
            0.9f,    // 19
            1.0f     // 20
        ];
        private readonly Dictionary<int, KeyValuePair<string, string>> hairOverlays = new Dictionary<int, KeyValuePair<string, string>>()
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
        private readonly List<string> overlayColorsList = [];
        private readonly List<string> blemishesStyleList = [];
        private readonly List<string> beardStylesList = [];
        private readonly List<string> eyebrowsStyleList = [];
        private readonly List<string> ageingStyleList = [];
        private readonly List<string> makeupStyleList = [];
        private readonly List<string> blushStyleList = [];
        private readonly List<string> complexionStyleList = [];
        private readonly List<string> sunDamageStyleList = [];
        private readonly List<string> lipstickStyleList = [];
        private readonly List<string> molesFrecklesStyleList = [];
        private readonly List<string> chestHairStyleList = [];
        private readonly List<string> bodyBlemishesList = [];


        private readonly Random _random = new Random();
        private int _parentOne;
        private int _parentOneSkin;
        private int _parentTwo;
        private int _parentTwoSkin;
        private float _shapeMixValue;
        private float _skinMixValue;
        private readonly Dictionary<int, int> shapeFaceValues = [];
        // TODO: Chris: Replace with enums or something more sane - updating with index/magic numbers is nuts
        private readonly Dictionary<int, Tuple<int, int, float>> appearanceValues = [];
        private int _hairSelection;
        private int _hairColorSelection;
        private int _hairHighlightColorSelection;
        private int _eyeColorSelection;
        private int _facialExpressionSelection;

        private MultiplayerPedData currentCharacter = new();
        private MpCharacterCategory currentCategory = new();

        private Ped _clone;

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
                currentCharacter.ModelHash = male ? Game.GenerateHashASCII("mp_m_freemode_01") : Game.GenerateHashASCII("mp_f_freemode_01");
                currentCharacter.IsMale = male;

                // Places the sliders in the middle by default
                _shapeMixValue = 0.5f;
                _skinMixValue = 0.5f;

                SetPlayerClothing();
            }
            else
            {
                PedHeadBlendData headBlendData = currentCharacter.PedHeadBlendData;

                _parentOne = headBlendData.FirstFaceShape;
                _parentOneSkin = headBlendData.FirstSkinTone;
                _parentTwo = headBlendData.SecondFaceShape;
                _parentTwoSkin = headBlendData.SecondSkinTone;
                _shapeMixValue = headBlendData.ParentFaceShapePercent;
                _skinMixValue = headBlendData.ParentSkinTonePercent;

                if (_shapeMixValue > 1f)
                {
                    Log("Shape mix value was incorrectly saved with a value higher than the possible maximum. Resetting to max value");
                    _shapeMixValue = 1f;
                }

                if (_skinMixValue > 1f)
                {
                    Log("Skin mix value was incorrectly saved with a value higher than the possible maximum. Resetting to max value");
                    _skinMixValue = 1f;
                }
            }

            currentCharacter.DrawableVariations.clothes ??= new Dictionary<int, KeyValuePair<int, int>>();
            currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();

            // Set the facial expression to default in case it doesn't exist yet, or keep the current one if it does.
            currentCharacter.FacialExpression ??= facial_expressions[0];

            // Set the facial expression on the ped itself.
            SetFacialIdleAnimOverride(Game.PlayerPed.Handle, currentCharacter.FacialExpression ?? facial_expressions[0], null);

            // Set the facial expression item list to the correct saved index.
            if (createCharacterMenu.GetMenuItems().ElementAt(6) is MenuListItem li)
            {
                var index = facial_expressions.IndexOf(currentCharacter.FacialExpression ?? facial_expressions[0]);
                if (index < 0)
                {
                    index = 0;
                }
                li.ListIndex = index;
            }

            appearanceMenu.ClearMenuItems();
            tattoosMenu.ClearMenuItems();
            clothesMenu.ClearMenuItems();
            propsMenu.ClearMenuItems();

            #region appearance menu.
            if (!editPed)
            {
                // Clears any saved appearance values from prior peds
                _hairSelection = 0;
                _hairColorSelection = 0;
                _hairHighlightColorSelection = 0;
                _eyeColorSelection = 0;

                for (int i = 0; i < 12; i++)
                {
                    appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0f);
                }
            }
            else
            {
                PedAppearance appearanceData = currentCharacter.PedAppearance;

                _hairSelection = appearanceData.hairStyle;
                _hairColorSelection = appearanceData.hairColor;
                _hairHighlightColorSelection = appearanceData.hairHighlightColor;

                appearanceValues[0] = new(appearanceData.blemishesStyle, 0, appearanceData.blemishesOpacity);
                appearanceValues[1] = new(appearanceData.beardStyle, appearanceData.beardColor, appearanceData.beardOpacity);
                appearanceValues[2] = new(appearanceData.eyebrowsStyle, appearanceData.eyebrowsColor, appearanceData.eyebrowsOpacity);
                appearanceValues[3] = new(appearanceData.ageingStyle, 0, appearanceData.ageingOpacity);
                appearanceValues[4] = new(appearanceData.makeupStyle, appearanceData.makeupColor, appearanceData.makeupOpacity);
                appearanceValues[5] = new(appearanceData.blushStyle, appearanceData.blushColor, appearanceData.blushOpacity);
                appearanceValues[6] = new(appearanceData.complexionStyle, 0, appearanceData.complexionOpacity);
                appearanceValues[7] = new(appearanceData.sunDamageStyle, 0, appearanceData.sunDamageOpacity);
                appearanceValues[8] = new(appearanceData.lipstickStyle, appearanceData.lipstickColor, appearanceData.lipstickOpacity);
                appearanceValues[9] = new(appearanceData.molesFrecklesStyle, 0, appearanceData.molesFrecklesOpacity);
                appearanceValues[10] = new(appearanceData.chestHairStyle, appearanceData.chestHairColor, appearanceData.chestHairOpacity);
                appearanceValues[11] = new(appearanceData.bodyBlemishesStyle, 0, appearanceData.bodyBlemishesOpacity);

                _eyeColorSelection = appearanceData.eyeColor;
            }

            var opacity = new List<string>() { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };

            var maxHairStyles = GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2);
            //if (currentCharacter.ModelHash == (uint)PedHash.FreemodeFemale01)
            //{
            //    maxHairStyles /= 2;
            //}
            var hairStylesList = new List<string>();
            for (var i = 0; i < maxHairStyles; i++)
            {
                hairStylesList.Add($"样式 #{i + 1}");
            }
            hairStylesList.Add($"样式 #{maxHairStyles + 1}");

            var eyeColorList = new List<string>();
            for (var i = 0; i < 32; i++)
            {
                eyeColorList.Add($"瞳孔颜色 #{i + 1}");
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
            var currentHairStyle = editPed ? currentCharacter.PedAppearance.hairStyle : GetPedDrawableVariation(Game.PlayerPed.Handle, 2);
            var currentHairColor = editPed ? currentCharacter.PedAppearance.hairColor : 0;
            var currentHairHighlightColor = editPed ? currentCharacter.PedAppearance.hairHighlightColor : 0;

            // 0 blemishes
            var currentBlemishesStyle = editPed ? currentCharacter.PedAppearance.blemishesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 0) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 0) : 0;
            var currentBlemishesOpacity = editPed ? currentCharacter.PedAppearance.blemishesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, currentBlemishesStyle, currentBlemishesOpacity);

            // 1 beard
            var currentBeardStyle = editPed ? currentCharacter.PedAppearance.beardStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 1) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 1) : 0;
            var currentBeardOpacity = editPed ? currentCharacter.PedAppearance.beardOpacity : 0f;
            var currentBeardColor = editPed ? currentCharacter.PedAppearance.beardColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, currentBeardStyle, currentBeardOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, currentBeardColor, currentBeardColor);

            // 2 eyebrows
            var currentEyebrowStyle = editPed ? currentCharacter.PedAppearance.eyebrowsStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 2) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 2) : 0;
            var currentEyebrowOpacity = editPed ? currentCharacter.PedAppearance.eyebrowsOpacity : 0f;
            var currentEyebrowColor = editPed ? currentCharacter.PedAppearance.eyebrowsColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, currentEyebrowStyle, currentEyebrowOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, currentEyebrowColor, currentEyebrowColor);

            // 3 ageing
            var currentAgeingStyle = editPed ? currentCharacter.PedAppearance.ageingStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 3) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 3) : 0;
            var currentAgeingOpacity = editPed ? currentCharacter.PedAppearance.ageingOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, currentAgeingStyle, currentAgeingOpacity);

            // 4 makeup
            var currentMakeupStyle = editPed ? currentCharacter.PedAppearance.makeupStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 4) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 4) : 0;
            var currentMakeupOpacity = editPed ? currentCharacter.PedAppearance.makeupOpacity : 0f;
            var currentMakeupColor = editPed ? currentCharacter.PedAppearance.makeupColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, currentMakeupStyle, currentMakeupOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, currentMakeupColor, currentMakeupColor);

            // 5 blush
            var currentBlushStyle = editPed ? currentCharacter.PedAppearance.blushStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 5) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 5) : 0;
            var currentBlushOpacity = editPed ? currentCharacter.PedAppearance.blushOpacity : 0f;
            var currentBlushColor = editPed ? currentCharacter.PedAppearance.blushColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, currentBlushStyle, currentBlushOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, currentBlushColor, currentBlushColor);

            // 6 complexion
            var currentComplexionStyle = editPed ? currentCharacter.PedAppearance.complexionStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 6) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 6) : 0;
            var currentComplexionOpacity = editPed ? currentCharacter.PedAppearance.complexionOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, currentComplexionStyle, currentComplexionOpacity);

            // 7 sun damage
            var currentSunDamageStyle = editPed ? currentCharacter.PedAppearance.sunDamageStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 7) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 7) : 0;
            var currentSunDamageOpacity = editPed ? currentCharacter.PedAppearance.sunDamageOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, currentSunDamageStyle, currentSunDamageOpacity);

            // 8 lipstick
            var currentLipstickStyle = editPed ? currentCharacter.PedAppearance.lipstickStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 8) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 8) : 0;
            var currentLipstickOpacity = editPed ? currentCharacter.PedAppearance.lipstickOpacity : 0f;
            var currentLipstickColor = editPed ? currentCharacter.PedAppearance.lipstickColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, currentLipstickStyle, currentLipstickOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, currentLipstickColor, currentLipstickColor);

            // 9 moles/freckles
            var currentMolesFrecklesStyle = editPed ? currentCharacter.PedAppearance.molesFrecklesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 9) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 9) : 0;
            var currentMolesFrecklesOpacity = editPed ? currentCharacter.PedAppearance.molesFrecklesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, currentMolesFrecklesStyle, currentMolesFrecklesOpacity);

            // 10 chest hair
            var currentChesthairStyle = editPed ? currentCharacter.PedAppearance.chestHairStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 10) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 10) : 0;
            var currentChesthairOpacity = editPed ? currentCharacter.PedAppearance.chestHairOpacity : 0f;
            var currentChesthairColor = editPed ? currentCharacter.PedAppearance.chestHairColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, currentChesthairStyle, currentChesthairOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, currentChesthairColor, currentChesthairColor);

            // 11 body blemishes
            var currentBodyBlemishesStyle = editPed ? currentCharacter.PedAppearance.bodyBlemishesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 11) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 11) : 0;
            var currentBodyBlemishesOpacity = editPed ? currentCharacter.PedAppearance.bodyBlemishesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 11, currentBodyBlemishesStyle, currentBodyBlemishesOpacity);

            var currentEyeColor = editPed ? currentCharacter.PedAppearance.eyeColor : 0;
            SetPedEyeColor(Game.PlayerPed.Handle, currentEyeColor);

            var hairStyles = new MenuListItem("头发发型", hairStylesList, currentHairStyle, "选择一个发型.");
            //MenuListItem hairColors = new MenuListItem("头发颜色", overlayColorsList, currentHairColor, "选择一个头发颜色.");
            var hairColors = new MenuListItem("头发颜色", overlayColorsList, currentHairColor, "选择一个头发颜色.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };
            //MenuListItem hairHighlightColors = new MenuListItem("头发高光颜色", overlayColorsList, currentHairHighlightColor, "选择一个头发高光颜色.");
            var hairHighlightColors = new MenuListItem("头发高光颜色", overlayColorsList, currentHairHighlightColor, "选择一个头发高光颜色.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };

            var blemishesStyle = new MenuListItem("瑕疵样式", blemishesStyleList, currentBlemishesStyle, "选择一个瑕疵样式.");
            //MenuSliderItem blemishesOpacity = new MenuSliderItem("瑕疵可见性", "选择一个瑕疵可见性.", 0, 10, (int)(currentBlemishesOpacity * 10f), false);
            var blemishesOpacity = new MenuListItem("瑕疵可见性", opacity, (int)(currentBlemishesOpacity * 10f), "选择一个瑕疵可见性.") { ShowOpacityPanel = true };

            var beardStyles = new MenuListItem("胡须样式", beardStylesList, currentBeardStyle, "选择一个胡须/面部毛发样式.");
            var beardOpacity = new MenuListItem("胡须可见性", opacity, (int)(currentBeardOpacity * 10f), "选择胡须/面部毛发的可见性.") { ShowOpacityPanel = true };
            var beardColor = new MenuListItem("胡须颜色", overlayColorsList, currentBeardColor, "选择一个胡须颜色.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };
            //MenuSliderItem beardOpacity = new MenuSliderItem("胡须可见性", "选择胡须/面部毛发的可见性.", 0, 10, (int)(currentBeardOpacity * 10f), false);
            //MenuListItem beardColor = new MenuListItem("胡须颜色", overlayColorsList, currentBeardColor, "选择胡须颜色。");

            var eyebrowStyle = new MenuListItem("眉毛样式", eyebrowsStyleList, currentEyebrowStyle, "选择眉毛样式。");
            var eyebrowOpacity = new MenuListItem("眉毛透明度", opacity, (int)(currentEyebrowOpacity * 10f), "选择眉毛透明度。") { ShowOpacityPanel = true };
            var eyebrowColor = new MenuListItem("眉毛颜色", overlayColorsList, currentEyebrowColor, "选择眉毛颜色。") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };
            //MenuSliderItem eyebrowOpacity = new MenuSliderItem("眉毛透明度", "选择眉毛透明度。", 0, 10, (int)(currentEyebrowOpacity * 10f), false);

            var ageingStyle = new MenuListItem("衰老效果样式", ageingStyleList, currentAgeingStyle, "选择衰老效果样式。");
            var ageingOpacity = new MenuListItem("衰老效果透明度", opacity, (int)(currentAgeingOpacity * 10f), "选择衰老效果透明度。") { ShowOpacityPanel = true };
            //MenuSliderItem ageingOpacity = new MenuSliderItem("衰老效果透明度", "选择衰老效果透明度。", 0, 10, (int)(currentAgeingOpacity * 10f), false);

            var makeupStyle = new MenuListItem("妆容样式", makeupStyleList, currentMakeupStyle, "选择妆容样式。");
            var makeupOpacity = new MenuListItem("妆容透明度", opacity, (int)(currentMakeupOpacity * 10f), "选择妆容透明度") { ShowOpacityPanel = true };
            //MenuSliderItem makeupOpacity = new MenuSliderItem("妆容透明度", 0, 10, (int)(currentMakeupOpacity * 10f), "选择妆容透明度.");
            var makeupColor = new MenuListItem("妆容颜色", overlayColorsList, currentMakeupColor, "选择妆容颜色。") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Makeup };

            var blushStyle = new MenuListItem("腮红样式", blushStyleList, currentBlushStyle, "选择腮红样式。");
            var blushOpacity = new MenuListItem("腮红透明度", opacity, (int)(currentBlushOpacity * 10f), "选择腮红透明度。") { ShowOpacityPanel = true };
            //MenuSliderItem blushOpacity = new MenuSliderItem("腮红透明度", 0, 10, (int)(currentBlushOpacity * 10f), "选择腮红透明度。");
            var blushColor = new MenuListItem("腮红颜色", overlayColorsList, currentBlushColor, "选择腮红颜色。") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Makeup };

            var complexionStyle = new MenuListItem("肤色纹理样式", complexionStyleList, currentComplexionStyle, "选择肤色纹理样式。");
            //MenuSliderItem complexionOpacity = new MenuSliderItem("肤色纹理透明度", 0, 10, (int)(currentComplexionOpacity * 10f), "选择肤色纹理透明度。");
            var complexionOpacity = new MenuListItem("肤色纹理透明度", opacity, (int)(currentComplexionOpacity * 10f), "选择肤色纹理透明度。") { ShowOpacityPanel = true };

            var sunDamageStyle = new MenuListItem("晒伤效果样式", sunDamageStyleList, currentSunDamageStyle, "选择晒伤效果样式。");
            //MenuSliderItem sunDamageOpacity = new MenuSliderItem("晒伤效果透明度", 0, 10, (int)(currentSunDamageOpacity * 10f), "选择晒伤效果透明度。");
            var sunDamageOpacity = new MenuListItem("晒伤效果透明度", opacity, (int)(currentSunDamageOpacity * 10f), "选择晒伤效果透明度。") { ShowOpacityPanel = true };

            var lipstickStyle = new MenuListItem("口红样式", lipstickStyleList, currentLipstickStyle, "选择口红样式。");
            //MenuSliderItem lipstickOpacity = new MenuSliderItem("口红透明度", 0, 10, (int)(currentLipstickOpacity * 10f), "选择口红透明度。");
            var lipstickOpacity = new MenuListItem("口红透明度", opacity, (int)(currentLipstickOpacity * 10f), "选择口红透明度。") { ShowOpacityPanel = true };
            var lipstickColor = new MenuListItem("口红颜色", overlayColorsList, currentLipstickColor, "选择口红颜色。") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Makeup };

            var molesFrecklesStyle = new MenuListItem("痣与雀斑样式", molesFrecklesStyleList, currentMolesFrecklesStyle, "选择痣与雀斑样式。");
            //MenuSliderItem molesFrecklesOpacity = new MenuSliderItem("痣与雀斑透明度", 0, 10, (int)(currentMolesFrecklesOpacity * 10f), "选择痣与雀斑透明度。");
            var molesFrecklesOpacity = new MenuListItem("痣与雀斑透明度", opacity, (int)(currentMolesFrecklesOpacity * 10f), "选择痣与雀斑透明度。") { ShowOpacityPanel = true };

            var chestHairStyle = new MenuListItem("胸毛样式", chestHairStyleList, currentChesthairStyle, "选择胸毛样式。");
            //MenuSliderItem chestHairOpacity = new MenuSliderItem("胸毛透明度", 0, 10, (int)(currentChesthairOpacity * 10f), "选择胸毛透明度。");
            var chestHairOpacity = new MenuListItem("胸毛透明度", opacity, (int)(currentChesthairOpacity * 10f), "选择胸毛透明度。") { ShowOpacityPanel = true };
            var chestHairColor = new MenuListItem("胸毛颜色", overlayColorsList, currentChesthairColor, "选择胸毛颜色。") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };

            // Body blemishes
            var bodyBlemishesStyle = new MenuListItem("身体瑕疵样式", bodyBlemishesList, currentBodyBlemishesStyle, "选择身体瑕疵样式。");
            var bodyBlemishesOpacity = new MenuListItem("身体瑕疵透明度", opacity, (int)(currentBodyBlemishesOpacity * 10f), "选择身体瑕疵透明度。") { ShowOpacityPanel = true };

            var eyeColor = new MenuListItem("眼睛颜色", eyeColorList, currentEyeColor, "选择眼睛或隐形眼镜颜色。");

            appearanceMenu.AddMenuItem(hairStyles);
            appearanceMenu.AddMenuItem(hairColors);
            appearanceMenu.AddMenuItem(hairHighlightColors);

            appearanceMenu.AddMenuItem(blemishesStyle);
            appearanceMenu.AddMenuItem(blemishesOpacity);

            appearanceMenu.AddMenuItem(beardStyles);
            appearanceMenu.AddMenuItem(beardOpacity);
            appearanceMenu.AddMenuItem(beardColor);

            appearanceMenu.AddMenuItem(eyebrowStyle);
            appearanceMenu.AddMenuItem(eyebrowOpacity);
            appearanceMenu.AddMenuItem(eyebrowColor);

            appearanceMenu.AddMenuItem(ageingStyle);
            appearanceMenu.AddMenuItem(ageingOpacity);

            appearanceMenu.AddMenuItem(makeupStyle);
            appearanceMenu.AddMenuItem(makeupOpacity);
            appearanceMenu.AddMenuItem(makeupColor);

            appearanceMenu.AddMenuItem(blushStyle);
            appearanceMenu.AddMenuItem(blushOpacity);
            appearanceMenu.AddMenuItem(blushColor);

            appearanceMenu.AddMenuItem(complexionStyle);
            appearanceMenu.AddMenuItem(complexionOpacity);

            appearanceMenu.AddMenuItem(sunDamageStyle);
            appearanceMenu.AddMenuItem(sunDamageOpacity);

            appearanceMenu.AddMenuItem(lipstickStyle);
            appearanceMenu.AddMenuItem(lipstickOpacity);
            appearanceMenu.AddMenuItem(lipstickColor);

            appearanceMenu.AddMenuItem(molesFrecklesStyle);
            appearanceMenu.AddMenuItem(molesFrecklesOpacity);

            appearanceMenu.AddMenuItem(chestHairStyle);
            appearanceMenu.AddMenuItem(chestHairOpacity);
            appearanceMenu.AddMenuItem(chestHairColor);

            appearanceMenu.AddMenuItem(bodyBlemishesStyle);
            appearanceMenu.AddMenuItem(bodyBlemishesOpacity);

            appearanceMenu.AddMenuItem(eyeColor);

            if (male)
            {
                // There are weird people out there that wanted makeup for male characters
                // so yeah.... here you go I suppose... strange...

                /*
                makeupStyle.Enabled = false;
                makeupStyle.LeftIcon = MenuItem.Icon.LOCK;
                makeupStyle.Description = "男性角色无法使用此项。";

                makeupOpacity.Enabled = false;
                makeupOpacity.LeftIcon = MenuItem.Icon.LOCK;
                makeupOpacity.Description = "男性角色无法使用此项。";

                makeupColor.Enabled = false;
                makeupColor.LeftIcon = MenuItem.Icon.LOCK;
                makeupColor.Description = "男性角色无法使用此项。";


                blushStyle.Enabled = false;
                blushStyle.LeftIcon = MenuItem.Icon.LOCK;
                blushStyle.Description = "男性角色无法使用此项。";

                blushOpacity.Enabled = false;
                blushOpacity.LeftIcon = MenuItem.Icon.LOCK;
                blushOpacity.Description = "男性角色无法使用此项。";

                blushColor.Enabled = false;
                blushColor.LeftIcon = MenuItem.Icon.LOCK;
                blushColor.Description = "男性角色无法使用此项。";


                lipstickStyle.Enabled = false;
                lipstickStyle.LeftIcon = MenuItem.Icon.LOCK;
                lipstickStyle.Description = "男性角色无法使用此项。";

                lipstickOpacity.Enabled = false;
                lipstickOpacity.LeftIcon = MenuItem.Icon.LOCK;
                lipstickOpacity.Description = "男性角色无法使用此项。";

                lipstickColor.Enabled = false;
                lipstickColor.LeftIcon = MenuItem.Icon.LOCK;
                lipstickColor.Description = "男性角色无法使用此项。";
                */
            }
            else
            {
                beardStyles.Enabled = false;
                beardStyles.LeftIcon = MenuItem.Icon.LOCK;
                beardStyles.Description = "此选项不适用于女性角色.";

                beardOpacity.Enabled = false;
                beardOpacity.LeftIcon = MenuItem.Icon.LOCK;
                beardOpacity.Description = "此选项不适用于女性角色.";

                beardColor.Enabled = false;
                beardColor.LeftIcon = MenuItem.Icon.LOCK;
                beardColor.Description = "此选项不适用于女性角色.";


                chestHairStyle.Enabled = false;
                chestHairStyle.LeftIcon = MenuItem.Icon.LOCK;
                chestHairStyle.Description = "此选项不适用于女性角色.";

                chestHairOpacity.Enabled = false;
                chestHairOpacity.LeftIcon = MenuItem.Icon.LOCK;
                chestHairOpacity.Description = "此选项不适用于女性角色.";

                chestHairColor.Enabled = false;
                chestHairColor.LeftIcon = MenuItem.Icon.LOCK;
                chestHairColor.Description = "此选项不适用于女性角色.";
            }

            #endregion

            #region clothing options menu
            var clothingCategoryNames = new string[12] { "未使用（头部）", "面具", "未使用（头发）", "上衣", "下身", "背包与降落伞", "鞋子", "围巾与链条", "衬衫与配件", "护甲与配件 2", "徽章与标志", "衬衫与夹克" };
            for (var i = 0; i < 12; i++)
            {
                if (i is not 0 and not 2)
                {
                    var currentVariationIndex = editPed && currentCharacter.DrawableVariations.clothes.ContainsKey(i) ? currentCharacter.DrawableVariations.clothes[i].Key : GetPedDrawableVariation(Game.PlayerPed.Handle, i);
                    var currentVariationTextureIndex = editPed && currentCharacter.DrawableVariations.clothes.ContainsKey(i) ? currentCharacter.DrawableVariations.clothes[i].Value : GetPedTextureVariation(Game.PlayerPed.Handle, i);

                    var maxDrawables = GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, i);

                    var items = new List<string>();
                    for (var x = 0; x < maxDrawables; x++)
                    {
                        items.Add($"可使用 # ({x}/ {maxDrawables})");
                    }

                    var maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, i, currentVariationIndex);

                    var listItem = new MenuListItem(clothingCategoryNames[i], items, currentVariationIndex, $"使用箭头键选择一个可绘制对象,使用 ~o~ENTER~s~ 以循环查看所有可用纹理. 当前选中的纹理: #{currentVariationTextureIndex + 1} (/ {maxTextures}).");
                    clothesMenu.AddMenuItem(listItem);
                }
            }
            #endregion

            #region props options menu
            var propNames = new string[5] { "帽子与头盔", "眼镜", "杂项配件", "手表", "手链" };
            for (var x = 0; x < 5; x++)
            {
                var propId = x;
                if (x > 2)
                {
                    propId += 3;
                }

                var currentProp = editPed && currentCharacter.PropVariations.props.ContainsKey(propId) ? currentCharacter.PropVariations.props[propId].Key : GetPedPropIndex(Game.PlayerPed.Handle, propId);
                var currentPropTexture = editPed && currentCharacter.PropVariations.props.ContainsKey(propId) ? currentCharacter.PropVariations.props[propId].Value : GetPedPropTextureIndex(Game.PlayerPed.Handle, propId);

                var propsList = new List<string>();
                for (var i = 0; i < GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propId); i++)
                {
                    propsList.Add($"可使用 # ({i} /{GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propId)})");
                }
                propsList.Add("暂无");


                if (GetPedPropIndex(Game.PlayerPed.Handle, propId) != -1)
                {
                    var maxPropTextures = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propId, currentProp);
                    var propListItem = new MenuListItem($"{propNames[x]}", propsList, currentProp, $"使用箭头键选择一个可绘制对象,使用 ~o~ENTER~s~ 以循环查看所有可用纹理. 当前选中的纹理: #{currentPropTexture + 1} (/ {maxPropTextures}).");
                    propsMenu.AddMenuItem(propListItem);
                }
                else
                {
                    var propListItem = new MenuListItem($"{propNames[x]}", propsList, currentProp, "使用箭头键选择一个可绘制对象,使用 ~o~ENTER~s~ 以循环查看所有可用纹理.");
                    propsMenu.AddMenuItem(propListItem);
                }


            }
            #endregion

            #region face features menu
            foreach (MenuSliderItem item in faceShapeMenu.GetMenuItems())
            {
                if (editPed)
                {
                    if (currentCharacter.FaceShapeFeatures.features == null)
                    {
                        currentCharacter.FaceShapeFeatures.features = new Dictionary<int, float>();
                    }
                    else
                    {
                        if (currentCharacter.FaceShapeFeatures.features.ContainsKey(item.Index))
                        {
                            item.Position = (int)(currentCharacter.FaceShapeFeatures.features[item.Index] * 10f) + 10;
                            SetPedFaceFeature(Game.PlayerPed.Handle, item.Index, currentCharacter.FaceShapeFeatures.features[item.Index]);
                        }
                        else
                        {
                            item.Position = 10;
                            SetPedFaceFeature(Game.PlayerPed.Handle, item.Index, 0f);
                        }
                    }
                }
                else
                {
                    item.Position = 10;
                    SetPedFaceFeature(Game.PlayerPed.Handle, item.Index, 0f);
                }
            }
            #endregion

            #region Tattoos menu
            var hairTattoosList = new List<string>();
            var headTattoosList = new List<string>();
            var torsoTattoosList = new List<string>();
            var leftArmTattoosList = new List<string>();
            var rightArmTattoosList = new List<string>();
            var leftLegTattoosList = new List<string>();
            var rightLegTattoosList = new List<string>();
            var badgeTattoosList = new List<string>();
            var addonTattoosList = new List<string>();

            TattoosData.GenerateTattoosData();
            if (male)
            {
                var counter = 1;
                foreach (var tattoo in MaleTattoosCollection.HAIR)
                {
                    hairTattoosList.Add($"Tattoo #{counter} (of {MaleTattoosCollection.HAIR.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.HEAD)
                {
                    headTattoosList.Add($"可用 #{counter} (/ {MaleTattoosCollection.HEAD.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.TORSO)
                {
                    torsoTattoosList.Add($"可用 #{counter} (/ {MaleTattoosCollection.TORSO.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.LEFT_ARM)
                {
                    leftArmTattoosList.Add($"可用 #{counter} (/ {MaleTattoosCollection.LEFT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.RIGHT_ARM)
                {
                    rightArmTattoosList.Add($"可用 #{counter} (/ {MaleTattoosCollection.RIGHT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.LEFT_LEG)
                {
                    leftLegTattoosList.Add($"可用 #{counter} (/ {MaleTattoosCollection.LEFT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.RIGHT_LEG)
                {
                    rightLegTattoosList.Add($"可用 #{counter} (/ {MaleTattoosCollection.RIGHT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.BADGES)
                {
                    badgeTattoosList.Add($"徽章 #{counter} (/ {MaleTattoosCollection.BADGES.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.ADDONS)
                {
                    addonTattoosList.Add($"Addon Tattoo #{counter} (of {MaleTattoosCollection.ADDONS.Count})");
                    counter++;
                }
            }
            else
            {
                var counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.HAIR)
                {
                    hairTattoosList.Add($"Tattoo #{counter} (of {FemaleTattoosCollection.HAIR.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.HEAD)
                {
                    headTattoosList.Add($"可用 #{counter} (/ {FemaleTattoosCollection.HEAD.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.TORSO)
                {
                    torsoTattoosList.Add($"可用 #{counter} (/ {FemaleTattoosCollection.TORSO.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.LEFT_ARM)
                {
                    leftArmTattoosList.Add($"可用 #{counter} (/ {FemaleTattoosCollection.LEFT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.RIGHT_ARM)
                {
                    rightArmTattoosList.Add($"可用 #{counter} (/ {FemaleTattoosCollection.RIGHT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.LEFT_LEG)
                {
                    leftLegTattoosList.Add($"可用 #{counter} (/ {FemaleTattoosCollection.LEFT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.RIGHT_LEG)
                {
                    rightLegTattoosList.Add($"可用 #{counter} (/ {FemaleTattoosCollection.RIGHT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.BADGES)
                {
                    badgeTattoosList.Add($"徽章 #{counter} (/ {FemaleTattoosCollection.BADGES.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.ADDONS)
                {
                    addonTattoosList.Add($"Addon Tattoo #{counter} (of {FemaleTattoosCollection.ADDONS.Count})");
                    counter++;
                }
            }

            const string tatDesc = "切换以预览纹身，按 ENTER 启用或禁用特定纹身。";
            var hairTatts = new MenuListItem("头发纹身", hairTattoosList, 0, tatDesc);
            var headTatts = new MenuListItem("头部纹身", headTattoosList, 0, tatDesc);
            var torsoTatts = new MenuListItem("躯干纹身", torsoTattoosList, 0, tatDesc);
            var leftArmTatts = new MenuListItem("左臂纹身", leftArmTattoosList, 0, tatDesc);
            var rightArmTatts = new MenuListItem("右臂纹身", rightArmTattoosList, 0, tatDesc);
            var leftLegTatts = new MenuListItem("左腿纹身", leftLegTattoosList, 0, tatDesc);
            var rightLegTatts = new MenuListItem("右腿纹身", rightLegTattoosList, 0, tatDesc);
            var badgeTatts = new MenuListItem("徽章贴纸", badgeTattoosList, 0, "切换以预览徽章，按 ENTER 启用或禁用特定徽章。注意：徽章需要搭配上衣才会显示！");
            var addonTatts = new MenuListItem("添加型纹身", addonTattoosList, 0, tatDesc);

            tattoosMenu.AddMenuItem(hairTatts);
            tattoosMenu.AddMenuItem(headTatts);
            tattoosMenu.AddMenuItem(torsoTatts);
            tattoosMenu.AddMenuItem(leftArmTatts);
            tattoosMenu.AddMenuItem(rightArmTatts);
            tattoosMenu.AddMenuItem(leftLegTatts);
            tattoosMenu.AddMenuItem(rightLegTatts);
            tattoosMenu.AddMenuItem(badgeTatts);
            if (addonTattoosList.Count > 0)
            {
                tattoosMenu.AddMenuItem(addonTatts);
            }
            tattoosMenu.AddMenuItem(new MenuItem("移除全部纹身与徽章", "点击以移除全部纹身并重新开始。"));
            #endregion

            List<string> categoryNames = GetAllCategoryNames();

            categoryNames.RemoveAt(0);

            List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);

            categoryBtn.ItemData = new Tuple<List<string>, List<MenuItem.Icon>>(categoryNames, categoryIcons);
            categoryBtn.ListItems = categoryNames;            

            if (editPed)
            {
                int characterCategoryIndex = categoryNames.IndexOf(currentCharacter.Category);

                categoryBtn.ListIndex = characterCategoryIndex;
            }
            else
            {
                categoryBtn.ListIndex = 0;
            }

            categoryBtn.RightIcon = categoryIcons[categoryBtn.ListIndex];

            createCharacterMenu.RefreshIndex();
            appearanceMenu.RefreshIndex();
            inheritanceMenu.RefreshIndex();
            tattoosMenu.RefreshIndex();
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
                var json = JsonConvert.SerializeObject(currentCharacter);
                if (StorageManager.SaveJsonData(currentCharacter.SaveName, json, true))
                {
                    Notify.Success("已成功保存您的角色数据.");
                    return true;
                }
                else
                {
                    Notify.Error("您的角色无法保存. 原因不明. :(");
                    return false;
                }
            }
            else
            {
                var name = await GetUserInput(windowTitle: "输入有效保存昵称.", maxInputLength: 30);
                if (string.IsNullOrEmpty(name))
                {
                    Notify.Error(CommonErrors.InvalidInput);
                    return false;
                }
                else
                {
                    currentCharacter.SaveName = "mp_ped_" + name;
                    var json = JsonConvert.SerializeObject(currentCharacter);

                    if (StorageManager.SaveJsonData("mp_ped_" + name, json, false))
                    {
                        Notify.Success($"角色 (~g~{name}~s~) 已完成保存.");
                        Log($"Saved Character {name}. Data: {json}");
                        return true;
                    }
                    else
                    {
                        Notify.Error($"保存失败, 很可能是因为该 (~y~{name}~s~) 名称已被使用.");
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
            for (int i = 0; i < GetNumHairColors(); i++)
            {
                overlayColorsList.Add($"颜色 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(0); i++)
            {
                blemishesStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(1); i++)
            {
                beardStylesList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(2); i++)
            {
                eyebrowsStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(3); i++)
            {
                ageingStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(4); i++)
            {
                makeupStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(5); i++)
            {
                blushStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(6); i++)
            {
                complexionStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(7); i++)
            {
                sunDamageStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(8); i++)
            {
                lipstickStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(9); i++)
            {
                molesFrecklesStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(10); i++)
            {
                chestHairStyleList.Add($"样式 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(11); i++)
            {
                bodyBlemishesList.Add($"样式 #{i + 1}");
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, "MP人物模型自定义");

            var savedCharacters = new MenuItem("已保存人物", "生成、编辑或删除你已保存的多人模式人物.")
            {
                Label = "→→→"
            };

            MenuController.AddMenu(createCharacterMenu);
            MenuController.AddMenu(savedCharactersMenu);
            MenuController.AddMenu(savedCharactersCategoryMenu);
            MenuController.AddMenu(inheritanceMenu);
            MenuController.AddMenu(appearanceMenu);
            MenuController.AddMenu(faceShapeMenu);
            MenuController.AddMenu(tattoosMenu);
            MenuController.AddMenu(clothesMenu);
            MenuController.AddMenu(propsMenu);

            CreateSavedPedsMenu();

            menu.AddMenuItem(createMaleBtn);
            MenuController.BindMenuItem(menu, createCharacterMenu, createMaleBtn);
            menu.AddMenuItem(createFemaleBtn);
            MenuController.BindMenuItem(menu, createCharacterMenu, createFemaleBtn);
            menu.AddMenuItem(savedCharacters);
            MenuController.BindMenuItem(menu, savedCharactersMenu, savedCharacters);

            menu.RefreshIndex();

            createCharacterMenu.InstructionalButtons.Add(Control.MoveLeftRight, "转动头部");
            inheritanceMenu.InstructionalButtons.Add(Control.MoveLeftRight, "转动头部");
            appearanceMenu.InstructionalButtons.Add(Control.MoveLeftRight, "转动头部");
            faceShapeMenu.InstructionalButtons.Add(Control.MoveLeftRight, "转动头部");
            tattoosMenu.InstructionalButtons.Add(Control.MoveLeftRight, "转动头部");
            clothesMenu.InstructionalButtons.Add(Control.MoveLeftRight, "转动头部");
            propsMenu.InstructionalButtons.Add(Control.MoveLeftRight, "转动头部");

            createCharacterMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "整体转身");
            inheritanceMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "整体转身");
            appearanceMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "整体转身");
            faceShapeMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "整体转身");
            tattoosMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "整体转身");
            clothesMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "整体转身");
            propsMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "整体转身");

            createCharacterMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "镜头向右转动");
            inheritanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "镜头向右转动");
            appearanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "镜头向右转动");
            faceShapeMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "镜头向右转动");
            tattoosMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "镜头向右转动");
            clothesMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "镜头向右转动");
            propsMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "镜头向右转动");

            createCharacterMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "镜头向左转动");
            inheritanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "镜头向左转动");
            appearanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "镜头向左转动");
            faceShapeMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "镜头向左转动");
            tattoosMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "镜头向左转动");
            clothesMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "镜头向左转动");
            propsMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "镜头向左转动");


            var randomizeButton = new MenuItem("随机角色外观", "随机生成人物外观。");
            var inheritanceButton = new MenuItem("角色继承", "角色继承选项.");
            var appearanceButton = new MenuItem("角色外观", "角色外观选项.");
            var faceButton = new MenuItem("角色脸型选项", "角色脸型选项.");
            var tattoosButton = new MenuItem("角色纹身选项", "角色纹身选项.");
            var clothesButton = new MenuItem("角色服装", "角色服装选项.");
            var propsButton = new MenuItem("角色配饰", "角色配饰选项.");
            var saveButton = new MenuItem("保存角色", "保存你的角色.");
            var exitNoSave = new MenuItem("不保存退出", "你确定吗？所有未保存的工作将丢失.");
            var faceExpressionList = new MenuListItem("面部表情", new List<string> { "正常", "开心", "愤怒", "瞄准", "受伤", "紧张", "自满", "生气" }, 0, "设置一个面部表情,当你的角色空闲时将使用该表情.");

            inheritanceButton.Label = "→→→";
            appearanceButton.Label = "→→→";
            faceButton.Label = "→→→";
            tattoosButton.Label = "→→→";
            clothesButton.Label = "→→→";
            propsButton.Label = "→→→";

            createCharacterMenu.AddMenuItem(randomizeButton);
            createCharacterMenu.AddMenuItem(inheritanceButton);
            createCharacterMenu.AddMenuItem(appearanceButton);
            createCharacterMenu.AddMenuItem(faceButton);
            createCharacterMenu.AddMenuItem(tattoosButton);
            createCharacterMenu.AddMenuItem(clothesButton);
            createCharacterMenu.AddMenuItem(propsButton);
            createCharacterMenu.AddMenuItem(faceExpressionList);
            createCharacterMenu.AddMenuItem(categoryBtn);
            createCharacterMenu.AddMenuItem(saveButton);
            createCharacterMenu.AddMenuItem(exitNoSave);

            MenuController.BindMenuItem(createCharacterMenu, inheritanceMenu, inheritanceButton);
            MenuController.BindMenuItem(createCharacterMenu, appearanceMenu, appearanceButton);
            MenuController.BindMenuItem(createCharacterMenu, faceShapeMenu, faceButton);
            MenuController.BindMenuItem(createCharacterMenu, tattoosMenu, tattoosButton);
            MenuController.BindMenuItem(createCharacterMenu, clothesMenu, clothesButton);
            MenuController.BindMenuItem(createCharacterMenu, propsMenu, propsButton);

            #region inheritance
            // Not the same as parentNames, because parentNames includes Addon faces
            List<string> skinNames = [];
            List<string> parentNames = [];

            foreach (KeyValuePair<int, string> kvp in blendHeadsNames)
            {
                int listId = kvp.Key;
                string listName = kvp.Value;
                bool isMale = listName.Contains("Male");

                for (int i = 0; i < GetNumParentPedsOfType(listId); i++)
                {
                    string label = GetLabelText($"{listName}{i}");

                    if (string.IsNullOrWhiteSpace(label) || label == "NULL")
                    {
                        label = i.ToString();
                    }

                    label += $" ({(isMale ? "Male" : "Female")})";

                    skinNames.Add(label);
                    parentNames.Add(label);
                }
            }

            if (ExtraBlendableFaces.Count > 0)
            {
                parentNames.AddRange(ExtraBlendableFaces);
            }

            var inheritanceParentOne = new MenuListItem("父母 #1", parentNames, 0, "选择第一位父母。");
            var inheritanceParentOneSkin = new MenuListItem("父母 #1 肤色", skinNames, 0, "选择第一位父母的皮肤纹理。");
            var inheritanceParentTwo = new MenuListItem("父母 #2", parentNames, 0, "选择第二位父母。");
            var inheritanceParentTwoSkin = new MenuListItem("父母 #2 肤色", skinNames, 0, "选择第二位父母的皮肤纹理。");
            var inheritanceShapeMix = new MenuSliderItem("Head Shape Mix", "Select how much of your head shape should be inherited from each parent. All the way on the left is 父母 #1, all the way on the right is 父母 #2.", 0, 10, 5, true) { ItemData = "shape_mix" };
            var inheritanceSkinMix = new MenuSliderItem("Body Skin Mix", "Select how much of your body skin tone should be inherited from each parent. All the way on the left is 父母 #1, all the way on the right is 父母 #2.", 0, 10, 5, true) { ItemData = "skin_mix" };

            inheritanceMenu.AddMenuItem(inheritanceParentOne);
            inheritanceMenu.AddMenuItem(inheritanceParentOneSkin);
            inheritanceMenu.AddMenuItem(inheritanceParentTwo);
            inheritanceMenu.AddMenuItem(inheritanceParentTwoSkin);
            inheritanceMenu.AddMenuItem(inheritanceShapeMix);
            inheritanceMenu.AddMenuItem(inheritanceSkinMix);

            inheritanceMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, itemIndex) =>
            {
                _parentOne = inheritanceParentOne.ListIndex;
                _parentOneSkin = inheritanceParentOneSkin.ListIndex;
                _parentTwo = inheritanceParentTwo.ListIndex;
                _parentTwoSkin = inheritanceParentTwoSkin.ListIndex;

                SetHeadBlend();
            };

            inheritanceMenu.OnSliderPositionChange += (sender, item, oldPosition, newPosition, itemIndex) =>
            {
                if (item.ItemData is not string sliderType)
                {
                    return;
                }

                // Chris: We can't call `.Position` on the slider items here because it returns the value *prior* to the change
                switch (sliderType)
                {
                    case "shape_mix":
                        _shapeMixValue = newPosition / 10f;
                        break;

                    case "skin_mix":
                        _skinMixValue = newPosition / 10f;
                        break;

                    default:
                        break;
                }

                SetHeadBlend();
            };
            #endregion

            #region appearance
            // manage the list changes for appearance items.
            appearanceMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, itemIndex) =>
            {
                if (itemIndex == 0) // hair style
                {
                    ChangePlayerHair(newSelectionIndex);
                }
                else if (itemIndex is 1 or 2) // hair colors
                {
                    var tmp = (MenuListItem)_menu.GetMenuItems()[1];
                    var hairColor = tmp.ListIndex;
                    tmp = (MenuListItem)_menu.GetMenuItems()[2];
                    var hairHighlightColor = tmp.ListIndex;

                    ChangePlayerHairColor(hairColor, hairHighlightColor);

                    currentCharacter.PedAppearance.hairColor = hairColor;
                    currentCharacter.PedAppearance.hairHighlightColor = hairHighlightColor;
                }
                else if (itemIndex == 33) // eye color
                {
                    var selection = ((MenuListItem)_menu.GetMenuItems()[itemIndex]).ListIndex;
                    ChangePlayerEyeColor(selection);
                    currentCharacter.PedAppearance.eyeColor = selection;
                }
                else
                {
                    var selection = ((MenuListItem)_menu.GetMenuItems()[itemIndex]).ListIndex;
                    var opacity = 0f;
                    if (_menu.GetMenuItems()[itemIndex + 1] is MenuListItem item2)
                    {
                        opacity = (((float)item2.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex - 1] is MenuListItem item1)
                    {
                        opacity = (((float)item1.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex] is MenuListItem item)
                    {
                        opacity = (((float)item.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else
                    {
                        opacity = 1f;
                    }

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
                        case 31: // body blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 11, selection, opacity);
                            currentCharacter.PedAppearance.bodyBlemishesStyle = selection;
                            currentCharacter.PedAppearance.bodyBlemishesOpacity = opacity;
                            break;
                    }
                }
            };

            // manage the slider changes for opacity on the appearance items.
            appearanceMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, itemIndex) =>
            {
                if (itemIndex is > 2 and < 33)
                {

                    var selection = ((MenuListItem)_menu.GetMenuItems()[itemIndex - 1]).ListIndex;
                    var opacity = 0f;
                    if (_menu.GetMenuItems()[itemIndex] is MenuListItem item2)
                    {
                        opacity = (((float)item2.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex + 1] is MenuListItem item1)
                    {
                        opacity = (((float)item1.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex - 1] is MenuListItem item)
                    {
                        opacity = (((float)item.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else
                    {
                        opacity = 1f;
                    }

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
                        case 32: // body blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 11, selection, opacity);
                            currentCharacter.PedAppearance.bodyBlemishesStyle = selection;
                            currentCharacter.PedAppearance.bodyBlemishesOpacity = opacity;
                            break;
                    }
                }
            };
            #endregion

            #region clothes
            clothesMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, realIndex) => ChangeClothingListItem(realIndex, newSelectionIndex, listItem);

            clothesMenu.OnListItemSelect += async (sender, listItem, listIndex, realIndex) =>
            {
                int controlIndex = 0;
                bool isCtrlPressed = Game.IsControlPressed(controlIndex, Control.Duck);

                if (isCtrlPressed)
                {
                    string userInput = await GetUserInput("Enter Drawable ID", 5);

                    if (string.IsNullOrEmpty(userInput) || !int.TryParse(userInput, out int drawableId) || drawableId < 0 || drawableId > listItem.ItemsCount)
                    {
                        Notify.Error("Invalid input");
                        return;
                    }

                    listItem.ListIndex = drawableId;

                    ChangeClothingListItem(realIndex, drawableId, listItem);
                    return;
                }

                var componentIndex = realIndex + 1; // skip face options as that fucks up with inheritance faces
                if (realIndex > 0) // skip hair features as that is done in the appeareance menu
                {
                    componentIndex += 1;
                }

                var textureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, componentIndex);
                var newTextureIndex = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, listIndex) - 1 < textureIndex + 1 ? 0 : textureIndex + 1;
                SetPedComponentVariation(Game.PlayerPed.Handle, componentIndex, listIndex, newTextureIndex, 0);
                currentCharacter.DrawableVariations.clothes ??= new Dictionary<int, KeyValuePair<int, int>>();

                var maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, listIndex);

                currentCharacter.DrawableVariations.clothes[componentIndex] = new KeyValuePair<int, int>(listIndex, newTextureIndex);
                listItem.Description = $"使用方向键选择模型，并按 ~o~ENTER~s~ 循环切换可用纹理。当前纹理：#{newTextureIndex + 1}/{maxTextures}。";
            };

            void ChangeClothingListItem(int itemIndex, int newSelectionIndex, MenuListItem listItem)
            {
                var componentIndex = itemIndex + 1;
                if (itemIndex > 0)
                {
                    componentIndex += 1;
                }

                var textureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, componentIndex);
                var newTextureIndex = 0;
                SetPedComponentVariation(Game.PlayerPed.Handle, componentIndex, newSelectionIndex, newTextureIndex, 0);
                currentCharacter.DrawableVariations.clothes ??= new Dictionary<int, KeyValuePair<int, int>>();

                var maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, newSelectionIndex);

                currentCharacter.DrawableVariations.clothes[componentIndex] = new KeyValuePair<int, int>(newSelectionIndex, newTextureIndex);
                listItem.Description = $"使用方向键选择模型，并按 ~o~ENTER~s~ 循环切换可用纹理。当前纹理：#{newTextureIndex + 1}/{maxTextures}。";
            }
            #endregion

            #region props
            propsMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, realIndex) => ChangePropListItem(realIndex, newSelectionIndex, listItem);

            propsMenu.OnListItemSelect += async (sender, listItem, listIndex, realIndex) =>
            {
                int controlIndex = 0;
                bool isCtrlPressed = Game.IsControlPressed(controlIndex, Control.Duck);

                if (isCtrlPressed)
                {
                    string userInput = await GetUserInput("Enter Prop ID", 5);

                    if (string.IsNullOrEmpty(userInput) || !int.TryParse(userInput, out int drawableId) || drawableId < -1 || drawableId > listItem.ItemsCount)
                    {
                        Notify.Error("Invalid input");
                        return;
                    }

                    listItem.ListIndex = drawableId;

                    if (drawableId == -1)
                    {
                        ClearPedProp(Game.PlayerPed.Handle, realIndex);
                        return;
                    }

                    ChangePropListItem(realIndex, drawableId, listItem);
                    return;
                }

                var propIndex = realIndex;
                if (realIndex == 3)
                {
                    propIndex = 6;
                }
                if (realIndex == 4)
                {
                    propIndex = 7;
                }

                var textureIndex = GetPedPropTextureIndex(Game.PlayerPed.Handle, propIndex);
                var newTextureIndex = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, listIndex) - 1 < textureIndex + 1 ? 0 : textureIndex + 1;
                if (textureIndex >= GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Game.PlayerPed.Handle, propIndex);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    listItem.Description = $"使用箭头键选择一个可绘制对象,使用 ~o~ENTER~s~ 以循环查看所有可用纹理.";
                }
                else
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, listIndex, newTextureIndex, true);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(listIndex, newTextureIndex);
                    if (GetPedPropIndex(Game.PlayerPed.Handle, propIndex) == -1)
                    {
                        listItem.Description = $"使用箭头键选择一个可绘制对象,使用 ~o~ENTER~s~ 以循环查看所有可用纹理.";
                    }
                    else
                    {
                        var maxPropTextures = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, listIndex);
                        listItem.Description = $"使用方向键选择饰品，并按 ~o~ENTER~s~ 循环切换可用纹理。当前纹理：#{newTextureIndex + 1}/{maxPropTextures}。";
                    }
                }
            };

            void ChangePropListItem(int itemIndex, int newSelectionIndex, MenuListItem listItem)
            {
                var propIndex = itemIndex;
                if (itemIndex == 3)
                {
                    propIndex = 6;
                }
                if (itemIndex == 4)
                {
                    propIndex = 7;
                }

                var textureIndex = 0;
                if (newSelectionIndex >= GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Game.PlayerPed.Handle, propIndex);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    listItem.Description = $"使用箭头键选择一个可绘制对象,使用 ~o~ENTER~s~ 以循环查看所有可用纹理.";
                }
                else
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, newSelectionIndex, textureIndex, true);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(newSelectionIndex, textureIndex);
                    if (GetPedPropIndex(Game.PlayerPed.Handle, propIndex) == -1)
                    {
                        listItem.Description = $"使用箭头键选择一个可绘制对象,使用 ~o~ENTER~s~ 以循环查看所有可用纹理.";
                    }
                    else
                    {
                        var maxPropTextures = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, newSelectionIndex);
                        listItem.Description = $"使用方向键选择饰品，并按 ~o~ENTER~s~ 循环切换可用纹理。当前纹理：#{textureIndex + 1}/{maxPropTextures}。";
                    }
                }
            }
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

            var faceFeaturesNamesList = new string[20]
            {
                "Nose Width",               // 0
                "Noes Peak Height",         // 1
                "Nose Peak Length",         // 2
                "Nose Bone Height",         // 3
                "Nose Peak Lowering",       // 4
                "Nose Bone Twist",          // 5
                "Eyebrows Height",          // 6
                "Eyebrows Depth",           // 7
                "Cheekbones Height",        // 8
                "Cheekbones Width",         // 9
                "Cheeks Width",             // 10
                "Eyes Opening",             // 11
                "Lips Thickness",           // 12
                "Jaw Bone Width",           // 13
                "Jaw Bone Depth/Length",    // 14
                "Chin Height",              // 15
                "Chin Depth/Length",        // 16
                "Chin Width",               // 17
                "Chin Hole Size",           // 18
                "Neck Thickness"            // 19
            };

            for (var i = 0; i < 20; i++)
            {
                var faceFeature = new MenuSliderItem(faceFeaturesNamesList[i], $"设置 {faceFeaturesNamesList[i]} 面部特征值。", 0, 20, 10, true);
                faceShapeMenu.AddMenuItem(faceFeature);
            }

            faceShapeMenu.OnSliderPositionChange += (sender, sliderItem, oldPosition, newPosition, itemIndex) =>
            {
                currentCharacter.FaceShapeFeatures.features ??= new Dictionary<int, float>();
                var value = faceFeaturesValuesList[newPosition];
                currentCharacter.FaceShapeFeatures.features[itemIndex] = value;
                SetPedFaceFeature(Game.PlayerPed.Handle, itemIndex, value);
            };

            #endregion

            #region tattoos
            void CreateListsIfNull()
            {
                currentCharacter.PedTatttoos.HairTattoos ??= [];
                currentCharacter.PedTatttoos.HeadTattoos ??= [];
                currentCharacter.PedTatttoos.TorsoTattoos ??= [];
                currentCharacter.PedTatttoos.LeftArmTattoos ??= [];
                currentCharacter.PedTatttoos.RightArmTattoos ??= [];
                currentCharacter.PedTatttoos.LeftLegTattoos ??= [];
                currentCharacter.PedTatttoos.RightLegTattoos ??= [];
                currentCharacter.PedTatttoos.BadgeTattoos ??= [];
                currentCharacter.PedTatttoos.AddonTattoos ??= [];
            }

            void ApplySavedTattoos()
            {
                // remove all decorations, and then manually re-add them all. what a retarded way of doing this R*....
                ClearPedDecorations(Game.PlayerPed.Handle);

                IEnumerable<KeyValuePair<string, string>> allTattoos = currentCharacter.PedTatttoos.HairTattoos
                    .Concat(currentCharacter.PedTatttoos.HeadTattoos)
                    .Concat(currentCharacter.PedTatttoos.TorsoTattoos)
                    .Concat(currentCharacter.PedTatttoos.LeftArmTattoos)
                    .Concat(currentCharacter.PedTatttoos.RightArmTattoos)
                    .Concat(currentCharacter.PedTatttoos.LeftLegTattoos)
                    .Concat(currentCharacter.PedTatttoos.RightLegTattoos)
                    .Concat(currentCharacter.PedTatttoos.BadgeTattoos)
                    .Concat(currentCharacter.PedTatttoos.AddonTattoos);

                foreach (var tattoo in allTattoos)
                {
                    AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tattoo.Key), Game.GenerateHashASCII(tattoo.Value));
                }

                if (!string.IsNullOrEmpty(currentCharacter.PedAppearance.HairOverlay.Key) && !string.IsNullOrEmpty(currentCharacter.PedAppearance.HairOverlay.Value))
                {
                    // reset hair value
                    AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(currentCharacter.PedAppearance.HairOverlay.Key), Game.GenerateHashASCII(currentCharacter.PedAppearance.HairOverlay.Value));
                }
            }

            tattoosMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                CreateListsIfNull();
                ApplySavedTattoos();
            };

            #region tattoos menu list select events
            tattoosMenu.OnListIndexChange += (sender, item, oldIndex, tattooIndex, menuIndex) =>
            {
                CreateListsIfNull();
                ApplySavedTattoos();
                if (menuIndex == 0) // hair
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HAIR.ElementAt(tattooIndex) : FemaleTattoosCollection.HAIR.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.HairTattoos.Contains(tat))
                    {
                        AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tat.Key), Game.GenerateHashASCII(tat.Value));
                    }
                }
                else if (menuIndex == 1) // head
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HEAD.ElementAt(tattooIndex) : FemaleTattoosCollection.HEAD.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.HeadTattoos.Contains(tat))
                    {
                        AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tat.Key), Game.GenerateHashASCII(tat.Value));
                    }
                }
                else if (menuIndex == 2) // torso
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.TORSO.ElementAt(tattooIndex) : FemaleTattoosCollection.TORSO.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.TorsoTattoos.Contains(tat))
                    {
                        AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tat.Key), Game.GenerateHashASCII(tat.Value));
                    }
                }
                else if (menuIndex == 3) // left arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.LeftArmTattoos.Contains(tat))
                    {
                        AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tat.Key), Game.GenerateHashASCII(tat.Value));
                    }
                }
                else if (menuIndex == 4) // right arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.RightArmTattoos.Contains(tat))
                    {
                        AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tat.Key), Game.GenerateHashASCII(tat.Value));
                    }
                }
                else if (menuIndex == 5) // left leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.LeftLegTattoos.Contains(tat))
                    {
                        AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tat.Key), Game.GenerateHashASCII(tat.Value));
                    }
                }
                else if (menuIndex == 6) // right leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.RightLegTattoos.Contains(tat))
                    {
                        AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tat.Key), Game.GenerateHashASCII(tat.Value));
                    }
                }
                else if (menuIndex == 7) // badges
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.BADGES.ElementAt(tattooIndex) : FemaleTattoosCollection.BADGES.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.BadgeTattoos.Contains(tat))
                    {
                        AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tat.Key), Game.GenerateHashASCII(tat.Value));
                    }
                }
                else if (menuIndex == 8) // addon
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.ADDONS.ElementAt(tattooIndex) : FemaleTattoosCollection.ADDONS.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.AddonTattoos.Contains(tat))
                    {
                        AddPedDecorationFromHashes(Game.PlayerPed.Handle, Game.GenerateHashASCII(tat.Key), Game.GenerateHashASCII(tat.Value));
                    }
                }
            };

            tattoosMenu.OnListItemSelect += async (sender, item, tattooIndex, menuIndex) =>
            {
                CreateListsIfNull();

                int controlIndex = 0;
                bool isBadgesItem = menuIndex == 7;
                bool isCtrlPressed = Game.IsControlPressed(controlIndex, Control.Duck);

                if (isCtrlPressed)
                {
                    string userInput = await GetUserInput($"Enter {(isBadgesItem ? "Badge" : "Tattoo")} ID", 5);

                    if (string.IsNullOrEmpty(userInput) || !int.TryParse(userInput, out int drawableId) || drawableId < 1 || drawableId > item.ItemsCount)
                    {
                        Notify.Error("Invalid input");
                        return;
                    }

                    drawableId--;

                    item.ListIndex = drawableId;

                    tattooIndex = drawableId;
                }

                if (menuIndex == 0) // hair
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HAIR.ElementAt(tattooIndex) : FemaleTattoosCollection.HAIR.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.HairTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"Hair Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                        currentCharacter.PedTatttoos.HairTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"hair Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                        currentCharacter.PedTatttoos.HairTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 1) // head
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HEAD.ElementAt(tattooIndex) : FemaleTattoosCollection.HEAD.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.HeadTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"Head Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                        currentCharacter.PedTatttoos.HeadTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"Head Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                        currentCharacter.PedTatttoos.HeadTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 2) // torso
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.TORSO.ElementAt(tattooIndex) : FemaleTattoosCollection.TORSO.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.TorsoTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"Torso Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                        currentCharacter.PedTatttoos.TorsoTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"Torso Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                        currentCharacter.PedTatttoos.TorsoTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 3) // left arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.LeftArmTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"Left Arm Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                        currentCharacter.PedTatttoos.LeftArmTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"Left Arm Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                        currentCharacter.PedTatttoos.LeftArmTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 4) // right arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.RightArmTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"Right Arm Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                        currentCharacter.PedTatttoos.RightArmTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"Right Arm Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                        currentCharacter.PedTatttoos.RightArmTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 5) // left leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.LeftLegTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"Left Leg Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                        currentCharacter.PedTatttoos.LeftLegTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"Left Leg Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                        currentCharacter.PedTatttoos.LeftLegTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 6) // right leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.RightLegTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"Right Leg Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                        currentCharacter.PedTatttoos.RightLegTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"Right Leg Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                        currentCharacter.PedTatttoos.RightLegTattoos.Add(tat);
                    }
                }
                else if (isBadgesItem)
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.BADGES.ElementAt(tattooIndex) : FemaleTattoosCollection.BADGES.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.BadgeTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"Badge #{tattooIndex + 1} has been ~r~removed~s~.");
                        currentCharacter.PedTatttoos.BadgeTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"Badge #{tattooIndex + 1} has been ~g~added~s~.");
                        currentCharacter.PedTatttoos.BadgeTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 8) // addon
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.ADDONS.ElementAt(tattooIndex) : FemaleTattoosCollection.ADDONS.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.AddonTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"Addon Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                        currentCharacter.PedTatttoos.AddonTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"Addon Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                        currentCharacter.PedTatttoos.AddonTattoos.Add(tat);
                    }
                }

                ApplySavedTattoos();

            };

            // eventhandler for when a tattoo is selected.
            tattoosMenu.OnItemSelect += (sender, item, index) =>
            {
                Notify.Success("所有纹身均已清除.");
                currentCharacter.PedTatttoos.HairTattoos.Clear();
                currentCharacter.PedTatttoos.HeadTattoos.Clear();
                currentCharacter.PedTatttoos.TorsoTattoos.Clear();
                currentCharacter.PedTatttoos.LeftArmTattoos.Clear();
                currentCharacter.PedTatttoos.RightArmTattoos.Clear();
                currentCharacter.PedTatttoos.LeftLegTattoos.Clear();
                currentCharacter.PedTatttoos.RightLegTattoos.Clear();
                currentCharacter.PedTatttoos.BadgeTattoos.Clear();
                currentCharacter.PedTatttoos.AddonTattoos.Clear();
                ClearPedDecorations(Game.PlayerPed.Handle);
            };

            #endregion
            #endregion


            // handle list changes in the character creator menu.
            createCharacterMenu.OnListIndexChange += (sender, item, oldListIndex, newListIndex, itemIndex) =>
            {
                if (item == faceExpressionList)
                {
                    currentCharacter.FacialExpression = facial_expressions[newListIndex];
                    SetFacialIdleAnimOverride(Game.PlayerPed.Handle, currentCharacter.FacialExpression ?? facial_expressions[0], null);
                }
                else if (item == categoryBtn && categoryBtn.ItemData is Tuple<List<string>, List<MenuItem.Icon>> categoryData)
                {
                    List<string> categoryNames = categoryData.Item1;
                    List<MenuItem.Icon> categoryIcons = categoryData.Item2;
                    currentCharacter.Category = categoryNames[newListIndex];
                    categoryBtn.RightIcon = categoryIcons[newListIndex];
                }
            };

            // handle button presses for the createCharacter menu.
            createCharacterMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == randomizeButton)
                {
                    _parentOne = _random.Next(parentNames.Count);
                    _parentOneSkin = _random.Next(skinNames.Count);
                    _parentTwo = _random.Next(parentNames.Count);
                    _parentTwoSkin = _random.Next(skinNames.Count);
                    _skinMixValue = (float)_random.NextDouble();
                    _shapeMixValue = (float)_random.NextDouble();

                    SetHeadBlend();

                    if (currentCharacter.FaceShapeFeatures.features == null)
                    {
                        currentCharacter.FaceShapeFeatures.features = [];
                    }

                    for (int i = 0; i < 20; i++)
                    {
                        shapeFaceValues[i] = _random.Next(5, 15);
                        SetPedFaceFeature(Game.PlayerPed.Handle, i, faceFeaturesValuesList[shapeFaceValues[i]]);
                        currentCharacter.FaceShapeFeatures.features[i] = faceFeaturesValuesList[shapeFaceValues[i]];
                    }

                    int bodyHair = _random.Next(31);

                    ChangePlayerHair(_random.Next(0, GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2)));
                    ChangePlayerHairColor(bodyHair, _random.Next(31));
                    ChangePlayerEyeColor(_random.Next(0, 9));

                    for (int i = 0; i < 12; i++)
                    {
                        int value;
                        int colorIndex = 0;
                        bool colorRequired = false;

                        int color = i == 1 || i == 2 || i == 10 ? bodyHair : _random.Next(17);
                        float opacity = (float)_random.NextDouble();

                        switch (i)
                        {
                            case 0:
                                value = _random.Next(blemishesStyleList.Count);

                                currentCharacter.PedAppearance.blemishesStyle = value;
                                currentCharacter.PedAppearance.blemishesOpacity = opacity;
                                break;

                            case 1:
                                if (!currentCharacter.IsMale)
                                {
                                    appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0f);
                                    continue;
                                }

                                value = _random.Next(beardStylesList.Count);
                                colorRequired = true;
                                colorIndex = 1;

                                currentCharacter.PedAppearance.beardStyle = value;
                                currentCharacter.PedAppearance.beardColor = color;
                                currentCharacter.PedAppearance.beardOpacity = opacity;
                                break;

                            case 2:
                                value = _random.Next(eyebrowsStyleList.Count);
                                colorRequired = true;
                                colorIndex = 1;

                                currentCharacter.PedAppearance.eyebrowsColor = value;
                                currentCharacter.PedAppearance.eyebrowsStyle = color;
                                currentCharacter.PedAppearance.eyebrowsOpacity = opacity;
                                break;

                            case 3:
                                value = _random.Next(ageingStyleList.Count);

                                currentCharacter.PedAppearance.ageingStyle = value;
                                currentCharacter.PedAppearance.ageingOpacity = opacity;
                                break;

                            case 8:
                                if (currentCharacter.IsMale)
                                {
                                    appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0f);
                                    continue;
                                }

                                value = _random.Next(6);
                                colorRequired = true;
                                colorIndex = 2;

                                currentCharacter.PedAppearance.lipstickStyle = value;
                                currentCharacter.PedAppearance.lipstickColor = color;
                                currentCharacter.PedAppearance.lipstickOpacity = opacity;
                                break;

                            case 9:
                                value = _random.Next(molesFrecklesStyleList.Count);

                                currentCharacter.PedAppearance.molesFrecklesStyle = value;
                                currentCharacter.PedAppearance.molesFrecklesOpacity = opacity;
                                break;

                            case 10:
                                if (!currentCharacter.IsMale)
                                {
                                    appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0f);
                                    continue;
                                }

                                value = _random.Next(8);
                                colorRequired = true;
                                colorIndex = 1;

                                currentCharacter.PedAppearance.chestHairStyle = value;
                                currentCharacter.PedAppearance.chestHairColor = color;
                                currentCharacter.PedAppearance.chestHairOpacity = opacity;
                                break;

                            case 11:
                                value = _random.Next(bodyBlemishesList.Count);

                                currentCharacter.PedAppearance.bodyBlemishesStyle = value;
                                currentCharacter.PedAppearance.bodyBlemishesOpacity = opacity;
                                break;

                            default:
                                appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0);
                                continue;
                        }

                        appearanceValues[i] = new Tuple<int, int, float>(value, color, opacity);
                        SetPedHeadOverlay(Game.PlayerPed.Handle, i, appearanceValues[i].Item1, appearanceValues[i].Item3);

                        if (colorRequired)
                        {
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, i, colorIndex, appearanceValues[i].Item2, appearanceValues[i].Item2);
                        }
                    }

                    _facialExpressionSelection = _random.Next(facial_expressions.Count);

                    SetFacialIdleAnimOverride(Game.PlayerPed.Handle, facial_expressions[_facialExpressionSelection], null);

                    currentCharacter.FacialExpression = facial_expressions[_facialExpressionSelection];

                    ((MenuListItem)createCharacterMenu.GetMenuItems()[7]).ListIndex = _facialExpressionSelection;

                    SetPlayerClothing();
                }
                else if (item == saveButton) // save ped
                {
                    if (await SavePed())
                    {
                        while (!MenuController.IsAnyMenuOpen())
                        {
                            await BaseScript.Delay(0);
                        }

                        while (IsControlPressed(2, 201) || IsControlPressed(2, 217) || IsDisabledControlPressed(2, 201) || IsDisabledControlPressed(2, 217))
                        {
                            await BaseScript.Delay(0);
                        }

                        await BaseScript.Delay(100);

                        createCharacterMenu.GoBack();
                    }
                }
                else if (item == exitNoSave) // exit without saving
                {
                    var confirm = false;
                    AddTextEntry("vmenu_warning_message_first_line", "二次确认 you want to exit the character creator?");
                    AddTextEntry("vmenu_warning_message_second_line", "You will lose all (unsaved) customization!");
                    createCharacterMenu.CloseMenu();

                    // wait for confirmation or cancel input.
                    while (true)
                    {
                        await BaseScript.Delay(0);
                        var unk = 1;
                        var unk2 = 1;
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
                        {
                            await BaseScript.Delay(0);
                        }

                        await BaseScript.Delay(100);
                        menu.OpenMenu();
                    }
                    else // otherwise cancel and go back to the editor.
                    {
                        createCharacterMenu.OpenMenu();
                    }
                }
                else if (item == inheritanceButton) // update the inheritance menu anytime it's opened to prevent some weird glitch where old data is used.
                {
                    inheritanceParentOne.ListIndex = _parentOne;
                    inheritanceParentOneSkin.ListIndex = _parentOneSkin;
                    inheritanceParentTwo.ListIndex = _parentTwo;
                    inheritanceParentTwoSkin.ListIndex = _parentTwoSkin;
                    inheritanceShapeMix.Position = (int)(_shapeMixValue * 10f);
                    inheritanceSkinMix.Position = (int)(_skinMixValue * 10f);
                    inheritanceMenu.RefreshIndex();
                }
                else if (item == faceButton)
                {
                    List<MenuItem> items = faceShapeMenu.GetMenuItems();

                    for (int i = 0; i < 20; i++)
                    {
                        if (items[i] is MenuSliderItem sliderItem)
                        {
                            sliderItem.Position = shapeFaceValues[i];
                        }
                    }

                    faceShapeMenu.RefreshIndex();
                }
                else if (item == appearanceButton)
                {
                    List<MenuListItem> menuListItems = [.. appearanceMenu.GetMenuItems().OfType<MenuListItem>()];

                    menuListItems.First(i => i.Text == "头发发型").ListIndex = _hairSelection;
                    menuListItems.First(i => i.Text == "头发颜色").ListIndex = _hairColorSelection;
                    menuListItems.First(i => i.Text == "头发高光颜色").ListIndex = _hairHighlightColorSelection;

                    menuListItems.First(i => i.Text == "瑕疵样式").ListIndex = appearanceValues[0].Item1;
                    menuListItems.First(i => i.Text == "瑕疵可见性").ListIndex = (int)(appearanceValues[0].Item3 * 10);

                    menuListItems.First(i => i.Text == "胡须样式").ListIndex = appearanceValues[1].Item1;
                    menuListItems.First(i => i.Text == "胡须可见性").ListIndex = (int)(appearanceValues[1].Item3 * 10);
                    menuListItems.First(i => i.Text == "胡须颜色").ListIndex = appearanceValues[1].Item2;

                    menuListItems.First(i => i.Text == "眉毛样式").ListIndex = appearanceValues[2].Item1;
                    menuListItems.First(i => i.Text == "眉毛透明度").ListIndex = (int)(appearanceValues[2].Item3 * 10);
                    menuListItems.First(i => i.Text == "眉毛颜色").ListIndex = appearanceValues[2].Item2;

                    menuListItems.First(i => i.Text == "衰老效果样式").ListIndex = appearanceValues[3].Item1;
                    menuListItems.First(i => i.Text == "衰老效果透明度").ListIndex = (int)(appearanceValues[3].Item3 * 10);

                    menuListItems.First(i => i.Text == "妆容样式").ListIndex = appearanceValues[4].Item1;
                    menuListItems.First(i => i.Text == "妆容透明度").ListIndex = (int)(appearanceValues[4].Item3 * 10);
                    menuListItems.First(i => i.Text == "妆容颜色").ListIndex = appearanceValues[4].Item2;

                    menuListItems.First(i => i.Text == "腮红样式").ListIndex = appearanceValues[5].Item1;
                    menuListItems.First(i => i.Text == "腮红透明度").ListIndex = (int)(appearanceValues[5].Item3 * 10);
                    menuListItems.First(i => i.Text == "腮红颜色").ListIndex = appearanceValues[5].Item2;

                    menuListItems.First(i => i.Text == "肤色纹理样式").ListIndex = appearanceValues[6].Item1;
                    menuListItems.First(i => i.Text == "肤色纹理透明度").ListIndex = (int)(appearanceValues[6].Item3 * 10);

                    menuListItems.First(i => i.Text == "晒伤效果样式").ListIndex = appearanceValues[7].Item1;
                    menuListItems.First(i => i.Text == "晒伤效果透明度").ListIndex = (int)(appearanceValues[7].Item3 * 10);

                    menuListItems.First(i => i.Text == "口红样式").ListIndex = appearanceValues[8].Item1;
                    menuListItems.First(i => i.Text == "口红透明度").ListIndex = (int)(appearanceValues[8].Item3 * 10);
                    menuListItems.First(i => i.Text == "口红颜色").ListIndex = appearanceValues[8].Item2;

                    menuListItems.First(i => i.Text == "痣与雀斑样式").ListIndex = appearanceValues[9].Item1;
                    menuListItems.First(i => i.Text == "痣与雀斑透明度").ListIndex = (int)(appearanceValues[9].Item3 * 10);

                    menuListItems.First(i => i.Text == "胸毛样式").ListIndex = appearanceValues[10].Item1;
                    menuListItems.First(i => i.Text == "胸毛透明度").ListIndex = (int)(appearanceValues[10].Item3 * 10);
                    menuListItems.First(i => i.Text == "胸毛颜色").ListIndex = appearanceValues[10].Item2;

                    menuListItems.First(i => i.Text == "身体瑕疵样式").ListIndex = appearanceValues[11].Item1;
                    menuListItems.First(i => i.Text == "身体瑕疵透明度").ListIndex = (int)(appearanceValues[11].Item3 * 10);

                    menuListItems.First(i => i.Text == "眼睛颜色").ListIndex = _eyeColorSelection;

                    appearanceMenu.RefreshIndex();

                    SetHeadBlend();
                }
            };

            // eventhandler for whenever a menu item is selected in the main mp characters menu.
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == createMaleBtn)
                {
                    var model = Game.GenerateHashASCII("mp_m_freemode_01");

                    if (!HasModelLoaded(model))
                    {
                        RequestModel(model);
                        while (!HasModelLoaded(model))
                        {
                            await BaseScript.Delay(0);
                        }
                    }

                    var maxHealth = Game.PlayerPed.MaxHealth;
                    var maxArmour = Game.Player.MaxArmor;
                    var health = Game.PlayerPed.Health;
                    var armour = Game.PlayerPed.Armor;

                    SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                    SetPlayerModel(Game.Player.Handle, model);
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, true);

                    Game.Player.MaxArmor = maxArmour;
                    Game.PlayerPed.MaxHealth = maxHealth;
                    Game.PlayerPed.Health = health;
                    Game.PlayerPed.Armor = armour;

                    ClearPedDecorations(Game.PlayerPed.Handle);
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                    ClearAllPedProps(Game.PlayerPed.Handle);
                    DefaultPlayerColors();

                    MakeCreateCharacterMenu(male: true);
                }
                else if (item == createFemaleBtn)
                {
                    var model = Game.GenerateHashASCII("mp_f_freemode_01");

                    if (!HasModelLoaded(model))
                    {
                        RequestModel(model);
                        while (!HasModelLoaded(model))
                        {
                            await BaseScript.Delay(0);
                        }
                    }

                    var maxHealth = Game.PlayerPed.MaxHealth;
                    var maxArmour = Game.Player.MaxArmor;
                    var health = Game.PlayerPed.Health;
                    var armour = Game.PlayerPed.Armor;

                    SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                    SetPlayerModel(Game.Player.Handle, model);
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, true);

                    Game.Player.MaxArmor = maxArmour;
                    Game.PlayerPed.MaxHealth = maxHealth;
                    Game.PlayerPed.Health = health;
                    Game.PlayerPed.Armor = armour;

                    ClearPedDecorations(Game.PlayerPed.Handle);
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                    ClearAllPedProps(Game.PlayerPed.Handle);
                    DefaultPlayerColors();

                    MakeCreateCharacterMenu(male: false);
                }
                else if (item == savedCharacters)
                {
                    UpdateSavedPedsMenu();
                }
            };
        }

        /// <summary>
        /// Spawns this saved ped.
        /// </summary>
        /// <param name="name"></param>
        internal async Task SpawnThisCharacter(string name, bool restoreWeapons)
        {
            currentCharacter = StorageManager.GetSavedMpCharacterData(name);
            await SpawnSavedPed(restoreWeapons);
        }

        /// <summary>
        /// Spawns the ped from the data inside <see cref="currentCharacter"/>.
        /// Character data MUST be set BEFORE calling this function.
        /// </summary>
        /// <returns></returns>
        private async Task SpawnSavedPed(bool restoreWeapons)
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
                var maxHealth = Game.PlayerPed.MaxHealth;
                var maxArmour = Game.Player.MaxArmor;
                var health = Game.PlayerPed.Health;
                var armour = Game.PlayerPed.Armor;

                SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                SetPlayerModel(Game.Player.Handle, currentCharacter.ModelHash);
                await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, true);

                Game.Player.MaxArmor = maxArmour;
                Game.PlayerPed.MaxHealth = maxHealth;
                Game.PlayerPed.Health = health;
                Game.PlayerPed.Armor = armour;

                ClearPedDecorations(Game.PlayerPed.Handle);
                ClearPedFacialDecorations(Game.PlayerPed.Handle);
                SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                SetPedHairColor(Game.PlayerPed.Handle, 0, 0);
                SetPedEyeColor(Game.PlayerPed.Handle, 0);
                ClearAllPedProps(Game.PlayerPed.Handle);

                await AppySavedDataToPed(currentCharacter, Game.PlayerPed.Handle);
            }

            // Set the facial expression, or set it to 'normal' if it wasn't saved/set before.
            SetFacialIdleAnimOverride(Game.PlayerPed.Handle, currentCharacter.FacialExpression ?? facial_expressions[0], null);
        }

        /// <summary>
        /// Creates the saved mp characters menu.
        /// </summary>
        private void CreateSavedPedsMenu()
        {
            UpdateSavedPedsMenu();

            MenuController.AddMenu(manageSavedCharacterMenu);

            var spawnPed = new MenuItem("生成保存的角色", "生成所选保存的角色.");
            editPedBtn = new MenuItem("编辑已保存角色", "这允许您编辑已保存角色的所有内容.更改将在您点击保存按钮后保存到该角色的保存文件条目.");
            var clonePed = new MenuItem("克隆保存的角色", "这将创建你保存角色的克隆.系统会要求你为该角色提供一个名称.如果该名称已经被使用,则操作将被取消.");
            var setAsDefaultPed = new MenuItem("设为默认角色", "如果你将此角色设置为默认角色,并在“杂项设置”菜单中启用“重生为默认MP角色”选项,那么每当你（重新）重生时,你将变成这个角色.");
            var renameCharacter = new MenuItem("重命名保存的角色", "你可以重命名这个保存的角色.如果该名称已经被使用,则操作将被取消.");
            var saveCurrentPedAsCharacter = new MenuItem("更新角色服装", "将当前服装应用到已保存角色。~r~这会覆盖该角色原有的服装。~w~仅更新服装，不会更改其他外观。")
            {
                LeftIcon = MenuItem.Icon.WARNING
            };
            var delPed = new MenuItem("删除保存的角色", "删除所选保存的角色.此操作无法撤销！")
            {
                LeftIcon = MenuItem.Icon.WARNING
            };
            manageSavedCharacterMenu.AddMenuItem(spawnPed);
            manageSavedCharacterMenu.AddMenuItem(editPedBtn);
            manageSavedCharacterMenu.AddMenuItem(clonePed);
            manageSavedCharacterMenu.AddMenuItem(setCategoryBtn);
            manageSavedCharacterMenu.AddMenuItem(setAsDefaultPed);
            manageSavedCharacterMenu.AddMenuItem(renameCharacter);
            manageSavedCharacterMenu.AddMenuItem(saveCurrentPedAsCharacter);
            manageSavedCharacterMenu.AddMenuItem(delPed);

            MenuController.BindMenuItem(manageSavedCharacterMenu, createCharacterMenu, editPedBtn);

            manageSavedCharacterMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == editPedBtn)
                {
                    currentCharacter = StorageManager.GetSavedMpCharacterData(selectedSavedCharacterManageName);

                    await SpawnSavedPed(true);

                    MakeCreateCharacterMenu(male: currentCharacter.IsMale, editPed: true);
                }
                else if (item == spawnPed)
                {
                    currentCharacter = StorageManager.GetSavedMpCharacterData(selectedSavedCharacterManageName);

                    await SpawnSavedPed(true);
                }
                else if (item == clonePed)
                {
                    var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);
                    var name = await GetUserInput(windowTitle: "Enter a name for the cloned character", defaultText: tmpCharacter.SaveName.Substring(7), maxInputLength: 30);
                    if (string.IsNullOrEmpty(name))
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
                            if (StorageManager.SaveJsonData("mp_ped_" + name, JsonConvert.SerializeObject(tmpCharacter), false))
                            {
                                Notify.Success($"角色已完成克隆. 克隆角色的名称是: ~g~{name}~s~.");
                                MenuController.CloseAllMenus();
                                UpdateSavedPedsMenu();
                                savedCharactersMenu.OpenMenu();
                            }
                            else
                            {
                                Notify.Error("克隆无法创建, 原因不明. 是否已经存在使用该名称的角色 :(");
                            }
                        }
                    }
                }
                else if (item == renameCharacter)
                {
                    var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);
                    var name = await GetUserInput(windowTitle: "Enter a new character name", defaultText: tmpCharacter.SaveName.Substring(7), maxInputLength: 30);
                    if (string.IsNullOrEmpty(name))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
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
                            if (StorageManager.SaveJsonData("mp_ped_" + name, JsonConvert.SerializeObject(tmpCharacter), false))
                            {
                                StorageManager.DeleteSavedStorageItem("mp_ped_" + selectedSavedCharacterManageName);
                                Notify.Success($"角色昵称已重命名为 ~g~{name}~s~.");
                                UpdateSavedPedsMenu();
                                while (!MenuController.IsAnyMenuOpen())
                                {
                                    await BaseScript.Delay(0);
                                }
                                manageSavedCharacterMenu.GoBack();
                            }
                            else
                            {
                                Notify.Error("重命名角色时发生问题, 您的旧角色不会因此被删除.");
                            }
                        }
                    }
                }
                else if (item == saveCurrentPedAsCharacter)
                {
                    if (saveCurrentPedAsCharacter.Label == "二次确认?")
                    {
                        saveCurrentPedAsCharacter.Label = "";
                        var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);

                        tmpCharacter = ReplacePedDataClothing(tmpCharacter);

                        if (StorageManager.SaveJsonData(tmpCharacter.SaveName, JsonConvert.SerializeObject(tmpCharacter), true))
                        {
                            Notify.Success($"This character's clothing has been updated!");
                            UpdateSavedPedsMenu();
                        }
                        else
                        {
                            Notify.Error("Unable to update this character's clothing. The reason is unknown.");
                        }
                    }
                    else
                    {
                        saveCurrentPedAsCharacter.Label = "二次确认?";
                    }
                }
                else if (item == delPed)
                {
                    if (delPed.Label == "二次确认?")
                    {
                        delPed.Label = "";
                        DeleteResourceKvp("mp_ped_" + selectedSavedCharacterManageName);
                        Notify.Success("您保存的角色已删除.");
                        manageSavedCharacterMenu.GoBack();
                        UpdateSavedPedsMenu();
                        manageSavedCharacterMenu.RefreshIndex();
                    }
                    else
                    {
                        delPed.Label = "二次确认?";
                    }
                }
                else if (item == setAsDefaultPed)
                {
                    Notify.Success($"角色: {selectedSavedCharacterManageName} 现在每当您重新生成时, 都会将其用作默认角色.");
                    SetResourceKvp("vmenu_default_character", "mp_ped_" + selectedSavedCharacterManageName);
                }

                if (item != delPed)
                {
                    if (delPed.Label == "二次确认?")
                    {
                        delPed.Label = "";
                    }
                }

                if (item != saveCurrentPedAsCharacter)
                {
                    if (saveCurrentPedAsCharacter.Label == "二次确认?")
                    {
                        saveCurrentPedAsCharacter.Label = "";
                    }
                }
            };

            // Update category preview icon
            manageSavedCharacterMenu.OnListIndexChange += (_, listItem, _, newSelectionIndex, _) =>
            {
                if (listItem.ItemData is List<MenuItem.Icon> icons)
                {
                    listItem.RightIcon = icons[newSelectionIndex];
                }
            };

            // Update character's category
            manageSavedCharacterMenu.OnListItemSelect += async (_, listItem, listIndex, _) =>
            {
                var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);

                string name = listItem.ListItems[listIndex];

                if (name == "Create New")
                {
                    var newName = await GetUserInput(windowTitle: "Enter a category name.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName) || newName.ToLower() == "uncategorized" || newName.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "Enter a category description (optional).", maxInputLength: 120);
                        var newCategory = new MpCharacterCategory
                        {
                            Name = newName,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("mp_character_category_" + newName, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"分类 (~g~{newName}~s~) 已完成保存.");
                            Log($"Saved Category {newName}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                            name = newName;
                        }
                        else
                        {
                            Notify.Error($"保存失败, 很可能是因为该（~y~{newName}~s~）名称已被使用.");
                            return;
                        }
                    }
                }

                tmpCharacter.Category = name;

                var json = JsonConvert.SerializeObject(tmpCharacter);
                if (StorageManager.SaveJsonData(tmpCharacter.SaveName, json, true))
                {
                    Notify.Success("已成功保存您的角色数据.");
                }
                else
                {
                    Notify.Error("您的角色无法保存. 原因不明. :(");
                }

                MenuController.CloseAllMenus();
                UpdateSavedPedsMenu();
                savedCharactersMenu.OpenMenu();
            };

            // reset the "二次确认" state.
            manageSavedCharacterMenu.OnMenuClose += (sender) =>
            {
                foreach (MenuItem item in manageSavedCharacterMenu.GetMenuItems())
                {
                    if (item.Label == "二次确认?")
                    {
                        item.Label = "";
                    }
                }
            };

            // Load selected category
            savedCharactersMenu.OnItemSelect += async (sender, item, index) =>
            {
                // Create new category
                if (item.ItemData is not MpCharacterCategory mpCharacterCategory)
                {
                    var name = await GetUserInput(windowTitle: "Enter a category name.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "Enter a category description (optional).", maxInputLength: 120);
                        var newCategory = new MpCharacterCategory
                        {
                            Name = name,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("mp_character_category_" + name, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"分类 (~g~{name}~s~) 已完成保存.");
                            Log($"Saved Category {name}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                        }
                        else
                        {
                            Notify.Error($"保存失败, 很可能是因为该 (~y~{name}~s~) 名称已被使用.");
                            return;
                        }
                    }
                }
                // Select an old category
                else
                {
                    currentCategory = mpCharacterCategory;
                }

                bool isUncategorized = currentCategory.Name == "未分类";

                savedCharactersCategoryMenu.MenuTitle = currentCategory.Name;
                savedCharactersCategoryMenu.MenuSubtitle = $"~s~分类: ~y~{currentCategory.Name}";
                savedCharactersCategoryMenu.ClearMenuItems();

                var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();

                string ChangeCallback(MenuDynamicListItem item, bool left)
                {
                    int currentIndex = iconNames.IndexOf(item.CurrentItem);
                    int newIndex = left ? currentIndex - 1 : currentIndex + 1;

                    // If going past the start or end of the list
                    if (iconNames.ElementAtOrDefault(newIndex) == default)
                    {
                        if (left)
                        {
                            newIndex = iconNames.Count - 1;
                        }
                        else
                        {
                            newIndex = 0;
                        }
                    }

                    item.RightIcon = (MenuItem.Icon)newIndex;

                    return iconNames[newIndex];
                }

                var renameBtn = new MenuItem("重命名分类", "重命名此分类。")
                {
                    Enabled = !isUncategorized
                };
                var descriptionBtn = new MenuItem("修改分类说明", "修改此分类的说明。")
                {
                    Enabled = !isUncategorized
                };
                var iconBtn = new MenuDynamicListItem("修改分类图标", iconNames[(int)currentCategory.Icon], new MenuDynamicListItem.ChangeItemCallback(ChangeCallback), "修改此分类的图标；选择后即保存。")
                {
                    Enabled = !isUncategorized,
                    RightIcon = currentCategory.Icon
                };
                var deleteBtn = new MenuItem("删除分类", "删除此分类。此操作不可撤销！")
                {
                    RightIcon = MenuItem.Icon.WARNING,
                    Enabled = !isUncategorized
                };
                var deleteCharsBtn = new MenuCheckboxItem("删除全部角色", "勾选后，点击“删除分类”会同时删除此分类中的所有已保存角色；未勾选时，角色将移至“未分类”。")
                {
                    Enabled = !isUncategorized
                };

                savedCharactersCategoryMenu.AddMenuItem(renameBtn);
                savedCharactersCategoryMenu.AddMenuItem(descriptionBtn);
                savedCharactersCategoryMenu.AddMenuItem(iconBtn);
                savedCharactersCategoryMenu.AddMenuItem(deleteBtn);
                savedCharactersCategoryMenu.AddMenuItem(deleteCharsBtn);

                var spacer = GetSpacerMenuItem("↓ Characters ↓");
                savedCharactersCategoryMenu.AddMenuItem(spacer);

                List<string> names = GetAllMpCharacterNames();

                if (names.Count > 0)
                {
                    var defaultChar = GetResourceKvpString("vmenu_default_character") ?? "";

                    names.Sort((a, b) => a.ToLower().CompareTo(b.ToLower()));
                    foreach (var name in names)
                    {
                        var tmpData = StorageManager.GetSavedMpCharacterData("mp_ped_" + name);

                        if (string.IsNullOrEmpty(tmpData.Category))
                        {
                            if (!isUncategorized)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (tmpData.Category != currentCategory.Name)
                            {
                                continue;
                            }
                        }

                        var btn = new MenuItem(name, "点击可生成、编辑、克隆、重命名或删除此已保存角色。")
                        {
                            Label = "→→→",
                            LeftIcon = tmpData.IsMale ? MenuItem.Icon.MALE : MenuItem.Icon.FEMALE,
                            ItemData = tmpData.IsMale
                        };
                        if (defaultChar == "mp_ped_" + name)
                        {
                            btn.LeftIcon = MenuItem.Icon.TICK;
                            btn.Description += " ~g~This character is currently set as your default character and will be used whenever you (re)spawn.";
                        }
                        savedCharactersCategoryMenu.AddMenuItem(btn);
                        MenuController.BindMenuItem(savedCharactersCategoryMenu, manageSavedCharacterMenu, btn);
                    }
                }
            };

            savedCharactersCategoryMenu.OnIndexChange += async (menu, oldItem, newItem, oldIndex, newIndex) =>
            {
                if (!GetSettingsBool(Setting.vmenu_mp_ped_preview) || !MainMenu.MiscSettingsMenu.MPPedPreviews)
                {
                    return;
                }

                if (Entity.Exists(_clone))
                {
                    _clone.Delete();
                }

                // Only show preview for ped items, not menu items
                if (newItem.ItemData is not bool)
                {
                    return;
                }

                MultiplayerPedData character = StorageManager.GetSavedMpCharacterData(newItem.Text);

                if (!HasModelLoaded(character.ModelHash))
                {
                    RequestModel(character.ModelHash);
                    while (!HasModelLoaded(character.ModelHash))
                    {
                        await Delay(0);
                    }
                }

                ///
                /// Credit to whbl (https://forum.cfx.re/u/whbl) for the inspiration for this feature.
                /// https://forum.cfx.re/t/free-standalone-virtual-ped/5052458
                ///

                Ped playerPed = Game.PlayerPed;
                Vector3 clientPedPosition = playerPed.Position;

                _clone = new Ped(CreatePed(26, character.ModelHash, clientPedPosition.X, clientPedPosition.Y, clientPedPosition.Z - 3f, playerPed.Heading, false, false))
                {
                    IsCollisionEnabled = false,
                    IsInvincible = true,
                    BlockPermanentEvents = true,
                    IsPositionFrozen = true
                };

                int cloneHandle = _clone.Handle;

                await AppySavedDataToPed(character, cloneHandle);

                SetEntityCanBeDamaged(cloneHandle, false);
                SetPedAoBlobRendering(cloneHandle, false);

                while (Entity.Exists(_clone))
                {
                    Vector3 worldCoord = Vector3.Zero;
                    Vector3 normal = Vector3.Zero;

                    GetWorldCoordFromScreenCoord(0.6f, 0.8f, ref worldCoord, ref normal);

                    Vector3 cameraRotation = GameplayCamera.Rotation;

                    _clone.Position = worldCoord + (normal * 3.5f);
                    _clone.Rotation = new Vector3(cameraRotation.X * -1, 0f, cameraRotation.Z + 180);
                    _clone.Heading = cameraRotation.Z + 180;

                    GameplayCamera.ClampPitch(0f, 0f);

                    await Delay(0);
                }
            };


            savedCharactersCategoryMenu.OnItemSelect += async (sender, item, index) =>
            {
                switch (index)
                {
                    // Rename Category
                    case 0:
                        var name = await GetUserInput(windowTitle: "Enter a new category name", defaultText: currentCategory.Name, maxInputLength: 30);

                        if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        else if (GetAllCategoryNames().Contains(name) || !string.IsNullOrEmpty(GetResourceKvpString("mp_character_category_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                            return;
                        }

                        string oldName = currentCategory.Name;

                        currentCategory.Name = name;

                        if (StorageManager.SaveJsonData("mp_character_category_" + name, JsonConvert.SerializeObject(currentCategory), false))
                        {
                            StorageManager.DeleteSavedStorageItem("mp_character_category_" + oldName);

                            int totalCount = 0;
                            int updatedCount = 0;
                            List<string> characterNames = GetAllMpCharacterNames();

                            if (characterNames.Count > 0)
                            {
                                foreach (var characterName in characterNames)
                                {
                                    var tmpData = StorageManager.GetSavedMpCharacterData("mp_ped_" + characterName);

                                    if (string.IsNullOrEmpty(tmpData.Category))
                                    {
                                        continue;
                                    }

                                    if (tmpData.Category != oldName)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    tmpData.Category = name;

                                    if (StorageManager.SaveJsonData(tmpData.SaveName, JsonConvert.SerializeObject(tmpData), true))
                                    {
                                        updatedCount++;
                                        Log($"Updated category for \"{tmpData.SaveName}\"");
                                    }
                                    else
                                    {
                                        Log($"Something went wrong when updating category for \"{tmpData.SaveName}\"");
                                    }
                                }
                            }

                            Notify.Success($"Your category has been renamed to ~g~<C>{name}</C>~s~. {updatedCount}/{totalCount} characters updated.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("Something went wrong while renaming your category, your old category will NOT be deleted because of this.");
                        }
                        break;

                    // Change Category Description
                    case 1:
                        var description = await GetUserInput(windowTitle: "Enter a new category description", defaultText: currentCategory.Description, maxInputLength: 120);

                        currentCategory.Description = description;

                        if (StorageManager.SaveJsonData("mp_character_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                        {
                            Notify.Success($"Your category description has been changed.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("Something went wrong while changing your category description.");
                        }
                        break;

                    // Delete Category
                    case 3:
                        if (item.Label == "二次确认?")
                        {
                            bool deletePeds = (sender.GetMenuItems().ElementAt(4) as MenuCheckboxItem).Checked;

                            item.Label = "";
                            DeleteResourceKvp("mp_character_category_" + currentCategory.Name);

                            int totalCount = 0;
                            int updatedCount = 0;

                            List<string> characterNames = GetAllMpCharacterNames();

                            if (characterNames.Count > 0)
                            {
                                foreach (var characterName in characterNames)
                                {
                                    var tmpData = StorageManager.GetSavedMpCharacterData("mp_ped_" + characterName);

                                    if (string.IsNullOrEmpty(tmpData.Category))
                                    {
                                        continue;
                                    }

                                    if (tmpData.Category != currentCategory.Name)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    if (deletePeds)
                                    {
                                        updatedCount++;

                                        DeleteResourceKvp("mp_ped_" + tmpData.SaveName);
                                    }
                                    else
                                    {
                                        tmpData.Category = "未分类";

                                        if (StorageManager.SaveJsonData(tmpData.SaveName, JsonConvert.SerializeObject(tmpData), true))
                                        {
                                            updatedCount++;
                                            Log($"Updated category for \"{tmpData.SaveName}\"");
                                        }
                                        else
                                        {
                                            Log($"Something went wrong when updating category for \"{tmpData.SaveName}\"");
                                        }
                                    }
                                }
                            }

                            Notify.Success($"Your saved category has been deleted. {updatedCount}/{totalCount} characters {(deletePeds ? "deleted" : "updated")}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersMenu.OpenMenu();
                        }
                        else
                        {
                            item.Label = "二次确认?";
                        }
                        break;

                    // Load saved character menu
                    default:
                        List<string> categoryNames = GetAllCategoryNames();
                        List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
                        int nameIndex = categoryNames.IndexOf(currentCategory.Name);

                        setCategoryBtn.ItemData = categoryIcons;
                        setCategoryBtn.ListItems = categoryNames;
                        setCategoryBtn.ListIndex = nameIndex == 1 ? 0 : nameIndex;
                        setCategoryBtn.RightIcon = categoryIcons[setCategoryBtn.ListIndex];
                        selectedSavedCharacterManageName = item.Text;
                        manageSavedCharacterMenu.MenuSubtitle = item.Text;
                        manageSavedCharacterMenu.CounterPreText = $"{(item.LeftIcon == MenuItem.Icon.MALE ? "(Male)" : "(Female)")} ";
                        manageSavedCharacterMenu.RefreshIndex();
                        break;
                }
            };

            // Change Category Icon
            savedCharactersCategoryMenu.OnDynamicListItemSelect += (_, _, currentItem) =>
            {
                var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();
                int iconIndex = iconNames.IndexOf(currentItem);

                currentCategory.Icon = (MenuItem.Icon)iconIndex;

                if (StorageManager.SaveJsonData("mp_character_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                {
                    Notify.Success($"Your category icon been changed to ~g~<C>{iconNames[iconIndex]}</C>~s~.");
                    UpdateSavedPedsMenu();
                }
                else
                {
                    Notify.Error("Something went wrong while changing your category icon.");
                }
            };

            savedCharactersCategoryMenu.OnMenuClose += (_) =>
            {
                if (Entity.Exists(_clone))
                {
                    _clone.Delete();
                }
            };

        }

        /// <summary>
        /// Updates the saved peds menu.
        /// </summary>
        private void UpdateSavedPedsMenu()
        {
            var categories = GetAllCategoryNames();

            savedCharactersMenu.ClearMenuItems();

            var createCategoryBtn = new MenuItem("创建分类", "创建新的角色分类。")
            {
                Label = "→→→"
            };
            savedCharactersMenu.AddMenuItem(createCategoryBtn);

            var spacer = GetSpacerMenuItem("↓ Character Categories ↓");
            savedCharactersMenu.AddMenuItem(spacer);

            var uncategorized = new MpCharacterCategory
            {
                Name = "未分类",
                Description = "所有未分配到分类的已保存 MP 角色。"
            };
            var uncategorizedBtn = new MenuItem(uncategorized.Name, uncategorized.Description)
            {
                Label = "→→→",
                ItemData = uncategorized
            };
            savedCharactersMenu.AddMenuItem(uncategorizedBtn);
            MenuController.BindMenuItem(savedCharactersMenu, savedCharactersCategoryMenu, uncategorizedBtn);

            // Remove "Create New" and "未分类"
            categories.RemoveRange(0, 2);

            if (categories.Count > 0)
            {
                categories.Sort((a, b) => a.ToLower().CompareTo(b.ToLower()));
                foreach (var item in categories)
                {
                    MpCharacterCategory category = StorageManager.GetSavedMpCharacterCategoryData("mp_character_category_" + item);

                    var btn = new MenuItem(category.Name, category.Description)
                    {
                        Label = "→→→",
                        LeftIcon = category.Icon,
                        ItemData = category
                    };
                    savedCharactersMenu.AddMenuItem(btn);
                    MenuController.BindMenuItem(savedCharactersMenu, savedCharactersCategoryMenu, btn);
                }
            }

            savedCharactersMenu.RefreshIndex();
        }

        private List<string> GetAllCategoryNames()
        {
            var categories = new List<string>();
            var handle = StartFindKvp("mp_character_category_");
            while (true)
            {
                var foundCategory = FindKvp(handle);
                if (string.IsNullOrEmpty(foundCategory))
                {
                    break;
                }
                else
                {
                    categories.Add(foundCategory.Substring(22));
                }
            }
            EndFindKvp(handle);

            categories.Insert(0, "Create New");
            categories.Insert(1, "未分类");

            return categories;
        }

        private List<MenuItem.Icon> GetCategoryIcons(List<string> categoryNames)
        {
            List<MenuItem.Icon> icons = new List<MenuItem.Icon> { };

            foreach (var name in categoryNames)
            {
                icons.Add(StorageManager.GetSavedMpCharacterCategoryData("mp_character_category_" + name).Icon);
            }

            return icons;
        }

        private List<string> GetAllMpCharacterNames()
        {
            var names = new List<string>();
            var handle = StartFindKvp("mp_ped_");
            while (true)
            {
                var foundName = FindKvp(handle);
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

            return names;
        }

        private MultiplayerPedData ReplacePedDataClothing(MultiplayerPedData character)
        {
            int handle = Game.PlayerPed.Handle;

            // Drawables
            for (int i = 0; i < 12; i++)
            {
                int drawable = GetPedDrawableVariation(handle, i);
                int texture = GetPedTextureVariation(handle, i);
                character.DrawableVariations.clothes[i] = new KeyValuePair<int, int>(drawable, texture);
            }

            for (int i = 0; i < 8; i++)
            {
                int prop = GetPedPropIndex(handle, i);
                int texture = GetPedPropTextureIndex(handle, i);
                character.PropVariations.props[i] = new KeyValuePair<int, int>(prop, texture);
            }

            return character;
        }

        internal void SetHeadBlend()
        {
            SetPedHeadBlendData(
                Game.PlayerPed.Handle,
                _parentOne, _parentTwo, shapeThirdID: 0,
                _parentOneSkin, _parentTwoSkin, skinThirdID: 0,
                _shapeMixValue, _skinMixValue, thirdMix: 0f, isParent: false
            );
        }

        internal void ChangePlayerHair(int newHairIndex)
        {
            ClearPedFacialDecorations(Game.PlayerPed.Handle);
            currentCharacter.PedAppearance.HairOverlay = new KeyValuePair<string, string>("", "");

            if (newHairIndex >= GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2))
            {
                SetPedComponentVariation(Game.PlayerPed.Handle, 2, 0, 0, 0);
                currentCharacter.PedAppearance.hairStyle = 0;
            }
            else
            {
                SetPedComponentVariation(Game.PlayerPed.Handle, 2, newHairIndex, 0, 0);
                currentCharacter.PedAppearance.hairStyle = newHairIndex;
                if (hairOverlays.ContainsKey(newHairIndex))
                {
                    SetPedFacialDecoration(Game.PlayerPed.Handle, Game.GenerateHashASCII(hairOverlays[newHairIndex].Key), Game.GenerateHashASCII(hairOverlays[newHairIndex].Value));
                    currentCharacter.PedAppearance.HairOverlay = new KeyValuePair<string, string>(hairOverlays[newHairIndex].Key, hairOverlays[newHairIndex].Value);
                }
            }

            _hairSelection = newHairIndex;
        }

        internal void ChangePlayerHairColor(int color, int highlight)
        {
            SetPedHairColor(Game.PlayerPed.Handle, color, highlight);

            currentCharacter.PedAppearance.hairColor = color;
            currentCharacter.PedAppearance.hairHighlightColor = highlight;

            _hairColorSelection = color;
            _hairHighlightColorSelection = highlight;
        }

        internal void ChangePlayerEyeColor(int color)
        {
            SetPedEyeColor(Game.PlayerPed.Handle, color);

            currentCharacter.PedAppearance.eyeColor = color;

            _eyeColorSelection = color;
        }

        internal void SetPlayerClothing()
        {
            SetPedComponentVariation(Game.PlayerPed.Handle, 3, 15, 0, 0);

            currentCharacter.DrawableVariations.clothes[3] = new KeyValuePair<int, int>(15, 0);

            if (currentCharacter.IsMale)
            {
                SetPedComponentVariation(Game.PlayerPed.Handle, 8, 15, 0, 0);

                currentCharacter.DrawableVariations.clothes[8] = new KeyValuePair<int, int>(15, 0);

                SetPedComponentVariation(Game.PlayerPed.Handle, 11, 15, 0, 0);

                currentCharacter.DrawableVariations.clothes[11] = new KeyValuePair<int, int>(15, 0);

                int pantsColor = _random.Next(GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 4, 61));

                SetPedComponentVariation(Game.PlayerPed.Handle, 4, 61, pantsColor, 0);

                currentCharacter.DrawableVariations.clothes[4] = new KeyValuePair<int, int>(61, pantsColor);

                SetPedComponentVariation(Game.PlayerPed.Handle, 6, 34, 0, 0);

                currentCharacter.DrawableVariations.clothes[6] = new KeyValuePair<int, int>(34, 0);
            }
            else
            {
                SetPedComponentVariation(Game.PlayerPed.Handle, 8, 14, 0, 0);
                SetPedComponentVariation(Game.PlayerPed.Handle, 8, 14, 0, 0);

                currentCharacter.DrawableVariations.clothes[8] = new KeyValuePair<int, int>(14, 0);

                int braColor = _random.Next(GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 4, 17));

                SetPedComponentVariation(Game.PlayerPed.Handle, 4, 17, braColor, 0);

                currentCharacter.DrawableVariations.clothes[4] = new KeyValuePair<int, int>(17, braColor);

                SetPedComponentVariation(Game.PlayerPed.Handle, 11, 18, braColor, 0);

                currentCharacter.DrawableVariations.clothes[11] = new KeyValuePair<int, int>(18, braColor);

                SetPedComponentVariation(Game.PlayerPed.Handle, 6, 35, 0, 0);

                currentCharacter.DrawableVariations.clothes[6] = new KeyValuePair<int, int>(35, 0);
            }
        }

        /// <summary>
        /// Sets all the ped's overlay colors to their default (0) entry.
        /// When called, prevents default color being bright green.
        /// </summary>
        internal void DefaultPlayerColors()
        {
            SetHeadBlend();

            for (int i = 0; i < 12; i++)
            {
                int color = 0;
                int colorIndex = 0;

                switch (i)
                {
                    case 1:
                        colorIndex = 1;
                        break;

                    case 2:
                        colorIndex = 1;
                        break;

                    case 8:
                        colorIndex = 2;
                        break;

                    case 10:
                        colorIndex = 1;
                        break;

                    default:
                        continue;
                }

                SetPedHeadOverlay(Game.PlayerPed.Handle, i, 0, 0f);

                if (colorIndex > 0)
                {
                    SetPedHeadOverlayColor(Game.PlayerPed.Handle, i, colorIndex, color, color);
                }
            }
        }

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

        internal async Task AppySavedDataToPed(MultiplayerPedData character, int pedHandle)
        {
            #region headblend
            PedHeadBlendData data = character.PedHeadBlendData;
            SetPedHeadBlendData(pedHandle, data.FirstFaceShape, data.SecondFaceShape, data.ThirdFaceShape, data.FirstSkinTone, data.SecondSkinTone, data.ThirdSkinTone, data.ParentFaceShapePercent, data.ParentSkinTonePercent, 0f, data.IsParentInheritance);

            while (!HasPedHeadBlendFinished(pedHandle))
            {
                await Delay(0);
            }
            #endregion

            #region appearance
            PedAppearance appData = character.PedAppearance;
            // hair
            SetPedComponentVariation(pedHandle, 2, appData.hairStyle, 0, 0);
            SetPedHairColor(pedHandle, appData.hairColor, appData.hairHighlightColor);
            if (!string.IsNullOrEmpty(appData.HairOverlay.Key) && !string.IsNullOrEmpty(appData.HairOverlay.Value))
            {
                SetPedFacialDecoration(pedHandle, Game.GenerateHashASCII(appData.HairOverlay.Key), Game.GenerateHashASCII(appData.HairOverlay.Value));
            }
            // blemishes
            SetPedHeadOverlay(pedHandle, 0, appData.blemishesStyle, appData.blemishesOpacity);
            // bread
            SetPedHeadOverlay(pedHandle, 1, appData.beardStyle, appData.beardOpacity);
            SetPedHeadOverlayColor(pedHandle, 1, 1, appData.beardColor, appData.beardColor);
            // eyebrows
            SetPedHeadOverlay(pedHandle, 2, appData.eyebrowsStyle, appData.eyebrowsOpacity);
            SetPedHeadOverlayColor(pedHandle, 2, 1, appData.eyebrowsColor, appData.eyebrowsColor);
            // ageing
            SetPedHeadOverlay(pedHandle, 3, appData.ageingStyle, appData.ageingOpacity);
            // makeup
            SetPedHeadOverlay(pedHandle, 4, appData.makeupStyle, appData.makeupOpacity);
            SetPedHeadOverlayColor(pedHandle, 4, 2, appData.makeupColor, appData.makeupColor);
            // blush
            SetPedHeadOverlay(pedHandle, 5, appData.blushStyle, appData.blushOpacity);
            SetPedHeadOverlayColor(pedHandle, 5, 2, appData.blushColor, appData.blushColor);
            // complexion
            SetPedHeadOverlay(pedHandle, 6, appData.complexionStyle, appData.complexionOpacity);
            // sundamage
            SetPedHeadOverlay(pedHandle, 7, appData.sunDamageStyle, appData.sunDamageOpacity);
            // lipstick
            SetPedHeadOverlay(pedHandle, 8, appData.lipstickStyle, appData.lipstickOpacity);
            SetPedHeadOverlayColor(pedHandle, 8, 2, appData.lipstickColor, appData.lipstickColor);
            // moles and freckles
            SetPedHeadOverlay(pedHandle, 9, appData.molesFrecklesStyle, appData.molesFrecklesOpacity);
            // chest hair 
            SetPedHeadOverlay(pedHandle, 10, appData.chestHairStyle, appData.chestHairOpacity);
            SetPedHeadOverlayColor(pedHandle, 10, 1, appData.chestHairColor, appData.chestHairColor);
            // body blemishes 
            SetPedHeadOverlay(pedHandle, 11, appData.bodyBlemishesStyle, appData.bodyBlemishesOpacity);
            // eyecolor
            SetPedEyeColor(pedHandle, appData.eyeColor);
            #endregion

            #region Face Shape Data
            for (var i = 0; i < 19; i++)
            {
                SetPedFaceFeature(pedHandle, i, 0f);
            }

            if (character.FaceShapeFeatures.features != null)
            {
                foreach (var t in character.FaceShapeFeatures.features)
                {
                    SetPedFaceFeature(pedHandle, t.Key, t.Value);
                }
            }
            else
            {
                character.FaceShapeFeatures.features = new Dictionary<int, float>();
            }

            #endregion

            #region Clothing Data
            if (character.DrawableVariations.clothes != null && character.DrawableVariations.clothes.Count > 0)
            {
                foreach (var cd in character.DrawableVariations.clothes)
                {
                    SetPedComponentVariation(pedHandle, cd.Key, cd.Value.Key, cd.Value.Value, 0);
                }
            }
            #endregion

            #region Props Data
            if (character.PropVariations.props != null && character.PropVariations.props.Count > 0)
            {
                foreach (var cd in character.PropVariations.props)
                {
                    if (cd.Value.Key > -1)
                    {
                        int textureIndex = cd.Value.Value > -1 ? cd.Value.Value : 0;
                        SetPedPropIndex(pedHandle, cd.Key, cd.Value.Key, textureIndex, true);
                    }
                }
            }
            #endregion

            #region Tattoos

            character.PedTatttoos.HairTattoos ??= [];
            character.PedTatttoos.HeadTattoos ??= [];
            character.PedTatttoos.TorsoTattoos ??= [];
            character.PedTatttoos.LeftArmTattoos ??= [];
            character.PedTatttoos.RightArmTattoos ??= [];
            character.PedTatttoos.LeftLegTattoos ??= [];
            character.PedTatttoos.RightLegTattoos ??= [];
            character.PedTatttoos.BadgeTattoos ??= [];
            character.PedTatttoos.AddonTattoos ??= [];

            IEnumerable<KeyValuePair<string, string>> allTattoos = character.PedTatttoos.HairTattoos
                .Concat(character.PedTatttoos.HeadTattoos)
                .Concat(character.PedTatttoos.TorsoTattoos)
                .Concat(character.PedTatttoos.LeftArmTattoos)
                .Concat(character.PedTatttoos.RightArmTattoos)
                .Concat(character.PedTatttoos.LeftLegTattoos)
                .Concat(character.PedTatttoos.RightLegTattoos)
                .Concat(character.PedTatttoos.BadgeTattoos)
                .Concat(character.PedTatttoos.AddonTattoos);

            foreach (var tattoo in allTattoos)
            {
                AddPedDecorationFromHashes(pedHandle, Game.GenerateHashASCII(tattoo.Key), Game.GenerateHashASCII(tattoo.Value));
            }
            #endregion
        }

        public struct MpCharacterCategory
        {
            public string Name;
            public string Description;
            public MenuItem.Icon Icon;
        }
    }
}
