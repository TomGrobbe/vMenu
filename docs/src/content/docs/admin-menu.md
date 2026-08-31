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

**Scheduled Announcements** is where announcements that send themselves live. See [announcements.json](#announcementsjson) below.

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

None of this is needed for the server to enforce a permission. The server checks the real thing every single time, so a permission you take away stops working immediately whether or not anybody refreshes. Refreshing is about the menu the player is looking at catching up.

## announcements.json

Announcements can send themselves, either every so many minutes or at a set time of day. They live in a file next to your other vMenu config:

```
resources/vMenu.Enhanced/config/announcements.json
```

The file ships with **everything commented out**, so a fresh server announces nothing at all until you decide otherwise. Take the `//` off the lines of one of the examples to switch it on, or write your own.

### What an announcement takes

| Field | What it does |
| --- | --- |
| `name` | A short label so you can find it again in the menu. Players never see this. |
| `text` | What everybody actually sees. Written exactly as you type it, never translated. |
| `everyMinutes` | Send it this often, counting from when the server started. Between 1 and 1440. |
| `at` | Send it at this time of day instead, written as `HH:MM` on a 24 hour clock. |
| `clock` | Only used with `at`. See below. Leave it out and you get `real`. |

Use **either** `everyMinutes` **or** `at` on one announcement, never both. An announcement with neither is skipped, and the server console says which one and why.

### Real time or game time

`clock` picks which clock the announcement measures itself against, and it applies to `everyMinutes` and `at` alike.

`"real"` is the real world. `everyMinutes: 30` is half an hour, and `at: "20:00"` is eight in the evening where the machine your server runs on thinks it is. This is the one you want for restart warnings and anything tied to your community's evening.

`"game"` is the clock inside GTA, which runs much faster than the real one. `everyMinutes: 30` with a game clock comes round in about a real minute, and `at: "20:00"` lands whenever it turns evening in the world. This is the one you want for something tied to what the world looks like, such as a message at sunrise.

One thing to know about `"game"`: it only lines up with what players actually see if vMenu is the one driving the clock, which means `vMenu.Enhanced.TimeOptions.Enabled` has to be on. With time sync off the game keeps its own time and your announcement lands at an hour nobody expects. The server console says so on start if you have a `game` entry while time sync is off.

### An example

```jsonc
{
  "announcements": [
    {
      "name": "Rules reminder",
      "text": "Remember to read the rules. You can find them in our Discord.",
      "everyMinutes": 45
    },
    {
      "name": "Restart warning",
      "text": "~y~The server restarts in 10 minutes.~s~ Park up somewhere safe.",
      "at": "05:50",
      "clock": "real"
    },
    {
      "name": "Sunrise",
      "text": "The sun is coming up over Los Santos.",
      "at": "06:00",
      "clock": "game"
    }
  ]
}
```

An `at` announcement that the server started past waits for the next time round rather than firing the moment it loads, so restarting at 20:05 will not replay your 20:00 message.

### Editing from inside the menu

Staff with the `ManageAnnouncements` permission can add and remove scheduled announcements from **Admin, Scheduled Announcements** without touching the file. Adding one asks you for three things in a row: a name, the text, and when it should go.

It asks for the timing **first**, on its own, and checks it before it asks for anything else. That is on purpose: the timing is the one answer that can be typed wrong, and finding that out after you had written the whole announcement would throw the writing away.

What it accepts:

| You type | What you get |
| --- | --- |
| `@45` | Every 45 minutes, real time |
| `@5 game` or `@5game` | Every 5 minutes on the in game clock, so about ten real seconds |
| `10:30` | At half past ten in the morning, real time |
| `10:30 game` or `10:30game` | At half past ten in the morning in game |

The `@` is what marks a repeat, so `@30` repeats every half hour and `10:30` is a time of day. Adding `game` on the end of either switches the clock, with or without a space in front of it.

In the list, an announcement on the in game clock is marked with a green **G** next to its timing, and the description underneath spells it out. Anything without the G runs on real time.

Saving from the menu rewrites `announcements.json`, which means **any comments you left in that file are lost**. This is the same trade `blips.json` makes, and it is called out in the file's own header.

### Turning the whole thing off

Setting `vMenu.Enhanced.Admin.ScheduledAnnouncements` to `false` in your `configuration.cfg` stops anything sending itself, without you having to delete the file. Staff can still send announcements by hand while it is off, and the menu says plainly that the schedule is switched off so nobody thinks it is broken.

## Settings

These go in your `configuration.cfg`. They are all listed in the `configuration.cfg.example` file your server writes on every start.

| Setting | What it does |
| --- | --- |
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
| `...Admin.SeeNoClipPlayers` | Seeing players who are hidden because they are in noclip |
| `...Admin.DeleteVehicle` | Deleting the vehicle in front of you, or the one you are sat in |
| `...Admin.DeleteEmptyVehicles` | Wiping every empty vehicle on the server |
| `...Admin.DeleteAllVehicles` | Wiping every vehicle on the server, occupied ones included |
| `...Admin.ClearArea` | Clearing the world around you, for everybody near you |
| `...Admin.Announce` | Sending an announcement to the whole server |
| `...Admin.ManageAnnouncements` | Adding and removing scheduled announcements, which writes the config file |
| `...Admin.RefreshPermissions` | Refreshing everybody's permissions at once |

All of them start with `vMenu.Enhanced.Menus.`, and `...Admin.All` grants the lot. They are listed in the `config/permissions.cfg.example` file your server writes on every start.

Two more live with the Online Players menu rather than here:

| Permission | What it allows |
| --- | --- |
| `...OnlinePlayers.RefreshPermissions` | Refreshing one player's permissions from their row in Online Players |
| `...OnlinePlayers.NoClip` | Lending a player noclip, and putting them into or out of it, from their row in Online Players |

## If you are updating from an older version

Two permissions moved, and their names moved with them:

| Old name | New name |
| --- | --- |
| `vMenu.Enhanced.Menus.MiscSettings.ClearArea` | `vMenu.Enhanced.Menus.Admin.ClearArea` |
| `vMenu.Enhanced.Menus.MiscSettings.SeeNoClipPlayers` | `vMenu.Enhanced.Menus.Admin.SeeNoClipPlayers` |

If your `permissions.cfg` still has the old lines, those two tools stop being granted until you update them. Nothing breaks, they simply stop appearing for the people who used to have them.

One setting moved as well:

| Old name | New name |
| --- | --- |
| `vMenu.Enhanced.MiscSettings.ClearAreaRadius` | `vMenu.Enhanced.Admin.ClearAreaRadius` |

If you had changed that from its default, set it again under the new name. Leaving the old line in does nothing, so it is safe to delete.

Both example files are rewritten every time your server starts, so the quickest way to see the new names is to start the server once and open `config/permissions.cfg.example` and `config/configuration.cfg.example`.
