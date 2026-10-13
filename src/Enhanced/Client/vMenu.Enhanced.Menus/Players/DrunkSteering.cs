using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Bully;

namespace vMenu.Enhanced.Menus.Players;

internal sealed class DrunkSteering
{
    private const int RerollMs = 1000;

    private const float DrunkRange = 0.05f;

    private const float DrunkOffset = 0.02f;

    private const float DruggedRange = 0.06f;

    private const float DruggedOffset = 0.025f;

    private const float CarScale = 0.35f;

    private const float BikeScale = 1.5f;

    private const float StoppedSpeed = 0.04f;

    private int _vehicle;

    private float _bias;

    private int _rerollAt;

    // Call every frame, the game forgets the bias otherwise. False while there is nothing to steer.
    public bool Update(bool drugged)
    {
        var driven = VehiclePranks.DrivenVehicle();

        if (driven != _vehicle)
        {
            Release();

            _vehicle = driven;
        }

        if (_vehicle == 0 || !Native.NetworkHasControlOfEntity(_vehicle))
        {
            return false;
        }

        var now = Native.GetGameTimer();

        if (now >= _rerollAt)
        {
            _rerollAt = now + RerollMs;
            _bias = Native.GetEntitySpeed(_vehicle) < StoppedSpeed ? 0f : Roll(_vehicle, drugged);
        }

        Native.SetVehicleSteerBias(_vehicle, _bias);

        return true;
    }

    public void Release()
    {
        if (_vehicle != 0 && Native.DoesEntityExist(_vehicle))
        {
            Native.SetVehicleSteerBias(_vehicle, 0f);
        }

        _vehicle = 0;
        _bias = 0f;
        _rerollAt = 0;
    }

    private static float Roll(int vehicle, bool drugged)
    {
        var range = drugged ? DruggedRange : DrunkRange;
        var bias = Dice.Float(-range, range);
        var offset = Dice.Float(0f, drugged ? DruggedOffset : DrunkOffset);
        var scale = Native.IsThisModelABike(Native.GetEntityModel(vehicle)) ? BikeScale : CarScale;

        bias = bias < 0f ? bias - offset : bias + offset;

        return Math.Clamp(bias * scale, -1f, 1f);
    }
}
