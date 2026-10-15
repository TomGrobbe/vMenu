---
title: "Peds, vehicles and weapons"
description: "Where the Ped Models, Vehicle Spawner and Weapon Options menus get their lists from, and how to add your own addons."
---

| Menu | Where the list comes from |
| --- | --- |
| Vehicle Spawner | Detected from the game, nothing to configure. |
| Ped Models | `config/ped-models.json` |
| Weapon Options | `config/weapons.json`, with attachment names from `config/weapon-components.json` |

The game has no way to list its peds or weapons, which is why those two need a file and vehicles do not.

## Vehicles

Valid vehicles are automatically detected by the game, including addons.

Start your vehicle resources before vMenu in your `server.cfg`:

```ini
ensure my-vehicles
ensure vMenu.Enhanced
```

### Custom vehicle names

An addon without a name of its own shows up under its model name. To give it a proper name, add a small script to the vehicle's own resource:

1. In the vehicle's `vehicles.meta`, set `<gameName>` to the model name, for example `<gameName>mgt</gameName>`. Not the name you want to see.
2. Create `vehicle-names.lua` in the vehicle's resource folder:

    ```lua
    Citizen.CreateThread(function()
        AddTextEntry("mgt", "Mustang GT")
        AddTextEntry("focusrs", "Focus RS")
    end)
    ```

    One line per vehicle. The first part is the `gameName`, the second part is the name shown in the menu.
3. Add it to that resource's `fxmanifest.lua`:

    ```lua
    client_script 'vehicle-names.lua'
    ```

4. Restart the vehicle resource. Players see the new names after they reconnect.

## ped-models.json

Every ped in the Ped Models menu, grouped into categories.

```json
{
  "Animals": {
    "a_c_chop": "Chop",
    "a_c_deer": "Deer"
  },
  "My Addon Peds": {
    "my_addon_ped": "Officer Smith"
  }
}
```

| Part | Meaning |
| --- | --- |
| Category name | Shown in the menu. Each category gets its own permission. |
| Model name (left) | The ped's model name, lowercased for you. |
| Label (right) | The text shown for it. Empty means the model name is shown. |

- The order in the file is the order in the menu.
- A ped can only be in one category. The first one wins.
- An empty category is skipped.
- A listed ped the game does not know is shown locked with a note, and unlocks on its own once the resource streaming it starts.
- Addon peds go in any category, or one of your own.

## weapons.json

Every weapon in Weapon Options, grouped into categories.

```json
{
  "Handguns": {
    "weapon_pistol": "WT_PIST",
    "weapon_combatpistol": "WT_PIST_CBT"
  },
  "My Addon Weapons": {
    "weapon_myaddonrifle": "Custom Rifle"
  }
}
```

| Part | Meaning |
| --- | --- |
| Category name | Shown in the menu. Each category gets its own permission. |
| Spawn name (left) | The weapon's spawn name, lowercased for you. Letters, digits and underscores only. |
| Label (right) | A game text label such as `WT_PIST` is shown in the player's own language. Anything else is shown as written. |

- The order in the file is the order in the menu.
- A weapon can only be in one category. The first one wins.
- An empty category is skipped.
- `weapon_unarmed` is skipped, every player already has it.
- A listed weapon the game does not know is shown locked with a note, and unlocks on its own once the resource streaming it starts.

## weapon-components.json

Names for weapon attachments, such as magazines, scopes and finishes.

```json
{
  "COMPONENT_PISTOL_CLIP_01": "WCT_CLIP1",
  "COMPONENT_PISTOL_CLIP_02": "WCT_CLIP2",
  "COMPONENT_MYADDONRIFLE_SCOPE": "Holographic Sight"
}
```

| Part | Meaning |
| --- | --- |
| Component name (left) | The component's spawn name, uppercased for you. |
| Label (right) | Same rule as `weapons.json`: a game text label is translated, anything else is shown as written. |

You never say which weapon takes which component. The game is asked when a weapon's menu opens, and only the components that weapon accepts are offered. A component that is not in this file is never offered.

Components have no permissions of their own. `vMenu.Enhanced.Menus.WeaponOptions.Modify` covers attaching, removing and tints.

## Permissions

Every category in `ped-models.json` and `weapons.json` gets a permission, generated when the server starts. It is the category name in lowercase, with anything that is not a letter, a digit or an underscore turned into an underscore.

| Category | Permission |
| --- | --- |
| `Police & Sheriff` in `ped-models.json` | `vMenu.Enhanced.Menus.PedModels.Categories.police_sheriff` |
| `Handguns` in `weapons.json` | `vMenu.Enhanced.Menus.WeaponOptions.Categories.handguns` |

- Every generated permission is listed in `config/permissions.cfg.example`, marked with the file it came from.
- `vMenu.Enhanced.Menus.PedModels.Categories.All` and `vMenu.Enhanced.Menus.WeaponOptions.Categories.All` cover your own categories too.
- A category whose permission matches one vMenu already has is skipped. Pick another name.
- To hand out one ped, vehicle or weapon on its own, use [model-whitelists.json](/vmenu/enhanced/configuration/model-whitelists-json/).

## Spawn by name

| Permission | Lets a player spawn |
| --- | --- |
| `vMenu.Enhanced.Menus.VehicleSpawner.SpawnByName` | Any vehicle the game knows. Their class permission still applies. |
| `vMenu.Enhanced.Menus.PedModels.SpawnByName` | Any ped the game knows, including peds not in `ped-models.json`. |
| `vMenu.Enhanced.Menus.WeaponOptions.SpawnByName` | Any weapon the game knows, including weapons not in `weapons.json`. |

A ped or weapon that is in the file still needs its category permission when spawned by name.

## Coming from vMenu Legacy

`addons.json` is gone. Addon vehicles are detected, addon peds go in `ped-models.json`, and addon weapons go in `weapons.json` with their attachments in `weapon-components.json`.
