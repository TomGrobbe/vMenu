---
title: "extras.json"
description: "Give the numbered vehicle extras a name your players will actually recognise."
---

Vehicles in GTA can be built with optional parts bolted on: a push bar, a light bar, a roof rack, a set of antennas. The game calls these **extras** and gives them numbers rather than names, so the **Vehicle Options, Vehicle Extras** menu can only say "Extra 1", "Extra 2" and so on. The game genuinely does not know what they are, and extra 3 might be a light bar on one car and a spare wheel on the next.

`extras.json` is where you write down what they are, so that menu says "Push Bar" instead of a number.

:::note
This only changes the **names on the rows**. It does not switch extras on, add new ones, or stop anybody using one.
:::

## Where it lives

`resources/vMenu.Enhanced/config/extras.json`

The server reads it once at startup and hands the names to every player as they join. Change the file and restart the resource so the server reads it again.

## How to write it

Each key is a **vehicle spawn name**, pointing at an object that maps **extra ids** to the text you want:

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

A few rules:

- Write the model name in lowercase. That is the spawn name, not the pretty name shown in the menu.
- The extra id is a JSON key, so it needs `"quotes"` even though it is a number.
- Ids run from `0` to `19`. Anything outside that is skipped.
- You do not have to list them in order, or list all of them. Anything you leave out keeps its numbered name.
- The name is free text.

The quickest way to find out which number is which is to hop in the vehicle, open **Vehicle Options, Vehicle Extras**, and tick the numbered rows one at a time to see what changes.

You can leave `//` comments in this file, and a trailing comma will not break it.

## When something is wrong with it

Nothing here takes the whole menu down. A problem with one entry only costs you that entry, and the server console says what it skipped and why:

| What you will see | What happened |
| --- | --- |
| `No config/extras.json found, so vehicle extras keep their numbered names.` | The file is missing. That is fine, it is optional. |
| `config/extras.json could not be parsed...` | The JSON is broken, usually a missing comma or bracket. The message says where. |
| `Skipping 'modelname'... its extras have to be written as an object of id to name.` | That vehicle points at something that is not an object. |
| `Skipping extra '3a' on 'modelname'... the id has to be a whole number in quotes.` | An id that is not a number. |
| `Skipping extra 40 on 'modelname'... only 0 to 19 are ever shown.` | An id outside the range the menu looks at. |
| `Extra 1 is listed more than once on 'modelname'...` | The same id twice. The first wins. |
| `'modelname' is listed more than once...` | The same vehicle twice. The first block wins. |

## Coming from vMenu Legacy

The format has not changed, so copy your old `extras.json` straight across. Two small differences: the old version read the file on each player's machine, whereas this one reads it on the server and sends the names out, and the old version stopped at extra 13 while this one goes to 19.
