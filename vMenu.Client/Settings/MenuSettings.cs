using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;

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

        public struct Colours
        {
            public struct Items {
                public static SColor HighlightColor = SColor.FromArgb(255, 243, 200, 87);
                public static SColor BackgroundColor = SColor.FromArgb(255, 21, 175, 173);
                public static SColor HighlightedTextColor = SColor.FromArgb(255, 234, 67, 67);
                public static SColor TextColor = SColor.FromArgb(255, 24, 47, 52);
            }

            public struct Spacers
            {
                public static SColor HighlightColor = SColor.FromArgb(255, 238, 52, 92);
                public static SColor BackgroundColor = SColor.FromArgb(255, 238, 52, 92);
                public static SColor HighlightedTextColor = SColor.FromArgb(255, 243, 200, 87);
                public static SColor TextColor = SColor.FromArgb(255, 243, 200, 87);
            }
        }
    }
}