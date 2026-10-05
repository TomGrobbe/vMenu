using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Events;

using vMenu.Enhanced.Menus.Players;

namespace vMenu.Enhanced.Menus.Bully;

internal static class AlienAbduction
{
    private const string Bank = "DLC_MPSUM2/MPSUM2_Generic";

    private const string SoundSet = "Sight_Seeing_Sounds";

    private const string FloatDictionary = "anim@scripted@freemode@ufo_invasion@ufo_float@male@";

    private const string GetUpDictionary = "get_up@standard";

    private const float ShipHeight = 120f;

    private const int SoundRange = 250;

    private const string ReactClip = "react_upperbody";

    private const string FloatClip = "float";

    private const string GetUpClip = "front";

    private const int HoverMs = 2500;

    private const int ReactFailsafeMs = 4000;

    private const int FloatFailsafeMs = 12000;

    private const float ReactDone = 0.99f;

    private const float FloatDone = 0.7f;

    private const int WhiteHoldMs = 2000;

    private const float MinimumTravel = 250f;

    private const int WhiteoutMs = 3000;

    private const int ReactFlags = 8 | 16 | 131072 | 1048576;

    private const int FloatFlags = 8 | 2 | 131072 | 1048576 | 2048 | 512 | 1024;

    private const int GetUpFlags = 8 | 131072 | 1048576;

    private const int AfterFade = 7;

    private static readonly (Vector3 Position, float Heading)[] WakeUpSpots =
    [
        (new(2445.3259f, 3777.7319f, 40.2572f), 279.8778f),
        (new(1571.6476f, 3576.7224f, 31.6067f), 120.3943f),
        (new(1324.4664f, 2981.9822f, 40.3988f), 87.6742f),
        (new(-294.2128f, 2544.1169f, 74.4209f), 275.1376f),
        (new(-659.0776f, 2454.7395f, 47.9649f), 68.2270f),
        (new(-2305.3804f, 2545.9353f, 0.5825f), 0.9353f),
        (new(-2057.1567f, 3240.1831f, 30.4989f), 60.7713f),
        (new(-576.3850f, 5462.1582f, 59.8029f), 192.2358f),
        (new(453.8146f, 5565.7817f, 780.1841f), 280.8828f),
        (new(424.0188f, 6472.5415f, 34.8781f), 231.9386f),
        (new(1532.6165f, 6340.1592f, 23.1857f), 105.1666f),
        (new(3852.0015f, 4463.7583f, 1.7185f), 90.4840f),
        (new(2394.2075f, 4907.2217f, 41.5675f), 215.0774f),
        (new(1777.1290f, 4823.7485f, 33.8178f), 23.1621f),
        (new(-884.9773f, 4429.7158f, 19.9556f), 255.9288f),
        (new(125.5299f, 630.8699f, 205.4403f), 174.3078f),
    ];

    private static bool _active;

    private static bool _white;

    public static void Initialize() => ResourceShutdown.Stopping += OnShutdown;

    // A restart mid abduction would otherwise leave the target frozen and invisible in the air.
    private static void OnShutdown()
    {
        if (!_active)
        {
            return;
        }

        _white = false;

        var ped = Native.PlayerPedId();

        Native.FreezeEntityPosition(ped, false);
        Native.NetworkFadeInEntity(ped, true, false);
    }

