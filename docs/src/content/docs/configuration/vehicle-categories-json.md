---
title: "vehicle-categories.json"
description: "Add your own vehicle categories to the Vehicle Spawner."
---

## Where it lives

`resources/vMenu.Enhanced/config/vehicle-categories.json`

The menu is built once when a player joins, so restart the resource after changing this file.

## How to write it

The name on the left is what players see in the menu. The list on the right is the vehicle model names that belong in it.

```json
{
  "Police": ["police", "police2", "police3", "policeb"],
  "Emergency Services": ["ambulance", "firetruk"]
}
```

- A vehicle you list is taken out of its game class and only shows up in your category.
- Your categories and the game's classes sit in the same list, sorted alphabetically together.
- A vehicle can only be in one category. If you list the same model twice, the first category wins and the server console tells you about it.

## Permissions

Every category gets its own permission, generated when the server starts. The example above creates:

```
vMenu.Enhanced.Menus.VehicleSpawner.Categories.police
vMenu.Enhanced.Menus.VehicleSpawner.Categories.emergency_services
```

The permission is your category name in lowercase, with anything that is not a letter, a digit or an underscore turned into an underscore. They are listed in `config/permissions.cfg.example`, each marked with a note saying it came from this file.

Granting `vMenu.Enhanced.Menus.VehicleSpawner.Categories.All` grants your own categories too.
