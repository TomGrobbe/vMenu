using System.Numerics;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace vMenu.Enhanced.Freecam;

public static class TargetLock
{
    private const int HeadBone = 31086;

    public static int Entity { get; private set; }

    public static bool LookAt { get; private set; }

    public static bool Follow { get; private set; }

    public static AttachMode Mode { get; private set; } = AttachMode.Full;

    public static Vector3 LookOffset { get; set; }

    public static bool IsLocked => Entity != 0;

    public static event Action? Changed;

    internal static bool IsValid => Entity != 0 && API.DoesEntityExist(Entity);

    internal static bool IsFollowing => IsValid && Follow;

    public static void Lock(int entity, bool lookAt, bool follow)
    {
        if (entity == 0 || !API.DoesEntityExist(entity))
        {
            return;
        }

        Change(() =>
        {
            Entity = entity;
            LookAt = lookAt;
            Follow = follow;
            LookOffset = Vector3.Zero;
        });
    }

    public static void SetLookAt(bool lookAt)
    {
        if (!IsLocked || LookAt == lookAt)
        {
            return;
        }

        Change(() => LookAt = lookAt);
    }

    public static void SetFollow(bool follow)
    {
        if (!IsLocked || Follow == follow)
        {
            return;
        }

        Change(() => Follow = follow);
    }

    public static void SetMode(AttachMode mode)
    {
        if (Mode == mode)
        {
            return;
        }

        Change(() => Mode = mode);
    }

    public static void Release()
    {
        if (!IsLocked)
        {
            return;
        }

        Change(Clear);
    }

    internal static void Reset()
    {
        if (!IsLocked)
        {
            return;
        }

        Clear();

        Changed?.Invoke();
    }

    private static void Change(Action change)
    {
        Freecam.ChangeBasis(change);

        Changed?.Invoke();
    }

    private static void Clear()
    {
        Entity = 0;
        LookAt = false;
        Follow = false;
        LookOffset = Vector3.Zero;
    }

    internal static (Vector3 Origin, Quaternion Rotation) Basis(AttachMode mode)
    {
        var origin = API.GetEntityCoords(Entity, false);

        return mode switch
        {
            AttachMode.Full => (origin, Orientation.FromEuler(API.GetEntityRotation(Entity, Orientation.RotationOrder))),
            AttachMode.PositionAndHeading => (origin, Orientation.Heading(API.GetEntityHeading(Entity))),
            _ => (origin, Quaternion.Identity),
        };
    }

    internal static (Vector3 Origin, Quaternion Rotation) FollowBasis() =>
        IsFollowing ? Basis(Mode) : (Vector3.Zero, Quaternion.Identity);

    internal static Vector3 LookPoint()
    {
        var offset = API.GetOffsetFromEntityInWorldCoords(Entity, LookOffset.X, LookOffset.Y, LookOffset.Z)
            - API.GetEntityCoords(Entity, false);

        var anchor = API.IsEntityAPed(Entity)
            ? API.GetPedBoneCoords(Entity, HeadBone, 0f, 0f, 0f)
            : API.GetEntityCoords(Entity, false);

        return anchor + offset;
    }
}
