using System.Globalization;

namespace vMenu.Enhanced.Data.Updates;

// A version, and the semver 2.0.0 ordering over it. A class rather than a record: generated equality
// routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
public sealed class SemanticVersion
{
    private readonly string[] _prerelease;

    private SemanticVersion(int major, int minor, int patch, string[] prerelease, string text)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        _prerelease = prerelease;
        Text = text;
    }

    public int Major { get; }

    public int Minor { get; }

    public int Patch { get; }

    // True for 0.0.1-alpha.67, false for 1.0.0. What the stable channel filters on.
    public bool IsPrerelease => _prerelease.Length > 0;

    // As it was written, minus a leading v. What gets shown to people.
    public string Text { get; }

    public static bool TryParse(string? text, out SemanticVersion? version)
    {
        version = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var value = text.Trim();

        if (value.Length > 1 && (value[0] == 'v' || value[0] == 'V'))
        {
            value = value[1..];
        }

        // Build metadata takes no part in precedence, so it is dropped rather than compared.
        var plus = value.IndexOf('+');

        if (plus >= 0)
        {
            value = value[..plus];
        }

        var dash = value.IndexOf('-');
        var core = dash < 0 ? value : value[..dash];
        string[] labels = dash < 0 ? [] : value[(dash + 1)..].Split('.');

        var parts = core.Split('.');

        if (parts.Length is 0 or > 3)
        {
            return false;
        }

        if (!Number(parts, 0, out var major) || !Number(parts, 1, out var minor) || !Number(parts, 2, out var patch))
        {
            return false;
        }

        foreach (var label in labels)
        {
            if (label.Length == 0)
            {
                return false;
            }

            foreach (var character in label)
            {
                if (!char.IsAsciiLetterOrDigit(character) && character != '-')
                {
                    return false;
                }
            }
        }

        version = new SemanticVersion(major, minor, patch, labels, value);

        return true;
    }

    // Negative when left sorts below right.
    public static int Compare(SemanticVersion left, SemanticVersion right)
    {
        var core = left.Major.CompareTo(right.Major);

        if (core != 0)
        {
            return core;
        }

        core = left.Minor.CompareTo(right.Minor);

        if (core != 0)
        {
            return core;
        }

        core = left.Patch.CompareTo(right.Patch);

        if (core != 0)
        {
            return core;
        }

        // A pre-release version has lower precedence than the release it belongs to.
        if (left._prerelease.Length == 0)
        {
            return right._prerelease.Length == 0 ? 0 : 1;
        }

        if (right._prerelease.Length == 0)
        {
            return -1;
        }

        var shared = Math.Min(left._prerelease.Length, right._prerelease.Length);

        for (var index = 0; index < shared; index++)
        {
            var order = CompareIdentifier(left._prerelease[index], right._prerelease[index]);

            if (order != 0)
            {
                return order;
            }
        }

        // A larger set of pre-release fields wins, everything before it being equal.
        return left._prerelease.Length.CompareTo(right._prerelease.Length);
    }

    public bool IsNewerThan(SemanticVersion other) => Compare(this, other) > 0;

    public override string ToString() => Text;

    // The whole reason this type exists: alpha.67 against alpha.123 has to compare 67 and 123 as
    // numbers. Ordinally "67" sorts above "123", and the checker would announce a downgrade as an
    // update on most builds.
    private static int CompareIdentifier(string left, string right)
    {
        var leftNumeric = IsNumeric(left, out var leftValue);
        var rightNumeric = IsNumeric(right, out var rightValue);

        if (leftNumeric && rightNumeric)
        {
            return leftValue.CompareTo(rightValue);
        }

        // Numeric identifiers always have lower precedence than alphanumeric ones.
        if (leftNumeric)
        {
            return -1;
        }

        if (rightNumeric)
        {
            return 1;
        }

        return string.Compare(left, right, StringComparison.Ordinal);
    }

    private static bool IsNumeric(string value, out long number) =>
        long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out number);

    // NumberStyles.None so a leading sign, a decimal point or surrounding whitespace is refused rather
    // than quietly accepted into a version number.
    private static bool Number(string[] parts, int index, out int value)
    {
        if (index >= parts.Length)
        {
            value = 0;

            return true;
        }

        return int.TryParse(parts[index], NumberStyles.None, CultureInfo.InvariantCulture, out value) && value >= 0;
    }
}
