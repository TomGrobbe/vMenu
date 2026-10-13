namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// A list of these rather than a dictionary keyed on the slot: a dictionary needs an equality
// comparer the client sandbox will not hand out, and an array serialises in a stable order.
public sealed class VehicleModValue
{
    public int Slot { get; set; }

    // Minus one means the part the vehicle left the factory with.
    public int Value { get; set; } = -1;
}

public sealed class VehicleExtraState
{
    public int Id { get; set; }

    public bool On { get; set; }
}

// The shape written into a saved vehicle, so adding or removing anything here is a change to the
// stored format and needs SavedVehicle.SchemaVersion raised with it. Deliberately left out is
// anything momentary rather than part of how the vehicle looks: open doors, windows, fuel.
//
// A class with settable properties rather than a record: generated equality reaches for
// EqualityComparer<T>.Default, which the client sandbox refuses to load.
public sealed class VehicleAppearance
{
    #region Identity

    public string ModelName { get; set; } = string.Empty;

    public uint ModelHash { get; set; }

    #endregion

    #region Upgrades

    public List<VehicleModValue> Mods { get; set; } = [];

    // The low profile tyres that come with a set of rims.
    public bool CustomTyres { get; set; }

    public int WheelType { get; set; }

    public bool Turbo { get; set; }

    public bool TyreSmoke { get; set; }

    public bool XenonLights { get; set; }

    public bool BulletproofTyres { get; set; }

    public bool DriftTyres { get; set; }

    #endregion

    #region Paint

    // The finish over the primary colour: normal, metallic, matte and so on.
    public int PrimaryPaintType { get; set; }

    public int PrimaryColor { get; set; }

    public int SecondaryPaintType { get; set; }

    public int SecondaryColor { get; set; }

    public int PearlescentColor { get; set; }

    public int WheelColor { get; set; }

    public int DashboardColor { get; set; }

    public int InteriorColor { get; set; }

    // Nullable rather than -1, so a custom colour with a zero channel is not mistaken for no custom
    // colour at all. Legacy used -1 and > 0 guards, and quietly lost any colour containing a zero.
    public int? CustomPrimaryRed { get; set; }

    public int? CustomPrimaryGreen { get; set; }

    public int? CustomPrimaryBlue { get; set; }

    public int? CustomSecondaryRed { get; set; }

    public int? CustomSecondaryGreen { get; set; }

    public int? CustomSecondaryBlue { get; set; }

    // How sun bleached the paint looks, from 0 for factory fresh to 1 for badly faded.
    public float PaintFade { get; set; }

    #endregion

    #region Lights

    // The game's headlight colour index, or 255 for the ones it came with.
    public int HeadlightColor { get; set; } = VehicleLightColors.DefaultHeadlightColor;

    public int? CustomXenonRed { get; set; }

    public int? CustomXenonGreen { get; set; }

    public int? CustomXenonBlue { get; set; }

    public bool NeonFront { get; set; }

    public bool NeonRear { get; set; }

    public bool NeonLeft { get; set; }

    public bool NeonRight { get; set; }

    public int NeonRed { get; set; }

    public int NeonGreen { get; set; }

    public int NeonBlue { get; set; }

    #endregion

    #region Everything else

    public int TyreSmokeRed { get; set; }

    public int TyreSmokeGreen { get; set; }

    public int TyreSmokeBlue { get; set; }

    public int Livery { get; set; } = -1;

    public int RoofLivery { get; set; } = -1;

    public List<VehicleExtraState> Extras { get; set; } = [];

    public int WindowTint { get; set; }

    public string PlateText { get; set; } = string.Empty;

    public int PlateStyle { get; set; }

    public float DirtLevel { get; set; }

    #endregion

    // Minus one when this vehicle has nothing recorded for the slot.
    public int ModAt(VehicleModSlot slot)
    {
        foreach (var mod in Mods)
        {
            if (mod.Slot == (int)slot)
            {
                return mod.Value;
            }
        }

        return -1;
    }

    // Null when the extra was not recorded.
    public bool? ExtraAt(int id)
    {
        foreach (var extra in Extras)
        {
            if (extra.Id == id)
            {
                return extra.On;
            }
        }

        return null;
    }
}
