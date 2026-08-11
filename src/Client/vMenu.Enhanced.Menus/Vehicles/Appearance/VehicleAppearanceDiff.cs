using System.Globalization;

using vMenu.Enhanced.Menus.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

/// <summary>
/// Compares what a vehicle was asked to look like against what it actually looks like.
/// </summary>
/// <remarks>
/// Written out field by field on purpose. Reflection would drift out of step with the model quietly,
/// and the names in the output are meant to be read by a person rather than to match a property.
/// </remarks>
public static class VehicleAppearanceDiff
{
    /// <summary>Floats the game rounds or clamps, so an exact match is not a fair test.</summary>
    private const float FloatTolerance = 0.02f;

    /// <summary>
    /// The game stores dirt in whole steps up to fifteen, so anything within half a step is the
    /// same dirt.
    /// </summary>
    private const float DirtTolerance = 0.5f;

    public static List<AppearanceDifference> Compare(VehicleAppearance expected, VehicleAppearance actual)
    {
        var differences = new List<AppearanceDifference>();

        if (expected.ModelHash != actual.ModelHash)
        {
            differences.Add(new AppearanceDifference("Model", expected.ModelName, actual.ModelName));
        }

        ComparePaint(expected, actual, differences);
        CompareWheels(expected, actual, differences);
        CompareLights(expected, actual, differences);
        CompareBodywork(expected, actual, differences);
        CompareMods(expected, actual, differences);
        CompareExtras(expected, actual, differences);

        return differences;
    }

    private static void ComparePaint(
        VehicleAppearance expected,
        VehicleAppearance actual,
        List<AppearanceDifference> differences)
    {
        Number("Primary colour", expected.PrimaryColor, actual.PrimaryColor, differences);
        Number("Primary paint finish", expected.PrimaryPaintType, actual.PrimaryPaintType, differences);
        Number("Secondary colour", expected.SecondaryColor, actual.SecondaryColor, differences);
        Number("Secondary paint finish", expected.SecondaryPaintType, actual.SecondaryPaintType, differences);
        Number("Pearlescent colour", expected.PearlescentColor, actual.PearlescentColor, differences);
        Number("Wheel colour", expected.WheelColor, actual.WheelColor, differences);
        Number("Dashboard colour", expected.DashboardColor, actual.DashboardColor, differences);
        Number("Interior colour", expected.InteriorColor, actual.InteriorColor, differences);

        Rgb(
            "Custom primary colour",
            expected.CustomPrimaryRed, expected.CustomPrimaryGreen, expected.CustomPrimaryBlue,
            actual.CustomPrimaryRed, actual.CustomPrimaryGreen, actual.CustomPrimaryBlue,
            differences);

        Rgb(
            "Custom secondary colour",
            expected.CustomSecondaryRed, expected.CustomSecondaryGreen, expected.CustomSecondaryBlue,
            actual.CustomSecondaryRed, actual.CustomSecondaryGreen, actual.CustomSecondaryBlue,
            differences);

        Decimal("Paint fade", expected.PaintFade, actual.PaintFade, FloatTolerance, differences);
    }

    private static void CompareWheels(
        VehicleAppearance expected,
        VehicleAppearance actual,
        List<AppearanceDifference> differences)
    {
        Number("Wheel type", expected.WheelType, actual.WheelType, differences);
        Flag("Custom tyres", expected.CustomTyres, actual.CustomTyres, differences);
        Flag("Bulletproof tyres", expected.BulletproofTyres, actual.BulletproofTyres, differences);
        Flag("Drift tyres", expected.DriftTyres, actual.DriftTyres, differences);
        Flag("Turbo", expected.Turbo, actual.Turbo, differences);
        Flag("Tyre smoke", expected.TyreSmoke, actual.TyreSmoke, differences);

        Rgb(
            "Tyre smoke colour",
            expected.TyreSmokeRed, expected.TyreSmokeGreen, expected.TyreSmokeBlue,
            actual.TyreSmokeRed, actual.TyreSmokeGreen, actual.TyreSmokeBlue,
            differences);
    }

    private static void CompareLights(
        VehicleAppearance expected,
        VehicleAppearance actual,
        List<AppearanceDifference> differences)
    {
        Flag("Xenon lights", expected.XenonLights, actual.XenonLights, differences);
        Number("Headlight colour", expected.HeadlightColor, actual.HeadlightColor, differences);

        Rgb(
            "Custom xenon colour",
            expected.CustomXenonRed, expected.CustomXenonGreen, expected.CustomXenonBlue,
            actual.CustomXenonRed, actual.CustomXenonGreen, actual.CustomXenonBlue,
            differences);

        Flag("Front neon", expected.NeonFront, actual.NeonFront, differences);
        Flag("Rear neon", expected.NeonRear, actual.NeonRear, differences);
        Flag("Left neon", expected.NeonLeft, actual.NeonLeft, differences);
        Flag("Right neon", expected.NeonRight, actual.NeonRight, differences);

        Rgb(
            "Neon colour",
            expected.NeonRed, expected.NeonGreen, expected.NeonBlue,
            actual.NeonRed, actual.NeonGreen, actual.NeonBlue,
            differences);
    }

