using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Vehicles;

using BullySettings = vMenu.Enhanced.Data.Configuration.Settings.Bully;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Bully;

internal static class VehiclePranks
{
    private const int DriverSeat = -1;

    private const int AnyPassengerSeat = -2;

    private const int ImpulseForce = 1;

    private const float FloatLift = 1.5f;

    private const int FloatMs = 45000;

    private const int RiseMs = 3000;

    private const float HoverDrift = 0.15f;

    private const int TransformMs = 15000;

    private const int WatchMs = 250;

    private const float MinimumRpm = 0.2f;

    private const int SeatSettleMs = 1000;

    private const int ControlAttempts = 4;

    private const string TransformAsset = "scr_as_trans";

    private const string TransformBank = "DLC_AIRRACES/AIR_RACE_01";

    private const string TransformSoundSet = "DLC_Air_Race_Sounds_Player";

    private const string TransformFlash = "MP_TransformRaceFlash";

    private const int TransformFlashMs = 2000;

    private const int TooDark = 15;

    private const int DarkGrey = 60;

    private static readonly HashSet<int> TransformClasses = [0, 1, 2, 3, 4, 5, 6, 7, 9, 12, 18, 20];

    private static bool _floating;

    private static int _original;

    private static int _replacement;

    private static bool _marked;

    private static bool _originalWasMission;

    private static bool _seating;

    public static void Initialize()
    {
        Script.EventHandlers[BullyEvents.TransformSeat] += new Action<string, string>(OnTransformSeat);

        ResourceShutdown.Stopping += RestoreOriginal;
    }

