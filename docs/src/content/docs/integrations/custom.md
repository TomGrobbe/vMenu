---
title: "Custom integrations"
description: "The minimal setup for building your own tool against vMenu Enhanced."
---

SnowstormBot (Discord) is the recommended option if you are a server owner and you just want to get up and running right away. 
See the [Integrations](/vmenu/enhanced/integrations/) page for that.

If you are a developer, you can build your own tool against the same endpoints. Keep in mind that no support will be provided for developing or using any external integration other than SnowstormBot. This page only shows that it is possible.

## How it works

Your FiveM server dials out to your tool. Your tool never calls the server. That means all you need on your side is a public HTTPS address that vMenu can reach.

## Convars to set

These are all secret server convars, so use `set` (not `setr`) and keep them out of anything public.

```cfg
set vMenu.Enhanced.Integration.ApiKey "a long random secret"
set vMenu.Enhanced.Integration.Endpoint "https://your-tool.example.com/api"
set vMenu.Enhanced.Integration.AllowActions true
```

`AllowActions` is optional and off by default. It has to be on before your tool is allowed to run any command that changes the game. Read only commands work without it.

## How the auth works

vMenu signs every message with HMAC-SHA256. HMAC is just a way to prove a message came from someone who knows the shared key, without ever sending the key itself. 
Your key stays on the server, and vMenu only ever sends signatures.

Every outbound request, and the websocket handshake, carries these headers.

- `X-vMenu-Timestamp`, the current time in seconds.
- `X-vMenu-Nonce`, a one time random value.
- `X-vMenu-Signature`, in the form `v1=<hex>`.
- `X-vMenu-Key-Id`, the `sha256` of the key, so you know which server's key to check the signature against.

The signature is an HMAC-SHA256 of this text, using your key.

```
scheme\naction\ntimestamp\nnonce\nbodyHashHex
```

`bodyHashHex` is the `sha256` of the request body, or the hash of an empty body for the websocket handshake. To verify, rebuild that same text on your side and recompute the HMAC with your copy of the key. If it matches the header, the message is genuine.

## What comes over HTTPS

vMenu sends these to your `Endpoint` as `POST` requests.

- `POST {Endpoint}/connect/status`, once when the server starts and then every 10 minutes. Body: `{ Resource, Version, Name, MaxPlayers, Count }`.
- `POST {Endpoint}/players/changed`, whenever the player count changes. Body: `{ Resource, Version, Count, Players }`.

## What comes over the websocket

vMenu opens a websocket to your `Endpoint` with the scheme swapped (`https` becomes `wss`, `http` becomes `ws`) and `/socket` added on the end. So `https://your-tool.example.com/api` becomes `wss://your-tool.example.com/api/socket`.

Every message is a small envelope.

```json
{ "type": "world", "payload": {} }
```

vMenu sends you:

- `world`, whenever the weather or time state changes, and at least once every 15 seconds.
- `players`, live player positions for the map, only after you ask for them.
- `blips`, the configured blips, sent once.
- `command-ack`, the result of a command you sent.

You send vMenu:

- `start-stream` and `stop-stream`, to turn the live position stream on and off.
- `command`, with an `id` and a `payload`, to make something happen.
- `ping` and `pong`, to keep the connection alive.

A `command` can be server wide (`announce`, `get-world`, `set-weather`, `set-time`, `set-blackout`, `set-snow`, `set-freeze`, `get-config`, `set-convar`) or aimed at one player (`kick`, `kill`, `noclip`, `notify`, `waypoint`, `teleport`, `heal`, `armor`, `spawnvehicle`). The read only ones (`get-world`, `get-config`) always work. Anything that changes the game needs `AllowActions true`.

## Data models

A `world` payload looks roughly like this (trimmed).

```json
{
  "Utc": 1757000000,
  "Clock": { "Hour": 13, "Minute": 5, "Speed": 30 },
  "Weather": { "Current": "CLEAR", "Next": "RAIN" },
  "Forecast": [{ "Type": "RAIN", "RealSecondsUntilStart": 240 }]
}
```

A player looks like this.

```json
{ "ServerId": 3, "Name": "Some Player", "Ping": 42, "Discord": "123456789" }
```

The `players` frame carries live positions for the map. We are not going to walk through the map here.

## Just want weather and time

If all you need is the current weather, time, date and moon, there is a simpler option that does not need any of the above. vMenu can serve that over a plain `GET /world` request guarded by its own token. See the [World API](/vmenu/enhanced/configuration/world-api/) page.

## Support

External integrations other than SnowstormBot will not receive support. If you want something that just works, use SnowstormBot. Use of any other external integrations is at your own risk.
