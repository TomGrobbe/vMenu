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
            playerRelatedOptions = new Objects.vMenu("Player Options").Create();

            UIMenuItem WeaponOptionsButton = new UIMenuItem("Weapon Options", "Spawn weapons, refill ammo and more through this menu!", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            WeaponOptionsButton.SetRightLabel(">>>");

            UIMenuItem NoClip = new UIMenuItem("NoClip Toggle", "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);

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
                sender.SwitchTo(PlayerRelated.WeaponOptions.Menu(), inheritOldMenuParams: true); ;
            };

            Main.Menus.Add(playerRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return playerRelatedOptions;
        }
    }
}