using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Menus.Players.Character;

public static class CharacterRespawn
{
    private const int VisibleTimeoutMs = 60000;

    private const int VisibleCheckMs = 100;

    private static bool _appliedOnJoin;

    private static MpCharacterEntry? Chosen
    {
        get
        {
            var name = UserDefaults.DefaultCharacterName.Value;

            return name.Length == 0 ? null : MpCharacterStore.Load(name);
        }
    }

    private static bool IsAllowed => ClientPermissions.IsAllowed(CharacterCreatorPermissions.Spawn);

    public static void Initialize() => LocalPlayerTicks.PlayerPedRevivedAsync += OnRevivedAsync;

    public static async Task ApplyOnJoinAsync()
    {
        if (_appliedOnJoin)
        {
            return;
        }

        _appliedOnJoin = true;

        await ApplyAsync("on join");
    }

    private static async Task OnRevivedAsync(PlayerPedRevived revived)
    {
        if (!revived.Respawned)
        {
            return;
        }

        await ApplyAsync("after respawn");
    }

    private static async Task ApplyAsync(string when)
    {
        if (!IsAllowed || Chosen is not { } entry)
        {
            return;
        }

        var character = entry.Character;

        await WaitUntilVisibleAsync();

        if (!await PedSpawning.SetPlayerModelAsync(PedSpawning.FreemodeModel(character.Core.IsMale)))
        {
            Log.Warning($"[Character] '{character.Name}' could not be applied {when}: its model would not load.");

            return;
        }

        var style = character.StyleNamed(character.LastStyle)
            ?? (character.Styles.Count > 0 ? character.Styles[0] : null);

        var outfit = character.OutfitNamed(character.LastOutfit)
            ?? (character.Outfits.Count > 0 ? character.Outfits[0] : null);

        await FreemodeWriter.ApplyAsync(Native.PlayerPedId(), character, style, outfit);

        MpCharacterState.Wearing(character, style, outfit, entry);

        Appearance.PedHeadFit.Forget();
        Appearance.Torso.TorsoFit.Forget();

        Log.Debug($"[Character] Applied '{character.Name}' {when}.");
    }

    private static async Task WaitUntilVisibleAsync()
    {
        var deadline = Native.GetGameTimer() + VisibleTimeoutMs;

        while (!Native.NetworkIsSessionStarted()
            || Native.IsPlayerSwitchInProgress()
            || !Native.IsScreenFadedIn()
            || Native.IsEntityDead(Native.PlayerPedId(), false))
        {
            if (Native.GetGameTimer() > deadline)
            {
                return;
            }

            await API.Delay(VisibleCheckMs);
        }
    }
}
