using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using vMenuShared;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class WeatherOptions
    {
        // Variables
        private Menu menu;
        public MenuCheckboxItem dynamicWeatherEnabled;
        public MenuCheckboxItem blackout;
        public MenuCheckboxItem vehicleBlackout;
        public MenuCheckboxItem snowEnabled;
        public static readonly List<string> weatherTypes = new()
        {
            "EXTRASUNNY",
            "CLEAR",
            "NEUTRAL",
            "SMOG",
            "FOGGY",
            "CLOUDS",
            "OVERCAST",
            "CLEARING",
            "RAIN",
            "THUNDER",
            "BLIZZARD",
            "SNOW",
            "SNOWLIGHT",
            "XMAS",
            "HALLOWEEN"
        };
        
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "游戏天气选项");

            dynamicWeatherEnabled = new MenuCheckboxItem("切换动态天气", "启用或禁用动态天气变化.", EventManager.DynamicWeatherEnabled);
            blackout = new MenuCheckboxItem("停电模式", "这将禁用或启用全地图的灯光.", EventManager.IsBlackoutEnabled);
            vehicleBlackout = new MenuCheckboxItem("切换载具灯光熄灭", "禁用或启用地图中所有载具的灯光。", !EventManager.IsVehicleLightsEnabled);
            snowEnabled = new MenuCheckboxItem("启用风雪效果", "这将强制地面上出现雪，并为行人和车辆启用雪粒子效果.最好与X-MAS或轻微雪天气结合使用.", ConfigManager.GetSettingsBool(ConfigManager.Setting.vmenu_enable_snow));
            
            var extrasunny = new MenuItem("极度晴朗", "将天气设置为 ~y~极度晴朗~s~!") { ItemData = "EXTRASUNNY" };
            var clear = new MenuItem("晴朗", "将天气设置为 ~y~晴朗~s~!") { ItemData = "CLEAR" };
            var neutral = new MenuItem("中性", "将天气设置为 ~y~中性~s~!") { ItemData = "NEUTRAL" };
            var smog = new MenuItem("烟雾", "将天气设置为 ~y~烟雾~s~!") { ItemData = "SMOG" };
            var foggy = new MenuItem("多雾", "将天气设置为 ~y~多雾~s~!") { ItemData = "FOGGY" };
            var clouds = new MenuItem("多云", "将天气设置为 ~y~多云~s~!") { ItemData = "CLOUDS" };
            var overcast = new MenuItem("阴云", "将天气设置为 ~y~阴云~s~!") { ItemData = "OVERCAST" };
            var clearing = new MenuItem("放晴", "将天气设置为 ~y~放晴~s~!") { ItemData = "CLEARING" };
            var rain = new MenuItem("下雨", "将天气设置为 ~y~下雨~s~!") { ItemData = "RAIN" };
            var thunder = new MenuItem("雷暴", "将天气设置为 ~y~雷暴~s~!") { ItemData = "THUNDER" };
            var blizzard = new MenuItem("暴风雪", "将天气设置为 ~y~暴风雪~s~!") { ItemData = "BLIZZARD" };
            var snow = new MenuItem("雪", "将天气设置为 ~y~雪~s~!") { ItemData = "SNOW" };
            var snowlight = new MenuItem("轻微雪", "将天气设置为 ~y~轻微雪~s~!") { ItemData = "SNOWLIGHT" };
            var xmas = new MenuItem("圣诞雪", "将天气设置为 ~y~圣诞~s~!") { ItemData = "XMAS" };
            var halloween = new MenuItem("万圣节", "将天气设置为 ~y~万圣节~s~!") { ItemData = "HALLOWEEN" };
            var removeclouds = new MenuItem("移除云层", "从天空中移除所有云层!");
            var randomizeclouds = new MenuItem("随机云层", "在天空中添加随机云层!");

            if (IsAllowed(Permission.WODynamic))
            {
                menu.AddMenuItem(dynamicWeatherEnabled);
            }
            if (IsAllowed(Permission.WOBlackout))
            {
                menu.AddMenuItem(blackout);
            }
            if (IsAllowed(Permission.WOVehBlackout))
            {
                menu.AddMenuItem(vehicleBlackout);
            }
            if (IsAllowed(Permission.WOSetWeather))
            {
                menu.AddMenuItem(snowEnabled);
                menu.AddMenuItem(extrasunny);
                menu.AddMenuItem(clear);
                menu.AddMenuItem(neutral);
                menu.AddMenuItem(smog);
                menu.AddMenuItem(foggy);
                menu.AddMenuItem(clouds);
                menu.AddMenuItem(overcast);
                menu.AddMenuItem(clearing);
                menu.AddMenuItem(rain);
                menu.AddMenuItem(thunder);
                menu.AddMenuItem(blizzard);
                menu.AddMenuItem(snow);
                menu.AddMenuItem(snowlight);
                menu.AddMenuItem(xmas);
                menu.AddMenuItem(halloween);
            }
            if (IsAllowed(Permission.WORandomizeClouds))
            {
                menu.AddMenuItem(randomizeclouds);
            }

            if (IsAllowed(Permission.WORemoveClouds))
            {
                menu.AddMenuItem(removeclouds);
            }

            menu.OnItemSelect += (sender, item, index2) =>
            {
                if (item == removeclouds)
                {
                    ModifyClouds(true);
                }
                else if (item == randomizeclouds)
                {
                    ModifyClouds(false);
                }
                else if (item.ItemData is string weatherType)
                {
                    Notify.Custom($"天气将更改为 ~y~{item.Text}~s~.尚需耐心等待 {EventManager.WeatherChangeTime} 秒完成更新.");
                    UpdateServerWeather(weatherType, EventManager.DynamicWeatherEnabled, EventManager.IsSnowEnabled);
                }
            };

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == dynamicWeatherEnabled)
                {
                    Notify.Custom($"Dynamic weather changes are now {(_checked ? "~g~enabled" : "~r~disabled")}~s~.");
                    UpdateServerWeather(EventManager.GetServerWeather, _checked, EventManager.IsSnowEnabled);
                }
                else if (item == blackout)
                {
                    Notify.Custom($"Blackout mode is now {(_checked ? "~g~enabled" : "~r~disabled")}~s~.");
                    UpdateServerBlackout(_checked);
                }
                else if (item == vehicleBlackout)
                {
                    Notify.Custom($"Vehicle light blackout mode is now {(_checked ? "~g~enabled" : "~r~disabled")}~s~.");
                    UpdateServerVehicleBlackout(!_checked);
                }
                else if (item == snowEnabled)
                {
                    if (EventManager.GetServerWeather is "XMAS" or "SNOWLIGHT" or "SNOW" or "BLIZZARD")
                    {
                        Notify.Custom($"Snow effects cannot be disabled when weather is ~y~{EventManager.GetServerWeather}~s~.");
                        return;
                    }

                    Notify.Custom($"Snow effects will now be forced {(_checked ? "~g~enabled" : "~r~disabled")}~s~.");
                    UpdateServerWeather(EventManager.GetServerWeather, EventManager.DynamicWeatherEnabled, _checked);
                }
            };
        }



        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
