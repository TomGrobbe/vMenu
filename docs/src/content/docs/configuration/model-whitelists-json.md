---
title: "model-whitelists.json"
description: "Give single vehicles, peds or weapons a permission of their own."
---

Normally a model is covered by the permission of the category it sits in. A model listed here is taken out of its category's permission and gets one of its own. It stays in the same place in the menu, and is shown locked to everybody without that permission.

## Format

```json
{
  "vehicles": ["adder"],
  "peds": ["a_c_chop"],
  "weapons": ["weapon_rpg"]
}
```

| List | Names to use | Permission created |
| --- | --- | --- |
| `vehicles` | Vehicle model names | `vMenu.Enhanced.SupplementalPermissions.VehicleModels.adder` |
| `peds` | Ped model names, as in `ped-models.json` | `vMenu.Enhanced.SupplementalPermissions.Peds.a_c_chop` |
| `weapons` | Weapon spawn names, as in `weapons.json`, so `weapon_rpg` and not `rpg` | `vMenu.Enhanced.SupplementalPermissions.Weapons.weapon_rpg` |

- Names are lowercased for you. Letters, digits and underscores only.
- Every list is optional.
- Every created permission is listed in `config/permissions.cfg.example`.

## Granting them

| Permission | Grants |
| --- | --- |
| `vMenu.Enhanced.SupplementalPermissions.VehicleModels.All` | Every whitelisted vehicle |
| `vMenu.Enhanced.SupplementalPermissions.Peds.All` | Every whitelisted ped |
| `vMenu.Enhanced.SupplementalPermissions.Weapons.All` | Every whitelisted weapon |

Somebody with `vMenu.Enhanced.Menus.VehicleSpawner.All`, `vMenu.Enhanced.Menus.PedModels.All` or `vMenu.Enhanced.Menus.WeaponOptions.All` already has every whitelisted model of that kind.

## Errors

| Console message | Cause |
| --- | --- |
| `No config/model-whitelists.json found...` | The file is missing. Every model follows its category permission. |
| `config/model-whitelists.json could not be parsed...` | Broken JSON. Nothing is whitelisted. |
| `Skipping whitelisted ... entry '...': only letters, digits and underscores...` | The name has a space, dot or other character a permission cannot hold. |
| `'...' is listed more than once under '...'` | A duplicate. It is used once. |

Read at server start. Restart the resource after changing it.
