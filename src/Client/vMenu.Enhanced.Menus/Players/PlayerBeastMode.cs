using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

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
        var ped = Native.PlayerPedId();

        Native.SetBeastJumpThisFrame(Native.PlayerId());
        Native.SetPedResetFlag(ped, DisablePlayerVaulting, true);

        if (!_bankLoaded)
        {
            _bankRequested = true;
            _bankLoaded = Native.RequestScriptAudioBank(SoundBank, true, AllPlayers);

            return;
        }

        if (!Native.IsAudioSceneActive(AudioScene))
        {
            Native.StartAudioScene(AudioScene);
        }

        PlayJumpSounds(ped);
        PlayAttackSound(ped);
        PlaySprintSound(ped);
    }

    private static void PlayJumpSounds(int ped)
    {
        if (!Native.IsPedDoingABeastJump(ped))
        {
            _jumpPlayed = false;
            _landPlayed = false;

            return;
        }

        var landing = Native.IsPedLanding(ped);

        if (!_jumpPlayed && !landing && !Native.IsPedClimbing(ped))
        {
            Native.PlaySoundFromEntity(-1, "Beast_Jump", ped, SoundSet, true, SoundRange);

            _jumpPlayed = true;
        }

        if (!_landPlayed && landing && !Native.IsEntityInAir(ped))
        {
            Native.PlaySoundFromEntity(-1, "Beast_Jump_Land", ped, SoundSet, true, SoundRange);

            _landPlayed = true;
        }
    }

    private static void PlayAttackSound(int ped)
    {
        var attacking = Native.IsPedPerformingMeleeAction(ped);

        if (attacking && !_wasAttacking)
        {
            Native.PlaySoundFromEntity(-1, "Beast_Attack", ped, SoundSet, true, SoundRange);
        }

        _wasAttacking = attacking;
    }

    private static void PlaySprintSound(int ped)
    {
        var airborne = Native.IsPedJumping(ped) || Native.IsPedLanding(ped) || Native.IsEntityInAir(ped);
        var sprinting = Native.IsPedSprinting(ped);

        if (Native.IsPedInjured(ped) || airborne || !(sprinting || Native.IsPedRunning(ped)))
        {
            StopSprintSound();

            return;
        }

        if (_sprintSound == -1)
        {
            _sprintSound = Native.GetSoundId();
        }

        if (Native.HasSoundFinished(_sprintSound))
        {
            Native.PlaySoundFromEntity(_sprintSound, "Beast_Sprint_Loop", ped, SoundSet, false, 0);
        }

        Native.SetVariableOnSound(_sprintSound, "Intensity", sprinting ? SprintIntensity : RunIntensity);
    }

    private static void StopSprintSound()
    {
        if (_sprintSound != -1)
        {
            Native.StopSound(_sprintSound);
        }
    }

    private static void Silence()
    {
        StopSprintSound();

        if (_sprintSound != -1)
        {
            Native.ReleaseSoundId(_sprintSound);

            _sprintSound = -1;
        }

        if (Native.IsAudioSceneActive(AudioScene))
        {
            Native.StopAudioScene(AudioScene);
        }

        if (_bankRequested)
        {
            Native.ReleaseNamedScriptAudioBank(SoundBank);

            _bankRequested = false;
            _bankLoaded = false;
        }

        _jumpPlayed = false;
        _landPlayed = false;
        _wasAttacking = false;
    }
}