    public static int DrivenVehicle()
    {
        var ped = API.PlayerPedId();
        var vehicle = API.GetVehiclePedIsIn(ped, false);

        return vehicle != 0 && NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false) == ped ? vehicle : 0;
    }

    public static async Task Float(BullyRun run)
    {
        var vehicle = DrivenVehicle();

        if (_floating)
        {
            run.Busy();

            return;
        }

        if (vehicle == 0)
        {
            run.Skip(BullyEvents.RefusedDriving);

            return;
        }

        _floating = true;

        try
        {
            if (!await NetworkEntity.TakeControlAsync(vehicle))
            {
                run.Failed();

                return;
            }

            API.SetVehicleGravity(vehicle, false);
            API.ApplyForceToEntity(vehicle, ImpulseForce, 0f, 0f, FloatLift, 0f, 0f, 0f, 0, false, true, true, false, true);

            run.Started();

            var stopAllCount = BullyState.StopAllCount;
            var started = API.GetGameTimer();
            var hovering = false;

            while (API.GetGameTimer() - started < FloatMs && stopAllCount == BullyState.StopAllCount)
            {
                if (!hovering && API.GetGameTimer() - started >= RiseMs && API.DoesEntityExist(vehicle))
                {
                    hovering = true;

                    var velocity = API.GetEntityVelocity(vehicle);

                    API.SetEntityVelocity(vehicle, velocity.X, velocity.Y, HoverDrift);
                }

                await BaseScript.Delay(250);
            }

            if (API.DoesEntityExist(vehicle))
            {
                await NetworkEntity.TakeControlAsync(vehicle);

                API.SetVehicleGravity(vehicle, true);
            }
        }
        finally
        {
            _floating = false;
        }
    }

    public static async Task Transform(BullyRun run)
    {
        var original = DrivenVehicle();

        if (_original != 0)
        {
            run.Busy();

            return;
        }

        if (original == 0)
        {
            run.Skip(BullyEvents.RefusedDriving);

            return;
        }

        _original = original;
        _originalWasMission = API.IsEntityAMissionEntity(original);

        var replacement = 0;

        try
        {
            if (!API.NetworkGetEntityIsNetworked(original)
                || !await ControlAsync(original)
                || PickModel() is not { } model
                || !await Streaming.ModelAsync(model))
            {
                run.Failed();

                return;
            }

            var position = API.GetEntityCoords(original, false);
            var heading = API.GetEntityHeading(original);
            var momentum = Momentum.Of(original);
            var spawnZ = Bottom(original, position) - MinZ(model);

            API.SetEntityAsMissionEntity(original, true, true);

            _marked = true;

            Conceal(original);

            replacement = NativeFixer.CreateVehicle(model, position.X, position.Y, spawnZ, heading, true, false, true);

            API.SetModelAsNoLongerNeeded(model);

            if (replacement == 0)
            {
                run.Failed();

                return;
            }

            _replacement = replacement;

            API.SetEntityAsMissionEntity(replacement, true, true);

            BullySpawnCleanup.TrackForCleanup(replacement, reportToServer: false);

            run.Started();

            var originalId = API.NetworkGetNetworkIdFromEntity(original);
            var replacementId = API.NetworkGetNetworkIdFromEntity(replacement);

            await SwapAsync(originalId, replacementId, replacement, momentum, DriverSeat);

            var back = API.GetEntityCoords(replacement, false);
            var backHeading = API.GetEntityHeading(replacement);
            var backBottom = Bottom(replacement, back);
            var swapAt = API.GetGameTimer() + TransformMs;

            while (API.GetGameTimer() < swapAt && API.DoesEntityExist(replacement))
            {
                back = API.GetEntityCoords(replacement, false);
                backHeading = API.GetEntityHeading(replacement);
                backBottom = Bottom(replacement, back);

                await BaseScript.Delay(WatchMs);
            }

            if (!API.DoesEntityExist(original))
            {
                return;
            }

            await ControlAsync(original);

            int? seat = DriverSeat;
            Momentum? backMomentum = null;

            if (API.DoesEntityExist(replacement))
            {
                await ControlAsync(replacement);

                back = API.GetEntityCoords(replacement, false);
                backHeading = API.GetEntityHeading(replacement);
                backBottom = Bottom(replacement, back);
                backMomentum = Momentum.Of(replacement);
                seat = SeatOf(replacement, API.PlayerPedId());

                Conceal(replacement);
            }

            API.SetEntityCoordsNoOffset(original, back.X, back.Y, backBottom - MinZ(NativeFixer.GetEntityModel(original)), false, false, false);
            API.SetEntityHeading(original, backHeading);

            Reveal(original);

            await SwapAsync(replacementId, originalId, original, backMomentum, seat);
        }
        finally
        {
            if (replacement != 0)
            {
                Conceal(replacement);

                await BullySpawnCleanup.DeleteAsync(replacement);
            }

            if (_marked && API.DoesEntityExist(original))
            {
                Reveal(original);
                Release(original);
            }

            _original = 0;
            _replacement = 0;
            _marked = false;
        }
    }

    // A restart mid transform would otherwise leave the real vehicle hidden and frozen where it was.
    private static void RestoreOriginal()
    {
        if (_original == 0 || !_marked || !API.DoesEntityExist(_original))
        {
            return;
        }

        var ped = API.PlayerPedId();
        int? seat = DriverSeat;

        if (_replacement != 0 && API.DoesEntityExist(_replacement))
        {
            var at = API.GetEntityCoords(_replacement, false);

            seat = SeatOf(_replacement, ped);

            Conceal(_replacement);

            API.SetEntityCoordsNoOffset(_original, at.X, at.Y, at.Z, false, false, false);
            API.SetEntityHeading(_original, API.GetEntityHeading(_replacement));
        }

        Reveal(_original);

        if (seat is { } inside)
        {
            API.SetPedIntoVehicle(ped, _original, SeatFor(_original, inside));
            API.SetEntityVisible(ped, true, false);
        }

        Release(_original);
    }

    private static void Release(int vehicle)
    {
        if (_originalWasMission)
        {
            return;
        }

        NativeFixer.SetVehicleAsNoLongerNeeded(ref vehicle);
    }

    private static async Task TransformFxAsync(int vehicle, bool flash)
    {
        var bank = await Streaming.AudioBankAsync(TransformBank);

        if (bank)
        {
            API.PlaySoundFromEntity(-1, "Vehicle_Transform", vehicle, TransformSoundSet, true, 250);
        }

        if (flash)
        {
            API.AnimpostfxPlay(TransformFlash, TransformFlashMs, false);
        }

        var smoke = 0;
        var asset = !ClientConfig.Value(BullySettings.DisableParticleEffects) && await Streaming.PtfxAsync(TransformAsset);

        if (asset)
        {
            NativeFixer.GetVehicleCustomPrimaryColour(vehicle, out var red, out var green, out var blue);

            if (red == green && green == blue && red <= TooDark)
            {
                red = green = blue = DarkGrey;
            }

            API.UseParticleFxAsset(TransformAsset);

            smoke = NativeFixer.StartNetworkedParticleFxLoopedOnEntity(
                "scr_as_trans_smoke",
                vehicle,
                0f, 0f, 0f,
                0f, 0f, 0f,
                1f, false, false, false,
                red / 255f, green / 255f, blue / 255f,
                true);

            if (smoke != 0)
            {
                var bike = API.IsThisModelABike(NativeFixer.GetEntityModel(vehicle));

                API.SetParticleFxLoopedEvolution(smoke, "width", bike ? 0f : 1f, false);
                API.SetParticleFxLoopedEvolution(smoke, "length", bike ? 0f : 1f, false);
            }
        }

        await BaseScript.Delay(TransformFlashMs);

        if (smoke != 0)
        {
            API.StopParticleFxLooped(smoke, false);
        }

        if (flash)
        {
            API.AnimpostfxStop(TransformFlash);
        }

        if (asset)
        {
            Streaming.ReleasePtfx(TransformAsset);
        }

        if (bank)
        {
            Streaming.ReleaseAudioBank(TransformBank);
        }
    }

    private static async Task<bool> ControlAsync(int vehicle)
    {
        for (var attempt = 0; attempt < ControlAttempts; attempt++)
        {
            if (await NetworkEntity.TakeControlAsync(vehicle))
            {
                return true;
            }
        }

        return false;
    }

    private static void Conceal(int vehicle)
    {
        if (!API.DoesEntityExist(vehicle))
        {
            return;
        }

        API.SetEntityVisible(vehicle, false, false);
        API.SetEntityCollision(vehicle, false, false);
        API.FreezeEntityPosition(vehicle, true);
    }

    private static void Reveal(int vehicle)
    {
        API.SetEntityCollision(vehicle, true, true);
        API.SetEntityVisible(vehicle, true, false);
        API.FreezeEntityPosition(vehicle, false);
    }

    private static float Bottom(int vehicle, Vector3 centre) => centre.Z + MinZ(NativeFixer.GetEntityModel(vehicle));

    private static float MinZ(uint model)
    {
        NativeFixer.GetModelDimensions(model, out var min, out _);

        return min.Z;
    }

    private static int? SeatOf(int vehicle, int ped)
    {
        for (var seat = DriverSeat; seat < API.GetVehicleMaxNumberOfPassengers(vehicle); seat++)
        {
            if (NativeFixer.GetPedInVehicleSeat(vehicle, seat, false) == ped)
            {
                return seat;
            }
        }

        return null;
    }

    private static int SeatFor(int vehicle, int seat) =>
        seat == DriverSeat
        || (seat < API.GetVehicleMaxNumberOfPassengers(vehicle) && NativeFixer.IsVehicleSeatFree(vehicle, seat, false))
            ? seat
            : AnyPassengerSeat;

    private static async Task SwapAsync(int fromId, int toId, int to, Momentum? momentum, int? seat)
    {
        BaseScript.TriggerServerEvent(
            BullyEvents.TransformSwap,
            fromId.ToString(CultureInfo.InvariantCulture),
            toId.ToString(CultureInfo.InvariantCulture));

        if (momentum is { } moving)
        {
            if (seat == DriverSeat)
            {
                API.SetVehicleEngineOn(to, true, true, false);
            }

            API.SetVehicleForwardSpeed(to, moving.ForwardSpeed);
            API.SetEntityVelocity(to, moving.Velocity.X, moving.Velocity.Y, moving.Velocity.Z);

            if (seat == DriverSeat && moving.Rpm > MinimumRpm)
            {
                API.SetVehicleCurrentRpm(to, moving.Rpm);
            }
        }

        if (seat is { } inside)
        {
            API.SetPedIntoVehicle(API.PlayerPedId(), to, SeatFor(to, inside));
            API.SetEntityVisible(API.PlayerPedId(), true, false);
        }

        BullyTask.Run(() => TransformFxAsync(to, seat is not null), "Transform.Effects");

        await BaseScript.Delay(SeatSettleMs);
    }

    private static uint? PickModel()
    {
        var models = NativeFixer.GetAllVehicleModels()
            .Select(Streaming.Hash)
            .Where(model => API.IsModelInCdimage(model)
                && API.IsThisModelACar(model)
                && TransformClasses.Contains(API.GetVehicleClassFromName(model)))
            .ToArray();

        return models.Length == 0 ? null : models[Dice.Next(models.Length)];
    }

    private static void OnTransformSeat(string netId, string seat)
    {
        if (_seating)
        {
            return;
        }

        _seating = true;

        BullyTask.Run(() => OnTransformSeatAsync(netId, seat), "TransformSeat");
    }

    private static async Task OnTransformSeatAsync(string netId, string seat)
    {
        try
        {
            if (int.TryParse(netId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                && int.TryParse(seat, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)
                && await NetworkEntity.ResolveAsync(id) is var vehicle and not 0)
            {
                API.SetPedIntoVehicle(API.PlayerPedId(), vehicle, SeatFor(vehicle, index));
                API.SetEntityVisible(API.PlayerPedId(), true, false);
            }
        }
        finally
        {
            _seating = false;
        }
    }
}

internal sealed class Momentum(Vector3 velocity, float forwardSpeed, float rpm)
{
    public Vector3 Velocity { get; } = velocity;

    public float ForwardSpeed { get; } = forwardSpeed;

    public float Rpm { get; } = rpm;

    public static Momentum Of(int vehicle) => new(
        API.GetEntityVelocity(vehicle),
        API.GetEntitySpeedVector(vehicle, true).Y,
        API.GetVehicleCurrentRpm(vehicle));
}
