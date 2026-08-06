namespace vMenu.Enhanced.Data.World;

/// <summary>Calendar to Unix seconds, for the client fallback that reads the machine's own clock.</summary>
// Hand rolled because DateTime crashes the client sandbox. Howard Hinnant's days from civil.
public static class CivilTime
{
    public static long ToUnixSeconds(int year, int month, int day, int hour, int minute, int second)
    {
        var y = year - (month <= 2 ? 1 : 0);
        var era = (y >= 0 ? y : y - 399) / 400;
        var yearOfEra = y - (era * 400);
        var dayOfYear = ((153 * (month + (month > 2 ? -3 : 9))) + 2) / 5 + day - 1;
        var dayOfEra = (yearOfEra * 365) + (yearOfEra / 4) - (yearOfEra / 100) + dayOfYear;
        var days = (era * 146097L) + dayOfEra - 719468L;

        return (days * 86400L) + (hour * 3600L) + (minute * 60L) + second;
    }
}
