using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

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
        API.SetPedRelationshipGroupHash(ped, HatesPlayer);

        foreach (var attribute in HostileAttributes)
        {
            API.SetPedCombatAttributes(ped, attribute, true);
        }

        API.SetPedConfigFlag(ped, ShouldChargeNow, true);
        API.SetPedTargetLossResponse(ped, NeverLoseTarget);
        API.SetPedCombatMovement(ped, WillAdvance);
        API.SetPedKeepTask(ped, true);
    }

    public static void MakeFleeFrom(int ped, int from)
    {
        API.SetPedCombatAttributes(ped, AlwaysFlee, true);
        API.TaskSmartFleePed(ped, from, 300f, -1, false, false);
    }

    public static void Attack(int ped, int target) => API.TaskCombatPed(ped, target, 0, 16);

    public static int AddHostileBlip(int entity)
    {
        var blip = API.AddBlipForEntity(entity);

        API.SetBlipColour(blip, HostileBlipColour);
        API.SetBlipScale(blip, HostileBlipScale);

        return blip;
    }

    public static void RemoveBlip(int blip)
    {
        if (blip != 0 && API.DoesBlipExist(blip))
        {
            NativeFixer.RemoveBlip(ref blip);
        }
    }
}
