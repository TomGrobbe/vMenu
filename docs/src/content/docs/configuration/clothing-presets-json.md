---
title: "clothing-presets.json"
description: "Outfits you publish for everybody, such as a set of police uniforms."
---

The list behind **Outfit Presets, Server Presets** in the [character creator](/vmenu/enhanced/character-creator/). It ships empty as `[]`.

## Making presets in game

The easiest way. Build the outfit in the character creator, then use **Outfit Presets, Server Presets, Save Clothes To Server Presets**. vMenu writes the file and sends the new list to every player, so nothing needs a restart.

## Format

```json
[
  {
    "name": "Police",
    "description": "Department outfits",
    "presets": [
      {
        "name": "Patrol Officer",
        "description": "Standard patrol",
        "gender": "male",
        "components": [
          { "slot": 11, "collection": "", "localDrawable": 55, "drawable": 55, "texture": 0, "palette": 0 }
        ],
        "props": [
          { "slot": 0, "collection": "", "localDrawable": 46, "drawable": 46, "texture": 0 }
        ]
      }
    ]
  }
]
```

| Field | Meaning |
| --- | --- |
| `name` | Shown in the menu. Required on categories and outfits. |
| `description` | Shown under the row. Optional. |
| `gender` | `male` or `female`. The outfit is hidden on the other freemode ped, because a drawable number is a different piece of clothing there. Leave it out to show it on both. |
| `components` | Clothing slots. An outfit without any is skipped. |
| `props` | Hats, glasses, watches and other props. |
| `slot` | The component or prop slot. |
| `collection`, `localDrawable` | The content pack the piece comes from and its number inside that pack. Keeps the outfit correct when you add clothing packs later. |
| `drawable`, `texture`, `palette` | The piece's plain numbers. Used when `collection` is empty. |

Hand written files are read at server start. Restart the resource after editing it by hand.

## Permissions

| Permission | What it allows |
| --- | --- |
| `vMenu.Enhanced.Menus.CharacterCreator.Presets` | Seeing and wearing the server presets. |
| `vMenu.Enhanced.Menus.CharacterCreator.PresetsManage` | Saving and removing server presets. |

## Errors

| Console message | Cause |
| --- | --- |
| `No config/clothing-presets.json found...` | The file is missing. The preset menu starts empty. |
| `config/clothing-presets.json could not be parsed...` | Broken JSON. The preset menu starts empty. |
| `Skipping a category in ...: it has no name.` | A category without `name`. |
| `Skipping an outfit in '...': it has no name.` | An outfit without `name`. |
| `Skipping '...' in '...': it has no clothes in it.` | An outfit with no `components`. |
