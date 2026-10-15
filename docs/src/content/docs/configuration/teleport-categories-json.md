---
title: "teleport-categories.json"
description: "The saved teleport locations under Teleportation Menu, Teleport Categories."
---

The list behind **Teleportation Menu, Teleport Categories**. Coming from vMenu Legacy, this is the teleport half of the old `locations.json`. The blip half is now [blips.json](/vmenu/enhanced/configuration/blips-json/).

## Format

```json
[
  {
    "name": "City",
    "description": "City locations",
    "locations": [
      {
        "name": "Legion Square",
        "description": "",
        "position": { "x": 197.78, "y": -934.75, "z": 30.69 },
        "heading": 141.93
      }
    ]
  }
]
```

| Field | Meaning |
| --- | --- |
| `name` | Shown in the menu. Required on categories and locations. |
| `description` | Shown under the row. Optional. |
| `locations` | The locations in the category. |
| `position` | Where the player lands. Required. |
| `heading` | Which way the player faces, in degrees. Optional. Without it the player keeps facing the way they were. |

The order in the file is the order in the menu.

## Editing from the menu

Staff with the Manage permission get four extra rows:

| Row | What it does |
| --- | --- |
| Create Category | Asks for a name and a description. |
| Create Position | Saves where you stand and the way you face, in the open category. |
| Delete Category | Removes a category and every location in it. |
| Delete Position | Removes one location. |

Every change is written to the file and sent to every player straight away. Names must be unique, within a category for locations.

## Permissions

| Permission | What it allows |
| --- | --- |
| `vMenu.Enhanced.Menus.TeleportMenu.Category` | Seeing and using the categories. |
| `vMenu.Enhanced.Menus.TeleportMenu.Manage` | Adding and removing categories and locations. Staff only by default. |

## Errors

| Console message | Cause |
| --- | --- |
| `No config/teleport-categories.json found...` | The file is missing. The menu starts empty. |
| `config/teleport-categories.json could not be parsed...` | Broken JSON. The menu starts empty. |
| `... has to hold a list of categories...` | The file is not a list `[ ]`. |
| `Skipping a category in ...: it has no name.` | A category without `name`. |
| `Skipping a location in '...': it has no name.` | A location without `name`. |
| `Skipping '...' in '...': it has no position.` | A location without `position`. |

Read at server start. Restart the resource after editing the file by hand.
