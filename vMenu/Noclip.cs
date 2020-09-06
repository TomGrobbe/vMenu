using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class NoClip : BaseScript
    {
        private static bool NoclipActive { get; set; } = false;
        private static int MovingSpeed { get; set; } = 0;
        private static int Scale { get; set; } = -1;
        private static bool FollowCamMode { get; set; } = true;


        private List<string> speeds = new List<string>()
        {
            "Very Slow",
            "Slow",
            "Normal",
            "Fast",
            "Very Fast",
            "Extremely Fast",
            "Extremely Fast v2.0",
            "Max Speed"
        };

        public NoClip()
        {
            Tick += NoClipHandler;
        }

        internal static void SetNoclipActive(bool active)
        {
            NoclipActive = active;
        }

        internal static bool IsNoclipActive()
        {
            return NoclipActive;
        }

        private async Task NoClipHandler()
        {
            if (NoclipActive)
            {
                Scale = RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
                while (!HasScaleformMovieLoaded(Scale))
                {
                    await Delay(0);
                }
            }
            while (NoclipActive)
            {
                if (!IsHudHidden())
                {
                    BeginScaleformMovieMethod(Scale, "CLEAR_ALL");
                    EndScaleformMovieMethod();

                    BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
                    ScaleformMovieMethodAddParamInt(0);
                    PushScaleformMovieMethodParameterString("~INPUT_SPRINT~");
                    PushScaleformMovieMethodParameterString($"Change Speed ({speeds[MovingSpeed]})");
                    EndScaleformMovieMethod();

                    BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
                    ScaleformMovieMethodAddParamInt(1);
                    PushScaleformMovieMethodParameterString("~INPUT_MOVE_LR~");
                    PushScaleformMovieMethodParameterString($"Turn Left/Right");
                    EndScaleformMovieMethod();

                    BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
                    ScaleformMovieMethodAddParamInt(2);
                    PushScaleformMovieMethodParameterString("~INPUT_MOVE_UD~");
                    PushScaleformMovieMethodParameterString($"Move");
                    EndScaleformMovieMethod();

                    BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
                    ScaleformMovieMethodAddParamInt(3);
                    PushScaleformMovieMethodParameterString("~INPUT_MULTIPLAYER_INFO~");
                    PushScaleformMovieMethodParameterString($"Down");
                    EndScaleformMovieMethod();

                    BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
                    ScaleformMovieMethodAddParamInt(4);
                    PushScaleformMovieMethodParameterString("~INPUT_COVER~");
                    PushScaleformMovieMethodParameterString($"Up");
                    EndScaleformMovieMethod();

                    BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
                    ScaleformMovieMethodAddParamInt(5);
                    PushScaleformMovieMethodParameterString("~INPUT_VEH_HEADLIGHT~");
                    PushScaleformMovieMethodParameterString($"Cam Mode");
                    EndScaleformMovieMethod();

                    BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
                    ScaleformMovieMethodAddParamInt(6);
                    PushScaleformMovieMethodParameterString(GetControlInstructionalButton(0, (int)MainMenu.NoClipKey, 1));
                    PushScaleformMovieMethodParameterString($"Toggle NoClip");
                    EndScaleformMovieMethod();

                    BeginScaleformMovieMethod(Scale, "DRAW_INSTRUCTIONAL_BUTTONS");
                    ScaleformMovieMethodAddParamInt(0);
                    EndScaleformMovieMethod();

                    DrawScaleformMovieFullscreen(Scale, 255, 255, 255, 255, 0);
                }

                var noclipEntity = Game.PlayerPed.IsInVehicle() ? Game.PlayerPed.CurrentVehicle.Handle : Game.PlayerPed.Handle;

                FreezeEntityPosition(noclipEntity, true);
                SetEntityInvincible(noclipEntity, true);

                Vector3 newPos;
                Game.DisableControlThisFrame(0, Control.MoveUpOnly);
                Game.DisableControlThisFrame(0, Control.MoveUp);
                Game.DisableControlThisFrame(0, Control.MoveUpDown);
                Game.DisableControlThisFrame(0, Control.MoveDown);
                Game.DisableControlThisFrame(0, Control.MoveDownOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeft);
                Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeftRight);
                Game.DisableControlThisFrame(0, Control.MoveRight);
                Game.DisableControlThisFrame(0, Control.MoveRightOnly);
                Game.DisableControlThisFrame(0, Control.Cover);
                Game.DisableControlThisFrame(0, Control.MultiplayerInfo);
                Game.DisableControlThisFrame(0, Control.VehicleHeadlight);
                if (Game.PlayerPed.IsInVehicle())
                    Game.DisableControlThisFrame(0, Control.VehicleRadioWheel);

                var yoff = 0.0f;
                var zoff = 0.0f;

                if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && UpdateOnscreenKeyboard() != 0 && !Game.IsPaused)
                {
                    if (Game.IsControlJustPressed(0, Control.Sprint))
                    {
                        MovingSpeed++;
                        if (MovingSpeed == speeds.Count)
                        {
                            MovingSpeed = 0;
                        }
                    }

                    if (Game.IsDisabledControlPressed(0, Control.MoveUpOnly))
                    {
                        yoff = 0.5f;
                    }
                    if (Game.IsDisabledControlPressed(0, Control.MoveDownOnly))
                    {
                        yoff = -0.5f;
                    }
                    if (!FollowCamMode && Game.IsDisabledControlPressed(0, Control.MoveLeftOnly))
                    {
                        SetEntityHeading(Game.PlayerPed.Handle, GetEntityHeading(Game.PlayerPed.Handle) + 3f);
                    }
                    if (!FollowCamMode && Game.IsDisabledControlPressed(0, Control.MoveRightOnly))
                    {
                        SetEntityHeading(Game.PlayerPed.Handle, GetEntityHeading(Game.PlayerPed.Handle) - 3f);
                    }
                    if (Game.IsDisabledControlPressed(0, Control.Cover))
                    {
                        zoff = 0.21f;
                    }
                    if (Game.IsDisabledControlPressed(0, Control.MultiplayerInfo))
                    {
                        zoff = -0.21f;
                    }
                    if (Game.IsDisabledControlJustPressed(0, Control.VehicleHeadlight))
                    {
                        FollowCamMode = !FollowCamMode;
                    }
                }
                float moveSpeed = (float)MovingSpeed;
                if (MovingSpeed > speeds.Count / 2)
                {
                    moveSpeed *= 1.8f;
                }
                moveSpeed = moveSpeed / (1f / GetFrameTime()) * 60;
                newPos = GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, yoff * (moveSpeed + 0.3f), zoff * (moveSpeed + 0.3f));

                var heading = GetEntityHeading(noclipEntity);
                SetEntityVelocity(noclipEntity, 0f, 0f, 0f);
                SetEntityRotation(noclipEntity, 0f, 0f, 0f, 0, false);
                SetEntityHeading(noclipEntity, FollowCamMode ? GetGameplayCamRelativeHeading() : heading);
                SetEntityCollision(noclipEntity, false, false);
                SetEntityCoordsNoOffset(noclipEntity, newPos.X, newPos.Y, newPos.Z, true, true, true);

                SetEntityVisible(noclipEntity, false, false);
                SetLocalPlayerVisibleLocally(true);
                SetEntityAlpha(noclipEntity, (int)(255 * 0.2), 0);

                SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, true);
                SetPoliceIgnorePlayer(Game.PlayerPed.Handle, true);

                // After the next game tick, reset the entity properties.
                await Delay(0);
                FreezeEntityPosition(noclipEntity, false);
                SetEntityInvincible(noclipEntity, false);
                SetEntityCollision(noclipEntity, true, true);

                SetEntityVisible(noclipEntity, true, false);
                SetLocalPlayerVisibleLocally(true);
                ResetEntityAlpha(noclipEntity);

                SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, false);
                SetPoliceIgnorePlayer(Game.PlayerPed.Handle, false);
            }

            await Task.FromResult(0);
        }
        //private bool setupDone = false;
        //private Menu noclipMenu = null;
        //private int currentSpeed = 0;

        //private List<string> speeds = new List<string>()
        //{
        //    "Very Slow",
        //    "Slow",
        //    "Normal",
        //    "Fast",
        //    "Very Fast",
        //    "Extremely Fast",
        //    "Extremely Fast v2.0",
        //    "Max Speed"
        //};

        ///// <summary>
        ///// Constructor
        ///// </summary>
        //public NoclipMenu()
        //{
        //    Tick += OnTick;
        //}

        ///// <summary>
        ///// OnTick to run the menu functions.
        ///// </summary>
        ///// <returns></returns>
        //private async Task OnTick()
        //{
        //    // Setup is not done or cf is null.
        //    if (!setupDone)
        //    {
        //        Setup();

        //        // wait for setup in MainMenu (permissions and addons) to be done before adding the noclip menu.
        //        while (!MainMenu.ConfigOptionsSetupComplete || !MainMenu.PermissionsSetupComplete)
        //        {
        //            await Delay(0);
        //        }
        //        // Add the noclip menu
        //        MenuController.AddMenu(noclipMenu);
        //    }
        //    // Setup is done.
        //    else
        //    {
        //        if (noclipMenu == null)
        //        {
        //            await Delay(0);
        //        }
        //        else
        //        {
        //            while (MainMenu.NoClipEnabled)
        //            {
        //                noclipMenu.InstructionalButtons[Control.Sprint] = $"Change speed ({speeds[currentSpeed]})";
        //                var noclipEntity = Game.PlayerPed.IsInVehicle() ? GetVehicle().Handle : Game.PlayerPed.Handle;

        //                if (noclipMenu.Visible == false)
        //                {
        //                    noclipMenu.OpenMenu();
        //                }
        //                FreezeEntityPosition(noclipEntity, true);
        //                SetEntityInvincible(noclipEntity, true);

        //                Vector3 newPos = GetEntityCoords(noclipEntity, true);
        //                Game.DisableControlThisFrame(0, Control.MoveUpOnly);
        //                Game.DisableControlThisFrame(0, Control.MoveUp);
        //                Game.DisableControlThisFrame(0, Control.MoveUpDown);
        //                Game.DisableControlThisFrame(0, Control.MoveDown);
        //                Game.DisableControlThisFrame(0, Control.MoveDownOnly);
        //                Game.DisableControlThisFrame(0, Control.MoveLeft);
        //                Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
        //                Game.DisableControlThisFrame(0, Control.MoveLeftRight);
        //                Game.DisableControlThisFrame(0, Control.MoveRight);
        //                Game.DisableControlThisFrame(0, Control.MoveRightOnly);
        //                Game.DisableControlThisFrame(0, Control.Cover);
        //                Game.DisableControlThisFrame(0, Control.MultiplayerInfo);

        //                //var xoff = 0.0f;
        //                var yoff = 0.0f;
        //                var zoff = 0.0f;

        //                if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && UpdateOnscreenKeyboard() != 0)
        //                {
        //                    if (Game.IsControlJustPressed(0, Control.Sprint))
        //                    {
        //                        currentSpeed++;
        //                        if (currentSpeed == speeds.Count)
        //                        {
        //                            currentSpeed = 0;
        //                        }
        //                        noclipMenu.GetMenuItems()[0].Label = speeds[currentSpeed];
        //                    }

        //                    if (Game.IsDisabledControlPressed(0, Control.MoveUpOnly))
        //                    {
        //                        yoff = 0.5f;
        //                    }
        //                    if (Game.IsDisabledControlPressed(0, Control.MoveDownOnly))
        //                    {
        //                        yoff = -0.5f;
        //                    }
        //                    if (Game.IsDisabledControlPressed(0, Control.MoveLeftOnly))
        //                    {
        //                        SetEntityHeading(Game.PlayerPed.Handle, GetEntityHeading(Game.PlayerPed.Handle) + 3f);
        //                    }
        //                    if (Game.IsDisabledControlPressed(0, Control.MoveRightOnly))
        //                    {
        //                        SetEntityHeading(Game.PlayerPed.Handle, GetEntityHeading(Game.PlayerPed.Handle) - 3f);
        //                    }
        //                    if (Game.IsDisabledControlPressed(0, Control.Cover))
        //                    {
        //                        zoff = 0.21f;
        //                    }
        //                    if (Game.IsDisabledControlPressed(0, Control.MultiplayerInfo))
        //                    {
        //                        zoff = -0.21f;
        //                    }
        //                }
        //                float moveSpeed = (float)currentSpeed;
        //                if (currentSpeed > speeds.Count / 2)
        //                {
        //                    moveSpeed *= 1.8f;
        //                }
        //                newPos = GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, yoff * (moveSpeed + 0.3f), zoff * (moveSpeed + 0.3f));

        //                var heading = GetEntityHeading(noclipEntity);
        //                SetEntityVelocity(noclipEntity, 0f, 0f, 0f);
        //                SetEntityRotation(noclipEntity, 0f, 0f, 0f, 0, false);
        //                SetEntityHeading(noclipEntity, heading);

        //                //if (!((yoff > -0.01f && yoff < 0.01f) && (zoff > -0.01f && zoff < 0.01f)))
        //                {
        //                    SetEntityCollision(noclipEntity, false, false);
        //                    SetEntityCoordsNoOffset(noclipEntity, newPos.X, newPos.Y, newPos.Z, true, true, true);
        //                }

        //                // After the next game tick, reset the entity properties.
        //                await Delay(0);
        //                FreezeEntityPosition(noclipEntity, false);
        //                SetEntityInvincible(noclipEntity, false);
        //                SetEntityCollision(noclipEntity, true, true);
        //            }

        //            if (noclipMenu.Visible && !MainMenu.NoClipEnabled)
        //            {
        //                noclipMenu.CloseMenu();
        //            }

        //        }
        //    }
        //}

        ///// <summary>
        ///// Setting up the menu.
        ///// </summary>
        //private void Setup()
        //{
        //    noclipMenu = new Menu("No Clip", "Controls") { IgnoreDontOpenMenus = true };

        //    MenuItem speed = new MenuItem("Current Moving Speed", "This is your current moving speed.")
        //    {
        //        Label = speeds[currentSpeed]
        //    };

        //    noclipMenu.OnMenuOpen += (m) =>
        //    {
        //        if (MainMenu.NoClipEnabled)
        //            HelpMessage.Custom("NoClip is now active. Look at the instructional buttons for all the keybinds. You can view your current moving speed all the way on the bottom right instructional button.");
        //    };

        //    noclipMenu.AddMenuItem(speed);

        //    noclipMenu.InstructionalButtons.Clear();
        //    noclipMenu.InstructionalButtons.Add(Control.Sprint, $"Change speed ({speeds[currentSpeed]})");
        //    noclipMenu.InstructionalButtons.Add(Control.MoveUpDown, "Go Forwards/Backwards");
        //    noclipMenu.InstructionalButtons.Add(Control.MoveLeftRight, "Turn Left/Right");
        //    noclipMenu.InstructionalButtons.Add(Control.MultiplayerInfo, "Go Down");
        //    noclipMenu.InstructionalButtons.Add(Control.Cover, "Go Up");
        //    noclipMenu.InstructionalButtons.Add((Control)MainMenu.NoClipKey, "Disable Noclip");

        //    setupDone = true;
        //}

        //public Menu GetMenu()
        //{
        //    return noclipMenu;
        //}
    }
}
