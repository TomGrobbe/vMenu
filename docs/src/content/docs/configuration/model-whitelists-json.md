---
title: "model-whitelists.json"
---

## About

The model-whitelists.json file (located in `resources\vMenu\config\`) is where you list the vehicles, peds and weapons that you want to lock behind a permission.

Listing a model here doesn't restrict it by itself. What it does is tell vMenu to **generate a permission** for that model, which you then hand out to whichever groups should be allowed to use it. Until a group is granted that permission, the model stays locked for them.

:::note
This page covers the **file** and its format. The permissions that it generates, the template file vMenu writes for you, and how to apply them in your `permissions.cfg`, are all covered on the [Supplemental Permissions](/vmenu/legacy/permissions/supplemental-permissions/) page.
:::

## Format

The file is an object with three lists, one per model type. All three are optional, so you can leave out any list you don't need:

```json
{
  "whitelistedvehicle": [
    "modelname1",
    "modelname2"
  ],
  "whitelistedpeds": [
    "pedname1"
  ],
  "whitelistedweapons": [
    "weapon_addonweaponname1"
  ]
}
```

| Key | Contains |
| --- | --- |
| `whitelistedvehicle` | Vehicle spawn names. Note that this one is **singular**, it's easy to accidentally type `whitelistedvehicles`. |
| `whitelistedpeds` | Ped model names. |
| `whitelistedweapons` | Weapon names, including the `weapon_` prefix. Addon weapons only, see [Weapons work differently](#weapons-work-differently). |

For vehicles and peds, base game models and addon models work exactly the same way, just use the model's spawn name. Weapons are the odd one out, so give that section a read before you fill in the third list.

Anything vMenu doesn't recognise as one of the three keys above is ignored, and a missing file is treated as empty.

:::note
Yes, `whitelistedvehicle` being singular while the other two are plural is a bug. It's staying that way for backwards compatibility, since renaming it would silently break the config of everyone already using it.
:::

:::caution
Always write your model names in **lowercase**. The permissions vMenu generates are always lowercase, but not every check lowercases the name it reads from this file, so a capitalised entry can end up never matching its own permission.
:::

## Weapons work differently

Vehicles and peds behave the way you'd expect: a model is restricted **because** you listed it here, and anything you leave out is untouched.

Weapons don't work like that, and it's worth knowing before you spend time on the third list.

**Base game weapons ignore this file entirely.** Every base game weapon already has its own permission, like `vMenu.WeaponOptions.AssaultRifle`, and that's the only thing vMenu checks for them. Listing `weapon_pistol` here will still generate a `vMenu.WeaponOptions.WhitelistedModels.pistol` line in the template file, but nothing ever reads it. To restrict a base game weapon, use its normal permission on the [Weapon Permissions](/vmenu/legacy/permissions/weapon-permissions/) page instead.

**Every addon weapon is restricted whether you list it or not.** Addon weapons are always permission checked, so this list doesn't decide *if* a weapon is restricted. What it decides is whether you get a permission for that one specific weapon.

There are three permissions that can unlock an addon weapon:

- `vMenu.WeaponOptions.WhitelistedModels.<name>` unlocks that one weapon, and **only exists if you list the weapon here**.
- `vMenu.WeaponOptions.WhitelistedModels.All` unlocks every addon weapon at once.
- `vMenu.WeaponOptions.All` unlocks every addon weapon too, along with the rest of the weapons menu.

So if you don't list an addon weapon, the only way to give someone access to it is to give them **all** of your addon weapons at the same time. Listing it is what lets you hand out that one weapon on its own.

:::caution
This also means that if you add addon weapons and nobody on your server has `vMenu.WeaponOptions.All` or `vMenu.WeaponOptions.WhitelistedModels.All`, those weapons will show up locked for everyone until you list them here and grant their permissions.
:::

## Comments are allowed

The default file has `//` comments in it, and vMenu is fine with that even though comments aren't part of standard JSON. Feel free to leave the existing ones in place or add your own notes.

## Example

Here I've whitelisted two vehicles, one ped and one addon weapon:

```json
{
  "whitelistedvehicle": [
    "adder",
    "zentorno"
  ],
  "whitelistedpeds": [
    "a_m_y_business_01"
  ],
  "whitelistedweapons": [
    "weapon_myaddonrifle"
  ]
}
```

After starting the server once, vMenu writes a matching permission for each of these into `config/templates/SupplementaryPermissionTemplate.cfg`, ready for you to copy into your `permissions.cfg`. See [Supplemental Permissions](/vmenu/legacy/permissions/supplemental-permissions/) for the rest of that workflow.

## What players see

When someone doesn't have the permission for a whitelisted model:

- In the vehicle spawner, ped menu or addon weapons menu, the model is greyed out with a lock icon and the description *"Access to this has been restricted by the server owner."*
- If they try to spawn it another way, for example the spawn by name option, they get *"You are not allowed to spawn this vehicle, because it is restricted by the server owner."* (or the ped equivalent).

Vehicles and peds that aren't listed in this file are not affected at all, they keep using the regular menu permissions. Addon weapons are the exception, as covered [above](#weapons-work-differently).

## Errors

:::caution
If you list the same model twice in one list, you'll see this in the client and server console, and the duplicate is ignored:

```
[vMenu] [Error] Your model-whitelists.json file contains 2 or more entries with the same vehicle name! (adder) Please remove duplicate lines!
```

Duplicates in the weapons list report *"the same ped name"* rather than "weapon name". That's just a wording slip in the message, the file name and the model name in the brackets will still point you at the right line.

If the file isn't valid JSON, you'll get this in both consoles instead:

```
[vMenu] [ERROR] Your model-whitelists.json file contains a problem! Error details: ...
```
:::

## Default model-whitelists.json

```json
{
  // Any model you list here will have a permission generated that you can use
  // inside the permissions.cfg file to give people access to use that model.
  //
  // After entering the model names here and starting vMenu, the
  // config/templates/SupplementaryPermissionTemplate.cfg file will be updated to show you which
  // permissions are available, and provides an example usage.
  //
  // Copy those entries to your permissions.cfg to start configuring them
  // for the groups that you want.
  "whitelistedvehicle": [
    "whitelistedvehiclename1",
    "whitelistedvehiclename2"
  ],
  "whitelistedpeds": [
    "whitelistedpedname1",
    "whitelistedpedname2"
  ],
  "whitelistedweapons": [
    "whitelistedweaponname1",
    "whitelistedweaponname2"
  ]
}
```

## Appreciate my work?
Consider supporting me on [Patreon](https://www.patreon.com/vespura)!
