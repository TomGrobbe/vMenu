using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Misc;
using vMenu.Enhanced.Menus.Players.Appearance;

namespace vMenu.Enhanced.Menus.Bully;

internal static class ScreenPranks
{
    private const string DrunkWalk = "move_m@drunk@verydrunk";

    private const string DrunkShake = "DRUNK_SHAKE";

    private const string ExplosionShake = "SMALL_EXPLOSION_SHAKE";

    private const string DrugsIn = "DrugsMichaelAliensFightIn";

    private const string DrugsOut = "DrugsMichaelAliensFightOut";

    private const string Flash = "MP_TransformRaceFlash";

    public const string ClownLaugh = "CLOWN_LAUGH";

    public const string ClownVoice = "CLOWNS";

    private const string FireworksAsset = "scr_indep_fireworks";

    private const string BeastBank = "DLC_APARTMENT/APT_BEAST";

    private const string BeastSoundSet = "APT_BvS_Soundset";

    private const string BeastAsset = "scr_powerplay";

    private const string FlameAsset = "weap_xs_vehicle_weapons";

    private const int IntoxicatedMs = 25000;

    private const int TimecycleMs = 20000;

    private const int DrugsOutMs = 3500;

    private const int FlashMs = 1500;

    private const int FireworkCount = 8;

    private const float FireworkRadius = 6f;

    private const float BeastDistance = 16f;

    private static readonly (string Prop, string Effect)[] Fireworks =
    [
        ("ind_prop_firework_03", "scr_indep_firework_fountain"),
        ("ind_prop_firework_01", "scr_indep_firework_shotburst"),
    ];

    private static readonly (float Red, float Green, float Blue)[] FireworkColours =
    [
        (255f, 0f, 0f),
        (0f, 255f, 0f),
        (0f, 0f, 255f),
        (255f, 255f, 0f),
        (0f, 255f, 255f),
        (255f, 0f, 255f),
        (255f, 255f, 255f),
    ];

    public static IReadOnlyList<string> Timecycles { get; } =
    [
        "spectator4",
        "CAMERA_BW",
        "CAMERA_secuirity_FUZZ",
        "spectator5",
        "stoned_aliens",
        "drug_wobbly",
        "REDMIST",
    ];

    public static IReadOnlyList<(string Name, string SoundSet, string? Bank)> Sounds { get; } =
    [
        ("Beast_Attack", BeastSoundSet, BeastBank),
        ("FE_Spawn", "DLC_Tuner_Halloween_Clown_Soundset", "DLC_TUNER/DLC_Tuner_Killer_Clown"),
        ("FE_Spawn", "DLC_Tuner_Halloween_Slasher_Soundset", "DLC_TUNER/DLC_Tuner_Killer_SackSlasher"),
        ("FE_Spawn", "Freemode_Mirror_Slash_sounds", "DLC_MPSUM2/Mirror_Slash"),
    ];

    public static int SoundCount => Sounds.Count + 1;

    private static readonly Window Intoxicated = new();

    private static readonly Window Coloured = new();

    private static bool _beast;

    public static void Initialize() => BullyState.Stopped += OnStopped;

    public static Task Drunk(BullyRun run) => IntoxicateAsync(run, drugged: false);

    public static Task Drugged(BullyRun run) => IntoxicateAsync(run, drugged: true);

    private static async Task IntoxicateAsync(BullyRun run, bool drugged)
    {
        var started = Intoxicated.Extend(IntoxicatedMs);
        var ped = Native.PlayerPedId();

        run.Started();

        if (await Streaming.ClipSetAsync(DrunkWalk))
        {
            Native.SetPedMovementClipset(ped, DrunkWalk, 1f);
        }

        Native.SetPedIsDrunk(ped, true);
        Native.ShakeGameplayCam(DrunkShake, drugged ? 2f : 1.2f);
        Native.SetTransitionTimecycleModifier(drugged ? "stoned_aliens" : "DRUNK", 3f);

        if (drugged && !Native.AnimpostfxIsRunning(DrugsIn))
        {
            Native.AnimpostfxPlay(DrugsIn, 0, true);
        }

        if (started)
        {
            await Intoxicated.WaitAsync();

            if (Intoxicated.Close())
            {
                await SoberUpAsync();
            }
        }
    }

    private static async Task SoberUpAsync()
    {
        var ped = Native.PlayerPedId();
        var drugged = Native.AnimpostfxIsRunning(DrugsIn);

        Native.AnimpostfxStop(DrugsIn);
        Native.SetPedIsDrunk(ped, false);
        Native.StopGameplayCamShaking(false);
        Native.ResetPedMovementClipset(ped, 1f);

        RestoreTimecycle();

        await PedWalkingStyle.ReapplyAsync();

        if (drugged)
        {
            await PostFxAsync(DrugsOut, DrugsOutMs);
        }
    }

    public static async Task Timecycle(BullyRun run)
    {
        var index = run.Option;

        if ((uint)index >= (uint)Timecycles.Count)
        {
            run.Failed();

            return;
        }

        var started = Coloured.Extend(TimecycleMs);

        Native.SetTransitionTimecycleModifier(Timecycles[index], 2f);

        run.Started();

        if (started)
        {
            await Coloured.WaitAsync();

            if (Coloured.Close())
            {
                RestoreTimecycle();
            }
        }
    }

