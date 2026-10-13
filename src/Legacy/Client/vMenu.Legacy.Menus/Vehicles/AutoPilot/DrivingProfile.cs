namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

public sealed class SavedDrivingProfile
{
    public const int SchemaVersion = 1;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public AutoPilotCategory Category { get; set; }

    // DRIVINGMODE for Vehicle, BOATMODE for Boat, HELIMODE for Helicopter, and for Plane the
    // DRIVINGMODE used only while it taxis along the ground.
    public int Flags { get; set; }

    // Zero means the vehicle's own top speed, which is what the old menu always used.
    public float CruiseSpeed { get; set; }

    public int FlightHeight { get; set; } = 40;

    public int MinHeightAboveTerrain { get; set; } = 20;

    public bool Precise { get; set; } = true;
}

public sealed class SavedDrivingProfileEntry(SavedDrivingProfile profile, int storedVersion)
{
    public SavedDrivingProfile Profile { get; } = profile;

    public int StoredVersion { get; } = storedVersion;

    public bool IsFromNewerBuild => StoredVersion > SavedDrivingProfile.SchemaVersion;
}
