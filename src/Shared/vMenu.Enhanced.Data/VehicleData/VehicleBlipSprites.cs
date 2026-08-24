namespace vMenu.Enhanced.Data.VehicleData;

// Ported from Rockstar's GET_CORRECT_PED_BLIP_SPRITE_FOR_VEHICLE_MODEL, so a player driving a tank
// shows up as a tank exactly as they do in GTA Online. Most vehicles are not in here, and that is
// correct: in GTA Online an ordinary car is still a plain dot, which is what legacy vMenu got wrong
// by falling back to a generic car sprite. Planes, helicopters and boats need natives, so those are
// decided on the client instead.
public static class VehicleBlipSprites
{
    // The plain player dot, which is what anything not listed here stays as.
    public const int StandardSprite = 1;

    public const int PlaneSprite = 423;

    public const int HelicopterSprite = 422;

    public const int BoatSprite = 427;

    public const int PersonalVehicleCarSprite = 225;

    public const int PersonalVehicleBikeSprite = 226;

    // The two sprites the game already turns to face the right way by itself. Setting a rotation on
    // these fights the engine and makes them jitter.
    public const int SubmarineSprite = 760;

    private static readonly Dictionary<uint, int> ByModel = Build();

    public static int? ForModel(uint model) => ByModel.TryGetValue(model, out var sprite) ? sprite : null;

    // Written out here rather than taken from GetHashKey so this whole file stays free of natives and
    // can live in the shared assembly. It is the same "joaat" the game uses, lowercased first.
    public static uint Hash(string model)
    {
        var hash = 0u;

        foreach (var character in model)
        {
            hash += char.ToLowerInvariant(character);
            hash += hash << 10;
            hash ^= hash >> 6;
        }

        hash += hash << 3;
        hash ^= hash >> 11;
        hash += hash << 15;

        return hash;
    }

    // Built once into a static field. Legacy rebuilt the equivalent dictionary and re-hashed every
    // string in it on each call, on every frame, for every player on screen.
    private static Dictionary<uint, int> Build()
    {
        var sprites = new Dictionary<uint, int>();

        void Add(int sprite, params string[] models)
        {
            foreach (var model in models)
            {
                sprites[Hash(model)] = sprite;
            }
        }

        // Fighter jets. Listed rather than asked of the game, because the native Rockstar uses for this is
        // not one FiveM exposes.
        Add(424, "lazer", "besra", "hydra");

        // Vehicles with a gun turret somebody else can sit behind.
        Add(426, "insurgent", "insurgent2", "insurgent3", "technical", "technical3");
        Add(460, "limo2");

        // Anything that goes underwater but is not one of the two named submarines.
        Add(BoatSprite, "submersible", "submersible2");

        Add(421, "rhino");
        Add(512, "blazer5");

        // Import/Export.
        Add(528, "phantom2");
        Add(529, "boxville5");
        Add(530, "ruiner2");
        Add(531, "dune4", "dune5");
        Add(532, "wastelander");
        Add(533, "voltic2");
        Add(534, "technical2");

        // Gunrunning.
        Add(558, "apc");
        Add(559, "oppressor");
        Add(560, "halftrack");
        Add(561, "dune3");
        Add(562, "tampa3");
        Add(563, "trailersmall2");
        Add(564, "trailerlarge");

        // Smuggler's Run.
        Add(572, "alphaz1");
        Add(573, "bombushka");
        Add(574, "havok");
        Add(575, "howard");
        Add(576, "hunter");
        Add(577, "microlight");
        Add(578, "mogul");
        Add(579, "molotok");
        Add(580, "nokota");
        Add(581, "pyro");
        Add(582, "rogue");
        Add(583, "starling");
        Add(584, "seabreeze");
        Add(585, "tula");

        // Doomsday Heist.
        Add(589, "avenger");
        Add(595, "stromberg");
        Add(596, "deluxo");
        Add(597, "thruster");
        Add(598, "khanjali");
        Add(599, "riot2");
        Add(600, "volatol");
        Add(601, "barrage");
        Add(602, "akula");
        Add(603, "chernobog");

        // Super Sport Series.
        Add(612, "seasparrow");
        Add(613, "caracara", "caracara2");

        // After Hours.
        Add(631, "pbus2");
        Add(632, "terbyte");
        Add(633, "menacer");
        Add(634, "scramjet");
        Add(635, "pounder2");
        Add(636, "mule4");
        Add(637, "speedo4");
        Add(639, "oppressor2");

        // 640 rather than 638, which is a blimp. The run really does have a hole in it.
        Add(640, "strikeforce");

        Add(646, "rcbandito");

        // Arena War.
        Add(658, "bruiser", "bruiser2", "bruiser3");
        Add(659, "brutus", "brutus2", "brutus3");
        Add(660, "cerberus", "cerberus2", "cerberus3");
        Add(661, "deathbike", "deathbike2", "deathbike3");
        Add(662, "dominator4", "dominator5", "dominator6");
        Add(663, "impaler2", "impaler3", "impaler4");
        Add(664, "imperator", "imperator2", "imperator3");
        Add(665, "issi4", "issi5", "issi6");
        Add(666, "monster3", "monster4", "monster5");
        Add(667, "scarab", "scarab2", "scarab3");
        Add(668, "slamvan4", "slamvan5", "slamvan6");
        Add(669, "zr380", "zr3802", "zr3803");

        Add(742, "minitank");

        // Los Santos Summer Special, Cayo Perico and later.
        Add(745, "winky");
        Add(746, "avisa");
        Add(747, "veto");
        Add(748, "veto2");
        Add(749, "verus");
        Add(750, "vetir");
        Add(753, "seasparrow2", "seasparrow3");
        Add(754, "dinghy5");
        Add(755, "patrolboat");
        Add(757, "squaddie");
        Add(758, "alkonost");
        Add(759, "annihilator2");
        Add(SubmarineSprite, "kosatka");

        // The two motorbikes with artwork of their own. Every other bike stays a plain dot, which is what
        // GTA Online does and what legacy got wrong by giving all of them the gang bike sprite.
        Add(348, "manchez2", "rrocket");

        // The Criminal Enterprises and beyond.
        Add(818, "patriot3");
        Add(820, "jubilee");
        Add(821, "granger2");
        Add(823, "deity");
        Add(824, "champion");
        Add(825, "buffalo4");

        // Not one of Rockstar's, carried over from legacy vMenu because people liked it. Legacy used sprite
        // 56, which is the police car, so this is the same idea with the right artwork.
        Add(198, "taxi");

        return sprites;
    }
}
