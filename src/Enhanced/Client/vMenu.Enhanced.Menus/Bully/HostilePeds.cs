using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Bully;

internal static class HostilePeds
{
    public const int CivilianPedType = 4;

    private const int AlwaysFight = 5;

    private const int Aggressive = 13;

    private const int AlwaysFlee = 17;

    private const int FightArmedWhenUnarmed = 46;

    private const int CanCharge = 50;

    private const int DisableFleeFromCombat = 58;

    private const int ShouldChargeNow = 286;

    private const int NeverLoseTarget = 1;

    private const int WillAdvance = 2;

    private const int HostileBlipColour = 1;

    private const float HostileBlipScale = 0.7f;

    private static readonly uint HatesPlayer = Streaming.Hash("HATES_PLAYER");

    private static readonly int[] HostileAttributes = [AlwaysFight, Aggressive, FightArmedWhenUnarmed, CanCharge, DisableFleeFromCombat];

    public static void MakeHostile(int ped)
    {
        Native.SetPedRelationshipGroupHash(ped, HatesPlayer);

        foreach (var attribute in HostileAttributes)
        {
            Native.SetPedCombatAttributes(ped, attribute, true);
        }

        Native.SetPedConfigFlag(ped, ShouldChargeNow, true);
        Native.SetPedTargetLossResponse(ped, NeverLoseTarget);
        Native.SetPedCombatMovement(ped, WillAdvance);
        Native.SetPedKeepTask(ped, true);
    }

    public static void MakeFleeFrom(int ped, int from)
    {
        Native.SetPedCombatAttributes(ped, AlwaysFlee, true);
        Native.TaskSmartFleePed(ped, from, 300f, -1, false, false);
    }

    public static void Attack(int ped, int target) => Native.TaskCombatPed(ped, target, 0, 16);

    public static int AddHostileBlip(int entity)
    {
        var blip = Native.AddBlipForEntity(entity);

        Native.SetBlipColour(blip, HostileBlipColour);
        Native.SetBlipScale(blip, HostileBlipScale);

        return blip;
    }

    public static void RemoveBlip(int blip)
    {
        if (blip != 0 && Native.DoesBlipExist(blip))
        {
            Native.RemoveBlip(ref blip);
        }
    }
}
