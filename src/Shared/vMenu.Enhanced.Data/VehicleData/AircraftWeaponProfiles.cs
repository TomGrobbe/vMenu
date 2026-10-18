namespace vMenu.Enhanced.Data.VehicleData;

// Ported from Rockstar's freemode bomb bay and countermeasure decompiled scripts, model for model.
public static class AircraftWeaponProfiles
{
    private const string Akula = "akula";

    private const string Alkonost = "alkonost";

    private const string AlphaZ1 = "alphaz1";

    private const string Avenger = "avenger";

    private const string Avenger3 = "avenger3";

    private const string Bombushka = "bombushka";

    private const string Cargobob5 = "cargobob5";

    private const string Cuban800 = "cuban800";

    private const string Duster2 = "duster2";

    private const string Havok = "havok";

    private const string Howard = "howard";

    private const string Hunter = "hunter";

    private const string Microlight = "microlight";

    private const string Mogul = "mogul";

    private const string Molotok = "molotok";

    private const string Nokota = "nokota";

    private const string Oppressor2 = "oppressor2";

    private const string Pyro = "pyro";

    private const string Rogue = "rogue";

    private const string Seabreeze = "seabreeze";

    private const string Seasparrow2 = "seasparrow2";

    private const string Seasparrow3 = "seasparrow3";

    private const string Starling = "starling";

    private const string Streamer216 = "streamer216";

    private const string Strikeforce = "strikeforce";

    private const string Thruster = "thruster";

    private const string Titan2 = "titan2";

    private const string Tula = "tula";

    private const string Volatol = "volatol";

    private static readonly Dictionary<uint, AircraftWeaponProfile> ByModel = Build();

    public static bool Knows(uint model) => ByModel.ContainsKey(model);

    public static AircraftWeaponProfile? ForModel(uint model) =>
        ByModel.TryGetValue(model, out var profile) ? profile.Copy() : null;

    public static AircraftWeaponProfile Generic(bool bombs, float bottom, float tail) => new()
    {
        Bombs = bombs,
        Flares = true,
        Chaff = true,
        Smoke = true,
        CameraZ = bottom,
        SmokeOffset = [0f, tail, 0f],
    };

