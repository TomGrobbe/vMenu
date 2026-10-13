using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Streaming
{
    private const int TimeoutMs = 5000;

    private const int AllPlayers = -1;

    private static readonly Dictionary<string, int> AudioBanks = new(StringComparer.Ordinal);

    private static readonly Dictionary<string, int> PtfxAssets = new(StringComparer.Ordinal);

    public static uint Hash(string name) => (uint)API.GetHashKey(name);

    public static async Task<bool> AnimDictAsync(string dictionary)
    {
        API.RequestAnimDict(dictionary);

        if (await WaitAsync(() => API.HasAnimDictLoaded(dictionary)))
        {
            return true;
        }

        API.RemoveAnimDict(dictionary);

        return false;
    }

    public static async Task<bool> ClipSetAsync(string clipSet)
    {
        API.RequestClipSet(clipSet);

        if (await WaitAsync(() => API.HasClipSetLoaded(clipSet)))
        {
            return true;
        }

        API.RemoveClipSet(clipSet);

        return false;
    }

    public static async Task<bool> PtfxAsync(string asset)
    {
        API.RequestNamedPtfxAsset(asset);

        if (!await WaitAsync(() => API.HasNamedPtfxAssetLoaded(asset)))
        {
            API.RemoveNamedPtfxAsset(asset);

            return false;
        }

        Hold(PtfxAssets, asset);

        return true;
    }

    public static void ReleasePtfx(string asset)
    {
        if (LetGo(PtfxAssets, asset))
        {
            API.RemoveNamedPtfxAsset(asset);
        }
    }

    public static Task<bool> ModelAsync(string model) => ModelAsync(Hash(model));

    public static async Task<bool> ModelAsync(uint model)
    {
        if (!API.IsModelInCdimage(model) || !API.IsModelValid(model))
        {
            return false;
        }

        API.RequestModel(model);

        if (await WaitAsync(() => API.HasModelLoaded(model)))
        {
            return true;
        }

        API.SetModelAsNoLongerNeeded(model);

        return false;
    }

    public static async Task<bool> AudioBankAsync(string bank)
    {
        if (!await WaitAsync(() => NativeFixer.RequestScriptAudioBank(bank, false, AllPlayers)))
        {
            return false;
        }

        Hold(AudioBanks, bank);

        return true;
    }

    public static void ReleaseAudioBank(string bank)
    {
        if (LetGo(AudioBanks, bank))
        {
            API.ReleaseNamedScriptAudioBank(bank);
        }
    }

    private static void Hold(Dictionary<string, int> holds, string name) =>
        holds[name] = holds.TryGetValue(name, out var count) ? count + 1 : 1;

    private static bool LetGo(Dictionary<string, int> holds, string name)
    {
        if (!holds.TryGetValue(name, out var count))
        {
            return false;
        }

        if (count > 1)
        {
            holds[name] = count - 1;

            return false;
        }

        holds.Remove(name);

        return true;
    }

    private static async Task<bool> WaitAsync(Func<bool> loaded)
    {
        var started = API.GetGameTimer();

        while (!loaded())
        {
            if (API.GetGameTimer() - started > TimeoutMs)
            {
                return false;
            }

            await BaseScript.Delay(0);
        }

        return true;
    }
}
