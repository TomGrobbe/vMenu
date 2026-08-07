namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class VehicleOptions
{
    public static readonly FloatSetting DeleteVehicleDistance = new("vMenu.Enhanced.VehicleOptions.DeleteVehicleDistance")
    {
        Description =
            "How far in front of a player on foot vMenu looks for a vehicle to delete, in metres. " +
            "The server checks this too, so raising it is what lets players delete vehicles further " +
            "away, and on a busy server that means deleting somebody else's car by accident more often.",
        Default = 5.0f,
    };

    public static readonly BoolSetting DeleteVehicleCommand = new("vMenu.Enhanced.VehicleOptions.DeleteVehicleCommand")
    {
        Description =
            "Registers a /dv command that does the same thing as the Delete Vehicle menu option, " +
            "including the same permission check. Off by default because /dv is a common command " +
            "name and another resource on your server may already register it.",
        Default = false,
    };

    public static readonly BoolSetting RepairVehicleCommand = new("vMenu.Enhanced.VehicleOptions.RepairVehicleCommand")
    {
        Description =
            "Registers a /fixveh command that does the same thing as the Repair Vehicle menu option, " +
            "including the same permission check. On by default, /fixveh being an unusual enough name " +
            "that another resource is unlikely to have claimed it already. Turn it off if one has.",
        Default = true,
    };

    public static readonly BoolSetting WashVehicleCommand = new("vMenu.Enhanced.VehicleOptions.WashVehicleCommand")
    {
        Description =
            "Registers a /washveh command that does the same thing as the Wash Vehicle menu option, " +
            "including the same permission check. On by default, /washveh being an unusual enough name " +
            "that another resource is unlikely to have claimed it already. Turn it off if one has.",
        Default = true,
    };
}
