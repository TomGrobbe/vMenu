namespace vMenu.Enhanced.Data.PlayerState;

public static class PlayerStateKeys
{
    // Written by the server. Read by player blips and overhead names, both of which hide somebody who
    // is in noclip, unless the person looking may see them anyway.
    public const string NoClip = "vMenu:noclip";

    // Written by the server. Read by player blips and overhead names, which mark a staff member out in
    // orange for everybody else.
    public const string Staff = "vMenu:staff";

    // A ClothingGlow value, written by the client that chose it.
    public const string ClothingGlow = "vMenu:clothingGlow";
}
