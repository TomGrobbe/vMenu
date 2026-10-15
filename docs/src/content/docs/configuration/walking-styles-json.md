---
title: "walking-styles.json"
description: "The walking styles players can pick under Player Appearance."
---

The list behind **Player, Player Appearance, Walking Style**.

## Format

```json
{
  "move_m@casual@a": "Casual",
  "move_f@sexy@a": "Sassy",
  "my_streamed_clipset": "My Custom Walk"
}
```

| Part | Meaning |
| --- | --- |
| Left | The clip set the game loads. Clip sets your server streams work too. |
| Right | The text shown in the menu. Empty means the clip set name is shown. |

- The order in the file is the order in the menu.
- A **Normal** choice is always added on top. It puts back the ped's own walk.
- Clip sets are made for one kind of skeleton. `move_m@` ones suit male peds, `move_f@` ones suit female peds. A player can try any style on any ped, and is told when the game has nothing to load for it.

## Permission

`vMenu.Enhanced.Menus.PlayerAppearance.WalkingStyle` shows the Walking Style row. There are no permissions per style.

## Errors

| Console message | Cause |
| --- | --- |
| `No config/walking-styles.json found...` | The file is missing. Only Normal is offered. |
| `config/walking-styles.json could not be parsed...` | Broken JSON. Only Normal is offered. |
| `Skipping walking style '...': the text to show for it has to be written in quotes.` | The right side is not text. |
| `'...' is listed more than once...` | A duplicate. The first one is used. |

Read at server start. Restart the resource after changing it.

