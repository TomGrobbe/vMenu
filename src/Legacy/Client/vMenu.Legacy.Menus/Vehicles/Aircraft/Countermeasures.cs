using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Bully;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

internal static class Countermeasures
{
    private const string Sounds = "DLC_SM_Countermeasures_Sounds";

    private const string ChaffAsset = "scr_sm_counter";

    private const string SmokeAsset = "scr_ar_planes";

    private const int FlareCooldownMs = 1000;

    private const int ChaffCooldownMs = 2000;

    private const int ChaffDurationMs = 6000;

    private const int OuterFlareDelayMs = 250;

    private const int EmptySoundSpacingMs = 500;

    private const float FlareTargetZ = -0.2f;

    private static readonly uint FlareWeapon = Hashing.Joaat("weapon_flaregun");

    private static readonly uint FlareModel = Hashing.Joaat("w_pi_flaregun_shell");

    private static readonly uint Titan = Hashing.Joaat("titan2");

    private static readonly Vector3[] ChaffSpread =
    [
        new(-90f, -10f, 0f),
        new(-90f, 10f, 0f),
        new(-180f, 45f, 0f),
        new(-180f, -45f, 0f),
    ];

    private static bool _armed = true;

    private static int _cooldownStart;

    private static bool _pending;

    private static bool _fired;

    private static int _emptySoundAt;

    private static int _sequenceStart;

    private static bool _innerLeft;

    private static bool _innerRight;

    private static bool _outerLeft;

    private static bool _outerRight;

    private static int _lockOnBlockedAt;

    private static int _lockOnVehicle;

    private static int _smoke;

    private static bool _helpShown;

    private static bool _chaffFireHelpShown;

    private static bool _chaffDelayHelpShown;

    private static bool _flareAssetsRequested;

    private static bool _chaffAssetRequested;

    private static bool _smokeAssetRequested;

    // The cooldown counts too, or the other seat could fire straight after.
    public static bool Busy => _pending || _lockOnBlockedAt != 0 || _smoke != 0;

    // The bomb bay shares the key, and the release after holding it open must not fire anything.
    public static void Disarm() => _armed = false;

    public static void Frame(AircraftSession session, int now)
    {
        if (AircraftControls.Muted)
        {
            _armed = false;
        }

        LockOnWindow(now);

        var kind = session.Countermeasure;

        if (kind == Countermeasure.None || !session.Airborne)
        {
            _cooldownStart = 0;
            _pending = false;
            _fired = false;

            ClearSequence();

            StopSmoke();
            Rearm();

            return;
        }

        API.DisableControlAction(AircraftControls.Group, AircraftControls.Headlight, true);

        ShowHelp(session, kind);

        switch (kind)
        {
            case Countermeasure.Flares:
                Cooldown(now, FlareCooldownMs);
                Flares(session, now);
                break;
            case Countermeasure.Chaff:
                Cooldown(now, ChaffCooldownMs);
                Chaff(session, now);
                break;
            case Countermeasure.Smoke:
                Smoke(session);
                break;
        }

        Rearm();
    }

    public static void Reset()
    {
        StopSmoke();

        if (_lockOnBlockedAt != 0 && _lockOnVehicle != 0 && API.DoesEntityExist(_lockOnVehicle))
        {
            AircraftWeaponOwner.Request(_lockOnVehicle, AircraftWeaponEvents.AllowLockOn);
        }

        _armed = true;
        _cooldownStart = 0;
        _pending = false;
        _fired = false;
        _lockOnBlockedAt = 0;
        _lockOnVehicle = 0;
        _helpShown = false;
        _chaffFireHelpShown = false;
        _chaffDelayHelpShown = false;

        ClearSequence();
        ReleaseAssets();
    }

    private static bool Triggered() =>
        _armed && API.IsControlJustReleased(AircraftControls.Group, AircraftControls.Countermeasure);

    private static void Rearm()
    {
        if (API.IsControlReleased(AircraftControls.Group, AircraftControls.Countermeasure))
        {
            _armed = true;
        }
    }

