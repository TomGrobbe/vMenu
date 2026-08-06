---
title: "addons.json"
---

## About

The addons.json file (located in `resources\vMenu\config\`) is where you tell vMenu about the custom content you've added to your server. It has five sections:

- [`vehicles`, `peds` and `weapons`](#adding-models) put your addon models into vMenu's addon submenus.
- [`weapon_components`](#weapon-components) adds your streamed weapon components to the weapons that accept them.
- [`extra_blendable_faces`](#blendable-faces) names any custom blendable heads you've installed, so they show up in the MP Character inheritance menu.

:::tip
This page covers all of them, so it's a long one. Use the **On this page** menu on the right to jump straight to the bit you need. On smaller screens it sits at the top of the page instead.
:::

## Removing sections

In this file you can list all your addon models. If you want to remove all addon models from one of the categories below, simply set that category to `[]`. For example, to remove all models, set each category to the following:

```json
{
  "vehicles": [],
  "peds": [],
  "weapons": []
}
```

## Adding models

If you want to add new models, simply add a comma after the last row in a specific category, add a new line below, and just copy the format from the line above, replacing the name with your new name.
For example, here I've added 4 more cars, 1 more ped and 5 more weapons:

```json
{
  "vehicles": [
    "addonvehiclename1",
    "addonvehiclename2",
    "addonvehiclename3",
    "addonvehiclename4",
    "addonvehiclename5",
    "addonvehiclename6"
  ],
  "peds": [
    "addonpedname1",
    "addonpedname2",
    "addonpedname3"
  ],
  "weapons": [
    "addonweaponname1",
    "addonweaponname2",
    "addonweaponname3",
    "addonweaponname4",
    "addonweaponname5",
    "addonweaponname6",
    "addonweaponname7"
  ],
  "weapon_components": [
    "weapon_component_name_1",
    "weapon_component_name_2"
  ]
}
```

## Weapon components

The `weapon_components` section is for addon (streamed) weapon components, things like custom scopes, clips and suppressors. You only need it for components you've added yourself, every base game component is already built in to vMenu.

Just list the component names:

```json
{
  "weapon_components": [
    "COMPONENT_MYRIFLE_SCOPE_01",
    "COMPONENT_MYRIFLE_CLIP_02",
    "COMPONENT_MYPISTOL_SUPP"
  ]
}
```

You don't have to say which weapon each component belongs to. vMenu checks every component in this list against every weapon it knows about, base game and addon weapons alike, and each one automatically shows up as a toggle in the menus of the weapons that actually accept it. Components that don't match any weapon simply never appear, so if one of yours doesn't show up, double check that the name matches the one in your weapon's meta files exactly.

:::note
Base game components are shown with a proper name like `Extended Clip`, because the game has a translated label for them. Your addon components don't have one, so they're displayed using the exact name you type here. Bear that in mind when naming them, since `COMPONENT_MYRIFLE_SCOPE_01` is what your players will see in the menu.
:::

## Blendable faces

The `extra_blendable_faces` section is used to add custom (streamed) blendable heads to the *Parent #1* and *Parent #2* lists in the MP Character inheritance menu.

If you've installed an add-on head pack on your server, the heads are already there as far as the game is concerned, but vMenu has no way of knowing what they're called. This section is where you give them names:

```json
{
  "extra_blendable_faces": [
    "Custom Head 1",
    "Custom Head 2",
    "Custom Head 3",
    "Custom Head 4",
    "Custom Head 5"
  ]
}
```

These entries are **labels only**. vMenu doesn't check them against anything, so you can name them whatever makes sense for your server.

### Order matters

This is the important bit. Each entry is matched to a head by its **position in the list**, continuing on from where the base game heads stop.

A stock server has 46 blendable parent heads, numbered `0` to `45`. So your first entry is head `46`, your second is head `47`, and so on. If your head pack occupies IDs 46 to 50, the example above is correct, and `"Custom Head 1"` is head 46.

If your entries are in a different order to the head IDs your pack actually uses, the names in the menu will point at the wrong faces. Adding, removing or reordering entries later shifts everything below them, so keep the list in the same order as your pack.

:::note
FiveM supports 92 blendable heads in total, which leaves room for up to 46 add-on heads on top of the base game's 46. Installing more than that can cause heads to load incorrectly or crash the game. Head pack installation itself is handled by the pack's own resource, not by vMenu.
:::

### Skin tones

Custom faces show up in the **Parent #1** and **Parent #2** lists, but deliberately **not** in the **Parent #1 Skin** and **Parent #2 Skin** lists. Skin tones always come from the base game heads.

That's why those are four separate options rather than two. Face shape and skin tone are picked independently, and then blended with the two sliders below them:

- **Head Shape Mix** blends the face shape between Parent #1 and Parent #2.
- **Body Skin Mix** blends the skin tone between Parent #1's skin and Parent #2's skin.

So you can give a character a custom head shape while still using a base game skin tone, which is exactly what you want, since a streamed head model doesn't come with its own skin textures. For the underlying game side of this, see the [SetPedHeadBlendData][head-blend-data] native.

:::caution
Listing the same name twice will log this in the client console, and the duplicate is ignored, which shifts every head below it by one:

```
[vMenu] [Error] Your addons.json file contains 2 or more entries with the same extra blendable face name! (Custom Head 1) Please remove duplicate lines!
```

Give each face a unique name, even if two heads look similar.
:::

## Renaming vehicles

Vehicles cannot be renamed through vMenu. While this would be fairly easy to add, it is not the recommended way to name vehicles, so vMenu intentionally does not include an option for it. The recommended approach is to add vehicle names properly. To **properly** set a vehicle's name, follow the easy steps below:

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

6. Now change the display name. Enter it in the second parameter of the `AddTextEntry` call, in place of the placeholder text.

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
  "extra_blendable_faces": [],
  "weapons": [
    "addonweaponname1",
    "addonweaponname2"
  ],
  "weapon_components": [
    "weapon_component_name_1",
    "weapon_component_name_2"
  ]
}
```

## Appreciate my work?
Consider supporting me on [Patreon](https://www.patreon.com/vespura)!

[head-blend-data]: https://docs.fivem.net/natives/?_0x9414E18B9434C2FE=
