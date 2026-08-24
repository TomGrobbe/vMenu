namespace vMenu.Enhanced.MenuFramework;

public enum GateBehaviour
{
    // Stays visible but disabled, with a lock icon and the restricted description. The default, because
    // it tells the player the feature exists and why they cannot use it, and because it avoids MenuAPI's
    // filter entirely.
    Lock,

    // Removed from view via the menu's filter. Nothing hints that it exists. Costs a re-filter whenever
    // the denied set changes, which resets and has to restore the cursor.
    Hide,
}