    private static void Cooldown(int now, int durationMs)
    {
        if (_cooldownStart != 0 && now - _cooldownStart >= durationMs)
        {
            _cooldownStart = 0;
            _pending = false;
            _fired = false;
        }
    }

    private static void ShowHelp(AircraftSession session, Countermeasure kind)
    {
        if (_helpShown || !AircraftHelp.CanShow)
        {
            return;
        }

        _helpShown = true;

        // Rockstar never shows countermeasure help in the Titan 250 D.
        if (session.Model == Titan)
        {
            return;
        }

        switch (kind)
        {
            case Countermeasure.Flares:
                AircraftHelp.Show("FLARE_HELP");
                break;
            case Countermeasure.Chaff:
                AircraftHelp.Show("CHAFF_HELP");
                break;
            case Countermeasure.Smoke:
                AircraftHelp.Show("SMOKE_HELP");
                break;
        }
    }

    private static void Flares(AircraftSession session, int now)
    {
        if (!_flareAssetsRequested)
        {
            API.RequestModel(FlareModel);
            API.RequestWeaponAsset(FlareWeapon, 31, 0);

            _flareAssetsRequested = true;
        }

        if (!_pending)
        {
            if (API.HasModelLoaded(FlareModel)
                && API.HasWeaponAssetLoaded(FlareWeapon)
                && Triggered()
                && AircraftWeaponClaim.TryTake(session))
            {
                ClearSequence();

                _pending = true;
            }

            return;
        }

        if (!_fired)
        {
            FlareSequence(session, now);

            return;
        }

        if (Triggered() && now - _emptySoundAt >= EmptySoundSpacingMs)
        {
            _emptySoundAt = now;

            API.PlaySoundFrontend(-1, "flares_empty", Sounds, true);
        }
    }

    // The inner pair leaves first and the outer pair a moment later, which is what spreads them out.
    private static void FlareSequence(AircraftSession session, int now)
    {
        var vehicle = session.Vehicle;
        var profile = session.Profile;
        var oppressor = session.Has(AircraftWeaponRules.Oppressor);

        var tilt = API.GetEntityRotation(vehicle, 0);
        tilt.Z = 0f;

        var origin = API.GetEntityCoords(vehicle, true);
        var heading = API.GetEntityHeading(vehicle);

        Vector3 At(float x, float y, float z)
        {
            var local = Rotate(new Vector3(x, y, z), tilt);

            return API.GetOffsetFromCoordAndHeadingInWorldCoords(origin.X, origin.Y, origin.Z, heading, local.X, local.Y, local.Z);
        }

        void Fire(int flare, float targetX) => Shoot(
            session,
            At(profile.FlareStartX[flare], profile.FlareStartY, profile.FlareStartZ),
            At(targetX, profile.FlareTargetY, FlareTargetZ));

        if (!_outerLeft && _innerLeft && now - _sequenceStart >= OuterFlareDelayMs)
        {
            if (!oppressor)
            {
                Fire(0, -6f);
            }

            _outerLeft = true;
        }

        if (!_innerLeft)
        {
            Fire(1, oppressor ? -1f : -3f);

            _sequenceStart = now;
            _innerLeft = true;

            API.PlaySoundFromEntity(-1, "flares_released", vehicle, Sounds, true, 0);
        }

        if (!_innerRight)
        {
            Fire(2, oppressor ? 1f : 3f);

            _innerRight = true;
        }

        if (!_outerRight && _innerRight && now - _sequenceStart >= OuterFlareDelayMs)
        {
            if (!oppressor)
            {
                Fire(3, 6f);
            }

            _outerRight = true;
        }

        if (_outerRight)
        {
            _fired = true;
            _cooldownStart = now;

            ClearSequence();
        }
    }

    private static void Shoot(AircraftSession session, Vector3 from, Vector3 to) =>
        NativeFixer.ShootSingleBulletBetweenCoordsIgnoreEntityNew(
            from.X, from.Y, from.Z, to.X, to.Y, to.Z,
            0, true, FlareWeapon, API.PlayerPedId(), true, true, -1f, session.Vehicle,
            false, false, 0, true, true, false, false);