    private static Dictionary<uint, AircraftWeaponProfile> Build()
    {
        var profiles = new Dictionary<uint, AircraftWeaponProfile>();

        AircraftWeaponProfile At(string model)
        {
            var hash = VehicleBlipSprites.Hash(model);

            if (!profiles.TryGetValue(hash, out var profile))
            {
                profile = new AircraftWeaponProfile
                {
                    CameraZ = 0f,
                    ChaffSide = [0f, 0f, 0f, 0f],
                    ChaffLength = [0f, 0f, 0f, 0f],
                    ChaffHeight = [0f, 0f, 0f, 0f],
                    ChaffSpread = false,
                    ChaffScale = 0f,
                };

                profiles[hash] = profile;
            }

            return profile;
        }

        void Each(Action<AircraftWeaponProfile> apply, params string[] models)
        {
            foreach (var model in models)
            {
                apply(At(model));
            }
        }

        void Bomb(string model, float position, float cameraY, float cameraZ)
        {
            var profile = At(model);

            profile.Bombs = true;
            profile.BombPosition = position;
            profile.CameraY = cameraY;
            profile.CameraZ = cameraZ;
        }

        Bomb(Cuban800, 0.5f, 1.3f, -1.5f);
        Bomb(Mogul, 0.45f, 1.25f, -1.5f);
        Bomb(Rogue, 0.46f, -0.5f, -1.5f);
        Bomb(Starling, 0.55f, 3f, -1.5f);
        Bomb(Seabreeze, 0.5f, 1f, -1.5f);
        Bomb(Tula, 0.6f, 0.5f, -1.5f);
        Bomb(Bombushka, 0.43f, -2f, -1.5f);
        Bomb(Titan2, 0.43f, -2f, -3f);
        Bomb(Hunter, 0.5f, 4f, -1.5f);
        Bomb(Avenger, 0.36f, 0.9f, -3f);
        Bomb(Avenger3, 0.36f, 0.9f, -3f);
        Bomb(Akula, 0.4f, 6.5f, -1.5f);
        Bomb(Volatol, 0.54f, -9f, -2f);
        Bomb(Alkonost, 0.4f, -9f, -2f);
        Bomb(Strikeforce, 0.7f, -1.5f, -1.5f);
        Bomb(Streamer216, 0.4f, 0f, -1.5f);

        string[] countermeasures =
        [
            Mogul, Rogue, Starling, Seabreeze, Tula, Bombushka, Hunter, Nokota, Pyro, Molotok,
            Havok, AlphaZ1, Microlight, Howard, Avenger, Avenger3, Thruster, Volatol, Oppressor2,
            Strikeforce, Alkonost, Seasparrow2, Seasparrow3, Cargobob5, Titan2, Duster2,
        ];

        Each(p => { p.Flares = true; p.Chaff = true; p.Smoke = true; }, countermeasures);
        Each(p => { p.Flares = true; p.Smoke = true; }, Cuban800);
        Each(p => { p.Flares = true; p.Chaff = true; }, Akula);

        Each(p => p.Rules |= AircraftWeaponRules.LowFlying, Volatol);
        Each(p => p.Rules |= AircraftWeaponRules.Vtol, Tula, Avenger, Avenger3);
        Each(p => p.Rules |= AircraftWeaponRules.Oppressor, Oppressor2);

        Each(p => p.FlareTargetY = -10f, Cargobob5, Titan2);

        Each(p => p.FlareStartZ = -0.3f, Cuban800, Mogul, AlphaZ1, Howard, Nokota, Pyro, Seabreeze, Rogue, Starling, Molotok, Duster2);
        Each(p => p.FlareStartZ = 2f, Tula, Bombushka, Avenger, Avenger3);
        Each(p => p.FlareStartZ = 3f, Microlight);
        Each(p => p.FlareStartZ = 0.96f, Cargobob5);
        Each(p => p.FlareStartZ = -0.95f, Titan2);

        Each(p => p.FlareStartY = 3f, Cuban800, Mogul, AlphaZ1, Tula, Bombushka, Howard, Avenger, Avenger3, Volatol);
        Each(p => p.FlareStartY = -0.8f, Nokota, Pyro);
        Each(p => p.FlareStartY = 1f, Seabreeze, Microlight, Havok, Thruster, Alkonost);
        Each(p => p.FlareStartY = 2f, Oppressor2);
        Each(p => p.FlareStartY = 4f, Hunter, Akula);
        Each(p => p.FlareStartY = -7.203f, Cargobob5);
        Each(p => p.FlareStartY = -4.66f, Titan2);

        Each(p => p.FlareStartX = [-1f, -0.5f, 0.5f, 1f], Hunter, Akula);
        Each(p => p.FlareStartX = [1.5f, 0.5f, -0.5f, 1.5f], Havok);
        Each(p => p.FlareStartX = [-4f, -3f, 3f, 4f], Avenger, Avenger3, Volatol);
        Each(p => p.FlareStartX = [1f, 0.5f, -0.5f, -1f], Thruster);
        Each(p => p.FlareStartX = [0f, 0f, 0f, 0f], Oppressor2);
        Each(p => p.FlareStartX = [-1.5f, -1.5f, 1.5f, 1.5f], Cargobob5);

        Each(p => p.ChaffSide = [0.2f, 0.8f, 0f, 1f], Pyro, Rogue, Seabreeze, Tula, Mogul, Starling, Nokota, Molotok, AlphaZ1, Microlight, Howard, Bombushka, Oppressor2, Strikeforce, Cargobob5, Titan2, Duster2);
        Each(p => p.ChaffSide = [0.4f, 0.6f, 0.35f, 0.65f], Hunter, Havok, Akula, Thruster, Seasparrow2, Seasparrow3);
        Each(p => p.ChaffSide = [0.4f, 0.6f, 0.3f, 0.7f], Avenger, Avenger3, Volatol);
        Each(p => p.ChaffSide = [0.2f, 0.8f, 0.1f, 0.9f], Alkonost);

        Each(p => p.ChaffLength = [0.4f, 0.4f, 0.45f, 0.45f], Pyro, Mogul, Starling, Nokota, Molotok, AlphaZ1, Microlight, Howard, Hunter, Havok, Akula, Thruster, Seasparrow2, Seasparrow3, Cargobob5, Titan2);
        Each(p => p.ChaffLength = [0.4f, 0.4f, 0.4f, 0.4f], Rogue, Tula, Avenger, Avenger3, Volatol);
        Each(p => p.ChaffLength = [0.45f, 0.45f, 0.45f, 0.45f], Seabreeze);
        Each(p => p.ChaffLength = [0.35f, 0.35f, 0.35f, 0.35f], Bombushka);
        Each(p => p.ChaffLength = [0.6f, 0.6f, 0.6f, 0.6f], Alkonost);

        Each(p => p.ChaffHeight = [1f, 1f, 1.4f, 1.4f], Pyro, Mogul, Starling, Nokota, Molotok, Howard, Thruster, Cargobob5, Titan2);
        Each(p => p.ChaffHeight = [1.5f, 1.5f, 1.8f, 1.8f], Rogue);
        Each(p => p.ChaffHeight = [1f, 1f, 1f, 1f], Seabreeze);
        Each(p => p.ChaffHeight = [2.4f, 2.4f, 2.6f, 2.6f], Tula);
        Each(p => p.ChaffHeight = [0f, 0f, 0.2f, 0.2f], AlphaZ1, Havok);
        Each(p => p.ChaffHeight = [2f, 2f, 2f, 2f], Microlight, Hunter, Akula, Seasparrow2, Seasparrow3);
        Each(p => p.ChaffHeight = [3f, 3f, 3f, 3f], Bombushka, Volatol);
        Each(p => p.ChaffHeight = [4.5f, 4.5f, 4.5f, 4.5f], Avenger, Avenger3);

        Each(p => p.ChaffSpread = true, Pyro, Rogue, Seabreeze, Tula, Mogul, Starling, Nokota, Molotok, AlphaZ1, Microlight, Howard, Bombushka, Hunter, Havok, Avenger, Avenger3, Akula, Thruster, Volatol, Alkonost, Seasparrow2, Seasparrow3, Cargobob5, Titan2);

        Each(p => p.ChaffScale = 10f, Pyro, Rogue, Seabreeze, Tula, Mogul, Starling, Nokota, Molotok, AlphaZ1, Microlight, Howard, Hunter, Akula, Oppressor2, Strikeforce, Alkonost, Seasparrow2, Seasparrow3, Cargobob5, Titan2, Duster2);
        Each(p => p.ChaffScale = 5f, Bombushka, Avenger, Avenger3, Volatol);
        Each(p => p.ChaffScale = 7.5f, Havok);
        Each(p => p.ChaffScale = 4f, Thruster);

        Each(p => p.SmokeScale = 0.82f, AlphaZ1);
        Each(p => p.SmokeScale = 0.7f, Bombushka, Avenger, Avenger3, Thruster, Volatol, Alkonost, Cargobob5, Titan2, Duster2);

        void Smoke(string model, float y, float z) => At(model).SmokeOffset = [0f, y, z];

        Smoke(Cuban800, -3f, -0.3f);
        Smoke(Mogul, -5f, 0.7f);
        Smoke(Rogue, -7f, 0.6f);
        Smoke(Starling, -3f, 0.6f);
        Smoke(Seabreeze, -3f, 0.2f);
        Smoke(Tula, -5f, 0.7f);
        Smoke(Bombushka, -21f, 4.5f);
        Smoke(Hunter, -6f, 0f);
        Smoke(Nokota, -4f, 0f);
        Smoke(Pyro, -3f, 0.3f);
        Smoke(Molotok, -5f, 0.3f);
        Smoke(Havok, -4f, 0.3f);
        Smoke(AlphaZ1, -2.5f, -0.2f);
        Smoke(Microlight, -2f, 0.5f);
        Smoke(Howard, -3.5f, 0.5f);
        Smoke(Avenger, -10f, 1f);
        Smoke(Avenger3, -10f, 1f);
        Smoke(Akula, -6f, 0f);
        Smoke(Thruster, -0.5f, 0f);
        Smoke(Oppressor2, -1.2f, -0.1f);
        Smoke(Volatol, -20f, 1f);
        Smoke(Alkonost, -26f, 0.3f);
        Smoke(Cargobob5, -9.965f, 1.54f);
        Smoke(Titan2, -19.2f, 1.4f);
        Smoke(Duster2, -6.11f, -0.7f);

        return profiles;
    }
}
