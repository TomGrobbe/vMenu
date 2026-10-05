namespace vMenu.Enhanced.Data.Bullying;

public static class BullyModels
{
    public const string ClownVan = "speedo2";

    public const string Clown = "s_m_y_clown_01";

    public const string Carjacker = "a_m_y_hipster_01";

    public const string Ship = "imp_prop_ship_01a";

    public const string FallbackShip = "p_spinning_anus_s";

    public static IReadOnlyList<string> Muggers { get; } =
    [
        "g_m_y_mexgoon_01",
        "g_m_y_lost_01",
        "g_m_y_ballaorig_01",
        "g_m_y_famca_01",
        "g_m_y_korean_02",
        "g_m_y_salvagoon_01",
    ];

    public static IReadOnlyList<string> Spawned { get; } = Muggers.Concat([ClownVan, Clown, Carjacker, Ship, FallbackShip]).ToList();
}
