using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Extensions;

namespace vMenu.Enhanced.NoClip
{
    public class NoClip
    {
        private static bool NoclipActive { get; set; } = false;
        private static int MovingSpeed { get; set; } = 0;
        private static bool FollowCamMode { get; set; } = true;

        private static int _instructionalButtonsScaleformId = -1;

        private static readonly List<string> speeds = new() {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
        };

        private static bool IsF8ConsoleLikelyOpen => !Native.IsControlEnabled(0, 360);


        public NoClip()
        {
            NoClipper();
            NoClipperKeyer();
        }

        async void NoClipper()
        {
            while (true)
            {
                await NoClipHandler();
                await API.Yield();
            }
        }

        static async void NoClipperKeyer()
        {
            while (true)
            {
                NoClipControls();
                await API.Yield();
            }
        }

        private static void NoClipControls()
        {
            if (!IsF8ConsoleLikelyOpen && (Native.IsControlJustPressed(0, 289) || Native.IsDisabledControlJustPressed(0, 289)))
            {
                NoclipActive = !NoclipActive;
            }
        }

        internal static void SetNoclipActive(bool active)
        {
            NoclipActive = active;

            if (!active)
            {
                Native.SetScaleformMovieAsNoLongerNeeded(out _instructionalButtonsScaleformId);

                _instructionalButtonsScaleformId = -1;
            }
        }

        internal static bool IsNoclipActive()
        {
            return NoclipActive;
        }

        private async Task NoClipHandler()
        {
            if (NoclipActive)
            {
                await PrepareInstructionalButtons();
            }

            while (NoclipActive)
            {
                if (!Native.IsHudHidden())
                {
                    DisplayInstructionalButtons();
                }

                var playerPed = API.Players.Local;

                var noclipEntity = playerPed.Ped?.IsPedInAnyVehicle() == true ? playerPed.Ped.GetVehiclePedIsIn() : playerPed.PedIndex;

                Native.FreezeEntityPosition(noclipEntity, true);
                Native.SetEntityInvincible(noclipEntity, true, false);

                System.Numerics.Vector3 newPos;

                Native.DisableControlAction(0, 30, false);  // Control.MoveLeftRight
                Native.DisableControlAction(0, 31, false);  // Control.MoveUpDown
                Native.DisableControlAction(0, 32, false);  // Control.MoveUp
                Native.DisableControlAction(0, 33, false);  // Control.MoveDownOnly
                Native.DisableControlAction(0, 34, false);  // Control.MoveLeftOnly
                Native.DisableControlAction(0, 35, false);  // Control.MoveRightOnly
                Native.DisableControlAction(0, 36, false);  // Control.Duck
                Native.DisableControlAction(0, 44, false);  // Control.Cover
                Native.DisableControlAction(0, 244, false); // Control.MultiplayerInfo
                Native.DisableControlAction(0, 74, false);  // Control.VehicleHeadlight

                if (playerPed.Ped?.IsPedInAnyVehicle() == true)
                {
                    Native.DisableControlAction(0, 81, false); // VehicleRadioWheel
                }

                var yoff = 0.0f;
                var zoff = 0.0f;

                if (Native.IsUsingKeyboardAndMouse(2) && Native.UpdateOnscreenKeyboard() != 0 && !Native.IsPauseMenuActive())
                {
                    if (Native.IsControlJustPressed(0, 21))
                    {
                        MovingSpeed++;
                        if (MovingSpeed >= speeds.Count)
                        {
                            MovingSpeed = 0;
                        }
                    }
                    if (Native.IsDisabledControlJustPressed(0, 36))
                    {
                        MovingSpeed--;
                        if (MovingSpeed < 0)
                        {
                            MovingSpeed = speeds.Count - 1;
                        }
                    }

                    if (Native.IsDisabledControlPressed(0, 32))
                    {
                        yoff = 0.5f;
                    }
                    if (Native.IsDisabledControlPressed(0, 33))
                    {
                        yoff = -0.5f;
                    }
                    if (!FollowCamMode && Native.IsDisabledControlPressed(0, 34))
                    {
                        Native.SetEntityHeading(playerPed.PedIndex, Native.GetEntityHeading(playerPed.PedIndex) + 3f);
                    }
                    if (!FollowCamMode && Native.IsDisabledControlPressed(0, 35))
                    {
                        Native.SetEntityHeading(playerPed.PedIndex, Native.GetEntityHeading(playerPed.PedIndex) - 3f);
                    }
                    if (Native.IsDisabledControlPressed(0, 44))
                    {
                        zoff = 0.21f;
                    }
                    if (Native.IsDisabledControlPressed(0, 20))
                    {
                        zoff = -0.21f;
                    }
                    if (Native.IsDisabledControlJustPressed(0, 74))
                    {
                        FollowCamMode = !FollowCamMode;
                    }
                }
                float moveSpeed = MovingSpeed switch
                {
                    0 => 0.1f,
                    1 => 0.5f,
                    2 => 1.0f,
                    3 => 1.5f,
                    4 => 2.5f,
                    5 => 5.5f,
                    6 => 8.5f,
                    7 => 12.5f,
                    8 => 18.5f,
                    9 => 25.5f,
                    _ => 0.1f,
                };
                newPos = Native.GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, yoff * moveSpeed, zoff * moveSpeed);

                var heading = Native.GetEntityHeading(noclipEntity);
                Native.SetEntityVelocity(noclipEntity, 0f, 0f, 0f);
                Native.SetEntityRotation(noclipEntity, 0f, 0f, 0f, 0, false);
                Native.SetEntityHeading(noclipEntity, FollowCamMode ? Native.GetGameplayCamRelativeHeading() : heading);
                Native.SetEntityCollision(noclipEntity, false, false);
                Native.SetEntityCoordsNoOffset(noclipEntity, newPos.X, newPos.Y, newPos.Z, true, true, true);

                Native.SetEntityVisible(noclipEntity, false, false);
                Native.SetLocalPlayerVisibleLocally(true);
                Native.SetEntityAlpha(noclipEntity, (int)(255 * 0.2), false);

                Native.SetEveryoneIgnorePlayer(playerPed.PedIndex, true);
                Native.SetPoliceIgnorePlayer(playerPed.PedIndex, true);

                // After the next game tick, reset the entity properties.
                await API.Delay(0);
                Native.FreezeEntityPosition(noclipEntity, false);
                Native.SetEntityInvincible(noclipEntity, false, false);
                Native.SetEntityCollision(noclipEntity, true, true);

                // If the player is not set as invisible by PlayerOptions or if the noclip entity is not the player ped, reset the visibility
                if (noclipEntity == playerPed.PedIndex)
                {
                    Native.SetEntityVisible(noclipEntity, true, false);
                    Native.SetLocalPlayerVisibleLocally(true);
                }

                // Always reset the alpha.
                Native.ResetEntityAlpha(noclipEntity);

                Native.SetEveryoneIgnorePlayer(playerPed.PedIndex, false);
                Native.SetPoliceIgnorePlayer(playerPed.PedIndex, false);
            }

