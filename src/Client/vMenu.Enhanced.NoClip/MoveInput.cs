namespace vMenu.Enhanced.NoClip;

/// <summary>Movement requested by the player this frame, relative to the noclipped entity.</summary>
internal readonly record struct MoveInput(float Forward, float Vertical, float HeadingDelta);
