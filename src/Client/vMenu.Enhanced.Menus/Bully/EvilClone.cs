using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;

namespace vMenu.Enhanced.Menus.Bully;

internal static class EvilClone
{
    private const string Bank = "DLC_MPSUM2/Mirror_Slash";

    private const string SoundSet = "Freemode_Mirror_Slash_sounds";

    private const float MinDistance = 25f;

    private const float MaxDistance = 40f;

    private const float LeashDistance = 1000f;

    private const int LifetimeMs = 3 * 60 * 1000;

    private const int CorpseMs = 4000;

    private const int CheckMs = 250;

    private const int CloneHealth = 2000;

    private static readonly string[] Weapons =
    [
        "WEAPON_HATCHET",
        "WEAPON_BAT",
        "WEAPON_MACHETE",
        "WEAPON_WRENCH",
    ];

    private static bool _active;

    public static async Task Start(BullyRun run)
    {
        var victim = Native.PlayerPedId();

        if (_active)
        {
            run.Busy();

            return;
        }

        if (!Native.IsPedOnFoot(victim))
        {
            run.Skip(BullyEvents.RefusedOnFoot);

            return;
        }

        _active = true;

        var clone = 0;
        var bank = false;

        try
        {
            var stopAllCount = BullyState.StopAllCount;
            var position = Native.GetEntityCoords(victim, false);

            if (SpawnSpots.FindSafeSpotAround(position, MinDistance, MaxDistance, hidden: true) is not { } spot)
            {
                run.Failed();

                return;
            }

            clone = Native.ClonePed(victim, false, false, true);

            if (clone == 0)
            {
                run.Failed();

                return;
            }

            run.Started();

            Native.SetEntityCoords(clone, spot.X, spot.Y, spot.Z, false, false, false, false);
            Native.SetEntityHeading(clone, Fx.HeadingTowards(spot, position));
            Native.SetPedCanRagdoll(clone, false);
            Native.DisablePedPainAudio(clone, true);
            Native.StopPedSpeaking(clone, true);
            Native.SetAmbientVoiceName(clone, "NO_VOICE");
            Native.SetPedDropsWeaponsWhenDead(clone, false);
            Native.SetPedSuffersCriticalHits(clone, false);
            Native.SetPedMaxHealth(clone, CloneHealth);
            Native.SetEntityHealth(clone, CloneHealth, 0, 0);
            Native.SetPedArmour(clone, 100);

            var weapon = Streaming.Hash(Weapons[Dice.Next(Weapons.Length)]);

            Native.GiveWeaponToPed(clone, weapon, 1, true, true);
            Native.SetCurrentPedWeapon(clone, weapon, true);

            HostilePeds.MakeHostile(clone);
            HostilePeds.Attack(clone, victim);

            bank = await Streaming.AudioBankAsync(Bank);

            if (bank)
            {
                Native.PlaySoundFrontend(-1, "FE_Spawn", SoundSet, true);
                Native.PlaySoundFromEntity(-1, "Ingame_Spawn", clone, SoundSet, false, 0);
            }

            var endsAt = Native.GetGameTimer() + LifetimeMs;

            while (stopAllCount == BullyState.StopAllCount && Native.GetGameTimer() < endsAt)
            {
                if (Native.PlayerPedId() != victim
                    || Native.IsPedDeadOrDying(victim, true)
                    || !Native.DoesEntityExist(clone)
                    || Vector3.Distance(Native.GetEntityCoords(clone, false), Native.GetEntityCoords(victim, false)) > LeashDistance)
                {
                    break;
                }

                if (Native.IsPedDeadOrDying(clone, true))
                {
                    await API.Delay(CorpseMs);

                    break;
                }

                await API.Delay(CheckMs);
            }

            if (bank)
            {
                Native.PlaySoundFrontend(-1, "FE_Despawn", SoundSet, true);
            }
        }
        finally
        {
            await FadeOutAsync(clone);

            if (bank)
            {
                Streaming.ReleaseAudioBank(Bank);
            }

            _active = false;
        }
    }

    private static async Task FadeOutAsync(int clone)
    {
        if (clone == 0 || !Native.DoesEntityExist(clone))
        {
            return;
        }

        for (var alpha = 255; alpha > 0; alpha -= 15)
        {
            Native.SetEntityAlpha(clone, alpha, false);

            await API.Delay(0);
        }

        Native.DeleteEntity(ref clone);
    }
}
