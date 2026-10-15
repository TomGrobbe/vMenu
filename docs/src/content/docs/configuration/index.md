---
title: "Configuration files"
description: "Every file in the vMenu config folder and what it is for."
---

All of vMenu's configuration files are in `resources/vMenu.Enhanced/config/`.

## Permissions and settings

| File | What it is for |
| --- | --- |
| `permissions.cfg` | Who may use which part of the menu. |
| `configuration.cfg` | Which features are turned on, and how they behave. |

You make these yourself by copying `permissions.cfg.example` and `configuration.cfg.example`, see the install guide for [FiveM Enhanced](/vmenu/enhanced/installing/fivem-enhanced/) or [FiveM Legacy](/vmenu/enhanced/installing/fivem-legacy/). Do not edit the `.example` files. vMenu rewrites them on every start.

## Lists and content

| File | What it is for |
| --- | --- |
| [announcements.json](/vmenu/enhanced/configuration/announcements-json/) | Messages that are sent to everybody automatically. |
| [blips.json](/vmenu/enhanced/configuration/blips-json/) | Markers on the map. |
| [clothing-presets.json](/vmenu/enhanced/configuration/clothing-presets-json/) | Outfits everybody can wear. |
| [extras.json](/vmenu/enhanced/configuration/extras-json/) | Names for vehicle extras. |
| [model-whitelists.json](/vmenu/enhanced/configuration/model-whitelists-json/) | Single vehicles, peds or weapons that need their own permission. |
| [ped-models.json](/vmenu/enhanced/configuration/peds-vehicles-weapons/#ped-modelsjson) | The peds players can turn into. |
| [tattoos.json](/vmenu/enhanced/configuration/tattoos-json/) | The tattoos in the character creator. |
| [teleport-categories.json](/vmenu/enhanced/configuration/teleport-categories-json/) | Saved teleport locations. |
| [vehicle-categories.json](/vmenu/enhanced/configuration/vehicle-categories-json/) | Your own categories in the Vehicle Spawner. |
| [walking-styles.json](/vmenu/enhanced/configuration/walking-styles-json/) | The walking styles players can pick. |
| [weapon-components.json](/vmenu/enhanced/configuration/peds-vehicles-weapons/#weapon-componentsjson) | Names for weapon attachments. |
| [weapons.json](/vmenu/enhanced/configuration/peds-vehicles-weapons/#weaponsjson) | The weapons players can get. |
| [weather-cycle.json](/vmenu/enhanced/configuration/weather-cycle-json/) | Your own weather schedule. |

There is no file for vehicles. vMenu finds them in the game by itself, see [Peds, vehicles and weapons](/vmenu/enhanced/configuration/peds-vehicles-weapons/).

## After you edit a file

Restart the resource. Changes only show up after that.

Two exceptions:

| File | When changes show up |
| --- | --- |
| `weather-cycle.json` | When you change a weather setting, or restart the resource. |
| `tattoos.json` | When a player reconnects. |

## Editing from inside the menu

Staff can change these files from inside the menu, without restarting anything:

- `announcements.json`
- `blips.json`
- `clothing-presets.json`
- `teleport-categories.json`

When the menu saves one of them, any `//` comments you wrote in that file are removed. For the menu to save at all, your `server.cfg` needs this line:

```ini
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
```

## Mistakes in a file

A mistake only breaks the entry it is in. That entry is skipped, the rest still works, and the server console tells you which entry and why. For `tattoos.json` that message is in the player's F8 console instead.

`//` comments and a comma after the last item are fine in every file.

## Updating vMenu

A new release comes with its own copies of these files. Back up the files you changed before you update, and put them back afterwards.
