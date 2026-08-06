---
title: "Supplemental Permissions"
---

## What are supplemental permissions?

Supplemental permissions are a separate permission system that lets you control who can spawn or use specific **whitelisted models**. This works for vehicles, peds, and weapons, and it includes addon models as well as base game models.

The idea is simple. You list the model spawn names that you want to protect, vMenu generates a permission for each one, and you then decide which groups are allowed to use them inside your `permissions.cfg`. Until you grant a whitelisted model to a group, it stays restricted.

This is handled by a separate config file and a generated template file, so it does not interfere with the rest of your `permissions.cfg`.

## The config file

Whitelisted models are listed in the `config/model-whitelists.json` file inside the vMenu resource folder. It has three lists, one for each type of model:

```json
{
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

## Whitelisting models

To whitelist a model, add its spawn name to the correct list.

- Add vehicle spawn names to `whitelistedvehicle`.
- Add ped spawn names to `whitelistedpeds`.
- Add addon weapon names to `whitelistedweapons`.

A few things to keep in mind:

- Model names are treated in lowercase, so the generated permission will always be lowercase regardless of how you type it here.
- For weapons, the `weapon_` prefix is removed from the generated permission. For example, `weapon_myaddonrifle` becomes the permission ending in `myaddonrifle`.

:::caution
For **vehicles and peds**, base game models and addon models work exactly the same way here.

**Weapons are different.** Base game weapons ignore this file completely, since they already have their own permissions, so only addon weapons are worth listing. Addon weapons are also restricted whether or not you list them here. See [Weapons work differently](/vmenu/legacy/configuration/model-whitelists-json/#weapons-work-differently) for the full explanation.
:::

For more detail on the file itself, including the error messages it can log and what players see when a model is restricted, see the [model-whitelists.json](/vmenu/legacy/configuration/model-whitelists-json/) page.

Here is an example with real values:

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

## The generated template file

You do not write the permissions yourself. After you list your models and start the server, vMenu generates a template file for you at:

```
config/templates/SupplementaryPermissionTemplate.cfg
```

This file is regenerated every time the vMenu resource starts, and it always overwrites the previous version. It contains a header and one ready to use permission line for each whitelisted model, along with the two catch all permissions. Using the example above, the generated file looks like this:

```bash
#################################################################
#                   THIS IS A TEMPLATE FILE.                    #
#          DO NOT EDIT, MAKE A COPY AND EDIT THE COPY.          #
#################################################################
add_ace builtin.everyone "vMenu.VehicleSpawner.WhitelistedModels.All" allow
add_ace builtin.everyone "vMenu.PlayerAppearance.WhitelistedModels.All" allow
add_ace builtin.everyone "vMenu.WeaponOptions.WhitelistedModels.All" allow
add_ace builtin.everyone "vMenu.WeaponOptions.WhitelistedModels.myaddonrifle" allow
add_ace builtin.everyone "vMenu.VehicleSpawner.WhitelistedModels.adder" allow
add_ace builtin.everyone "vMenu.VehicleSpawner.WhitelistedModels.zentorno" allow
add_ace builtin.everyone "vMenu.PlayerAppearance.WhitelistedModels.a_m_y_business_01" allow
```

:::note
This is a template only, so please do not edit it directly. As the header says, make a copy of the lines you need and place them in your `permissions.cfg`. The file is overwritten on every resource start, so any changes made here would be lost.
:::

### Permission node format

The generated permissions follow these patterns:

| Model type | Permission node |
| --- | --- |
| Vehicle | `vMenu.VehicleSpawner.WhitelistedModels.<model>` |
| Ped | `vMenu.PlayerAppearance.WhitelistedModels.<model>` |
| Weapon | `vMenu.WeaponOptions.WhitelistedModels.<name without weapon_ prefix>` |

There is also a catch all permission for each type. Granting one of these allows every whitelisted model of that type at once:

- `vMenu.VehicleSpawner.WhitelistedModels.All`
- `vMenu.PlayerAppearance.WhitelistedModels.All`
- `vMenu.WeaponOptions.WhitelistedModels.All`

## Applying the permissions

Once the template file has been generated, the workflow is:

1. Add your model spawn names to `config/model-whitelists.json`.
2. Start the server, and make sure the vMenu resource has run at least once. This is what generates the template file.
3. Open `config/templates/SupplementaryPermissionTemplate.cfg` and copy the lines you want into your `permissions.cfg`.
4. In `permissions.cfg`, change `builtin.everyone` to the group or ace that you want to give access to, and use `allow` or `deny` as needed. For example, to allow only your VIP group to spawn the Adder:

```bash
add_ace group.vip "vMenu.VehicleSpawner.WhitelistedModels.adder" allow
```

5. Restart the server so the updated `permissions.cfg` is executed.

:::tip
Every generated line defaults to `builtin.everyone` with `allow`, which grants the model to everyone. Change the group name to something more specific when you want to restrict a model to a certain group.
:::

## Server crash when generating the template

vMenu writes the template file directly to the `config/templates/` folder when the resource starts. If the account running the server cannot write to that folder, vMenu will fail to create or update the file, and the resource will error out while starting.

:::caution
If your server crashes when starting vMenu and the error mentions not being able to access or write to the `templates` folder, make sure the user account that runs the server has permission to read from and write to the `config/templates` folder inside the vMenu resource.

On Linux this is the most common cause, since the server process often runs under a user that does not have write access to that folder by default. Give that user write access to the folder so vMenu can generate the file.

On Windows this is much less likely, because file permissions are usually more relaxed, but the same fix applies if you do run into it.
:::

## Appreciate my work?

Consider supporting me on [Patreon](https://www.patreon.com/vespura)!
