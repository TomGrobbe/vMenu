using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Players;

namespace vMenu.Enhanced.Menus.Bully;

internal static class PedPranks
{
    private const int GrenadeExplosion = 0;

    private const int RaygunExplosion = 70;

    private const int HeadBone = 31086;

    private const int SpineBone = 24818;

    private const int PelvisBone = 11816;

    private const int NeckBone = 39317;

    private const int StunGroundMs = 5000;

    private const int RagdollMs = 4000;

    private static readonly uint StunGun = Streaming.Hash("WEAPON_STUNGUN");

    private static readonly Window Floppy = new();

    public static void Explode() => Blast(GrenadeExplosion, 0f);

    public static void Atomize() => Blast(RaygunExplosion, 0.5f);

    public static async Task Stun(BullyRun run)
    {
        var ped = Native.PlayerPedId();

        AllowRagdoll(StunGroundMs);

        Native.SetPedMinGroundTimeForStungun(ped, StunGroundMs);

        Zap(Bone(ped, HeadBone, 0.2f), Bone(ped, HeadBone), false);
        Zap(Native.GetFinalRenderedCamCoord(), Bone(ped, SpineBone), false);

        run.Started();

        await API.Delay(StunGroundMs);

        Native.SetPedMinGroundTimeForStungun(Native.PlayerPedId(), -1);
    }

    public static void ShockFromDoor(int ped)
    {
        AllowRagdoll(StunGroundMs);

        Zap(Bone(ped, PelvisBone), Bone(ped, NeckBone), true);
    }

    public static async Task Fire(BullyRun run)
    {
        var ped = Native.PlayerPedId();

        Native.StartEntityFire(ped);

        run.Started();

        await API.Delay(4000);

        Native.StopEntityFire(ped);
    }

    public static void Ragdoll()
    {
        AllowRagdoll(RagdollMs);

        Native.SetPedToRagdoll(Native.PlayerPedId(), RagdollMs, RagdollMs, 0, false, false, false);
    }

    public static void Jump() => Native.SetForcedJumpThisFrame(Native.PlayerId());

    // Overrides the player's own no ragdoll option for a moment, then hands it back.
    public static void AllowRagdoll(int durationMs)
    {
        var started = Floppy.Extend(durationMs);

        Native.SetPedCanRagdoll(Native.PlayerPedId(), true);

        if (started)
        {
            BullyTask.Run(RestoreRagdollAsync, "AllowRagdoll");
        }
    }

    private static async Task RestoreRagdollAsync()
    {
        await Floppy.WaitAsync();

        Floppy.Close();

        PlayerNoRagdoll.Reapply();
    }

    private static void Blast(int explosion, float raise)
    {
        var position = Native.GetEntityCoords(Native.PlayerPedId(), false);

        AllowRagdoll(RagdollMs);

        Native.AddExplosion(position.X, position.Y, position.Z + raise, explosion, 1f, true, false, 1f, false);
    }

    private static Vector3 Bone(int ped, int bone, float raise = 0f) => Native.GetPedBoneCoords(ped, bone, 0f, 0f, raise);

    private static void Zap(Vector3 from, Vector3 to, bool rumble) =>
        Native.ShootSingleBulletBetweenCoords(from.X, from.Y, from.Z, to.X, to.Y, to.Z, 1, true, StunGun, 0, true, rumble, -1f);
}
