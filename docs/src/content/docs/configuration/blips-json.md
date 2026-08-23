---
title: "blips.json"
description: "Put named markers on everybody's map, for police stations, shops, or anything else worth pointing at."
---

## What this file is for

`blips.json` is a list of markers you want on the map for everybody on your server. A hospital, a
police station, the garage you want people to find, your own custom locations. Each one gets an icon,
a colour and a name, and it shows up on the pause map and the minimap like any other map marker.

If you used vMenu Legacy, this is the half of the old `locations.json` that put blips on the map. The
teleport half of that file lives in `teleport-categories.json` now, and this is where the blips went.

## Where it lives

`resources/vMenu.Enhanced/config/blips.json`

The server reads it once when it starts, and sends the list to every player as they join. Staff can
also add and remove blips from inside the menu, which writes the file again and tells everybody
straight away, so nobody has to rejoin.

:::caution
When the menu writes this file, any `//` comments you left in it are lost. The comments in the file
that ships with vMenu Enhanced are there to explain it, not to be kept forever.
:::

## The two lists

The file has two lists, and the difference between them is who gets to hide them.

| List | What it is for |
| --- | --- |
| `alwaysOn` | Always drawn. A player cannot switch these off. Use it for things people genuinely need to find. |
| `toggleable` | Drawn only while the player has **Location Blips** ticked in the Display Settings menu. Use it for everything else. |

That split is the point of the file. It means you can cover the map in useful markers without forcing
every one of them on the people who would rather have a clean map.

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

What each part means:

| Field | What it does |
| --- | --- |
| `name` | The text shown beside the marker. It goes on the map exactly as you type it, and is never translated. |
| `sprite` | Which icon to draw. There is a full list of sprite ids on the FiveM documentation site. |
| `colour` | Which colour the icon is drawn in. |
| `scaleOffset` | Added to the normal size. `0` is normal, `-0.1` is slightly smaller, `0.2` is slightly bigger. |
| `shortRange` | `true` means it only appears once you are near it, which is what most of the game's own map markers do. `false` means it is always on the big map. |
| `x`, `y`, `z` | Where it goes. |

Both lists are optional. If you only want always on blips, leave `toggleable` as an empty list, or
leave it out entirely.

## Adding blips without editing the file

Staff with the `vMenu.Enhanced.Menus.DisplaySettings.ManageBlips` permission get a **Manage Blips**
option in the Display Settings menu. It has three things in it:

- **Add Always On Blip Here** puts a blip where you are standing, in the `alwaysOn` list.
- **Add Toggleable Blip Here** does the same, in the `toggleable` list.
- **Remove Nearest Blip** takes the closest blip to you off the map.

Adding one asks you for the name, the sprite id, the colour id and the size offset. The position
comes from wherever you happen to be standing, which is usually easier than looking coordinates up.

Everything you do here is written back to `blips.json` and sent to every player on the server
immediately.

## Comments are allowed

You can leave `//` comments in this file and a trailing comma will not break it, the same as with the
other config files. Just remember that the menu rewrites the file without them.

## When something is wrong with it

A problem with one blip only costs you that blip. The rest still work, and the server console says
what it skipped:

| What you will see | What happened |
| --- | --- |
| `No config/blips.json found, so no map blips are added.` | The file is missing. That is fine, it is optional. |
| `config/blips.json could not be parsed...` | The JSON itself is broken, usually a missing comma or bracket. |
| `Skipping a blip in 'alwayson': it has no name.` | A blip with no `name`. |
| `'X' is listed more than once in 'toggleable'...` | Two blips with the same name in one list. The first wins. |

A sprite, colour or size that is out of range is not an error. It gets pulled back to the nearest
sensible value, because losing a whole blip over a typo in one number helps nobody.

## Permissions

| Permission | What it allows |
| --- | --- |
| `vMenu.Enhanced.Menus.DisplaySettings.LocationBlips` | Seeing the **Location Blips** toggle, which controls the `toggleable` list. |
| `vMenu.Enhanced.Menus.DisplaySettings.ManageBlips` | Adding and removing blips from the menu. Staff only by default. |

Blips in `alwaysOn` need no permission at all. Everybody sees them, which is what "always on" means.