    private static Vector3 Rotate(Vector3 point, Vector3 rotation)
    {
        var sin = Sin(rotation.X);
        var cos = Cos(rotation.X);
        point = new Vector3(point.X, (cos * point.Y) - (sin * point.Z), (sin * point.Y) + (cos * point.Z));

        sin = Sin(rotation.Y);
        cos = Cos(rotation.Y);
        point = new Vector3((cos * point.X) + (sin * point.Z), point.Y, (cos * point.Z) - (sin * point.X));

        sin = Sin(rotation.Z);
        cos = Cos(rotation.Z);

        return new Vector3((cos * point.X) - (sin * point.Y), (sin * point.X) + (cos * point.Y), point.Z);
    }

    private static float Sin(float degrees) => MathF.Sin(degrees * (MathF.PI / 180f));

    private static float Cos(float degrees) => MathF.Cos(degrees * (MathF.PI / 180f));

    private static void Chaff(AircraftSession session, int now)
    {
        if (!_chaffAssetRequested)
        {
            Streaming.RequestPtfx(ChaffAsset);

            _chaffAssetRequested = true;
        }

        if (_pending && !_fired)
        {
            FireChaff(session, now);

            return;
        }

        if (!API.HasNamedPtfxAssetLoaded(ChaffAsset) || !Triggered())
        {
            return;
        }

        if (!_pending)
        {
            if (AircraftWeaponClaim.TryTake(session))
            {
                _pending = true;
            }

            return;
        }

        if (now - _emptySoundAt < EmptySoundSpacingMs)
        {
            return;
        }

        _emptySoundAt = now;

        API.PlaySoundFrontend(-1, "chaff_cooldown", Sounds, true);

        if (!_chaffDelayHelpShown && AircraftHelp.CanShow)
        {
            AircraftHelp.Show("CHAFF_DELAY");

            _chaffDelayHelpShown = true;
        }
    }

    private static void FireChaff(AircraftSession session, int now)
    {
        var vehicle = session.Vehicle;
        var profile = session.Profile;

        var rotation = API.GetEntityRotation(vehicle, 2);

        NativeFixer.GetModelDimensions(session.Model, out var min, out var max);

        var frontLeft = API.GetOffsetFromEntityInWorldCoords(vehicle, min.X, max.Y, min.Z);
        var frontRight = API.GetOffsetFromEntityInWorldCoords(vehicle, max.X, max.Y, min.Z);
        var rearLeft = API.GetOffsetFromEntityInWorldCoords(vehicle, min.X, min.Y, min.Z);
        var rearRight = API.GetOffsetFromEntityInWorldCoords(vehicle, max.X, min.Y, min.Z);

        for (var burst = 0; burst < ChaffSpread.Length; burst++)
        {
            var front = Vector3.Lerp(frontLeft, frontRight, profile.ChaffSide[burst]);
            var rear = Vector3.Lerp(rearLeft, rearRight, profile.ChaffSide[burst]);
            var position = Vector3.Lerp(front, rear, profile.ChaffLength[burst]);

            position.Z += profile.ChaffHeight[burst];

            var angle = new Vector3(rotation.X, 0f, rotation.Z);

            if (profile.ChaffSpread)
            {
                angle += ChaffSpread[burst];
            }

            API.UseParticleFxAsset(ChaffAsset);
            NativeFixer.StartNetworkedParticleFxNonLoopedAtCoord(
                "scr_sm_counter_chaff",
                position.X, position.Y, position.Z,
                angle.X, angle.Y, angle.Z,
                profile.ChaffScale, false, false, false, false);
        }

        API.PlaySoundFromEntity(-1, "chaff_released", vehicle, Sounds, true, 0);

        AircraftWeaponOwner.Request(vehicle, AircraftWeaponEvents.BlockLockOn);

        _lockOnBlockedAt = now;
        _lockOnVehicle = vehicle;
        _fired = true;
        _cooldownStart = now;

        if (!_chaffFireHelpShown && AircraftHelp.CanShow)
        {
            AircraftHelp.Show("CHAFF_FIRE");

            _chaffFireHelpShown = true;
        }
    }

