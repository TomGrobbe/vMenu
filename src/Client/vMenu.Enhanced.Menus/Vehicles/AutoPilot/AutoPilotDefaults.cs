using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

public static class AutoPilotDefaults
{
    public static StringDefault Preference(AutoPilotCategory category) => category switch
    {
        AutoPilotCategory.Plane => UserDefaults.AutoPilotPlaneProfile,
        AutoPilotCategory.Boat => UserDefaults.AutoPilotBoatProfile,
        AutoPilotCategory.Helicopter => UserDefaults.AutoPilotHeliProfile,
        _ => UserDefaults.AutoPilotVehicleProfile,
    };

    public static string Selected(AutoPilotCategory category) => Preference(category).Value;

    public static void Select(AutoPilotCategory category, string name) => Preference(category).Value = name;

    public static bool IsSelected(AutoPilotCategory category, string name) =>
        name.Length > 0 && string.Equals(Selected(category), name, StringComparison.Ordinal);

    public static void Rename(AutoPilotCategory category, string oldName, string newName)
    {
        if (IsSelected(category, oldName))
        {
            Select(category, newName);
        }
    }

    public static void Forget(AutoPilotCategory category, string name)
    {
        if (IsSelected(category, name))
        {
            Select(category, string.Empty);
        }
    }

    public static SavedDrivingProfile Resolve(AutoPilotCategory category)
    {
        var name = Selected(category);

        if (name.Length == 0)
        {
            return AutoPilotPresets.Default(category);
        }

        if (DrivingProfileStore.Load(name) is { } stored && stored.Profile.Category == category)
        {
            return stored.Profile;
        }

        return AutoPilotPresets.Find(category, name) ?? AutoPilotPresets.Default(category);
    }

    public static List<SavedDrivingProfile> Choices(AutoPilotCategory category)
    {
        var choices = new List<SavedDrivingProfile>(AutoPilotPresets.For(category));

        foreach (var entry in DrivingProfileStore.InCategory(category))
        {
            choices.Add(entry.Profile);
        }

        return choices;
    }
}
