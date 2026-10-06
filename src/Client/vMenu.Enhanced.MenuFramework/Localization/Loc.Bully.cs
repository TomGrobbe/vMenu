using System.Globalization;

namespace vMenu.Enhanced.MenuFramework.Localization;

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

        public const string Title = "bully.title";

        public const string Subtitle = "bully.subtitle";

        public const string LinkDescription = "bully.link.desc";

        public const string PlayerActions = "bully.playeractions";

        public const string PlayerActionsDescription = "bully.playeractions.desc";

        public const string SelfMenu = "bully.self";

        public const string SelfMenuDescription = "bully.self.desc";

        public const string EveryoneMenu = "bully.everyone";

        public const string EveryoneMenuDescription = "bully.everyone.desc";

        public const string IncludeSelf = "bully.everyone.includeself";

        public const string IncludeSelfDescription = "bully.everyone.includeself.desc";

        public const string ServerMenu = "bully.server";

        public const string ServerMenuDescription = "bully.server.desc";

        public const string ActiveMenu = "bully.active";

        public const string ActiveMenuDescription = "bully.active.desc";

        public const string ActiveEmpty = "bully.active.empty";

        public const string ActiveEmptyDescription = "bully.active.empty.desc";

        public const string ActiveRowDescription = "bully.active.row.desc";

        public const string ActiveServerWide = "bully.active.serverwide";

        public const string StopAll = "bully.stopall";

        public const string StopAllDescription = "bully.stopall.desc";

        public const string StopAllConfirm = "bully.stopall.confirm";

        public const string StopAllDone = "bully.stopall.done";

        public const string PranksGroup = "bully.group.pranks";

        public const string ScaresGroup = "bully.group.scares";

        public const string AttackersGroup = "bully.group.attackers";

        public const string VehicleGroup = "bully.group.vehicle";

        public const string ContinuousGroup = "bully.group.continuous";

        public const string Sent = "bully.sent";

        public const string SentEveryone = "bully.sent.everyone";

        public const string ToggleOn = "bully.toggle.on";

        public const string ToggleOff = "bully.toggle.off";

        public const string ServerToggleOn = "bully.servertoggle.on";

        public const string ServerToggleOff = "bully.servertoggle.off";

        public const string Disabled = "bully.disabled";

        public const string NeedsOnFoot = "bully.needsonfoot";

        public const string NeedsDriving = "bully.needsdriving";

        public const string TurnedOff = "bully.turnedoff";

        public const string Busy = "bully.busy";

        public const string NoVehicle = "bully.novehicle";

        public const string DidNotWork = "bully.didnotwork";

        public const string SentUnconfirmed = "bully.sent.unconfirmed";

        public const string Timecycle0 = "bully.timecycle.0";

        public const string Timecycle1 = "bully.timecycle.1";

        public const string Timecycle2 = "bully.timecycle.2";

        public const string Timecycle3 = "bully.timecycle.3";

        public const string Timecycle4 = "bully.timecycle.4";

        public const string Timecycle5 = "bully.timecycle.5";

        public const string Timecycle6 = "bully.timecycle.6";

        public const string Sound0 = "bully.sound.0";

        public const string Sound1 = "bully.sound.1";

        public const string Sound2 = "bully.sound.2";

        public const string Sound3 = "bully.sound.3";

        public const string Sound4 = "bully.sound.4";

        public const string Explode = "bully.effect.explode";

        public const string ExplodeDescription = "bully.effect.explode.desc";

        public const string Atomizer = "bully.effect.atomizer";

        public const string AtomizerDescription = "bully.effect.atomizer.desc";

        public const string Stun = "bully.effect.stun";

        public const string StunDescription = "bully.effect.stun.desc";

        public const string Fire = "bully.effect.fire";

        public const string FireDescription = "bully.effect.fire.desc";

        public const string Ragdoll = "bully.effect.ragdoll";

        public const string RagdollDescription = "bully.effect.ragdoll.desc";

        public const string Jump = "bully.effect.jump";

        public const string JumpDescription = "bully.effect.jump.desc";

        public const string Dance = "bully.effect.dance";

        public const string DanceDescription = "bully.effect.dance.desc";

        public const string Drunk = "bully.effect.drunk";

        public const string DrunkDescription = "bully.effect.drunk.desc";

        public const string Drugged = "bully.effect.drugged";

        public const string DruggedDescription = "bully.effect.drugged.desc";

        public const string Timecycle = "bully.effect.timecycle";

        public const string TimecycleDescription = "bully.effect.timecycle.desc";

        public const string Sound = "bully.effect.sound";

        public const string SoundDescription = "bully.effect.sound.desc";

        public const string Fireworks = "bully.effect.fireworks";

        public const string FireworksDescription = "bully.effect.fireworks.desc";

        public const string Beast = "bully.effect.beast";

        public const string BeastDescription = "bully.effect.beast.desc";

        public const string Carjack = "bully.effect.carjack";

        public const string CarjackDescription = "bully.effect.carjack.desc";

        public const string Mug = "bully.effect.mug";

        public const string MugDescription = "bully.effect.mug.desc";

        public const string Clowns = "bully.effect.clowns";

        public const string ClownsDescription = "bully.effect.clowns.desc";

        public const string Cougar = "bully.effect.cougar";

        public const string CougarDescription = "bully.effect.cougar.desc";

        public const string Clone = "bully.effect.clone";

        public const string CloneDescription = "bully.effect.clone.desc";

        public const string Teleport = "bully.effect.teleport";

        public const string TeleportDescription = "bully.effect.teleport.desc";

        public const string Abduct = "bully.effect.abduct";

        public const string AbductDescription = "bully.effect.abduct.desc";

        public const string Float = "bully.effect.float";

        public const string FloatDescription = "bully.effect.float.desc";

        public const string Transform = "bully.effect.transform";

        public const string TransformDescription = "bully.effect.transform.desc";

        public const string ToggleShockOnEntry = "bully.toggle.shockonentry";

        public const string ToggleShockOnEntryDescription = "bully.toggle.shockonentry.desc";

        public const string ToggleRiot = "bully.toggle.riot";

        public const string ToggleRiotDescription = "bully.toggle.riot.desc";

        public const string ToggleInvertControls = "bully.toggle.invertcontrols";

        public const string ToggleInvertControlsDescription = "bully.toggle.invertcontrols.desc";

        public const string ToggleRandomThrottle = "bully.toggle.randomthrottle";

        public const string ToggleRandomThrottleDescription = "bully.toggle.randomthrottle.desc";

        public const string ToggleLowGrip = "bully.toggle.lowgrip";

        public const string ToggleLowGripDescription = "bully.toggle.lowgrip.desc";

        public const string ToggleLod = "bully.toggle.lod";

        public const string ToggleLodDescription = "bully.toggle.lod.desc";
    }
}
