using System.Globalization;

namespace vMenu.Enhanced.Data.Configuration;

// One configurable value, declared once and read by both sides. Default on the derived types
// describes what vMenu does when the convar is unset; it is written into the example file and applied
// by the Value accessors. The four nullable getters never substitute it, so "unset" stays
// distinguishable from "set to the default".
public abstract class Setting(string name)
{
    public string Name { get; } = name;

    public required string Description { get; init; }

    // The value as it is written in the generated example file.
    public abstract string DefaultText { get; }
}

public sealed class BoolSetting(string name) : Setting(name)
{
    public bool Default { get; init; }

    public override string DefaultText => Default ? "true" : "false";
}

public sealed class IntSetting(string name) : Setting(name)
{
    public int Default { get; init; }

    public override string DefaultText => Default.ToString(CultureInfo.InvariantCulture);
}

public sealed class FloatSetting(string name) : Setting(name)
{
    public float Default { get; init; }

    public override string DefaultText => Default.ToString("0.0###", CultureInfo.InvariantCulture);
}

public sealed class StringSetting(string name) : Setting(name)
{
    public string Default { get; init; } = string.Empty;

    public override string DefaultText => $"\"{Default}\"";
}
