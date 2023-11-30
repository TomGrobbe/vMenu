using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Objects;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus
{
    public class PlayerOptionsMenu
    {
        private static UIMenu playerRelatedOptions = null;

        public static bool NoClipEnabled { get { return NoClip.IsNoclipActive(); } set { NoClip.SetNoclipActive(value); } }

        public PlayerOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["PlayerOptionsMenu"];

            playerRelatedOptions = new Objects.vMenu(MenuLanguage.Subtitle ?? "Player Options").Create();

            UIMenuItem WeaponOptionsButton = new vMenuItem(MenuLanguage.Items["WeaponOptionsItem"], "Weapon Options", "Spawn weapons, refill ammo and more through this menu!").Create();
            WeaponOptionsButton.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            WeaponOptionsButton.SetRightLabel(">>>");

            UIMenuItem NoClip = new vMenuItem(MenuLanguage.Items["NoClipItem"], "NoClip Toggle", "Toggle to fly around in NoClip mode.").Create();
            NoClip.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            playerRelatedOptions.AddItem(NoClip);
            playerRelatedOptions.AddItem(WeaponOptionsButton);

            NoClip.Activated += (sender, i) =>
            {
                NoClipEnabled = !NoClipEnabled;

                if (!NoClipEnabled)
                {
                    playerRelatedOptions.Visible = false;
                    playerRelatedOptions.Visible = true;
                }
            };

            WeaponOptionsButton.Activated += (sender, i) =>
            {
                sender.SwitchTo(PlayerSubmenus.WeaponOptionsMenu.Menu(), inheritOldMenuParams: true);
            };

            Main.Menus.Add(playerRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return playerRelatedOptions;
        }
    }
}