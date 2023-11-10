using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using CitizenFX.Core;

using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;

namespace vMenu.Client.Settings
{
    public class MenuSettings : BaseScript
    {
        public struct Theme
        {
            public string MenuTheme;
            public ARGBTheme SpacerHighlightARGB;
            public ARGBTheme ItemHighlightARGB;
            public ARGBTheme SpacerBackgroundARGB;
            public ARGBTheme ItemHighlightedTextARGB;
            public ARGBTheme ItemTextARGB;
            public ARGBTheme SpacerHighlightedTextARGB;
            public ARGBTheme SpacerTextARGB;
            public ARGBTheme ItemBackgroundARGB;
            public Theme(string MenuTheme, ARGBTheme SpacerHighlightARGB, ARGBTheme SpacerBackgroundARGB, ARGBTheme ItemHighlightARGB, ARGBTheme ItemBackgroundARGB, ARGBTheme ItemHighlightedTextARGB, ARGBTheme ItemTextARGB, ARGBTheme SpacerHighlightedTextARGB, ARGBTheme SpacerTextARGB)
            {
                this.MenuTheme = MenuTheme;
                this.SpacerHighlightARGB = SpacerHighlightARGB;
                this.SpacerHighlightedTextARGB = SpacerHighlightedTextARGB;
                this.SpacerTextARGB = SpacerTextARGB;
                this.SpacerBackgroundARGB = SpacerBackgroundARGB;
                this.ItemHighlightARGB = ItemHighlightARGB;
                this.ItemHighlightedTextARGB = ItemHighlightedTextARGB;
                this.ItemTextARGB = ItemTextARGB;
                this.ItemBackgroundARGB = ItemBackgroundARGB;


            }
        }
        public struct ARGBTheme
        {
            public int alpha;
            public int red;
            public int blue;
            public int green;

            public ARGBTheme(int alpha, int red, int blue, int green)
            {
                this.alpha = alpha;
                this.red = red;
                this.green = green;
                this.blue = blue;
            }
        }
        public static int MaxItemsOnScreen = 9;
        public static float FadingTime = 0.001f;
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
        private static string JsonData = LoadResourceFile(GetCurrentResourceName(), "Theme.json") ?? "{}";
        private static Theme JsonTheme = JsonConvert.DeserializeObject<Theme>(JsonData);
        private static string Themes = JsonTheme.MenuTheme;

        public MenuSettings()
        {
            if (Themes == "NativeUI")
            {
                // spacers
                Colours.Spacers.HighlightColor = SColor.FromArgb(255, 255, 255, 255);
                Colours.Spacers.BackgroundColor = SColor.FromArgb(180, 36, 36, 36);
                Colours.Spacers.HighlightedTextColor = SColor.FromArgb(255, 0, 0, 0);
                Colours.Spacers.TextColor = SColor.FromArgb(255, 255, 255, 255);


                // items
                Colours.Items.HighlightColor = SColor.FromArgb(255, 255, 255, 255);
                Colours.Items.BackgroundColor = SColor.FromArgb(180, 36, 36, 36);
                Colours.Items.HighlightedTextColor = SColor.FromArgb(255, 0, 0, 0);
                Colours.Items.TextColor = SColor.FromArgb(255, 255, 255, 255);
            }
            else if (Themes == "Revamp")
            {
                // spacers
                Colours.Spacers.HighlightColor = SColor.FromArgb(255, 236, 197, 79);
                Colours.Spacers.BackgroundColor = SColor.FromArgb(255, 24, 44, 74);
                Colours.Spacers.HighlightedTextColor = SColor.FromArgb(255, 0, 0, 0);
                Colours.Spacers.TextColor = SColor.FromArgb(255, 255, 255, 255);


                // items
                Colours.Items.HighlightColor = SColor.FromArgb(255, 236, 197, 79);
                Colours.Items.BackgroundColor = SColor.FromArgb(255, 24, 44, 74);
                Colours.Items.HighlightedTextColor = SColor.FromArgb(255, 0, 0, 0);
                Colours.Items.TextColor = SColor.FromArgb(255, 255, 255, 255);
            }
            else if (Themes == "Custom")
            {
                Colours.Spacers.HighlightColor = SColor.FromArgb(JsonTheme.SpacerHighlightARGB.alpha, JsonTheme.SpacerHighlightARGB.red, JsonTheme.SpacerHighlightARGB.green, JsonTheme.SpacerHighlightARGB.blue);
                Colours.Spacers.BackgroundColor = SColor.FromArgb(JsonTheme.SpacerBackgroundARGB.alpha, JsonTheme.SpacerBackgroundARGB.red, JsonTheme.SpacerBackgroundARGB.green, JsonTheme.SpacerBackgroundARGB.blue); 
                Colours.Spacers.HighlightedTextColor = SColor.FromArgb(JsonTheme.SpacerHighlightedTextARGB.alpha, JsonTheme.SpacerHighlightedTextARGB.red, JsonTheme.SpacerHighlightedTextARGB.green, JsonTheme.SpacerHighlightedTextARGB.blue); 
                Colours.Spacers.TextColor = SColor.FromArgb(JsonTheme.SpacerTextARGB.alpha, JsonTheme.SpacerTextARGB.red, JsonTheme.SpacerTextARGB.green, JsonTheme.SpacerTextARGB.blue); 
                // items
                Colours.Items.HighlightColor = SColor.FromArgb(JsonTheme.ItemHighlightARGB.alpha, JsonTheme.ItemHighlightARGB.red, JsonTheme.ItemHighlightARGB.green, JsonTheme.ItemHighlightARGB.blue); 
                Colours.Items.BackgroundColor = SColor.FromArgb(JsonTheme.ItemBackgroundARGB.alpha, JsonTheme.ItemBackgroundARGB.red, JsonTheme.ItemBackgroundARGB.green, JsonTheme.ItemBackgroundARGB.blue); 
                Colours.Items.HighlightedTextColor = SColor.FromArgb(JsonTheme.ItemHighlightedTextARGB.alpha, JsonTheme.ItemHighlightedTextARGB.red, JsonTheme.ItemHighlightedTextARGB.green, JsonTheme.ItemHighlightedTextARGB.blue); 
                Colours.Items.TextColor = SColor.FromArgb(JsonTheme.ItemTextARGB.alpha, JsonTheme.ItemTextARGB.red, JsonTheme.ItemTextARGB.green, JsonTheme.ItemTextARGB.blue); 
                
            }
        }
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
