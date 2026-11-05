using System.Numerics;

namespace vMenu.Enhanced.Freecam;

public readonly struct CameraPose(Vector3 position, Vector3 rotation, float fov, float focus)
{
    public Vector3 Position { get; } = position;

    public Vector3 Rotation { get; } = rotation;

    public float Fov { get; } = fov;

    public float Focus { get; } = focus;
}

public enum KeyframeBlend
{
    Cut,
    Linear,
    Smooth,
}

public enum KeyframeEase
{
    None,
    In,
    Out,
    InOut,
}

public enum PathSpace
{
    World,
    Target,
}

public enum AttachMode
{
    Full,
    PositionAndHeading,
    PositionOnly,
}

public interface IFilmItem
{
    string Name { get; set; }
}

public abstract class FilmPose
{
    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }

    public float Pitch { get; set; }

    public float Roll { get; set; }

    public float Yaw { get; set; }

    public float Fov { get; set; } = 45f;

    public float Focus { get; set; } = 10f;

    public CameraPose ToPose() => new(new Vector3(X, Y, Z), new Vector3(Pitch, Roll, Yaw), Fov, Focus);

    public void CopyPose(CameraPose pose)
    {
        X = pose.Position.X;
        Y = pose.Position.Y;
        Z = pose.Position.Z;
        Pitch = pose.Rotation.X;
        Roll = pose.Rotation.Y;
        Yaw = pose.Rotation.Z;
        Fov = pose.Fov;
        Focus = pose.Focus;
    }
}

public sealed class FilmKeyframe : FilmPose
{
    public float Duration { get; set; } = 3f;

    public KeyframeBlend Blend { get; set; } = KeyframeBlend.Smooth;

    public KeyframeEase Ease { get; set; } = KeyframeEase.None;

    public static FilmKeyframe From(CameraPose pose)
    {
        var frame = new FilmKeyframe();

        frame.CopyPose(pose);

        return frame;
    }

    public FilmKeyframe Copy()
    {
        var copy = From(ToPose());

        copy.Duration = Duration;
        copy.Blend = Blend;
        copy.Ease = Ease;

        return copy;
    }
}

public sealed class FilmPath : IFilmItem
{
    public const int SchemaVersion = 1;

    public const int MaxKeyframes = 100;

    public const float MinKeyframeDuration = 0.1f;

    public string Name { get; set; } = string.Empty;

    public PathSpace Space { get; set; } = PathSpace.World;

    public AttachMode Attach { get; set; } = AttachMode.Full;

    public List<FilmKeyframe> Keyframes { get; set; } = [];

    public FilmPath Copy(string name) => new()
    {
        Name = name,
        Space = Space,
        Attach = Attach,
        Keyframes = Keyframes.Select(static frame => frame.Copy()).ToList(),
    };

    public float Duration(bool closed = false)
    {
        var total = 0f;

        for (var index = 0; index < PathMath.Segments(Keyframes.Count, closed); index++)
        {
            total += PathMath.Length(Keyframes[index]);
        }

        return total;
    }
}

public sealed class FilmShot : FilmPose, IFilmItem
{
    public const int SchemaVersion = 1;

    public string Name { get; set; } = string.Empty;

    public static FilmShot From(string name, CameraPose pose)
    {
        var shot = new FilmShot { Name = name };

        shot.CopyPose(pose);

        return shot;
    }
}
