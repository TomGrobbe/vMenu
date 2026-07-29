using System.Numerics;

using CitizenFX.Base;
using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.IamAproblem;

internal class ThisCausesProblems
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


    internal void Fuck()
    {
        NoClipper();
        NoClipperKeyer();
    }
    async void NoClipper()
    {
        SharedAPI.Log.Info($"id: {Native.PlayerPedId()}");
        //while (true)
        //{
        //    //await NoClipHandler();
        //    await API.Yield();
        //}
    }

    static async void NoClipperKeyer()
    {
        //while (true)
        //{
        //    NoClipControls();
        //    await API.Yield();
        //}
    }


    private static void NoClipControls()
    {
        //if (Native.IsControlJustPressed(0, 289) || Native.IsDisabledControlJustPressed(0, 289))
        {
            NoclipActive = !NoclipActive;
        }
    }

    internal static void SetNoclipActive(bool active)
    {
        NoclipActive = active;

        if (!active)
        {
            //Native.SetScaleformMovieAsNoLongerNeeded(out Scale);

            Scale = -1;
        }
    }

    internal static bool IsNoclipActive()
    {
        return NoclipActive;
    }

    private async Task NoClipHandler()
    {
        
    }

}