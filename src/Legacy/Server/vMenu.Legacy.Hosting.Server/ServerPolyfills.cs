using System.Numerics;
using System.Security.Cryptography;

namespace System;

public static class ServerPolyfills
{
    extension(Convert)
    {
        public static string ToHexStringLower(byte[] bytes)
        {
            var characters = new char[bytes.Length * 2];

            for (var i = 0; i < bytes.Length; i++)
            {
                characters[i * 2] = Nibble(bytes[i] >> 4);
                characters[(i * 2) + 1] = Nibble(bytes[i] & 0xF);
            }

            return new string(characters);
        }

        public static string ToHexString(byte[] bytes) => Convert.ToHexStringLower(bytes).ToUpperInvariant();

        public static byte[] FromHexString(string text)
        {
            if (text.Length % 2 != 0)
            {
                throw new FormatException("The hex string has an odd length.");
            }

            var bytes = new byte[text.Length / 2];

            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)((Value(text[i * 2]) << 4) | Value(text[(i * 2) + 1]));
            }

            return bytes;
        }
    }

    extension(SHA256)
    {
        public static byte[] HashData(byte[] source)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(source);
        }
    }

    extension(HMACSHA256)
    {
        public static byte[] HashData(byte[] key, byte[] source)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(source);
        }
    }

    extension(RandomNumberGenerator)
    {
        public static byte[] GetBytes(int count)
        {
            var bytes = new byte[count];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            return bytes;
        }
    }

    public static byte[] ToByteArray(this BigInteger value, bool isUnsigned, bool isBigEndian)
    {
        var bytes = value.ToByteArray();

        if (isUnsigned && bytes.Length > 1 && bytes[bytes.Length - 1] == 0)
        {
            Array.Resize(ref bytes, bytes.Length - 1);
        }

        if (isBigEndian)
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    private static char Nibble(int value) => (char)(value < 10 ? '0' + value : 'a' + value - 10);

    private static int Value(char character) => character switch
    {
        >= '0' and <= '9' => character - '0',
        >= 'a' and <= 'f' => character - 'a' + 10,
        >= 'A' and <= 'F' => character - 'A' + 10,
        _ => throw new FormatException($"'{character}' is not a hex digit."),
    };
}
