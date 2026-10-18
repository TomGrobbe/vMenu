using CitizenFX.FiveM.Client;

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

    public bool IsPlane { get; } = Native.IsThisModelAPlane(model);

    public int BombType { get; private set; } = -1;

    public Countermeasure Countermeasure { get; private set; }

    public bool Has(AircraftWeaponRules rule) => (Profile.Rules & rule) == rule;

    public bool Airborne { get; private set; }

    public int NetworkId => _networkId != 0 ? _networkId : _networkId = Native.NetworkGetNetworkIdFromEntity(Vehicle);

    public void Sample() => Airborne = IsAirborne();

    public void ReadMods()
    {
        var bombs = Native.GetVehicleMod(Vehicle, BombSlot);

        BombType = Profile.Bombs && bombs > -1 ? Math.Min(bombs, 3) : -1;

        var slot = Has(AircraftWeaponRules.Oppressor) ? OppressorCountermeasureSlot : CountermeasureSlot;
        var fitted = Native.GetVehicleMod(Vehicle, slot);

        Countermeasure = fitted switch
        {
            1 when Profile.Flares => Countermeasure.Flares,
            0 when Profile.Chaff => Countermeasure.Chaff,
            < 0 when Profile.Smoke && Native.IsToggleModOn(Vehicle, SmokeToggle) => Countermeasure.Smoke,
            _ => Countermeasure.None,
        };
    }

    private bool IsAirborne()
    {
        var lowFlying = Has(AircraftWeaponRules.LowFlying);

        return (lowFlying || Native.GetEntityHeightAboveGround(Vehicle) >= MinimumHeight)
            && (Native.IsEntityInAir(Vehicle) || (lowFlying && !Native.IsVehicleOnAllWheels(Vehicle)));
    }

    public bool CanBomb() =>
        Airborne && Native.IsEntityUpright(Vehicle, UprightLimit) && !Native.IsEntityUpsidedown(Vehicle);

    public bool HasLanded() =>
        (Native.GetEntityHeightAboveGround(Vehicle) < MinimumHeight || !Native.IsEntityInAir(Vehicle))
        && (!Has(AircraftWeaponRules.LowFlying) || Native.IsVehicleOnAllWheels(Vehicle));
}
