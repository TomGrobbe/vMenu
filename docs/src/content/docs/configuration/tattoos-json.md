---
title: "tattoos.json"
description: "Every tattoo and badge the character creator offers."
---

Ships with every base game tattoo up to 2026. Only edit it to add tattoos your server streams.

This file is read by each player's game, not by the server. Players see changes after they reconnect, and errors appear in their F8 console rather than the server console.

## Format

```json
{
  "version": 1,
  "tattoos": [
    { "collection": "mytattoos_overlays", "name": "mytattoos_01_M", "zone": "torso", "gender": "male" },
    { "collection": "mytattoos_overlays", "name": "mytattoos_02_A", "zone": "addon", "gender": "both", "label": "Anchor" }
  ]
}
```

| Field | Required | Meaning |
| --- | --- | --- |
| `collection` | Yes | The overlay collection the tattoo is in. |
| `name` | Yes | The overlay name. |
| `zone` | Yes | Which list it shows in, see below. |
| `gender` | No | `male`, `female`, or anything else for both. Default both. |
| `label` | No | The name shown. Without one it is numbered, like the base game tattoos. |

| Zone | List |
| --- | --- |
| `hair`, `head`, `torso`, `leftArm`, `rightArm`, `leftLeg`, `rightLeg` | That part of the body. |
| `badge` | The badge overlays. |
| `addon` | A list of its own, only shown once something is in it. |

A tattoo listed twice is used once.

## Finding a missing tattoo

With the debugging convar on, `vmenu_character_tattoos` lists every decoration on your current ped and whether the file knows it. `vmenu_character` prints the whole character.
