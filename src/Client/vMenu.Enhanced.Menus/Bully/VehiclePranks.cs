using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Vehicles;

using BullySettings = vMenu.Enhanced.Data.Configuration.Settings.Bully;

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
        API.OnNetEvent(BullyEvents.TransformSeat, new Action<string, string>(OnTransformSeat), false);

        ResourceShutdown.Stopping += RestoreOriginal;
    }

    public static int DrivenVehicle()
    {
        var ped = Native.PlayerPedId();
        var vehicle = Native.GetVehiclePedIsIn(ped, false);

        return vehicle != 0 && Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) == ped ? vehicle : 0;
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

            Native.SetVehicleGravity(vehicle, false);
            Native.ApplyForceToEntity(vehicle, ImpulseForce, 0f, 0f, FloatLift, 0f, 0f, 0f, 0, false, true, true, false, true);

            run.Started();

            var stopAllCount = BullyState.StopAllCount;
            var started = Native.GetGameTimer();
            var hovering = false;

            while (Native.GetGameTimer() - started < FloatMs && stopAllCount == BullyState.StopAllCount)
            {
                if (!hovering && Native.GetGameTimer() - started >= RiseMs && Native.DoesEntityExist(vehicle))
                {
                    hovering = true;

                    var velocity = Native.GetEntityVelocity(vehicle);

                    Native.SetEntityVelocity(vehicle, velocity.X, velocity.Y, HoverDrift);
                }

                await API.Delay(250);
            }

            if (Native.DoesEntityExist(vehicle))
            {
                await NetworkEntity.TakeControlAsync(vehicle);

                Native.SetVehicleGravity(vehicle, true);
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
        _originalWasMission = Native.IsEntityAMissionEntity(original);

        var replacement = 0;

        try
        {
            if (!Native.NetworkGetEntityIsNetworked(original)
                || !await ControlAsync(original)
                || PickModel() is not { } model
                || !await Streaming.ModelAsync(model))
            {
                run.Failed();

                return;
            }

            var position = Native.GetEntityCoords(original, false);
            var heading = Native.GetEntityHeading(original);
            var momentum = Momentum.Of(original);
            var spawnZ = Bottom(original, position) - MinZ(model);

            Native.SetEntityAsMissionEntity(original, true, true);

            _marked = true;

            Conceal(original);

            replacement = Native.CreateVehicle(model, position.X, position.Y, spawnZ, heading, true, false, true);

            Native.SetModelAsNoLongerNeeded(model);

            if (replacement == 0)
            {
                run.Failed();

                return;
            }

            _replacement = replacement;

            Native.SetEntityAsMissionEntity(replacement, true, true);

            NpcSupport.Own(replacement, report: false);

            run.Started();

            var originalId = Native.NetworkGetNetworkIdFromEntity(original);
            var replacementId = Native.NetworkGetNetworkIdFromEntity(replacement);

            await SwapAsync(originalId, replacementId, replacement, momentum, DriverSeat);

            var back = Native.GetEntityCoords(replacement, false);
            var backHeading = Native.GetEntityHeading(replacement);
            var backBottom = Bottom(replacement, back);
            var swapAt = Native.GetGameTimer() + TransformMs;

            while (Native.GetGameTimer() < swapAt && Native.DoesEntityExist(replacement))
            {
                back = Native.GetEntityCoords(replacement, false);
                backHeading = Native.GetEntityHeading(replacement);
                backBottom = Bottom(replacement, back);

                await API.Delay(WatchMs);
            }

            if (!Native.DoesEntityExist(original))
            {
                return;
            }

            await ControlAsync(original);

            int? seat = DriverSeat;
            Momentum? backMomentum = null;

            if (Native.DoesEntityExist(replacement))
            {
                await ControlAsync(replacement);

                back = Native.GetEntityCoords(replacement, false);
                backHeading = Native.GetEntityHeading(replacement);
                backBottom = Bottom(replacement, back);
                backMomentum = Momentum.Of(replacement);
                seat = SeatOf(replacement, Native.PlayerPedId());

                Conceal(replacement);
            }

            Native.SetEntityCoordsNoOffset(original, back.X, back.Y, backBottom - MinZ(Native.GetEntityModel(original)), false, false, false);
            Native.SetEntityHeading(original, backHeading);

            Reveal(original);

            await SwapAsync(replacementId, originalId, original, backMomentum, seat);
        }
        finally
        {
            if (replacement != 0)
            {
                Conceal(replacement);

                await NpcSupport.DeleteAsync(replacement);
            }

            if (_marked && Native.DoesEntityExist(original))
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
        if (_original == 0 || !_marked || !Native.DoesEntityExist(_original))
        {
            return;
        }

        var ped = Native.PlayerPedId();
        int? seat = DriverSeat;

        if (_replacement != 0 && Native.DoesEntityExist(_replacement))
        {
            var at = Native.GetEntityCoords(_replacement, false);

            seat = SeatOf(_replacement, ped);

            Conceal(_replacement);

            Native.SetEntityCoordsNoOffset(_original, at.X, at.Y, at.Z, false, false, false);
            Native.SetEntityHeading(_original, Native.GetEntityHeading(_replacement));
        }

        Reveal(_original);

        if (seat is { } inside)
        {
            Native.SetPedIntoVehicle(ped, _original, SeatFor(_original, inside));
            Native.SetEntityVisible(ped, true, false);
        }

        Release(_original);
    }

    private static void Release(int vehicle)
    {
        if (_originalWasMission)
        {
            return;
        }

        Native.SetVehicleAsNoLongerNeeded(ref vehicle);
    }

    private static async Task TransformFxAsync(int vehicle, bool flash)
    {
        var bank = await Streaming.AudioBankAsync(TransformBank);

        if (bank)
        {
            Native.PlaySoundFromEntity(-1, "Vehicle_Transform", vehicle, TransformSoundSet, true, 250);
        }

        if (flash)
        {
            Native.AnimpostfxPlay(TransformFlash, TransformFlashMs, false);
        }

        var smoke = 0;
        var asset = !ClientConfig.Value(BullySettings.DisableParticleEffects) && await Streaming.PtfxAsync(TransformAsset);

        if (asset)
        {
            Native.GetVehicleCustomPrimaryColour(vehicle, out var red, out var green, out var blue);

            if (red == green && green == blue && red <= TooDark)
            {
                red = green = blue = DarkGrey;
            }

            Native.UseParticleFxAsset(TransformAsset);

            smoke = Native.StartNetworkedParticleFxLoopedOnEntity(
                "scr_as_trans_smoke",
                vehicle,
                0f, 0f, 0f,
                0f, 0f, 0f,
                1f, false, false, false,
                red / 255f, green / 255f, blue / 255f,
                true);

            if (smoke != 0)
            {
                var bike = Native.IsThisModelABike(Native.GetEntityModel(vehicle));

                Native.SetParticleFxLoopedEvolution(smoke, "width", bike ? 0f : 1f, false);
                Native.SetParticleFxLoopedEvolution(smoke, "length", bike ? 0f : 1f, false);
            }
        }

        await API.Delay(TransformFlashMs);

        if (smoke != 0)
        {
            Native.StopParticleFxLooped(smoke, false);
        }

        if (flash)
        {
            Native.AnimpostfxStop(TransformFlash);
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
        if (!Native.DoesEntityExist(vehicle))
        {
            return;
        }

        Native.SetEntityVisible(vehicle, false, false);
        Native.SetEntityCollision(vehicle, false, false);
        Native.FreezeEntityPosition(vehicle, true);
    }

    private static void Reveal(int vehicle)
    {
        Native.SetEntityCollision(vehicle, true, true);
        Native.SetEntityVisible(vehicle, true, false);
        Native.FreezeEntityPosition(vehicle, false);
    }

    private static float Bottom(int vehicle, Vector3 centre) => centre.Z + MinZ(Native.GetEntityModel(vehicle));

    private static float MinZ(uint model)
    {
        Native.GetModelDimensions(model, out var min, out _);

        return min.Z;
    }

    private static int? SeatOf(int vehicle, int ped)
    {
        for (var seat = DriverSeat; seat < Native.GetVehicleMaxNumberOfPassengers(vehicle); seat++)
        {
            if (Native.GetPedInVehicleSeat(vehicle, seat, false) == ped)
            {
                return seat;
            }
        }

        return null;
    }

    private static int SeatFor(int vehicle, int seat) =>
        seat == DriverSeat
        || (seat < Native.GetVehicleMaxNumberOfPassengers(vehicle) && Native.IsVehicleSeatFree(vehicle, seat, false))
            ? seat
            : AnyPassengerSeat;

    private static async Task SwapAsync(int fromId, int toId, int to, Momentum? momentum, int? seat)
    {
        API.EmitServer(
            BullyEvents.TransformSwap,
            fromId.ToString(CultureInfo.InvariantCulture),
            toId.ToString(CultureInfo.InvariantCulture));

        if (momentum is { } moving)
        {
            if (seat == DriverSeat)
            {
                Native.SetVehicleEngineOn(to, true, true, false);
            }

            Native.SetVehicleForwardSpeed(to, moving.ForwardSpeed);
            Native.SetEntityVelocity(to, moving.Velocity.X, moving.Velocity.Y, moving.Velocity.Z);

            if (seat == DriverSeat && moving.Rpm > MinimumRpm)
            {
                Native.SetVehicleCurrentRpm(to, moving.Rpm);
            }
        }

        if (seat is { } inside)
        {
            Native.SetPedIntoVehicle(Native.PlayerPedId(), to, SeatFor(to, inside));
            Native.SetEntityVisible(Native.PlayerPedId(), true, false);
        }

        BullyTask.Run(() => TransformFxAsync(to, seat is not null), "Transform.Effects");

        await API.Delay(SeatSettleMs);
    }

    private static uint? PickModel()
    {
        var models = NativeFixer.GetAllVehicleModels()
            .Select(Streaming.Hash)
            .Where(model => Native.IsModelInCdimage(model)
                && Native.IsThisModelACar(model)
                && TransformClasses.Contains(Native.GetVehicleClassFromName(model)))
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
                Native.SetPedIntoVehicle(Native.PlayerPedId(), vehicle, SeatFor(vehicle, index));
                Native.SetEntityVisible(Native.PlayerPedId(), true, false);
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
        Native.GetEntityVelocity(vehicle),
        Native.GetEntitySpeedVector(vehicle, true).Y,
        Native.GetVehicleCurrentRpm(vehicle));
}
