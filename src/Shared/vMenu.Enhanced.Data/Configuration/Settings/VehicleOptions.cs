namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class VehicleOptions
{
    public static readonly FloatSetting DeleteVehicleDistance = new("vMenu.Enhanced.VehicleOptions.DeleteVehicleDistance")
    {
        Description =
            "Distance in meters how far away the /dv command (and menu delete vehicle action) reaches for cars in front of the player.",
        Default = 5.0f,
    };

    public static readonly BoolSetting DeleteVehicleCommand = new("vMenu.Enhanced.VehicleOptions.DeleteVehicleCommand")
    {
        Description =
            "Enables or disables the /dv command from vMenu. Uses the same permission as the delete vehicle option inside the vehicle options menu.",
        Default = true,
    };

    public static readonly BoolSetting RepairVehicleCommand = new("vMenu.Enhanced.VehicleOptions.RepairVehicleCommand")
    {
        Description =
            "Enables the /fixveh command to quickly repair a vehicle. Uses the same permission as the fix vehicle option inside the vehicle options menu.",
        Default = true,
    };

    public static readonly BoolSetting WashVehicleCommand = new("vMenu.Enhanced.VehicleOptions.WashVehicleCommand")
    {
        Description =
            "Enables the /washveh command to quickly clean a vehicle. Uses the same permission as the clean vehicle option inside the vehicle options menu.",
        Default = true,
    };

    public static readonly BoolSetting ClearGodModeOnExit = new("vMenu.Enhanced.VehicleOptions.ClearGodModeOnExit")
    {
        Description =
            "When set to true, it disables vehicle god mode for a vehicle as soon as the driver leaves the car (if they had vehicle god mode enabled).",
        Default = true,
    };
}
