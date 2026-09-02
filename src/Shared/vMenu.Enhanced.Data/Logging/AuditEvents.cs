namespace vMenu.Enhanced.Data.Logging;

public static class AuditEvents
{
    public const string Menu = "vMenu.Enhanced:Audit:Menu";

    public const string Theme = "vMenu.Enhanced:Audit:Theme";

    public const string Action = "vMenu.Enhanced:Audit:Action";

    public const string Plugin = "vMenu.Enhanced:Audit:Plugin";
}

public static class MenuActionKinds
{
    public const string Button = "button";

    public const string Checkbox = "checkbox";

    public const string List = "list";

    public const string Slider = "slider";

    public const string DynamicList = "dynamic";
}

public static class AuditActions
{
    public const string VehicleSpawned = "vehicle.spawned";

    public const string TeleportWaypoint = "teleport.waypoint";

    public const string TeleportCoords = "teleport.coords";

    public const string TeleportLocation = "teleport.location";

    public const string LoadoutEquipped = "loadout.equipped";

    public const string VehicleModsChanged = "vehicle.mods";
}
