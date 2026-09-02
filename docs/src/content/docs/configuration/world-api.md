---
title: "World API"
description: "Let your own tools read the weather, time, date and moon phase over HTTP."
---

vMenu decides what the sky is doing. It picks the weather, it runs the clock, and it works out the date and the moon phase from that clock. All of that lives inside the game, which is a problem the moment you want something outside the game to know about it. A Discord bot that posts "rain in twenty minutes", a website that shows the in-game time, a companion app that says whether tonight is a full moon: none of them can see any of it.

The **World API** is a small web address on your server that answers exactly that question. You ask it, and it hands back a single block of data describing the time, the date, the moon, the current weather and what is coming next.

It only ever reads. Nothing you send to it can change the weather, the time, or anything else.

:::note
This is for people building their own tools. If you just want to play, or to change the weather from the menu, you can happily ignore this whole page.
:::

## Switching it on

The endpoint is off until you give it a password. That password is called a **token**, and it is just a long piece of random text that only you and your tools know.

Add this to your server config, replacing the text between the quotes with your own random string:

```cfg
set vMenu.Enhanced.WorldApi.Token "a-long-random-string-nobody-can-guess"
```

Then restart the resource.

:::caution
Notice it says `set` and not `setr`. That matters. `setr` would send the token to every player who joins, which would hand it to exactly the people it is meant to keep out. Keep it as `set`, and treat it like a password: do not paste it into a public Discord, and do not commit it to a public repository.
:::

Leave the token empty, or leave the line out entirely, and the endpoint answers every request with a polite refusal instead. That is the default.

## Where it lives

```
http://your-server-address:30120/vMenu.Enhanced/world
```

It runs on the **same port your game server already uses**, so there is nothing new to open in your firewall and nothing new to forward on your router. `30120` is the usual FiveM port. If you changed yours, use that number instead.

## Asking it something

Send the token in a header called `X-vMenu-Token`:

```bash
curl -H "X-vMenu-Token: a-long-random-string-nobody-can-guess" \
  http://your-server-address:30120/vMenu.Enhanced/world
```

Some tools make headers awkward. If yours is one of them, you can put the token in the address instead:

```bash
curl "http://your-server-address:30120/vMenu.Enhanced/world?token=a-long-random-string-nobody-can-guess"
```

The header is the better of the two, because addresses tend to end up written down in logs and browser history while headers usually do not.

### Asking for a longer forecast

By default you get the next **10** weather changes. Add `forecast` to the address to ask for a different number, anywhere from `0` to `48`:

```
http://your-server-address:30120/vMenu.Enhanced/world?forecast=24
```

## What comes back

