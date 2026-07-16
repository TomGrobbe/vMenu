using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class PlayerOptions
    {
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        // Public variables (getters only), return the private variables.
        public bool PlayerGodMode { get; private set; } = UserDefaults.PlayerGodMode;
        public bool PlayerInvisible { get; private set; } = false;
        public bool PlayerStamina { get; private set; } = UserDefaults.UnlimitedStamina;
        public bool PlayerFastRun { get; private set; } = UserDefaults.FastRun;
        public bool PlayerFastSwim { get; private set; } = UserDefaults.FastSwim;
        public bool PlayerSuperJump { get; private set; } = UserDefaults.SuperJump;
        public bool PlayerNoRagdoll { get; private set; } = UserDefaults.NoRagdoll;
        public bool PlayerNeverWanted { get; private set; } = UserDefaults.NeverWanted;
        public bool PlayerIsIgnored { get; private set; } = UserDefaults.EveryoneIgnorePlayer;
        public bool PlayerStayInVehicle { get; private set; } = UserDefaults.PlayerStayInVehicle;
        public bool PlayerFrozen { get; private set; } = false;

        public int PlayerBlood { get; private set; } = 0;

        private readonly Menu CustomDrivingStyleMenu = new("驾驶风格", "驾驶风格自定义");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create menu and menu items
            // Create the menu.
            menu = new Menu(Game.Player.Name, "玩家个人选项");

            // Create all checkboxes.
            var playerGodModeCheckbox = new MenuCheckboxItem("无敌模式", "开启/关闭无敌模式.", PlayerGodMode);
            var invisibleCheckbox = new MenuCheckboxItem("隐身模式", "开启/关闭隐身模式.", PlayerInvisible);
            var unlimitedStaminaCheckbox = new MenuCheckboxItem("无限体力", "持续奔跑而不会减速或受到伤害.", PlayerStamina);
            var fastRunCheckbox = new MenuCheckboxItem("快速奔跑", "增加当前奔跑速率!", PlayerFastRun);
            SetRunSprintMultiplierForPlayer(Game.Player.Handle, PlayerFastRun && IsAllowed(Permission.POFastRun) ? 1.49f : 1f);
            var fastSwimCheckbox = new MenuCheckboxItem("快速游泳", "增加当前游泳速率!", PlayerFastSwim);
            SetSwimMultiplierForPlayer(Game.Player.Handle, PlayerFastSwim && IsAllowed(Permission.POFastSwim) ? 1.49f : 1f);
            var superJumpCheckbox = new MenuCheckboxItem("超级跳跃", "增加当前跳跃高度!", PlayerSuperJump);
            var noRagdollCheckbox = new MenuCheckboxItem("无布娃娃", "禁用布娃娃物理系统, 让您不再从自行车上摔下来.", PlayerNoRagdoll);
            var neverWantedCheckbox = new MenuCheckboxItem("永不通缉", "禁用所有通缉级别.", PlayerNeverWanted);
            var everyoneIgnoresPlayerCheckbox = new MenuCheckboxItem("无视自己", "行人等讲无视您所有操作.", PlayerIsIgnored);
            var playerStayInVehicleCheckbox = new MenuCheckboxItem("保持车内", "启用此功能后,其他行人无法把您拖出车外.", PlayerStayInVehicle);
            var playerFrozenCheckbox = new MenuCheckboxItem("冻结位置", "冻结当前位置, 无法移动.", PlayerFrozen);

            // Wanted level options
            var wantedLevelList = new List<string> { "无通缉级别", "1", "2", "3", "4", "5" };
            var setWantedLevel = new MenuListItem("设置通缉级别", wantedLevelList, GetPlayerWantedLevel(Game.Player.Handle), "选择有效值并确认.");
            var setArmorItem = new MenuListItem("设置防弹衣级别", new List<string> { "无", GetLabelText("WT_BA_0"), GetLabelText("WT_BA_1"), GetLabelText("WT_BA_2"), GetLabelText("WT_BA_3"), GetLabelText("WT_BA_4"), }, 0, "快速设置防弹衣等级/类型.");

            // Blood level options
            var clearBloodBtn = new MenuItem("清除血迹", "清除角色身上的血迹。");
            var bloodList = new List<string> { "BigHitByVehicle", "SCR_Torture", "SCR_TrevorTreeBang", "HOSPITAL_0", "HOSPITAL_1", "HOSPITAL_2", "HOSPITAL_3", "HOSPITAL_4", "HOSPITAL_5", "HOSPITAL_6", "HOSPITAL_7", "HOSPITAL_8", "HOSPITAL_9", "Explosion_Med", "Skin_Melee_0", "Explosion_Large", "Car_Crash_Light", "Car_Crash_Heavy", "Fall_Low", "Fall", "HitByVehicle", "BigRunOverByVehicle", "RunOverByVehicle", "TD_KNIFE_FRONT", "TD_KNIFE_FRONT_VA", "TD_KNIFE_FRONT_VB", "TD_KNIFE_REAR", "TD_KNIFE_REAR_VA", "TD_KNIFE_REAR_VB", "TD_KNIFE_STEALTH", "TD_MELEE_FRONT", "TD_MELEE_REAR", "TD_MELEE_STEALTH", "TD_MELEE_BATWAIST", "TD_melee_face_l", "MTD_melee_face_r", "MTD_melee_face_jaw", "TD_PISTOL_FRONT", "TD_PISTOL_FRONT_KILL", "TD_PISTOL_REAR", "TD_PISTOL_REAR_KILL", "TD_RIFLE_FRONT_KILL", "TD_RIFLE_NONLETHAL_FRONT", "TD_RIFLE_NONLETHAL_REAR", "TD_SHOTGUN_FRONT_KILL", "TD_SHOTGUN_REAR_KILL" };
            var setBloodLevel = new MenuListItem("设置血迹等级", bloodList, PlayerBlood, "设置角色身上的血迹等级。");

            var healPlayerBtn = new MenuItem("生命值重置", "给予最大生命值.");
            var cleanPlayerBtn = new MenuItem("衣物清洗", "清除衣物任何脏污, 血迹等.");
            var dryPlayerBtn = new MenuItem("衣物烘干", "烘干衣物, 去除水渍等.");
            var wetPlayerBtn = new MenuItem("衣物潮湿", "弄湿当前衣物.");
            var suicidePlayerBtn = new MenuItem("~r~自杀", "服药/使用手枪自杀.");

            var vehicleAutoPilot = new Menu("自动驾驶", "自动驾驶选项.");

            MenuController.AddSubmenu(menu, vehicleAutoPilot);

            var vehicleAutoPilotBtn = new MenuItem("自动驾驶选项", "管理和查看载具自动驾驶功能.")
            {
                Label = "→→→"
            };

            var drivingStyles = new List<string>() { "正常", "鲁莽", "避免高速", "倒车行驶", "自定义" };
            var drivingStyle = new MenuListItem("驾驶风格", drivingStyles, 0, "设置 '驶向航点' 和 '随机周边驾驶' 功能所使用的驾驶风格.");

            // Scenarios (list can be found in the PedScenarios class)
            var playerScenarios = new MenuListItem("人物场景动作", PedScenarios.Scenarios, 0, "选择一个场景动作,按回车键启动.选择另一个场景动作将覆盖当前场景.如果您已经在播放所选场景动作,再次选择该场景动作将停止播放.");
            var stopScenario = new MenuItem("强制停止人物场景", "这将强制播放场景立即停止, 而无需等待它完成 '停止'动画.");
            #endregion

            #region add items to menu based on permissions
            // Add all checkboxes to the menu. (keeping permissions in mind)
            if (IsAllowed(Permission.POGod))
            {
                menu.AddMenuItem(playerGodModeCheckbox);
            }
            if (IsAllowed(Permission.POInvisible))
            {
                menu.AddMenuItem(invisibleCheckbox);
            }
            if (IsAllowed(Permission.POUnlimitedStamina))
            {
                menu.AddMenuItem(unlimitedStaminaCheckbox);
            }
            if (IsAllowed(Permission.POFastRun))
            {
                menu.AddMenuItem(fastRunCheckbox);
            }
            if (IsAllowed(Permission.POFastSwim))
            {
                menu.AddMenuItem(fastSwimCheckbox);
            }
            if (IsAllowed(Permission.POSuperjump))
            {
                menu.AddMenuItem(superJumpCheckbox);
            }
            if (IsAllowed(Permission.PONoRagdoll))
            {
                menu.AddMenuItem(noRagdollCheckbox);
            }
            if (IsAllowed(Permission.PONeverWanted))
            {
                menu.AddMenuItem(neverWantedCheckbox);
            }
            if (IsAllowed(Permission.POSetWanted))
            {
                menu.AddMenuItem(setWantedLevel);
            }
            if (IsAllowed(Permission.POClearBlood))
            {
                menu.AddMenuItem(clearBloodBtn);
            }
            if (IsAllowed(Permission.POSetBlood))
            {
                menu.AddMenuItem(setBloodLevel);
            }
            if (IsAllowed(Permission.POIgnored))
            {
                menu.AddMenuItem(everyoneIgnoresPlayerCheckbox);
            }
            if (IsAllowed(Permission.POStayInVehicle))
            {
                menu.AddMenuItem(playerStayInVehicleCheckbox);
            }
            if (IsAllowed(Permission.POMaxHealth))
            {
                menu.AddMenuItem(healPlayerBtn);
            }
            if (IsAllowed(Permission.POMaxArmor))
            {
                menu.AddMenuItem(setArmorItem);
            }
            if (IsAllowed(Permission.POCleanPlayer))
            {
                menu.AddMenuItem(cleanPlayerBtn);
            }
            if (IsAllowed(Permission.PODryPlayer))
            {
                menu.AddMenuItem(dryPlayerBtn);
            }
            if (IsAllowed(Permission.POWetPlayer))
            {
                menu.AddMenuItem(wetPlayerBtn);
            }

            menu.AddMenuItem(suicidePlayerBtn);

            if (IsAllowed(Permission.POVehicleAutoPilotMenu))
            {
                menu.AddMenuItem(vehicleAutoPilotBtn);
                MenuController.BindMenuItem(menu, vehicleAutoPilot, vehicleAutoPilotBtn);

                vehicleAutoPilot.AddMenuItem(drivingStyle);

                var startDrivingWaypoint = new MenuItem("驾驶至导航点", "自动驾驶载具到达已设置的导航点位置.");
                var startDrivingRandomly = new MenuItem("区域随机驾驶", "自动驾驶载具进行附近区域随机驾驶.");
                var stopDriving = new MenuItem("停止驾驶", "自动驾驶将寻找合适地点停下. 一旦载具到达合适的停车地点, 自动驾驶则停止.");
                var forceStopDriving = new MenuItem("强制停车", "这将立即停止驾驶任务, 而非寻找合适的地方停车.");
                var customDrivingStyle = new MenuItem("驾驶风格自定义", "选择自定义驾驶风格.确保在驾驶风格列表中选择'自定义'驾驶风格,将其启用.") { Label = "→→→" };
                MenuController.AddSubmenu(vehicleAutoPilot, CustomDrivingStyleMenu);
                vehicleAutoPilot.AddMenuItem(customDrivingStyle);
                MenuController.BindMenuItem(vehicleAutoPilot, CustomDrivingStyleMenu, customDrivingStyle);
                var knownNames = new Dictionary<int, string>()
                {
                    { 0, "Stop for vehicles" },  // Using this flag will allow the driver to stop for other vehicles when appropriate, instead of hitting them.
                    { 1, "Stop for pedestrians" },  // Using this flag will allow the driver to stop for pedestrians when appropriate, instead of hitting them.
                    { 2, "Swerve around all vehicles" },  // Using this flag will allow the driver to stop for other vehicles when appropriate, instead of hitting them.
                    { 3, "Steer around stationary vehicles" },  // Using this flag will allow the driver to steer around a vehicle in order to avoid them when driving.
                    { 4, "Steer around pedestrians" },  // Using this flag will allow the driver to steer around a pedestrian in order to avoid them when driving.
                    { 5, "Steer around objects" },  // Using this flag will allow the driver to steer around objects in order to avoid them when driving.
                    { 6, "Don't steer around Player" },  // Using this flag will prevent the driver trying steer around an pedestrian if that pedestrian is the player
                    { 7, "Stop at lights" },  // Using this flag will cause the driver to stop at traffic lights when appropriate.
                    { 8, "Go off road when avoiding" },  // Using this flag will allow the driver to deliberately go off-road when trying to avoid something.
                    { 9, "Drive into oncoming traffic" },  // Using this flag will allow the driver to deliberately drive into oncoming traffic, when certain conditions are met. Such as avoiding or swerving, or overtaking.
                    { 10, "倒车行驶" },  // Using this flag will tell the driver that they should drive in reverse.
                    { 11, "Use wander fallback instead of straight line" },  // Using this flag will make the driver use wander pathfinding instead of straight line pathfinding when no route is found.
                    { 12, "Avoid restricted areas" },  // Using this flag will tell the driver that they should avoid areas considered restricted. This includes areas like the Army Base, Prison, Airport, or any other areas that might give the Player a Wanted Level for driving through.
                    { 13, "Prevent background pathfinding" },  // The driver will not perform background pathfinding.
                    { 14, "Adjust speed for current road" },  // Using this flag will tell the driver that they should match their speed to the road that they are driving on. Highways use a faster speed than main roads.
                    { 15, "Prevent join in road direction when moving" },  // The driver avoids joining roads in the direction of traffic when moving.
                    { 16, "Don't avoid target" },  // The driver does not avoid the target destination.
                    { 17, "Target position overrides entity" },  // The target position takes precedence over the target entity.
                    { 18, "Use shortcut links (Use shortest path)" },  // Using this flag will tell the driver that they should try to use Road Nodes flagged as shortcuts. This may divert them down side streets, or through oncoming traffic, if this behavior is not restricted.
                    { 19, "Change lanes around obstructions" },  // Using this flag allows the Driver to change lanes to avoid an obstruction.
                    { 20, "Avoid target coords" },  // The driver pathfind away from instead of towards the target coordinates.
                    { 21, "Use switched-off nodes" },  // Using this flag allows the Driver to use Road Nodes that have been disabled by Zones.
                    { 22, "Prefer navmesh route" },  // Using this flag allows the Driver to use the Navmesh to find a path to their destination instead of the Road Nodes system. This is useful for off-road driving.
                    { 23, "Plane taxi mode" },  // The driver operates as if taxiing an aircraft.
                    { 24, "Force straight line" },  // Using this flag forces the Driver to drive in a straight line to their next destination, ignoring the Navmesh or the Road Nodes system.
                    { 25, "Use string pulling at junctions" },  // The driver uses string pulling for smoother turns at junctions.
                    { 26, "Avoid Adverse Conditions" },  // The driver avoids "adverse conditions" (shocking events, etc.) when cruising.
                    { 27, "Avoid turns" },  // The driver avoids turns when cruising.
                    { 28, "Extend route with wander results" },  // The driver extends the route using wandering paths.
                    { 29, "Avoid highways (if possible)" },  // Using this flag prevents the Driver from using a highway when possible.
                    { 30, "Force join in road direction" },  // The driver joins roads in direction of the flow of traffic only.
                    { 31, "Don't terminate task when achieved" },  // The driver will not terminate the driving task upon reaching the destination.
                };
                for (var i = 0; i < 32; i++)
                {
                    var name = "~r~未知风格";
                    if (knownNames.ContainsKey(i))
                    {
                        name = knownNames[i];
                    }
                    var checkbox = new MenuCheckboxItem(name, "管理此驾驶风格开关", false);
                    CustomDrivingStyleMenu.AddMenuItem(checkbox);
                }
                CustomDrivingStyleMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                    CustomDrivingStyleMenu.MenuSubtitle = $"custom style: {style}";
                    if (drivingStyle.ListIndex == 4)
                    {
                        Notify.Custom("驾驶风格已更新.");
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                    }
                    else
                    {
                        Notify.Custom("由于在之前的菜单中没有启用自定义驾驶风格, 驾驶风格未更新.");
                    }
                };

                vehicleAutoPilot.AddMenuItem(startDrivingWaypoint);
                vehicleAutoPilot.AddMenuItem(startDrivingRandomly);
                vehicleAutoPilot.AddMenuItem(stopDriving);
                vehicleAutoPilot.AddMenuItem(forceStopDriving);

                vehicleAutoPilot.RefreshIndex();

                vehicleAutoPilot.OnItemSelect += async (sender, item, index) =>
                {
                    if (Game.PlayerPed.IsInVehicle() && item != stopDriving && item != forceStopDriving)
                    {
                        if (Game.PlayerPed.CurrentVehicle != null && Game.PlayerPed.CurrentVehicle.Exists() && !Game.PlayerPed.CurrentVehicle.IsDead && Game.PlayerPed.CurrentVehicle.IsDriveable)
                        {
                            if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                            {
                                if (item == startDrivingWaypoint)
                                {
                                    if (IsWaypointActive())
                                    {
                                        var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                        DriveToWp(style);
                                        Notify.Info("当前正在为您驾驶载具. 您可以随时按下停止驾驶按钮来取消, 载具将在到达目的地时停止.");
                                    }
                                    else
                                    {
                                        Notify.Error("您需要预先放置标记一个有效导航点!");
                                    }

                                }
                                else if (item == startDrivingRandomly)
                                {
                                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                    DriveWander(style);
                                    Notify.Info("当前正在为您驾驶载具. 您可以随时按下停止驾驶按钮来取消, 载具将在到达目的地时停止.");
                                }
                            }
                            else
                            {
                                Notify.Error("您必须处于载具的驾驶位!!");
                            }
                        }
                        else
                        {
                            Notify.Error("您的载具损坏或不存在!");
                        }
                    }
                    else if (item != stopDriving && item != forceStopDriving)
                    {
                        Notify.Error("您需要先进入载具!");
                    }
                    if (item == stopDriving)
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            var veh = GetVehicle();
                            if (veh != null && veh.Exists() && !veh.IsDead)
                            {
                                var outPos = new Vector3();
                                if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
                                {
                                    Notify.Info("将为您寻觅一个合适的地方停车,然后停止驾驶.请稍等.");
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    TaskVehiclePark(Game.PlayerPed.Handle, veh.Handle, outPos.X, outPos.Y, outPos.Z, Game.PlayerPed.Heading, 3, 60f, true);
                                    while (Game.PlayerPed.Position.DistanceToSquared2D(outPos) > 3f)
                                    {
                                        await BaseScript.Delay(0);
                                    }
                                    SetVehicleHalt(veh.Handle, 3f, 0, false);
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    Notify.Info("当前已停止驾驶.");
                                }
                            }
                        }
                        else
                        {
                            ClearPedTasks(Game.PlayerPed.Handle);
                            Notify.Alert("当前已离开载具.");
                        }
                    }
                    else if (item == forceStopDriving)
                    {
                        ClearPedTasks(Game.PlayerPed.Handle);
                        Notify.Info("自动驾驶模式已取消.");
                    }
                };

                vehicleAutoPilot.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
                {
                    if (item == drivingStyle)
                    {
                        var style = GetStyleFromIndex(listIndex);
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                        Notify.Info($"驾驶任务风格现在设置为: ~r~{drivingStyles[listIndex]}~s~.");
                    }
                };
            }

            if (IsAllowed(Permission.POFreeze))
            {
                menu.AddMenuItem(playerFrozenCheckbox);
            }
            if (IsAllowed(Permission.POScenarios))
            {
                menu.AddMenuItem(playerScenarios);
                menu.AddMenuItem(stopScenario);
            }
            #endregion

            #region handle all events
            // Checkbox changes.
            menu.OnCheckboxChange += (sender, item, itemIndex, _checked) =>
            {
                // God Mode toggled.
                if (item == playerGodModeCheckbox)
                {
                    PlayerGodMode = _checked;
                }
                // Invisibility toggled.
                else if (item == invisibleCheckbox)
                {
                    PlayerInvisible = _checked;
                    SetEntityVisible(Game.PlayerPed.Handle, !PlayerInvisible, false);
                }
                // Unlimited Stamina toggled.
                else if (item == unlimitedStaminaCheckbox)
                {
                    PlayerStamina = _checked;
                    StatSetInt(Game.GenerateHashASCII("MP0_STAMINA"), _checked ? 100 : 0, true);
                }
                // Fast run toggled.
                else if (item == fastRunCheckbox)
                {
                    PlayerFastRun = _checked;
                    SetRunSprintMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Fast swim toggled.
                else if (item == fastSwimCheckbox)
                {
                    PlayerFastSwim = _checked;
                    SetSwimMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Super jump toggled.
                else if (item == superJumpCheckbox)
                {
                    PlayerSuperJump = _checked;
                }
                // No ragdoll toggled.
                else if (item == noRagdollCheckbox)
                {
                    PlayerNoRagdoll = _checked;
                }
                // Never wanted toggled.
                else if (item == neverWantedCheckbox)
                {
                    PlayerNeverWanted = _checked;
                    if (!_checked)
                    {
                        SetMaxWantedLevel(5);
                    }
                    else
                    {
                        SetMaxWantedLevel(0);
                    }
                }
                // Everyone ignores player toggled.
                else if (item == everyoneIgnoresPlayerCheckbox)
                {
                    PlayerIsIgnored = _checked;

                    // Manage player is ignored by everyone.
                    SetEveryoneIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPoliceIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPlayerCanBeHassledByGangs(Game.Player.Handle, !PlayerIsIgnored);
                }
                else if (item == playerStayInVehicleCheckbox)
                {
                    PlayerStayInVehicle = _checked;
                }
                // Freeze player toggled.
                else if (item == playerFrozenCheckbox)
                {
                    PlayerFrozen = _checked;

                    if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                    else if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                }
            };

            // List selections
            menu.OnListItemSelect += (sender, listItem, listIndex, itemIndex) =>
            {
                // Set wanted Level
                if (listItem == setWantedLevel)
                {
                    SetPlayerWantedLevel(Game.Player.Handle, listIndex, false);
                    SetPlayerWantedLevelNow(Game.Player.Handle, false);
                }
                // Set blood level
                else if (listItem == setBloodLevel)
                {
                    ApplyPedDamagePack(Game.PlayerPed.Handle, bloodList[listIndex], 100, 100);
                }
                // Player Scenarios 
                else if (listItem == playerScenarios)
                {
                    PlayScenario(PedScenarios.ScenarioNames[PedScenarios.Scenarios[listIndex]]);
                }
                else if (listItem == setArmorItem)
                {
                    Game.PlayerPed.Armor = listItem.ListIndex * 20;
                }
            };

            // button presses
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Force Stop Scenario button
                if (item == stopScenario)
                {
                    // Play a new scenario named "forcestop" (this scenario doesn't exist, but the "Play" function checks
                    // for the string "forcestop", if that's provided as th scenario name then it will forcefully clear the player task.
                    PlayScenario("forcestop");
                }
                else if (item == clearBloodBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Game.PlayerPed.ResetVisibleDamage();
                    // not ideal for removing visible bruises & scars, may have some sync issues but could not find an alternative method, anyone who does feel free to update

                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 0, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 1, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 2, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 3, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 4, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 5, "ALL");
                }
                else if (item == healPlayerBtn)
                {
                    Game.PlayerPed.Health = Game.PlayerPed.MaxHealth;
                    Notify.Success("当前生命值已恢复治愈.");
                }
                else if (item == cleanPlayerBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Notify.Success("当前服装已清污.");
                }
                else if (item == dryPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 0f;
                    Notify.Success("当前服装已烘干.");
                }
                else if (item == wetPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 2f;
                    Notify.Success("当前服装已侵湿.");
                }
                else if (item == suicidePlayerBtn)
                {
                    CommitSuicide();
                }
            };
            #endregion

        }

        private int GetCustomDrivingStyle()
        {
            var items = CustomDrivingStyleMenu.GetMenuItems();
            var flags = new int[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is MenuCheckboxItem checkbox)
                {
                    flags[i] = checkbox.Checked ? 1 : 0;
                }
            }
            var binaryString = "";
            var reverseFlags = flags.Reverse();
            foreach (var i in reverseFlags)
            {
                binaryString += i;
            }
            var binaryNumber = Convert.ToUInt32(binaryString, 2);
            return (int)binaryNumber;
        }

        private int GetStyleFromIndex(int index)
        {
            var style = index switch
            {
                0 => 443,// normal
                1 => 575,// rushed
                2 => 536871355,// Avoid highways
                3 => 1467,// Go in reverse
                4 => GetCustomDrivingStyle(),// custom driving style;
                _ => 0,// no style (impossible, but oh well)
            };
            return style;
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Player Options Menu</returns>
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
