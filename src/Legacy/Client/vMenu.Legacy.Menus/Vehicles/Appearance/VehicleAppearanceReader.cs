using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// Every value here is asked of the game the moment it is wanted. Nothing is remembered from when it
// was set, which is the whole point: this is what the dump command reports and what the writer checks
// its own work against, so a value that came from vMenu's memory would prove nothing.
public static class VehicleAppearanceReader
{
    // The game has no way to say how many optional part slots a vehicle has, so this walks a fixed
    // range. Twenty covers every stock vehicle with room to spare for add-ons.
    public const int ExtraCount = VehicleExtras.Count;

    private const int NeonLeft = 0;

    private const int NeonRight = 1;

    private const int NeonFront = 2;

    private const int NeonBack = 3;

    // Drift tyres arrived in this game build. Asking an older one throws.
    private const int DriftTyresBuild = 2372;

    public static VehicleAppearance Read(Vehicle vehicle) => Read(vehicle.Handle);

    public static VehicleAppearance Read(int handle)
    {
        // Without this the game answers zero upgrades for every slot, whatever is actually fitted.
        API.SetVehicleModKit(handle, 0);

        var model = NativeFixer.GetEntityModel(handle);

        var appearance = new VehicleAppearance
        {
            ModelName = VehicleModelNames.Resolve(model),
            ModelHash = model,
            WheelType = API.GetVehicleWheelType(handle),
            CustomTyres = NativeFixer.GetVehicleModVariation(handle, (int)VehicleModSlot.Wheels) != 0,
            Turbo = API.IsToggleModOn(handle, (int)VehicleModSlot.Turbo),
            TyreSmoke = API.IsToggleModOn(handle, (int)VehicleModSlot.TyreSmoke),
            XenonLights = API.IsToggleModOn(handle, (int)VehicleModSlot.XenonLights),
            BulletproofTyres = !API.GetVehicleTyresCanBurst(handle),
            DriftTyres = ReadDriftTyres(handle),
            PaintFade = API.GetVehicleEnveffScale(handle),
            Livery = API.GetVehicleLivery(handle),
            RoofLivery = API.GetVehicleRoofLivery(handle),
            WindowTint = API.GetVehicleWindowTint(handle),
            PlateText = API.GetVehicleNumberPlateText(handle) ?? string.Empty,
            PlateStyle = API.GetVehicleNumberPlateTextIndex(handle),
            DirtLevel = API.GetVehicleDirtLevel(handle),
            Mods = ReadMods(handle),
            Extras = ReadExtras(handle),
        };

        ReadPaint(handle, appearance);
        ReadLights(handle, appearance);
        ReadTyreSmokeColor(handle, appearance);

        return appearance;
    }

    private static List<VehicleModValue> ReadMods(int handle)
    {
        var mods = new List<VehicleModValue>();

        foreach (var slot in VehicleModSlots.Available(handle, includeWheelSlots: true))
        {
            mods.Add(new VehicleModValue
            {
                Slot = (int)slot,
                Value = API.GetVehicleMod(handle, (int)slot),
            });
        }

        return mods;
    }

    private static List<VehicleExtraState> ReadExtras(int handle)
    {
        var extras = new List<VehicleExtraState>();

        for (var id = 0; id < ExtraCount; id++)
        {
            if (!API.DoesExtraExist(handle, id))
            {
                continue;
            }

            extras.Add(new VehicleExtraState
            {
                Id = id,
                On = API.IsVehicleExtraTurnedOn(handle, id),
            });
        }

        return extras;
    }

    private static void ReadPaint(int handle, VehicleAppearance appearance)
    {
        NativeFixer.GetVehicleColours(handle, out var primary, out var secondary);
        NativeFixer.GetVehicleExtraColours(handle, out var pearlescent, out var wheel);
        NativeFixer.GetVehicleDashboardColour(handle, out var dashboard);
        NativeFixer.GetVehicleInteriorColour(handle, out var interior);

        appearance.PrimaryColor = primary;
        appearance.SecondaryColor = secondary;
        appearance.PearlescentColor = pearlescent;
        appearance.WheelColor = wheel;
        appearance.DashboardColor = dashboard;
        appearance.InteriorColor = interior;

        appearance.PrimaryPaintType = ReadPrimaryPaintType(handle);
        appearance.SecondaryPaintType = ReadSecondaryPaintType(handle);

        if (API.GetIsVehiclePrimaryColourCustom(handle))
        {
            NativeFixer.GetVehicleCustomPrimaryColour(handle, out var red, out var green, out var blue);

            appearance.CustomPrimaryRed = red;
            appearance.CustomPrimaryGreen = green;
            appearance.CustomPrimaryBlue = blue;
        }

        if (API.GetIsVehicleSecondaryColourCustom(handle))
        {
            NativeFixer.GetVehicleCustomSecondaryColour(handle, out var red, out var green, out var blue);

            appearance.CustomSecondaryRed = red;
            appearance.CustomSecondaryGreen = green;
            appearance.CustomSecondaryBlue = blue;
        }
    }

    private static void ReadLights(int handle, VehicleAppearance appearance)
    {
        appearance.HeadlightColor = API.GetVehicleHeadlightsColour(handle);

        if (TryReadCustomXenon(handle, out var red, out var green, out var blue))
        {
            appearance.CustomXenonRed = red;
            appearance.CustomXenonGreen = green;
            appearance.CustomXenonBlue = blue;
        }

        appearance.NeonLeft = API.IsVehicleNeonLightEnabled(handle, NeonLeft);
        appearance.NeonRight = API.IsVehicleNeonLightEnabled(handle, NeonRight);
        appearance.NeonFront = API.IsVehicleNeonLightEnabled(handle, NeonFront);
        appearance.NeonRear = API.IsVehicleNeonLightEnabled(handle, NeonBack);

        NativeFixer.GetVehicleNeonLightsColour(handle, out var neonRed, out var neonGreen, out var neonBlue);

        appearance.NeonRed = neonRed;
        appearance.NeonGreen = neonGreen;
        appearance.NeonBlue = neonBlue;
    }

    private static void ReadTyreSmokeColor(int handle, VehicleAppearance appearance)
    {
        NativeFixer.GetVehicleTyreSmokeColor(handle, out var red, out var green, out var blue);

        appearance.TyreSmokeRed = red;
        appearance.TyreSmokeGreen = green;
        appearance.TyreSmokeBlue = blue;
    }

    // The generated wrapper reads all three output slots whether or not the game filled them, and the
    // runtime throws rather than handing back a zero when it did not. A vehicle with no custom xenon
    // colour is the normal case, so this is expected rather than exceptional.
    private static bool TryReadCustomXenon(int handle, out int red, out int green, out int blue)
    {
        try
        {
            return NativeFixer.GetVehicleXenonLightsCustomColor(handle, out red, out green, out blue);
        }
        catch (Exception)
        {
            red = 0;
            green = 0;
            blue = 0;

            return false;
        }
    }

    private static int ReadPrimaryPaintType(int handle)
    {
        try
        {
            NativeFixer.GetVehicleModColor_1(handle, out var paintType, out _, out _);

            return paintType;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private static int ReadSecondaryPaintType(int handle)
    {
        try
        {
            NativeFixer.GetVehicleModColor_2(handle, out var paintType, out _);

            return paintType;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private static bool ReadDriftTyres(int handle) =>
        API.GetGameBuildNumber() >= DriftTyresBuild && API.GetDriftTyresEnabled(handle);
}
