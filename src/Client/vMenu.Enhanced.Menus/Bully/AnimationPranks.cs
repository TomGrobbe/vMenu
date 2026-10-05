using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;

namespace vMenu.Enhanced.Menus.Bully;

internal static class AnimationPranks
{
    private const int Looping = 1;

    private const int NotInterruptable = 8;

    private const float BlendOut = -8f;

    private const int DanceMs = 12000;

    private static readonly (string Dictionary, string Clip)[] Dances =
    [
        ("anim@amb@nightclub@mini@dance@dance_solo@male@var_a@", "high_center"),
        ("anim@amb@nightclub@mini@dance@dance_solo@male@var_b@", "high_center"),
        ("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@", "high_center"),
        ("anim@amb@nightclub@mini@dance@dance_solo@female@var_b@", "high_center"),
        ("anim@amb@nightclub@mini@dance@dance_solo@male@var_a@", "med_center"),
        ("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@", "high_center_up"),
    ];

    private static bool _busy;

    public static async Task Dance(BullyRun run)
    {
        var ped = Native.PlayerPedId();
        var (dictionary, clip) = Dances[Dice.Next(Dances.Length)];

        if (_busy)
        {
            run.Busy();

            return;
        }

        if (!Native.IsPedOnFoot(ped))
        {
            run.Skip(BullyEvents.RefusedOnFoot);

            return;
        }

        _busy = true;

        try
        {
            if (!await Streaming.AnimDictAsync(dictionary))
            {
                run.Failed();

                return;
            }

            var generation = BullyState.Generation;
            var until = Native.GetGameTimer() + DanceMs;

            Native.TaskPlayAnim(ped, dictionary, clip, 8f, BlendOut, -1, Looping | NotInterruptable, 0f, false, 0, false);

            run.Started();

            while (Native.GetGameTimer() < until && generation == BullyState.Generation)
            {
                await API.Delay(250);
            }

            Native.StopAnimTask(ped, dictionary, clip, BlendOut);
            Native.RemoveAnimDict(dictionary);
        }
        finally
        {
            _busy = false;
        }
    }
}
