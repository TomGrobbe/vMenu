namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class DisplaySettings
    {
        [English("Display Settings")]
        public const string Title = "displaysettings.title";

        [English("Display Settings")]
        public const string Subtitle = "displaysettings.subtitle";

        [English("Change what vMenu draws on your screen, from the speedometer to the minimap.")]
        public const string LinkDescription = "displaysettings.link.desc";

        [English("Right Align Menu", "Press Enter to toggle left or right aligned menu.")]
        public const string MenuRightAlignment = "displaysettings.rightaligned";

        [English("A right aligned menu is not supported here, so the menu has been left aligned.")]
        public const string MenuRightAlignmentUnsupported = "displaysettings.rightaligned.unsupported";

        [English("Death Notifications", "Show a message whenever a player on this server dies, and who killed them.")]
        public const string DeathNotifications = "displaysettings.deathnotifications";

        [English("Join / Leave Notifications", "Show a message whenever somebody arrives on this server or leaves it.")]
        public const string JoinLeaveNotifications = "displaysettings.joinleavenotifications";

        [English("Speedometer", "Show how fast you are going while you are in a vehicle.")]
        public const string Speedometer = "displaysettings.speedometer";

        [English("Off")]
        public const string SpeedometerOff = "displaysettings.speedometer.off";

        [English("KM/h")]
        public const string SpeedometerKmh = "displaysettings.speedometer.kmh";

        [English("MPH")]
        public const string SpeedometerMph = "displaysettings.speedometer.mph";

        [English("Both")]
        public const string SpeedometerBoth = "displaysettings.speedometer.both";

        [English(
            "Speedometer Position",
            "Where the speedometer sits. In the corner it lifts itself out of the way while the game is showing the vehicle, area and street names, and settles back once they fade.")]
        public const string SpeedometerPosition = "displaysettings.speedometer.position";

        [English("Turn the speedometer on first, then you can pick where it goes.")]
        public const string SpeedometerPositionLocked = "displaysettings.speedometer.position.locked";

        [English("Bottom Right")]
        public const string SpeedometerPositionRight = "displaysettings.speedometer.position.right";

        [English("Bottom Middle")]
        public const string SpeedometerPositionCenter = "displaysettings.speedometer.position.center";

        [English(
            "Vehicle Health",
            "Adds three small bars to the speedometer panel showing how healthy the engine, the body and the fuel tank of your vehicle are. They work whether the speedometer itself is on or off.")]
        public const string VehicleHealth = "displaysettings.vehiclehealth";

        [English(
            "Minimap Key",
            "Change what the minimap key does. When pressed (~b~Z~s~ by default) the minimap will either zoom out or be expanded for 10 seconds, or until you press the key again. You can change the keybind in the pause menu under ~y~Settings~s~, ~y~Key Bindings~s~, ~y~FiveM~s~.")]
        public const string MinimapAction = "displaysettings.minimap.action";

        [English("Nothing")]
        public const string MinimapActionOff = "displaysettings.minimap.action.off";

        [English("Expand The Radar")]
        public const string MinimapActionExpand = "displaysettings.minimap.action.expand";

        [English("Zoom The Radar Out")]
        public const string MinimapActionZoom = "displaysettings.minimap.action.zoom";

        [English(
            "Zoom Amount",
            "How far out the radar zooms. It stays its normal size and shows more ground around you.")]
        public const string MinimapZoom = "displaysettings.minimap.zoom";

        [English("This only matters when the minimap key is set to zoom the radar out.")]
        public const string MinimapZoomLocked = "displaysettings.minimap.zoom.locked";

        [English(
            "Always Keep It That Way",
            "Keeps the radar expanded or zoomed out all the time, so you do not have to press anything. The key does nothing while this is ticked, there being nothing left for it to turn on.")]
        public const string MinimapAlwaysOn = "displaysettings.minimap.alwayson";

        [English("Pick what the minimap key does first, then this can keep it that way.")]
        public const string MinimapAlwaysOnLocked = "displaysettings.minimap.alwayson.locked";

        [English(
            "Location Display",
            "Show the street, the area you are in and which way you are facing, just above the minimap.")]
        public const string LocationDisplay = "displaysettings.locationdisplay";

        [English("Near")]
        public const string LocationNear = "displaysettings.locationdisplay.near";

        [English("N")]
        public const string CompassNorth = "displaysettings.compass.north";

        [English("E")]
        public const string CompassEast = "displaysettings.compass.east";

        [English("S")]
        public const string CompassSouth = "displaysettings.compass.south";

        [English("W")]
        public const string CompassWest = "displaysettings.compass.west";

        [English(
            "Coordinates Display",
            "Show your exact position and heading in the top left corner. Handy when you are building something.")]
        public const string CoordinatesDisplay = "displaysettings.coordinatesdisplay";

        [English("Heading")]
        public const string CoordinatesHeading = "displaysettings.coordinatesdisplay.heading";

        [English(
            "Weather Forecast",
            "Keep a weather forecast on your screen, showing what is coming and how long each spell lasts. The forecast key, which is = unless you changed it in the FiveM key bindings, switches it between off, compact and full.")]
        public const string Forecast = "displaysettings.forecast";

        [English("Forecast")]
        public const string ForecastTitle = "displaysettings.forecast.title";

        [English("Now")]
        public const string ForecastNow = "displaysettings.forecast.now";

        [English("Coming up")]
        public const string ForecastNext = "displaysettings.forecast.next";

        [English("The weather is set by hand on this server, so there is nothing scheduled after it.")]
        public const string ForecastForced = "displaysettings.forecast.forced";

        [English("Moon")]
        public const string ForecastMoon = "displaysettings.forecast.moon";

        [English("Waiting for the server clock.")]
        public const string ForecastNoClock = "displaysettings.forecast.noclock";

        [English("Next 48 hours")]
        public const string ForecastOutlook = "displaysettings.forecast.outlook";

        [English("Outside")]
        public const string ForecastOutside = "displaysettings.forecast.outside";

        [English(
            "Forecast Style",
            "Full shows the whole panel at the top of your screen. Compact shows one quiet line above the minimap with the weather now, the time, and what is coming next.")]
        public const string ForecastStyle = "displaysettings.forecast.style";

        [English("Turn the weather forecast on first.")]
        public const string ForecastStyleLocked = "displaysettings.forecast.style.locked";

        [English("Full")]
        public const string ForecastStyleFull = "displaysettings.forecast.style.full";

        [English("Compact")]
        public const string ForecastStyleCompact = "displaysettings.forecast.style.compact";

        [English(
            "Temperature Unit",
            "Pick whether the weather forecast shows temperatures in degrees Celsius or degrees Fahrenheit.")]
        public const string TemperatureUnit = "displaysettings.forecast.unit";

        [English("Turn the weather forecast on first.")]
        public const string TemperatureUnitLocked = "displaysettings.forecast.unit.locked";

        [English("Celsius (°C)")]
        public const string TemperatureUnitCelsius = "displaysettings.forecast.unit.celsius";

        [English("Fahrenheit (°F)")]
        public const string TemperatureUnitFahrenheit = "displaysettings.forecast.unit.fahrenheit";

        [English(
            "Show Time On Screen",
            "Puts the in game clock above your minimap. It shows inside the weather forecast when that is on, and on its own when it is not.")]
        public const string ShowTime = "displaysettings.showtime";

        [English("Hide Hud", "Takes the hud elements off the screen, including your weapon and anything vMenu draws.")]
        public const string HideHud = "displaysettings.hidehud";

        [English(
            "Hide Radar",
            "Takes the radar and minimap off the screen. Turning it back on respects the radar setting in your pause menu.")]
        public const string HideRadar = "displaysettings.hideradar";

        [English("Toggle Night Vision", "Lights the world up in green, the way the night vision goggles do.")]
        public const string NightVision = "displaysettings.nightvision";

        [English(
            "Toggle Thermal Vision",
            "Shows warm bodies through walls. The game draws this over night vision when you have both switched on.")]
        public const string ThermalVision = "displaysettings.thermalvision";

        [English(
            "Timecycle Modifiers",
            "Coloured screen filters. Two can be on at once, and turning on a third drops whichever has been on longest.")]
        public const string Timecycles = "displaysettings.timecycles";

        [English("Timecycle Modifiers")]
        public const string TimecycleSubtitle = "displaysettings.timecycles.subtitle";

        [English("{query} ({count} of {total})")]
        public const string TimecycleSubtitleFiltered = "displaysettings.timecycles.subtitle.filtered";

        [English("Switch this filter on. Two can be on at once.")]
        public const string TimecycleRowDescription = "displaysettings.timecycles.row.desc";

        [English("On, intensity ~b~{intensity}~s~ of {max}. Page Up and Page Down change it.")]
        public const string TimecycleRowActive = "displaysettings.timecycles.row.active";

        [English(
            "Search",
            "Show only the filters whose name contains what you type. Leave it empty to show them all again.")]
        public const string TimecycleFilter = "displaysettings.timecycles.filter";

        [English("Part of a filter name")]
        public const string TimecycleFilterPrompt = "displaysettings.timecycles.filter.prompt";

        [English("Showing {count} filter(s) matching ~b~{query}~s~.")]
        public const string TimecycleFilterApplied = "displaysettings.timecycles.filter.applied";

        [English("Showing every filter again.")]
        public const string TimecycleFilterCleared = "displaysettings.timecycles.filter.cleared";

        [English("No filter name contains ~b~{query}~s~.")]
        public const string TimecycleFilterNoMatches = "displaysettings.timecycles.filter.nomatches";

        [English("Intensity ({intensity}/{max})")]
        public const string TimecycleIntensityButton = "displaysettings.timecycles.button.intensity";

        [English("Clear All")]
        public const string TimecycleClearButton = "displaysettings.timecycles.button.clear";

        [English("Back To Top")]
        public const string TimecycleTopButton = "displaysettings.timecycles.button.top";

        [English("Favorite")]
        public const string TimecycleFavoriteButton = "displaysettings.timecycles.button.favorite";

        [English(
            "Favorite Timecycle Modifiers",
            "Only the filters you marked with a star. Press End on a filter in the full list to add it here.")]
        public const string TimecycleFavorites = "displaysettings.timecycles.favorites";

        [English("Timecycle Intensity", "How strongly the filters you have on are drawn. Both share the one setting.")]
        public const string TimecycleIntensity = "displaysettings.timecycles.intensity";

        [English(
            "Location Blips",
            "Show the markers this server put on the map. Some of them are always on and are not affected by this.")]
        public const string LocationBlips = "displaysettings.locationblips";

        [English(
            "Show Player Blips",
            "Put a blip on the map for every other player, which changes shape depending on what they are driving. Players far away from you are kept off the minimap to stop it getting cluttered, and appear when you expand it.")]
        public const string PlayerBlips = "displaysettings.playerblips";

        [English(
            "Show Player Names",
            "Float a name above nearby players, along with their health bar and their wanted level. Further away this shrinks to a small arrow, and past that it disappears altogether.")]
        public const string OverheadNames = "displaysettings.overheadnames";

        [English(
            "Show Players In NoClip",
            "Keep showing the blip and the name of anybody who is flying around in noclip, who everybody else has them hidden from. Their blip turns ~g~green~s~ so you can tell them apart. This does not make them visible in the world, so you still see a dot and a name over somebody who is not really there to look at.")]
        public const string SeeNoClipPlayers = "displaysettings.seenoclipplayers";

        [English(
            "Manage Blips",
            "Add a blip where you are standing, or take the nearest one off the map, for everybody on the server.")]
        public const string ManageBlips = "displaysettings.blips.manage";

        [English("Manage Blips")]
        public const string ManageBlipsSubtitle = "displaysettings.blips.manage.subtitle";

        [English("Add Always On Blip Here", "Puts a blip where you are standing that nobody can switch off.")]
        public const string BlipAddAlwaysOn = "displaysettings.blips.add.alwayson";

        [English(
            "Add Toggleable Blip Here",
            "Puts a blip where you are standing that players can hide with the Location Blips option.")]
        public const string BlipAddToggleable = "displaysettings.blips.add.toggleable";

        [English("Remove Nearest Blip", "Takes the blip closest to you off the map, for everybody.")]
        public const string BlipRemoveNearest = "displaysettings.blips.remove";

        [English("~r~Are you sure?~s~ Press again to remove it for everybody.")]
        public const string BlipRemoveNearestConfirm = "displaysettings.blips.remove.confirm";

        [English("Name for the blip")]
        public const string BlipName = "displaysettings.blips.name";

        [English("Sprite id")]
        public const string BlipSprite = "displaysettings.blips.sprite";

        [English("Colour id")]
        public const string BlipColour = "displaysettings.blips.colour";

        [English("Size offset, 0 for normal")]
        public const string BlipScale = "displaysettings.blips.scale";

        [English("The sprite, colour and size all have to be numbers.")]
        public const string BlipInvalidNumber = "displaysettings.blips.invalidnumber";

        [English("There is no blip anywhere near you.")]
        public const string BlipNoneNearby = "displaysettings.blips.nonenearby";

        [English("Blip added.")]
        public const string BlipSaved = "displaysettings.blips.saved";

        [English("You are not allowed to add blips.")]
        public const string BlipSaveDenied = "displaysettings.blips.savedenied";

        [English("There is already a blip with that name right here.")]
        public const string BlipNameTaken = "displaysettings.blips.nametaken";

        [English("The blip could not be saved.")]
        public const string BlipSaveFailed = "displaysettings.blips.savefailed";

        [English("Blip removed.")]
        public const string BlipDeleted = "displaysettings.blips.deleted";

        [English("You are not allowed to remove blips.")]
        public const string BlipDeleteDenied = "displaysettings.blips.deletedenied";

        [English("That blip is already gone.")]
        public const string BlipDeleteGone = "displaysettings.blips.deletegone";

        [English("The blip could not be removed.")]
        public const string BlipDeleteFailed = "displaysettings.blips.deletefailed";
    }
}
