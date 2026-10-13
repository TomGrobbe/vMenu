namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

// The four kinds of vehicle the game gives a different driving task to. There are deliberately no
// custom categories: which one applies is decided by what the player is sitting in, not by them.
public enum AutoPilotCategory
{
    Vehicle,
    Plane,
    Boat,
    Helicopter,
}
