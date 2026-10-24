using System.Globalization;

namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class Bully
    {
        public static string EffectName(string effect) => "bully.effect." + effect.ToLowerInvariant();

        public static string EffectDescription(string effect) => EffectName(effect) + ".desc";

        public static string ToggleName(string toggle) => "bully.toggle." + toggle.ToLowerInvariant();

        public static string ToggleDescription(string toggle) => ToggleName(toggle) + ".desc";

        public static string TimecycleOption(int index) => "bully.timecycle." + index.ToString(CultureInfo.InvariantCulture);

        public static string SoundOption(int index) => "bully.sound." + index.ToString(CultureInfo.InvariantCulture);

        [English("Bully")]
        public const string Title = "bully.title";

        [English("Mean Tricks")]
        public const string Subtitle = "bully.subtitle";

        [English("Be mean to players, one at a time or all at once, or try it on yourself first to see what it does.")]
        public const string LinkDescription = "bully.link.desc";

        [English("Bully Actions")]
        public const string PlayerActions = "bully.playeractions";

        [English("Do something mean to this player. They are not told who did it.")]
        public const string PlayerActionsDescription = "bully.playeractions.desc";

        [English("Bully Yourself")]
        public const string SelfMenu = "bully.self";

        [English("Try everything on yourself first, so you know exactly what the other player is in for.")]
        public const string SelfMenuDescription = "bully.self.desc";

        [English("Bully Everyone")]
        public const string EveryoneMenu = "bully.everyone";

        [English("Do something mean to every player on the server at the same time.")]
        public const string EveryoneMenuDescription = "bully.everyone.desc";

        [English("Include Yourself")]
        public const string IncludeSelf = "bully.everyone.includeself";

        [English("When ticked, you get it too. Otherwise everybody except you gets it.")]
        public const string IncludeSelfDescription = "bully.everyone.includeself.desc";

        [English("Server Wide Effects")]
        public const string ServerMenu = "bully.server";

        [English(
            "Effects that stay on for every player, including anybody who joins later, until you switch them off again.")]
        public const string ServerMenuDescription = "bully.server.desc";

        [English("Active Effects")]
        public const string ActiveMenu = "bully.active";

        [English("Every effect that is still switched on right now, and who it is on. Untick one to stop it.")]
        public const string ActiveMenuDescription = "bully.active.desc";

        [English("Nothing Active")]
        public const string ActiveEmpty = "bully.active.empty";

        [English("There are no bully effects switched on at the moment.")]
        public const string ActiveEmptyDescription = "bully.active.empty.desc";

        [English("Untick to switch this off for ~b~{player}~s~.")]
        public const string ActiveRowDescription = "bully.active.row.desc";

        [English("Everyone")]
        public const string ActiveServerWide = "bully.active.serverwide";

        [English("Stop Every Bully Effect")]
        public const string StopAll = "bully.stopall";

        [English(
            "Switch off every effect on every player, and end everything that is still running, like clowns or an evil clone.")]
        public const string StopAllDescription = "bully.stopall.desc";

        [English("~r~This stops every bully effect on the whole server. Press enter again to confirm.")]
        public const string StopAllConfirm = "bully.stopall.confirm";

        [English("Every bully effect has been stopped.")]
        public const string StopAllDone = "bully.stopall.done";

        [English("Mean Tricks")]
        public const string PranksGroup = "bully.group.pranks";

        [English("Scares")]
        public const string ScaresGroup = "bully.group.scares";

        [English("Attackers")]
        public const string AttackersGroup = "bully.group.attackers";

        [English("Vehicle")]
        public const string VehicleGroup = "bully.group.vehicle";

        [English("Stays On Until You Switch It Off")]
        public const string ContinuousGroup = "bully.group.continuous";

        [English("Done. ~b~{player}~s~ got it.")]
        public const string Sent = "bully.sent";

        [English("Done. ~b~{count}~s~ player(s) got it.")]
        public const string SentEveryone = "bully.sent.everyone";

        [English("Switched that on for ~b~{player}~s~.")]
        public const string ToggleOn = "bully.toggle.on";

        [English("Switched that off for ~b~{player}~s~.")]
        public const string ToggleOff = "bully.toggle.off";

        [English("Switched that on for everyone.")]
        public const string ServerToggleOn = "bully.servertoggle.on";

        [English("Switched that off for everyone.")]
        public const string ServerToggleOff = "bully.servertoggle.off";

        [English("The bully menu is switched off on this server.")]
        public const string Disabled = "bully.disabled";

        [English("~b~{player}~s~ has to be on foot for this one.")]
        public const string NeedsOnFoot = "bully.needsonfoot";

        [English("~b~{player}~s~ has to be driving a vehicle for this one.")]
        public const string NeedsDriving = "bully.needsdriving";

        [English("The server owner turned this one off.")]
        public const string TurnedOff = "bully.turnedoff";

        [English("~b~{player}~s~ already has this one going.")]
        public const string Busy = "bully.busy";

        [English("~b~{player}~s~ is already drunk or drugged by their own choice.")]
        public const string AlreadyIntoxicated = "bully.alreadyintoxicated";

        [English("~b~{player}~s~ has no vehicle for this one.")]
        public const string NoVehicle = "bully.novehicle";

        [English("That did not work on ~b~{player}~s~.")]
        public const string DidNotWork = "bully.didnotwork";

        [English("Sent to ~b~{player}~s~, but their game did not confirm it.")]
        public const string SentUnconfirmed = "bully.sent.unconfirmed";

        [English("Black And White")]
        public const string Timecycle0 = "bully.timecycle.0";

        [English("Old Camera")]
        public const string Timecycle1 = "bully.timecycle.1";

        [English("Security Camera Fuzz")]
        public const string Timecycle2 = "bully.timecycle.2";

        [English("Trippy Colours")]
        public const string Timecycle3 = "bully.timecycle.3";

        [English("Alien Vision")]
        public const string Timecycle4 = "bully.timecycle.4";

        [English("Wobbly")]
        public const string Timecycle5 = "bully.timecycle.5";

        [English("Red Mist")]
        public const string Timecycle6 = "bully.timecycle.6";

        [English("Broken Camera")]
        public const string Timecycle7 = "bully.timecycle.7";

        [English("Beast Roar")]
        public const string Sound0 = "bully.sound.0";

        [English("Killer Clown")]
        public const string Sound1 = "bully.sound.1";

        [English("Slasher")]
        public const string Sound2 = "bully.sound.2";

        [English("Mirror Slash")]
        public const string Sound3 = "bully.sound.3";

        [English("Mountain Lion")]
        public const string Sound4 = "bully.sound.4";

        [English("Explode")]
        public const string Explode = "bully.effect.explode";

        [English("Sets off a small explosion right where they stand.")]
        public const string ExplodeDescription = "bully.effect.explode.desc";

        [English("Up-n-Atomizer")]
        public const string Atomizer = "bully.effect.atomizer";

        [English("Blasts them into the air like a hit from the Up-n-Atomizer.")]
        public const string AtomizerDescription = "bully.effect.atomizer.desc";

        [English("Tase")]
        public const string Stun = "bully.effect.stun";

        [English("Hits them with a stun gun, so they drop to the floor for a few seconds.")]
        public const string StunDescription = "bully.effect.stun.desc";

        [English("Set On Fire")]
        public const string Fire = "bully.effect.fire";

        [English("Sets them on fire for a few seconds, and then puts the fire out again.")]
        public const string FireDescription = "bully.effect.fire.desc";

        [English("Ragdoll")]
        public const string Ragdoll = "bully.effect.ragdoll";

        [English("Makes them fall over like a sack of potatoes.")]
        public const string RagdollDescription = "bully.effect.ragdoll.desc";

        [English("Jump")]
        public const string Jump = "bully.effect.jump";

        [English("Makes them jump once, whether they wanted to or not. Only works on foot.")]
        public const string JumpDescription = "bully.effect.jump.desc";

        [English("Dance")]
        public const string Dance = "bully.effect.dance";

        [English("Makes them break out in a random dance for a little while. Only works on foot.")]
        public const string DanceDescription = "bully.effect.dance.desc";

        [English("Drunk")]
        public const string Drunk = "bully.effect.drunk";

        [English(
            "Makes them very drunk for a little while. Wobbly walking, a swaying camera, blurry colours and a car that keeps drifting off course.")]
        public const string DrunkDescription = "bully.effect.drunk.desc";

        [English("Drugged")]
        public const string Drugged = "bully.effect.drugged";

        [English("Like drunk, but with a proper trip on top.")]
        public const string DruggedDescription = "bully.effect.drugged.desc";

        [English("Screen Colours")]
        public const string Timecycle = "bully.effect.timecycle";

        [English("Changes how their screen looks for twenty seconds. Pick a look with the arrows.")]
        public const string TimecycleDescription = "bully.effect.timecycle.desc";

        [English("Jumpscare Sound")]
        public const string Sound = "bully.effect.sound";

        [English(
            "Plays a loud scary sound with a flash on their screen. The mountain lion has no flash, it growls from somewhere close by. Pick a sound with the arrows.")]
        public const string SoundDescription = "bully.effect.sound.desc";

        [English("Fireworks")]
        public const string Fireworks = "bully.effect.fireworks";

        [English("Lets off a bunch of fireworks right around their head.")]
        public const string FireworksDescription = "bully.effect.fireworks.desc";

        [English("Chaff")]
        public const string Chaff = "bully.effect.chaff";

        [English("Fires clouds of aircraft chaff all around them, like a jet flying right past. Only they see it.")]
        public const string ChaffDescription = "bully.effect.chaff.desc";

        [English("Beast Scare")]
        public const string Beast = "bully.effect.beast";

        [English(
            "A beast appears in front of them out of a cloud of smoke, roars and breathes fire at them. Only they see it.")]
        public const string BeastDescription = "bully.effect.beast.desc";

        [English("Carjackers")]
        public const string Carjack = "bully.effect.carjack";

        [English("The closest person or animal near their vehicle, or the one they last drove, tries to steal it.")]
        public const string CarjackDescription = "bully.effect.carjack.desc";

        [English("Mugger")]
        public const string Mug = "bully.effect.mug";

        [English("A gang member with a knife runs up, shoves them over and runs off again.")]
        public const string MugDescription = "bully.effect.mug.desc";

        [English("Killer Clowns")]
        public const string Clowns = "bully.effect.clowns";

        [English(
            "Vans full of clowns chase them down, and up to ten clowns jump out to beat them up. They leave after five minutes.")]
        public const string ClownsDescription = "bully.effect.clowns.desc";

        [English("Mountain Lion")]
        public const string Cougar = "bully.effect.cougar";

        [English(
            "A mountain lion sneaks up from somewhere out of sight and attacks them. It runs off again after a minute.")]
        public const string CougarDescription = "bully.effect.cougar.desc";

        [English("Evil Clone")]
        public const string Clone = "bully.effect.clone";

        [English(
            "An evil copy of them shows up somewhere nearby and comes after them with a weapon. Only they see it. Only works on foot.")]
        public const string CloneDescription = "bully.effect.clone.desc";

        [English("Random Teleport")]
        public const string Teleport = "bully.effect.teleport";

        [English(
            "Teleports them somewhere random, and brings them back between twenty and sixty seconds later. Only works on foot.")]
        public const string TeleportDescription = "bully.effect.teleport.desc";

        [English("Alien Abduction")]
        public const string Abduct = "bully.effect.abduct";

        [English("A UFO shows up and beams them up, and they wake up somewhere else. Only works on foot.")]
        public const string AbductDescription = "bully.effect.abduct.desc";

        [English("Lamar's Haircut Roast")]
        public const string Haircut = "bully.effect.haircut";

        [English("Put them into the haircut roast cutscene from GTA V.")]
        public const string HaircutDescription = "bully.effect.haircut.desc";

        [English("Pulled Over On The Bridge")]
        public const string PulledOver = "bully.effect.pulledover";

        [English("Put them into the bridge pull over cutscene from GTA V.")]
        public const string PulledOverDescription = "bully.effect.pulledover.desc";

        [English("Cutscenes")]
        public const string CutscenesGroup = "bully.group.cutscenes";

        [English("Put them into a cutscene from GTA V, playing a part in it as themselves.")]
        public const string CutscenesDescription = "bully.group.cutscenes.desc";

        [English("Floating Vehicle")]
        public const string Float = "bully.effect.float";

        [English(
            "Turns off gravity for their vehicle, so it slowly floats up and then drops back down. They have to be driving.")]
        public const string FloatDescription = "bully.effect.float.desc";

        [English("Transform Vehicle")]
        public const string Transform = "bully.effect.transform";

        [English(
            "Swaps their vehicle for a random one for fifteen seconds, and then gives the old one back with all of its damage and upgrades. They have to be driving.")]
        public const string TransformDescription = "bully.effect.transform.desc";

        [English("Electric Door Handles")]
        public const string ToggleShockOnEntry = "bully.toggle.shockonentry";

        [English("Every vehicle door handle or handlebar they touch gives them an electric shock.")]
        public const string ToggleShockOnEntryDescription = "bully.toggle.shockonentry.desc";

        [English("Gang Attack")]
        public const string ToggleRiot = "bully.toggle.riot";

        [English("Makes everybody around them angry and violent.")]
        public const string ToggleRiotDescription = "bully.toggle.riot.desc";

        [English("Invert Vehicle Controls")]
        public const string ToggleInvertControls = "bully.toggle.invertcontrols";

        [English(
            "Flips every vehicle control while they drive. Left becomes right, gas becomes brake, and even leaning in the air goes the wrong way.")]
        public const string ToggleInvertControlsDescription = "bully.toggle.invertcontrols.desc";

        [English("Possessed Pedals")]
        public const string ToggleRandomThrottle = "bully.toggle.randomthrottle";

        [English("Every now and then their vehicle suddenly speeds up or slams on the brakes by itself.")]
        public const string ToggleRandomThrottleDescription = "bully.toggle.randomthrottle.desc";

        [English("Slippery Tyres")]
        public const string ToggleLowGrip = "bully.toggle.lowgrip";

        [English("Whatever they drive slides around like it is on ice.")]
        public const string ToggleLowGripDescription = "bully.toggle.lowgrip.desc";

        [English("Glitched Body")]
        public const string ToggleLod = "bully.toggle.lod";

        [English("Makes their character look glitched to everybody around them.")]
        public const string ToggleLodDescription = "bully.toggle.lod.desc";
    }
}
