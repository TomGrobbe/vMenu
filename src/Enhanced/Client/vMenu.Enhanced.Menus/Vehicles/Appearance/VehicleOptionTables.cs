using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// One choice in a fixed list, where the order shown is not the order the natives use.
public sealed class VehicleChoice(int value, MenuText text)
{
    // What the native wants, which is not the same as this choice's position in the list.
    public int Value { get; } = value;

    public MenuText Text { get; } = text;
}

public static class VehicleOptionTables
{
    // In the order SetVehicleWheelType numbers them, so the position in this list is the value.
    public static IReadOnlyList<string> WheelTypeKeys { get; } =
    [
        Loc.VehicleOptions.WheelTypeSports,
        Loc.VehicleOptions.WheelTypeMuscle,
        Loc.VehicleOptions.WheelTypeLowrider,
        Loc.VehicleOptions.WheelTypeSuv,
        Loc.VehicleOptions.WheelTypeOffroad,
        Loc.VehicleOptions.WheelTypeTuner,
        Loc.VehicleOptions.WheelTypeBike,
        Loc.VehicleOptions.WheelTypeHighEnd,
        Loc.VehicleOptions.WheelTypeBennysOriginal,
        Loc.VehicleOptions.WheelTypeBennysBespoke,
        Loc.VehicleOptions.WheelTypeOpenWheel,
        Loc.VehicleOptions.WheelTypeStreet,
        Loc.VehicleOptions.WheelTypeTrack,
    ];

    // Clearest to darkest, which is not the order the native numbers them in.
    public static IReadOnlyList<VehicleChoice> WindowTints { get; } =
    [
        new(4, MenuText.Key(Loc.VehicleOptions.TintStock)),
        new(0, MenuText.Key(Loc.VehicleOptions.TintNone)),
        new(5, MenuText.Key(Loc.VehicleOptions.TintLimo)),
        new(3, MenuText.Key(Loc.VehicleOptions.TintLightSmoke)),
        new(2, MenuText.Key(Loc.VehicleOptions.TintDarkSmoke)),
        new(1, MenuText.Key(Loc.VehicleOptions.TintPureBlack)),
        new(6, MenuText.Key(Loc.VehicleOptions.TintGreen)),
    ];

    // The position in the list is the value SetVehicleNumberPlateTextIndex wants. North Yankton is the
    // odd one out: the game files its name under PROL rather than under the CMOD_PLA key its neighbours use.
    public static IReadOnlyList<MenuText> PlateStyles { get; } =
    [
        GameLabels.GameOrLiteral("CMOD_PLA_0", "Blue on White 1"),
        GameLabels.GameOrLiteral("CMOD_PLA_1", "Blue on White 2"),
        GameLabels.GameOrLiteral("CMOD_PLA_2", "Blue on White 3"),
        GameLabels.GameOrLiteral("CMOD_PLA_3", "Yellow on Blue"),
        GameLabels.GameOrLiteral("CMOD_PLA_4", "Yellow on Black"),
        GameLabels.GameOrLiteral("PROL", "North Yankton"),
        GameLabels.GameOrLiteral("CMOD_PLA_6", "eCola"),
        GameLabels.GameOrLiteral("CMOD_PLA_7", "Las Venturas"),
        GameLabels.GameOrLiteral("CMOD_PLA_8", "Liberty City"),
        GameLabels.GameOrLiteral("CMOD_PLA_9", "LS Car Meet"),
        GameLabels.GameOrLiteral("CMOD_PLA_10", "LS Panic"),
        GameLabels.GameOrLiteral("CMOD_PLA_11", "LS Pounders"),
        GameLabels.GameOrLiteral("CMOD_PLA_12", "Sprunk"),
    ];

    // The position in the list is the value SetVehicleModColor_1 wants for its colour type.
    public static IReadOnlyList<string> PaintFinishKeys { get; } =
    [
        Loc.VehicleOptions.FinishNormal,
        Loc.VehicleOptions.FinishMetallic,
        Loc.VehicleOptions.FinishPearlescent,
        Loc.VehicleOptions.FinishMatte,
        Loc.VehicleOptions.FinishMetal,
        Loc.VehicleOptions.FinishChrome,
        Loc.VehicleOptions.FinishChameleon,
    ];

    // The position in a choice list holding this native value, or 0 when it is not there.
    public static int IndexOfValue(IReadOnlyList<VehicleChoice> choices, int value)
    {
        for (var index = 0; index < choices.Count; index++)
        {
            if (choices[index].Value == value)
            {
                return index;
            }
        }

        return 0;
    }
}
