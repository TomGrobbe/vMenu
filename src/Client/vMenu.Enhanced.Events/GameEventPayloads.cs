namespace vMenu.Enhanced.Events;

#region Player ped events
public readonly record struct PlayerPedIdChanged(int NewPed, int PreviousPed);

public readonly record struct PlayerPedModelChanged(int NewPed, uint NewModel, uint PreviousModel);

public readonly record struct PlayerPedDamaged(int Ped, int NewHealth, int HealthLost, int NewArmour, int ArmourLost);

public readonly record struct PlayerPedDied(int Ped, int Killer, uint Weapon);

public readonly record struct PlayerPedRevived(int Ped, int Health, bool Respawned);
#endregion

#region Vehicle events
public readonly record struct VehicleEntered(int Vehicle, int Seat);

public readonly record struct VehicleExited(int Vehicle, int Seat);

/// <summary>Straight from one vehicle into another, (at most 100ms in between old/new vehicle, due to tick handler running once every 100ms).</summary>
public readonly record struct VehicleSwapped(int Vehicle, int Previous, int Seat);

public readonly record struct VehicleSeatChanged(int Vehicle, int Seat, int PreviousSeat);

/// <summary>
/// Combined event handler for <see cref="VehicleEntered" />, <see cref="VehicleExited" />, <see cref="VehicleSwapped" /> and <see cref="VehicleSeatChanged" />.
/// Any value can be null, depending on what exactly happened (a new or old vehicle may not exist, same with seat index).
/// </summary>
// Printed by hand rather than by the record's own generated ToString. That one reaches a member's
// ToString through a call the FiveM sandbox resolves at runtime, and resolving it against a null
// throws, so a payload with a missing vehicle or seat would take down anything that logged it.
public readonly record struct VehicleChanged(int? Vehicle, int? PreviousVehicle, int? Seat, int? PreviousSeat)
{
    public override string ToString() =>
        $"{nameof(VehicleChanged)} {{ {nameof(Vehicle)} = {Text(Vehicle)}"
        + $", {nameof(PreviousVehicle)} = {Text(PreviousVehicle)}"
        + $", {nameof(Seat)} = {Text(Seat)}"
        + $", {nameof(PreviousSeat)} = {Text(PreviousSeat)} }}";

    private static string Text(int? value) => value.HasValue ? value.Value.ToString() : "null";
}

public readonly record struct VehicleDamaged(
    int Vehicle,
    float Body,
    float Engine,
    float PetrolTank,
    float BodyLost,
    float EngineLost,
    float PetrolTankLost);

#endregion