namespace vMenu.Enhanced.NoClip;

internal sealed record InstructionalButton(Func<string> TextGetter, string Control)
{
    /// <summary>Pre-formatted texture name, so it doesn't have to be built on every rebuild.</summary>
    internal string ControlName { get; } = $"~{Control}~";
}

