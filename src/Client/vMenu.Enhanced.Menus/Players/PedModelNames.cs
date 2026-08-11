using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>
/// Turns a ped model hash back into the name it was spawned from.
/// </summary>
/// <remarks>
/// The game has no reverse lookup for a ped model, so the only names available are the ones the
/// server owner listed in <c>config/ped-models.json</c>. A hash from outside that list stays a hash,
/// which is honest: guessing at a name nothing could confirm would be worse than admitting the model
/// has none here.
/// </remarks>
public static class PedModelNames
{
    /// <summary>The model's name, or an empty string when this client cannot name it.</summary>
    public static string Resolve(uint hash)
    {
        foreach (var category in PedModelSync.Categories)
        {
            foreach (var ped in category.Peds)
            {
                if (API.Hash(ped.Model) == hash)
                {
                    return ped.Model;
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// The model's name, falling back to one recorded earlier and then to the hash itself, so a row
    /// always has something to show.
    /// </summary>
    public static string Resolve(uint hash, string stored)
    {
        if (Resolve(hash) is { Length: > 0 } known)
        {
            return known;
        }

        return stored.Length > 0 ? stored : hash.ToString();
    }
}
