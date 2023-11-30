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

            UIMenuItem vMenuVersion = new vMenuItem(MenuLanguage.Items["VMenuVersionItem"], "vMenu Version", "").Create();
            vMenuVersion.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            vMenuVersion.SetRightLabel($"~h~v{MenuFunctions.Version}~h~");
            UIMenuItem vMenuCredits = new vMenuItem(MenuLanguage.Items["VMenuCreditsItem"], "vMenu Credits", "vMenu Revamped was created by XdGoldenTiger, Ricky, Katt, Christopher, and the FiveM community").Create();
            vMenuCredits.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            UIMenuItem vMenuDevBoard = new vMenuItem(MenuLanguage.Items["VMenuDevBoardItem"], "Dev Board", "https://trello.com/b/HpQdFX9J/").Create();
            vMenuDevBoard.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            UIMenuItem vMenuFounder = new vMenuItem(MenuLanguage.Items["VMenuFounderItem"], "vMenu Founder", "https://vespura.com/").Create();
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