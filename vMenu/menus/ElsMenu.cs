using CitizenFX.Core;
using MenuAPI;
using System.Collections.Generic;
using static vMenuClient.CommonFunctions;

namespace vMenuClient
{
    public class ElsMenu
    {
        // Variables
        public Menu Menu { get; private set; }
        public float VoxtyAirhornClickVolume { get; private set; } = Settings.VoxtyAirhornClickVolume;
        public float VoxtyLightingClickVolume { get; private set; } = Settings.VoxtyLightingClickVolume;
        public float VoxtySirenClickVolume { get; private set; } = Settings.VoxtySirenClickVolume;
        public bool VoxtyFiveSirenSystem { get; private set; } = Settings.VoxtyFiveSirenSystem;

        /// <summary>
        /// Creates the Menu.
        /// </summary>
        private void CreateMenu()
        {
            SetAirHornClickVolume(VoxtyAirhornClickVolume);
            SetLightingClickVolume(VoxtyLightingClickVolume);
            SetSirenClickVolume(VoxtySirenClickVolume);
            SetFiveSirenSystem(VoxtyFiveSirenSystem);
            this.Menu = new Menu(Game.Player.Name, "Server Options");
     
            // Create the Menu.
            Menu menu1 = new Menu(Game.Player.Name, "ELS Menu");

            // Create the Menu items.
            List<float> volumeFloats = new List<float> { -1, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };
            List<string> volumeLevels = new List<string> { "Disabled" };
            for (int i = 1; i < volumeFloats.Count; i++)
            {
                volumeLevels.Add($"{volumeFloats[i] * 100}%");
            }

            MenuListItem airHornClickVolume = new MenuListItem("Air Horn Click Volume", volumeLevels, volumeFloats.FindIndex(f => f == VoxtyAirhornClickVolume), "Modifies the volume of the air horn click.");
            MenuListItem lightingClickVolume = new MenuListItem("Lighting Click Volume", volumeLevels, volumeFloats.FindIndex(f => f == VoxtyLightingClickVolume), "Modifies the volume of the lighting cycle click.");
            MenuListItem sirenClickVolume = new MenuListItem("Siren Click Volume", volumeLevels, volumeFloats.FindIndex(f => f == VoxtySirenClickVolume), "Modifies the volume of the siren click.");
            MenuCheckboxItem fiveSirenSystem = new MenuCheckboxItem("Toggle Server 5SS™", "Toggle whether the Server Five Siren System™ is enabled or not.", VoxtyFiveSirenSystem);

            Menu.AddMenuItem(airHornClickVolume);
            Menu.AddMenuItem(lightingClickVolume);
            Menu.AddMenuItem(sirenClickVolume);
            Menu.AddMenuItem(fiveSirenSystem);

            // Handle list item changes.
            Menu.OnListIndexChange += (MenuAPI.Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex) =>
            {
                if (listItem == airHornClickVolume)
                {
                    TriggerEvent("siren:volume:airHornClickVolume", volumeFloats[newSelectionIndex]);
                    Settings.VoxtyAirhornClickVolume = volumeFloats[newSelectionIndex];
                }
                else if (listItem == lightingClickVolume)
                {
                    TriggerEvent("siren:volume:lightingClickVolume", volumeFloats[newSelectionIndex]);
                    Settings.VoxtyLightingClickVolume = volumeFloats[newSelectionIndex];
                }
                else if (listItem == sirenClickVolume)
                {
                    TriggerEvent("siren:volume:sirenClickVolume", volumeFloats[newSelectionIndex]);
                    Settings.VoxtySirenClickVolume = volumeFloats[newSelectionIndex];
                }
            };

            Menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == fiveSirenSystem)
                {
                    SetFiveSirenSystem(_checked);
                    Settings.VoxtyFiveSirenSystem = _checked;
                }
            };
        }

        private void SetAirHornClickVolume(float volume)
        {
            if (volume < 0)
            {
                volume = 0;
            }

            TriggerEvent("siren:volume:airHornClickVolume", volume);
        }

        private void SetLightingClickVolume(float volume)
        {
            if (volume < 0)
            {
                volume = 0;
            }

            TriggerEvent("siren:volume:lightingClickVolume", volume);
        }

        private void SetSirenClickVolume(float volume)
        {
            if (volume < 0)
            {
                volume = 0;
            }

            TriggerEvent("siren:volume:sirenClickVolume", volume);
        }

        private void SetFiveSirenSystem(bool toggle) => TriggerEvent("Voxty:Siren:Toggle5SS", toggle);

        /// <summary>
        /// Create the Menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (Menu == null)
            {
                CreateMenu();
            }
            return Menu;
        }
    }
}