using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Appearance;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// Applying is not reliable on the first go. Upgrades have to stream in before the game will accept
// them, and a few settings quietly reset others. Rather than legacy's "apply twice and hope", this
// applies, waits for the upgrades to load, reads the vehicle back and applies again while anything
// still disagrees. Whatever is still wrong after the last pass is handed back to the caller.
public static class VehicleAppearanceWriter
{
    private const int MaxPasses = 3;

    // Frames rather than milliseconds: the wait exists so the game gets a chance to do work, and a
    // player on a slow machine needs more real time for the same amount of it.
    private const int StreamWaitFrames = 60;

    private const int DriftTyresBuild = 2372;

    private const int NeonLeft = 0;

    private const int NeonRight = 1;

    private const int NeonFront = 2;

    private const int NeonBack = 3;

    // Empty when the vehicle now matches exactly.
    public static async Task<List<AppearanceDifference>> ApplyAsync(Vehicle vehicle, VehicleAppearance appearance) =>
        await ApplyAsync(vehicle.Handle, appearance);

    public static async Task<List<AppearanceDifference>> ApplyAsync(int handle, VehicleAppearance appearance)
    {
        var differences = new List<AppearanceDifference>();

        for (var pass = 0; pass < MaxPasses; pass++)
        {
            Apply(handle, appearance);

            await WaitForModsAsync(handle);

            if (!API.DoesEntityExist(handle))
            {
                return differences;
            }

            differences = VehicleAppearanceDiff.Compare(appearance, VehicleAppearanceReader.Read(handle));

            if (differences.Count == 0)
            {
                return differences;
            }
        }

        return differences;
    }

    // One pass. Every call here is idempotent, so repeating it is safe.
    public static void Apply(int handle, VehicleAppearance appearance)
    {
        // Nothing below this line works without it, including reading back what it did.
        API.SetVehicleModKit(handle, 0);

        ApplyExtras(handle, appearance);
        ApplyWheels(handle, appearance);
        ApplyToggles(handle, appearance);
        ApplyLiveries(handle, appearance);
        ApplyPaint(handle, appearance);
        ApplyBodywork(handle, appearance);
        ApplyLights(handle, appearance);
        ApplyMods(handle, appearance);
    }

    private static void ApplyExtras(int handle, VehicleAppearance appearance)
    {
        foreach (var extra in appearance.Extras)
        {
            if (!API.DoesExtraExist(handle, extra.Id))
            {
                continue;
            }

            // The flag says whether to turn the extra off, not on. Getting this backwards is the classic way to
            // have every extra come back inverted.
            API.SetVehicleExtra(handle, extra.Id, !extra.On);
        }
    }

    private static void ApplyWheels(int handle, VehicleAppearance appearance)
    {
        API.SetVehicleWheelType(handle, appearance.WheelType);

        var rims = appearance.ModAt(VehicleModSlot.Wheels);

        API.SetVehicleMod(handle, (int)VehicleModSlot.Wheels, rims, appearance.CustomTyres);

        // A bike carries its rear wheel in its own slot, and the game expects both to be set.
        if (API.IsThisModelABike(appearance.ModelHash))
        {
            var rear = appearance.ModAt(VehicleModSlot.RearWheels);

            API.SetVehicleMod(handle, (int)VehicleModSlot.RearWheels, rear, appearance.CustomTyres);
        }

        API.SetVehicleTyresCanBurst(handle, !appearance.BulletproofTyres);

        if (API.GetGameBuildNumber() >= DriftTyresBuild)
        {
            API.SetDriftTyresEnabled(handle, appearance.DriftTyres);
        }
    }

    private static void ApplyToggles(int handle, VehicleAppearance appearance)
    {
        API.ToggleVehicleMod(handle, (int)VehicleModSlot.Turbo, appearance.Turbo);

        // Colour before the toggle: fitting the kit with the wrong colour on it shows the wrong colour until
        // something else changes.
        API.SetVehicleTyreSmokeColor(
            handle,
            appearance.TyreSmokeRed,
            appearance.TyreSmokeGreen,
            appearance.TyreSmokeBlue);

        API.ToggleVehicleMod(handle, (int)VehicleModSlot.TyreSmoke, appearance.TyreSmoke);

        // Toggling the kit off on its own leaves the smoke showing. The game only lets go of it once the mod
        // is removed as well.
        if (!appearance.TyreSmoke)
        {
            API.RemoveVehicleMod(handle, (int)VehicleModSlot.TyreSmoke);
        }

        API.ToggleVehicleMod(handle, (int)VehicleModSlot.XenonLights, appearance.XenonLights);
    }