            await API.Yield();
        }

        private static async Task PrepareInstructionalButtons()
        {
            _instructionalButtonsScaleformId = Native.RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
            while (!Native.HasScaleformMovieLoaded(_instructionalButtonsScaleformId))
            {
                await API.Delay(0);
            }
        }

        internal record InstructionalButton(Func<string> TextGetter, string Control);
        private readonly List<InstructionalButton> _instructionalButtons = new()
        {
            new InstructionalButton(() => $"Speed: {speeds[MovingSpeed]}x", ""),
            new InstructionalButton(() => $"Increase speed", "INPUT_SPRINT"),
            new InstructionalButton(() => $"Decrease speed", "INPUT_DUCK"),
            new InstructionalButton(() => "Move Up", "INPUT_COVER"),
            new InstructionalButton(() => "Move Down", "INPUT_MULTIPLAYER_INFO"),
            new InstructionalButton(() => "Move Backward / Forward", "INPUT_MOVE_UD"),
            new InstructionalButton(() => FollowCamMode ? "Follow Cam: On" : "Follow Cam: Off", "INPUT_VEH_HEADLIGHT")
        };

        private void DisplayInstructionalButtons()
        {
            Native.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "CLEAR_ALL");
            Native.EndScaleformMovieMethod();

            int i = 0;
            foreach (var button in _instructionalButtons)
            {
                SetDataSlot(i, button);
                i++;
            }

            if (!FollowCamMode)
            {
                SetDataSlot(_instructionalButtons.Count, new InstructionalButton(() => "Turn Right / Left", "INPUT_MOVE_LR"));
            }

            Native.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "DRAW_INSTRUCTIONAL_BUTTONS");
            Native.ScaleformMovieMethodAddParamInt(0);
            Native.EndScaleformMovieMethod();

            Native.DrawScaleformMovieFullscreen(_instructionalButtonsScaleformId, 255, 255, 255, 255, 0);
        }

        private static void SetDataSlot(int i, InstructionalButton button)
        {
            Native.BeginScaleformMovieMethod(_instructionalButtonsScaleformId, "SET_DATA_SLOT");
            Native.ScaleformMovieMethodAddParamInt(i);
            Native.ScaleformMovieMethodAddParamTextureNameString($"~{button.Control}~");
            Native.ScaleformMovieMethodAddParamTextureNameString(button.TextGetter());
            Native.EndScaleformMovieMethod();
        }
    }
}
