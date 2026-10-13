namespace vMenu.Enhanced.Polyfills;

public static class MathF
{
    public const float PI = (float)Math.PI;

    public const float E = (float)Math.E;

    public const float Tau = (float)(Math.PI * 2);

    public static float Abs(float x) => Math.Abs(x);

    public static float Acos(float x) => (float)Math.Acos(x);

    public static float Asin(float x) => (float)Math.Asin(x);

    public static float Atan(float x) => (float)Math.Atan(x);

    public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);

    public static float Ceiling(float x) => (float)Math.Ceiling(x);

    public static float CopySign(float x, float y) => y < 0 || (y == 0 && 1 / y < 0) ? -Math.Abs(x) : Math.Abs(x);

    public static float Cos(float x) => (float)Math.Cos(x);

    public static float Exp(float x) => (float)Math.Exp(x);

    public static float Floor(float x) => (float)Math.Floor(x);

    public static float Log(float x) => (float)Math.Log(x);

    public static float Log10(float x) => (float)Math.Log10(x);

    public static float Max(float x, float y) => Math.Max(x, y);

    public static float Min(float x, float y) => Math.Min(x, y);

    public static float Pow(float x, float y) => (float)Math.Pow(x, y);

    public static float Round(float x) => (float)Math.Round(x);

    public static float Round(float x, int digits) => (float)Math.Round(x, digits);

    public static int Sign(float x) => Math.Sign(x);

    public static float Sin(float x) => (float)Math.Sin(x);

    public static float Sqrt(float x) => (float)Math.Sqrt(x);

    public static float Tan(float x) => (float)Math.Tan(x);

    public static float Truncate(float x) => (float)(x < 0 ? Math.Ceiling(x) : Math.Floor(x));
}
