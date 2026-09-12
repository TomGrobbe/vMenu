using System.Globalization;

using vMenu.Enhanced.Configuration.Server;

using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;

namespace vMenu.Enhanced.Integration.Server;

public static class IntegrationAuth
{
    public const string SignatureHeader = "X-vMenu-Signature";

    public const string TimestampHeader = "X-vMenu-Timestamp";

    public const string NonceHeader = "X-vMenu-Nonce";

    public const string KeyIdHeader = "X-vMenu-Key-Id";

    public static bool IsConfigured => Key.Length > 0;

    // Convar read: main thread only.
    public static string Key => ServerConfig.Value(IntegrationSettings.ApiKey);

    public static Dictionary<string, string> SignHeaders(string action, string key, byte[] body)
    {
        var timestamp = ((long)ServerClock.Now()).ToString(CultureInfo.InvariantCulture);
        var nonce = IntegrationSigning.NewNonce();
        var canonical = IntegrationSigning.Canonical(action, timestamp, nonce, IntegrationSigning.BodyHash(body));

        return new Dictionary<string, string>
        {
            [TimestampHeader] = timestamp,
            [NonceHeader] = nonce,
            [SignatureHeader] = IntegrationSigning.Signature(key, canonical),
            [KeyIdHeader] = IntegrationSigning.KeyId(key),
        };
    }
}
