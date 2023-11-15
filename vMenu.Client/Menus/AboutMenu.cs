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
using ScaleformUI.Menus;

using vMenu.Client.Functions;
using vMenu.Client.Objects;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus
{
    public class AboutMenu
    {
        private static UIMenu aboutMenu = null;

        public AboutMenu()
        {
            var MenuLanguage = Languages.Menus["AboutMenu"];

            aboutMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "About vMenu").Create();

            UIMenuItem vMenuVersion = new UIMenuItem(MenuLanguage.Items["VMenuVersionItem"].Name ?? "vMenu Version", MenuLanguage.Items["VMenuVersionItem"].Description ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            vMenuVersion.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            vMenuVersion.SetRightLabel($"~h~v{MenuFunctions.Version}~h~");
            UIMenuItem vMenuCredits = new UIMenuItem(MenuLanguage.Items["VMenuCreditsItem"].Name ?? "vMenu Credits", MenuLanguage.Items["VMenuCreditsItem"].Description ?? "vMenu Revamped was created by XdGoldenTiger, Ricky, Katt, and the FiveM community", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            vMenuCredits.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            UIMenuItem vMenuDevBoard = new UIMenuItem(MenuLanguage.Items["VMenuDevBoardItem"].Name ?? "Dev Board", MenuLanguage.Items["VMenuDevBoardItem"].Description ?? "https://trello.com/b/HpQdFX9J/", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            vMenuDevBoard.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            UIMenuItem vMenuFounder = new UIMenuItem(MenuLanguage.Items["VMenuFounderItem"].Name ?? "vMenu Founder", MenuLanguage.Items["VMenuFounderItem"].Description ?? "https://vespura.com/", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            vMenuFounder.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            aboutMenu.AddItem(vMenuVersion);
            aboutMenu.AddItem(vMenuCredits);
            aboutMenu.AddItem(vMenuDevBoard);
            aboutMenu.AddItem(vMenuFounder);

            Main.Menus.Add(aboutMenu);
        }

        public static UIMenu Menu()
        {
            return aboutMenu;
        }
    }
}