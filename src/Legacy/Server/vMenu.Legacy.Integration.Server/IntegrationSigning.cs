using System.Security.Cryptography;
using System.Text;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Integration.Server;

// Pure BCL crypto, so it is safe to run off the tick thread (the socket does).
internal static class IntegrationSigning
{
    public const string Scheme = "v1";

    public static byte[] Hmac(string key, string message) =>
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(message));

    public static string KeyId(string key) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)));

    public static string BodyHash(byte[] body) =>
        Convert.ToHexStringLower(SHA256.HashData(body));

    public static string EmptyBodyHash() => BodyHash([]);

    public static string Canonical(string action, string timestamp, string nonce, string bodyHashHex) =>
        $"{Scheme}\n{action}\n{timestamp}\n{nonce}\n{bodyHashHex}";

    public static string Signature(string key, string canonical) =>
        Scheme + "=" + Convert.ToHexStringLower(Hmac(key, canonical));

    public static string NewNonce()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);

        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    // Constant-time compare of a presented "v1=<hex>" signature against the expected HMAC bytes.
    public static bool SignatureMatches(string presented, byte[] expected)
    {
        if (!presented.StartsWith(Scheme + "=", StringComparison.Ordinal))
        {
            return false;
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromHexString(presented[(Scheme.Length + 1)..]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (bytes.Length != expected.Length)
        {
            return false;
        }

        var difference = 0;
        for (var index = 0; index < bytes.Length; index++)
        {
            difference |= bytes[index] ^ expected[index];
        }

        return difference == 0;
    }

    // Boot self-test: proves the crypto loads in this runtime rather than failing silently per request.
    public static void Verify()
    {
        try
        {
            var nonce = NewNonce();
            var canonical = Canonical(IntegrationActions.Socket, "1700000000", nonce, EmptyBodyHash());
            var signature = Signature("self-test-key", canonical);

            if (nonce.Length == 0 || !SignatureMatches(signature, Hmac("self-test-key", canonical)))
            {
                throw new CryptographicException("signing round-trip did not verify.");
            }

            Log.Debug("[Integration] Request signing self-test passed.");
        }
        catch (Exception exception)
        {
            Log.Error(
                $"[Integration] Request signing is unavailable ({exception.GetType().Name}: {exception.Message}). " +
                "The integration cannot verify requests and will refuse every one.");
        }
    }
}
