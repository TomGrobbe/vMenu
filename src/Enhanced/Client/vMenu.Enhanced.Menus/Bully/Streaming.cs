using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Streaming
{
    private const int TimeoutMs = 5000;

    private const int AllPlayers = -1;

    private static readonly Dictionary<string, int> AudioBanks = new(StringComparer.Ordinal);

    private static readonly Dictionary<string, int> PtfxAssets = new(StringComparer.Ordinal);

    public static uint Hash(string name) => (uint)Native.GetHashKey(name);

    public static async Task<bool> AnimDictAsync(string dictionary)
    {
        Native.RequestAnimDict(dictionary);

        if (await WaitAsync(() => Native.HasAnimDictLoaded(dictionary)))
        {
            return true;
        }

        Native.RemoveAnimDict(dictionary);

        return false;
    }

    public static async Task<bool> ClipSetAsync(string clipSet)
    {
        Native.RequestClipSet(clipSet);

        if (await WaitAsync(() => Native.HasClipSetLoaded(clipSet)))
        {
            return true;
        }

        Native.RemoveClipSet(clipSet);

        return false;
    }

    public static async Task<bool> PtfxAsync(string asset)
    {
        Native.RequestNamedPtfxAsset(asset);

        if (!await WaitAsync(() => Native.HasNamedPtfxAssetLoaded(asset)))
        {
            Native.RemoveNamedPtfxAsset(asset);

            return false;
        }

        Hold(PtfxAssets, asset);

        return true;
    }

    public static void ReleasePtfx(string asset)
    {
        if (LetGo(PtfxAssets, asset))
        {
            Native.RemoveNamedPtfxAsset(asset);
        }
    }

    public static Task<bool> ModelAsync(string model) => ModelAsync(Hash(model));

    public static async Task<bool> ModelAsync(uint model)
    {
        if (!Native.IsModelInCdimage(model) || !Native.IsModelValid(model))
        {
            return false;
        }

        Native.RequestModel(model);

        if (await WaitAsync(() => Native.HasModelLoaded(model)))
        {
            return true;
        }

        Native.SetModelAsNoLongerNeeded(model);

        return false;
    }

    public static async Task<bool> AudioBankAsync(string bank)
    {
        if (!await WaitAsync(() => Native.RequestScriptAudioBank(bank, false, AllPlayers)))
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
            Native.ReleaseNamedScriptAudioBank(bank);
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
        var started = Native.GetGameTimer();

        while (!loaded())
        {
            if (Native.GetGameTimer() - started > TimeoutMs)
            {
                return false;
            }

            await API.Delay(0);
        }

        return true;
    }
}
