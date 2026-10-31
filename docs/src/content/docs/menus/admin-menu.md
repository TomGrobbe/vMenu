---
title: "Admin Menu"
description: "The staff only menu in vMenu Enhanced, what each of its tools does, the permissions behind them, and the announcements.json file that lets the server talk to everybody on its own."
---

The Admin menu is where every tool that only your staff team should touch now lives. Freezing somebody, carrying them somewhere, wiping stray vehicles, talking to the whole server, and pushing a permission refresh are all here, in one place, instead of scattered around the rest of vMenu.

It sits near the top of the main menu, right under Staff Alerts.

## Nobody sees it unless you let them

The Admin menu is **hidden**, not locked. A player without the permission does not see a greyed out row telling them the server has staff tools, they simply have no Admin row at all. Every option inside it works the same way, so a staff member who only has some of the permissions sees only the tools they can actually use.

To give somebody access, grant them the permission for the menu itself plus whichever tools you want them to have. See [Permissions](#permissions) below.

## What is in it

### Players

**Freeze Closest Player** pins whoever is standing nearest to you to the spot. They cannot walk, run, or drive off. Press it again while stood next to the same person and they are released.

The freeze survives them dying and respawning, and survives them changing character, so somebody cannot wriggle out of it by taking a nap. It is lifted automatically if vMenu is restarted, so nobody is ever left stuck by an accident on your end.

**Grab Closest Player** picks the nearest person up and carries them in front of you, facing the same way you are. They come along wherever you go, and they cannot break free on their own, so remember to put them down again. The button changes to **Release Player** while you are carrying somebody, and releasing works no matter how far away they have ended up.

Only the staff member who picked somebody up can put them down. If two of you reach for the same person, the second one is told they are already being carried. If the person carrying them disconnects, whoever they were carrying is released straight away rather than being left attached to somebody who is no longer there.

"Closest" means the closest player your game actually knows about. On a busy server your game is only told about people near you, so this finds somebody standing in front of you, not somebody on the other side of the map. How far it reaches is set by `vMenu.Enhanced.Admin.ClosestPlayerRange`, which defaults to 5 metres and can go up to 15.

**Return To The Default World** puts you back in the main routing bucket, for when you teleported to someone who was in a different bucket.

**See Private Messages** shows you a copy of every private message players send each other through the Online Players menu. It is off until you turn it on, and vMenu remembers your choice. Messages you send or receive yourself are not copied. Players are warned in the Send Message description that staff may read their messages.

**Show Players In NoClip** is the same option that used to live in Miscellaneous Settings. It keeps showing the blip and the name of anybody flying around in noclip, who everybody else has them hidden from.

### Vehicles

**Delete Vehicle** removes the vehicle in front of you, or the one you are sat in, whether you are driving it or only a passenger.

This is the wider version. Ordinary players with the Vehicle Options delete button can only delete a vehicle they are actually **driving**, which is a change from how it used to behave. See [The /dv command](#the-dv-command) below.

**Delete Empty Vehicles** clears up after a busy night, when the map is covered in cars people spawned and walked away from. It leaves two things alone:

- **Anything with somebody in it.** Driver or passenger, player or NPC. If a person is sat in it, it stays.
- **The map's own traffic.** Parked cars, passing traffic, police patrols and anything else the game spawns for itself are not touched. Wiping those empties the streets for everybody, and the game only spawns them straight back.

So what actually goes is the vehicles scripts put there and nobody is using, which is what you wanted gone.

**Delete All Vehicles** means all of them. Occupied ones, ambient traffic, parked cars, the lot. It yanks drivers out of moving cars everywhere at once and strips the streets bare until the game refills them. Both buttons ask you to press a second time before they do anything, but this is the one to be careful with.

### Server

**Clear Area** tidies the world around you. Wrecks, dropped props, wandering people, scorch marks, dirt and broken street lights all go back to how they started. This one is not only for you: everybody standing near you gets the same patch of world cleaned up at the same moment. How far it reaches is set by `vMenu.Enhanced.Admin.ClearAreaRadius`, which defaults to 100 metres.

This option used to live in Miscellaneous Settings.

**Send Announcement** puts a message on the screen of everybody on the server, wherever they happen to be. It uses the same banner as a staff alert, in blue rather than amber and headed ANNOUNCEMENT, so nobody mistakes the server talking for a player asking for help.

**Scheduled Announcements** is where announcements that send themselves live. See [announcements.json](/vmenu/enhanced/configuration/announcements-json/).

**Refresh All Permissions** sends everybody on the server a fresh copy of what they are allowed to do. See [Refreshing permissions](#refreshing-permissions).

## The /dv command

`/dv` deletes a vehicle without opening the menu. What it can reach now depends on what you are allowed to do.

| What you hold | What `/dv` deletes |
| --- | --- |
| `...VehicleOptions.DeleteVehicle` | Only the vehicle you are **driving**. Being a passenger is not enough. |
| `...Admin.DeleteVehicle` | The vehicle in front of you, **or** the one you are sat in, driving or not. |

You do not have to pick. If you hold both, `/dv` quietly uses the wider one. If you hold neither, the command is not registered for you at all.

The **Delete Vehicle** button in Vehicle Options follows the first row: it now refuses unless you are in the driver seat. The one on the Admin menu follows the second.

`/dv` only exists while `vMenu.Enhanced.VehicleOptions.DeleteVehicleCommand` is turned on in your `configuration.cfg`. That has not changed.

## Refreshing permissions

First, a thing worth knowing: **editing `permissions.cfg` on its own changes nothing.** The server does not watch that file. It only knows what it was told, so a permission moves when you actually tell it, either by re executing the file with `exec` or by running the `add_ace`, `remove_ace`, `add_principal` and `remove_principal` commands in the console.

Once the server does know, it enforces the new answer immediately. Each player's copy of vMenu does not, because it keeps its own list of what it is allowed to do and only asks for that list once, when they join.

Refreshing hands them a new list. There are three ways to do it.

**For everybody, from the menu.** Admin menu, **Refresh All Permissions**. It tells you how many players it reached.

**For one player, from the menu.** Online Players, pick a player, **Refresh Permissions**. Handy when you have just promoted one person and do not want to bother everybody else.

**From the server console.**

```
vmenu_refresh_permissions
```

with no argument refreshes everybody, exactly as it always has. Add a server id to refresh just that one player:

```
vmenu_refresh_permissions 42
```

If nobody on the server has that id, the console says so rather than quietly doing nothing.

If an [integration](/vmenu/enhanced/integrations/) with role sync changes a player's groups, it refreshes that player's menu by itself, so you do not need to do anything.

None of this is needed for the server to enforce a permission. The server checks the real thing every single time, so a permission you take away stops working immediately whether or not anybody refreshes. Refreshing is about the menu the player is looking at catching up.

## Staff only mode

Set `vMenu.Enhanced.Admin.StaffOnly` to `true` to make vMenu staff only. A player counts as staff when they have `vMenu.Enhanced.Staff` (or `vMenu.Enhanced.Everything`). You can turn it on and off while the server runs, everybody's menu follows straight away.

For everybody else:

| Turns off | Stays on |
| --- | --- |
| Opening the menu, including plugin menus | Weather, time, snow and blackout |
| Every feature that needs a permission, also through key bindings and commands | Bully effects staff put on them |
| Speedometer, clock, minimap zoom key, finger pointing | Freeze, hold, teleport, kill and messages from staff |
| `vmenu_ticks_overlay` and the developer outlines | Noclip that staff forced on or lent them |
| Their own illuminated clothing, walking style, idle camera choice and keeping props on | Seeing other players' illuminated clothing |
| Raising staff alerts | Death and join/leave messages, at their default |

The server enforces it, a player cannot get around it by changing their own game files.

## announcements.json

Scheduled announcements, their format and how to edit them from the menu are on the [announcements.json](/vmenu/enhanced/configuration/announcements-json/) page.

## Settings

These go in your `configuration.cfg`. They are all listed in the `configuration.cfg.example` file your server writes on every start.

| Setting | What it does |
| --- | --- |
| `vMenu.Enhanced.Admin.StaffOnly` | Makes vMenu staff only, see [Staff only mode](#staff-only-mode). Default false. |
| `vMenu.Enhanced.Admin.ClearAreaRadius` | How far around a player Clear Area reaches, in metres. Default 100. |
| `vMenu.Enhanced.Admin.ClosestPlayerRange` | How far away freeze and grab will still find somebody, in metres. Default 5, maximum 15. |
| `vMenu.Enhanced.Admin.ScheduledAnnouncements` | Whether the announcement schedule runs at all. Default true. |
| `vMenu.Enhanced.Admin.AnnouncementSeconds` | How long an announcement stays on screen. Default 20. |

## Permissions

| Permission | What it allows |
| --- | --- |
| `...Admin.Menu` | Seeing the menu at all |
| `...Admin.FreezePlayer` | Freezing and unfreezing the closest player |
| `...Admin.GrabPlayer` | Picking the closest player up and putting them down |
| `...Admin.DeleteVehicle` | Deleting the vehicle in front of you, or the one you are sat in |
| `...Admin.DeleteEmptyVehicles` | Wiping every empty vehicle on the server |
| `...Admin.DeleteAllVehicles` | Wiping every vehicle on the server, occupied ones included |
| `...Admin.ClearArea` | Clearing the world around you, for everybody near you |
| `...Admin.Announce` | Sending an announcement to the whole server |
| `...Admin.ManageAnnouncements` | Adding and removing scheduled announcements, which writes the config file |
| `...Admin.RefreshPermissions` | Refreshing everybody's permissions at once |
| `...Admin.ResetRoutingBucket` | Putting yourself back in the default world |
| `...Admin.SeePrivateMessages` | Turning on copies of private messages between players |

All of them start with `vMenu.Enhanced.Menus.`, and `...Admin.All` grants the lot. They are listed in the `config/permissions.cfg.example` file your server writes on every start.

Two more live with the Online Players menu rather than here:

| Permission | What it allows |
| --- | --- |
| `...OnlinePlayers.RefreshPermissions` | Refreshing one player's permissions from their row in Online Players |
| `...OnlinePlayers.NoClip` | Lending a player noclip, and putting them into or out of it, from their row in Online Players |

And one that used to be here now lives with Display Settings, because it only changes what you see on your own screen rather than doing anything to the server:

| Permission | What it allows |
| --- | --- |
| `...DisplaySettings.SeeNoClipPlayers` | Seeing players who are hidden because they are in noclip |

## If you are updating from an older version

Two permissions moved, and their names moved with them:

| Old name | New name |
| --- | --- |
| `vMenu.Enhanced.Menus.MiscSettings.ClearArea` | `vMenu.Enhanced.Menus.Admin.ClearArea` |
| `vMenu.Enhanced.Menus.MiscSettings.SeeNoClipPlayers` | `vMenu.Enhanced.Menus.DisplaySettings.SeeNoClipPlayers` |

If your `permissions.cfg` still has the old lines, those two tools stop being granted until you update them. Nothing breaks, they simply stop appearing for the people who used to have them.

Seeing players in noclip has moved twice. It started in Misc Settings, spent a while in this menu, and now sits in Display Settings. So if your `permissions.cfg` has the middle name, `vMenu.Enhanced.Menus.Admin.SeeNoClipPlayers`, that one needs updating to the Display Settings name as well.

One setting moved as well:

| Old name | New name |
| --- | --- |
| `vMenu.Enhanced.MiscSettings.ClearAreaRadius` | `vMenu.Enhanced.Admin.ClearAreaRadius` |

If you had changed that from its default, set it again under the new name. Leaving the old line in does nothing, so it is safe to delete.

Both example files are rewritten every time your server starts, so the quickest way to see the new names is to start the server once and open `config/permissions.cfg.example` and `config/configuration.cfg.example`.
