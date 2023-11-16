using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Objects;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus
{
    public class MiscOptionsMenu
    {
        private static UIMenu miscOptionsMenu = null;

        public MiscOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["MiscOptionsMenu"];

            miscOptionsMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Miscellaneous Options").Create();

            UIMenuItem toggleMenuAlign = new vMenuItem(MenuLanguage.Items["ToggleMenuAlignItem"], "Toggle Menu Align", "Change the Menu Alignment (Left | Right)").Create();
            toggleMenuAlign.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            toggleMenuAlign.SetRightLabel(Main.MenuAlign.ToString());
            toggleMenuAlign.RightLabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuListItem changeLanguageSlider = new UIMenuListItem(MenuLanguage.Items["ChangeLanguageSliderItem"].Name ?? "Change Menu Language", LanguagesList.List, 0, MenuLanguage.Items["ChangeLanguageSliderItem"].Description ?? "Change the Menu Language", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            changeLanguageSlider.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            changeLanguageSlider.RightLabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            miscOptionsMenu.AddItem(toggleMenuAlign);
            miscOptionsMenu.AddItem(changeLanguageSlider);

            toggleMenuAlign.Activated += (sender, i) =>
            {
                if (Main.MenuAlign == Shared.Enums.MenuAlign.Left)
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Right;
                    MenuFunctions.Instance.RestartMenu();
                }
                else
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Left;
                    MenuFunctions.Instance.RestartMenu();
                }
            };

            changeLanguageSlider.OnListSelected += (sender, i) =>
            {
                Debug.WriteLine(sender.Items[i].ToString());
            };

            Main.Menus.Add(miscOptionsMenu);
        }

        public static UIMenu Menu()
        {
            return miscOptionsMenu;
        }
    }
}
