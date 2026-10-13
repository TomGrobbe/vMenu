using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace vMenu.Enhanced.Polyfills;

public static class InstancePolyfills
{
    public static string[] Split(this string text, char separator, StringSplitOptions options = StringSplitOptions.None) =>
        text.Split([separator], options);

    public static string[] Split(this string text, char separator, int count, StringSplitOptions options = StringSplitOptions.None) =>
        text.Split([separator], count, options);

    public static string[] Split(this string text, string separator, StringSplitOptions options = StringSplitOptions.None) =>
        text.Split([separator], options);

    public static string[] Split(this string text, string separator, int count, StringSplitOptions options = StringSplitOptions.None) =>
        text.Split([separator], count, options);

    public static bool Contains(this string text, string value, StringComparison comparison) =>
        text.IndexOf(value, comparison) >= 0;

    public static bool Contains(this string text, char value) => text.IndexOf(value) >= 0;

    public static bool StartsWith(this string text, char value) => text.Length > 0 && text[0] == value;

    public static bool EndsWith(this string text, char value) => text.Length > 0 && text[text.Length - 1] == value;

    public static string Replace(this string text, string oldValue, string? newValue, StringComparison comparison)
    {
        if (comparison == StringComparison.Ordinal)
        {
            return text.Replace(oldValue, newValue);
        }

        var builder = new System.Text.StringBuilder();
        var position = 0;
        int found;

        while ((found = text.IndexOf(oldValue, position, comparison)) >= 0)
        {
            builder.Append(text, position, found - position).Append(newValue);
            position = found + oldValue.Length;
        }

        return builder.Append(text, position, text.Length - position).ToString();
    }

    public static int IndexOf(this string text, char value, StringComparison comparison) =>
        text.IndexOf(value.ToString(), comparison);

    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary.ContainsKey(key))
        {
            return false;
        }

        dictionary.Add(key, value);
        return true;
    }

    public static bool Remove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out value!))
        {
            dictionary.Remove(key);
            return true;
        }

        return false;
    }

    public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out var value) ? value : default;

    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue fallback) =>
        dictionary.TryGetValue(key, out var value) ? value : fallback;

    public static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull =>
        dictionary.TryGetValue(key, out var value) ? value : default;

    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue fallback)
        where TKey : notnull =>
        dictionary.TryGetValue(key, out var value) ? value : fallback;

    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new(source);

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer) => new(source, comparer);

    public static bool TryDequeue<T>(this Queue<T> queue, out T result)
    {
        if (queue.Count > 0)
        {
            result = queue.Dequeue();
            return true;
        }

        result = default!;
        return false;
    }

    public static bool TryPeek<T>(this Queue<T> queue, out T result)
    {
        if (queue.Count > 0)
        {
            result = queue.Peek();
            return true;
        }

        result = default!;
        return false;
    }

    public static bool TryPop<T>(this Stack<T> stack, out T result)
    {
        if (stack.Count > 0)
        {
            result = stack.Pop();
            return true;
        }

        result = default!;
        return false;
    }

    public static void Clear<T>(this ConcurrentQueue<T> queue)
    {
        while (queue.TryDequeue(out _))
        {
        }
    }

    public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source) => source.OrderBy(static item => item);

    public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source, IComparer<T>? comparer) => source.OrderBy(static item => item, comparer);

    public static IOrderedEnumerable<T> OrderDescending<T>(this IEnumerable<T> source) => source.OrderByDescending(static item => item);

    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item) => source.Concat([item]);

    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item) => new[] { item }.Concat(source);

    public static string ToTitleCase(this System.Globalization.TextInfo textInfo, string text)
    {
        var chars = text.ToCharArray();
        var i = 0;

        while (i < chars.Length)
        {
            if (!char.IsLetter(chars[i]))
            {
                i++;
                continue;
            }

            var start = i;
            var allUpper = true;

            while (i < chars.Length && (char.IsLetterOrDigit(chars[i]) || chars[i] == '\''))
            {
                allUpper &= !char.IsLower(chars[i]);
                i++;
            }

            if (allUpper)
            {
                continue;
            }

            chars[start] = textInfo.ToUpper(chars[start]);

            for (var j = start + 1; j < i; j++)
            {
                chars[j] = textInfo.ToLower(chars[j]);
            }
        }

        return new string(chars);
    }
}
