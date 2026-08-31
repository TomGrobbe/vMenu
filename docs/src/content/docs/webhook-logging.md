---
title: "Webhook Logging"
description: "Send a record of what happens on your server to Discord, or to your own tooling as plain JSON. What each channel logs, how to set it up, and what the settings do."
---

vMenu can post a running record of what happens on your server to Discord, so it survives a restart and your whole staff team can search it.

It is off by default. Nothing is sent until you fill in a webhook URL.

## The three channels

Each goes to its own webhook. Point them all at one channel, use one, or use none. An empty URL means that channel is not logged.

**Events**, things that happen to the server:

- Players connecting, joining and leaving, with the reason where there is one
- Deaths, and who did the killing
- Weather, time, blackout and snow changes
- vMenu starting and stopping
- Plugins being added or removed
- Menu themes, once each, the first time the server hears about one

**Actions**, the things a player does to themselves that somebody might argue about later:

- God mode, invisibility, no ragdoll, super jump, fast run, fast swim, unlimited oxygen
- Never wanted, setting their own wanted level, armour, healing themselves, killing themselves
- Getting every weapon, removing every weapon, unlimited ammo, no reload, refilling ammo, spawning a weapon by name
- Vehicle invincibility, the engine power and torque multipliers, repairing, deleting and hiding their vehicle
- Turbo and bulletproof tyres, and every other mod they change, summed up in one line
- Personal vehicle actions: claiming one, kicking everybody out of it, locking it, starting it, sounding the horn, blowing it up, deleting it
- Spawning a vehicle, whether from the spawner menu or from their saved vehicles
- Equipping a weapon loadout, listing every weapon it actually handed over
- Noclip going on and off, including when the key is used rather than the menu
- Teleporting to a waypoint, to typed coordinates, or to a saved location, key or menu either way

Everything else a menu can do is deliberately not logged. Opening the about page, exporting a backup, changing a colour or scrolling a list tells you nothing useful, so those never leave the player's game at all.

If you want something added to this list, open an issue. The list lives in the code, not in a setting.

**Staff**, what a player does to somebody else. Kicking, killing, summoning, messaging, wanted levels, freezing, carrying, lending noclip, deleting vehicles, clearing areas, announcing.

Staff alerts live here too. You get a line when somebody raises one, with their reason, and another when a staff member answers or dismisses it, naming both people. An alert nobody answers before it runs out is logged as well, so a missed call for help leaves a trace.

Refused attempts are logged here too. If somebody without the kick permission tries to kick, you see it.

The player on the receiving end is read before the action runs rather than after, so somebody who was kicked is still named properly in the line that says they were kicked.

## Setup

**1. Make a webhook.** In Discord: **Edit Channel** → **Integrations** → **Webhooks** → **New Webhook** → **Copy Webhook URL**.

The URL is a password. Anybody holding it can post to that channel without being in your server.

**2. Add it to `configuration.cfg`:**

```ini
setr vMenu.Enhanced.Logging.Enabled true

set vMenu.Enhanced.Logging.Webhook.Events "https://discord.com/api/webhooks/..."
set vMenu.Enhanced.Logging.Webhook.Actions "https://discord.com/api/webhooks/..."
set vMenu.Enhanced.Logging.Webhook.Staff "https://discord.com/api/webhooks/..."
```

:::caution[`set`, not `setr`]
`setr` sends a value to every player who connects. The webhook URLs use plain `set` so they stay on the server. Changing one to `setr` hands your webhook to everybody who joins.
:::

**3. Restart, then run `vmenu_webhook_test`** in the server console. It puts one test line in each channel.

## Generic webhook

For your own tooling instead of a chat channel:

```ini
set vMenu.Enhanced.Logging.Webhook.Generic "http://127.0.0.1:3000/vmenu"
```

Plain JSON, not Discord. It never retries, ignores whether anything answered, and skips certificate checks, so a bare IP or a self signed certificate works. Because certificates are not checked, only point it at something you control.

Events are posted in batches:

```json
{
  "resource": "vMenu.Enhanced",
  "version": "1.2.3",
  "sentAt": "2026-08-31T20:36:00.1200000+00:00",
  "events": [
    {
      "category": "staff",
      "message": "kicked",
      "timestamp": "2026-08-31T20:35:58.4000000+00:00",
      "actor": {
        "name": "Tom",
        "serverId": 12,
        "identifiers": { "discord": "123456789", "steam": "110000100000000", "license": "abc123", "license2": "def456" }
      },
      "targets": [
        { "name": "Bob", "serverId": 9, "identifiers": { "discord": null, "steam": null, "license": "ghi789", "license2": null } }
      ],
      "data": {}
    }
  ]
}
```

