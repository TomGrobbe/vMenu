namespace vMenu.Enhanced.NoClip;

internal sealed record InstructionalButton(
    Func<string> TextGetter,
    Func<string> ButtonGetter);