A block of [JSON](https://en.wikipedia.org/wiki/JSON), which is a plain text format that essentially every programming language can read without any extra work. It looks like this:

```json
{
  "utc": 1756800000,
  "sync": { "weather": true, "time": true },
  "clock": {
    "speed": 1.0,
    "frozen": false,
    "frozenAtUnix": null,
    "offsetSeconds": 0,
    "secondOfDay": 43200,
    "hour": 12,
    "minute": 0,
    "second": 0,
    "realSecondsPerGameHour": 120.0
  },
  "date": {
    "dayOfLoop": 12,
    "loopDays": 385,
    "year": 2000,
    "month": 1,
    "day": 13,
    "weekday": "Thursday"
  },
  "moon": {
    "dayOfCycle": 12.5,
    "cycleDays": 55.0,
    "phase": "waxing crescent",
    "illumination": 0.43,
    "angleDegrees": -95.4
  },
  "weather": {
    "override": null,
    "scheduled": "EXTRASUNNY",
    "current": "EXTRASUNNY",
    "next": "CLOUDS",
    "gameHoursUntilNext": 2.5,
    "realSecondsUntilNext": 300.0,
    "cycleGameHours": 123.45,
    "cycleLengthGameHours": 384.0,
    "blackout": "off",
    "snow": "auto",
    "snowFalling": false
  },
  "forecast": [
    {
      "type": "CLOUDS",
      "gameHoursUntilStart": 2.5,
      "realSecondsUntilStart": 300.0,
      "gameHoursLong": 4.0,
      "realSecondsLong": 480.0
    }
  ]
}
```

### The top level

| Field | What it means |
| --- | --- |
| `utc` | The server's own clock, as the number of seconds since the 1st of January 1970. That is the standard way computers write a moment in time, and every language has a way to turn it back into a date. |
| `sync` | Whether vMenu is actually in charge. If `weather` is `false` something else on your server is driving the sky, and the weather below is what vMenu *would* be showing rather than what players are seeing. Same idea for `time`. |

### `clock`

| Field | What it means |
| --- | --- |
| `speed` | How much faster than normal the in-game clock runs. `1.0` is the game's own pace. |
| `frozen` | Whether somebody has stopped the clock from the menu. |
| `frozenAtUnix` | The moment it was stopped, or `null` when it is running. |
| `offsetSeconds` | How far the in-game clock has been nudged away from where the schedule would put it. |
| `secondOfDay` | How far through the in-game day it is, in seconds, from `0` at midnight to `86400`. |
| `hour`, `minute`, `second` | The same thing, split up, so you can print it without doing any sums. |
| `realSecondsPerGameHour` | How many real seconds one in-game hour takes at the current speed. `120` at normal speed, so a full in-game day takes 48 real minutes. |

### `date`

The in-game calendar. GTA does not really have one, so vMenu keeps its own: a 385 day loop that always leaves the weekday and the moon phase lining up when it wraps around.

| Field | What it means |
| --- | --- |
| `dayOfLoop` | Which day of that loop it is. |
| `loopDays` | How long the loop is, always `385`. |
| `year`, `month`, `day` | The date those days work out to. |
| `weekday` | The day of the week, written out. |

### `moon`

| Field | What it means |
| --- | --- |
| `dayOfCycle` | How far through the moon's cycle it is, in days. |
| `cycleDays` | How long that cycle is, always `55` in-game days. |
| `phase` | The phase written out, such as `full moon` or `waning gibbous`. |
| `illumination` | How much of the moon is lit, from `0` at new moon to `1` at full. |
| `angleDegrees` | The angle the game tilts the moon by, if you want to draw it yourself. `0` is a full moon. |

### `weather`

| Field | What it means |
| --- | --- |
| `override` | The weather somebody forced from the menu, or `null` when the schedule is running normally. |
| `scheduled` | What the schedule says the weather is, whether or not anybody has overridden it. |
| `current` | What players are actually seeing. The override if there is one, otherwise the scheduled weather. This is the one to show people. |
| `next` | The weather the schedule moves to next. |
| `gameHoursUntilNext` | How many in-game hours away that change is. |
| `realSecondsUntilNext` | How many real seconds away it is, which already accounts for the clock speed. This is the one to count down with. |
| `cycleGameHours` | How far through the weather schedule the server is. |
| `cycleLengthGameHours` | How long the whole schedule is, always `384` in-game hours. |
| `blackout` | Whether street lighting is cut: `off`, `city`, or `all`. |
| `snow` | The snow setting: `auto`, `on`, or `off`. |
| `snowFalling` | Whether that setting works out to snow actually falling right now. |

:::note
Forcing a weather type does not pause the schedule underneath. It carries on ticking, which is why `scheduled` and `next` still change while an override is in place. Hand the weather back to the schedule and it picks up wherever it has got to.
:::

### `forecast`

A list of the weather changes still to come, in order. Each entry says what the weather turns into, how long until it starts, and how long it lasts. Every one of those is given twice, once in in-game hours and once in real seconds, so you never have to do the clock speed maths yourself.

## When something goes wrong

| Code | What happened |
| --- | --- |
| `401` | The token was missing or wrong. |
| `404` | The address was not `/world`. |
| `405` | You used something other than a `GET` request. |
| `503` | The endpoint is switched off, because `vMenu.Enhanced.WorldApi.Token` is empty. |

Wrong tokens are noted in your server console, at most one line every ten seconds so somebody poking at it cannot bury everything else.
