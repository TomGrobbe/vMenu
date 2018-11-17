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
    public class NoclipMenu : BaseScript
    {
        private CommonFunctions cf = MainMenu.Cf;
        private bool setupDone = false;
        private UIMenu noclipMenu = null;
        private int currentSpeed = 0;

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

        /// <summary>
        /// Constructor
        /// </summary>
        public NoclipMenu()
        {
            Tick += OnTick;
        }

        /// <summary>
        /// OnTick to run the menu functions.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            // Setup is not done or cf is null.
            if (cf != null && !setupDone)
            {
                Setup();
            }
            // Cf is null, update it.
            else if (cf == null)
            {
                cf = MainMenu.Cf;
                await Delay(0);
            }
            // Setup is done.
            else
            {
                if (noclipMenu == null)
                {
                    await Delay(0);
                }
                else
                {
                    while (MainMenu.NoClipEnabled)
                    {
                        var noclipEntity = IsPedInAnyVehicle(PlayerPedId(), false) ? cf.GetVehicle() : PlayerPedId();

                        if (noclipMenu.Visible == false)
                        {
                            noclipMenu.Visible = true;
                        }
                        FreezeEntityPosition(noclipEntity, true);
                        SetEntityInvincible(noclipEntity, true);

                        Vector3 newPos = GetEntityCoords(noclipEntity, true);
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

                        //var xoff = 0.0f;
                        var yoff = 0.0f;
                        var zoff = 0.0f;

                        if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                        {
                            if (Game.IsControlJustPressed(0, Control.Sprint))
                            {
                                currentSpeed++;
                                if (currentSpeed == speeds.Count)
                                {
                                    currentSpeed = 0;
                                }
                                noclipMenu.MenuItems[0].SetRightLabel(speeds[currentSpeed]);
                            }

                            if (Game.IsDisabledControlPressed(0, Control.MoveUpOnly))
                            {
                                yoff = 0.5f;
                            }
                            if (Game.IsDisabledControlPressed(0, Control.MoveDownOnly))
                            {
                                yoff = -0.5f;
                            }
                            if (Game.IsDisabledControlPressed(0, Control.MoveLeftOnly))
                            {
                                SetEntityHeading(PlayerPedId(), GetEntityHeading(PlayerPedId()) + 3f);
                            }
                            if (Game.IsDisabledControlPressed(0, Control.MoveRightOnly))
                            {
                                SetEntityHeading(PlayerPedId(), GetEntityHeading(PlayerPedId()) - 3f);
                            }
                            if (Game.IsDisabledControlPressed(0, Control.Cover))
                            {
                                zoff = 0.21f;
                            }
                            if (Game.IsDisabledControlPressed(0, Control.MultiplayerInfo))
                            {
                                zoff = -0.21f;
                            }
                        }
                        float moveSpeed = (float)currentSpeed;
                        if (currentSpeed > speeds.Count / 2)
                        {
                            moveSpeed *= 1.8f;
                        }
                        newPos = GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, yoff * (moveSpeed + 0.3f), zoff * (moveSpeed + 0.3f));

                        var heading = GetEntityHeading(noclipEntity);
                        SetEntityVelocity(noclipEntity, 0f, 0f, 0f);
                        SetEntityRotation(noclipEntity, 0f, 0f, 0f, 0, false);
                        SetEntityHeading(noclipEntity, heading);

                        //if (!((yoff > -0.01f && yoff < 0.01f) && (zoff > -0.01f && zoff < 0.01f)))
                        {
                            SetEntityCollision(noclipEntity, false, false);
                            SetEntityCoordsNoOffset(noclipEntity, newPos.X, newPos.Y, newPos.Z, true, true, true);
                        }

                        // After the next game tick, reset the entity properties.
                        await Delay(0);
                        FreezeEntityPosition(noclipEntity, false);
                        SetEntityInvincible(noclipEntity, false);
                        SetEntityCollision(noclipEntity, true, true);
                    }

                    if (noclipMenu.Visible && !MainMenu.NoClipEnabled)
                    {
                        noclipMenu.Visible = false;
                    }

                }
            }
        }

        /// <summary>
        /// Setting up the menu.
        /// </summary>
        private void Setup()
        {

            noclipMenu = new UIMenu("No Clip", "Controls", true)
            {
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false,
                ScaleWithSafezone = false
            };
            noclipMenu.SetMenuWidthOffset(50);



            UIMenuItem speed = new UIMenuItem("Current Moving Speed", "This is your current moving speed.");
            speed.SetRightLabel(speeds[currentSpeed]);

            noclipMenu.AddItem(speed);

            noclipMenu.DisableInstructionalButtons(true);
            noclipMenu.DisableInstructionalButtons(false);

            // Only disable the default instructional buttons (back & select) (requires modified NativeUI build.)
            noclipMenu.DisableInstructionalButtons(false, disableDefaultButtons: true);

            noclipMenu.AddInstructionalButton(new InstructionalButton(Control.Sprint, "Change Speed"));
            noclipMenu.AddInstructionalButton(new InstructionalButton(Control.MoveUpDown, "Go Forwards/Backwards"));
            noclipMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Left/Right"));
            noclipMenu.AddInstructionalButton(new InstructionalButton(Control.Cover, "Go Up"));
            noclipMenu.AddInstructionalButton(new InstructionalButton(Control.MultiplayerInfo, "Go Down"));


            MainMenu.Mp.Add(noclipMenu);

            setupDone = true;
        }

        public UIMenu GetMenu()
        {
            return noclipMenu;
        }
    }
}