    private static void CompareBodywork(
        VehicleAppearance expected,
        VehicleAppearance actual,
        List<AppearanceDifference> differences)
    {
        Number("Livery", expected.Livery, actual.Livery, differences);
        Number("Roof livery", expected.RoofLivery, actual.RoofLivery, differences);
        Number("Window tint", expected.WindowTint, actual.WindowTint, differences);
        Number("Plate style", expected.PlateStyle, actual.PlateStyle, differences);

        if (!string.Equals(expected.PlateText, actual.PlateText, StringComparison.Ordinal))
        {
            differences.Add(new AppearanceDifference("Plate text", $"\"{expected.PlateText}\"", $"\"{actual.PlateText}\""));
        }

        Decimal("Dirt level", expected.DirtLevel, actual.DirtLevel, DirtTolerance, differences);
    }

    private static void CompareMods(
        VehicleAppearance expected,
        VehicleAppearance actual,
        List<AppearanceDifference> differences)
    {
        foreach (var mod in expected.Mods)
        {
            var slot = (VehicleModSlot)mod.Slot;
            var fitted = actual.ModAt(slot);

            if (fitted != mod.Value)
            {
                differences.Add(new AppearanceDifference(
                    $"Upgrade slot {mod.Slot} ({VehicleModSlots.TechnicalName(slot)})",
                    ModValue(mod.Value),
                    ModValue(fitted)));
            }
        }

        // A slot the vehicle has now but nothing was recorded for. Worth saying, since it means the
        // two vehicles do not have the same set of slots.
        foreach (var mod in actual.Mods)
        {
            if (HasSlot(expected.Mods, mod.Slot))
            {
                continue;
            }

            var slot = (VehicleModSlot)mod.Slot;

            differences.Add(new AppearanceDifference(
                $"Upgrade slot {mod.Slot} ({VehicleModSlots.TechnicalName(slot)})",
                "not recorded",
                ModValue(mod.Value)));
        }
    }

    private static void CompareExtras(
        VehicleAppearance expected,
        VehicleAppearance actual,
        List<AppearanceDifference> differences)
    {
        foreach (var extra in expected.Extras)
        {
            var fitted = actual.ExtraAt(extra.Id);

            if (fitted is null)
            {
                differences.Add(new AppearanceDifference($"Extra {extra.Id}", OnOff(extra.On), "not on this vehicle"));

                continue;
            }

            if (fitted.Value != extra.On)
            {
                differences.Add(new AppearanceDifference($"Extra {extra.Id}", OnOff(extra.On), OnOff(fitted.Value)));
            }
        }

        foreach (var extra in actual.Extras)
        {
            if (expected.ExtraAt(extra.Id) is null)
            {
                differences.Add(new AppearanceDifference($"Extra {extra.Id}", "not recorded", OnOff(extra.On)));
            }
        }
    }

    private static bool HasSlot(List<VehicleModValue> mods, int slot)
    {
        foreach (var mod in mods)
        {
            if (mod.Slot == slot)
            {
                return true;
            }
        }

        return false;
    }

    private static void Number(string field, int expected, int actual, List<AppearanceDifference> differences)
    {
        if (expected != actual)
        {
            differences.Add(new AppearanceDifference(field, Text(expected), Text(actual)));
        }
    }

    private static void Flag(string field, bool expected, bool actual, List<AppearanceDifference> differences)
    {
        if (expected != actual)
        {
            differences.Add(new AppearanceDifference(field, OnOff(expected), OnOff(actual)));
        }
    }

    private static void Decimal(
        string field,
        float expected,
        float actual,
        float tolerance,
        List<AppearanceDifference> differences)
    {
        if (Math.Abs(expected - actual) > tolerance)
        {
            differences.Add(new AppearanceDifference(field, Text(expected), Text(actual)));
        }
    }

    private static void Rgb(
        string field,
        int? expectedRed, int? expectedGreen, int? expectedBlue,
        int? actualRed, int? actualGreen, int? actualBlue,
        List<AppearanceDifference> differences)
    {
        if (expectedRed == actualRed && expectedGreen == actualGreen && expectedBlue == actualBlue)
        {
            return;
        }

        differences.Add(new AppearanceDifference(
            field,
            RgbText(expectedRed, expectedGreen, expectedBlue),
            RgbText(actualRed, actualGreen, actualBlue)));
    }

    private static string RgbText(int? red, int? green, int? blue) =>
        red is null || green is null || blue is null ? "none" : $"{red},{green},{blue}";

    private static string ModValue(int value) =>
        value < 0 ? "stock" : value.ToString(CultureInfo.InvariantCulture);

    private static string OnOff(bool value) => value ? "on" : "off";

    private static string Text(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Text(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
