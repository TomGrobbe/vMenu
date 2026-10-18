namespace vMenu.Enhanced.Data.VehicleData;

[Flags]
public enum AircraftWeaponRules
{
    None = 0,

    // The Volatol only has to be off its wheels, not properly airborne.
    LowFlying = 1,

    Vtol = 2,

    // The Oppressor Mk II keeps its countermeasures in mod slot 6 and fires only the inner flares.
    Oppressor = 4,
}

// A class, not a record: the client sandbox refuses the equality a record generates.
public sealed class AircraftWeaponProfile
{
    public bool Bombs { get; set; }

    public bool Flares { get; set; }

    public bool Chaff { get; set; }

    public bool Smoke { get; set; }

    public AircraftWeaponRules Rules { get; set; }

    public float BombPosition { get; set; } = 0.5f;

    public float CameraY { get; set; }

    public float CameraZ { get; set; } = -1.5f;

    public float[] FlareStartX { get; set; } = [-3f, -1.5f, 1.5f, 3f];

    public float FlareStartY { get; set; }

    public float FlareStartZ { get; set; }

    public float FlareTargetY { get; set; } = -4f;

    public float[] ChaffSide { get; set; } = [0.2f, 0.8f, 0f, 1f];

    public float[] ChaffLength { get; set; } = [0.4f, 0.4f, 0.45f, 0.45f];

    public float[] ChaffHeight { get; set; } = [1f, 1f, 1.4f, 1.4f];

    public bool ChaffSpread { get; set; } = true;

    public float ChaffScale { get; set; } = 10f;

    public float[] SmokeOffset { get; set; } = [0f, 0f, 0f];

    public float SmokeScale { get; set; } = 1f;

    public AircraftWeaponProfile Copy() => new()
    {
        Bombs = Bombs,
        Flares = Flares,
        Chaff = Chaff,
        Smoke = Smoke,
        Rules = Rules,
        BombPosition = BombPosition,
        CameraY = CameraY,
        CameraZ = CameraZ,
        FlareStartX = (float[])FlareStartX.Clone(),
        FlareStartY = FlareStartY,
        FlareStartZ = FlareStartZ,
        FlareTargetY = FlareTargetY,
        ChaffSide = (float[])ChaffSide.Clone(),
        ChaffLength = (float[])ChaffLength.Clone(),
        ChaffHeight = (float[])ChaffHeight.Clone(),
        ChaffSpread = ChaffSpread,
        ChaffScale = ChaffScale,
        SmokeOffset = (float[])SmokeOffset.Clone(),
        SmokeScale = SmokeScale,
    };
}

// One entry of aircraft-weapons.json. Every field is optional, a missing one keeps the built in value.
public sealed class AircraftWeaponOverride
{
    public bool? Bombs { get; set; }

    public bool? Flares { get; set; }

    public bool? Chaff { get; set; }

    public bool? Smoke { get; set; }

    public float? BombPosition { get; set; }

    public float? CameraY { get; set; }

    public float? CameraZ { get; set; }

    public float[]? FlareStartX { get; set; }

    public float? FlareStartY { get; set; }

    public float? FlareStartZ { get; set; }

    public float? FlareTargetY { get; set; }

    public float[]? ChaffSide { get; set; }

    public float[]? ChaffLength { get; set; }

    public float[]? ChaffHeight { get; set; }

    public bool? ChaffSpread { get; set; }

    public float? ChaffScale { get; set; }

    public float[]? SmokeOffset { get; set; }

    public float? SmokeScale { get; set; }

    public void ApplyTo(AircraftWeaponProfile profile)
    {
        profile.Bombs = Bombs ?? profile.Bombs;
        profile.Flares = Flares ?? profile.Flares;
        profile.Chaff = Chaff ?? profile.Chaff;
        profile.Smoke = Smoke ?? profile.Smoke;
        profile.BombPosition = BombPosition ?? profile.BombPosition;
        profile.CameraY = CameraY ?? profile.CameraY;
        profile.CameraZ = CameraZ ?? profile.CameraZ;
        profile.FlareStartX = (float[]?)FlareStartX?.Clone() ?? profile.FlareStartX;
        profile.FlareStartY = FlareStartY ?? profile.FlareStartY;
        profile.FlareStartZ = FlareStartZ ?? profile.FlareStartZ;
        profile.FlareTargetY = FlareTargetY ?? profile.FlareTargetY;
        profile.ChaffSide = (float[]?)ChaffSide?.Clone() ?? profile.ChaffSide;
        profile.ChaffLength = (float[]?)ChaffLength?.Clone() ?? profile.ChaffLength;
        profile.ChaffHeight = (float[]?)ChaffHeight?.Clone() ?? profile.ChaffHeight;
        profile.ChaffSpread = ChaffSpread ?? profile.ChaffSpread;
        profile.ChaffScale = ChaffScale ?? profile.ChaffScale;
        profile.SmokeOffset = (float[]?)SmokeOffset?.Clone() ?? profile.SmokeOffset;
        profile.SmokeScale = SmokeScale ?? profile.SmokeScale;
    }
}
