using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerActions
{
    public const int ArmorTiers = 5;

    private const float Soaked = 2f;

    private const float Dry = 0f;

    private const int DamageZones = 6;

    private const string AllDecals = "ALL";

    public static void Heal()
    {
        var ped = API.PlayerPedId();

        NativeFixer.SetEntityHealth(ped, API.GetEntityMaxHealth(ped), 0, 0);

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.HealPlayerDone));
    }

    public static void CommitSuicide()
    {
        var player = API.PlayerId();
        var ped = API.PlayerPedId();

        // God mode would swallow the health write. Both flags, since either half could be live.
        API.SetPlayerInvincibleKeepRagdollEnabled(player, false);
        NativeFixer.SetEntityInvincible(ped, false, false);

        NativeFixer.SetEntityHealth(ped, 0, 0, 0);

        PlayerGodMode.Reapply();
    }

    public static int ArmorTier()
    {
        var player = API.PlayerId();
        var max = API.GetPlayerMaxArmour(player);

        if (max <= 0)
        {
            return 0;
        }

        var worn = API.GetPedArmour(API.PlayerPedId());

        // Rounded, so armour a point under a tier still reads as that tier.
        return Math.Clamp((int)MathF.Round(worn * (float)ArmorTiers / max), 0, ArmorTiers);
    }

    public static void SetArmorTier(int tier)
    {
        var player = API.PlayerId();
        var max = API.GetPlayerMaxArmour(player);

        API.SetPedArmour(API.PlayerPedId(), Math.Clamp(tier, 0, ArmorTiers) * max / ArmorTiers);
    }

    public static void ApplyDamagePack(int index)
    {
        if (index < 0 || index >= PedDamagePacks.Names.Length)
        {
            return;
        }

        API.ApplyPedDamagePack(API.PlayerPedId(), PedDamagePacks.Names[index], 100f, 100f);
    }

    public static void ClearBlood()
    {
        var ped = API.PlayerPedId();

        API.ClearPedBloodDamage(ped);
        NativeFixer.ResetPedVisibleDamage(ped);

        // Bruises and scars are decals filed per body part, and no native clears them all at once.
        for (var zone = 0; zone < DamageZones; zone++)
        {
            API.ClearPedDamageDecalByZone(ped, zone, AllDecals);
        }

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.ClearBloodDone));
    }

    public static void CleanClothes()
    {
        API.ClearPedBloodDamage(API.PlayerPedId());

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.CleanPlayerDone));
    }

    public static void DryClothes()
    {
        API.SetPedWetnessHeight(API.PlayerPedId(), Dry);

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.DryPlayerDone));
    }

    public static void WetClothes()
    {
        API.SetPedWetnessHeight(API.PlayerPedId(), Soaked);

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.WetPlayerDone));
    }

    public static int WantedLevel() => API.GetPlayerWantedLevel(API.PlayerId());

    public static void SetWantedLevel(int stars)
    {
        var player = API.PlayerId();

        API.SetPlayerWantedLevel(player, stars, false);

        API.SetPlayerWantedLevelNow(player, false);
    }
}
