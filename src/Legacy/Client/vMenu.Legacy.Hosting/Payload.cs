using System.Collections;
using System.Globalization;

namespace vMenu.Legacy.Hosting;

public static class Payload
{
    public static string[] Strings(object? value) =>
        value is IEnumerable items and not string ? items.Cast<object?>().Select(item => item?.ToString()!).ToArray() : [];

    public static int[] Ints(object? value) =>
        value is IEnumerable items and not string ? items.Cast<object?>().Select(item => Convert.ToInt32(item, CultureInfo.InvariantCulture)).ToArray() : [];
}
