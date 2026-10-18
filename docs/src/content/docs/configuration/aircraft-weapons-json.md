---
title: "aircraft-weapons.json"
description: "Want to add bombs, countermeassures or smoke to your custom (addon) helicopter or plane that has bomb bay doors? Look here."
---

GTA Online helicopters or planes need no custom setup (their offsets are hardcoded in the code), this file is only meant for adding these features to aircraft that don't already have them.

If you want to get these things working for your vehicle, make sure to give them these vehicle mods. Note that smoke should have no countermeasures fitted, otherwise the countermeasures take priority over the smoke. Smoke works using the tyre smoke color, so tyresmoke should be enabled in the vehicle modifications menu.

| Mod                               | Gives                                        |
| --------------------------------- | -------------------------------------------- |
| Bombs                             | Bombs: standard, incendiary, gas or cluster. |
| Countermeasures                   | Flares or chaff.                             |
| Smoke (no countermeasures fitted) | A smoke trail in the tyre smoke colour.      |

Pilot and copilot share controls over the bombs. Whoever goes to the bomb camera first keeps them until they exit the camera. The other person can still use the bomb camera, just not fire any bombs.

Addon aircraft with the same mods work on their own, positions come from the model size.
Bombs also need bomb bay doors on the model.

## File

`resources/vMenu.Enhanced/config/aircraft-weapons.json`, read at startup. Restart the resource after a change. `//` comments and trailing commas are allowed.

Keys are spawn names in lowercase. Every setting is optional:

```json
{
    "myaddonplane": {
        "bombPosition": 0.45,
        "cameraY": 1.0,
        "cameraZ": -1.5,
        "smokeOffset": [0.0, -6.0, 0.5]
    },
    "anotherplane": {
        "chaff": false
    }
}
```

| Setting                             | Meaning                                                              |
| ----------------------------------- | -------------------------------------------------------------------- |
| `bombs`, `flares`, `chaff`, `smoke` | `false` turns it off. `true` turns it on where it is off by default. |
| `bombPosition`                      | Where bombs leave the belly, `0` nose to `1` tail.                   |
| `cameraY`, `cameraZ`                | Bomb camera position in metres, forward and up.                      |
| `flareStartX`                       | 4 numbers, flare start left to right in metres.                      |
| `flareStartY`, `flareStartZ`        | Flare start, forward and up.                                         |
| `flareTargetY`                      | How far behind the aircraft flares head.                             |
| `chaffSide`                         | 4 numbers, `0` left to `1` right.                                    |
| `chaffLength`                       | 4 numbers, `0` nose to `1` tail.                                     |
| `chaffHeight`                       | 4 numbers, metres above the bottom.                                  |
| `chaffSpread`                       | `true` tilts the bursts outwards.                                    |
| `chaffScale`                        | Chaff size.                                                          |
| `smokeOffset`                       | 3 numbers, smoke start right, forward and up.                        |
| `smokeScale`                        | Smoke thickness.                                                     |

`bombPosition` is clamped to `0` to `1`, `chaffScale` and `smokeScale` to `0` to `50`.

## Errors

A bad entry only skips that entry.

| Console message                                                         | Cause                                       |
| ----------------------------------------------------------------------- | ------------------------------------------- |
| `No config/aircraft-weapons.json found...`                              | File missing, it is optional.               |
| `config/aircraft-weapons.json could not be parsed...`                   | Broken JSON. The message says where.        |
| `config/aircraft-weapons.json has to hold a single object...`           | The file is not one `{ }`. Nothing is used. |
| `Skipping 'modelname'... its settings have to be written as an object.` | The entry is not an object.                 |
| `Skipping 'modelname'... every number has to be a real number.`         | `NaN` or `Infinity` used.                   |
| `Ignoring flareStartX on 'modelname'... it needs exactly 4 numbers.`    | Wrong amount of numbers.                    |
| `Ignoring unknown setting 'name' on 'modelname'...`                     | Unknown setting, usually a typo.            |
| `'modelname' is listed more than once...`                               | Duplicate, the first one wins.              |

## Permissions

| Permission                                           | Allows                                       |
| ---------------------------------------------------- | -------------------------------------------- |
| `vMenu.Enhanced.Menus.VehicleOptions.BombsAndFlares` | Bombs, flares, chaff and smoke. No menu row. |
