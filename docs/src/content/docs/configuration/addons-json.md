---
title: "addons.json"
---

## About

The addons.json file (located in `resources\vMenu\config\`) is used to add addon vehicles / peds / weapons in to vMenu's addon submenus.

## Removing sections

In this file you can list all your addon models. If you want to remove a all addon models from one of the categories below, simply set that category to `[]`, for example, to remove all models set each category to the following:

```json
{
  "vehicles": [],
  "peds": [],
  "weapons": []
}
```

## Adding models

If you want to add new models, imply add a comma after the last row in a specific category, add a new line below and just copy the format from the line above, replacing the name with your new name.
For example, here I've added 4 extra cars, 1 extra ped and 5 extra weapons:

```json
{
  "vehicles": [
    "addonvehiclename1",
    "addonvehiclename2",
    "addonvehiclenameextra1",
    "addonvehiclenameextra2",
    "addonvehiclenameextra3",
    "addonvehiclenameextra4"
  ],
  "peds": [
    "addonpedname1",
    "addonpedname2",
    "addonpednameextra1"
  ],
  "weapons": [
    "addonweaponname1",
    "addonweaponname2",
    "addonweaponnameextra1",
    "addonweaponnameextra2",
    "addonweaponnameextra3",
    "addonweaponnameextra4",
    "addonweaponnameextra5"
  ],
  "weapon_components": [
    "weapon_component_name_1",
    "weapon_component_name_2"
  ]
}
```

*Note: `weapon_components` are untested and un-documented for now*

## Renaming vehicles

Vehicles can not be renamed through vMenu. This would be really easy to add, but because this is not the proper way to name vehicles, I refuse to add an option for it because you should learn to properly add vehicle names. To **properly** set a vehicle's name, follow the easy steps below:

1. Go to the `vehicles.meta` file for your vehicle, set the `<gameName>MODELNAME</gameName>` entry to the **vehicle model** name. **Do <u>NOT</u> enter a custom name in there**. For example, if my addonvehicle is called `mgt` in-game (shows up as `mgt` in the addons vehicle spawner/you spawn it by typing `mgt` in the spawn by name function) then set the `gameName` to `mgt` in the `vehicles.meta` file.

2. Go to the `fxmanifest.lua` (previously `__resource.lua`) of your vehicle resource, and add a new `client_script`, you can name it whatever you want. For now I'll use `veh_names.lua`, so it should look like this: `client_script 'veh_names.lua'`.

3. Create the file you just listed in the `fxmanifest.lua` (previously `__resource.lua`) in your vehicle resource folder: `veh_names.lua`.

4. In that lua file, paste the following:<br>
    ```lua
    Citizen.CreateThread(function()
        AddTextEntry("MODELNAME", "The Display Name You Want Your Vehicle To Appear As, Enter That Name Here")
    end)
    ```

5. Now change the `"MODELNAME"` to your vehicle's spawn name (the same name that you set as the `gameName` in the vehicles.meta file.) In this case: `"mgt"`.

6. Now change the display name. I'm sure you can figure out where you're supposed to enter that.

7. The final result should be something like this:<br>
    ```lua
    Citizen.CreateThread(function()
        AddTextEntry("mgt", "Mustang GT")
    end)
    ```

8. Do this for every vehicle you have/want to set a custom name for, for example, if I wanted to also do `focusrs` and `bmwm5`. I'd do it like this:
    ```lua
    Citizen.CreateThread(function()
        AddTextEntry("mgt", "Mustang GT")
        AddTextEntry("focusrs", "Focus RS")
        AddTextEntry("bmwm5", "BMW M5")
    end)
    ```

## Default addons.json

```json
{
  "vehicles": [
    "addonvehiclename1",
    "addonvehiclename2"
  ],
  "peds": [
    "addonpedname1",
    "addonpedname2"
  ],
  "weapons": [
    "addonweaponname1",
    "addonweaponname2"
  ]
}
```

## Appreciate my work?
Consider supporting me on [Patreon](https://www.patreon.com/vespura)!
