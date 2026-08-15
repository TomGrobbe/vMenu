namespace vMenu.Enhanced.Data.PlayerState;

/// <summary>
/// The keys vMenu hangs off a player's state bag, so every other machine can see them.
/// </summary>
public static class PlayerStateKeys
{
    /// <summary>
    /// Whether this player is in noclip. Written by the server. Read by player blips and overhead
    /// names, both of which hide somebody who is, unless the person looking has been granted the
    /// permission to see them anyway.
    /// </summary>
    public const string NoClip = "vMenu:noclip";

    /// <summary>
    /// Whether this player holds the staff permission. Written by the server. Read by player blips
    /// and overhead names, which mark a staff member out in orange for everybody else.
    /// </summary>
    public const string Staff = "vMenu:staff";

    /// <summary>
    /// How this player's glowing clothes behave, as a <c>ClothingGlow</c> value. Written by the
    /// client that chose it.
    /// </summary>
    public const string ClothingGlow = "vMenu:clothingGlow";
}
