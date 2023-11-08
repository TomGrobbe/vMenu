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

namespace vMenu.Client.MenuSettings
{
    public class menuSettings : BaseScript
    {
        public static int maxItemsOnScreen = 9;
        public static float fadingTime = 0.01f;
        public static bool AlternativeTitle = true;
        public static bool Glare = false;
        public static bool enabled3DAnimations = false;
        public static bool mouseControlsEnabled = false;
        public static bool controlDisablingEnabled = false;
        public static bool enableAnimation = false;
        public static MenuBuildingAnimation buildingAnimation = MenuBuildingAnimation.NONE;
        public static ScrollingType scrollingType = ScrollingType.ENDLESS;
        public static SColor HighlightColor = SColor.FromArgb(255, 236, 236, 0) ;
        public static SColor BackgroundColor = SColor.HUD_Panel_light;
    }
}