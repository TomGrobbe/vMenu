using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;

namespace vMenu.Enhanced.Menus.Misc;

/// <summary>
/// Turns the vehicle somebody is driving into the blip sprite that should be drawn for them.
/// </summary>
/// <remarks>
/// The model specific half of the answer is shared, in <see cref="VehicleBlipSprites" />. The rest
/// needs the game to say whether a model is a plane or a boat, which only a client can ask, so it
/// is here. Both halves run for a player standing next to you and for one on the far side of the
/// map, from the same model hash, so the two can never disagree about what somebody is driving.
/// </remarks>
public static class BlipSprites
{
    /// <summary>The sprite for whatever this player is in, or the plain dot for anything ordinary.</summary>
    public static int ForVehicleModel(uint model)
    {
        if (model == 0)
        {
            return VehicleBlipSprites.StandardSprite;
        }

        if (VehicleBlipSprites.ForModel(model) is { } known)
        {
            return known;
        }

        // Rockstar's order, which is not the obvious one: a helicopter is also a plane as far as
        // some of these answer, so the narrower question has to be asked first.
        if (Native.IsThisModelAPlane(model))
        {
            return VehicleBlipSprites.PlaneSprite;
        }

        if (Native.IsThisModelAHeli(model))
        {
            return VehicleBlipSprites.HelicopterSprite;
        }

        if (Native.IsThisModelABoat(model))
        {
            return VehicleBlipSprites.BoatSprite;
        }

        // Everything else, cars and motorbikes included, stays a plain dot. That is deliberate and
        // it is what GTA Online does: your blip only changes shape for a vehicle Rockstar drew a
        // symbol for.
        return VehicleBlipSprites.StandardSprite;
    }
}
