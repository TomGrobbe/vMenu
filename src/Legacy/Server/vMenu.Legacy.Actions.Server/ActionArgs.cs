using System.Globalization;

namespace vMenu.Enhanced.Actions.Server;

/// <summary>Reads action arguments the way every client writes them: invariant culture, so a comma decimal
/// separator on the server never misreads a number.</summary>
public static class ActionArgs
{
    public static bool TryInt(string value, out int result) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

    public static bool TryInt(string[] args, int index, out int result)
    {
        result = 0;

        return args.Length > index && TryInt(args[index], out result);
    }

    public static bool TryFloat(string value, out float result) =>
        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
}
