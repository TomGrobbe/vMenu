using System.Globalization;

namespace vMenu.Enhanced.Polyfills;

public static class StaticPolyfills
{
    extension(char)
    {
        public static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';

        public static bool IsAsciiLetter(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

        public static bool IsAsciiLetterLower(char c) => c is >= 'a' and <= 'z';

        public static bool IsAsciiLetterUpper(char c) => c is >= 'A' and <= 'Z';

        public static bool IsAsciiLetterOrDigit(char c) => char.IsAsciiLetter(c) || char.IsAsciiDigit(c);

        public static bool IsAsciiHexDigit(char c) => char.IsAsciiDigit(c) || c is >= 'a' and <= 'f' or >= 'A' and <= 'F';

        public static bool IsHighSurrogate(char c) => c is >= '\uD800' and <= '\uDBFF';
    }

    extension(Math)
    {
        public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

        public static uint Clamp(uint value, uint min, uint max) => value < min ? min : value > max ? max : value;

        public static long Clamp(long value, long min, long max) => value < min ? min : value > max ? max : value;

        public static short Clamp(short value, short min, short max) => value < min ? min : value > max ? max : value;

        public static byte Clamp(byte value, byte min, byte max) => value < min ? min : value > max ? max : value;

        public static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;

        public static double Clamp(double value, double min, double max) => value < min ? min : value > max ? max : value;
    }

#nullable disable
    extension(Enum)
    {
        public static T[] GetValues<T>()
            where T : struct, Enum => (T[])Enum.GetValues(typeof(T));

        public static string[] GetNames<T>()
            where T : struct, Enum => Enum.GetNames(typeof(T));

        public static bool IsDefined<T>(T value)
            where T : struct, Enum => Enum.IsDefined(typeof(T), value);

        public static T Parse<T>(string value)
            where T : struct, Enum => (T)Enum.Parse(typeof(T), value, false);

        public static T Parse<T>(string value, bool ignoreCase)
            where T : struct, Enum => (T)Enum.Parse(typeof(T), value, ignoreCase);
    }
#nullable restore

    extension(Array)
    {
        public static bool Exists<T>(T[] array, Predicate<T> match)
        {
            foreach (var item in array)
            {
                if (match(item))
                {
                    return true;
                }
            }

            return false;
        }

        public static T? Find<T>(T[] array, Predicate<T> match)
        {
            foreach (var item in array)
            {
                if (match(item))
                {
                    return item;
                }
            }

            return default;
        }

        public static T[] FindAll<T>(T[] array, Predicate<T> match)
        {
            var found = new System.Collections.Generic.List<T>();

            foreach (var item in array)
            {
                if (match(item))
                {
                    found.Add(item);
                }
            }

            return found.ToArray();
        }

        public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
        {
            var converted = new TOutput[array.Length];

            for (var i = 0; i < array.Length; i++)
            {
                converted[i] = converter(array[i]);
            }

            return converted;
        }
    }

    private static readonly System.Threading.Tasks.Task Completed = CompletedSource();

    private static System.Threading.Tasks.Task CompletedSource()
    {
        var source = new System.Threading.Tasks.TaskCompletionSource<bool>();
        source.SetResult(true);
        return source.Task;
    }

    extension(System.Threading.Tasks.Task)
    {
        public static System.Threading.Tasks.Task CompletedTask => Completed;
    }

    private static readonly object TickLock = new();
    private static uint _lastTick;
    private static long _tickWraps;

    extension(Environment)
    {
        public static long TickCount64
        {
            get
            {
                lock (TickLock)
                {
                    var now = unchecked((uint)Environment.TickCount);

                    if (now < _lastTick)
                    {
                        _tickWraps++;
                    }

                    _lastTick = now;
                    return (_tickWraps << 32) + now;
                }
            }
        }
    }

    extension(BitConverter)
    {
        public static float Int32BitsToSingle(int value) => BitConverter.ToSingle(BitConverter.GetBytes(value), 0);

        public static int SingleToInt32Bits(float value) => BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
    }

    extension(string)
    {
        public static string Join(char separator, params string?[] values) => string.Join(separator.ToString(), values);

        public static string Join(char separator, params object?[] values) => string.Join(separator.ToString(), values);

        public static string Join<T>(char separator, System.Collections.Generic.IEnumerable<T> values) => string.Join(separator.ToString(), values);
    }

    extension(System.Threading.Tasks.Task task)
    {
        public bool IsCompletedSuccessfully => task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion;
    }

    extension(int)
    {
        public static int Parse(string s, IFormatProvider? provider) => int.Parse(s, NumberStyles.Integer, provider);
    }
}
