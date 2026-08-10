namespace vMenu.Enhanced.Data.PedModels;

/// <summary>A group of ped models, as the server read them out of the config file.</summary>
public sealed class PedModelCategory
{
    public string Name { get; set; } = string.Empty;

    public List<PedModelEntry> Peds { get; set; } = [];
}

public sealed class PedModelEntry
{
    public string Model { get; set; } = string.Empty;

    /// <summary>What shows next to the model name in the menu.</summary>
    public string Label { get; set; } = string.Empty;
}
