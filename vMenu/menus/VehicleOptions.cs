using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class VehicleOptions
    {
        #region Variables
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;
        public static Dictionary<uint, Dictionary<int, string>> VehicleExtras;

        // Submenus
        public Menu VehicleModMenu { get; private set; }
        public Menu VehicleDoorsMenu { get; private set; }
        public Menu VehicleWindowsMenu { get; private set; }
        public Menu VehicleComponentsMenu { get; private set; }
        public Menu VehicleLiveriesMenu { get; private set; }
        public Menu VehicleColorsMenu { get; private set; }
        public Menu DeleteConfirmMenu { get; private set; }
        public Menu VehicleUnderglowMenu { get; private set; }

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = UserDefaults.VehicleGodMode;
        public bool VehicleGodInvincible { get; private set; } = UserDefaults.VehicleGodInvincible;
        public bool VehicleGodEngine { get; private set; } = UserDefaults.VehicleGodEngine;
        public bool VehicleGodVisual { get; private set; } = UserDefaults.VehicleGodVisual;
        public bool VehicleGodStrongWheels { get; private set; } = UserDefaults.VehicleGodStrongWheels;
        public bool VehicleGodRamp { get; private set; } = UserDefaults.VehicleGodRamp;
        public bool VehicleGodAutoRepair { get; private set; } = UserDefaults.VehicleGodAutoRepair;

        public bool VehicleNeverDirty { get; private set; } = UserDefaults.VehicleNeverDirty;
        public bool VehicleEngineAlwaysOn { get; private set; } = UserDefaults.VehicleEngineAlwaysOn;
        public bool VehicleNoSiren { get; private set; } = UserDefaults.VehicleNoSiren;
        public bool VehicleNoBikeHelemet { get; private set; } = UserDefaults.VehicleNoBikeHelmet;
        public bool FlashHighbeamsOnHonk { get; private set; } = UserDefaults.VehicleHighbeamsOnHonk;
        public bool DisablePlaneTurbulence { get; private set; } = UserDefaults.VehicleDisablePlaneTurbulence;
        public bool DisableHelicopterTurbulence { get; private set; } = UserDefaults.VehicleDisableHelicopterTurbulence;
        public bool AnchorBoat { get; private set; } = UserDefaults.VehicleAnchorBoat;
        public bool VehicleBikeSeatbelt { get; private set; } = UserDefaults.VehicleBikeSeatbelt;
        public bool VehicleInfiniteFuel { get; private set; } = false;
        public bool VehicleShowHealth { get; private set; } = false;
        public bool VehicleRadioOverride { get; private set; } = UserDefaults.VehicleDefaultRadio >= 0;
        public bool VehicleFrozen { get; private set; } = false;
        public bool VehicleTorqueMultiplier { get; private set; } = false;
        public bool VehiclePowerMultiplier { get; private set; } = false;
        public float VehicleTorqueMultiplierAmount { get; private set; } = 2f;
        public float VehiclePowerMultiplierAmount { get; private set; } = 2f;

        private readonly Dictionary<MenuItem, int> vehicleExtras = new();
        #endregion

        #region CreateMenu()
        /// <summary>
        /// Create menu creates the vehicle options menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "载具数据编辑");

            #region menu items variables
            // vehicle god mode menu
            var vehGodMenu = new Menu("无敌模式选项", "载具无敌模式选项");
            var vehGodMenuBtn = new MenuItem("无敌模式选项", "启用或禁用特定载具伤害类型.") { Label = "→→→" };
            MenuController.AddSubmenu(menu, vehGodMenu);

            // Create Checkboxes.
            var vehicleGod = new MenuCheckboxItem("载具无敌模式", "使载具不受某种伤害类型破坏. 注意,您需要进入下面的'无敌模式选项',选择要禁用的伤害类型.", VehicleGodMode);
            var vehicleNeverDirty = new MenuCheckboxItem("载具永不脏污", "如载具污垢水平超过0, 它将持续清洁您的载具. 请注意, 这只能清洁~o~灰尘~s~或~o~ 垢~s~. 无法清洁类似于泥、雪或其他~r~损坏~s~, 修理载具以清除它们.", VehicleNeverDirty);
            var vehicleBikeSeatbelt = new MenuCheckboxItem("单车安全带", "防止您从自行车、脚踏车、全地形车或类似装置被撞下.", VehicleBikeSeatbelt);
            var vehicleEngineAO = new MenuCheckboxItem("载具引擎常开", "当您离开载具时, 载具发动机仍能保持开启状态.", VehicleEngineAlwaysOn);
            var vehicleNoTurbulence = new MenuCheckboxItem("禁用飞机乱流", "禁用所有飞机的乱流功能.", DisablePlaneTurbulence);
            var vehicleNoTurbulenceHeli = new MenuCheckboxItem("禁用直升机乱流", "禁用所有直升机的乱流功能.", DisableHelicopterTurbulence);
            var vehicleSetAnchor = new MenuCheckboxItem("抛锚", "仅在当前载具为船只且位置允许抛锚时生效。", AnchorBoat);
            var vehicleNoSiren = new MenuCheckboxItem("禁用警笛", "禁用载具的警报器. 仅当您的载具确实有警报器时才有效.", VehicleNoSiren);
            var vehicleNoBikeHelmet = new MenuCheckboxItem("无单车头盔", "骑上自行车, 四轮车, 摩托车时不再自动装备头盔.", VehicleNoBikeHelemet);
            var vehicleFreeze = new MenuCheckboxItem("载具冻结", "冻结载具位置.", VehicleFrozen);
            var torqueEnabled = new MenuCheckboxItem("启用扭矩倍增器", "启用从下面列表中选择的扭矩倍增器.", VehicleTorqueMultiplier);
            var powerEnabled = new MenuCheckboxItem("启用功率倍增器", "启用从以下列表中选择的功率倍增器.", VehiclePowerMultiplier);
            var highbeamsOnHonk = new MenuCheckboxItem("鸣笛灯光闪烁", "按喇叭时打开载具的远光灯. 关闭车灯时不起作用.", FlashHighbeamsOnHonk);
            var showHealth = new MenuCheckboxItem("显示载具健康状况", "于屏幕上显示载具健康值的文本.", VehicleShowHealth);
            var infiniteFuel = new MenuCheckboxItem("无限油量", "启用或禁用该载具的无限油量, 仅在安装了 FRFuel 时有效.", VehicleInfiniteFuel);
            var vehicleRadioOverride = new MenuCheckboxItem("启用默认电台", "启用或禁用对所有载具默认电台的覆盖。", VehicleRadioOverride);

            // Create buttons.
            var fixVehicle = new MenuItem("载具全面修复", "修复载具上的任何外观和物理损坏.");
            var cleanVehicle = new MenuItem("载具全面清洗", "清洁载具上的任何泥垢及其它脏污.");
            var toggleEngine = new MenuItem("载具引擎开关", "打开/关闭发动机.");
            var setLicensePlateText = new MenuItem("载具自定义车牌", "为您的载具输入自定义车牌号码.");
            var modMenuBtn = new MenuItem("载具改装菜单", "在此调整和定制您的载具.")
            {
                Label = "→→→"
            };
            var doorsMenuBtn = new MenuItem("载具车门管理", "在此打开、关闭、拆卸和修复车门.")
            {
                Label = "→→→"
            };
            var windowsMenuBtn = new MenuItem("载具车窗管理", "在此摇上/摇下车窗或拆下/修复车窗.")
            {
                Label = "→→→"
            };
            var componentsMenuBtn = new MenuItem("载具额外改件", "添加/移除载具部件/改件装置.")
            {
                Label = "→→→"
            };
            var liveriesMenuBtn = new MenuItem("载具额外涂装", "用花哨的涂装为您的载具增添自有风格!")
            {
                Label = "→→→"
            };
            var colorsMenuBtn = new MenuItem("载具配色管理", "为载具增添一些 ~g~Snailsome ~s~ 色彩,让载具更具风格!")
            {
                Label = "→→→"
            };
            var underglowMenuBtn = new MenuItem("载具霓虹灯套件", "用花哨的霓虹底光让您的载具熠熠生辉!")
            {
                Label = "→→→"
            };
            var vehicleInvisible = new MenuItem("载具可见性", "使载具可见/不可见. ~r~您离开载具后,载具将立即恢复可见.");
            var flipVehicle = new MenuItem("载具翻转", "如载具发生翻滚, 可尝试使用此功能.");
            var vehicleAlarm = new MenuItem("载具警报器", "启动/停止载具警报器.");
            var cycleSeats = new MenuItem("载具座位循环", "循环切换可用的载具座位.");
            var lights = new List<string>()
            {
                "危险指示灯",
                "左指示灯",
                "右指示灯",
                "车内照明",
                //"Taxi Light", // this doesn't seem to work no matter what.
                "直升机聚光灯",
            };
            var vehicleLights = new MenuListItem("载具灯具管理", lights, 0, "开启/关闭载具各项灯具.");

            var stationNames = new List<string>();

            foreach (var radioStationName in Enum.GetNames(typeof(RadioStation)))
            {
                stationNames.Add(radioStationName);
            }

            var radioIndex = UserDefaults.VehicleDefaultRadio;

            if (radioIndex == (int)RadioStation.RadioOff)
            {
                var stations = (RadioStation[])Enum.GetValues(typeof(RadioStation));
                var index = Array.IndexOf(stations, RadioStation.RadioOff);
                radioIndex = index;
            }
            else if (radioIndex < 0)
            {
                radioIndex = 0;
            }

            var radioStations = new MenuListItem("设置默认电台", stationNames, radioIndex, "为生成的所有汽车选择默认电台。")
            {
                Enabled = VehicleRadioOverride
            };

            var tiresList = new List<string>() { "全部轮胎", "轮胎 #1", "轮胎 #2", "轮胎 #3", "轮胎 #4", "轮胎 #5", "轮胎 #6", "轮胎 #7", "轮胎 #8" };
            var vehicleTiresList = new MenuListItem("载具轮胎修复/销毁", tiresList, 0, "修复或毁坏载具的轮胎.注意,并非所有索引对所有载具都有效,有某些载具不起作用.");

            var destroyEngine = new MenuItem("载具引擎摧毁", "直接摧毁载具引擎.");

            var deleteBtn = new MenuItem("~r~载具删除", "删除载具, ~r~此项操作无法撤销~s~!")
            {
                LeftIcon = MenuItem.Icon.WARNING,
                Label = "→→→"
            };
            var deleteNoBtn = new MenuItem("取消", "取消删除载具并返回!");
            var deleteYesBtn = new MenuItem("~r~确认删除", "已知晓无法撤销并确认删除载具.")
            {
                LeftIcon = MenuItem.Icon.WARNING
            };

            // Create lists.
            var dirtlevel = new List<string> { "无", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            var setDirtLevel = new MenuListItem("设置污渍等级", dirtlevel, 0, "选择载具显示的污渍程度，按 ~r~ENTER~s~ " +
                "to apply the selected level.");
            var licensePlates = new List<string> {
                GetLabelText("CMOD_PLA_0"), // Plate Index 0 // BlueOnWhite1
                GetLabelText("CMOD_PLA_1"), // Plate Index 1 // BlueOnWhite2
                GetLabelText("CMOD_PLA_2"), // Plate Index 2 // BlueOnWhite3
                GetLabelText("CMOD_PLA_3"), // Plate Index 3 // YellowOnBlue
                GetLabelText("CMOD_PLA_4"), // Plate Index 4 // YellowOnBlack
                GetLabelText("PROL"), // Plate Index 5 // NorthYankton
                GetLabelText("CMOD_PLA_6"), // Plate Index 6 // ECola
                GetLabelText("CMOD_PLA_7"), // Plate Index 7 // LasVenturas
                GetLabelText("CMOD_PLA_8"), // Plate Index 8 // LibertyCity
                GetLabelText("CMOD_PLA_9"), // Plate Index 9 // LSCarMeet
                GetLabelText("CMOD_PLA_10"), // Plate Index 10 // LSPanic
                GetLabelText("CMOD_PLA_11"), // Plate Index 11 // LSPounders
                GetLabelText("CMOD_PLA_12"), // Plate Index 12 // Sprunk
            };
            var setLicensePlateType = new MenuListItem("载具牌照类型", licensePlates, 0, "选择车牌类型并键下 ~r~ENTER ~s~确认 " +
                "修改至载具.");
            var torqueMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            var torqueMultiplier = new MenuListItem("设置发动机扭矩乘数", torqueMultiplierList, 0, "设置发动机扭矩乘数.");
            var powerMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            var powerMultiplier = new MenuListItem("设置发动机功率乘数", powerMultiplierList, 0, "设置发动机功率乘数.");
            var speedLimiterOptions = new List<string>() { "设置", "重置", "自定义" };
            var speedLimiter = new MenuListItem("载具限速器", speedLimiterOptions, 0, "'设置'=设置最大速度为~y~当前速度~s~. '重置'=设置最大速度恢复默认值. 此选项只影响当前驾驶载具.");
            #endregion

            #region Submenus
            // Submenu's
            VehicleModMenu = new Menu("载具改装菜单", "载具改装");
            VehicleModMenu.InstructionalButtons.Add(Control.Jump, "载具车门开关");
            VehicleModMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_PRESSED, new Action<Menu, Control>((m, c) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    var open = GetVehicleDoorAngleRatio(veh.Handle, 0) < 0.1f;
                    if (open)
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            SetVehicleDoorOpen(veh.Handle, i, false, false);
                        }
                    }
                    else
                    {
                        SetVehicleDoorsShut(veh.Handle, false);
                    }
                }
            }), false));
            VehicleDoorsMenu = new Menu("载具车门管理", "载具车门管理");
            VehicleWindowsMenu = new Menu("载具车窗管理", "载具车窗管理");
            VehicleComponentsMenu = new Menu("载具额外改件", "载具附件/改件");
            VehicleLiveriesMenu = new Menu("载具额外涂装", "载具额外涂装");
            VehicleColorsMenu = new Menu("载具配色管理", "载具配色管理");
            DeleteConfirmMenu = new Menu("二次确认", "确认删除载具?");
            VehicleUnderglowMenu = new Menu("载具霓虹灯套件", "载具霓虹灯套件");

            MenuController.AddSubmenu(menu, VehicleModMenu);
            MenuController.AddSubmenu(menu, VehicleDoorsMenu);
            MenuController.AddSubmenu(menu, VehicleWindowsMenu);
            MenuController.AddSubmenu(menu, VehicleComponentsMenu);
            MenuController.AddSubmenu(menu, VehicleLiveriesMenu);
            MenuController.AddSubmenu(menu, VehicleColorsMenu);
            MenuController.AddSubmenu(menu, DeleteConfirmMenu);
            MenuController.AddSubmenu(menu, VehicleUnderglowMenu);
            #endregion

            #region Add items to the menu.
            // Add everything to the menu. (based on permissions)
            if (IsAllowed(Permission.VOGod)) // GOD MODE
            {
                menu.AddMenuItem(vehicleGod);
                menu.AddMenuItem(vehGodMenuBtn);
                MenuController.BindMenuItem(menu, vehGodMenu, vehGodMenuBtn);

                var godInvincible = new MenuCheckboxItem("战无不胜", "使载具战无不胜, 所向披靡. 免疫包括火焰伤害、爆炸伤害、碰撞伤害等.", VehicleGodInvincible);
                var godEngine = new MenuCheckboxItem("引擎损坏", "使载具引擎免受任何损坏.", VehicleGodEngine);
                var godVisual = new MenuCheckboxItem("视觉损伤", "可防止载具贴花出现划痕和其他损坏. 无法防止车体变形损坏.", VehicleGodVisual);
                var godStrongWheels = new MenuCheckboxItem("强劲车轮", "防止车轮变形, 降低操控性. 此功能非轮胎防弹.", VehicleGodStrongWheels);
                var godRamp = new MenuCheckboxItem("坡道损坏", "使斜坡越野车等载具在使用斜坡时无法受到伤害.", VehicleGodRamp);
                var godAutoRepair = new MenuCheckboxItem("~r~自动维修", "在载具出现任何类型的损坏时自动修复. 建议将其关闭, 以防出现故障.", VehicleGodAutoRepair);

                vehGodMenu.AddMenuItem(godInvincible);
                vehGodMenu.AddMenuItem(godEngine);
                vehGodMenu.AddMenuItem(godVisual);
                vehGodMenu.AddMenuItem(godStrongWheels);
                vehGodMenu.AddMenuItem(godRamp);
                vehGodMenu.AddMenuItem(godAutoRepair);

                vehGodMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    if (item == godInvincible)
                    {
                        VehicleGodInvincible = _checked;
                    }
                    else if (item == godEngine)
                    {
                        VehicleGodEngine = _checked;
                    }
                    else if (item == godVisual)
                    {
                        VehicleGodVisual = _checked;
                    }
                    else if (item == godStrongWheels)
                    {
                        VehicleGodStrongWheels = _checked;
                    }
                    else if (item == godRamp)
                    {
                        VehicleGodRamp = _checked;
                    }
                    else if (item == godAutoRepair)
                    {
                        VehicleGodAutoRepair = _checked;
                    }
                };

            }
            if (IsAllowed(Permission.VORepair)) // REPAIR VEHICLE
            {
                menu.AddMenuItem(fixVehicle);
            }
            if (IsAllowed(Permission.VOKeepClean))
            {
                menu.AddMenuItem(vehicleNeverDirty);
            }
            if (IsAllowed(Permission.VOWash))
            {
                menu.AddMenuItem(cleanVehicle); // CLEAN VEHICLE
                menu.AddMenuItem(setDirtLevel); // SET DIRT LEVEL
            }
            if (IsAllowed(Permission.VOMod)) // MOD MENU
            {
                menu.AddMenuItem(modMenuBtn);
            }
            if (IsAllowed(Permission.VOColors)) // COLORS MENU
            {
                menu.AddMenuItem(colorsMenuBtn);
            }
            if (IsAllowed(Permission.VOUnderglow)) // UNDERGLOW EFFECTS
            {
                menu.AddMenuItem(underglowMenuBtn);
                MenuController.BindMenuItem(menu, VehicleUnderglowMenu, underglowMenuBtn);
            }
            if (IsAllowed(Permission.VOLiveries)) // LIVERIES MENU
            {
                menu.AddMenuItem(liveriesMenuBtn);
            }
            if (IsAllowed(Permission.VOComponents)) // COMPONENTS MENU
            {
                menu.AddMenuItem(componentsMenuBtn);
            }
            if (IsAllowed(Permission.VOEngine)) // TOGGLE ENGINE ON/OFF
            {
                menu.AddMenuItem(toggleEngine);
            }
            if (IsAllowed(Permission.VOChangePlate))
            {
                menu.AddMenuItem(setLicensePlateText); // SET LICENSE PLATE TEXT
                menu.AddMenuItem(setLicensePlateType); // SET LICENSE PLATE TYPE
            }
            if (IsAllowed(Permission.VODoors)) // DOORS MENU
            {
                menu.AddMenuItem(doorsMenuBtn);
            }
            if (IsAllowed(Permission.VOWindows)) // WINDOWS MENU
            {
                menu.AddMenuItem(windowsMenuBtn);
            }
            if (IsAllowed(Permission.VOBikeSeatbelt))
            {
                menu.AddMenuItem(vehicleBikeSeatbelt);
            }
            if (IsAllowed(Permission.VOSpeedLimiter)) // SPEED LIMITER
            {
                menu.AddMenuItem(speedLimiter);
            }
            if (IsAllowed(Permission.VOTorqueMultiplier))
            {
                menu.AddMenuItem(torqueEnabled); // TORQUE ENABLED
                menu.AddMenuItem(torqueMultiplier); // TORQUE LIST
            }
            if (IsAllowed(Permission.VOPowerMultiplier))
            {
                menu.AddMenuItem(powerEnabled); // POWER ENABLED
                menu.AddMenuItem(powerMultiplier); // POWER LIST
            }
            if (IsAllowed(Permission.VODisableTurbulence))
            {
                menu.AddMenuItem(vehicleNoTurbulence);
                menu.AddMenuItem(vehicleNoTurbulenceHeli);
            }
            if (IsAllowed(Permission.VOAnchorBoat))
            {
                menu.AddMenuItem(vehicleSetAnchor);
            }
            if (IsAllowed(Permission.VOFlip)) // FLIP VEHICLE
            {
                menu.AddMenuItem(flipVehicle);
            }
            if (IsAllowed(Permission.VOAlarm)) // TOGGLE VEHICLE ALARM
            {
                menu.AddMenuItem(vehicleAlarm);
            }
            if (IsAllowed(Permission.VOCycleSeats)) // CYCLE THROUGH VEHICLE SEATS
            {
                menu.AddMenuItem(cycleSeats);
            }
            if (IsAllowed(Permission.VOLights)) // VEHICLE LIGHTS LIST
            {
                menu.AddMenuItem(vehicleLights);
            }
            if (IsAllowed(Permission.VOFixOrDestroyTires))
            {
                menu.AddMenuItem(vehicleTiresList);
            }
            if (IsAllowed(Permission.VODestroyEngine))
            {
                menu.AddMenuItem(destroyEngine);
            }
            if (IsAllowed(Permission.VOFreeze)) // FREEZE VEHICLE
            {
                menu.AddMenuItem(vehicleFreeze);
            }
            if (IsAllowed(Permission.VOInvisible)) // MAKE VEHICLE INVISIBLE
            {
                menu.AddMenuItem(vehicleInvisible);
            }
            if (IsAllowed(Permission.VOEngineAlwaysOn)) // LEAVE ENGINE RUNNING
            {
                menu.AddMenuItem(vehicleEngineAO);
            }
            if (IsAllowed(Permission.VOInfiniteFuel)) // INFINITE FUEL
            {
                menu.AddMenuItem(infiniteFuel);
            }
            // always allowed
            menu.AddMenuItem(showHealth); // SHOW VEHICLE HEALTH

            // I don't really see why would you want to disable this so I will not add useless permissions
            menu.AddMenuItem(vehicleRadioOverride);
            menu.AddMenuItem(radioStations);

            if (IsAllowed(Permission.VONoSiren) && !GetSettingsBool(Setting.vmenu_use_els_compatibility_mode)) // DISABLE SIREN
            {
                menu.AddMenuItem(vehicleNoSiren);
            }
            if (IsAllowed(Permission.VONoHelmet)) // DISABLE BIKE HELMET
            {
                menu.AddMenuItem(vehicleNoBikeHelmet);
            }
            if (IsAllowed(Permission.VOFlashHighbeamsOnHonk)) // FLASH HIGHBEAMS ON HONK
            {
                menu.AddMenuItem(highbeamsOnHonk);
            }

            if (IsAllowed(Permission.VODelete)) // DELETE VEHICLE
            {
                menu.AddMenuItem(deleteBtn);
            }
            #endregion

            #region delete vehicle handle stuff
            DeleteConfirmMenu.AddMenuItem(deleteNoBtn);
            DeleteConfirmMenu.AddMenuItem(deleteYesBtn);
            DeleteConfirmMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteNoBtn)
                {
                    DeleteConfirmMenu.GoBack();
                }
                else
                {
                    DeleteVehicle();
                    DeleteConfirmMenu.GoBack();
                }
            };
            #endregion

            #region Bind Submenus to their buttons.
            MenuController.BindMenuItem(menu, VehicleModMenu, modMenuBtn);
            MenuController.BindMenuItem(menu, VehicleDoorsMenu, doorsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleWindowsMenu, windowsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleComponentsMenu, componentsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleLiveriesMenu, liveriesMenuBtn);
            MenuController.BindMenuItem(menu, VehicleColorsMenu, colorsMenuBtn);
            MenuController.BindMenuItem(menu, DeleteConfirmMenu, deleteBtn);
            #endregion

            #region Handle button presses
            // Manage button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteBtn) // reset the index so that "no" / "cancel" will always be selected by default.
                {
                    DeleteConfirmMenu.RefreshIndex();
                }
                // If the player is actually in a vehicle, continue.
                if (GetVehicle() != null && GetVehicle().Exists())
                {
                    // Create a vehicle object.
                    var vehicle = GetVehicle();

                    // Check if the player is the driver of the vehicle, if so, continue.
                    if (vehicle.GetPedOnSeat(VehicleSeat.Driver) == Game.PlayerPed)
                    {
                        // Repair vehicle.
                        if (item == fixVehicle)
                        {
                            vehicle.Repair();
                        }
                        // Clean vehicle.
                        else if (item == cleanVehicle)
                        {
                            vehicle.Wash();
                        }
                        // Flip vehicle.
                        else if (item == flipVehicle)
                        {
                            SetVehicleOnGroundProperly(vehicle.Handle);
                        }
                        // Toggle alarm.
                        else if (item == vehicleAlarm)
                        {
                            ToggleVehicleAlarm(vehicle);
                        }
                        // Toggle engine
                        else if (item == toggleEngine)
                        {
                            SetVehicleEngineOn(vehicle.Handle, !vehicle.IsEngineRunning, false, true);
                        }
                        // Set license plate text
                        else if (item == setLicensePlateText)
                        {
                            SetLicensePlateCustomText();
                        }
                        // Make vehicle invisible.
                        else if (item == vehicleInvisible)
                        {
                            if (vehicle.IsVisible)
                            {
                                // Check the visibility of all peds inside before setting the vehicle as invisible.
                                var visiblePeds = new Dictionary<Ped, bool>();
                                foreach (var p in vehicle.Occupants)
                                {
                                    visiblePeds.Add(p, p.IsVisible);
                                }

                                // Set the vehicle invisible or invincivble.
                                vehicle.IsVisible = !vehicle.IsVisible;

                                // Restore visibility for each ped.
                                foreach (var pe in visiblePeds)
                                {
                                    pe.Key.IsVisible = pe.Value;
                                }
                            }
                            else
                            {
                                // Set the vehicle invisible or invincivble.
                                vehicle.IsVisible = !vehicle.IsVisible;
                            }
                        }
                        // Destroy vehicle engine
                        else if (item == destroyEngine)
                        {
                            SetVehicleEngineHealth(vehicle.Handle, -4000);
                        }
                    }

                    // If the player is not the driver seat and a button other than the option below (cycle seats) was pressed, notify them.
                    else if (item != cycleSeats)
                    {
                        Notify.Error("您必须是载具驾驶员才能访问该菜单!", true, false);
                    }

                    // Cycle vehicle seats
                    if (item == cycleSeats)
                    {
                        CycleThroughSeats();
                    }
                }
            };
            #endregion

            #region Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // Create a vehicle object.
                var vehicle = GetVehicle();

                if (item == vehicleGod) // God Mode Toggled
                {
                    VehicleGodMode = _checked;
                }
                else if (item == vehicleFreeze) // Freeze Vehicle Toggled
                {
                    VehicleFrozen = _checked;
                    if (!_checked)
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            FreezeEntityPosition(vehicle.Handle, false);
                        }
                    }
                }
                else if (item == torqueEnabled) // Enable Torque Multiplier Toggled
                {
                    VehicleTorqueMultiplier = _checked;
                }
                else if (item == powerEnabled) // Enable Power Multiplier Toggled
                {
                    VehiclePowerMultiplier = _checked;
                    if (_checked)
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, VehiclePowerMultiplierAmount);
                        }
                    }
                    else
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, 1f);
                        }
                    }
                }
                else if (item == vehicleEngineAO) // Leave Engine Running (vehicle always on) Toggled
                {
                    VehicleEngineAlwaysOn = _checked;
                }
                else if (item == showHealth) // show vehicle health on screen.
                {
                    VehicleShowHealth = _checked;
                }
                else if (item == vehicleRadioOverride)
                {
                    int overrideDisabled = -1;
                    int starterChannel = (int)RadioStation.LosSantosRockRadio;

                    VehicleRadioOverride = _checked;

                    UserDefaults.VehicleDefaultRadio = _checked ? starterChannel : overrideDisabled;

                    radioStations.ListIndex = starterChannel;
                    radioStations.Enabled = _checked;
                }
                else if (item == vehicleNoSiren) // Disable Siren Toggled
                {
                    VehicleNoSiren = _checked;
                    if (vehicle != null && vehicle.Exists())
                    {
                        vehicle.IsSirenSilent = _checked;
                    }
                }
                else if (item == vehicleNoBikeHelmet) // No Helemet Toggled
                {
                    VehicleNoBikeHelemet = _checked;
                }
                else if (item == highbeamsOnHonk)
                {
                    FlashHighbeamsOnHonk = _checked;
                }
                else if (item == vehicleNoTurbulence)
                {
                    DisablePlaneTurbulence = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsPlane)
                    {
                        if (MainMenu.VehicleOptionsMenu.DisablePlaneTurbulence)
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 0f);
                        }
                        else
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 1.0f);
                        }
                    }
                }
                else if (item == vehicleNoTurbulenceHeli)
                {
                    DisableHelicopterTurbulence = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsHelicopter)
                    {
                        if (MainMenu.VehicleOptionsMenu.DisableHelicopterTurbulence)
                        {
                            SetHeliTurbulenceScalar(vehicle.Handle, 0f);
                        }
                        else
                        {
                            SetHeliTurbulenceScalar(vehicle.Handle, 1f);
                        }
                    }
                }
                else if (item == vehicleSetAnchor)
                {
                    AnchorBoat = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsBoat && CanAnchorBoatHere(vehicle.Handle))
                    {
                        if (MainMenu.VehicleOptionsMenu.AnchorBoat)
                        {
                            SetBoatAnchor(vehicle.Handle, true);
                            SetBoatFrozenWhenAnchored(vehicle.Handle, true);
                            SetForcedBoatLocationWhenAnchored(vehicle.Handle, true);
                        }
                        else
                        {
                            SetBoatAnchor(vehicle.Handle, false);
                            SetBoatFrozenWhenAnchored(vehicle.Handle, false);
                            SetForcedBoatLocationWhenAnchored(vehicle.Handle, false);
                        }
                    }
                }
                else if (item == vehicleNeverDirty)
                {
                    VehicleNeverDirty = _checked;
                }
                else if (item == vehicleBikeSeatbelt)
                {
                    VehicleBikeSeatbelt = _checked;
                }
                else if (item == infiniteFuel)
                {
                    VehicleInfiniteFuel = _checked;
                    TriggerEvent("vMenu:InfiniteFuelToggled", _checked);
                }
            };
            #endregion

            #region Handle List Changes.
            // Handle list changes.
            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                Vehicle veh = GetVehicle();
                bool vehExists = veh != null && veh.Exists();

                if (vehExists)
                {
                    // If the torque multiplier changed. Change the torque multiplier to the new value.
                    if (item == torqueMultiplier)
                    {
                        // Get the selected value and remove the "x" in the string with nothing.
                        var value = torqueMultiplierList[newIndex].ToString().Replace("x", "");
                        // Convert the value to a float and set it as a public variable.
                        VehicleTorqueMultiplierAmount = float.Parse(value);
                    }
                    // If the power multiplier is changed. Change the power multiplier to the new value.
                    else if (item == powerMultiplier)
                    {
                        // Get the selected value. Remove the "x" from the string.
                        var value = powerMultiplierList[newIndex].ToString().Replace("x", "");
                        // Conver the string into a float and set it to be the value of the public variable.
                        VehiclePowerMultiplierAmount = float.Parse(value);
                        if (VehiclePowerMultiplier)
                        {
                            SetVehicleEnginePowerMultiplier(veh.Handle, VehiclePowerMultiplierAmount);
                        }
                    }
                    else if (item == setLicensePlateType)
                    {
                        // Set the license plate style.
                        switch (newIndex)
                        {
                            case 0:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite1;
                                break;
                            case 1:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite2;
                                break;
                            case 2:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite3;
                                break;
                            case 3:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;
                                break;
                            case 4:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlack;
                                break;
                            case 5:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.NorthYankton;
                                break;
                            case 6:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.ECola;
                                break;
                            case 7:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LasVenturas;
                                break;
                            case 8:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LibertyCity;
                                break;
                            case 9:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LSCarMeet;
                                break;
                            case 10:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LSPanic;
                                break;
                            case 11:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LSPounders;
                                break;
                            case 12:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.Sprunk;
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (item == radioStations)
                {
                    var newStation = (RadioStation)Enum.GetValues(typeof(RadioStation)).GetValue(newIndex);

                    if (vehExists && DoesPlayerVehHaveRadio())
                    {
                        veh.RadioStation = newStation;
                    }

                    UserDefaults.VehicleDefaultRadio = (int)newStation;
                }
            };
            #endregion

            #region Handle List Items Selected
            menu.OnListItemSelect += async (sender, item, listIndex, itemIndex) =>
            {
                // Set dirt level
                if (item == setDirtLevel)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        GetVehicle().DirtLevel = float.Parse(listIndex.ToString());
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Toggle vehicle lights
                else if (item == vehicleLights)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        // We need to do % 4 because this seems to be some sort of flags system. For a taxi, this function returns 65, 66, etc.
                        // So % 4 takes care of that.
                        var state = GetVehicleIndicatorLights(veh.Handle) % 4; // 0 = none, 1 = left, 2 = right, 3 = both

                        if (listIndex == 0) // Hazard lights
                        {
                            if (state != 3) // either all lights are off, or one of the two (left/right) is off.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else // both are on.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 1) // left indicator
                        {
                            if (state != 1) // Left indicator is (only) off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 2) // right indicator
                        {
                            if (state != 2) // Right indicator (only) is off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 3) // Interior lights
                        {
                            SetVehicleInteriorlight(veh.Handle, !IsVehicleInteriorLightOn(veh.Handle));
                            //CommonFunctions.Log("Something cool here.");
                        }
                        //else if (listIndex == 4) // taxi light
                        //{
                        //    veh.IsTaxiLightOn = !veh.IsTaxiLightOn;
                        //    //    SetTaxiLights(veh, true);
                        //    //    SetTaxiLights(veh, false);
                        //    //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //    //SetTaxiLights(veh, true);
                        //    //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //    //SetTaxiLights(veh, false);
                        //    //    //SetTaxiLights(veh, !IsTaxiLightOn(veh));
                        //    //    CommonFunctions.Log
                        //}
                        else if (listIndex == 4) // helicopter spotlight
                        {
                            SetVehicleSearchlight(veh.Handle, !IsVehicleSearchlightOn(veh.Handle), true);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Speed Limiter
                else if (item == speedLimiter)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var vehicle = GetVehicle();

                        if (vehicle != null && vehicle.Exists())
                        {
                            if (listIndex == 0) // Set
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                SetEntityMaxSpeed(vehicle.Handle, vehicle.Speed);

                                if (ShouldUseMetricMeasurements()) // kph
                                {
                                    Notify.Info($"车速现在限制为 ~b~{Math.Round(vehicle.Speed * 3.6f, 1)} KPH~s~.");
                                }
                                else // mph
                                {
                                    Notify.Info($"车速现在限制为 ~b~{Math.Round(vehicle.Speed * 2.237f, 1)} MPH~s~.");
                                }

                            }
                            else if (listIndex == 1) // Reset
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f); // Default max speed seemingly for all vehicles.
                                Notify.Info("车速已不再受限.");
                            }
                            else if (listIndex == 2) // custom speed
                            {
                                var inputSpeed = await GetUserInput("请配置载具车速(单位：米/秒)", "20.0", 5);
                                if (!string.IsNullOrEmpty(inputSpeed))
                                {
                                    if (float.TryParse(inputSpeed, out var outFloat))
                                    {
                                        //vehicle.MaxSpeed = outFloat;
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outFloat + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // kph
                                        {
                                            Notify.Info($"车速现在限制为 ~b~{Math.Round(outFloat * 3.6f, 1)} KPH~s~.");
                                        }
                                        else // mph
                                        {
                                            Notify.Info($"车速现在限制为 ~b~{Math.Round(outFloat * 2.237f, 1)} MPH~s~.");
                                        }
                                    }
                                    else if (int.TryParse(inputSpeed, out var outInt))
                                    {
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outInt + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // kph
                                        {
                                            Notify.Info($"车速现在限制为 ~b~{Math.Round(outInt * 3.6f, 1)} KPH~s~.");
                                        }
                                        else // mph
                                        {
                                            Notify.Info($"车速现在限制为 ~b~{Math.Round(outInt * 2.237f, 1)} MPH~s~.");
                                        }
                                    }
                                    else
                                    {
                                        Notify.Error("这不是有效数字. 请输入有效的速度(米/秒).");
                                    }
                                }
                                else
                                {
                                    Notify.Error(CommonErrors.InvalidInput);
                                }
                            }
                        }
                    }
                }
                else if (item == vehicleTiresList)
                {
                    //bool fix = item == vehicleTiresList;

                    var veh = GetVehicle();
                    if (veh != null && veh.Exists())
                    {
                        if (Game.PlayerPed == veh.Driver)
                        {
                            if (listIndex == 0)
                            {
                                if (IsVehicleTyreBurst(veh.Handle, 0, false))
                                {
                                    for (var i = 0; i < 8; i++)
                                    {
                                        SetVehicleTyreFixed(veh.Handle, i);
                                    }
                                    Notify.Success("所有载具轮胎均已修复.");
                                }
                                else
                                {
                                    for (var i = 0; i < 8; i++)
                                    {
                                        SetVehicleTyreBurst(veh.Handle, i, false, 1f);
                                    }
                                    Notify.Success("所有载具轮胎均已损毁.");
                                }
                            }
                            else
                            {
                                var index = listIndex - 1;
                                if (IsVehicleTyreBurst(veh.Handle, index, false))
                                {
                                    SetVehicleTyreFixed(veh.Handle, index);
                                    Notify.Success($"载具轮胎 #{listIndex} 已修复.");
                                }
                                else
                                {
                                    SetVehicleTyreBurst(veh.Handle, index, false, 1f);
                                    Notify.Success($"载具轮胎 #{listIndex} 已销毁.");
                                }
                            }
                        }
                        else
                        {
                            Notify.Error(CommonErrors.NeedToBeTheDriver);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
            };
            #endregion

            #region Vehicle Colors Submenu Stuff
            // color customization menu
            var customizeColorMenu = new Menu("载具配色管理", "自定义颜色");
            MenuController.AddSubmenu(VehicleColorsMenu, customizeColorMenu);

            var colorsCustomizationBtn = new MenuItem("自定义颜色") { Label = "→→→" };
            VehicleColorsMenu.AddMenuItem(colorsCustomizationBtn);
            MenuController.BindMenuItem(VehicleColorsMenu, customizeColorMenu, colorsCustomizationBtn);

            // primary menu
            var primaryColorsMenu = new Menu("载具配色管理", "主要颜色");
            MenuController.AddSubmenu(customizeColorMenu, primaryColorsMenu);

            var primaryColorsBtn = new MenuItem("主要颜色") { Label = "→→→" };
            customizeColorMenu.AddMenuItem(primaryColorsBtn);
            MenuController.BindMenuItem(customizeColorMenu, primaryColorsMenu, primaryColorsBtn);

            // secondary menu
            var secondaryColorsMenu = new Menu("载具配色管理", "次要颜色");
            MenuController.AddSubmenu(customizeColorMenu, secondaryColorsMenu);

            var secondaryColorsBtn = new MenuItem("次要颜色") { Label = "→→→" };
            customizeColorMenu.AddMenuItem(secondaryColorsBtn);
            MenuController.BindMenuItem(customizeColorMenu, secondaryColorsMenu, secondaryColorsBtn);

            var presetColorsBtn = new MenuListItem("预设颜色", [], 0);
            customizeColorMenu.AddMenuItem(presetColorsBtn);

            var chrome = new MenuItem("镀铬效果");
            customizeColorMenu.AddMenuItem(chrome);

            // color lists
            var classic = new List<string>();
            var matte = new List<string>();
            var metals = new List<string>();
            var util = new List<string>();
            var worn = new List<string>();
            var chameleon = new List<string>();
            var wheelColors = new List<string>() { "默认" };

            // Just quick and dirty solution to put this in a new enclosed section so that we can still use 'i' as a counter in the other code parts.
            {
                var i = 0;
                foreach (var vc in VehicleData.ClassicColors)
                {
                    classic.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.ClassicColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MatteColors)
                {
                    matte.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MatteColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MetalColors)
                {
                    metals.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MetalColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.UtilColors)
                {
                    util.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.UtilColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.WornColors)
                {
                    worn.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.WornColors.Count})");
                    i++;
                }

                if (GetSettingsBool(Setting.vmenu_using_chameleon_colours))
                {
                    i = 0;
                    foreach (var vc in VehicleData.变色龙漆面Colors)
                    {
                        chameleon.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.变色龙漆面Colors.Count})");
                        i++;
                    }
                }

                wheelColors.AddRange(classic);
            }

            var wheelColorsList = new MenuListItem("轮毂颜色", wheelColors, 0);
            var dashColorList = new MenuListItem("仪表板颜色", classic, 0);
            var intColorList = new MenuListItem("内饰/饰边颜色", classic, 0);
            var vehicleEnveffScale = new MenuSliderItem("载具能效比", "这只对某些载具有效, 例如 besra.它能'淡化'某些油漆层.", 0, 20, 10, true);

            VehicleColorsMenu.AddMenuItem(vehicleEnveffScale);

            VehicleColorsMenu.OnSliderPositionChange += (m, sliderItem, oldPosition, newPosition, itemIndex) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Driver == Game.PlayerPed && !veh.IsDead)
                {
                    if (sliderItem == vehicleEnveffScale)
                    {
                        SetVehicleEnveffScale(veh.Handle, newPosition / 20f);
                    }
                }
                else
                {
                    Notify.Error("您必须是可驾驶载具的驾驶员, 才能更改此滑块.");
                }
            };

            VehicleColorsMenu.AddMenuItem(dashColorList);
            VehicleColorsMenu.AddMenuItem(intColorList);
            VehicleColorsMenu.AddMenuItem(wheelColorsList);

            VehicleColorsMenu.OnListIndexChange += HandleListIndexChanges;

            void HandleListIndexChanges(Menu sender, MenuListItem listItem, int oldIndex, int newIndex, int itemIndex)
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    var primaryColor = 0;
                    var secondaryColor = 0;
                    var pearlColor = 0;
                    var wheelColor = 0;
                    var dashColor = 0;
                    var intColor = 0;

                    GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);
                    GetVehicleExtraColours(veh.Handle, ref pearlColor, ref wheelColor);
                    GetVehicleDashboardColour(veh.Handle, ref dashColor);
                    GetVehicleInteriorColour(veh.Handle, ref intColor);

                    if (sender == primaryColorsMenu)
                    {
                        if (itemIndex == 2)
                        {
                            pearlColor = VehicleData.ClassicColors[newIndex].id;
                        }
                        else
                        {
                            pearlColor = 0;
                        }

                        switch (itemIndex)
                        {
                            case 0:
                            case 1:
                            case 2:
                                primaryColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 3:
                                primaryColor = VehicleData.MatteColors[newIndex].id;
                                break;
                            case 4:
                                primaryColor = VehicleData.MetalColors[newIndex].id;
                                break;
                            case 5:
                                primaryColor = VehicleData.UtilColors[newIndex].id;
                                break;
                            case 6:
                                primaryColor = VehicleData.WornColors[newIndex].id;
                                break;
                        }

                        if (GetSettingsBool(Setting.vmenu_using_chameleon_colours))
                        {
                            if (itemIndex == 7)
                            {
                                primaryColor = VehicleData.变色龙漆面Colors[newIndex].id;
                                secondaryColor = VehicleData.变色龙漆面Colors[newIndex].id;

                                SetVehicleModKit(veh.Handle, 0);
                            }
                        }

                        ClearVehicleCustomPrimaryColour(veh.Handle);
                        veh.State.Set("vMenu:PrimaryPaintFinish", null, true);
                        SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
                    }
                    else if (sender == secondaryColorsMenu)
                    {
                        switch (itemIndex)
                        {
                            case 0:
                            case 1:
                                pearlColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 2:
                            case 3:
                                secondaryColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 4:
                                secondaryColor = VehicleData.MatteColors[newIndex].id;
                                break;
                            case 5:
                                secondaryColor = VehicleData.MetalColors[newIndex].id;
                                break;
                            case 6:
                                secondaryColor = VehicleData.UtilColors[newIndex].id;
                                break;
                            case 7:
                                secondaryColor = VehicleData.WornColors[newIndex].id;
                                break;
                        }

                        ClearVehicleCustomSecondaryColour(veh.Handle);
                        veh.State.Set("vMenu:SecondaryPaintFinish", null, true);
                        SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
                    }
                    else if (sender == VehicleColorsMenu)
                    {
                        if (listItem == wheelColorsList)
                        {
                            if (newIndex == 0)
                            {
                                wheelColor = 156; // default alloy color.
                            }
                            else
                            {
                                wheelColor = VehicleData.ClassicColors[newIndex - 1].id;
                            }
                        }
                        else if (listItem == dashColorList)
                        {
                            dashColor = VehicleData.ClassicColors[newIndex].id;
                            // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                            // this should actually be called SetVehicleDashboardColour
                            SetVehicleInteriorColour(veh.Handle, dashColor);
                        }
                        else if (listItem == intColorList)
                        {
                            intColor = VehicleData.ClassicColors[newIndex].id;
                            // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                            // this should actually be called SetVehicleInteriorColour
                            SetVehicleDashboardColour(veh.Handle, intColor);
                        }
                    }

                    SetVehicleExtraColours(veh.Handle, pearlColor, wheelColor);
                }
                else
                {
                    Notify.Error("您必须是载具的驾驶员才能更改载具颜色.");
                }
            }

            for (var i = 0; i < 2; i++)
            {
                var customColour = new MenuItem("自定义 RGB 颜色") { Label = "→→→" };
                var pearlescentList = new MenuListItem("珠光喷漆", classic, 0);
                var classicList = new MenuListItem("经典喷漆", classic, 0);
                var metallicList = new MenuListItem("光面喷漆", classic, 0);
                var matteList = new MenuListItem("哑光喷漆", matte, 0);
                var metalList = new MenuListItem("金属喷漆", metals, 0);
                var utilList = new MenuListItem("工业喷漆", util, 0);
                var wornList = new MenuListItem("旧化喷漆", worn, 0);

                if (i == 0)
                {
                    var customColourMenuPrimary = new Menu("自定义颜色", "自定义载具颜色");
                    primaryColorsMenu.AddMenuItem(customColour);
                    primaryColorsMenu.AddMenuItem(classicList);
                    primaryColorsMenu.AddMenuItem(metallicList);
                    primaryColorsMenu.AddMenuItem(matteList);
                    primaryColorsMenu.AddMenuItem(metalList);
                    primaryColorsMenu.AddMenuItem(utilList);
                    primaryColorsMenu.AddMenuItem(wornList);
                    MenuController.AddSubmenu(primaryColorsMenu, customColourMenuPrimary);
                    MenuController.BindMenuItem(primaryColorsMenu, customColourMenuPrimary, customColour);
                    

                    if (GetSettingsBool(Setting.vmenu_using_chameleon_colours))
                    {
                        var chameleonList = new MenuListItem("变色龙漆面", chameleon, 0);

                        primaryColorsMenu.AddMenuItem(chameleonList);
                    }
                    
                    CreateCustomColourMenu(customColourMenuPrimary, RGBType.primaryPaint);
                    primaryColorsMenu.OnListIndexChange += HandleListIndexChanges;
                }
                else
                {
                    var customColourMenuSecondary = new Menu("自定义颜色", "自定义载具颜色");
                    secondaryColorsMenu.AddMenuItem(customColour);
                    secondaryColorsMenu.AddMenuItem(pearlescentList);
                    secondaryColorsMenu.AddMenuItem(classicList);
                    secondaryColorsMenu.AddMenuItem(metallicList);
                    secondaryColorsMenu.AddMenuItem(matteList);
                    secondaryColorsMenu.AddMenuItem(metalList);
                    secondaryColorsMenu.AddMenuItem(utilList);
                    secondaryColorsMenu.AddMenuItem(wornList);
                    MenuController.AddSubmenu(secondaryColorsMenu, customColourMenuSecondary);
                    MenuController.BindMenuItem(secondaryColorsMenu, customColourMenuSecondary, customColour);

                    CreateCustomColourMenu(customColourMenuSecondary, RGBType.secondaryPaint);
                    secondaryColorsMenu.OnListIndexChange += HandleListIndexChanges;
                }
            }

            customizeColorMenu.OnMenuOpen += (_) =>
            {
                int numVehColors = GetNumberOfVehicleColours(GetVehicle().Handle);

                if (numVehColors == 0)
                {
                    presetColorsBtn.Enabled = false;
                    presetColorsBtn.ListItems = ["No 预设颜色"];
                    presetColorsBtn.ListIndex = 0;
                    return;
                }

                List<string> colorOptions = [];

                presetColorsBtn.Enabled = true;

                for (int i = 0; i < numVehColors; i++)
                {
                    colorOptions.Add($"Preset Color #{i + 1}");
                }

                int currentColor = GetVehicleColourCombination(GetVehicle().Handle);

                presetColorsBtn.ListItems = colorOptions;
                presetColorsBtn.ListIndex = currentColor < 0 ? 0 : currentColor;
            };

            customizeColorMenu.OnItemSelect += (_, item, _) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    if (item == chrome)
                    {
                        SetVehicleColours(veh.Handle, 120, 120); // chrome is index 120
                    }
                }
                else
                {
                    Notify.Error("您必须是可驾驶载具的驾驶员, 才能更改此功能.");
                }
            };

            customizeColorMenu.OnListItemSelect += (_, _, index, _) => ChangeVehiclePresetColor(index);

            customizeColorMenu.OnListIndexChange += (_, _, _, index, _) => ChangeVehiclePresetColor(index);

            void ChangeVehiclePresetColor(int index)
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    SetVehicleColourCombination(veh.Handle, index);
                }
                else
                {
                    Notify.Error("您必须是可驾驶载具的驾驶员, 才能更改此功能.");
                }
            }
            #endregion

            #region Vehicle Doors Submenu Stuff
            var openAll = new MenuItem("打开全部车门", "开启所有车门.");
            var closeAll = new MenuItem("关闭全部车门", "关闭所有车门.");
            var LF = new MenuItem("左前门", "打开/关闭左前门.");
            var RF = new MenuItem("右前门", "打开/关闭右前门.");
            var LR = new MenuItem("左后门", "打开/关闭左后门.");
            var RR = new MenuItem("右后门", "打开/关闭右后门.");
            var HD = new MenuItem("引擎盖", "打开/关闭引擎盖.");
            var TR = new MenuItem("后备箱", "打开/关闭后备箱.");
            var E1 = new MenuItem("额外门 1", "打开/关闭额外门（#1）.请注意, 大多数载具没有此门.");
            var E2 = new MenuItem("额外门 2", "打开/关闭额外门（#2）.请注意, 大多数载具没有此门.");
            var BB = new MenuItem("炸弹舱", "打开/关闭炸弹舱.仅在某些飞机上可用.");
            var doors = new List<string>() { "左前门", "右前门", "左后门", "右后门", "引擎盖", "后备箱", "额外门 1", "额外门 2" };
            var removeDoorList = new MenuListItem("移除车门", doors, 0, "完全移除特定载具的车门.");
            var deleteDoors = new MenuCheckboxItem("删除移除的车门", "启用时,通过上面的列表移除的车门将从世界中删除. 否则车门将只会掉到地上.", false);

            VehicleDoorsMenu.AddMenuItem(LF);
            VehicleDoorsMenu.AddMenuItem(RF);
            VehicleDoorsMenu.AddMenuItem(LR);
            VehicleDoorsMenu.AddMenuItem(RR);
            VehicleDoorsMenu.AddMenuItem(HD);
            VehicleDoorsMenu.AddMenuItem(TR);
            VehicleDoorsMenu.AddMenuItem(E1);
            VehicleDoorsMenu.AddMenuItem(E2);
            VehicleDoorsMenu.AddMenuItem(BB);
            VehicleDoorsMenu.AddMenuItem(openAll);
            VehicleDoorsMenu.AddMenuItem(closeAll);
            VehicleDoorsMenu.AddMenuItem(removeDoorList);
            VehicleDoorsMenu.AddMenuItem(deleteDoors);

            VehicleDoorsMenu.OnListItemSelect += (sender, item, index, itemIndex) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists())
                {
                    if (veh.Driver == Game.PlayerPed)
                    {
                        if (item == removeDoorList)
                        {
                            SetVehicleDoorBroken(veh.Handle, index, deleteDoors.Checked);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NeedToBeTheDriver);
                    }
                }
                else
                {
                    Notify.Error(CommonErrors.NoVehicle);
                }
            };

            // Handle button presses.
            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                // Get the vehicle.
                var veh = GetVehicle();
                // If the player is in a vehicle, it's not dead and the player is the driver, continue.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    // If button 0-5 are pressed, then open/close that specific index/door.
                    if (index < 8)
                    {
                        // If the door is open.
                        var open = GetVehicleDoorAngleRatio(veh.Handle, index) > 0.1f;

                        if (open)
                        {
                            // Close the door.
                            SetVehicleDoorShut(veh.Handle, index, false);
                        }
                        else
                        {
                            // Open the door.
                            SetVehicleDoorOpen(veh.Handle, index, false, false);
                        }
                    }
                    // If the index >= 8, and the button is "openAll": open all doors.
                    else if (item == openAll)
                    {
                        // Loop through all doors and open them.
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorOpen(veh.Handle, door, false, false);
                        }
                        if (veh.HasBombBay)
                        {
                            veh.OpenBombBay();
                        }
                    }
                    // If the index >= 8, and the button is "closeAll": close all doors.
                    else if (item == closeAll)
                    {
                        // Close all doors.
                        SetVehicleDoorsShut(veh.Handle, false);
                        if (veh.HasBombBay)
                        {
                            veh.CloseBombBay();
                        }
                    }
                    // If bomb bay doors button is pressed and the vehicle has bomb bay doors.
                    else if (item == BB && veh.HasBombBay)
                    {
                        var bombBayOpen = AreBombBayDoorsOpen(veh.Handle);
                        // If open, close them.
                        if (bombBayOpen)
                        {
                            veh.CloseBombBay();
                        }
                        // Otherwise, open them.
                        else
                        {
                            veh.OpenBombBay();
                        }
                    }
                }
                else
                {
                    Notify.Alert(CommonErrors.NoVehicle, placeholderValue: "打开或关闭车门");
                }
            };

            #endregion

            #region Vehicle Windows Submenu Stuff
            var fwu = new MenuItem("~y~↑~s~ 升起前车窗", "升起前面的车窗.");
            var fwd = new MenuItem("~o~↓~s~ 降下前车窗", "降下前面的车窗.");
            var rwu = new MenuItem("~y~↑~s~ 升起后车窗", "升起后面的车窗.");
            var rwd = new MenuItem("~o~↓~s~ 降下后车窗", "降下后面的车窗.");
            VehicleWindowsMenu.AddMenuItem(fwu);
            VehicleWindowsMenu.AddMenuItem(fwd);
            VehicleWindowsMenu.AddMenuItem(rwu);
            VehicleWindowsMenu.AddMenuItem(rwd);
            VehicleWindowsMenu.OnItemSelect += (sender, item, index) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead)
                {
                    if (item == fwu)
                    {
                        RollUpWindow(veh.Handle, 0);
                        RollUpWindow(veh.Handle, 1);
                    }
                    else if (item == fwd)
                    {
                        RollDownWindow(veh.Handle, 0);
                        RollDownWindow(veh.Handle, 1);
                    }
                    else if (item == rwu)
                    {
                        RollUpWindow(veh.Handle, 2);
                        RollUpWindow(veh.Handle, 3);
                    }
                    else if (item == rwd)
                    {
                        RollDownWindow(veh.Handle, 2);
                        RollDownWindow(veh.Handle, 3);
                    }
                }
            };
            #endregion

            #region Vehicle Liveries Submenu Stuff
            menu.OnItemSelect += (sender, item, idex) =>
            {
                // If the liverys menu button is selected.
                if (item == liveriesMenuBtn)
                {
                    // Get the player's vehicle.
                    var veh = GetVehicle();
                    // If it exists, isn't dead and the player is in the drivers seat continue.
                    if (veh != null && veh.Exists() && !veh.IsDead)
                    {
                        if (veh.Driver == Game.PlayerPed)
                        {
                            VehicleLiveriesMenu.ClearMenuItems();
                            SetVehicleModKit(veh.Handle, 0);
                            var liveryCount = GetVehicleLiveryCount(veh.Handle);

                            if (liveryCount > 0)
                            {
                                var liveryList = new List<string>();
                                for (var i = 0; i < liveryCount; i++)
                                {
                                    var livery = GetLiveryName(veh.Handle, i);
                                    livery = GetLabelText(livery) != "NULL" ? GetLabelText(livery) : $"涂装 #{i}";
                                    liveryList.Add(livery);
                                }
                                var liveryListItem = new MenuListItem("设置涂装", liveryList, GetVehicleLivery(veh.Handle), "为此载具选择一种涂装.");
                                VehicleLiveriesMenu.AddMenuItem(liveryListItem);
                                VehicleLiveriesMenu.OnListIndexChange += (_menu, listItem, oldIndex, newIndex, itemIndex) =>
                                {
                                    if (listItem == liveryListItem)
                                    {
                                        veh = GetVehicle();
                                        SetVehicleLivery(veh.Handle, newIndex);
                                    }
                                };
                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                            else
                            {
                                Notify.Error("该车辆无任何涂装.");
                                VehicleLiveriesMenu.CloseMenu();
                                menu.OpenMenu();
                                var backBtn = new MenuItem("无可用的涂装 :(", "点击我返回.")
                                {
                                    Label = "返回"
                                };
                                VehicleLiveriesMenu.AddMenuItem(backBtn);
                                VehicleLiveriesMenu.OnItemSelect += (sender2, item2, index2) =>
                                {
                                    if (item2 == backBtn)
                                    {
                                        VehicleLiveriesMenu.GoBack();
                                    }
                                };

                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                        }
                        else
                        {
                            Notify.Error("您必须是车辆驾驶员才能访问该菜单.");
                        }
                    }
                    else
                    {
                        Notify.Error("您必须是车辆驾驶员才能访问该菜单.");
                    }
                }
            };
            #endregion

            #region Vehicle Mod Submenu Stuff
            menu.OnItemSelect += (sender, item, index) =>
            {
                // When the mod submenu is openend, reset all items in there.
                if (item == modMenuBtn)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        UpdateMods();
                    }
                    else
                    {
                        VehicleModMenu.CloseMenu();
                        menu.OpenMenu();
                    }

                }
            };
            #endregion

            #region Vehicle Components Submenu
            // when the components menu is opened.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If the components menu is opened.
                if (item == componentsMenuBtn)
                {
                    // Empty the menu in case there were leftover buttons from another vehicle.
                    if (VehicleComponentsMenu.Size > 0)
                    {
                        VehicleComponentsMenu.ClearMenuItems();
                        vehicleExtras.Clear();
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();
                    }

                    // Get the vehicle.
                    var veh = GetVehicle();

                    // Check if the vehicle exists, it's actually a vehicle, it's not dead/broken and the player is in the drivers seat.
                    if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                    {
                        Dictionary<int, string> extraLabels;
                        if (!VehicleExtras.TryGetValue((uint)veh.Model.Hash, out extraLabels))
                        {
                            extraLabels = new Dictionary<int, string>();
                        }
                      
                        //List<int> extraIds = new List<int>();
                        // Loop through all possible extra ID's (AFAIK: 0-14).
                        for (var extra = 0; extra < 14; extra++)
                        {
                            // If this extra exists...
                            if (veh.ExtraExists(extra))
                            {
                                // Add it's ID to the list.
                                //extraIds.Add(extra);

                                // Create the checkbox label
                                string extraLabel;
                                if (!extraLabels.TryGetValue(extra, out extraLabel))
                                    extraLabel = $"改件 #{extra}";
                                // Create a checkbox for it.
                                var extraCheckbox = new MenuCheckboxItem(extraLabel, extra.ToString(), veh.IsExtraOn(extra));
                                // Add the checkbox to the menu.
                                VehicleComponentsMenu.AddMenuItem(extraCheckbox);

                                // Add it's ID to the dictionary.
                                vehicleExtras[extraCheckbox] = extra;
                            }
                        }



                        if (vehicleExtras.Count > 0)
                        {
                            var backBtn = new MenuItem("返回", "返回上一菜单.");
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        else
                        {
                            var backBtn = new MenuItem("无附件/改件 :(", "返回上一菜单.")
                            {
                                Label = "返回"
                            };
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        // And update the submenu to prevent weird glitches.
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();

                    }
                }
            };

            // Disable all extra options if vehicle is too damaged
            VehicleComponentsMenu.OnMenuOpen += (menu) =>
            {
                Vehicle vehicle;
                bool checkDamageBeforeChangingExtras = GetSettingsBool(Setting.vmenu_prevent_extras_when_damaged) && !IsAllowed(Permission.VOBypassExtraDamage);

                if (!checkDamageBeforeChangingExtras || !Entity.Exists(vehicle = GetVehicle()))
                {
                    return;
                }

                List<MenuItem> menuItems = menu.GetMenuItems();
                bool isTooDamaged = IsVehicleTooDamagedToChangeExtras(vehicle);

                menu.ClearMenuItems();

                if (isTooDamaged && !menuItems.Exists(i => i.Text.Contains("too damaged")))
                {
                    MenuItem spacer = GetSpacerMenuItem("Vehicle too damaged!", "Vehicle is too damaged to change extras, repair it first!");

                    // Place at the start of the menu
                    menuItems.Insert(0, spacer);
                }

                foreach (MenuItem item in menuItems)
                {
                    // Check for spacer
                    if (item.Text.Contains("too damaged"))
                    {
                        if (!isTooDamaged)
                        {
                            continue;
                        }
                    }
                    else if (item.Text != "返回")
                    {
                        item.Enabled = !isTooDamaged;
                    }

                    menu.AddMenuItem(item);
                }

                menu.RefreshIndex();
            };

            // when a checkbox in the components menu changes
            VehicleComponentsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // When a checkbox is checked/unchecked, get the selected checkbox item index and use that to get the component ID from the list.
                // Then toggle that extra.
                if (vehicleExtras.TryGetValue(item, out var extra))
                {
                    var veh = GetVehicle();

                    if (!Entity.Exists(veh))
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                        return;
                    }

                    bool checkDamageBeforeChangingExtras = GetSettingsBool(Setting.vmenu_prevent_extras_when_damaged) && !IsAllowed(Permission.VOBypassExtraDamage);

                    if (checkDamageBeforeChangingExtras)
                    {
                        bool isTooDamaged = IsVehicleTooDamagedToChangeExtras(veh);

                        if (isTooDamaged)
                        {
                            // Send message to player when extra change is denied
                            Notify.Alert("Vehicle is too damaged to change extra, repair it first!", true, false);

                            // Send to previous menu
                            VehicleComponentsMenu.GoBack();
                            return;
                        }
                    }

                    veh.ToggleExtra(extra, _checked);
                }
            };
            #endregion

            #region Underglow Submenu
            var underglowFront = new MenuCheckboxItem("启用前灯", "启用或禁用载具前侧的底部灯光.注意,并不是所有载具都有灯光.", false);
            var underglowBack = new MenuCheckboxItem("启用后灯", "启用或禁用载具后侧的底部灯光.注意,并不是所有载具都有灯光.", false);
            var underglowLeft = new MenuCheckboxItem("启用左灯", "启用或禁用载具左侧的底部灯光.注意,并不是所有载具都有灯光.", false);
            var underglowRight = new MenuCheckboxItem("启用右灯", "启用或禁用载具右侧的底部灯光.注意,并不是所有载具都有灯光.", false);
            var underglowColorsList = new List<string>();
            for (var i = 0; i < 13; i++)
            {
                underglowColorsList.Add(GetLabelText($"CMOD_NEONCOL_{i}"));
            }
            var underglowColor = new MenuListItem(GetLabelText("CMOD_NEON_1"), underglowColorsList, 0, "选择底盘霓虹灯颜色。");

            VehicleUnderglowMenu.AddMenuItem(underglowFront);
            VehicleUnderglowMenu.AddMenuItem(underglowBack);
            VehicleUnderglowMenu.AddMenuItem(underglowLeft);
            VehicleUnderglowMenu.AddMenuItem(underglowRight);

            VehicleUnderglowMenu.AddMenuItem(underglowColor);

            CreateCustomColourMenu(VehicleUnderglowMenu, RGBType.underglow);
            menu.OnItemSelect += (sender, item, index) =>
            {
                #region reset checkboxes state when opening the menu.
                if (item == underglowMenuBtn)
                {
                    var veh = GetVehicle();
                    if (veh != null)
                    {
                        if (veh.Mods.HasNeonLights)
                        {
                            underglowFront.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Front) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front);
                            underglowBack.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Back) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back);
                            underglowLeft.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Left) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left);
                            underglowRight.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Right) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right);

                            underglowFront.Enabled = true;
                            underglowBack.Enabled = true;
                            underglowLeft.Enabled = true;
                            underglowRight.Enabled = true;

                            underglowFront.LeftIcon = MenuItem.Icon.NONE;
                            underglowBack.LeftIcon = MenuItem.Icon.NONE;
                            underglowLeft.LeftIcon = MenuItem.Icon.NONE;
                            underglowRight.LeftIcon = MenuItem.Icon.NONE;
                        }
                        else
                        {
                            underglowFront.Checked = false;
                            underglowBack.Checked = false;
                            underglowLeft.Checked = false;
                            underglowRight.Checked = false;

                            underglowFront.Enabled = false;
                            underglowBack.Enabled = false;
                            underglowLeft.Enabled = false;
                            underglowRight.Enabled = false;

                            underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                            underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                            underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                            underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                        }
                    }
                    else
                    {
                        underglowFront.Checked = false;
                        underglowBack.Checked = false;
                        underglowLeft.Checked = false;
                        underglowRight.Checked = false;

                        underglowFront.Enabled = false;
                        underglowBack.Enabled = false;
                        underglowLeft.Enabled = false;
                        underglowRight.Enabled = false;

                        underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                        underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                        underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                        underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                    }

                    underglowColor.ListIndex = GetIndexFromColor();
                }
                #endregion
            };
            // handle item selections
            VehicleUnderglowMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    var veh = GetVehicle();
                    if (veh.Mods.HasNeonLights)
                    {
                        veh.Mods.NeonLightsColor = GetColorFromIndex(underglowColor.ListIndex);
                        if (item == underglowLeft)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Left, veh.Mods.HasNeonLight(VehicleNeonLight.Left) && _checked);
                        }
                        else if (item == underglowRight)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Right, veh.Mods.HasNeonLight(VehicleNeonLight.Right) && _checked);
                        }
                        else if (item == underglowBack)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Back, veh.Mods.HasNeonLight(VehicleNeonLight.Back) && _checked);
                        }
                        else if (item == underglowFront)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Front, veh.Mods.HasNeonLight(VehicleNeonLight.Front) && _checked);
                        }
                    }
                }
            };

            VehicleUnderglowMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == underglowColor)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        if (veh.Mods.HasNeonLights)
                        {
                            veh.Mods.NeonLightsColor = GetColorFromIndex(newIndex);
                        }
                    }
                }
            };
            #endregion

            #region Handle menu-opening refreshing license plate
            menu.OnMenuOpen += (sender) =>
            {
                menu.GetMenuItems().ForEach((item) =>
                {
                    var veh = GetVehicle(true);

                    if (item == setLicensePlateType && item is MenuListItem listItem && veh != null && veh.Exists())
                    {
                        // Set the license plate style.
                        switch (veh.Mods.LicensePlateStyle)
                        {
                            case LicensePlateStyle.BlueOnWhite1:
                                listItem.ListIndex = 0;
                                break;
                            case LicensePlateStyle.BlueOnWhite2:
                                listItem.ListIndex = 1;
                                break;
                            case LicensePlateStyle.BlueOnWhite3:
                                listItem.ListIndex = 2;
                                break;
                            case LicensePlateStyle.YellowOnBlue:
                                listItem.ListIndex = 3;
                                break;
                            case LicensePlateStyle.YellowOnBlack:
                                listItem.ListIndex = 4;
                                break;
                            case LicensePlateStyle.NorthYankton:
                                listItem.ListIndex = 5;
                                break;
                            case LicensePlateStyle.ECola:
                                listItem.ListIndex = 6;
                                break;
                            case LicensePlateStyle.LasVenturas:
                                listItem.ListIndex = 7;
                                break;
                            case LicensePlateStyle.LibertyCity:
                                listItem.ListIndex = 8;
                                break;
                            case LicensePlateStyle.LSCarMeet:
                                listItem.ListIndex = 9;
                                break;
                            case LicensePlateStyle.LSPanic:
                                listItem.ListIndex = 10;
                                break;
                            case LicensePlateStyle.LSPounders:
                                listItem.ListIndex = 11;
                                break;
                            case LicensePlateStyle.Sprunk:
                                listItem.ListIndex = 12;
                                break;
                            default:
                                break;
                        }
                    }
                });
            };
            #endregion

        }
        #endregion

        /// <summary>
        /// Public get method for the menu. Checks if the menu exists, if not create the menu first.
        /// </summary>
        /// <returns>Returns the Vehicle Options menu.</returns>
        public Menu GetMenu()
        {
            // If menu doesn't exist. Create one.
            if (menu == null)
            {
                CreateMenu();
            }
            // Return the menu.
            return menu;
        }

        #region Update Vehicle Mods Menu
        /// <summary>
        /// Refreshes the mods page. The selectedIndex allows you to go straight to a specific index after refreshing the menu.
        /// This is used because when the wheel type is changed, the menu is refreshed to update the available wheels list.
        /// </summary>
        /// <param name="selectedIndex">Pass this if you want to go straight to a specific mod/index.</param>
        public void UpdateMods(int selectedIndex = 0)
        {
            // If there are items, remove all of them.
            if (VehicleModMenu.Size > 0)
            {
                if (selectedIndex != 0)
                {
                    VehicleModMenu.ClearMenuItems(true);
                }
                else
                {
                    VehicleModMenu.ClearMenuItems(false);
                }

            }

            // Get the vehicle.
            var veh = GetVehicle();

            // Check if the vehicle exists, is still drivable/alive and it's actually a vehicle.
            if (veh != null && veh.Exists() && !veh.IsDead)
            {
                #region initial setup & dynamic vehicle mods setup
                // Set the modkit so we can modify the car.
                SetVehicleModKit(veh.Handle, 0);

                // Get all mods available on this vehicle.
                var mods = GetAllVehicleMods(veh);

                // Loop through all the mods.
                foreach (var mod in mods)
                {
                    veh = GetVehicle();

                    // Get the proper localized mod type (suspension, armor, etc) name.
                    var typeName = mod.LocalizedModTypeName;

                    // Create a list to all available upgrades for this modtype.
                    var modlist = new List<string>();

                    // Get the current item index ({current}/{max upgrades})
                    var currentItem = $"[1/{mod.ModCount + 1}]";

                    // Add the stock value for this mod.
                    var name = $"Stock {typeName} {currentItem}";
                    modlist.Add(name);

                    // Loop through all available upgrades for this specific mod type.
                    for (var x = 0; x < mod.ModCount; x++)
                    {
                        // Create the item index.
                        currentItem = $"[{2 + x}/{mod.ModCount + 1}]";

                        // Create the name (again, converting to proper case), then add the name.
                        name = mod.GetLocalizedModName(x) != "" ? $"{ToProperString(mod.GetLocalizedModName(x))} {currentItem}" : $"{typeName} #{x} {currentItem}";
                        modlist.Add(name);
                    }

                    // Create the MenuListItem for this mod type.
                    var currIndex = GetVehicleMod(veh.Handle, (int)mod.ModType) + 1;
                    var modTypeListItem = new MenuListItem(
                        typeName,
                        modlist,
                        currIndex,
                        $"对 ~y~{typeName}~s~ 进行升级, 它将自动应用于您的载具."
                    )
                    {
                        ItemData = (int)mod.ModType
                    };

                    // Add the list item to the menu.
                    VehicleModMenu.AddMenuItem(modTypeListItem);
                }
                #endregion

                #region more variables and setup
                veh = GetVehicle();
                // Create the wheel types list & listitem and add it to the menu.
                var wheelTypes = new List<string>()
                {
                    "运动",       // 0
                    "肌肉",       // 1
                    "低底盘",     // 2
                    "SUV",          // 3
                    "Offroad",      // 4
                    "Tuner",        // 5
                    "Bike Wheels",  // 6
                    "High End",     // 7
                    "Benny's (1)",  // 8
                    "Benny's (2)",  // 9
                    "Open Wheel",   // 10
                    "街头",       // 11
                    "Track"         // 12    
                };
                var vehicleWheelType = new MenuListItem("轮毂类型", wheelTypes, MathUtil.Clamp(GetVehicleWheelType(veh.Handle), 0, 12), $"选择适合您载具的~y~轮毂类型.");
                if (!veh.Model.IsBoat && !veh.Model.IsHelicopter && !veh.Model.IsPlane && !veh.Model.IsBicycle && !veh.Model.IsTrain)
                {
                    VehicleModMenu.AddMenuItem(vehicleWheelType);
                }

                var headlightsButton = new MenuItem("车头灯");
                var headlightsMenu = new Menu("车头灯", "车头灯设置");
                var xenonHeadlights = new MenuCheckboxItem("氙气大灯", "启用或禁用~b~氙气大灯~s~.", IsToggleModOn(veh.Handle, 22));
                headlightsMenu.AddMenuItem(xenonHeadlights);
                var currentHeadlightColor = GetHeadlightsColorForVehicle(veh);
                if (currentHeadlightColor is < 0 or > 12)
                {
                    currentHeadlightColor = 13;
                }
                var headlightColor = new MenuListItem("大灯配色", new List<string>() { "白色", "蓝色", "电蓝", "薄荷绿", "酸橙绿", "黄色", "喷射金", "橙色", "红色", "小马粉", "艳粉色", "紫色", "黑光", "默认" }, currentHeadlightColor, "在GTA V的DLC更新中新增：彩色大灯. 请注意, 您必须先启用氙气大灯.");
                headlightsMenu.AddMenuItem(headlightColor);
                headlightsMenu.OnCheckboxChange += (sender2, item2, index2, _checked) =>
                {
                    veh = GetVehicle();

                    // Xenon Headlights
                    if (item2 == xenonHeadlights)
                    {
                        ToggleVehicleMod(veh.Handle, 22, _checked);
                    }
                };
                headlightsMenu.OnListIndexChange += (sender2, item2, oldIndex, newIndex, itemIndex) =>
                {
                    veh = GetVehicle();
                    if (item2 == headlightColor)
                    {
                        if (newIndex == 13) // default
                        {
                            SetHeadlightsColorForVehicle(veh, 255);
                        }
                        else if (newIndex is > (-1) and < 13)
                        {
                            ClearVehicleXenonLightsCustomColor(veh.Handle);
                            SetHeadlightsColorForVehicle(veh, newIndex);
                        }
                    }
                };
                MenuController.AddSubmenu(VehicleModMenu, headlightsMenu);
                MenuController.BindMenuItem(VehicleModMenu, headlightsMenu, headlightsButton);
                VehicleModMenu.AddMenuItem(headlightsButton);
                CreateCustomColourMenu(headlightsMenu, RGBType.headlight);


                // Create the checkboxes for some options.
                var toggleCustomWheels = new MenuCheckboxItem("定制轮毂开关", "按下此键可添加或删除~y~定制轮毂~s~.", GetVehicleModVariation(veh.Handle, 23));
                var turbo = new MenuCheckboxItem("涡轮增压", "启用或禁用该载具的~y~涡轮增压~s.", IsToggleModOn(veh.Handle, 18));
                var bulletProofTires = new MenuCheckboxItem("防弹轮胎", "启用或禁用此载具的防弹轮胎.", !GetVehicleTyresCanBurst(veh.Handle));

            

                // Add the checkboxes to the menu.
                VehicleModMenu.AddMenuItem(toggleCustomWheels);
                VehicleModMenu.AddMenuItem(turbo);
                VehicleModMenu.AddMenuItem(bulletProofTires);

                bool isLowGripAvailable = GetGameBuildNumber() >= 2372;
                var lowGripTires = new MenuCheckboxItem("降低轮胎抓地力", "启用或禁用 ~y~ 降低轮胎抓地力 ~s~s.", isLowGripAvailable ? GetDriftTyresEnabled(veh.Handle) : false);
                if (isLowGripAvailable)
                {
                    VehicleModMenu.AddMenuItem(lowGripTires);
                }

                var tireSmokeButton = new MenuItem("轮胎烟雾");
                var tireSmokeMenu = new Menu("轮胎烟雾", "轮胎烟雾");
                // Create a list of tire smoke options.
                var tireSmokes = new List<string>() { "红色", "橙色", "黄色", "金色", "浅绿色", "深绿色", "浅蓝色", "深蓝色", "紫色", "粉色", "黑色" };
                var tireSmokeColors = new Dictionary<string, int[]>()
                {
                    ["红色"] = new int[] { 244, 65, 65 },
                    ["橙色"] = new int[] { 244, 167, 66 },
                    ["黄色"] = new int[] { 244, 217, 65 },
                    ["金色"] = new int[] { 181, 120, 0 },
                    ["浅绿色"] = new int[] { 158, 255, 84 },
                    ["深绿色"] = new int[] { 44, 94, 5 },
                    ["浅蓝色"] = new int[] { 65, 211, 244 },
                    ["深蓝色"] = new int[] { 24, 54, 163 },
                    ["紫色"] = new int[] { 108, 24, 192 },
                    ["粉色"] = new int[] { 192, 24, 172 },
                    ["黑色"] = new int[] { 1, 1, 1 }
                };
                int smoker = 0, smokeg = 0, smokeb = 0;
                GetVehicleTyreSmokeColor(veh.Handle, ref smoker, ref smokeg, ref smokeb);
                var item = tireSmokeColors.ToList().Find((f) => { return f.Value[0] == smoker && f.Value[1] == smokeg && f.Value[2] == smokeb; });
                var index = tireSmokeColors.ToList().IndexOf(item);
                if (index < 0)
                {
                    index = 0;
                }

                var tireSmoke = new MenuListItem("胎烟颜色", tireSmokes, index, $"选择适合您载具的~y ~烧胎胎烟颜色 ~s ~.");
                tireSmokeMenu.AddMenuItem(tireSmoke);

                // Create the checkbox to enable/disable the tiresmoke.
                var tireSmokeEnabled = new MenuCheckboxItem("轮胎烟雾", "启用或禁用载具的 ~y~轮胎烟雾~s~ 功能. ~h~~r~提醒:~s~ 禁用轮胎烟雾时, 您需要在其生效前驾驶一圈.", IsToggleModOn(veh.Handle, 20));
                tireSmokeMenu.AddMenuItem(tireSmokeEnabled);


                tireSmokeMenu.OnCheckboxChange += (sender2, item2, index2, _checked) =>
                {
                    veh = GetVehicle();

                    // Toggle Tire Smoke
                    if (item2 == tireSmokeEnabled)
                    {
                        // If it should be enabled:
                        if (_checked)
                        {
                            // Enable it.
                            ToggleVehicleMod(veh.Handle, 20, true);
                            // Get the selected color values.
                            var r = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][0];
                            var g = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][1];
                            var b = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][2];
                            // Set the color.
                            SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                        }
                        // If it should be disabled:
                        else
                        {
                            // Set the smoke to white.
                            SetVehicleTyreSmokeColor(veh.Handle, 255, 255, 255);
                            // Disable it.
                            ToggleVehicleMod(veh.Handle, 20, false);
                            // Remove the mod.
                            RemoveVehicleMod(veh.Handle, 20);
                        }
                    }
                };
                tireSmokeMenu.OnListIndexChange += (sender2, item2, oldIndex, newIndex, itemIndex) =>
                {
                    // Get the vehicle and set the mod kit.
                    veh = GetVehicle();
                    SetVehicleModKit(veh.Handle, 0);

                    // Tire smoke
                    if (item2 == tireSmoke)
                    {
                        // Get the selected color values.
                        var r = tireSmokeColors[tireSmokes[newIndex]][0];
                        var g = tireSmokeColors[tireSmokes[newIndex]][1];
                        var b = tireSmokeColors[tireSmokes[newIndex]][2];

                        // Set the color.
                        SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                    }
                };

                MenuController.AddSubmenu(VehicleModMenu, tireSmokeMenu);
                MenuController.BindMenuItem(VehicleModMenu, tireSmokeMenu, tireSmokeButton);
                VehicleModMenu.AddMenuItem(tireSmokeButton);
                CreateCustomColourMenu(tireSmokeMenu, RGBType.tiresmoke);

                // Create list for window tint
                var windowTints = new List<string>() { "原厂 [1/7]", "无色 [2/7]", "豪华 [3/7]", "浅灰 [4/7]", "深灰 [5/7]", "纯黑 [6/7]", "绿色 [7/7]" };
                var currentTint = GetVehicleWindowTint(veh.Handle);
                if (currentTint == -1)
                {
                    currentTint = 4; // stock
                }

                // Convert window tint to the correct index of the list above.
                switch (currentTint)
                {
                    case 0:
                        currentTint = 1; // None
                        break;
                    case 1:
                        currentTint = 5; // Pure Black
                        break;
                    case 2:
                        currentTint = 4; // Dark Smoke
                        break;
                    case 3:
                        currentTint = 3; // Light Smoke
                        break;
                    case 4:
                        currentTint = 0; // Stock
                        break;
                    case 5:
                        currentTint = 2; // Limo
                        break;
                    case 6:
                        currentTint = 6; // Green
                        break;
                    default:
                        break;
                }

                var windowTint = new MenuListItem("车窗贴膜", windowTints, currentTint, "为车窗应用不同颜色玻璃膜.");
                VehicleModMenu.AddMenuItem(windowTint);

                #endregion

                #region Checkbox Changes
                // Handle checkbox changes.
                VehicleModMenu.OnCheckboxChange += (sender2, item2, index2, _checked) =>
                {
                    veh = GetVehicle();

                    // Turbo
                    if (item2 == turbo)
                    {
                        ToggleVehicleMod(veh.Handle, 18, _checked);
                    }
                    // Bullet Proof Tires
                    else if (item2 == bulletProofTires)
                    {
                        SetVehicleTyresCanBurst(veh.Handle, !_checked);
                    }
                    // Low Grip Tyres
                    else if (item2 == lowGripTires)
                    {
                        SetDriftTyresEnabled(veh.Handle, _checked);
                    }
                    // Custom Wheels
                    else if (item2 == toggleCustomWheels)
                    {
                        SetVehicleMod(veh.Handle, 23, GetVehicleMod(veh.Handle, 23), !GetVehicleModVariation(veh.Handle, 23));

                        // If the player is on a motorcycle, also change the back wheels.
                        if (IsThisModelABike((uint)GetEntityModel(veh.Handle)))
                        {
                            SetVehicleMod(veh.Handle, 24, GetVehicleMod(veh.Handle, 24), GetVehicleModVariation(veh.Handle, 23));
                        }
                    }
                };
                #endregion

                #region List Changes
                // Handle list selections
                VehicleModMenu.OnListIndexChange += (sender2, item2, oldIndex, newIndex, itemIndex) =>
                {
                    // Get the vehicle and set the mod kit.
                    veh = GetVehicle();
                    SetVehicleModKit(veh.Handle, 0);

                    #region handle the dynamic (vehicle-specific) mods
                    // If the affected list is actually a "dynamically" generated list, continue. If it was one of the manual options, go to else.
                    if (item2.ItemData is int modType)
                    {
                        var selectedUpgrade = item2.ListIndex - 1;
                        var customWheels = GetVehicleModVariation(veh.Handle, 23);

                        SetVehicleMod(veh.Handle, modType, selectedUpgrade, customWheels);
                    }
                    #endregion
                    // If it was not one of the lists above, then it was one of the manual lists/options selected, 
                    // either: vehicle Wheel Type, or window tint:
                    #region Handle the items available on all vehicles.
                    // Wheel types
                    else if (item2 == vehicleWheelType)
                    {
                        var vehicleClass = GetVehicleClass(veh.Handle);
                        var isBikeOrOpenWheel = (newIndex == 6 && veh.Model.IsBike) || (newIndex == 10 && vehicleClass == 22);
                        var isNotBikeNorOpenWheel = newIndex != 6 && !veh.Model.IsBike && newIndex != 10 && vehicleClass != 22;
                        var isCorrectVehicleType = isBikeOrOpenWheel || isNotBikeNorOpenWheel;
                        if (!isCorrectVehicleType)
                        {
                            // Go past the index if it's not a bike.
                            if (!veh.Model.IsBike && vehicleClass != 22)
                            {
                                if (newIndex > oldIndex)
                                {
                                    item2.ListIndex++;
                                }
                                else
                                {
                                    item2.ListIndex--;
                                }
                            }
                            // Reset the index to 6 if it is a bike
                            else
                            {
                                item2.ListIndex = veh.Model.IsBike ? 6 : 10;
                            }
                        }
                        // Set the wheel type
                        SetVehicleWheelType(veh.Handle, item2.ListIndex);

                        var customWheels = GetVehicleModVariation(veh.Handle, 23);

                        // Reset the wheel mod index for front wheels
                        SetVehicleMod(veh.Handle, 23, -1, customWheels);

                        // If the model is a bike, do the same thing for the rear wheels.
                        if (veh.Model.IsBike)
                        {
                            SetVehicleMod(veh.Handle, 24, -1, customWheels);
                        }

                        // Refresh the menu with the item index so that the view doesn't change
                        UpdateMods(selectedIndex: itemIndex);
                    }
                    // Window Tint
                    else if (item2 == windowTint)
                    {
                        // Stock = 4,
                        // None = 0,
                        // Limo = 5,
                        // LightSmoke = 3,
                        // DarkSmoke = 2,
                        // PureBlack = 1,
                        // Green = 6,

                        switch (newIndex)
                        {
                            case 1:
                                SetVehicleWindowTint(veh.Handle, 0); // None
                                break;
                            case 2:
                                SetVehicleWindowTint(veh.Handle, 5); // Limo
                                break;
                            case 3:
                                SetVehicleWindowTint(veh.Handle, 3); // Light Smoke
                                break;
                            case 4:
                                SetVehicleWindowTint(veh.Handle, 2); // Dark Smoke
                                break;
                            case 5:
                                SetVehicleWindowTint(veh.Handle, 1); // Pure Black
                                break;
                            case 6:
                                SetVehicleWindowTint(veh.Handle, 6); // Green
                                break;
                            case 0:
                            default:
                                SetVehicleWindowTint(veh.Handle, 4); // Stock
                                break;
                        }
                    }
                    #endregion
                };

                #endregion
            }
            // Refresh Index and update the scaleform to prevent weird broken menus.
            if (selectedIndex == 0)
            {
                VehicleModMenu.RefreshIndex();
            }

            //VehicleModMenu.UpdateScaleform();

            // Set the selected index to the provided index (0 by default)
            // Used for example, when the wheelstype is changed, the menu is refreshed and we want to set the
            // selected item back to the "wheelsType" list so the user doesn't have to scroll down each time they
            // change the wheels type.
            //VehicleModMenu.CurrentIndex = selectedIndex;
        }

        internal static void SetHeadlightsColorForVehicle(Vehicle veh, int newIndex)
        {

            if (veh != null && veh.Exists() && veh.Driver == Game.PlayerPed)
            {
                if (newIndex is > (-1) and < 13)
                {
                    SetVehicleHeadlightsColour(veh.Handle, newIndex);
                }
                else
                {
                    SetVehicleHeadlightsColour(veh.Handle, -1);
                }
            }
        }

        internal static int GetHeadlightsColorForVehicle(Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Exists())
            {
                if (IsToggleModOn(vehicle.Handle, 22))
                {
                    var val = GetVehicleHeadlightsColour(vehicle.Handle);
                    if (val is > (-1) and < 13)
                    {
                        return val;
                    }
                    return -1;
                }
            }
            return -1;
        }
        #endregion

        #region GetColorFromIndex function (underglow)
        /// <summary>
        /// Converts a list index to a <see cref="System.Drawing.Color"/> struct.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private static System.Drawing.Color GetColorFromIndex(int index)
        {
            if (index is >= 0 and < 13)
            {
                return System.Drawing.Color.FromArgb(VehicleData.NeonLightColors[index][0], VehicleData.NeonLightColors[index][1], VehicleData.NeonLightColors[index][2]);
            }
            return System.Drawing.Color.FromArgb(255, 255, 255);
        }
        public enum Colour_Types
        {
            Metallic = 0,
            Classic,
            Pearlescent,
            Matte,
            Metals,
            Chrome,
            变色龙漆面
        }
        private static bool IsHex(IEnumerable<char> chars)
        {
            bool isHex;
            foreach (var c in chars)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }
        public enum RGBType
        {
            primaryPaint,
            secondaryPaint,
            underglow,
            headlight,
            tiresmoke,

        }

        public static void CreateCustomColourMenu(Menu menu, RGBType type = RGBType.primaryPaint)
        {
            
            var hexColour = new MenuItem("十六进制颜色代码");
            var typeList = new MenuListItem("漆面效果", Enum.GetNames(typeof(Colour_Types)).ToList(), 0);
            
            int r = 0,  g = 0, b = 0;     

            var redColour = new MenuSliderItem("红色数值", 0, 255, 128, true)
            {
                BarColor = System.Drawing.Color.FromArgb(155, 0, 0, 0),
                BackgroundColor = System.Drawing.Color.FromArgb(200, 79, 79, 79),
            };
            var greenColour = new MenuSliderItem("绿色数值", 0, 255, 128, true)
            {
                BarColor = System.Drawing.Color.FromArgb(155, 0, 0, 0),
                BackgroundColor = System.Drawing.Color.FromArgb(200, 79, 79, 79),
            };
            var blueColour = new MenuSliderItem("蓝色数值", 0, 255, 128, true)
            {
                BarColor = System.Drawing.Color.FromArgb(155, 0, 0, 0),
                BackgroundColor = System.Drawing.Color.FromArgb(200, 79, 79, 79),
            };

            menu.AddMenuItem(hexColour);
            menu.AddMenuItem(redColour);
            menu.AddMenuItem(greenColour);
            menu.AddMenuItem(blueColour);
            if (type == RGBType.primaryPaint || type == RGBType.secondaryPaint)
                menu.AddMenuItem(typeList);

            menu.OnMenuOpen += (menu) =>
            {
                Vehicle vehicle = GetVehicle();

                if (type == RGBType.primaryPaint)
                    GetVehicleCustomPrimaryColour(vehicle.Handle, ref r, ref g, ref b);
                else if (type == RGBType.secondaryPaint)
                    GetVehicleCustomSecondaryColour(vehicle.Handle, ref r, ref g, ref b);
                else if (type == RGBType.underglow)
                {
                    r = vehicle.Mods.NeonLightsColor.R;
                    g = vehicle.Mods.NeonLightsColor.G;
                    b = vehicle.Mods.NeonLightsColor.B; 
                }
                else if (type == RGBType.headlight)
                {
                    if (!GetVehicleXenonLightsCustomColor(vehicle.Handle, ref r, ref g, ref b))
                    {
                        if (IsToggleModOn(vehicle.Handle, 22))
                        {
                            int headlight = GetHeadlightsColorForVehicle(vehicle) < 0 ? 0 : GetHeadlightsColorForVehicle(vehicle);
                            r = VehicleData.NeonLightColors[headlight][0];
                            g = VehicleData.NeonLightColors[headlight][1];
                            b = VehicleData.NeonLightColors[headlight][2]; 
                        }
                    }

                }
                else if (type == RGBType.tiresmoke)
                    GetVehicleTyreSmokeColor(vehicle.Handle, ref r, ref g, ref b);


                redColour.Text = $"红色数值 ({r})";
                redColour.Position = r;

                greenColour.Text = $"绿色数值 ({g})";
                greenColour.Position = g;

                blueColour.Text = $"蓝色数值 ({b})";
                blueColour.Position = b;

                redColour.BarColor = System.Drawing.Color.FromArgb(255, redColour.Position, greenColour.Position, blueColour.Position);
                greenColour.BarColor = System.Drawing.Color.FromArgb(255, redColour.Position, greenColour.Position, blueColour.Position);
                blueColour.BarColor = System.Drawing.Color.FromArgb(255, redColour.Position, greenColour.Position, blueColour.Position);
            };

            menu.OnItemSelect += async (sender, item, index) =>
            {
                Vehicle vehicle = GetVehicle();
                if (item == hexColour)
                {
                    string hexValue = redColour.Position.ToString("X2") + greenColour.Position.ToString("X2") + blueColour.Position.ToString("X2");
                    var result = await GetUserInput(windowTitle: "Enter Color Hex", defaultText: (hexValue).Replace("#", ""), maxInputLength: 6);
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (IsHex(result))
                        {
                            int RGBint = Convert.ToInt32(result, 16);
                            byte red = (byte)((RGBint >> 16) & 255);
                            byte green = (byte)((RGBint >> 8) & 255);
                            byte blue = (byte)(RGBint & 255);

                            if (type == RGBType.primaryPaint)
                                SetVehicleCustomPrimaryColour(vehicle.Handle, red, green, blue);
                            else if (type == RGBType.secondaryPaint)
                                SetVehicleCustomSecondaryColour(vehicle.Handle, red, green, blue);
                            else if (type == RGBType.underglow)
                                vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(255, red, green, blue);
                            else if (type == RGBType.headlight)
                                SetVehicleXenonLightsCustomColor(vehicle.Handle, red, green, blue);
                            else if (type == RGBType.tiresmoke)
                                SetVehicleTyreSmokeColor(vehicle.Handle, red, green, blue);

                            redColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);
                            greenColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);
                            blueColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);

                            redColour.Text = $"红色数值 ({red})";
                            redColour.Position = red;

                            greenColour.Text = $"绿色数值 ({green})";
                            greenColour.Position = green;

                            blueColour.Text = $"蓝色数值 ({blue})";
                            blueColour.Position = blue;
                        }
                    }
                }
            };
            menu.OnSliderPositionChange += (m, sliderItem, oldPosition, newPosition, itemIndex) =>
            {
                Vehicle vehicle = GetVehicle();

                if (type == RGBType.primaryPaint)
                    GetVehicleCustomPrimaryColour(vehicle.Handle, ref r, ref g, ref b);
                else if (type == RGBType.secondaryPaint)
                    GetVehicleCustomSecondaryColour(vehicle.Handle, ref r, ref g, ref b);
                else if (type == RGBType.underglow)
                {
                    r = vehicle.Mods.NeonLightsColor.R;
                    g = vehicle.Mods.NeonLightsColor.G;
                    b = vehicle.Mods.NeonLightsColor.B; 
                }
                else if (type == RGBType.headlight)
                {
                    if (!GetVehicleXenonLightsCustomColor(vehicle.Handle, ref r, ref g, ref b))
                    {
                        if (IsToggleModOn(vehicle.Handle, 22))
                        {
                            int headlight = GetHeadlightsColorForVehicle(vehicle) < 0 ? 0 : GetHeadlightsColorForVehicle(vehicle);
                            r = VehicleData.NeonLightColors[headlight][0];
                            g = VehicleData.NeonLightColors[headlight][1];
                            b = VehicleData.NeonLightColors[headlight][2]; 
                        }
                    }

                }
                else if (type == RGBType.tiresmoke)
                    GetVehicleTyreSmokeColor(vehicle.Handle, ref r, ref g, ref b);


                if (sliderItem == redColour)
                {
                    if (type == RGBType.primaryPaint)
                        SetVehicleCustomPrimaryColour(vehicle.Handle, newPosition, g, b);
                    else if (type == RGBType.secondaryPaint)
                        SetVehicleCustomSecondaryColour(vehicle.Handle, newPosition, g, b);
                    else if (type == RGBType.underglow)
                        vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(255, newPosition, g, b);
                    else if (type == RGBType.headlight)
                        SetVehicleXenonLightsCustomColor(vehicle.Handle, newPosition, g, b);
                    else if (type == RGBType.tiresmoke)
                        vehicle.Mods.TireSmokeColor = System.Drawing.Color.FromArgb(255, newPosition, g, b);

                    redColour.Text = $"红色数值 ({newPosition})";
                }
                if (sliderItem == greenColour)
                {
                    if (type == RGBType.primaryPaint)
                        SetVehicleCustomPrimaryColour(vehicle.Handle, r, newPosition, b);
                    else if (type == RGBType.secondaryPaint)
                        SetVehicleCustomSecondaryColour(vehicle.Handle, r, newPosition, b);
                    else if (type == RGBType.underglow)
                        vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(255, r, newPosition, b);
                    else if (type == RGBType.headlight)
                        SetVehicleXenonLightsCustomColor(vehicle.Handle, r, newPosition, b);
                    else if (type == RGBType.tiresmoke)
                        vehicle.Mods.TireSmokeColor = System.Drawing.Color.FromArgb(255, r, newPosition, b);

                    greenColour.Text = $"绿色数值 ({newPosition})";
                }
                if (sliderItem == blueColour)
                {
                     if (type == RGBType.primaryPaint)
                        SetVehicleCustomPrimaryColour(vehicle.Handle, r, g, newPosition);
                    else if (type == RGBType.secondaryPaint)
                        SetVehicleCustomSecondaryColour(vehicle.Handle, r, g, newPosition);
                    else if (type == RGBType.underglow)
                        vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(255, r, g, newPosition);
                    else if (type == RGBType.headlight)
                        SetVehicleXenonLightsCustomColor(vehicle.Handle, r, g, newPosition);
                    else if (type == RGBType.tiresmoke)
                        vehicle.Mods.TireSmokeColor = System.Drawing.Color.FromArgb(255, r, g, newPosition);

                    blueColour.Text = $"蓝色数值 ({newPosition})";
                }
                redColour.BarColor = System.Drawing.Color.FromArgb(255, redColour.Position, greenColour.Position, blueColour.Position);
                greenColour.BarColor = System.Drawing.Color.FromArgb(255, redColour.Position, greenColour.Position, blueColour.Position);
                blueColour.BarColor = System.Drawing.Color.FromArgb(255, redColour.Position, greenColour.Position, blueColour.Position);
            };

            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                Vehicle vehicle = GetVehicle();

                if (type == RGBType.primaryPaint)
                {
                    var pearlColorReset = 0;
                    var wheelColorReset = 0;
                    GetVehicleExtraColours(vehicle.Handle, ref pearlColorReset, ref wheelColorReset);
                    SetVehicleModColor_1(vehicle.Handle, newIndex, 0, 0);
                    vehicle.State.Set("vMenu:PrimaryPaintFinish", newIndex, true);
                    SetVehicleExtraColours(vehicle.Handle, pearlColorReset, wheelColorReset);
                }
                else if (type == RGBType.secondaryPaint)
                {
                    var pearlColorReset = 0;
                    var wheelColorReset = 0;
                    GetVehicleExtraColours(vehicle.Handle, ref pearlColorReset, ref wheelColorReset);
                    SetVehicleModColor_2(vehicle.Handle, newIndex, 0);
                    vehicle.State.Set("vMenu:SecondaryPaintFinish", newIndex, true);
                    SetVehicleExtraColours(vehicle.Handle, pearlColorReset, wheelColorReset);
                }
                else if (type == RGBType.underglow)
                {
                    Color underglow = GetColorFromIndex(newIndex);
                    int red = underglow.R;
                    int green = underglow.G;
                    int blue = underglow.B;
                    
                    redColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);
                    greenColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);
                    blueColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);

                    redColour.Text = $"红色数值 ({red})";
                    redColour.Position = red;

                    greenColour.Text = $"绿色数值 ({green})";
                    greenColour.Position = green;

                    blueColour.Text = $"蓝色数值 ({blue})";
                    blueColour.Position = blue; 
                }
                else if (type == RGBType.headlight)
                {
                    int headlight = GetHeadlightsColorForVehicle(vehicle) < 0 ? 0 : GetHeadlightsColorForVehicle(vehicle);
                    int red = VehicleData.NeonLightColors[headlight][0];
                    int green = VehicleData.NeonLightColors[headlight][1];
                    int blue = VehicleData.NeonLightColors[headlight][2];
                    

                    redColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);
                    greenColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);
                    blueColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);

                    redColour.Text = $"红色数值 ({red})";
                    redColour.Position = red;

                    greenColour.Text = $"绿色数值 ({green})";
                    greenColour.Position = green;

                    blueColour.Text = $"蓝色数值 ({blue})";
                    blueColour.Position = blue; 
                }
                else if (type == RGBType.tiresmoke)
                {
                    int red = vehicle.Mods.TireSmokeColor.R;
                    int green = vehicle.Mods.TireSmokeColor.G;
                    int blue = vehicle.Mods.TireSmokeColor.B; 
                    
                    redColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);
                    greenColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);
                    blueColour.BarColor = System.Drawing.Color.FromArgb(255, red, green, blue);

                    redColour.Text = $"红色数值 ({red})";
                    redColour.Position = red;

                    greenColour.Text = $"绿色数值 ({green})";
                    greenColour.Position = green;

                    blueColour.Text = $"蓝色数值 ({blue})";
                    blueColour.Position = blue; 
                }
    
            };
        }

        /// <summary>
        /// Returns the color index that is applied on the current vehicle. 
        /// If a color is active on the vehicle which is not in the list, it'll return the default index 0 (white).
        /// </summary>
        /// <returns></returns>
        private int GetIndexFromColor()
        {
            var veh = GetVehicle();

            if (veh == null || !veh.Exists() || !veh.Mods.HasNeonLights)
            {
                return 0;
            }

            int r = 255, g = 255, b = 255;

            GetVehicleNeonLightsColour(veh.Handle, ref r, ref g, ref b);

            if (r == 255 && g == 0 && b == 255) // default return value when the vehicle has no neon kit selected.
            {
                return 0;
            }

            if (VehicleData.NeonLightColors.Any(a => { return a[0] == r && a[1] == g && a[2] == b; }))
            {
                return VehicleData.NeonLightColors.FindIndex(a => { return a[0] == r && a[1] == g && a[2] == b; });
            }

            return 0;
        }
        #endregion

        private bool IsVehicleTooDamagedToChangeExtras(Vehicle vehicle)
        {
            float bodyHealth = vehicle.BodyHealth;
            float engineHealth = vehicle.EngineHealth;
            float allowedBodyHealth = GetSettingsInt(Setting.vmenu_allowed_body_damage_for_extra_change);
            float allowedEngineHealth = GetSettingsInt(Setting.vmenu_allowed_engine_damage_for_extra_change);

            return bodyHealth < allowedBodyHealth || engineHealth < allowedEngineHealth;
        }
    }
}
