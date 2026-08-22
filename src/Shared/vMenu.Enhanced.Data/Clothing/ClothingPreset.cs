using vMenu.Enhanced.Data.Appearance;

namespace vMenu.Enhanced.Data.Clothing;

public sealed class ClothingPreset : PedOutfit
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public bool Fits(bool male)
    {
        if (string.Equals(Gender, "male", StringComparison.OrdinalIgnoreCase))
        {
            return male;
        }

        return !string.Equals(Gender, "female", StringComparison.OrdinalIgnoreCase) || !male;
    }
}

public sealed class ClothingPresetCategory
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<ClothingPreset> Presets { get; set; } = [];
}
