namespace vMenu.Enhanced.Data.PedModels;

// A group of ped models, as the server read them out of the config file.
public sealed class PedModelCategory
{
    public string Name { get; set; } = string.Empty;

    public List<PedModelEntry> Peds { get; set; } = [];
}

public sealed class PedModelEntry
{
    public string Model { get; set; } = string.Empty;

    // What shows next to the model name in the menu.
    public string Label { get; set; } = string.Empty;
}