    public static async Task Sound(BullyRun run)
    {
        var index = run.Option;

        if (index == Sounds.Count)
        {
            await MountainLion.Growl(run);

            return;
        }

        if ((uint)index >= (uint)Sounds.Count)
        {
            run.Failed();

            return;
        }

        var (name, soundSet, bank) = Sounds[index];

        if (bank is not null && !await Streaming.AudioBankAsync(bank))
        {
            run.Failed();

            return;
        }

        Native.PlaySoundFrontend(-1, name, soundSet, true);
        Native.ShakeGameplayCam(ExplosionShake, 0.4f);

        run.Started();

        await PostFxAsync(Flash, FlashMs);
        await API.Delay(8000);

        if (bank is not null)
        {
            Streaming.ReleaseAudioBank(bank);
        }
    }

    public static async Task Firework(BullyRun run)
    {
        if (!await Streaming.PtfxAsync(FireworksAsset))
        {
            run.Failed();

            return;
        }

        var stopAllCount = BullyState.StopAllCount;
        var centre = Native.GetEntityCoords(Native.PlayerPedId(), false);
        var start = Dice.Float(0f, MathF.Tau);
        var launchers = new List<(int Prop, string Effect)>();

        try
        {
            for (var index = 0; index < FireworkCount; index++)
            {
                var (model, effect) = Fireworks[index % Fireworks.Length];
                var angle = start + index * MathF.Tau / FireworkCount;
                var spot = centre + new Vector3(MathF.Cos(angle) * FireworkRadius, MathF.Sin(angle) * FireworkRadius, 0f);

                if (!await Streaming.ModelAsync(model))
                {
                    continue;
                }

                var prop = Native.CreateObject(Streaming.Hash(model), spot.X, spot.Y, spot.Z, false, false, false);

                Native.SetModelAsNoLongerNeeded(Streaming.Hash(model));

                if (prop != 0)
                {
                    Native.PlaceObjectOnGroundProperly(prop);

                    launchers.Add((prop, effect));
                }
            }

            if (launchers.Count == 0)
            {
                run.Failed();

                return;
            }

            run.Started();

            foreach (var (prop, effect) in launchers.OrderBy(_ => Dice.Next(int.MaxValue)))
            {
                if (stopAllCount != BullyState.StopAllCount)
                {
                    break;
                }

                var (red, green, blue) = FireworkColours[Dice.Next(FireworkColours.Length)];

                Native.SetParticleFxNonLoopedColour(red, green, blue);

                Fx.Burst(FireworksAsset, effect, Native.GetEntityCoords(prop, false) + new Vector3(0f, 0f, 0.12f));

                Native.ShakeGameplayCam(ExplosionShake, 0.15f);

                await API.Delay(Dice.Next(250, 600));
            }

            await API.Delay(6000);
        }
        finally
        {
            foreach (var (prop, _) in launchers)
            {
                var doomed = prop;

                Native.DeleteObject(ref doomed);
            }

            Streaming.ReleasePtfx(FireworksAsset);
        }
    }

    public static async Task Beast(BullyRun run)
    {
        if (_beast)
        {
            run.Busy();

            return;
        }

        _beast = true;

        var bank = false;
        var beast = false;
        var flame = false;

        try
        {
            bank = await Streaming.AudioBankAsync(BeastBank);
            beast = await Streaming.PtfxAsync(BeastAsset);
            flame = await Streaming.PtfxAsync(FlameAsset);

            if (!bank || !beast || !flame)
            {
                run.Failed();

                return;
            }

            run.Started();

            var player = Native.GetEntityCoords(Native.PlayerPedId(), false);
            var camera = Native.GetFinalRenderedCamCoord();
            var facing = new Vector3(player.X - camera.X, player.Y - camera.Y, 0f);
            var spot = player + (facing.LengthSquared() > 0.01f ? Vector3.Normalize(facing) : Vector3.UnitY) * BeastDistance;

            Fx.Burst(BeastAsset, "scr_powerplay_beast_appear", spot, 2f);
            Fx.Sound("Beast_Attack", BeastSoundSet, spot);
            Native.ShakeGameplayCam(ExplosionShake, 0.6f);

            await API.Delay(500);

            var side = Vector3.Normalize(new Vector3(spot.Y - player.Y, player.X - spot.X, 0f)) * (Dice.Next(2) == 0 ? -1f : 1f);
            var breath = Fx.Loop(FlameAsset, "muz_xs_turret_flamethrower_looping", spot + new Vector3(0f, 0f, 1.2f), Fx.HeadingTowards(spot, player + side), 3f);

            await API.Delay(1800);

            Native.StopParticleFxLooped(breath, true);

            Fx.Burst(BeastAsset, "scr_powerplay_beast_vanish", spot, 2f);
            Fx.Sound("Beast_Attack", BeastSoundSet, spot);

            await API.Delay(3000);
        }
        finally
        {
            if (beast)
            {
                Streaming.ReleasePtfx(BeastAsset);
            }

            if (flame)
            {
                Streaming.ReleasePtfx(FlameAsset);
            }

            if (bank)
            {
                Streaming.ReleaseAudioBank(BeastBank);
            }

            _beast = false;
        }
    }

    private static async Task PostFxAsync(string effect, int durationMs)
    {
        Native.AnimpostfxPlay(effect, 0, false);

        await API.Delay(durationMs);

        Native.AnimpostfxStop(effect);
    }

    private static void RestoreTimecycle()
    {
        Native.ClearTimecycleModifier();

        TimecycleState.Reapply();
    }

    private static void OnStopped()
    {
        if (Intoxicated.Close())
        {
            _ = SoberUpAsync();
        }

        if (Coloured.Close())
        {
            RestoreTimecycle();
        }
    }
}
