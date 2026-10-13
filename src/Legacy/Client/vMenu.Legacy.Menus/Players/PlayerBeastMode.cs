using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerBeastMode
{
    private const int DisablePlayerVaulting = 47;

    private const string SoundBank = "DLC_APARTMENT/APT_BEAST";

    private const string SoundSet = "APT_BvS_Soundset";

    private const string AudioScene = "GTAO_BvS_Gameplay_Scene";

    private const int AllPlayers = -1;

    private const int SoundRange = 60;

    private const float SprintIntensity = 1f;

    private const float RunIntensity = 0.7f;

    private static TickHandle? _tick;

    private static bool _bankRequested;

    private static bool _bankLoaded;

    private static bool _jumpPlayed;

    private static bool _landPlayed;

    private static bool _wasAttacking;

    private static int _sprintSound = -1;

    public static bool Enabled => UserDefaults.PlayerBeastMode.Value && PlayerSuperJump.Enabled;

    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "Player.BeastMode",
            Apply,
            TickRate.PerFrame,
            () => Enabled,
            onStopped: Silence);

        ResourceShutdown.Stopping += Silence;
    }

    public static void SetEnabled(bool enabled)
    {
        if (enabled && !PlayerSuperJump.IsAllowed)
        {
            return;
        }

        UserDefaults.PlayerBeastMode.Value = enabled;

        if (enabled)
        {
            PlayerSuperJump.SetEnabled(true);
        }

        _tick?.Reevaluate();
    }

    internal static void Reevaluate() => _tick?.Reevaluate();

    private static void Apply()
    {
        var ped = API.PlayerPedId();

        NativeFixer.SetBeastJumpThisFrame(API.PlayerId());
        API.SetPedResetFlag(ped, DisablePlayerVaulting, true);

        if (!_bankLoaded)
        {
            _bankRequested = true;
            _bankLoaded = NativeFixer.RequestScriptAudioBank(SoundBank, true, AllPlayers);

            return;
        }

        if (!API.IsAudioSceneActive(AudioScene))
        {
            API.StartAudioScene(AudioScene);
        }

        PlayJumpSounds(ped);
        PlayAttackSound(ped);
        PlaySprintSound(ped);
    }

    private static void PlayJumpSounds(int ped)
    {
        if (!NativeFixer.IsPedDoingABeastJump(ped))
        {
            _jumpPlayed = false;
            _landPlayed = false;

            return;
        }

        var landing = NativeFixer.IsPedLanding(ped);

        if (!_jumpPlayed && !landing && !API.IsPedClimbing(ped))
        {
            API.PlaySoundFromEntity(-1, "Beast_Jump", ped, SoundSet, true, SoundRange);

            _jumpPlayed = true;
        }

        if (!_landPlayed && landing && !API.IsEntityInAir(ped))
        {
            API.PlaySoundFromEntity(-1, "Beast_Jump_Land", ped, SoundSet, true, SoundRange);

            _landPlayed = true;
        }
    }

    private static void PlayAttackSound(int ped)
    {
        var attacking = API.IsPedPerformingMeleeAction(ped);

        if (attacking && !_wasAttacking)
        {
            API.PlaySoundFromEntity(-1, "Beast_Attack", ped, SoundSet, true, SoundRange);
        }

        _wasAttacking = attacking;
    }

    private static void PlaySprintSound(int ped)
    {
        var airborne = API.IsPedJumping(ped) || NativeFixer.IsPedLanding(ped) || API.IsEntityInAir(ped);
        var sprinting = API.IsPedSprinting(ped);

        if (API.IsPedInjured(ped) || airborne || !(sprinting || API.IsPedRunning(ped)))
        {
            StopSprintSound();

            return;
        }

        if (_sprintSound == -1)
        {
            _sprintSound = API.GetSoundId();
        }

        if (API.HasSoundFinished(_sprintSound))
        {
            API.PlaySoundFromEntity(_sprintSound, "Beast_Sprint_Loop", ped, SoundSet, false, 0);
        }

        API.SetVariableOnSound(_sprintSound, "Intensity", sprinting ? SprintIntensity : RunIntensity);
    }

    private static void StopSprintSound()
    {
        if (_sprintSound != -1)
        {
            API.StopSound(_sprintSound);
        }
    }

    private static void Silence()
    {
        StopSprintSound();

        if (_sprintSound != -1)
        {
            API.ReleaseSoundId(_sprintSound);

            _sprintSound = -1;
        }

        if (API.IsAudioSceneActive(AudioScene))
        {
            API.StopAudioScene(AudioScene);
        }

        if (_bankRequested)
        {
            API.ReleaseNamedScriptAudioBank(SoundBank);

            _bankRequested = false;
            _bankLoaded = false;
        }

        _jumpPlayed = false;
        _landPlayed = false;
        _wasAttacking = false;
    }
}
