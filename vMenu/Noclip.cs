using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

using static CitizenFX.Core.Native.API;

using static vMenuShared.ConfigManager;

namespace vMenuClient
{
    public class NoClip : BaseScript
    {
        private static bool NoclipActive { get; set; } = false;
        private static int MovingSpeed { get; set; } = 0;
        private static int Scale = -1;
        private static bool FollowCamMode { get; set; } = true;


        private readonly List<string> speeds = new()
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

            if (!active)
            {
                SetScaleformMovieAsNoLongerNeeded(ref Scale);

                Scale = -1;
            }
        }

        internal static bool IsNoclipActive()
        {
            return NoclipActive;
        }
        static string JOAAT(string command)
        {
            uint hash = 0;
            string str = command.ToLower();

            for (int i = 0; i < str.Length; i++)
            {
                uint letter = (uint)str[i];
                hash += letter;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            hash += (hash << 3);
            if (hash < 0)
            {
                hash = (uint)((int)hash);
            }

            hash ^= (hash >> 11);
            hash += (hash << 15);

            if (hash < 0)
            {
                hash = (uint)((int)hash);
            }

            return hash.ToString("X");
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

                DrawScaleformMovieFullscreen(Scale, 255, 255, 255, 0, 0);
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
                    string KeyMappingID = String.IsNullOrWhiteSpace(GetSettingsString(Setting.vmenu_keymapping_id)) ? "Default" : GetSettingsString(Setting.vmenu_keymapping_id);
                    PushScaleformMovieMethodParameterString($"~INPUT_{JOAAT($"vMenu:{KeyMappingID}:NoClip")}~");
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
                {
                    Game.DisableControlThisFrame(0, Control.VehicleRadioWheel);
                }

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
                float moveSpeed = MovingSpeed;
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

                // If the player is not set as invisible by PlayerOptions or if the noclip entity is not the player ped, reset the visibility
                if (MainMenu.PlayerOptionsMenu == null || !MainMenu.PlayerOptionsMenu.PlayerInvisible || (MainMenu.PlayerOptionsMenu.PlayerInvisible && noclipEntity == Game.PlayerPed.Handle))
                {
                    SetEntityVisible(noclipEntity, true, false);
                    SetLocalPlayerVisibleLocally(true);
                }

                // Always reset the alpha.
                ResetEntityAlpha(noclipEntity);

                SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, false);
                SetPoliceIgnorePlayer(Game.PlayerPed.Handle, false);
            }

            await Task.FromResult(0);
        }
    }
}
