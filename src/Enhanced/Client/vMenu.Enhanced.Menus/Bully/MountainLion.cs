using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;

namespace vMenu.Enhanced.Menus.Bully;

internal static class MountainLion
{
    public const int AnimalPedType = 28;

    private const int AngryMood = 0;

    private const string GrowlDictionary = "creatures@cougar@melee@";

    private const string GrowlClip = "growling";

    private const string GrowlFacialClip = "growling_facial";

    private const float MinDistance = 7f;

    private const float MaxDistance = 13f;

    private const float GroundProbeHeight = 10f;

    private const int LionCount = 2;

    private const int SecondGrowlDelayMs = 500;

    private const int GrowlStartMs = 500;

    private const int GrowlTimeoutMs = 8000;

    private static bool _prowling;

    public static async Task Growl(BullyRun run)
    {
        if (_prowling)
        {
            run.Busy();

            return;
        }

        _prowling = true;

        var lions = new List<int>();
        var stopAllCount = BullyState.StopAllCount;

        try
        {
            var model = Streaming.Hash(BullyModels.MountainLion);

            if (!await Streaming.ModelAsync(model))
            {
                run.Failed();

                return;
            }

            // Animal vocalizations cover only boars, chickens, dogs and horses, the cougar's growl lives in this clip.
            if (!await Streaming.AnimDictAsync(GrowlDictionary))
            {
                Native.SetModelAsNoLongerNeeded(model);

                run.Failed();

                return;
            }

            var spot = SpotAround(Native.GetEntityCoords(Native.PlayerPedId(), false));

            for (var index = 0; index < LionCount; index++)
            {
                if (Spawn(model, spot) is var lion and not 0)
                {
                    lions.Add(lion);
                }
            }

            Native.SetModelAsNoLongerNeeded(model);

            var growling = new List<int>();

            foreach (var lion in lions)
            {
                if (growling.Count > 0)
                {
                    await API.Delay(SecondGrowlDelayMs);
                }

                if (await StartGrowlAsync(lion))
                {
                    growling.Add(lion);

                    run.Started();
                }
            }

            if (growling.Count == 0)
            {
                run.Failed();

                return;
            }

            var giveUpAt = Native.GetGameTimer() + GrowlTimeoutMs;

            while (growling.Any(IsGrowling)
                && Native.GetGameTimer() < giveUpAt
                && stopAllCount == BullyState.StopAllCount)
            {
                await API.Delay(100);
            }
        }
        finally
        {
            foreach (var lion in lions)
            {
                await BullySpawnCleanup.DeleteAsync(lion);
            }

            Native.RemoveAnimDict(GrowlDictionary);

            _prowling = false;
        }
    }

    private static int Spawn(uint model, Vector3 spot)
    {
        var lion = Native.CreatePed(AnimalPedType, model, spot.X, spot.Y, spot.Z, Dice.Float(0f, 360f), true, false);

        if (lion == 0)
        {
            return 0;
        }

        Native.SetEntityAsMissionEntity(lion, true, true);

        BullySpawnCleanup.TrackForCleanup(lion);

        Native.SetEntityVisible(lion, false, false);
        Native.SetEntityCollision(lion, false, false);
        Native.FreezeEntityPosition(lion, true);
        Native.SetEntityInvincible(lion, true, false);
        Native.SetBlockingOfNonTemporaryEvents(lion, true);
        Native.SetAnimalMood(lion, AngryMood);

        return lion;
    }

    private static Vector3 SpotAround(Vector3 centre)
    {
        var angle = Dice.Float(0f, MathF.Tau);
        var distance = Dice.Float(MinDistance, MaxDistance);
        var spot = centre + new Vector3(MathF.Cos(angle) * distance, MathF.Sin(angle) * distance, 0f);

        return Native.GetGroundZFor_3dCoord(spot.X, spot.Y, centre.Z + GroundProbeHeight, out var groundZ, false, false)
            ? spot with { Z = groundZ }
            : spot;
    }

    private static async Task<bool> StartGrowlAsync(int lion)
    {
        Native.TaskPlayAnim(lion, GrowlDictionary, GrowlClip, 8f, -8f, -1, 0, 0f, false, 0, false);
        Native.PlayFacialAnim(lion, GrowlFacialClip, GrowlDictionary);

        var startBy = Native.GetGameTimer() + GrowlStartMs;

        while (!IsGrowling(lion))
        {
            if (Native.GetGameTimer() > startBy)
            {
                return false;
            }

            await API.Delay(0);
        }

        return true;
    }

    private static bool IsGrowling(int lion) => Native.IsEntityPlayingAnim(lion, GrowlDictionary, GrowlClip, 3);
}
