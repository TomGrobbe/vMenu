//using System.Numerics;

//using CitizenFX.FiveM.Client;
//using CitizenFX.FiveM.Client.Extensions;

namespace vMenu.Enhanced.IamAproblem;

internal class ThisCausesProblems;

/*
{

private static bool NoclipActive { get; set; } = false;
private static int MovingSpeed { get; set; } = 0;
private static int Scale = -1;
private static bool FollowCamMode { get; set; } = true;


private readonly List<string> speeds = new() {
            "Very Slow",
            "Slow",
            "Normal",
            "Fast",
            "Very Fast",
            "Extremely Fast",
            "Extremely Fast v2.0",
            "Max Speed"
        };

public void Initializefuck()
{
    //NoClipper();
    //NoClipperKeyer();
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
    if (Native.IsControlJustPressed(0, 289) || Native.IsDisabledControlJustPressed(0, 289))
    {
        NoclipActive = !NoclipActive;
    }
}

internal static void SetNoclipActive(bool active)
{
    NoclipActive = active;

    if (!active)
    {
        Native.SetScaleformMovieAsNoLongerNeeded(out Scale);

        Scale = -1;
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
        Scale = Native.RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
        while (!Native.HasScaleformMovieLoaded(Scale))
        {
            await API.Delay(0);
        }

        Native.DrawScaleformMovieFullscreen(Scale, 255, 255, 255, 0, 0);
    }
    while (NoclipActive)
    {
        if (!Native.IsHudHidden())
        {
            Native.BeginScaleformMovieMethod(Scale, "CLEAR_ALL");
            Native.EndScaleformMovieMethod();

            Native.BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
            Native.ScaleformMovieMethodAddParamInt(0);
            Native.ScaleformMovieMethodAddParamTextureNameString("~INPUT_SPRINT~");
            Native.ScaleformMovieMethodAddParamTextureNameString($"Change Speed ({speeds[MovingSpeed]})");
            Native.EndScaleformMovieMethod();

            Native.BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
            Native.ScaleformMovieMethodAddParamInt(1);
            Native.ScaleformMovieMethodAddParamTextureNameString("~INPUT_MOVE_LR~");
            Native.ScaleformMovieMethodAddParamTextureNameString($"Turn Left/Right");
            Native.EndScaleformMovieMethod();

            Native.BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
            Native.ScaleformMovieMethodAddParamInt(2);
            Native.ScaleformMovieMethodAddParamTextureNameString("~INPUT_MOVE_UD~");
            Native.ScaleformMovieMethodAddParamTextureNameString($"Move");
            Native.EndScaleformMovieMethod();

            Native.BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
            Native.ScaleformMovieMethodAddParamInt(3);
            Native.ScaleformMovieMethodAddParamTextureNameString("~INPUT_MULTIPLAYER_INFO~");
            Native.ScaleformMovieMethodAddParamTextureNameString($"Down");
            Native.EndScaleformMovieMethod();

            Native.BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
            Native.ScaleformMovieMethodAddParamInt(4);
            Native.ScaleformMovieMethodAddParamTextureNameString("~INPUT_COVER~");
            Native.ScaleformMovieMethodAddParamTextureNameString($"Up");
            Native.EndScaleformMovieMethod();

            Native.BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
            Native.ScaleformMovieMethodAddParamInt(5);
            Native.ScaleformMovieMethodAddParamTextureNameString("~INPUT_VEH_HEADLIGHT~");
            Native.ScaleformMovieMethodAddParamTextureNameString($"Cam Mode");
            Native.EndScaleformMovieMethod();

            Native.BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
            Native.ScaleformMovieMethodAddParamInt(6);

            Native.ScaleformMovieMethodAddParamTextureNameString($"~INPUT_{API.Hash($"vMenu:NoClip")}~");
            Native.ScaleformMovieMethodAddParamTextureNameString($"Toggle NoClip");
            Native.EndScaleformMovieMethod();

            Native.BeginScaleformMovieMethod(Scale, "DRAW_INSTRUCTIONAL_BUTTONS");
            Native.ScaleformMovieMethodAddParamInt(0);
            Native.EndScaleformMovieMethod();

            Native.DrawScaleformMovieFullscreen(Scale, 255, 255, 255, 255, 0);
        }

        var playerPed = API.Players.Local;

        var noclipEntity = playerPed.Ped?.IsPedInAnyVehicle() == true ? playerPed.Ped.GetVehiclePedIsIn() : playerPed.PedIndex;

        Native.FreezeEntityPosition(noclipEntity, true);
        Native.SetEntityInvincible(noclipEntity, true, false);

        Vector3 newPos;
        Native.DisableControlAction(0, 32, false);  // Control.MoveUpOnly
        Native.DisableControlAction(0, 32, false);  // Control.MoveUp
        Native.DisableControlAction(0, 31, false);  // Control.MoveUpDown
        Native.DisableControlAction(0, 33, false);  // Control.MoveDown
        Native.DisableControlAction(0, 33, false);  // Control.MoveDownOnly
        Native.DisableControlAction(0, 34, false);  // Control.MoveLeft
        Native.DisableControlAction(0, 34, false);  // Control.MoveLeftOnly
        Native.DisableControlAction(0, 30, false);  // Control.MoveLeftRight
        Native.DisableControlAction(0, 35, false);  // Control.MoveRight
        Native.DisableControlAction(0, 35, false);  // Control.MoveRightOnly
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
                if (MovingSpeed == speeds.Count)
                {
                    MovingSpeed = 0;
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
        float moveSpeed = MovingSpeed;
        if (MovingSpeed > speeds.Count / 2)
        {
            moveSpeed *= 1.8f;
        }
        moveSpeed = moveSpeed / (1f / Native.GetFrameTime()) * 60;
        newPos = Native.GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, yoff * (moveSpeed + 0.3f), zoff * (moveSpeed + 0.3f));

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
}
*/