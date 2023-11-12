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

using static CitizenFX.Core.Native.API;

using vMenu.Client.Functions;
using Newtonsoft.Json;

namespace vMenu.Client.Settings
{
    public class MenuSettings
    {
        private static readonly object _padlock = new();
        private static MenuSettings _instance;

        public struct MenuSettingJson
        {
            public int MaxItemsOnScreen;
            public float FadingTime;
            public bool AlternativeTitle;
            public bool Glare;
            public bool Enabled3DAnimations;
            public bool MouseControlsEnabled;
            public bool MouseEdgeEnabled;
            public bool MouseWheelControlEnabled;
            public bool ControlDisablingEnabled;
            public bool EnableAnimation;
            public string BannerTitle;
            public string TextureUrl;
            public string TextureDictionary;
            public string TextureName; 

            public MenuSettingJson(int MaxItemsOnScreen, float FadingTime, bool AlternativeTitle, bool Glare, bool Enabled3DAnimations, bool MouseControlsEnabled, bool MouseEdgeEnabled, bool MouseWheelControlEnabled, bool ControlDisablingEnabled, bool EnableAnimation, string BannerTitle, string TextureUrl, string TextureDictionary, string TextureName)
            {
                this.MaxItemsOnScreen = MaxItemsOnScreen;
                this.FadingTime = FadingTime;
                this.AlternativeTitle = AlternativeTitle;
                this.Glare = Glare;
                this.Enabled3DAnimations = Enabled3DAnimations;
                this.MouseControlsEnabled = MouseControlsEnabled;
                this.MouseEdgeEnabled = MouseEdgeEnabled;
                this.MouseWheelControlEnabled = MouseWheelControlEnabled;
                this.ControlDisablingEnabled = ControlDisablingEnabled;
                this.EnableAnimation = EnableAnimation;
                this.BannerTitle = BannerTitle;
                this.TextureUrl = TextureUrl;
                this.TextureDictionary = TextureDictionary;
                this.TextureName = TextureName;
            }
        }



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

        private static string Themes;

        private static string JsonSettingsData = LoadResourceFile(GetCurrentResourceName(), "MenuSettings.jsonc") ?? "{}";
        private static vMenu.Client.Settings.MenuSettings.MenuSettingJson JsonSettings = JsonConvert.DeserializeObject<vMenu.Client.Settings.MenuSettings.MenuSettingJson>(JsonSettingsData);

        public static int MaxItemsOnScreen = JsonSettings.MaxItemsOnScreen;
        public static float FadingTime = JsonSettings.FadingTime;
        public static bool AlternativeTitle = JsonSettings.AlternativeTitle;
        public static bool Glare = JsonSettings.Glare;
        public static bool Enabled3DAnimations = JsonSettings.Enabled3DAnimations;
        public static bool MouseControlsEnabled = JsonSettings.MouseControlsEnabled;
        public static bool MouseEdgeEnabled = JsonSettings.MouseEdgeEnabled;
        public static bool MouseWheelControlEnabled = JsonSettings.MouseWheelControlEnabled;
        public static bool ControlDisablingEnabled = JsonSettings.ControlDisablingEnabled;
        public static bool EnableAnimation = JsonSettings.EnableAnimation;
        public static MenuBuildingAnimation BuildingAnimation = MenuBuildingAnimation.NONE;
        public static ScrollingType ScrollingType = ScrollingType.ENDLESS;

        public MenuSettings()
        {
            string JsonData = LoadResourceFile(GetCurrentResourceName(), "Themes.jsonc") ?? "{}";

            Theme JsonTheme = JsonConvert.DeserializeObject<Theme>(JsonData);

            Themes = JsonTheme.MenuTheme;

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

            Debug.WriteLine("MenuSettings Initialized");
        }

        internal static MenuSettings Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new MenuSettings();
                }
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
