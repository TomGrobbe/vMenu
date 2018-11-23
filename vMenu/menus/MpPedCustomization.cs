using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    public class MpPedCustomization
    {
        // Variables
        private UIMenu menu;
        private readonly CommonFunctions cf = MainMenu.Cf;
        public UIMenu createMaleMenu = new UIMenu("Create Character", "Create A Male Character", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu createFemaleMenu = new UIMenu("Create Character", "Create A Female Character", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        public UIMenu savedCharactersMenu = new UIMenu("Saved Characters", "Manage Saved MP Characters", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu("vMenu", "About vMenu", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            UIMenuItem createMale = new UIMenuItem("Create Male Character", "Create a new male character.");
            createMale.SetRightLabel("→→→");

            UIMenuItem savedCharacters = new UIMenuItem("Saved Characters", "Spawn, edit or delete your existing saved multiplayer characters.");
            savedCharacters.SetRightLabel("→→→");

            UIMenuItem createFemale = new UIMenuItem("Create Female Character", "Create a new female character.");
            createFemale.SetRightLabel("→→→");

            MainMenu.Mp.Add(createMaleMenu);
            MainMenu.Mp.Add(createFemaleMenu);
            MainMenu.Mp.Add(savedCharactersMenu);

            CreateMaleCreatorMenu();
            CreateFemaleCreatorMenu();
            CreateSavedPedsMenu();

            menu.AddItem(createMale);
            menu.BindMenuToItem(createMaleMenu, createMale);

            menu.AddItem(createFemale);
            menu.BindMenuToItem(createFemaleMenu, createFemale);

            menu.AddItem(savedCharacters);
            menu.BindMenuToItem(savedCharactersMenu, savedCharacters);

            menu.UpdateScaleform();

            menu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            createMaleMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));
            createFemaleMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Head"));

            createMaleMenu.UpdateScaleform();
            createFemaleMenu.UpdateScaleform();
            savedCharactersMenu.UpdateScaleform();
        }

        /// <summary>
        /// Creates the saved mp characters menu.
        /// </summary>
        private void CreateSavedPedsMenu()
        {
        }

        /// <summary>
        /// Creates the female creator menu.
        /// </summary>
        private void CreateFemaleCreatorMenu()
        {
        }

        /// <summary>
        /// Creates the male creator menu.
        /// </summary>
        private void CreateMaleCreatorMenu()
        {
        }


        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }



    }
}
