namespace vMenu.Enhanced.Data.Actions;

public static class ActionIds
{
    public static class TeleportMenu
    {
        public const string AddCategory = "TeleportMenu.AddCategory";

        public const string AddLocation = "TeleportMenu.AddLocation";

        public const string RemoveCategory = "TeleportMenu.RemoveCategory";

        public const string RemoveLocation = "TeleportMenu.RemoveLocation";
    }

    public static class CharacterCreator
    {
        public const string AddPresetCategory = "CharacterCreator.AddPresetCategory";

        public const string AddPreset = "CharacterCreator.AddPreset";

        public const string RemovePresetCategory = "CharacterCreator.RemovePresetCategory";

        public const string RemovePreset = "CharacterCreator.RemovePreset";
    }

    public static class VehicleOptions
    {
        public const string DeleteVehicle = "VehicleOptions.DeleteVehicle";
    }

    public static class PersonalVehicle
    {
        public const string Set = "PersonalVehicle.Set";

        public const string Forget = "PersonalVehicle.Forget";

        public const string Delete = "PersonalVehicle.Delete";

        public const string KickOccupants = "PersonalVehicle.KickOccupants";

        public const string SetLocked = "PersonalVehicle.SetLocked";

        public const string SetEngine = "PersonalVehicle.SetEngine";

        public const string SetLights = "PersonalVehicle.SetLights";

        public const string SetDoor = "PersonalVehicle.SetDoor";

        public const string SetAllDoors = "PersonalVehicle.SetAllDoors";

        public const string SetWindow = "PersonalVehicle.SetWindow";

        public const string SetAllWindows = "PersonalVehicle.SetAllWindows";

        public const string PlayHornTune = "PersonalVehicle.PlayHornTune";

        public const string Explode = "PersonalVehicle.Explode";
    }

    public static class WeatherOptions
    {
        public const string SetWeather = "WeatherOptions.SetWeather";

        public const string SetBlackout = "WeatherOptions.SetBlackout";

        public const string SetSnow = "WeatherOptions.SetSnow";
    }

    public static class OnlinePlayers
    {
        public const string GetList = "OnlinePlayers.GetList";

        public const string GetCoordsForTeleport = "OnlinePlayers.GetCoordsForTeleport";

        public const string GetCoordsForWaypoint = "OnlinePlayers.GetCoordsForWaypoint";

        public const string Kick = "OnlinePlayers.Kick";

        public const string Kill = "OnlinePlayers.Kill";

        public const string Summon = "OnlinePlayers.Summon";

        public const string GetVehicleForTeleport = "OnlinePlayers.GetVehicleForTeleport";

        public const string SetWantedLevel = "OnlinePlayers.SetWantedLevel";

        public const string SendMessage = "OnlinePlayers.SendMessage";

        public const string GetIdentifiers = "OnlinePlayers.GetIdentifiers";

        public const string DeleteVehicle = "OnlinePlayers.DeleteVehicle";

        public const string GetStatus = "OnlinePlayers.GetStatus";

        public const string RefreshPermissions = "OnlinePlayers.RefreshPermissions";
    }

    public static class DisplaySettings
    {
        public const string AddBlip = "DisplaySettings.AddBlip";

        public const string RemoveBlip = "DisplaySettings.RemoveBlip";
    }

    public static class Admin
    {
        public const string ClearArea = "Admin.ClearArea";

        public const string SetFrozen = "Admin.SetFrozen";

        public const string SetHeld = "Admin.SetHeld";

        public const string DeleteVehicle = "Admin.DeleteVehicle";

        public const string DeleteEmptyVehicles = "Admin.DeleteEmptyVehicles";

        public const string DeleteAllVehicles = "Admin.DeleteAllVehicles";

        public const string Announce = "Admin.Announce";

        public const string GetAnnouncements = "Admin.GetAnnouncements";

        public const string AddAnnouncement = "Admin.AddAnnouncement";

        public const string RemoveAnnouncement = "Admin.RemoveAnnouncement";

        public const string RefreshPermissions = "Admin.RefreshPermissions";
    }

    public static class StaffAlerts
    {
        public const string Raise = "StaffAlerts.Raise";

        public const string Respond = "StaffAlerts.Respond";

        public const string GetList = "StaffAlerts.GetList";

        public const string Dismiss = "StaffAlerts.Dismiss";
    }

    public static class TimeOptions
    {
        public const string SetTime = "TimeOptions.SetTime";

        public const string SetFrozen = "TimeOptions.SetFrozen";

        public const string RealTime = "realtime";
    }
}
