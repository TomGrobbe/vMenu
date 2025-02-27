using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.data
{
    public struct ValidAddonWeapon
    {
        public uint Hash;
        public string Name;
        public Dictionary<string, uint> AddonComponents;
        public Permission Perm;
        public string SpawnName;
        public readonly int GetMaxAmmo
        {
            get
            {
                var ammo = 0; GetMaxAmmo(Game.PlayerPed.Handle, Hash, ref ammo); return ammo;
            }
        }
        public int CurrentAmmo;
        public int CurrentTint;
        public readonly float Accuracy
        {
            get
            {
                var stats = new Game.WeaponHudStats(); Game.GetWeaponHudStats(Hash, ref stats); return stats.hudAccuracy;
            }
        }
        public readonly float Damage
        {
            get
            {
                var stats = new Game.WeaponHudStats(); Game.GetWeaponHudStats(Hash, ref stats); return stats.hudDamage;
            }
        }
        public readonly float Range
        {
            get
            {
                var stats = new Game.WeaponHudStats(); Game.GetWeaponHudStats(Hash, ref stats); return stats.hudRange;
            }
        }
        public readonly float Speed
        {
            get
            {
                var stats = new Game.WeaponHudStats(); Game.GetWeaponHudStats(Hash, ref stats); return stats.hudSpeed;
            }
        }
    }

    public static class ValidAddonWeapons
    {
        private static readonly List<ValidAddonWeapon> _addonWeaponsList = new();

        public static List<ValidAddonWeapon> AddonWeaponsList
        {
            get
            {
                var addons = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
                var addonsFile = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(addons);
                if (_addonWeaponsList.Count == addonsFile["weapons"].Count - 1)
                {
                    return _addonWeaponsList;
                }
                CreateAddonWeaponsList();
                return _addonWeaponsList;
            }
        }

        private static void CreateAddonWeaponsList()
        {
            _addonWeaponsList.Clear();
            var addons = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
            var addonsFile = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(addons);
            if (addonsFile.ContainsKey("weapons"))
            {
                var keyValuePairs = addonsFile["weapons"]
                    .ToDictionary(key => key, key => GetLabelText(key));
                foreach (var addonWeapon in keyValuePairs)
                {
                    var realName = addonWeapon.Key;
                    var localizedName = addonWeapon.Value;
                    if (realName == "weapon_unarmed") continue;
                    var hash = (uint)GetHashKey(realName);
                    var componentHashes = new Dictionary<string, uint>();
                    var weaponComponents = ValidWeapons.GetWeaponComponents();
                    foreach (var comp in weaponComponents.Keys)
                    {
                        uint componentHash = (uint)GetHashKey(comp);
                        if (DoesWeaponTakeWeaponComponent(hash, componentHash))
                        {
                            string componentName = weaponComponents[comp];
                            if (!componentHashes.ContainsKey(componentName))
                            {
                                componentHashes[componentName] = componentHash;
                            }
                        }
                    }
                    var avw = new ValidAddonWeapon
                    {
                        Hash = hash,
                        SpawnName = realName,
                        Name = localizedName,
                        AddonComponents = componentHashes,
                    };
                    if (!_addonWeaponsList.Contains(avw))
                    {
                        _addonWeaponsList.Add(avw);
                    }
                }
            }
        }
    }
}