`category` is `event`, `action` or `staff`. `targets` is empty when nobody was targeted. Missing identifiers are `null`.

## What a line looks like

Each line starts with a bullet and the date and time, then says who did what. Anything extra sits underneath in Discord's small grey text, so the sentence stays the part you read:

```
• 31/08/2026 22:40:35 Vespura (1) turned god mode on.
   menu: Player options
• 31/08/2026 22:41:02 Vespura (1) spawned the vehicle 'Adder'.
• 31/08/2026 22:41:20 Vespura (1) teleported to their waypoint.
```

The time is sent as a Discord timestamp, which means everybody reading the channel sees it in their own timezone. It matches the time Discord itself prints under the message rather than fighting with it.

## Who did it

Every line names the player and their server id, like `Vespura (1)`. Server ids get reused, so the id alone is not enough to find somebody tomorrow.

That is what the identifiers are for, and they are attached to the join and leave lines only:

```
• 31/08/2026 22:39:58 Vespura (1) joined the server.
   discord: 123456789 · license: abc123 · license2: def456
```

One line at the top of somebody's session ties their name and server id to their identifiers, and everything they do afterwards stays short and readable. Only the identifiers that exist are shown.

:::danger[Personal information]
Everyone who can read the channel can read these identifiers. Keep these channels staff only.
:::

## Settings

| Setting | What it does |
| --- | --- |
| `vMenu.Enhanced.Logging.Enabled` | Master switch. Default `false`. |
| `vMenu.Enhanced.Logging.Webhook.Events` | Events webhook URL. |
| `vMenu.Enhanced.Logging.Webhook.Actions` | Actions webhook URL. |
| `vMenu.Enhanced.Logging.Webhook.Staff` | Staff webhook URL. |
| `vMenu.Enhanced.Logging.Webhook.Generic` | Plain JSON endpoint. Certificates are not checked. |
| `vMenu.Enhanced.Logging.FlushSeconds` | How often queued lines are sent. Default 2, clamped to 1 to 60. |
| `vMenu.Enhanced.Logging.QueueLimit` | Lines held per webhook while waiting. Default 500. |
| `vMenu.Enhanced.Logging.MenuActionLimit` | Menu actions logged per player per window. Default 30, `0` disables the limit. |
| `vMenu.Enhanced.Logging.MenuActionLimitSeconds` | Length of that window. Default 10. |

The four `Webhook.*` options use `set`. Everything else uses `setr`.

## Behaviour

- **Batched, not one message per event.** Discord rate limits fast posting. When it does refuse a batch, vMenu waits as long as Discord asks and retries.
- **Failures back off and give up.** Only `QueueLimit` lines are held meanwhile, and the next message through says how many were dropped.
- **A wrong URL switches itself off.** On a `401`, `403` or `404`, vMenu stops sending and says so once. Fix the setting and it resumes without a restart.
- **Your console will not fill up.** A failing webhook is reported once, then stays quiet for five minutes at a time.
- **Nothing is logged twice.** Anything a player does to somebody else, or to the world, is recorded on the server with the real target and outcome, so it lands in the staff or events channel instead of the actions one.
- **You are told when the log has a gap.** A player who trips `MenuActionLimit` gets a warning line in the actions channel, marked with a warning sign, saying the rest of what they do is not being logged. You get one of those per window at most, so a spammer cannot flood the channel with warnings either.
- **Scrolling is not an action.** Only committing to something logs. Browsing menus, arrowing through lists and dragging sliders do not.
- **Vehicle mods are summed up, not counted.** Parts go on the car as you scroll past them, so logging each one would fill the channel with things the player only glanced at. Instead you get a single line when they leave the mods menu, listing every slot that ended up different from how they found it.
- **Filtered before it is sent.** A player's game only reports the handful of actions listed above, so the rest never crosses the network to begin with.

## Nothing arriving?

- Is `vMenu.Enhanced.Logging.Enabled` set to `true`?
- Is your `configuration.cfg` `exec`d **above** the line that starts vMenu?
- Run `vmenu_webhook_test` and read the output.
- Run `vmenu_config`. The webhook options should say `set (hidden)`. `unset` means the server never read them.
- Look for `[Webhooks]` lines in the server console.
