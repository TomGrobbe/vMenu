using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

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
        var ped = Native.PlayerPedId();

        Native.SetEntityHealth(ped, Native.GetEntityMaxHealth(ped), 0, 0);

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.HealPlayerDone));
    }

    public static int ArmorTier()
    {
        var player = Native.PlayerId();
        var max = Native.GetPlayerMaxArmour(player);

        if (max <= 0)
        {
            return 0;
        }

        var worn = Native.GetPedArmour(Native.PlayerPedId());

        // Rounded, so armour a point under a tier still reads as that tier.
        return Math.Clamp((int)MathF.Round(worn * (float)ArmorTiers / max), 0, ArmorTiers);
    }

    public static void SetArmorTier(int tier)
    {
        var player = Native.PlayerId();
        var max = Native.GetPlayerMaxArmour(player);

        Native.SetPedArmour(Native.PlayerPedId(), Math.Clamp(tier, 0, ArmorTiers) * max / ArmorTiers);
    }

    public static void ApplyDamagePack(int index)
    {
        if (index < 0 || index >= PedDamagePacks.Names.Length)
        {
            return;
        }

        Native.ApplyPedDamagePack(Native.PlayerPedId(), PedDamagePacks.Names[index], 100f, 100f);
    }

    public static void ClearBlood()
    {
        var ped = Native.PlayerPedId();

        Native.ClearPedBloodDamage(ped);
        Native.ResetPedVisibleDamage(ped);

        // Bruises and scars are decals filed per body part, and no native clears them all at once.
        for (var zone = 0; zone < DamageZones; zone++)
        {
            Native.ClearPedDamageDecalByZone(ped, zone, AllDecals);
        }

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.ClearBloodDone));
    }

    public static void CleanClothes()
    {
        Native.ClearPedBloodDamage(Native.PlayerPedId());

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.CleanPlayerDone));
    }

    public static void DryClothes()
    {
        Native.SetPedWetnessHeight(Native.PlayerPedId(), Dry);

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.DryPlayerDone));
    }

    public static void WetClothes()
    {
        Native.SetPedWetnessHeight(Native.PlayerPedId(), Soaked);

        Notifications.Success(MenuText.Key(Loc.PlayerOptions.WetPlayerDone));
    }

    public static int WantedLevel() => Native.GetPlayerWantedLevel(Native.PlayerId());

    public static void SetWantedLevel(int stars)
    {
        var player = Native.PlayerId();

        Native.SetPlayerWantedLevel(player, stars, false);

        Native.SetPlayerWantedLevelNow(player, false);
    }
}
