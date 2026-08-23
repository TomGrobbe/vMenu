---
title: "extras.json"
description: "Give the numbered vehicle extras a name your players will actually recognise."
---

## What this file is for

Vehicles in GTA can be built with optional parts bolted on: a push bar, a light bar, a roof rack, a
set of antennas. The game calls these **extras**, and it gives them numbers rather than names.

That means the **Vehicle Options, Vehicle Extras** menu can only tell your players "Extra 1",
"Extra 2", "Extra 3" and so on. There is no way for vMenu to find out what those numbers actually
are, because the game genuinely does not know. Extra 3 might be a light bar on one car and a spare
wheel on the next.

`extras.json` is where you write down what they are. Once you have, that same menu says "Push Bar"
and "Light Bar" instead of a number.

:::note
This file only changes the **names on the rows**. It does not switch extras on, add new ones, or stop
anybody using one. If a vehicle does not have an extra, no amount of writing about it here will make
it appear.
:::

## Where it lives

`resources/vMenu.Enhanced/config/extras.json`

The server reads it once when it starts up, and hands the names to every player as they join. If you
change the file, restart the resource so the server reads it again.

## How to write it

The file is one big object. Each key is a **vehicle spawn name**, and what it points at is another
object mapping **extra ids** to the text you want on the row:

```json
{
  "modelname": {
    "1": "Name for extra 1",
    "2": "Name for extra 2"
  }
}
```

A few rules:

- Write the model name in lowercase. That is the vehicle's spawn name, the one you would use to spawn
  it by name, not the pretty name shown in the menu.
- The extra id is a JSON key, so it has to be in `"quotes"` even though it is a number.
- Ids run from `0` to `19`. Anything outside that is skipped, because the menu never looks past 19.
- You do not have to list them in order, and you do not have to list all of them. Anything you leave
  out keeps its numbered name.
- The name is free text. Write whatever makes sense to your players.

## Adding one of your own vehicles

Say you have an addon police car with the spawn name `mypolicecar`. Add a block for it and list the
extras you care about:

```json
{
  "policecharger": {
    "1": "Push Bar",
    "3": "Light Bar",
    "9": "Rear Deck Lights"
  },
  "mypolicecar": {
    "1": "Push Bar",
    "2": "Light Bar",
    "3": "Spotlight",
    "6": "Rear Deck Lights",
    "10": "Rifle Rack"
  }
}
```

The quickest way to find out which number is which is to hop in the vehicle, open
**Vehicle Options, Vehicle Extras**, and tick the numbered rows one at a time to see what changes.

## Comments are allowed

You can leave `//` comments in this file, and a trailing comma at the end of a list will not break
it. The file that ships with vMenu Enhanced uses comments to explain itself, so feel free to keep
notes in yours.

## When something is wrong with it

Nothing here ever takes the whole menu down. A problem with one entry only costs you that entry, and
everything else still works. The server console tells you exactly what it skipped and why:

| What you will see | What happened |
| --- | --- |
| `No config/extras.json found, so vehicle extras keep their numbered names.` | The file is missing. That is fine, it is optional. |
| `config/extras.json could not be parsed...` | The JSON itself is broken, usually a missing comma or bracket. The message says where. |
| `Skipping 'modelname'... its extras have to be written as an object of id to name.` | That vehicle points at something that is not an object. |
| `Skipping extra '3a' on 'modelname'... the id has to be a whole number in quotes.` | An id that is not a number. |
| `Skipping extra 40 on 'modelname'... only 0 to 19 are ever shown.` | An id outside the range the menu looks at. |
| `Extra 1 is listed more than once on 'modelname'...` | You wrote the same id twice. The first one wins. |
| `'modelname' is listed more than once...` | You wrote the same vehicle twice. The first block wins. |

## Coming from vMenu Legacy

The format has not changed, so you can copy your old `extras.json` straight across and it will work.

Two small differences worth knowing. The old version read the file on each player's own machine,
whereas this one reads it on the server and sends the names out, so your players never need a copy of
the file. And the old version stopped at extra 13, while this one goes to 19.
