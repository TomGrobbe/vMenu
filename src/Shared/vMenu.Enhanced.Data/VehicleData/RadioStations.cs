namespace vMenu.Enhanced.Data.VehicleData;

// The game answers GET_RADIO_STATION_NAME with an internal name like RADIO_01_CLASS_ROCK, which is no
// use in a menu. This maps the ones the base game ships to something readable. A station that is not
// listed, which is what an add-on radio resource adds, falls back to its own tidied up name.
public static class RadioStations
{
    public const string Off = "OFF";

    private static readonly Dictionary<string, string> Names = new(StringComparer.Ordinal)
    {
        ["RADIO_01_CLASS_ROCK"] = "Los Santos Rock Radio",
        ["RADIO_02_POP"] = "Non-Stop-Pop FM",
        ["RADIO_03_HIPHOP_NEW"] = "Radio Los Santos",
        ["RADIO_04_PUNK"] = "Channel X",
        ["RADIO_05_TALK_01"] = "West Coast Talk Radio",
        ["RADIO_06_COUNTRY"] = "Rebel Radio",
        ["RADIO_07_DANCE_01"] = "Soulwax FM",
        ["RADIO_08_MEXICAN"] = "East Los FM",
        ["RADIO_09_HIPHOP_OLD"] = "West Coast Classics",
        ["RADIO_11_TALK_02"] = "Blaine County Radio",
        ["RADIO_12_reggae"] = "Blue Ark",
        ["RADIO_13_JAZZ"] = "Worldwide FM",
        ["RADIO_14_DANCE_02"] = "FlyLo FM",
        ["RADIO_15_MOTOWN"] = "The Lowdown 91.1",
        ["RADIO_16_SILVERLAKE"] = "Radio Mirror Park",
        ["RADIO_17_FUNK"] = "Space 103.2",
        ["RADIO_18_90S_ROCK"] = "Vinewood Boulevard Radio",
        ["RADIO_19_USER"] = "Self Radio",
        ["RADIO_20_THELAB"] = "The Lab",
        ["RADIO_21_DLC_XM17"] = "Blonded Los Santos 97.8 FM",
        ["RADIO_22_DLC_BATTLE_MIX1_RADIO"] = "Los Santos Underground Radio",
        ["RADIO_23_DLC_XM19_RADIO"] = "iFruit Radio",
        ["RADIO_27_DLC_PRHEI4"] = "Still Slipping Los Santos",
        ["RADIO_34_DLC_HEI4_KULT"] = "Kult FM",
        ["RADIO_35_DLC_HEI4_MLR"] = "The Music Locker",
        ["RADIO_36_AUDIOPLAYER"] = "Media Player",
        ["RADIO_37_MOTOMAMI"] = "MOTOMAMI Los Santos",
    };

    public static string DisplayName(string station) =>
        Names.TryGetValue(station, out var name) ? name : Tidy(station);

    // RADIO_42_SOME_STATION becomes Some Station, which is a far better guess than the raw name for an
    // add-on station vMenu has never heard of.
    private static string Tidy(string station)
    {
        var parts = station.Split('_');
        var words = new List<string>(parts.Length);

        foreach (var part in parts)
        {
            if (part.Length == 0 || part.Equals("RADIO", StringComparison.OrdinalIgnoreCase) || int.TryParse(part, out _))
            {
                continue;
            }

            words.Add(part.Length == 1
                ? part.ToUpperInvariant()
                : char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant());
        }

        return words.Count == 0 ? station : string.Join(" ", words);
    }
}
