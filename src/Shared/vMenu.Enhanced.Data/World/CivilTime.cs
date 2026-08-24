namespace vMenu.Enhanced.Data.World;

// Calendar to Unix seconds, for the client fallback that reads the machine's own clock. Hand rolled
// because DateTime crashes the client sandbox. Howard Hinnant's days from civil.
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

    // The inverse: a day count since 1 January 1970 back to a calendar date.
    public static void FromDays(long days, out int year, out int month, out int day)
    {
        var z = days + 719468L;
        var era = (z >= 0 ? z : z - 146096) / 146097;
        var dayOfEra = z - (era * 146097);
        var yearOfEra = (dayOfEra - (dayOfEra / 1460) + (dayOfEra / 36524) - (dayOfEra / 146096)) / 365;
        var dayOfYear = dayOfEra - ((365 * yearOfEra) + (yearOfEra / 4) - (yearOfEra / 100));
        var mp = ((5 * dayOfYear) + 2) / 153;

        day = (int)(dayOfYear - (((153 * mp) + 2) / 5) + 1);
        month = (int)(mp + (mp < 10 ? 3 : -9));
        year = (int)(yearOfEra + (era * 400) + (month <= 2 ? 1 : 0));
    }
}
