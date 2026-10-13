
using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Bullying;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

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
        var victim = API.PlayerPedId();

        if (_active)
        {
            run.Busy();

            return;
        }

        if (!API.IsPedOnFoot(victim))
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
            var position = API.GetEntityCoords(victim, false);

            if (SpawnSpots.FindSafeSpotAround(position, MinDistance, MaxDistance, hidden: true) is not { } spot)
            {
                run.Failed();

                return;
            }

            clone = NativeFixer.ClonePed(victim, false, false, true);

            if (clone == 0)
            {
                run.Failed();

                return;
            }

            run.Started();

            API.SetEntityCoords(clone, spot.X, spot.Y, spot.Z, false, false, false, false);
            API.SetEntityHeading(clone, Fx.HeadingTowards(spot, position));
            API.SetPedCanRagdoll(clone, false);
            API.DisablePedPainAudio(clone, true);
            API.StopPedSpeaking(clone, true);
            API.SetAmbientVoiceName(clone, "NO_VOICE");
            API.SetPedDropsWeaponsWhenDead(clone, false);
            API.SetPedSuffersCriticalHits(clone, false);
            API.SetPedMaxHealth(clone, CloneHealth);
            NativeFixer.SetEntityHealth(clone, CloneHealth, 0, 0);
            API.SetPedArmour(clone, 100);

            var weapon = Streaming.Hash(Weapons[Dice.Next(Weapons.Length)]);

            API.GiveWeaponToPed(clone, weapon, 1, true, true);
            API.SetCurrentPedWeapon(clone, weapon, true);

            HostilePeds.MakeHostile(clone);
            HostilePeds.Attack(clone, victim);

            bank = await Streaming.AudioBankAsync(Bank);

            if (bank)
            {
                API.PlaySoundFrontend(-1, "FE_Spawn", SoundSet, true);
                API.PlaySoundFromEntity(-1, "Ingame_Spawn", clone, SoundSet, false, 0);
            }

            var endsAt = API.GetGameTimer() + LifetimeMs;

            while (stopAllCount == BullyState.StopAllCount && API.GetGameTimer() < endsAt)
            {
                if (API.PlayerPedId() != victim
                    || API.IsPedDeadOrDying(victim, true)
                    || !API.DoesEntityExist(clone)
                    || Vector3.Distance(API.GetEntityCoords(clone, false), API.GetEntityCoords(victim, false)) > LeashDistance)
                {
                    break;
                }

                if (API.IsPedDeadOrDying(clone, true))
                {
                    await BaseScript.Delay(CorpseMs);

                    break;
                }

                await BaseScript.Delay(CheckMs);
            }

            if (bank)
            {
                API.PlaySoundFrontend(-1, "FE_Despawn", SoundSet, true);
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
        if (clone == 0 || !API.DoesEntityExist(clone))
        {
            return;
        }

        for (var alpha = 255; alpha > 0; alpha -= 15)
        {
            NativeFixer.SetEntityAlpha(clone, alpha, false);

            await BaseScript.Delay(0);
        }

        NativeFixer.DeleteEntity(ref clone);
    }
}
