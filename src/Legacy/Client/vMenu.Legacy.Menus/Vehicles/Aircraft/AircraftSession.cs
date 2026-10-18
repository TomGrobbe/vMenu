using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

internal enum Countermeasure
{
    None,

    Flares,

    Chaff,

    Smoke,
}

internal sealed class AircraftSession(int vehicle, uint model, int seat, AircraftWeaponProfile profile)
{
    private const int BombSlot = 9;

    private const int CountermeasureSlot = 1;

    private const int OppressorCountermeasureSlot = 6;

    private const int SmokeToggle = 20;

    private const float MinimumHeight = 1.5f;

    private const float UprightLimit = 50f;

    private int _networkId;

    public int Vehicle { get; } = vehicle;

    public uint Model { get; } = model;

    public int Seat { get; } = seat;

    public AircraftWeaponProfile Profile { get; } = profile;

    public bool IsPilot => Seat == -1;

    public bool IsPlane { get; } = API.IsThisModelAPlane(model);

    public int BombType { get; private set; } = -1;

    public Countermeasure Countermeasure { get; private set; }

    public bool Has(AircraftWeaponRules rule) => (Profile.Rules & rule) == rule;

    public bool Airborne { get; private set; }

    public int NetworkId => _networkId != 0 ? _networkId : _networkId = API.NetworkGetNetworkIdFromEntity(Vehicle);

    public void Sample() => Airborne = IsAirborne();

    public void ReadMods()
    {
        var bombs = API.GetVehicleMod(Vehicle, BombSlot);

        BombType = Profile.Bombs && bombs > -1 ? Math.Min(bombs, 3) : -1;

        var slot = Has(AircraftWeaponRules.Oppressor) ? OppressorCountermeasureSlot : CountermeasureSlot;
        var fitted = API.GetVehicleMod(Vehicle, slot);

        Countermeasure = fitted switch
        {
            1 when Profile.Flares => Countermeasure.Flares,
            0 when Profile.Chaff => Countermeasure.Chaff,
            < 0 when Profile.Smoke && API.IsToggleModOn(Vehicle, SmokeToggle) => Countermeasure.Smoke,
            _ => Countermeasure.None,
        };
    }

    private bool IsAirborne()
    {
        var lowFlying = Has(AircraftWeaponRules.LowFlying);

        return (lowFlying || API.GetEntityHeightAboveGround(Vehicle) >= MinimumHeight)
            && (API.IsEntityInAir(Vehicle) || (lowFlying && !API.IsVehicleOnAllWheels(Vehicle)));
    }

    public bool CanBomb() =>
        Airborne && API.IsEntityUpright(Vehicle, UprightLimit) && !API.IsEntityUpsidedown(Vehicle);

    public bool HasLanded() =>
        (API.GetEntityHeightAboveGround(Vehicle) < MinimumHeight || !API.IsEntityInAir(Vehicle))
        && (!Has(AircraftWeaponRules.LowFlying) || API.IsVehicleOnAllWheels(Vehicle));
}
