---
title: "announcements.json"
description: "Announcements that send themselves, every so many minutes or at a set time of day."
---

The file ships with everything commented out, so nothing is announced until you switch something on.

## Format

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

| Field | Meaning |
| --- | --- |
| `name` | A label to find it in the menu. Players never see it. |
| `text` | What players see. Shown as written, never translated. |
| `everyMinutes` | Send it this often, counted from server start. 1 to 1440. |
| `at` | Send it at this time of day, as `HH:MM` on a 24 hour clock. |
| `clock` | `real` (default) or `game`. Applies to `everyMinutes` and `at`. |

Use either `everyMinutes` or `at`, never both. An announcement with neither is skipped.

An `at` time the server started past waits for the next day, so a restart at 20:05 does not replay the 20:00 message.

## Real or game clock

| Clock | `everyMinutes: 30` | `at: "20:00"` |
| --- | --- | --- |
| `real` | Every half hour | 20:00 on the server machine's clock |
| `game` | About every real minute | When it turns 20:00 in game |

`game` only matches what players see while `vMenu.Enhanced.TimeOptions.Enabled` is on. The console warns on start when a `game` entry exists while time sync is off.

## Editing from the menu

Staff with the Manage permission can add and remove announcements under **Admin, Scheduled Announcements**. Adding one asks for the timing, then the name, then the text.

| Timing typed | Result |
| --- | --- |
| `@45` | Every 45 minutes, real clock |
| `@5 game` | Every 5 minutes, game clock |
| `10:30` | At 10:30, real clock |
| `10:30 game` | At 10:30, game clock |

Announcements on the game clock are marked with a green **G** in the list. Every change is written to the file straight away.

## Settings and permissions

| Name | What it does |
| --- | --- |
| `vMenu.Enhanced.Admin.ScheduledAnnouncements` | Setting. `false` stops the schedule without deleting the file. Default `true`. |
| `vMenu.Enhanced.Admin.AnnouncementSeconds` | Setting. How long an announcement stays on screen. Default 20. |
| `vMenu.Enhanced.Menus.Admin.Announce` | Permission. Sending an announcement by hand. |
| `vMenu.Enhanced.Menus.Admin.ManageAnnouncements` | Permission. Adding and removing scheduled announcements. |

## Errors

| Console message | Cause |
| --- | --- |
| `No config/announcements.json found...` | The file is missing. Nothing sends itself. |
| `config/announcements.json could not be parsed...` | Broken JSON. Nothing sends itself. |
| `... has to hold an object with an announcements list...` | The `announcements` list is missing. |
| `Skipping the announcement '...' in ...` | The reason follows, such as both or neither of `everyMinutes` and `at`. |

Read at server start. Restart the resource after editing the file by hand.
