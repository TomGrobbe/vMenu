namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class VehicleOptions
    {
        [English("Vehicle Options")]
        public const string Title = "vehicleoptions.title";

        [English("Vehicle Options Menu")]
        public const string Subtitle = "vehicleoptions.subtitle";

        [English("Options for your current vehicle.")]
        public const string LinkDescription = "vehicleoptions.link.desc";

        [English("Delete Vehicle")]
        public const string DeleteVehicle = "vehicleoptions.delete";

        [English(
            "Deletes the vehicle you are driving. You have to be in the driver seat, being a passenger is not enough.")]
        public const string DeleteVehicleDescription = "vehicleoptions.delete.desc";

        [English("~r~Are you sure you want to delete this vehicle? Press enter again to confirm.")]
        public const string DeleteVehicleConfirm = "vehicleoptions.delete.confirm";

        [English("You have to be the ~y~driver~s~ to delete this vehicle.")]
        public const string DeleteNotDriver = "vehicleoptions.delete.notdriver";

        [English("You have to be ~y~driving~s~ a vehicle to delete it.")]
        public const string DeleteNotDriving = "vehicleoptions.delete.notdriving";

        [English("~r~No vehicle found~s~ in front of you.")]
        public const string DeleteNoVehicle = "vehicleoptions.delete.novehicle";

        [English("You do not have permission to delete vehicles.")]
        public const string DeleteDenied = "vehicleoptions.delete.denied";

        [English("You are ~r~too far away~s~ from that vehicle.")]
        public const string DeleteTooFar = "vehicleoptions.delete.toofar";

        [English("~r~Could not delete~s~ that vehicle. Try again.")]
        public const string DeleteFailed = "vehicleoptions.delete.failed";

        [English("~g~Vehicle deleted~s~.")]
        public const string Deleted = "vehicleoptions.delete.success";

        [English("Repair Vehicle")]
        public const string RepairVehicle = "vehicleoptions.repair";

        [English("Repairs all visual and mechanical damage on the vehicle you're driving.")]
        public const string RepairVehicleDescription = "vehicleoptions.repair.desc";

        [English("You have to be the ~y~driver~s~ to repair this vehicle.")]
        public const string RepairNotDriver = "vehicleoptions.repair.notdriver";

        [English("You do not have permission to repair vehicles.")]
        public const string RepairDenied = "vehicleoptions.repair.denied";

        [English("You have to be ~y~in a vehicle~s~ to repair it.")]
        public const string RepairNoVehicle = "vehicleoptions.repair.novehicle";

        [English("Vehicle ~g~repaired~s~.")]
        public const string Repaired = "vehicleoptions.repair.success";

        [English("Wash Vehicle")]
        public const string WashVehicle = "vehicleoptions.wash";

        [English("Cleans the dirt and the scrape marks off the vehicle you're driving.")]
        public const string WashVehicleDescription = "vehicleoptions.wash.desc";

        [English("You have to be the ~y~driver~s~ to wash this vehicle.")]
        public const string WashNotDriver = "vehicleoptions.wash.notdriver";

        [English("You have to be ~y~in a vehicle~s~ to wash it.")]
        public const string WashNoVehicle = "vehicleoptions.wash.novehicle";

        [English("You do not have permission to wash vehicles.")]
        public const string WashDenied = "vehicleoptions.wash.denied";

        [English("Vehicle ~g~washed~s~.")]
        public const string Washed = "vehicleoptions.wash.success";

        [English("Cycle Seat")]
        public const string CycleSeat = "vehicleoptions.cycleseat";

        [English(
            "Moves you to the next free seat, and back round to the driver's seat after the last one. Seats somebody else is in are skipped.")]
        public const string CycleSeatDescription = "vehicleoptions.cycleseat.desc";

        [English("You have to be ~y~in a vehicle~s~ to change seats.")]
        public const string CycleSeatNoVehicle = "vehicleoptions.cycleseat.novehicle";

        [English("There is ~y~no free seat~s~ to move to.")]
        public const string CycleSeatNoFreeSeat = "vehicleoptions.cycleseat.nofreeseat";

        #region Shared by every modification section

        [English("Not In A Vehicle")]
        public const string NoVehicle = "vehicleoptions.novehicle";

        [English("Get into the driver's seat of a vehicle and open this menu again.")]
        public const string NoVehicleDescription = "vehicleoptions.novehicle.desc";

        [English("Not The Driver")]
        public const string NotDriverTitle = "vehicleoptions.notdriver.title";

        [English("You have to be the one driving to change anything on this vehicle.")]
        public const string NotDriver = "vehicleoptions.notdriver";

        [English("Nothing To Change Here")]
        public const string Nothing = "vehicleoptions.nothing";

        [English("This vehicle does not offer any of these options.")]
        public const string NothingDescription = "vehicleoptions.nothing.desc";

        /// <summary>Fallback name for anything the game has no label for, such as "Colour #42".</summary>
        [English("{name} #{number}")]
        public const string Numbered = "vehicleoptions.numbered";

        #endregion

        #region Section links

        [English("Modifications")]
        public const string Modifications = "vehicleoptions.mods";

        [English("Every upgrade this vehicle can be fitted with, plus turbo, tyre smoke and window tint.")]
        public const string ModificationsDescription = "vehicleoptions.mods.desc";

        [English("Vehicle Modifications")]
        public const string ModificationsSubtitle = "vehicleoptions.mods.subtitle";

        [English("Colours")]
        public const string Colors = "vehicleoptions.colors";

        [English("Paint this vehicle, inside and out.")]
        public const string ColorsDescription = "vehicleoptions.colors.desc";

        [English("Vehicle Colours")]
        public const string ColorsSubtitle = "vehicleoptions.colors.subtitle";

        [English("Liveries")]
        public const string Liveries = "vehicleoptions.liveries";

        [English("Pick one of the paint jobs this vehicle came with.")]
        public const string LiveriesDescription = "vehicleoptions.liveries.desc";

        [English("Vehicle Liveries")]
        public const string LiveriesSubtitle = "vehicleoptions.liveries.subtitle";

        [English("Extras")]
        public const string Extras = "vehicleoptions.extras";

        [English(
            "Switch the optional parts this vehicle was built with on and off, such as push bars, roof racks and light bars.")]
        public const string ExtrasDescription = "vehicleoptions.extras.desc";

        [English("Vehicle Extras")]
        public const string ExtrasSubtitle = "vehicleoptions.extras.subtitle";

        [English("Neon Lights")]
        public const string Neon = "vehicleoptions.neon";

        [English("Underglow: which sides are lit, and what colour they are.")]
        public const string NeonDescription = "vehicleoptions.neon.desc";

        [English("Vehicle Neon Lights")]
        public const string NeonSubtitle = "vehicleoptions.neon.subtitle";

        [English("Headlights")]
        public const string Lights = "vehicleoptions.lights";

        [English("Fit xenon headlights and choose what colour they shine.")]
        public const string LightsDescription = "vehicleoptions.lights.desc";

        [English("Vehicle Headlights")]
        public const string LightsSubtitle = "vehicleoptions.lights.subtitle";

        [English("Wheels & Tyres")]
        public const string Wheels = "vehicleoptions.wheels";

        [English("Wheel type, rims, custom tyres and bulletproof tyres.")]
        public const string WheelsDescription = "vehicleoptions.wheels.desc";

        [English("Vehicle Wheels & Tyres")]
        public const string WheelsSubtitle = "vehicleoptions.wheels.subtitle";

        [English("Licence Plate")]
        public const string Plate = "vehicleoptions.plate";

        [English("Change what the plate says and which style it uses.")]
        public const string PlateDescription = "vehicleoptions.plate.desc";

        [English("Vehicle Licence Plate")]
        public const string PlateSubtitle = "vehicleoptions.plate.subtitle";

        [English("Doors")]
        public const string Doors = "vehicleoptions.doors";

        [English("Open, close and remove this vehicle's doors.")]
        public const string DoorsDescription = "vehicleoptions.doors.desc";

        [English("Vehicle Doors")]
        public const string DoorsSubtitle = "vehicleoptions.doors.subtitle";

        [English("Windows")]
        public const string Windows = "vehicleoptions.windows";

        [English("Roll the windows up and down, or smash them.")]
        public const string WindowsDescription = "vehicleoptions.windows.desc";

        [English("Vehicle Windows")]
        public const string WindowsSubtitle = "vehicleoptions.windows.subtitle";

        #endregion

        #region Modifications

        [English("~b~[{position}]~s~ Choose a ~y~{slot}~s~ upgrade. It is fitted straight away. Mod slot {number}.")]
        public const string ModSlotDescription = "vehicleoptions.mods.slot.desc";

        /// <summary>Shown instead when the slot's name is vMenu's guess rather than the game's.</summary>
        [English(
            "~b~[{position}]~s~ Choose an upgrade for mod slot {number}. It is fitted straight away.\nThe ~y~*~s~ means vMenu named this slot itself, so it may not match what really changes.")]
        public const string ModSlotGuessedDescription = "vehicleoptions.mods.slot.guessed.desc";

        [English("Stock {slot}")]
        public const string ModStock = "vehicleoptions.mods.stock";

        [English("{slot} #{number}")]
        public const string ModNumbered = "vehicleoptions.mods.numbered";

        /// <summary>Used when the game has no name for a slot, so vMenu's guess is not passed off as fact.</summary>
        [English("*{name}")]
        public const string ModSlotUnnamed = "vehicleoptions.mods.slot.unnamed";

        [English("Turbo")]
        public const string Turbo = "vehicleoptions.mods.turbo";

        [English("Fit a turbo charger. Mod slot {number}.")]
        public const string TurboDescription = "vehicleoptions.mods.turbo.desc";

        [English("Tyre Smoke")]
        public const string TyreSmoke = "vehicleoptions.mods.tyresmoke";

        [English("Fit coloured tyre smoke, which shows when you spin the wheels. Mod slot {number}.")]
        public const string TyreSmokeDescription = "vehicleoptions.mods.tyresmoke.desc";

        [English("Tyre Smoke Colour")]
        public const string TyreSmokeColor = "vehicleoptions.mods.tyresmoke.color";

        [English("What colour the tyre smoke is. Fit tyre smoke first, or you will not see it.")]
        public const string TyreSmokeColorDescription = "vehicleoptions.mods.tyresmoke.color.desc";

        [English("Red")]
        public const string SmokeRed = "vehicleoptions.smoke.red";

        [English("Orange")]
        public const string SmokeOrange = "vehicleoptions.smoke.orange";

        [English("Yellow")]
        public const string SmokeYellow = "vehicleoptions.smoke.yellow";

        [English("Gold")]
        public const string SmokeGold = "vehicleoptions.smoke.gold";

        [English("Light Green")]
        public const string SmokeLightGreen = "vehicleoptions.smoke.lightgreen";

        [English("Dark Green")]
        public const string SmokeDarkGreen = "vehicleoptions.smoke.darkgreen";

        [English("Light Blue")]
        public const string SmokeLightBlue = "vehicleoptions.smoke.lightblue";

        [English("Dark Blue")]
        public const string SmokeDarkBlue = "vehicleoptions.smoke.darkblue";

        [English("Purple")]
        public const string SmokePurple = "vehicleoptions.smoke.purple";

        [English("Pink")]
        public const string SmokePink = "vehicleoptions.smoke.pink";

        [English("Black")]
        public const string SmokeBlack = "vehicleoptions.smoke.black";

        [English("Window Tint")]
        public const string WindowTint = "vehicleoptions.mods.tint";

        [English("How dark the windows are.")]
        public const string WindowTintDescription = "vehicleoptions.mods.tint.desc";

        [English("Stock")]
        public const string TintStock = "vehicleoptions.mods.tint.stock";

        [English("None")]
        public const string TintNone = "vehicleoptions.mods.tint.none";

        [English("Limo")]
        public const string TintLimo = "vehicleoptions.mods.tint.limo";

        [English("Light Smoke")]
        public const string TintLightSmoke = "vehicleoptions.mods.tint.lightsmoke";

        [English("Dark Smoke")]
        public const string TintDarkSmoke = "vehicleoptions.mods.tint.darksmoke";

        [English("Pure Black")]
        public const string TintPureBlack = "vehicleoptions.mods.tint.pureblack";

        [English("Green")]
        public const string TintGreen = "vehicleoptions.mods.tint.green";

        #endregion

        #region Wheels and tyres

        [English("Wheel Type")]
        public const string WheelType = "vehicleoptions.wheels.type";

        [English("Which family of rims this vehicle picks from. Changing it puts the rims back to stock.")]
        public const string WheelTypeDescription = "vehicleoptions.wheels.type.desc";

        [English("Custom Tyres")]
        public const string CustomTyres = "vehicleoptions.wheels.customtyres";

        [English("Fit the low profile tyres that come with these rims.")]
        public const string CustomTyresDescription = "vehicleoptions.wheels.customtyres.desc";

        [English("Bulletproof Tyres")]
        public const string BulletproofTyres = "vehicleoptions.wheels.bulletproof";

        [English("Stops these tyres bursting when they are shot or scraped. Applies to this vehicle only.")]
        public const string BulletproofTyresDescription = "vehicleoptions.wheels.bulletproof.desc";

        [English(
            "Vehicle god mode is holding the tyres bulletproof on every vehicle you drive, so this cannot be changed here. Turn off Bulletproof Tyres in the god mode options to take it back.")]
        public const string BulletproofTyresLocked = "vehicleoptions.wheels.bulletproof.locked";

        [English("Drift Tyres")]
        public const string DriftTyres = "vehicleoptions.wheels.drift";

        [English("Low grip tyres that make the back end step out. Only some vehicles are built to take them.")]
        public const string DriftTyresDescription = "vehicleoptions.wheels.drift.desc";

        [English("This vehicle cannot take ~y~drift tyres~s~.")]
        public const string DriftTyresUnsupported = "vehicleoptions.wheels.drift.unsupported";

        [English("Sports")]
        public const string WheelTypeSports = "vehicleoptions.wheels.type.sports";

        [English("Muscle")]
        public const string WheelTypeMuscle = "vehicleoptions.wheels.type.muscle";

        [English("Lowrider")]
        public const string WheelTypeLowrider = "vehicleoptions.wheels.type.lowrider";

        [English("SUV")]
        public const string WheelTypeSuv = "vehicleoptions.wheels.type.suv";

        [English("Off-Road")]
        public const string WheelTypeOffroad = "vehicleoptions.wheels.type.offroad";

        [English("Tuner")]
        public const string WheelTypeTuner = "vehicleoptions.wheels.type.tuner";

        [English("Bike Wheels")]
        public const string WheelTypeBike = "vehicleoptions.wheels.type.bike";

        [English("High End")]
        public const string WheelTypeHighEnd = "vehicleoptions.wheels.type.highend";

        [English("Benny's Originals")]
        public const string WheelTypeBennysOriginal = "vehicleoptions.wheels.type.bennys1";

        [English("Benny's Bespoke")]
        public const string WheelTypeBennysBespoke = "vehicleoptions.wheels.type.bennys2";

        [English("Open Wheel")]
        public const string WheelTypeOpenWheel = "vehicleoptions.wheels.type.openwheel";

        [English("Street")]
        public const string WheelTypeStreet = "vehicleoptions.wheels.type.street";

        [English("Track")]
        public const string WheelTypeTrack = "vehicleoptions.wheels.type.track";

        #endregion

        #region Colours

        [English("Primary Colour")]
        public const string PrimaryColor = "vehicleoptions.colors.primary";

        [English("The main body colour.")]
        public const string PrimaryColorDescription = "vehicleoptions.colors.primary.desc";

        [English("Secondary Colour")]
        public const string SecondaryColor = "vehicleoptions.colors.secondary";

        [English("The second body colour, used for stripes and trim on vehicles that have them.")]
        public const string SecondaryColorDescription = "vehicleoptions.colors.secondary.desc";

        [English("The shimmer that shows through the primary colour at an angle.")]
        public const string PearlescentColorDescription = "vehicleoptions.colors.pearlescent.desc";

        [English("Wheel Colour")]
        public const string WheelColor = "vehicleoptions.colors.wheel";

        [English("What colour the rims are painted.")]
        public const string WheelColorDescription = "vehicleoptions.colors.wheel.desc";

        [English("Dashboard Colour")]
        public const string DashboardColor = "vehicleoptions.colors.dashboard";

        [English("The dials and the dashboard itself.")]
        public const string DashboardColorDescription = "vehicleoptions.colors.dashboard.desc";

        [English("Interior Colour")]
        public const string InteriorColor = "vehicleoptions.colors.interior";

        [English("The trim inside the cabin.")]
        public const string InteriorColorDescription = "vehicleoptions.colors.interior.desc";

        [English("Metallic")]
        public const string FinishMetallic = "vehicleoptions.colors.finish.metallic";

        [English("Pearlescent")]
        public const string FinishPearlescent = "vehicleoptions.colors.finish.pearl";

        [English("Chrome")]
        public const string FinishChrome = "vehicleoptions.colors.finish.chrome";

        [English("Chameleon")]
        public const string FinishChameleon = "vehicleoptions.colors.finish.chameleon";

        [English("Classic")]
        public const string ColorGroupClassic = "vehicleoptions.colors.group.classic";

        [English("Matte")]
        public const string ColorGroupMatte = "vehicleoptions.colors.group.matte";

        [English("Metals")]
        public const string ColorGroupMetal = "vehicleoptions.colors.group.metal";

        [English("Utility")]
        public const string ColorGroupUtility = "vehicleoptions.colors.group.utility";

        [English("Worn")]
        public const string ColorGroupWorn = "vehicleoptions.colors.group.worn";

        [English("Default Alloy")]
        public const string ColorDefaultAlloy = "vehicleoptions.colors.defaultalloy";

        [English("Factory Colour Combination")]
        public const string PresetCombination = "vehicleoptions.colors.preset";

        [English("The colour pairs this vehicle actually ships with. Picking one replaces both body colours.")]
        public const string PresetCombinationDescription = "vehicleoptions.colors.preset.desc";

        [English("Combination {number}")]
        public const string PresetCombinationValue = "vehicleoptions.colors.preset.value";

        [English("Paint Fade")]
        public const string PaintFade = "vehicleoptions.colors.fade";

        [English("How sun bleached the paint looks, from factory fresh on the left to badly faded on the right.")]
        public const string PaintFadeDescription = "vehicleoptions.colors.fade.desc";

        #endregion

        #region Custom colour picker

        [English("Custom Colour")]
        public const string CustomColor = "vehicleoptions.rgb";

        [English("Mix your own colour instead of picking one from the game's list.")]
        public const string CustomColorDescription = "vehicleoptions.rgb.desc";

        [English("Hex Colour Code")]
        public const string HexColor = "vehicleoptions.rgb.hex";

        [English("Type a six character hex code, the same kind websites use, such as FF8800.")]
        public const string HexColorDescription = "vehicleoptions.rgb.hex.desc";

        [English("Enter a hex colour code")]
        public const string HexColorPrompt = "vehicleoptions.rgb.hex.prompt";

        [English("~r~{code}~s~ is not a hex colour code. It has to be six characters, using 0 to 9 and A to F.")]
        public const string HexColorInvalid = "vehicleoptions.rgb.hex.invalid";

        [English("Red")]
        public const string ChannelRed = "vehicleoptions.rgb.red";

        [English("Green")]
        public const string ChannelGreen = "vehicleoptions.rgb.green";

        [English("Blue")]
        public const string ChannelBlue = "vehicleoptions.rgb.blue";

        [English("Left and right to change this channel. The colour is applied as you move.")]
        public const string ChannelDescription = "vehicleoptions.rgb.channel.desc";

        [English("Back To A Game Colour")]
        public const string ResetCustomColor = "vehicleoptions.rgb.reset";

        [English("Drops the custom colour so the colour lists take over again.")]
        public const string ResetCustomColorDescription = "vehicleoptions.rgb.reset.desc";

        #endregion

        #region Liveries

        [English("Livery")]
        public const string Livery = "vehicleoptions.liveries.livery";

        [English("The paint job printed onto the bodywork.")]
        public const string LiveryDescription = "vehicleoptions.liveries.livery.desc";

        [English("Roof Livery")]
        public const string RoofLivery = "vehicleoptions.liveries.roof";

        [English("A second paint job that only a few vehicles have, on the roof.")]
        public const string RoofLiveryDescription = "vehicleoptions.liveries.roof.desc";

        [English("None")]
        public const string LiveryNone = "vehicleoptions.liveries.none";

        #endregion

        #region Extras

        [English("Extra #{number}")]
        public const string ExtraName = "vehicleoptions.extras.name";

        [English(
            "Switch extra ~y~{number}~s~ on or off. The game does not name its extras, so try it and see what changes.")]
        public const string ExtraDescription = "vehicleoptions.extras.name.desc";

        #endregion

        #region Neon

        [English("Front Light")]
        public const string NeonFront = "vehicleoptions.neon.front";

        [English("Rear Light")]
        public const string NeonRear = "vehicleoptions.neon.rear";

        [English("Left Light")]
        public const string NeonLeft = "vehicleoptions.neon.left";

        [English("Right Light")]
        public const string NeonRight = "vehicleoptions.neon.right";

        [English("Switch this neon tube on or off.")]
        public const string NeonSideDescription = "vehicleoptions.neon.side.desc";

        [English("Neon Colour")]
        public const string NeonColor = "vehicleoptions.neon.color";

        [English("What colour every neon tube on this vehicle glows.")]
        public const string NeonColorDescription = "vehicleoptions.neon.color.desc";

        #endregion

        #region Lights

        [English("Xenon Headlights")]
        public const string XenonLights = "vehicleoptions.lights.xenon";

        [English("Fit brighter headlights, which is what lets you colour them. Mod slot {number}.")]
        public const string XenonLightsDescription = "vehicleoptions.lights.xenon.desc";

        [English("Headlight Colour")]
        public const string HeadlightColor = "vehicleoptions.lights.color";

        [English("What colour the headlights shine. Fit xenon headlights first, or nothing changes.")]
        public const string HeadlightColorDescription = "vehicleoptions.lights.color.desc";

        [English("Default Xenon")]
        public const string HeadlightDefault = "vehicleoptions.lights.color.default";

        #endregion

        #region Plate

        [English("Plate Text")]
        public const string PlateText = "vehicleoptions.plate.text";

        [English("What is written on the licence plate. Eight characters at most.")]
        public const string PlateTextDescription = "vehicleoptions.plate.text.desc";

        [English("Enter the plate text")]
        public const string PlateTextPrompt = "vehicleoptions.plate.text.prompt";

        [English("Plate Style")]
        public const string PlateStyle = "vehicleoptions.plate.style";

        [English("Which state's plate design this vehicle carries.")]
        public const string PlateStyleDescription = "vehicleoptions.plate.style.desc";

        #endregion

        #region Doors and windows

        [English("Front Left Door")]
        public const string DoorFrontLeft = "vehicleoptions.doors.frontleft";

        [English("Front Right Door")]
        public const string DoorFrontRight = "vehicleoptions.doors.frontright";

        [English("Rear Left Door")]
        public const string DoorRearLeft = "vehicleoptions.doors.rearleft";

        [English("Rear Right Door")]
        public const string DoorRearRight = "vehicleoptions.doors.rearright";

        [English("Hood")]
        public const string DoorHood = "vehicleoptions.doors.hood";

        [English("Trunk")]
        public const string DoorTrunk = "vehicleoptions.doors.trunk";

        [English("Extra Door 1")]
        public const string DoorExtraLeft = "vehicleoptions.doors.extraleft";

        [English("Extra Door 2")]
        public const string DoorExtraRight = "vehicleoptions.doors.extraright";

        [English("Open or close this door.")]
        public const string DoorDescription = "vehicleoptions.doors.item.desc";

        [English("Open All Doors")]
        public const string OpenAllDoors = "vehicleoptions.doors.openall";

        [English("Opens every door this vehicle has, including the hood and trunk.")]
        public const string OpenAllDoorsDescription = "vehicleoptions.doors.openall.desc";

        [English("Close All Doors")]
        public const string CloseAllDoors = "vehicleoptions.doors.closeall";

        [English("Shuts every door this vehicle has.")]
        public const string CloseAllDoorsDescription = "vehicleoptions.doors.closeall.desc";

        [English("Toggle vehicle doors")]
        public const string ToggleDoorsButton = "vehicleoptions.doors.toggleall.button";

        [English("Open or close every vehicle door")]
        public const string ToggleDoorsBinding = "vehicleoptions.toggledoors.binding";

        [English("Remove Door")]
        public const string RemoveDoor = "vehicleoptions.doors.remove";

        [English("Rips the chosen door off the vehicle. Repairing the vehicle puts it back.")]
        public const string RemoveDoorDescription = "vehicleoptions.doors.remove.desc";

        [English("~r~Rip {name} off this vehicle? Press enter again to confirm.")]
        public const string RemoveDoorConfirm = "vehicleoptions.doors.remove.confirm";

        [English("Front Left Window")]
        public const string WindowFrontLeft = "vehicleoptions.windows.frontleft";

        [English("Front Right Window")]
        public const string WindowFrontRight = "vehicleoptions.windows.frontright";

        [English("Rear Left Window")]
        public const string WindowRearLeft = "vehicleoptions.windows.rearleft";

        [English("Rear Right Window")]
        public const string WindowRearRight = "vehicleoptions.windows.rearright";

        [English("Pick whether this window goes up or down, then press enter.")]
        public const string WindowDescription = "vehicleoptions.windows.item.desc";

        [English("Roll Up")]
        public const string WindowRollUp = "vehicleoptions.windows.up";

        [English("Roll Down")]
        public const string WindowRollDown = "vehicleoptions.windows.down";

        [English("Roll Down All Windows")]
        public const string RollDownAllWindows = "vehicleoptions.windows.downall";

        [English("Winds every window all the way down.")]
        public const string RollDownAllWindowsDescription = "vehicleoptions.windows.downall.desc";

        [English("Roll Up All Windows")]
        public const string RollUpAllWindows = "vehicleoptions.windows.upall";

        [English("Winds every window all the way up.")]
        public const string RollUpAllWindowsDescription = "vehicleoptions.windows.upall.desc";

        [English("Smash All Windows")]
        public const string SmashWindows = "vehicleoptions.windows.smash";

        [English("Breaks every window. Repairing the vehicle puts them back.")]
        public const string SmashWindowsDescription = "vehicleoptions.windows.smash.desc";

        [English("~r~Smash every window on this vehicle? Press enter again to confirm.")]
        public const string SmashWindowsConfirm = "vehicleoptions.windows.smash.confirm";

        #endregion

        #region Dirt

        [English("Dirt Level")]
        public const string DirtLevel = "vehicleoptions.dirt.level";

        [English("How dirty the vehicle looks, from spotless on the left to filthy on the right.")]
        public const string DirtLevelDescription = "vehicleoptions.dirt.level.desc";

        [English("Spotless")]
        public const string DirtClean = "vehicleoptions.dirt.clean";

        [English("Level {number}")]
        public const string DirtValue = "vehicleoptions.dirt.value";

        [English("Keep Vehicle Clean")]
        public const string KeepClean = "vehicleoptions.keepclean";

        [English(
            "Washes dust and dirt off again as soon as it appears. It does not clear mud, snow or the marks a crash leaves behind, repair the vehicle for those.")]
        public const string KeepCleanDescription = "vehicleoptions.keepclean.desc";

        #endregion

        #region God mode

        [English("Vehicle God Mode")]
        public const string GodMode = "vehicleoptions.god";

        [English("Stops your vehicle taking damage. Which kinds of damage it stops is up to the options below.")]
        public const string GodModeDescription = "vehicleoptions.god.desc";

        [English("God Mode Options")]
        public const string GodModeOptions = "vehicleoptions.god.options";

        [English("Choose which kinds of damage god mode turns off.")]
        public const string GodModeOptionsDescription = "vehicleoptions.god.options.desc";

        [English("Vehicle God Mode Options")]
        public const string GodModeSubtitle = "vehicleoptions.god.subtitle";

        [English("Invincible")]
        public const string GodInvincible = "vehicleoptions.god.invincible";

        [English(
            "Bullets, fire, explosions, collisions and melee all stop hurting the vehicle, and its doors stop coming off.")]
        public const string GodInvincibleDescription = "vehicleoptions.god.invincible.desc";

        [English("Protect Engine")]
        public const string GodEngine = "vehicleoptions.god.engine";

        [English("The engine stops wearing out, and any damage it has already taken is undone.")]
        public const string GodEngineDescription = "vehicleoptions.god.engine.desc";

        [English("Prevent Visual Damage")]
        public const string GodVisual = "vehicleoptions.god.visual";

        [English(
            "Keeps scratches and other damage marks off the paint. It does not stop the bodywork being bent out of shape.")]
        public const string GodVisualDescription = "vehicleoptions.god.visual.desc";

        [English("Strong Wheels")]
        public const string GodStrongWheels = "vehicleoptions.god.strongwheels";

        [English("Wheels stop bending out of shape and ruining the handling. It does not make the tyres bulletproof.")]
        public const string GodStrongWheelsDescription = "vehicleoptions.god.strongwheels.desc";

        [English("Bulletproof Tyres")]
        public const string GodBulletproofTyres = "vehicleoptions.god.bulletprooftyres";

        [English(
            "Tyres stop bursting when they are shot or scraped, on every vehicle you drive. Whatever the vehicle had before you got in is put back when you leave it.")]
        public const string GodBulletproofTyresDescription = "vehicleoptions.god.bulletprooftyres.desc";

        [English("Prevent Ramp Damage")]
        public const string GodRamp = "vehicleoptions.god.ramp";

        [English("Stops vehicles such as the Ramp Buggy taking damage from their own ramp.")]
        public const string GodRampDescription = "vehicleoptions.god.ramp.desc";

        [English("~r~Auto Repair")]
        public const string GodAutoRepair = "vehicleoptions.god.autorepair";

        [English(
            "Repairs the vehicle the moment it takes any damage at all. Best left off, it fixes the car out from under you.")]
        public const string GodAutoRepairDescription = "vehicleoptions.god.autorepair.desc";

        #endregion

        #region Engine

        [English("Engine")]
        public const string Engine = "vehicleoptions.engine";

        [English(
            "Start and stop the engine, keep it running after you get out, and turn the power up well past anything the factory intended.")]
        public const string EngineDescription = "vehicleoptions.engine.desc";

        [English("Engine Options")]
        public const string EngineSubtitle = "vehicleoptions.engine.subtitle";

        [English("You need to be in a vehicle to do that.")]
        public const string EngineNoVehicle = "vehicleoptions.engine.novehicle";

        [English("You have to be the driver to touch the engine.")]
        public const string EngineNotDriver = "vehicleoptions.engine.notdriver";

        [English("Engine Always On")]
        public const string EngineAlwaysOn = "vehicleoptions.engine.alwayson";

        [English("Leaves the engine running when you get out, instead of the game switching it off behind you.")]
        public const string EngineAlwaysOnDescription = "vehicleoptions.engine.alwayson.desc";

        [English("Toggle Engine On/Off")]
        public const string ToggleEngine = "vehicleoptions.engine.toggle";

        [English(
            "Starts the engine if it is off, stops it if it is running. Also on a key, ~b~Right Ctrl~s~ by default, which you can change in the pause menu under ~y~Settings~s~, ~y~Key Bindings~s~, ~y~FiveM~s~.")]
        public const string ToggleEngineDescription = "vehicleoptions.engine.toggle.desc";

        [English("Engine ~g~started~s~.")]
        public const string EngineStarted = "vehicleoptions.engine.started";

        [English("Engine ~r~stopped~s~.")]
        public const string EngineStopped = "vehicleoptions.engine.stopped";

        [English("x{amount}")]
        public const string MultiplierOption = "vehicleoptions.multiplier.option";

        [English("Enable Power Multiplier")]
        public const string PowerMultiplierEnabled = "vehicleoptions.power.enabled";

        [English(
            "Switches on the power multiplier picked below. Power is how hard the engine pulls once you are already moving.")]
        public const string PowerMultiplierEnabledDescription = "vehicleoptions.power.enabled.desc";

        [English("Engine Power Multiplier")]
        public const string PowerMultiplier = "vehicleoptions.power";

        [English(
            "How many times the normal engine power your vehicle gets. Anything past ~y~x8~s~ is more of a toy than a car.")]
        public const string PowerMultiplierDescription = "vehicleoptions.power.desc";

        [English("Switch on ~b~Enable Power Multiplier~s~ above first.")]
        public const string PowerMultiplierLocked = "vehicleoptions.power.locked";

        [English("Enable Torque Multiplier")]
        public const string TorqueMultiplierEnabled = "vehicleoptions.torque.enabled";

        [English("Switches on the torque multiplier picked below. Torque is the shove you get away from a standstill.")]
        public const string TorqueMultiplierEnabledDescription = "vehicleoptions.torque.enabled.desc";

        [English("Engine Torque Multiplier")]
        public const string TorqueMultiplier = "vehicleoptions.torque";

        [English(
            "How many times the normal engine torque your vehicle gets. High values will happily flip a car onto its roof.")]
        public const string TorqueMultiplierDescription = "vehicleoptions.torque.desc";

        [English("Switch on ~b~Enable Torque Multiplier~s~ above first.")]
        public const string TorqueMultiplierLocked = "vehicleoptions.torque.locked";

        #endregion

        #region Handling

        [English("Handling")]
        public const string Handling = "vehicleoptions.handling";

        [English(
            "Cap your speed, freeze the vehicle where it stands, flip it back onto its wheels, hide it, or set the alarm off.")]
        public const string HandlingDescription = "vehicleoptions.handling.desc";

        [English("Handling Options")]
        public const string HandlingSubtitle = "vehicleoptions.handling.subtitle";

        [English("Speed Limiter")]
        public const string SpeedLimiter = "vehicleoptions.speedlimiter";

        [English(
            "~b~Set~s~ caps the vehicle at the speed you are doing right now, ~b~Reset~s~ takes the cap off again, and ~b~Custom~s~ lets you type a number. Only the vehicle you are in is affected.")]
        public const string SpeedLimiterDescription = "vehicleoptions.speedlimiter.desc";

        [English("Set To Current Speed")]
        public const string SpeedLimiterSet = "vehicleoptions.speedlimiter.set";

        [English("Reset")]
        public const string SpeedLimiterReset = "vehicleoptions.speedlimiter.reset";

        [English("Custom Speed")]
        public const string SpeedLimiterCustom = "vehicleoptions.speedlimiter.custom";

        [English("Enter a top speed")]
        public const string SpeedLimiterPrompt = "vehicleoptions.speedlimiter.prompt";

        [English("That is not a speed. Type a number greater than zero.")]
        public const string SpeedLimiterBadNumber = "vehicleoptions.speedlimiter.badnumber";

        [English("Top speed is now ~b~{speed} km/h~s~.")]
        public const string SpeedLimiterSetKmh = "vehicleoptions.speedlimiter.setkmh";

        [English("Top speed is now ~b~{speed} mph~s~.")]
        public const string SpeedLimiterSetMph = "vehicleoptions.speedlimiter.setmph";

        [English("Your top speed is no longer capped.")]
        public const string SpeedLimiterCleared = "vehicleoptions.speedlimiter.cleared";

        [English("You are not moving. Get up to the speed you want to be capped at first, or use ~b~Custom Speed~s~.")]
        public const string SpeedLimiterStandstill = "vehicleoptions.speedlimiter.standstill";

        [English("You need to be in a vehicle to limit its speed.")]
        public const string SpeedLimiterNoVehicle = "vehicleoptions.speedlimiter.novehicle";

        [English("You have to be the driver to limit the speed.")]
        public const string SpeedLimiterNotDriver = "vehicleoptions.speedlimiter.notdriver";

        [English("Freeze Vehicle")]
        public const string Freeze = "vehicleoptions.freeze";

        [English(
            "Pins the vehicle exactly where it is, in mid air if that is where it happens to be. It thaws out on its own when you get out.")]
        public const string FreezeDescription = "vehicleoptions.freeze.desc";

        [English("Flip Vehicle")]
        public const string Flip = "vehicleoptions.flip";

        [English("Drops the vehicle back onto all four wheels, the right way up.")]
        public const string FlipDescription = "vehicleoptions.flip.desc";

        [English("Vehicle set back on its wheels.")]
        public const string Flipped = "vehicleoptions.flip.done";

        [English("You need to be in a vehicle to flip it.")]
        public const string FlipNoVehicle = "vehicleoptions.flip.novehicle";

        [English("You have to be the driver to flip the vehicle.")]
        public const string FlipNotDriver = "vehicleoptions.flip.notdriver";

        [English("Toggle Vehicle Visibility")]
        public const string Visibility = "vehicleoptions.visibility";

        [English(
            "Makes the vehicle invisible. You go invisible along with it, because the game hides everyone sitting inside. Both of you turn visible again the moment you get out, otherwise you would never find the vehicle to get back in.")]
        public const string VisibilityDescription = "vehicleoptions.visibility.desc";

        [English("Vehicle is now ~y~invisible~s~.")]
        public const string VisibilityHidden = "vehicleoptions.visibility.hidden";

        [English("Vehicle is ~g~visible~s~ again.")]
        public const string VisibilityShown = "vehicleoptions.visibility.shown";

        [English("You need to be in a vehicle to hide it.")]
        public const string VisibilityNoVehicle = "vehicleoptions.visibility.novehicle";

        [English("You have to be the driver to hide the vehicle.")]
        public const string VisibilityNotDriver = "vehicleoptions.visibility.notdriver";

        [English("Toggle Vehicle Alarm")]
        public const string Alarm = "vehicleoptions.alarm";

        [English("Sets the alarm off, or shuts it up if it is already going. It stops on its own after a while.")]
        public const string AlarmDescription = "vehicleoptions.alarm.desc";

        [English("Alarm going off.")]
        public const string AlarmStarted = "vehicleoptions.alarm.started";

        [English("Alarm silenced.")]
        public const string AlarmStopped = "vehicleoptions.alarm.stopped";

        [English("You need to be in a vehicle to set its alarm off.")]
        public const string AlarmNoVehicle = "vehicleoptions.alarm.novehicle";

        [English("You have to be the driver to touch the alarm.")]
        public const string AlarmNotDriver = "vehicleoptions.alarm.notdriver";

        #endregion

        #region Damage

        [English("Damage")]
        public const string Damage = "vehicleoptions.damage";

        [English("Wreck the engine, nurse it back, and burst or fix the tyres.")]
        public const string DamageDescription = "vehicleoptions.damage.desc";

        [English("Damage Options")]
        public const string DamageSubtitle = "vehicleoptions.damage.subtitle";

        [English("Engine Health")]
        public const string EngineHealth = "vehicleoptions.enginehealth";

        [English(
            "Sets how healthy the engine is, from dead on the left to perfect on the right. Below about a third it starts smoking, and near zero it catches fire.")]
        public const string EngineHealthDescription = "vehicleoptions.enginehealth.desc";

        [English("Destroy Engine")]
        public const string DestroyEngine = "vehicleoptions.destroyengine";

        [English("Kills the engine outright. Repairing the vehicle brings it back.")]
        public const string DestroyEngineDescription = "vehicleoptions.destroyengine.desc";

        [English("~r~This will wreck your engine. Press enter again to confirm.")]
        public const string DestroyEngineConfirm = "vehicleoptions.destroyengine.confirm";

        [English("Engine destroyed.")]
        public const string EngineDestroyed = "vehicleoptions.destroyengine.done";

        [English("Fix / Destroy Tyres")]
        public const string Tyres = "vehicleoptions.tyres";

        [English(
            "Pick a tyre and press enter to burst it, or press enter again to fix it. Only the tyres this vehicle actually has are listed.")]
        public const string TyresDescription = "vehicleoptions.tyres.desc";

        [English("All Tyres")]
        public const string AllTyres = "vehicleoptions.tyres.all";

        [English("Tyre #{number}")]
        public const string TyreNumbered = "vehicleoptions.tyres.numbered";

        [English("Tyre fixed.")]
        public const string TyreFixed = "vehicleoptions.tyres.fixed";

        [English("Tyre burst.")]
        public const string TyreBurst = "vehicleoptions.tyres.burst";

        [English("All tyres fixed.")]
        public const string TyresFixed = "vehicleoptions.tyres.allfixed";

        [English("All tyres burst.")]
        public const string TyresBurst = "vehicleoptions.tyres.allburst";

        #endregion

        #region Aircraft and boats

        [English("Aircraft & Boats")]
        public const string Aircraft = "vehicleoptions.aircraft";

        [English("How rough the air is in helicopters and planes, and whether boats stay put when you stop.")]
        public const string AircraftDescription = "vehicleoptions.aircraft.desc";

        [English("Aircraft & Boat Options")]
        public const string AircraftSubtitle = "vehicleoptions.aircraft.subtitle";

        [English("Helicopter Turbulence")]
        public const string HelicopterTurbulence = "vehicleoptions.turbulence.heli";

        [English(
            "How much helicopters get pushed around by the air, as a percentage. ~b~100~s~ is the amount the game normally uses, ~b~0~s~ makes them fly perfectly steady.")]
        public const string HelicopterTurbulenceDescription = "vehicleoptions.turbulence.heli.desc";

        [English("Plane Turbulence")]
        public const string PlaneTurbulence = "vehicleoptions.turbulence.plane";

        [English(
            "How much planes get pushed around by the air, as a percentage. ~b~100~s~ is the amount the game normally uses, ~b~0~s~ makes them fly perfectly steady.")]
        public const string PlaneTurbulenceDescription = "vehicleoptions.turbulence.plane.desc";

        [English("Anchor Boat")]
        public const string AnchorBoat = "vehicleoptions.anchor";

        [English(
            "Drops the anchor so the boat stops drifting. Only works on a boat, and only where the water is deep enough for the game to allow it.")]
        public const string AnchorBoatDescription = "vehicleoptions.anchor.desc";

        [English(
            "The other pilot is using the bombs and countermeasures right now. You can still look through the bomb camera.")]
        public const string AircraftWeaponsInUse = "vehicleoptions.aircraftweapons.inuse";

        #endregion

        #region Radio

        [English("Radio")]
        public const string Radio = "vehicleoptions.radio";

        [English(
            "Pick the station every vehicle you get into starts on, and take stations you never want to hear off the radio wheel.")]
        public const string RadioDescription = "vehicleoptions.radio.desc";

        [English("Radio Options")]
        public const string RadioSubtitle = "vehicleoptions.radio.subtitle";

        [English("Radio Off")]
        public const string RadioOff = "vehicleoptions.radio.off";

        [English("Enable Default Radio Station")]
        public const string RadioDefaultEnabled = "vehicleoptions.radio.default.enabled";

        [English(
            "Tunes every vehicle you get into to the station picked below, instead of whatever the game felt like playing.")]
        public const string RadioDefaultEnabledDescription = "vehicleoptions.radio.default.enabled.desc";

        [English("Default Radio Station")]
        public const string RadioDefaultStation = "vehicleoptions.radio.default";

        [English("The station every vehicle you get into starts on. Pick ~b~Radio Off~s~ for silence.")]
        public const string RadioDefaultStationDescription = "vehicleoptions.radio.default.desc";

        [English("Switch on ~b~Enable Default Radio Station~s~ above first.")]
        public const string RadioDefaultStationLocked = "vehicleoptions.radio.default.locked";

        [English("Available Stations")]
        public const string RadioBlocked = "vehicleoptions.radio.blocked";

        [English(
            "Choose which stations show up on the radio wheel at all. Your choices are remembered between sessions.")]
        public const string RadioBlockedDescription = "vehicleoptions.radio.blocked.desc";

        [English("Available Radio Stations")]
        public const string RadioBlockedSubtitle = "vehicleoptions.radio.blocked.subtitle";

        [English("Ticked means this station shows on the radio wheel. Unticked takes it off entirely.")]
        public const string RadioStationDescription = "vehicleoptions.radio.station.desc";

        [English("Turn All Stations Back On")]
        public const string RadioUnblockAll = "vehicleoptions.radio.unblockall";

        [English("Puts every station back on the radio wheel, undoing everything unticked below.")]
        public const string RadioUnblockAllDescription = "vehicleoptions.radio.unblockall.desc";

        #endregion

        #region Doors

        [English("Delete Removed Doors")]
        public const string DeleteRemovedDoors = "vehicleoptions.deleteremoveddoors";

        [English(
            "When on, a door you rip off with ~b~Remove Door~s~ vanishes instead of being left lying in the road.")]
        public const string DeleteRemovedDoorsDescription = "vehicleoptions.deleteremoveddoors.desc";

        #endregion
    }
}
