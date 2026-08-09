using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

/// <summary>
/// The game's names for horns, which it does not hand out the way it does every other part.
/// </summary>
/// <remarks>
/// <c>GetModTextLabel</c> answers nothing for the horn slot. The mod shop instead reads the horn's
/// identifier hash, which is the name of the sound it plays, and looks the display name up from
/// that. This is the shop's own table, flattened: the game goes hash to an internal horn enum and
/// the enum to a text key, and since nothing else uses that enum the two steps are collapsed into
/// one here.
///
/// <para>
/// The Luxe entries really are crossed over in the game's data, with <c>LUXE_HORN_2</c> naming the
/// first one. That is copied rather than corrected, so vMenu says what the mod shop says.
/// </para>
/// </remarks>
public static class VehicleHornLabels
{
    /// <summary>What the shop calls the horn a vehicle was built with.</summary>
    public const string StockKey = "CMOD_HRN_0";

    private static readonly (string Sound, string TextKey)[] Names =
    [
        ("INDEP_HORN_1", "HORN_INDI_1"),
        ("INDEP_HORN_2", "HORN_INDI_2"),
        ("INDEP_HORN_3", "HORN_INDI_3"),
        ("INDEP_HORN_4", "HORN_INDI_4"),

        ("HIPSTER_HORN_1", "HORN_HIPS1"),
        ("HIPSTER_HORN_2", "HORN_HIPS2"),
        ("HIPSTER_HORN_3", "HORN_HIPS3"),
        ("HIPSTER_HORN_4", "HORN_HIPS4"),

        ("DLC_BUSI2_C_MAJOR_NOTES_C0", "HORN_CNOTE_C0"),
        ("DLC_BUSI2_C_MAJOR_NOTES_D0", "HORN_CNOTE_D0"),
        ("DLC_BUSI2_C_MAJOR_NOTES_E0", "HORN_CNOTE_E0"),
        ("DLC_BUSI2_C_MAJOR_NOTES_F0", "HORN_CNOTE_F0"),
        ("DLC_BUSI2_C_MAJOR_NOTES_G0", "HORN_CNOTE_G0"),
        ("DLC_BUSI2_C_MAJOR_NOTES_A0", "HORN_CNOTE_A0"),
        ("DLC_BUSI2_C_MAJOR_NOTES_B0", "HORN_CNOTE_B0"),
        ("DLC_BUSI2_C_MAJOR_NOTES_C1", "HORN_CNOTE_C1"),

        ("MUSICAL_HORN_BUSINESS_1", "HORN_CLAS1"),
        ("MUSICAL_HORN_BUSINESS_2", "HORN_CLAS2"),
        ("MUSICAL_HORN_BUSINESS_3", "HORN_CLAS3"),
        ("MUSICAL_HORN_BUSINESS_4", "HORN_CLAS4"),
        ("MUSICAL_HORN_BUSINESS_5", "HORN_CLAS5"),
        ("MUSICAL_HORN_BUSINESS_6", "HORN_CLAS6"),
        ("MUSICAL_HORN_BUSINESS_7", "HORN_CLAS7"),

        ("LUXE_HORN_2", "HORN_LUXE1"),
        ("LUXE_HORN_1", "HORN_LUXE2"),
        ("LUXE_HORN_3", "HORN_LUXE3"),

        ("LOWRIDER_HORN_1", "HORN_LOWRDER1"),
        ("LOWRIDER_HORN_2", "HORN_LOWRDER2"),

        ("ORGAN_HORN_LOOP_01", "HORN_HWEEN1"),
        ("ORGAN_HORN_LOOP_02", "HORN_HWEEN2"),

        ("XM15_HORN_01", "HORN_XM15_1"),
        ("XM15_HORN_02", "HORN_XM15_2"),
        ("XM15_HORN_03", "HORN_XM15_3"),

        ("HORN_CLOWN", "CMOD_HRN_CLO"),
        ("HORN_COP", "CMOD_HRN_COP"),
        ("HORN_TRUCK", "CMOD_HRN_TRK"),
        ("HORN_MUSICAL_1", "CMOD_HRN_MUS1"),
        ("HORN_MUSICAL_2", "CMOD_HRN_MUS2"),
        ("HORN_MUSICAL_3", "CMOD_HRN_MUS3"),
        ("HORN_MUSICAL_4", "CMOD_HRN_MUS4"),
        ("HORN_MUSICAL_5", "CMOD_HRN_MUS5"),
        ("HORN_SAD_TROMBONE", "CMOD_HRN_SAD"),

        ("DLC_AW_AIRHORN_01", "CMOD_AIRHORN_01"),
        ("DLC_AW_AIRHORN_02", "CMOD_AIRHORN_02"),
        ("DLC_AW_AIRHORN_03", "CMOD_AIRHORN_03"),
    ];

    // Hashed once, on first use rather than at type load, so nothing runs before the game is ready
    // to answer. A flat array walked in order rather than a dictionary, which would want an equality
    // comparer the client sandbox will not hand out, and forty three entries is nothing to walk.
    private static (uint Hash, string TextKey)[]? _hashed;

    /// <summary>The game's text key for the horn in a slot, or null when it is not one vMenu knows.</summary>
    public static string? TextKey(int handle, int index)
    {
        var sound = (uint)Native.GetVehicleModIdentifierHash(handle, (int)VehicleModSlot.Horn, index);

        foreach (var horn in Hashed())
        {
            if (horn.Hash == sound)
            {
                return horn.TextKey;
            }
        }

        return null;
    }

    private static (uint Hash, string TextKey)[] Hashed()
    {
        if (_hashed is { } hashed)
        {
            return hashed;
        }

        var built = new (uint Hash, string TextKey)[Names.Length];

        for (var index = 0; index < Names.Length; index++)
        {
            built[index] = (API.Hash(Names[index].Sound), Names[index].TextKey);
        }

        _hashed = built;

        return built;
    }
}
