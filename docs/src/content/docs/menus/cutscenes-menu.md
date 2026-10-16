---
title: "Cutscenes Menu"
description: "The cutscenes players can star in, and the permission behind the menu."
---

The Cutscenes menu lets a player pick a scene from the story and play a part in it as their own character, in their own clothes. It sits right below Misc Settings.

While the scene plays, the player is moved into a private world (a separate copy of the map that only they are in), so nobody else sees them or drives into the scene. When it ends, they are put back where they started.

The player has to be on foot to start a scene. How often somebody can start one is limited by `vMenu.Enhanced.OnlinePlayers.ActionLimit`, the same as the Online Players actions.

## Permissions

| Permission | What it allows | Default |
| --- | --- | --- |
| `vMenu.Enhanced.Menus.Cutscenes.Menu` | Opening the Cutscenes menu and starting any scene in it. | `builtin.everyone` |

It is listed in the `config/permissions.cfg.example` file your server writes on every start.

## Cutscenes

Every scene below is also in the Cutscenes submenu of the [Bully menu](/vmenu/enhanced/menus/bully-menu/), so it can also be played on another player.

| Cutscene | Who you play | What happens |
| --- | --- | --- |
| Lamar's Haircut Roast | Franklin | Lamar roasts your new haircut in Franklin's driveway. From the mission Franklin and Lamar. |
| Pulled Over On The Bridge | One of the street racers | Trevor and Michael, dressed as cops, pull you over on a bridge and Trevor drags you out of your car. From the mission I Fought the Law. |
