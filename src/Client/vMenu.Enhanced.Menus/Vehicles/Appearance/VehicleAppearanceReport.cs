using System.Globalization;

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

public static class VehicleAppearanceReport
{
    // One line per setting, grouped. handle is a live vehicle to ask for slot names; without one the
    // slots are named by vMenu instead, which is what happens for a saved vehicle that is not spawned.
    public static List<string> Describe(VehicleAppearance appearance, int? handle = null)
    {
        var lines = new List<string>
        {
            $"Model: {appearance.ModelName} ({appearance.ModelHash})",
            string.Empty,
            "Paint:",
            $"  Primary: {ColorText(appearance.PrimaryColor)}, finish {FinishName(appearance.PrimaryPaintType)}",
            $"  Secondary: {ColorText(appearance.SecondaryColor)}, finish {FinishName(appearance.SecondaryPaintType)}",
            $"  Pearlescent: {ColorText(appearance.PearlescentColor)}",
            $"  Wheel: {ColorText(appearance.WheelColor)}",
            $"  Dashboard: {ColorText(appearance.DashboardColor)}",
            $"  Interior: {ColorText(appearance.InteriorColor)}",
            $"  Custom primary: {Rgb(appearance.CustomPrimaryRed, appearance.CustomPrimaryGreen, appearance.CustomPrimaryBlue)}",
            $"  Custom secondary: {Rgb(appearance.CustomSecondaryRed, appearance.CustomSecondaryGreen, appearance.CustomSecondaryBlue)}",
            $"  Paint fade: {Number(appearance.PaintFade)}",
            string.Empty,
            "Wheels and tyres:",
            $"  Wheel type: {appearance.WheelType} ({WheelTypeName(appearance.WheelType)})",
            $"  Custom tyres: {appearance.CustomTyres}",
            $"  Bulletproof tyres: {appearance.BulletproofTyres}",
            $"  Drift tyres: {appearance.DriftTyres}",
            string.Empty,
            "Toggles:",
            $"  Turbo: {appearance.Turbo}",
            $"  Tyre smoke: {appearance.TyreSmoke}, colour {appearance.TyreSmokeRed},{appearance.TyreSmokeGreen},{appearance.TyreSmokeBlue}",
            $"  Xenon lights: {appearance.XenonLights}",
            string.Empty,
            "Lights:",
            $"  Headlight colour: {appearance.HeadlightColor} ({HeadlightName(appearance.HeadlightColor)})",
            $"  Custom xenon: {Rgb(appearance.CustomXenonRed, appearance.CustomXenonGreen, appearance.CustomXenonBlue)}",
            $"  Neon: front {appearance.NeonFront}, rear {appearance.NeonRear}, left {appearance.NeonLeft}, right {appearance.NeonRight}",
            $"  Neon colour: {appearance.NeonRed},{appearance.NeonGreen},{appearance.NeonBlue}",
            string.Empty,
            "Bodywork:",
            $"  Livery: {appearance.Livery}",
            $"  Roof livery: {appearance.RoofLivery}",
            $"  Window tint: {appearance.WindowTint}",
            $"  Plate: \"{appearance.PlateText}\" style {appearance.PlateStyle}",
            $"  Dirt level: {Number(appearance.DirtLevel)}",
            string.Empty,
            $"Extras ({appearance.Extras.Count}):",
        };

        if (appearance.Extras.Count == 0)
        {
            lines.Add("  none");
        }

        foreach (var extra in appearance.Extras)
        {
            lines.Add($"  #{extra.Id}: {(extra.On ? "on" : "off")}");
        }

        lines.Add(string.Empty);
        lines.Add($"Upgrades ({appearance.Mods.Count}):");

        if (appearance.Mods.Count == 0)
        {
            lines.Add("  none");
        }

        foreach (var mod in appearance.Mods)
        {
            var slot = (VehicleModSlot)mod.Slot;
            var value = mod.Value < 0 ? "stock" : mod.Value.ToString(CultureInfo.InvariantCulture);

            lines.Add($"  [{mod.Slot}] {SlotName(slot, handle)}: {value}");
        }

        return lines;
    }

    // The name for an upgrade slot, preferring whatever the game calls it.
    public static string SlotName(VehicleModSlot slot, int? handle)
    {
        var fallback = VehicleModSlots.TechnicalName(slot);

        if (handle is not { } vehicle)
        {
            return fallback;
        }

        return GameLabels.Text(Native.GetModSlotName(vehicle, (int)slot), fallback);
    }

    private static string ColorText(int colorId)
    {
        var option = VehicleColorTables.Find(colorId);

        if (option is null)
        {
            return colorId.ToString(CultureInfo.InvariantCulture);
        }

        return $"{colorId} ({GameLabels.Text(option.GxtKey, VehicleColorTables.FallbackName(option.GxtKey))})";
    }

    private static string HeadlightName(int index)
    {
        if (index == VehicleLightColors.DefaultHeadlightColor)
        {
            return "default";
        }

        var color = VehicleLightColors.At(index);

        return color is null ? "unknown" : GameLabels.Text(color.GxtKey, GameLabels.Humanise(color.GxtKey));
    }

    private static string FinishName(int paintType) => paintType switch
    {
        0 => "normal",
        1 => "metallic",
        2 => "pearlescent",
        3 => "matte",
        4 => "metal",
        5 => "chrome",
        6 => "chameleon",
        _ => paintType.ToString(CultureInfo.InvariantCulture),
    };

    // Spelled out rather than looked up: this goes to a console, where the player's menu language would
    // only make the report harder to compare against somebody else's.
    private static string WheelTypeName(int wheelType) => wheelType switch
    {
        0 => "sports",
        1 => "muscle",
        2 => "lowrider",
        3 => "SUV",
        4 => "off-road",
        5 => "tuner",
        6 => "bike",
        7 => "high end",
        8 => "Benny's originals",
        9 => "Benny's bespoke",
        10 => "open wheel",
        11 => "street",
        12 => "track",
        _ => "unknown",
    };

    private static string Rgb(int? red, int? green, int? blue) =>
        red is null || green is null || blue is null ? "none" : $"{red},{green},{blue}";

    private static string Number(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
