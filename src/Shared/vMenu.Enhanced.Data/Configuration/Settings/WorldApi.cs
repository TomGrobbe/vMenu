namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class WorldApi
{
    public static readonly StringSetting Token = new("vMenu.Enhanced.WorldApi.Token")
    {
        Description =
            "A password that lets your own tools read the weather, time, date and moon phase over " +
            "HTTP, so a Discord bot or a website can show what the sky is doing without guessing. " +
            "Leave it empty and the endpoint is switched off and answers nobody. Set it to any long " +
            "random string and callers hand it back in an X-vMenu-Token header, or a token query " +
            "parameter, to be let in. Uses 'set' and not 'setr': this is a secret, and anybody " +
            "holding it can read your server's world state.",
        ServerOnly = true,
    };
}
