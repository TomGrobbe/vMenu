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
        private int speed = 1;

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
                        if (noclipMenu.Visible == false)
                        {
                            noclipMenu.Visible = true;
                        }
                        //var pos = GetEntityCoords(PlayerPedId(), true);
                        Vector3 newPos = GetEntityCoords(PlayerPedId(), true);
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
                        Game.DisableControlThisFrame(0, Control.Pickup);
                        SetEntityCollision(PlayerPedId(), false, false);

                        var xoff = 0f;
                        var yoff = 0f;
                        var zoff = 0f;

                        if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                        {
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
                                zoff = 0.18f;
                            }
                            if (Game.IsDisabledControlPressed(0, Control.Pickup))
                            {
                                zoff = -0.18f;
                            }
                        }

                        newPos = GetOffsetFromEntityInWorldCoords(PlayerPedId(), xoff * speed, yoff * speed, zoff * speed);

                        var heading = GetEntityHeading(PlayerPedId());
                        SetEntityVelocity(PlayerPedId(), 0f, 0f, 0f);
                        SetEntityRotation(PlayerPedId(), 0f, 0f, 0f, 0, false);
                        SetEntityHeading(PlayerPedId(), heading);
                        SetEntityCoordsNoOffset(PlayerPedId(), newPos.X, newPos.Y, newPos.Z + 0.001f, true, true, true);

                        await Delay(0);
                    }
                    SetEntityCollision(PlayerPedId(), true, true);
                    noclipMenu.Visible = false;
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

            var speedList = new List<dynamic>()
            {
                "Slow",
                "Medium",
                "Fast",
                "Very Fast",
                "Extremely Fast",
                "~g~Snail Power!"
            };

            UIMenuListItem speed = new UIMenuListItem("Speed", speedList, 1, "Select a moving speed.");

            MainMenu.Mp.Add(noclipMenu);

            setupDone = true;
        }

        public UIMenu GetMenu()
        {
            return noclipMenu;
        }
    }
}
