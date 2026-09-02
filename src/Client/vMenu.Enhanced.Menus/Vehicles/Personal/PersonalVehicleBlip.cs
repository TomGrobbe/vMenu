using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Misc;

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

            Native.SetBlipCoords(_handle, position.X, position.Y, position.Z);
            Native.SetBlipRotation(_handle, BlipRotation.WantedForModel(PersonalVehicle.Model) ? heading : 0);
        }

        Native.SetBlipAlpha(_handle, FullAlpha);
        Native.SetBlipAsShortRange(_handle, false);
    }

    public static void RemoveAll()
    {
        if (_handle != 0 && Native.DoesBlipExist(_handle))
        {
            Native.RemoveBlip(_handle);
        }

        _handle = 0;
        _entity = 0;
        _sprite = 0;
    }

    private static bool LocalPlayerIsInside() =>
        PersonalVehicle.Owns(Native.GetVehiclePedIsIn(Native.PlayerPedId(), false));

    private static int Streamed()
    {
        if (!Native.NetworkDoesNetworkIdExist(PersonalVehicle.NetworkId))
        {
            return 0;
        }

        var entity = Native.NetworkGetEntityFromNetworkId(PersonalVehicle.NetworkId);

        return entity != 0 && Native.DoesEntityExist(entity) ? entity : 0;
    }

    private static bool Ensure(int entity, int sprite)
    {
        if (_handle != 0 && Native.DoesBlipExist(_handle) && _entity == entity)
        {
            if (_sprite != sprite)
            {
                Native.SetBlipSprite(_handle, sprite);

                _sprite = sprite;

                Decorate();
            }

            return true;
        }

        RemoveAll();

        var position = PersonalVehicle.Position;

        var handle = entity != 0
            ? Native.AddBlipForEntity(entity)
            : Native.AddBlipForCoord(position.X, position.Y, position.Z);

        if (handle == 0 || !Native.DoesBlipExist(handle))
        {
            return false;
        }

        _handle = handle;
        _entity = entity;
        _sprite = sprite;

        Native.SetBlipSprite(handle, sprite);

        Decorate();

        return true;
    }

    private static void Decorate()
    {
        Native.SetBlipColour(_handle, Colour);
        Native.SetBlipCategory(_handle, VehicleCategory);
        Native.SetBlipDisplay(_handle, DisplayBoth);
        Native.SetBlipHighDetail(_handle, true);
        Native.SetBlipAsShortRange(_handle, false);

        Native.BeginTextCommandSetBlipName("STRING");
        Native.AddTextComponentSubstringPlayerName(Localizer.Current.Get(Loc.PersonalVehicle.BlipName));
        Native.EndTextCommandSetBlipName(_handle);
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

        return Native.IsThisModelABike(model)
            ? VehicleBlipSprites.PersonalVehicleBikeSprite
            : VehicleBlipSprites.PersonalVehicleCarSprite;
    }
}
