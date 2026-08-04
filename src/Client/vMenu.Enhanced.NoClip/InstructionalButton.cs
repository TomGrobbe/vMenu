namespace vMenu.Enhanced.NoClip;

/// <summary>
/// One instructional button. <paramref name="ButtonGetter"/> is resolved on every rebuild rather
/// than cached, because the buttons come from key mappings the player can rebind at any time.
/// </summary>
internal sealed record InstructionalButton(Func<string> TextGetter, Func<string> ButtonGetter);

