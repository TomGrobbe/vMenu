---
title: "blips.json"
description: "Put named markers on everybody's map, for police stations, shops, or anything else worth pointing at."
---

`blips.json` is a list of markers you want on the map for everybody on your server. Each one gets an icon, a colour and a name, and shows up on the pause map and minimap like any other marker.

Coming from vMenu Legacy, this is the half of the old `locations.json` that put blips on the map. The teleport half now lives in `teleport-categories.json`.

## Where it lives

`resources/vMenu.Enhanced/config/blips.json`

The server reads it once at startup and sends the list to every player as they join. Staff can also add and remove blips from inside the menu, which rewrites the file and tells everybody straight away, so nobody has to rejoin.

:::caution
When the menu writes this file, any `//` comments you left in it are lost.
:::

## The two lists

| List | What it is for |
| --- | --- |
| `alwaysOn` | Always drawn. Players cannot switch these off. Use it for things people genuinely need to find. |
| `toggleable` | Drawn only while the player has **Location Blips** ticked in Display Settings. Use it for everything else. |

That split is the point of the file. You can cover the map in useful markers without forcing all of them on people who would rather have a clean map.

## How to write it

```json
{
  "alwaysOn": [
    {
      "name": "Police Station",
      "sprite": 60,
      "colour": 29,
      "scaleOffset": 0.0,
      "shortRange": true,
      "x": 425.13,
      "y": -979.55,
      "z": 30.71
    }
  ],
  "toggleable": [
    {
      "name": "Ammu-Nation",
      "sprite": 110,
      "colour": 1,
      "scaleOffset": -0.1,
      "shortRange": true,
      "x": 22.09,
      "y": -1107.28,
      "z": 29.8
    }
  ]
}
```

| Field | What it does |
| --- | --- |
| `name` | The text beside the marker. Shown exactly as you type it, never translated. |
| `sprite` | Which icon to draw. There is a full list of sprite ids on the FiveM documentation site. |
| `colour` | Which colour the icon is drawn in. |
| `scaleOffset` | Added to the normal size. `0` is normal, `-0.1` slightly smaller, `0.2` slightly bigger. |
| `shortRange` | `true` means it only appears once you are near it, like most of the game's own markers. `false` means always on the big map. |
| `x`, `y`, `z` | Where it goes. |

Both lists are optional. Leave one out entirely if you do not need it.

You can leave `//` comments in this file, and a trailing comma will not break it, the same as the other config files.

## Adding blips without editing the file

Staff with the `vMenu.Enhanced.Menus.DisplaySettings.ManageBlips` permission get **Manage Blips** in Display Settings:

- **Add Always On Blip Here** puts a blip where you are standing, in the `alwaysOn` list.
- **Add Toggleable Blip Here** does the same, in the `toggleable` list.
- **Remove Nearest Blip** takes the closest blip off the map.

Adding one asks for the name, sprite id, colour id and size offset. The position comes from wherever you are standing, which is usually easier than looking coordinates up. Everything is written back to `blips.json` and sent to every player immediately.

## When something is wrong with it

A problem with one blip only costs you that blip. The rest still work, and the server console says what it skipped:

| What you will see | What happened |
| --- | --- |
| `No config/blips.json found, so no map blips are added.` | The file is missing. That is fine, it is optional. |
| `config/blips.json could not be parsed...` | The JSON is broken, usually a missing comma or bracket. |
| `Skipping a blip in 'alwayson': it has no name.` | A blip with no `name`. |
| `'X' is listed more than once in 'toggleable'...` | Two blips with the same name in one list. The first wins. |

A sprite, colour or size that is out of range is not an error. It is pulled back to the nearest sensible value.

## Permissions

| Permission | What it allows |
| --- | --- |
| `vMenu.Enhanced.Menus.DisplaySettings.LocationBlips` | Seeing the **Location Blips** toggle, which controls the `toggleable` list. |
| `vMenu.Enhanced.Menus.DisplaySettings.ManageBlips` | Adding and removing blips from the menu. Staff only by default. |

Blips in `alwaysOn` need no permission. Everybody sees them, which is what "always on" means.
