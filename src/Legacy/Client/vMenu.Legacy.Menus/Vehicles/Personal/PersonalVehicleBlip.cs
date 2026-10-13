using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Misc;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles.Personal;

public static class PersonalVehicleBlip
{
    private const int Colour = 32;

    private const int VehicleCategory = 1;

    private const int DisplayBoth = 2;

    private const int FullAlpha = 255;

    private static int _handle;

    private static int _entity;

    private static int _sprite;

    private static int _heading;

    public static void Reevaluate()
    {
        if (!PersonalVehicle.IsMarked || !PersonalVehicle.BlipWanted)
        {
            RemoveAll();

            return;
        }

        Apply(_heading);
    }

    public static void Apply(int heading)
    {
        _heading = heading;

        if (!PersonalVehicle.IsMarked
            || !PersonalVehicle.BlipWanted
            || !PersonalVehicle.InRange
            || LocalPlayerIsInside())
        {
            RemoveAll();

            return;
        }

        var entity = Streamed();

        if (entity == 0 && !PersonalVehicle.HasPosition)
        {
            RemoveAll();

            return;
        }

        var sprite = SpriteFor(PersonalVehicle.Model);

        if (!Ensure(entity, sprite))
        {
            return;
        }

        if (entity == 0)
        {
            var position = PersonalVehicle.Position;

            API.SetBlipCoords(_handle, position.X, position.Y, position.Z);
            API.SetBlipRotation(_handle, BlipRotation.Wanted(PersonalVehicle.Model, sprite) ? heading : 0);
        }

        API.SetBlipAlpha(_handle, FullAlpha);
        API.SetBlipAsShortRange(_handle, false);
    }

    public static void RemoveAll()
    {
        if (_handle != 0 && API.DoesBlipExist(_handle))
        {
            NativeFixer.RemoveBlip(_handle);
        }

        _handle = 0;
        _entity = 0;
        _sprite = 0;
    }

    private static bool LocalPlayerIsInside() =>
        PersonalVehicle.Owns(API.GetVehiclePedIsIn(API.PlayerPedId(), false));

    private static int Streamed()
    {
        if (!API.NetworkDoesNetworkIdExist(PersonalVehicle.NetworkId))
        {
            return 0;
        }

        var entity = API.NetworkGetEntityFromNetworkId(PersonalVehicle.NetworkId);

        return entity != 0 && API.DoesEntityExist(entity) ? entity : 0;
    }

    private static bool Ensure(int entity, int sprite)
    {
        if (_handle != 0 && API.DoesBlipExist(_handle) && _entity == entity)
        {
            if (_sprite != sprite)
            {
                API.SetBlipSprite(_handle, sprite);

                _sprite = sprite;

                Decorate();
            }

            return true;
        }

        RemoveAll();

        var position = PersonalVehicle.Position;

        var handle = entity != 0
            ? API.AddBlipForEntity(entity)
            : API.AddBlipForCoord(position.X, position.Y, position.Z);

        if (handle == 0 || !API.DoesBlipExist(handle))
        {
            return false;
        }

        _handle = handle;
        _entity = entity;
        _sprite = sprite;

        API.SetBlipSprite(handle, sprite);

        Decorate();

        return true;
    }

    private static void Decorate()
    {
        API.SetBlipColour(_handle, Colour);
        API.SetBlipCategory(_handle, VehicleCategory);
        API.SetBlipDisplay(_handle, DisplayBoth);
        API.SetBlipHighDetail(_handle, true);
        API.SetBlipAsShortRange(_handle, false);

        API.BeginTextCommandSetBlipName("STRING");
        API.AddTextComponentSubstringPlayerName(Localizer.Current.Get(Loc.PersonalVehicle.BlipName));
        API.EndTextCommandSetBlipName(_handle);
    }

    private static int SpriteFor(uint model)
    {
        if (model == 0)
        {
            return VehicleBlipSprites.PersonalVehicleCarSprite;
        }

        if (VehicleBlipSprites.ForModel(model) is { } known)
        {
            return known;
        }

        if (API.IsThisModelAPlane(model))
        {
            return VehicleBlipSprites.PlaneSprite;
        }

        if (API.IsThisModelAHeli(model))
        {
            return VehicleBlipSprites.HelicopterSprite;
        }

        if (API.IsThisModelABoat(model))
        {
            return VehicleBlipSprites.BoatSprite;
        }

        return API.IsThisModelABike(model)
            ? VehicleBlipSprites.PersonalVehicleBikeSprite
            : VehicleBlipSprites.PersonalVehicleCarSprite;
    }
}
