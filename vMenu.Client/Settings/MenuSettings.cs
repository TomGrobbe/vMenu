using CitizenFX.Core;
using ScaleformUI.Menu;
using ScaleformUI.Elements;
using ScaleformUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMenu.Client.Functions;

namespace vMenu.Client.Settings
{
    public class MenuSettings : BaseScript
    {
        public static int MaxItemsOnScreen = 9;
        public static float FadingTime = 0.01f;
        public static bool AlternativeTitle = true;
        public static bool Glare = false;
        public static bool Enabled3DAnimations = false;
        public static bool MouseControlsEnabled = false;
        public static bool MouseEdgeEnabled = false;
        public static bool MouseWheelControlEnabled = true;
        public static bool ControlDisablingEnabled = false;
        public static bool EnableAnimation = false;
        public static MenuBuildingAnimation BuildingAnimation = MenuBuildingAnimation.NONE;
        public static ScrollingType ScrollingType = ScrollingType.ENDLESS;
        public static SColor HighlightColor = SColor.FromArgb(255, 236, 236, 0) ;
        public static SColor BackgroundColor = SColor.HUD_Panel_light;
    }
}