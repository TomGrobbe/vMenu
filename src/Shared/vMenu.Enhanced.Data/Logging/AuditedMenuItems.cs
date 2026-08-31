namespace vMenu.Enhanced.Data.Logging;

public static class AuditedMenuItems
{
    private static readonly Dictionary<string, string> Labels = new(StringComparer.Ordinal)
    {
        ["playeroptions.godmode"] = "god mode",
        ["playeroptions.invisible"] = "invisibility",
        ["playeroptions.noragdoll"] = "no ragdoll",
        ["playeroptions.everyoneignores"] = "being ignored by everyone",
        ["playeroptions.superjump"] = "super jump",
        ["playeroptions.fastrun"] = "fast run",
        ["playeroptions.fastswim"] = "fast swim",
        ["playeroptions.unlimitedoxygen"] = "unlimited oxygen",
        ["playeroptions.neverwanted"] = "never wanted",
        ["playeroptions.setwanted"] = "their wanted level",
        ["playeroptions.setarmor"] = "their armour",
        ["playeroptions.healplayer"] = "healed themselves",
        ["playeroptions.suicide"] = "killed themselves",

        ["weaponoptions.unlimitedammo"] = "unlimited ammo",
        ["weaponoptions.noreload"] = "no reload",
        ["weaponoptions.getall"] = "gave themselves every weapon",
        ["weaponoptions.removeall"] = "removed all of their weapons",
        ["weaponoptions.setallammo"] = "set the ammo on every weapon",
        ["weaponoptions.refillall"] = "refilled all of their ammo",
        ["weaponoptions.byname"] = "spawned a weapon by name",

        ["vehicleoptions.god"] = "vehicle god mode",
        ["vehicleoptions.god.invincible"] = "vehicle invincibility",
        ["vehicleoptions.power.enabled"] = "the engine power multiplier",
        ["vehicleoptions.torque.enabled"] = "the engine torque multiplier",
        ["vehicleoptions.power"] = "the engine power multiplier",
        ["vehicleoptions.torque"] = "the engine torque multiplier",
        ["vehicleoptions.repair"] = "repaired their vehicle",
        ["vehicleoptions.delete"] = "deleted their vehicle",
        ["vehicleoptions.visibility"] = "changed their vehicle's visibility",
        ["vehicleoptions.mods.turbo"] = "the turbo",
        ["vehicleoptions.wheels.bulletproof"] = "bulletproof tyres",

        ["personalvehicle.setcurrent"] = "made the vehicle they are in their personal vehicle",
        ["personalvehicle.kick"] = "kicked everybody out of their personal vehicle",
        ["personalvehicle.delete"] = "deleted their personal vehicle",
        ["personalvehicle.forget"] = "forgot their personal vehicle",
        ["personalvehicle.explode"] = "blew up their personal vehicle",
        ["personalvehicle.horn"] = "sounded the horn on their personal vehicle",
        ["personalvehicle.lock"] = "the locks on their personal vehicle",
        ["personalvehicle.engine"] = "the engine of their personal vehicle",
        ["personalvehicle.lights"] = "the lights on their personal vehicle",
    };

    public static bool Includes(string key) => Labels.ContainsKey(key);

    public static string LabelFor(string key) => Labels.TryGetValue(key, out var label) ? label : key;
}
