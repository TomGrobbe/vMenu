using System.Numerics;

namespace vMenu.Enhanced.Freecam;

// Own interpolation, because the game's interpolation natives can't reliably pause and resume midway.
internal static class PathMath
{
    private const float EaseShare = 0.35f;

    internal static float Length(FilmKeyframe frame) => MathF.Max(frame.Duration, FilmPath.MinKeyframeDuration);

    internal static float Lerp(float from, float to, float u) => from + ((to - from) * u);

    internal static int Segments(int keyframes, bool closed) =>
        keyframes < 2 ? 0 : closed ? keyframes : keyframes - 1;

    internal static CameraPose Sample(FilmPath path, float time, bool closed)
    {
        var frames = path.Keyframes;

        if (frames.Count == 0)
        {
            return default;
        }

        if (frames.Count == 1 || time <= 0f)
        {
            return frames[0].ToPose();
        }

        var segments = Segments(frames.Count, closed);
        var start = 0f;

        for (var index = 0; index < segments; index++)
        {
            var length = Length(frames[index]);

            if (time < start + length || index == segments - 1)
            {
                return Segment(frames, index, Math.Clamp((time - start) / length, 0f, 1f), closed);
            }

            start += length;
        }

        return frames[^1].ToPose();
    }

    private static float Ease(float u, KeyframeEase ease)
    {
        var easeIn = ease is KeyframeEase.In or KeyframeEase.InOut ? EaseShare : 0f;
        var easeOut = ease is KeyframeEase.Out or KeyframeEase.InOut ? EaseShare : 0f;

        if (easeIn == 0f && easeOut == 0f)
        {
            return u;
        }

        var peak = 1f / (1f - (easeIn / 2f) - (easeOut / 2f));

        if (u < easeIn)
        {
            return peak * u * u / (2f * easeIn);
        }

        if (u <= 1f - easeOut)
        {
            return peak * (u - (easeIn / 2f));
        }

        var remaining = 1f - u;

        return 1f - (peak * remaining * remaining / (2f * easeOut));
    }

    private static CameraPose Segment(List<FilmKeyframe> frames, int index, float u, bool closed)
    {
        var from = frames[index].ToPose();
        var to = At(frames, index + 1, closed);

        if (frames[index].Blend == KeyframeBlend.Cut)
        {
            return u >= 1f ? to : from;
        }

        u = Ease(u, frames[index].Ease);

        var position = frames[index].Blend == KeyframeBlend.Smooth
            ? CatmullRom(At(frames, index - 1, closed).Position, from.Position, to.Position, At(frames, index + 2, closed).Position, u)
            : Vector3.Lerp(from.Position, to.Position, u);

        var rotation = Quaternion.Slerp(Orientation.FromEuler(from.Rotation), Orientation.FromEuler(to.Rotation), u);

        return new CameraPose(
            position,
            Orientation.ToEuler(rotation),
            Lerp(from.Fov, to.Fov, u),
            Lerp(from.Focus, to.Focus, u));
    }

    private static CameraPose At(List<FilmKeyframe> frames, int index, bool closed)
    {
        var count = frames.Count;

        return frames[closed ? ((index % count) + count) % count : Math.Clamp(index, 0, count - 1)].ToPose();
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        var t2 = t * t;
        var t3 = t2 * t;

        return 0.5f * (
            (2f * p1)
            + ((p2 - p0) * t)
            + (((2f * p0) - (5f * p1) + (4f * p2) - p3) * t2)
            + (((3f * p1) - p0 - (3f * p2) + p3) * t3));
    }
}
