using vMenu.Enhanced.Data.Configuration;

using BullySettings =vMenu.Enhanced.Data.Configuration.Settings.Bully;

namespace vMenu.Enhanced.Data.Bullying;

public static class BullyEffects
{
    public const string Explode = "Explode";

    public const string Atomizer = "Atomizer";

    public const string Stun = "Stun";

    public const string Fire = "Fire";

    public const string Ragdoll = "Ragdoll";

    public const string Jump = "Jump";

    public const string Dance = "Dance";

    public const string Drunk = "Drunk";

    public const string Drugged = "Drugged";

    public const string Timecycle = "Timecycle";

    public const string Sound = "Sound";

    public const string Fireworks = "Fireworks";

    public const string Beast = "Beast";

    public const string Carjack = "Carjack";

    public const string Mug = "Mug";

    public const string Clowns = "Clowns";

    public const string Cougar = "Cougar";

    public const string Clone = "Clone";

    public const string Teleport = "Teleport";

    public const string Abduct = "Abduct";

    public const string Haircut = "Haircut";

    public const string Float = "Float";

    public const string Transform = "Transform";

    public static IReadOnlyList<BullyEffect> All { get; } =
    [
        new(Explode, "blew up", BullyRequirement.None) { TurnedOffBy = BullySettings.DisableExplosions },
        new(Atomizer, "atomized", BullyRequirement.None) { TurnedOffBy = BullySettings.DisableExplosions },
        new(Stun, "tased", BullyRequirement.None),
        new(Fire, "set fire to", BullyRequirement.None),
        new(Ragdoll, "knocked over", BullyRequirement.None),
        new(Jump, "made jump", BullyRequirement.OnFoot),
        new(Dance, "made dance", BullyRequirement.OnFoot),
        new(Drunk, "intoxicated", BullyRequirement.None),
        new(Drugged, "drugged", BullyRequirement.None),
        new(Timecycle, "changed the screen colours of", BullyRequirement.None),
        new(Sound, "played a jumpscare sound for", BullyRequirement.None) { SpawnsEntities = true },
        new(Fireworks, "set off fireworks around", BullyRequirement.None) { TurnedOffBy = BullySettings.DisableParticleEffects },
        new(Beast, "sent the beast after", BullyRequirement.None) { TurnedOffBy = BullySettings.DisableParticleEffects },
        new(Carjack, "sent carjackers after", BullyRequirement.None) { SpawnsEntities = true },
        new(Mug, "sent a mugger after", BullyRequirement.None) { SpawnsEntities = true, Grouped = true },
        new(Clowns, "sent killer clowns after", BullyRequirement.None) { SpawnsEntities = true, Grouped = true },
        new(Cougar, "sent a mountain lion after", BullyRequirement.None) { SpawnsEntities = true },
        new(Clone, "sent an evil clone after", BullyRequirement.OnFoot),
        new(Teleport, "randomly teleported", BullyRequirement.OnFoot),
        new(Abduct, "had aliens abduct", BullyRequirement.OnFoot) { SpawnsEntities = true },
        new(Haircut, "had Lamar roast the haircut of", BullyRequirement.OnFoot),
        new(Float, "levitated the vehicle of", BullyRequirement.Driving),
        new(Transform, "transformed the vehicle of", BullyRequirement.Driving) { SpawnsEntities = true },
    ];

    public static BullyEffect? Find(string id) => All.FirstOrDefault(effect => effect.Id == id);
}

public enum BullyRequirement
{
    None,
    OnFoot,
    Driving,
}

public sealed class BullyEffect(string id, string verb, BullyRequirement requirement)
{
    public string Id { get; } = id;

    public string Verb { get; } = verb;

    public BullyRequirement Requirement { get; } = requirement;

    public bool SpawnsEntities { get; init; }

    public bool Grouped { get; init; }

    public BoolSetting? TurnedOffBy { get; init; }
}