    private static void ApplyLiveries(int handle, VehicleAppearance appearance)
    {
        if (appearance.Livery >= 0)
        {
            API.SetVehicleLivery(handle, appearance.Livery);
        }

        if (appearance.RoofLivery >= 0)
        {
            API.SetVehicleRoofLivery(handle, appearance.RoofLivery);
        }
    }

    private static void ApplyPaint(int handle, VehicleAppearance appearance)
    {
        // The mod colour form carries the finish as well as the colour, so it goes first and the plain
        // colour call after it settles any disagreement about the ids themselves.
        API.SetVehicleModColor_1(
            handle,
            appearance.PrimaryPaintType,
            appearance.PrimaryColor,
            appearance.PearlescentColor);

        API.SetVehicleModColor_2(handle, appearance.SecondaryPaintType, appearance.SecondaryColor);

        API.SetVehicleColours(handle, appearance.PrimaryColor, appearance.SecondaryColor);

        // Last of the group: setting a mod colour resets these, so writing them earlier would be undone.
        API.SetVehicleExtraColours(handle, appearance.PearlescentColor, appearance.WheelColor);

        API.SetVehicleInteriorColour(handle, appearance.InteriorColor);
        API.SetVehicleDashboardColour(handle, appearance.DashboardColor);

        if (appearance.CustomPrimaryRed is { } primaryRed
            && appearance.CustomPrimaryGreen is { } primaryGreen
            && appearance.CustomPrimaryBlue is { } primaryBlue)
        {
            API.SetVehicleCustomPrimaryColour(handle, primaryRed, primaryGreen, primaryBlue);
        }
        else
        {
            NativeFixer.ClearVehicleCustomPrimaryColour(handle);
        }

        if (appearance.CustomSecondaryRed is { } secondaryRed
            && appearance.CustomSecondaryGreen is { } secondaryGreen
            && appearance.CustomSecondaryBlue is { } secondaryBlue)
        {
            API.SetVehicleCustomSecondaryColour(handle, secondaryRed, secondaryGreen, secondaryBlue);
        }
        else
        {
            NativeFixer.ClearVehicleCustomSecondaryColour(handle);
        }

        API.SetVehicleEnveffScale(handle, appearance.PaintFade);
    }

    private static void ApplyBodywork(int handle, VehicleAppearance appearance)
    {
        API.SetVehicleWindowTint(handle, appearance.WindowTint);

        if (!string.IsNullOrEmpty(appearance.PlateText))
        {
            API.SetVehicleNumberPlateText(handle, appearance.PlateText);
        }

        API.SetVehicleNumberPlateTextIndex(handle, appearance.PlateStyle);
        API.SetVehicleDirtLevel(handle, appearance.DirtLevel);
    }

    private static void ApplyLights(int handle, VehicleAppearance appearance)
    {
        API.SetVehicleHeadlightsColour(handle, appearance.HeadlightColor);

        if (appearance.CustomXenonRed is { } red
            && appearance.CustomXenonGreen is { } green
            && appearance.CustomXenonBlue is { } blue)
        {
            API.SetVehicleXenonLightsCustomColor(handle, red, green, blue);
        }
        else
        {
            API.ClearVehicleXenonLightsCustomColor(handle);
        }

        API.SetVehicleNeonLightsColour(handle, appearance.NeonRed, appearance.NeonGreen, appearance.NeonBlue);

        API.SetVehicleNeonLightEnabled(handle, NeonLeft, appearance.NeonLeft);
        API.SetVehicleNeonLightEnabled(handle, NeonRight, appearance.NeonRight);
        API.SetVehicleNeonLightEnabled(handle, NeonFront, appearance.NeonFront);
        API.SetVehicleNeonLightEnabled(handle, NeonBack, appearance.NeonRear);
    }

    private static void ApplyMods(int handle, VehicleAppearance appearance)
    {
        foreach (var mod in appearance.Mods)
        {
            var slot = (VehicleModSlot)mod.Slot;

            // The wheel slots were already set alongside the wheel type, which has to come first.
            if (VehicleModSlots.IsWheelSlot(slot))
            {
                continue;
            }

            API.SetVehicleMod(handle, mod.Slot, mod.Value, appearance.CustomTyres);
        }
    }

    private static async Task WaitForModsAsync(int handle)
    {
        for (var frame = 0; frame < StreamWaitFrames; frame++)
        {
            if (!API.DoesEntityExist(handle))
            {
                return;
            }

            if (API.HaveVehicleModsStreamedIn(handle))
            {
                return;
            }

            await BaseScript.Delay(0);
        }
    }
}