    private static void LockOnWindow(int now)
    {
        if (_lockOnBlockedAt == 0 || now - _lockOnBlockedAt < ChaffDurationMs)
        {
            return;
        }

        AircraftWeaponOwner.Request(_lockOnVehicle, AircraftWeaponEvents.AllowLockOn);

        _lockOnBlockedAt = 0;
        _lockOnVehicle = 0;
    }

    private static void Smoke(AircraftSession session)
    {
        if (!_smokeAssetRequested)
        {
            Streaming.RequestPtfx(SmokeAsset);

            _smokeAssetRequested = true;
        }

        if (_smoke != 0 && !API.DoesParticleFxLoopedExist(_smoke))
        {
            _smoke = 0;
        }

        if (!Triggered())
        {
            return;
        }

        if (_smoke != 0)
        {
            StopSmoke();

            return;
        }

        if (API.HasNamedPtfxAssetLoaded(SmokeAsset) && AircraftWeaponClaim.TryTake(session))
        {
            StartSmoke(session);
        }
    }

    private static void StartSmoke(AircraftSession session)
    {
        var vehicle = session.Vehicle;
        var offset = session.Profile.SmokeOffset;

        NativeFixer.GetVehicleTyreSmokeColor(vehicle, out var red, out var green, out var blue);

        var colour = SmokeColour(red, green, blue);

        API.UseParticleFxAsset(SmokeAsset);

        _smoke = NativeFixer.StartNetworkedParticleFxLoopedOnEntityBone(
            "scr_ar_trail_smoke", vehicle,
            offset[0], offset[1], offset[2], 0f, 0f, 0f, -1,
            session.Profile.SmokeScale, false, false, false,
            colour.X / 255f, colour.Y / 255f, colour.Z / 255f, true);
    }

    // I don't know why, but some values in the decompiled scripts get changed into other ones.
    // So I'm copying that logic, if we wanna scrap this later that's fine by me. - Tom
    private static Vector3 SmokeColour(int red, int green, int blue)
    {
        var colour = new Vector3(red, green, blue);

        if (colour == new Vector3(255f, 127f, 0f))
        {
            return new Vector3(255f, 165f, 0f);
        }

        if (colour == new Vector3(252f, 238f, 0f))
        {
            return new Vector3(255f, 255f, 0f);
        }

        if (colour == new Vector3(0f, 174f, 239f))
        {
            return new Vector3(0f, 0f, 255f);
        }

        if (colour == new Vector3(226f, 6f, 6f))
        {
            return new Vector3(255f, 0f, 0f);
        }

        if (colour == new Vector3(20f, 20f, 20f) || colour == Vector3.One)
        {
            return Vector3.Zero;
        }

        if (colour == new Vector3(114f, 204f, 114f) || colour == new Vector3(102f, 152f, 104f))
        {
            return new Vector3(0f, 255f, 0f);
        }

        return colour;
    }

    private static void StopSmoke()
    {
        if (_smoke == 0)
        {
            return;
        }

        if (API.DoesParticleFxLoopedExist(_smoke))
        {
            API.StopParticleFxLooped(_smoke, false);
        }

        _smoke = 0;
    }

    private static void ClearSequence()
    {
        _innerLeft = false;
        _innerRight = false;
        _outerLeft = false;
        _outerRight = false;
        _sequenceStart = 0;
    }

    private static void ReleaseAssets()
    {
        if (_flareAssetsRequested)
        {
            API.SetModelAsNoLongerNeeded(FlareModel);
            API.RemoveWeaponAsset(FlareWeapon);
        }

        if (_chaffAssetRequested)
        {
            Streaming.ReleasePtfx(ChaffAsset);
        }

        if (_smokeAssetRequested)
        {
            Streaming.ReleasePtfx(SmokeAsset);
        }

        _flareAssetsRequested = false;
        _chaffAssetRequested = false;
        _smokeAssetRequested = false;
    }
}
