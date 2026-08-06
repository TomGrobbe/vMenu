---
title: "extras.json"
---

## About

The extras.json file (located in `resources\vMenu\config\`) is used to give the vehicle extras in vMenu a proper name.

Without it, the *Vehicle Options* > *Vehicle Extras* menu just lists `Extra #1`, `Extra #2`, and so on, which tells you nothing about what those extras actually do. With it, you get to see `Push Bar`, `Light Bar`, `Antennas`, etc.

:::note
This file only changes the **labels** in the menu. It does not enable, disable, restrict, or add extras. If an extra doesn't exist on the vehicle model, no amount of configuration here will make it show up.
:::

## Format

The file is one big object. Each key is a **vehicle model spawn name**, and its value is another object mapping **extra IDs** to the label you want to see:

```json
{
  "modelname": {
    "1": "Label for extra 1",
    "2": "Label for extra 2"
  }
}
```

A few rules:

- The model name should be the vehicle's spawn name, in lowercase.
- The extra ID is written as a JSON key, so it has to be in `"quotes"`, even though it's a number.
- The IDs don't need to be in order, and you don't need to list every extra. Anything you leave out just keeps the default `Extra #<id>` label.
- The label is free text, so you can write whatever you want in there.
- Only extras `0` through `13` are shown in the menu, and only if they actually exist on the vehicle you're currently in.

## Adding a vehicle

Say I've got an addon police car called `mypolicecar`, and I want to label its extras. I add a new entry with the model name, and list the extras I care about:

```json
{
  "policecharger": {
    "1": "1: Push Bar",
    "2": "2: Push Bar Wrap",
    "3": "3: Light Bar",
    "4": "4: Visor Lights",
    "5": "5: Antennas",
    "9": "9: Rear Deck Lights"
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

## Errors

If you make a mistake in this file, vMenu will tell you about it. A missing extras.json file is not an error, vMenu just treats it as empty and falls back to the default labels.

:::caution
If you list the same extra ID twice for one vehicle, you'll see this in the client console:

```
[vMenu] [Warning] Your extras.json file contains 2 or more entries with the same extra index! (modelname, Extra 1) Please remove duplicate!
```

If the file isn't valid JSON at all (a missing comma or bracket is the usual culprit), you'll get this in **both** the client and the server console, with the exact location of the problem:

```
[vMenu] [ERROR] Your extras.json file contains a problem! Error details: ...
```
:::

## Default extras.json

```json
{
  "policecharger": {
    "1": "1: Push Bar",
    "2": "2: Push Bar Wrap",
    "3": "3: Light Bar",
    "4": "4: Visor Lights",
    "5": "5: Antennas",
    "9": "9: Rear Deck Lights"
  },
  "policecvpi": {
    "1": "Push Bar",
    "2": "Push Bar Wrap",
    "3": "Light Bar",
    "4": "Visor Lights",
    "11": "Cage",
    "12": "Trunk Modem/Antennas",
    "5": "Antennas",
    "7": "Rear Deck Lights"
  }
}
```

## Appreciate my work?
Consider supporting me on [Patreon](https://www.patreon.com/vespura)!
