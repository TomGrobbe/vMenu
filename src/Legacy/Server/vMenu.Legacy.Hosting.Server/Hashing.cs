namespace vMenu.Legacy.Hosting;

public static class Hashing
{
    public static uint Joaat(string key)
    {
        uint hash = 0;

        foreach (var value in System.Text.Encoding.UTF8.GetBytes(key))
        {
            var lower = value is >= (byte)'A' and <= (byte)'Z' ? (byte)(value + 32) : value;

            unchecked
            {
                hash += lower;
                hash += hash << 10;
                hash ^= hash >> 6;
            }
        }

        unchecked
        {
            hash += hash << 3;
            hash ^= hash >> 11;
            hash += hash << 15;
        }

        return hash;
    }
}
