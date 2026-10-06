---
title: "weather-cycle.json"
description: "Your own weather schedule, with optional snow and scheduled blackouts."
---

## Picking a schedule

```ini
setr vMenu.Enhanced.WeatherOptions.WeatherCycle "default"
```

| Value | Schedule |
| --- | --- |
| `default` | The GTA Online schedule. |
| `snowy` | A winter schedule with snow on the ground and cold temperatures. |
| `custom` | Your own schedule from `config/weather-cycle.json`. Only this one can schedule blackouts. |

Staff with `vMenu.Enhanced.Menus.WeatherOptions.WeatherCycle` can also switch it from the **Weather Cycle** row in the world menu, and SnowstormBot can switch it from the dashboard. A switch from there lasts until the server restarts, after which your config value applies again.

Changes apply live. The file is only read while `vMenu.Enhanced.WeatherOptions.Enabled` is on, and it is read again whenever either setting changes or the resource restarts.

## Format

```json
{
  "snowPass": false,
  "cycle": [
    { "hours": 4, "weather": "EXTRASUNNY" },
    { "hours": 2, "weather": "RAIN" },
    { "hours": 1, "weather": "THUNDER", "blackout": "city" }
  ]
}
```

The blocks play from top to bottom and then repeat.

| Field | Meaning |
| --- | --- |
| `hours` | How long the block lasts, in in-game hours. Any number above 0. One in-game hour is 2 real minutes at normal speed. |
| `weather` | `CLEAR`, `EXTRASUNNY`, `CLOUDS`, `OVERCAST`, `RAIN`, `CLEARING`, `THUNDER`, `SMOG`, `FOGGY`, `XMAS`, `SNOW`, `SNOWLIGHT`, `BLIZZARD`, `HALLOWEEN`, `NEUTRAL`, `RAIN_HALLOWEEN` or `SNOW_HALLOWEEN`. |
| `blackout` | Optional. `city` turns off the city lights. `all` turns off the city and vehicle lights. Leave it out for no blackout. |
| `snowPass` | `true` keeps snow on the ground and the temperatures cold for the whole schedule, while snow is set to automatic. |

Comments and trailing commas are allowed.

## Blackouts

Scheduled blackouts only apply while the menu's Blackout option is on **Follow Weather Schedule**, which is the default. Picking Off, City or All overrides the schedule. Scheduled blackouts are also paused while an admin has forced a weather.

## Errors

A broken block is skipped and the reason is logged in the server console. If the file is missing or broken, or no block is usable, the default schedule is used instead.