    public static async Task Start(BullyRun run)
    {
        var ped = Native.PlayerPedId();

        if (_active)
        {
            run.Busy();

            return;
        }

        if (!Native.IsPedOnFoot(ped))
        {
            run.Skip(BullyEvents.RefusedOnFoot);

            return;
        }

        _active = true;

        run.Started();

        var ship = 0;
        var sound = -1;
        var bank = false;
        var faded = false;
        var announced = false;
        var beamGround = Vector3.Zero;

        try
        {
            var generation = BullyState.Generation;
            var ground = Native.GetEntityCoords(ped, false);
            var shipAt = new Vector3(ground.X, ground.Y, ground.Z + ShipHeight);

            beamGround = ground;

            ship = await CreateShipAsync(shipAt);
            bank = await Streaming.AudioBankAsync(Bank);

            if (bank)
            {
                sound = Native.GetSoundId();

                Native.PlaySoundFromCoord(sound, "UFO_Ambience", shipAt.X, shipAt.Y, shipAt.Z, SoundSet, true, SoundRange, false);
            }

            UfoBeam.Announce(true, ground, ShipHeight);

            announced = true;

            var animated = await Streaming.AnimDictAsync(FloatDictionary) && await Streaming.AnimDictAsync(GetUpDictionary);

            await BeamAsync(ship, ground, shipAt, HoverMs, generation, null);

            if (generation != BullyState.Generation)
            {
                return;
            }

            if (bank)
            {
                Fx.Sound("Abducted", SoundSet, shipAt, true, SoundRange);
            }

            ped = Native.PlayerPedId();

            if (animated)
            {
                Native.TaskPlayAnim(ped, FloatDictionary, ReactClip, 8f, -8f, -1, ReactFlags, 0f, false, 0, false);

                await BeamAsync(ship, ground, shipAt, ReactFailsafeMs, generation, () => Phase(ped, ReactClip) > ReactDone);

                Native.TaskPlayAnim(ped, FloatDictionary, FloatClip, 8f, -8f, -1, FloatFlags, 0f, false, 0, false);

                await BeamAsync(ship, ground, shipAt, FloatFailsafeMs, generation, () => Phase(ped, FloatClip) > FloatDone);
            }
            else
            {
                await BeamAsync(ship, ground, shipAt, ReactFailsafeMs, generation, null);
            }

            if (generation != BullyState.Generation)
            {
                return;
            }

            Native.FreezeEntityPosition(ped, true);
            Native.NetworkFadeOutEntity(ped, false, true);

            faded = true;

            await WhiteoutAsync(fadeIn: true);

            _white = true;

            BullyTask.Run(HoldWhiteAsync, "AlienAbduction.HoldWhite");

            UfoBeam.Announce(false, ground, ShipHeight);

            announced = false;

            await NpcSupport.DeleteAsync(ship);

            ship = 0;

            StopSound(ref sound);

            var wakeUp = WakeUpSpot(ground);

            await PlayerTeleport.ToCoordsAsync(wakeUp.Position, wakeUp.Heading);

            faded = false;
            ped = Native.PlayerPedId();

            Native.FreezeEntityPosition(ped, true);

            if (animated)
            {
                Native.TaskPlayAnim(ped, GetUpDictionary, GetUpClip, 8f, -8f, -1, GetUpFlags, 0f, false, 0, false);
            }

            var holdUntil = Native.GetGameTimer() + WhiteHoldMs;

            while (Native.GetGameTimer() < holdUntil)
            {
                if (animated)
                {
                    Native.SetEntityAnimCurrentTime(ped, GetUpDictionary, GetUpClip, 0f);
                }

                await API.Delay(0);
            }

            _white = false;

            await WhiteoutAsync(fadeIn: false);
        }
        finally
        {
            _white = false;

            var current = Native.PlayerPedId();

            Native.FreezeEntityPosition(current, false);

            if (faded)
            {
                Native.NetworkFadeInEntity(current, true, false);
            }

            if (announced)
            {
                UfoBeam.Announce(false, beamGround, ShipHeight);
            }

            await NpcSupport.DeleteAsync(ship);

            StopSound(ref sound);

            if (bank)
            {
                Streaming.ReleaseAudioBank(Bank);
            }

            Native.RemoveAnimDict(FloatDictionary);
            Native.RemoveAnimDict(GetUpDictionary);

            _active = false;
        }
    }

    private static float Phase(int ped, string clip) =>
        Native.IsEntityPlayingAnim(ped, FloatDictionary, clip, 3)
            ? Native.GetEntityAnimCurrentTime(ped, FloatDictionary, clip)
            : 0f;

    private static (Vector3 Position, float Heading) WakeUpSpot(Vector3 awayFrom)
    {
        var far = WakeUpSpots.Where(spot => Vector3.Distance(spot.Position, awayFrom) >= MinimumTravel).ToArray();

        return far.Length > 0 ? far[Dice.Next(far.Length)] : WakeUpSpots[0];
    }

    private static async Task<int> CreateShipAsync(Vector3 at)
    {
        var model = Streaming.Hash(BullyModels.Ship);

        if (!await Streaming.ModelAsync(model))
        {
            model = Streaming.Hash(BullyModels.FallbackShip);

            if (!await Streaming.ModelAsync(model))
            {
                return 0;
            }
        }

        var ship = Native.CreateObjectNoOffset(model, at.X, at.Y, at.Z, true, false, false, false);

        Native.SetModelAsNoLongerNeeded(model);

        if (ship == 0)
        {
            return 0;
        }

        Native.SetEntityCollision(ship, false, false);
        Native.FreezeEntityPosition(ship, true);
        Native.SetEntityLodDist(ship, 9000);

        NpcSupport.Own(ship);

        return ship;
    }

    private static async Task BeamAsync(int ship, Vector3 ground, Vector3 shipAt, int durationMs, int generation, Func<bool>? done)
    {
        var endsAt = Native.GetGameTimer() + durationMs;

        while (Native.GetGameTimer() < endsAt && generation == BullyState.Generation && done?.Invoke() != true)
        {
            if (ship != 0 && Native.DoesEntityExist(ship))
            {
                Native.SetEntityHeading(ship, Native.GetEntityHeading(ship) + Native.GetFrameTime() * 10f);
            }

            UfoBeam.Draw(ground, shipAt.Z - ground.Z);

            await API.Delay(0);
        }
    }

    private static async Task WhiteoutAsync(bool fadeIn)
    {
        var endsAt = Native.GetGameTimer() + WhiteoutMs;

        while (Native.GetGameTimer() < endsAt)
        {
            var progress = 1f - (endsAt - Native.GetGameTimer()) / (float)WhiteoutMs;
            var alpha = (int)(255 * (fadeIn ? progress : 1f - progress));

            DrawWhite(Math.Clamp(alpha, 0, 255));

            await API.Delay(0);
        }
    }

    private static async Task HoldWhiteAsync()
    {
        while (_white)
        {
            DrawWhite(255);

            await API.Delay(0);
        }
    }

    private static void DrawWhite(int alpha)
    {
        Native.SetScriptGfxDrawOrder(AfterFade);
        Native.DrawRect(0.5f, 0.5f, 3f, 3f, 255, 255, 255, alpha, false);
    }

    private static void StopSound(ref int sound)
    {
        if (sound == -1)
        {
            return;
        }

        Native.StopSound(sound);
        Native.ReleaseSoundId(sound);

        sound = -1;
    }
}
