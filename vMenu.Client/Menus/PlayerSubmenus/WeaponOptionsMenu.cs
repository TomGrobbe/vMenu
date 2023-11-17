using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Menus.OnlinePlayersSubmenus;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;

namespace vMenu.Client.Menus.PlayerSubmenus
{
    public class WeaponOptionsMenu
    {
        private static UIMenu weaponOptionsMenu = null;

        // Checkbox items
        public bool UnlimitedAmmo { get; private set; } = false;
        public bool NoReload { get; private set; } = false;
        public bool AutoEquipChute { get; private set; } = false;
        public bool UnlimitedParachutes { get; private set; } = false;

        public WeaponOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["WeaponOptionsMenu"];

            // Creating the menu
            weaponOptionsMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Weapon Options").Create();

            UIMenuSeparatorItem button = new UIMenuSeparatorItem("Under Construction!", false)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };
            button.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            weaponOptionsMenu.AddItem(button);
            // Menu Items

            UIMenuItem getAllWeapons = new UIMenuItem(MenuLanguage.Items["GetAllWeaponsItem"].Name ?? "Get All Weapons", MenuLanguage.Items["GetAllWeaponsItem"].Description ?? "Get all weapons.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            getAllWeapons.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuCheckboxItem unlimitedAmmo = new UIMenuCheckboxItem(MenuLanguage.Items["UnlimitedAmmoItem"].Name ?? "Unlimited Ammo", UnlimitedAmmo, MenuLanguage.Items["UnlimitedAmmoItem"].Description ?? "Unlimited ammunition supply.")
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            unlimitedAmmo.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuCheckboxItem noReload = new UIMenuCheckboxItem(MenuLanguage.Items["NoReloadItem"].Name ?? "No Reload", NoReload, MenuLanguage.Items["NoReloadItem"].Description ?? "Never reload.")
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            noReload.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuItem setAmmo = new UIMenuItem(MenuLanguage.Items["SetAmmoItem"].Name ?? "Set All Ammo Count", MenuLanguage.Items["SetAmmoItem"].Description ?? "Set the amount of ammo in all your weapons.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            setAmmo.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            UIMenuItem refillMaxAmmo = new UIMenuItem(MenuLanguage.Items["RefillMaxAmmoItem"].Name ?? "Refill All Ammo", MenuLanguage.Items["RefillMaxAmmoItem"].Description ?? "Give all your weapons max ammo.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            refillMaxAmmo.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            UIMenuItem spawnByName = new UIMenuItem(MenuLanguage.Items["SpawnByNameItem"].Name ?? "Spawn Weapon By Name", MenuLanguage.Items["SpawnByNameItem"].Description ?? "Enter a weapon mode name to spawn.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            spawnByName.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            weaponOptionsMenu.AddItem(getAllWeapons);
            weaponOptionsMenu.AddItem(noReload);
            weaponOptionsMenu.AddItem(setAmmo);
            weaponOptionsMenu.AddItem(refillMaxAmmo);
            weaponOptionsMenu.AddItem(spawnByName);
            // Add-on Weapons Menu

            UIMenuItem addonWeaponsBtn = new UIMenuItem(MenuLanguage.Items["AddonWeaponsItem"].Name ?? "Addon Weapons", MenuLanguage.Items["AddonWeaponsItem"].Description ?? "Equip / remove addon weapons available on this server.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            addonWeaponsBtn.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            addonWeaponsBtn.SetRightLabel("→→→");

            weaponOptionsMenu.AddItem(addonWeaponsBtn);

            addonWeaponsBtn.Activated += (sender, _) => {
                Notify.Alert("This feature isn't currently finished.");
            };

            // Create Weapon Category Submenus
            UIMenuSeparatorItem spacer = new UIMenuSeparatorItem(MenuLanguage.Items["WeaponCategoryItem"].Name ?? "Weapon Categories", true)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };
            spacer.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            weaponOptionsMenu.AddItem(spacer);

            UIMenuItem handGunsBtn = new UIMenuItem(MenuLanguage.Items["HandGunsItem"].Name ?? "Handguns", MenuLanguage.Items["HandGunsItem"].Description ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            handGunsBtn.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuItem riflesBtn = new UIMenuItem(MenuLanguage.Items["RiflesItem"].Name ?? "Assault Rifles", MenuLanguage.Items["RiflesItem"].Description ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            riflesBtn.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuItem shotgunsBtn = new UIMenuItem(MenuLanguage.Items["ShotgunsItem"].Name ?? "Shotguns", MenuLanguage.Items["ShotgunsItem"].Description ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            shotgunsBtn.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuItem smgsBtn = new UIMenuItem(MenuLanguage.Items["SmgsItem"].Name ?? "Sub-/Light Machine Guns", MenuLanguage.Items["SmgsItem"].Description ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            smgsBtn.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuItem throwablesBtn = new UIMenuItem(MenuLanguage.Items["ThrowablesItem"].Name ?? "Throwables", MenuLanguage.Items["ThrowablesItem"].Description ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            throwablesBtn.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuItem meleeBtn = new UIMenuItem(MenuLanguage.Items["MeleeItem"].Name ?? "Melee", MenuLanguage.Items["MeleeItem"].Description ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            meleeBtn.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuItem heavyBtn = new UIMenuItem(MenuLanguage.Items["HeavyItem"].Name ?? "Heavy Weapons", MenuLanguage.Items["HeavyItem"].Description ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            heavyBtn.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuItem snipersBtn = new UIMenuItem(MenuLanguage.Items["SnipersItem"].Name ?? "Sniper Rifles", MenuLanguage.Items["SnipersItem"].Description ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
            {
                MainColor = MenuSettings.Colours.Items.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Items.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Items.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Items.TextColor
            };
            snipersBtn.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            // Setting up Weapon Category Buttons / Submenus for later configuation.

            handGunsBtn.SetRightLabel("→→→");
            handGunsBtn.Activated += (sender, _) =>
            {
                Notify.Alert("This MenuItem isn't ready for use.");
            };
            weaponOptionsMenu.AddItem(handGunsBtn);

            riflesBtn.SetRightLabel("→→→");
            riflesBtn.Activated += (sender, _) =>
            {
                Notify.Alert("This MenuItem isn't ready for use.");
            };
            weaponOptionsMenu.AddItem(riflesBtn);

            shotgunsBtn.SetRightLabel("→→→");
            shotgunsBtn.Activated += (sender, _) =>
            {
                Notify.Alert("This MenuItem isn't ready for use.");
            };
            weaponOptionsMenu.AddItem(shotgunsBtn);

            smgsBtn.SetRightLabel("→→→");
            smgsBtn.Activated += (sender, _) =>
            {
                Notify.Alert("This MenuItem isn't ready for use.");
            };
            weaponOptionsMenu.AddItem(smgsBtn);

            throwablesBtn.SetRightLabel("→→→");
            throwablesBtn.Activated += (sender, _) =>
            {
                Notify.Alert("This MenuItem isn't ready for use.");
            };
            weaponOptionsMenu.AddItem(throwablesBtn);

            meleeBtn.SetRightLabel("→→→");
            meleeBtn.Activated += (sender, _) =>
            {
                Notify.Alert("This MenuItem isn't ready for use.");
            };
            weaponOptionsMenu.AddItem(meleeBtn);

            heavyBtn.SetRightLabel("→→→");
            heavyBtn.Activated += (sender, _) =>
            {
                Notify.Alert("This MenuItem isn't ready for use.");
            };
            weaponOptionsMenu.AddItem(heavyBtn);

            snipersBtn.SetRightLabel("→→→");
            snipersBtn.Activated += (sender, _) =>
            {
                Notify.Alert("This MenuItem isn't ready for use.");
            };
            weaponOptionsMenu.AddItem(snipersBtn);

            Main.Menus.Add(weaponOptionsMenu);
        }

        public static UIMenu Menu()
        {
            return weaponOptionsMenu;
        }
    }
}
