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

- `POST {Endpoint}/connect/status`, once when the server starts and then every 10 minutes. Body: `{ Resource, Version, Name, MaxPlayers, Count, ActionsEnabled, PrincipalCommandsAllowed }`. The last two tell you whether `AllowActions` is on and whether the two `add_ace` lines that [role sync](#role-sync) needs are in place.
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
- `buckets`, the named routing bucket worlds from the optional Routing Buckets plugin. Sent once when the stream starts and again when the list changes. Empty without that plugin.
- `command-ack`, the result of a command you sent.

You send vMenu:

- `start-stream` and `stop-stream`, to turn the live position stream on and off.
- `command`, with an `id` and a `payload`, to make something happen.
- `ping` and `pong`, to keep the connection alive.

A `command` can be server wide (`announce`, `get-world`, `set-weather`, `set-time`, `set-blackout`, `set-snow`, `set-freeze`, `get-config`, `set-convar`, `waypoint-everyone`, `teleport-everyone`) or aimed at one player (`kick`, `kill`, `noclip`, `notify`, `waypoint`, `teleport`, `heal`, `armor`, `spawnvehicle`). The read only ones (`get-world`, `get-config`) always work. Anything that changes the game needs `AllowActions true`.

`waypoint-everyone` and `teleport-everyone` are the server wide versions of `waypoint` and `teleport`, both taking a spot as `params.x` and `params.y`. `teleport-everyone` spreads players out around the spot so a crowd does not stack on one point. Both need `AllowActions true` and answer with `{ "ok": true, "reached": <count> }`.

The routing bucket commands (`world-create`, `world-rename`, `world-settings`, `world-delete`, `world-move-one`, `world-move-all`, `world-move-ids`, `world-empty`, `world-transfer`) manage the named worlds and move players between them. They need `AllowActions true` and the optional Routing Buckets plugin, without it they answer with `plugin-unavailable`.

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

The `players` frame carries live positions for the map (not covered here). Each player also has a `RoutingBucket`, the number of the world they are in.

A `buckets` payload lists the named worlds from the Routing Buckets plugin.

```json
{ "buckets": [ { "id": 0, "name": "Main World" }, { "id": 3, "name": "Race world" } ] }
```

Only named worlds appear here. Unnamed worlds are left out, but you can still read their number from the players. Empty without the plugin.

## Join gates: allowlist and queue

Your integration can also decide who joins the server, and hold people in a queue. These are turned on by two server convars, and everything about how they behave lives in your integration, not in vMenu.

```cfg
set vMenu.Enhanced.Integration.Allowlist true
set vMenu.Enhanced.Integration.Queue true
```

When either is on, vMenu holds each connecting player on the loading screen and asks you what to do. The player who holds the `vMenu.Enhanced.Integration.Allowlist.Bypass` or `vMenu.Enhanced.Integration.Queue.Bypass` permission on the server passes that gate without you being asked, so an owner always has a way in even if your tool is offline.

### Keep each gate doing what its name says

Server owners switch these on by name, so they should get exactly what the name promises, without having to guess what your integration does behind it.

- **The allowlist is only an allowlist.** It answers one question: may this player join, yes or no? That can be a list of who is allowed, a ban list, or both. It never makes people wait.
- **The queue is only a queue.** It decides when a player gets in and in what order, for example with priorities or join spike protection. It never decides whether someone is allowed at all, so don't reject players from the queue for failing an allowlist check.

So don't build a whitelist application into the join queue, and don't turn the allowlist into a waiting line. If your tool needs something that fits neither, give it its own setting in your tool instead of hiding it behind one of these convars.

### Say which identifiers you want

When you accept the socket you send a `ready` frame. Add an `identifiers` list to it and vMenu will send you only those identifier types for connecting players. Leave it out and you get Discord only.

```json
{ "type": "ready", "payload": { "identifiers": ["discord"] } }
```

### The gate request

When a player connects, vMenu sends you a `gate-request`.

```json
{
  "type": "gate-request",
  "payload": {
    "id": "1f3c...",
    "serverId": 12,
    "name": "Some Player",
    "identifiers": { "discord": "123456789" },
    "allowlist": true,
    "queue": true,
    "allowlistBypass": false,
    "queueBypass": false
  }
}
```

`id` is your handle on this connection, echo it back in every reply. `allowlist` and `queue` say which gates are on. `allowlistBypass` and `queueBypass` are true when the player already passed locally through a bypass permission, so you can skip that check for them.

### Your replies

You answer with one or more `gate` frames, all carrying the same `id`. Send `waiting` updates while they are in the queue, and one final `admit` or `reject`.

```json
{ "type": "gate", "payload": { "id": "1f3c...", "state": "waiting", "queue": "Members", "position": 3, "ahead": 1 } }
{ "type": "gate", "payload": { "id": "1f3c...", "state": "admit" } }
{ "type": "gate", "payload": { "id": "1f3c...", "state": "reject", "reason": "Optional message." } }
```

For a `reject` you can send a `reason`, or leave it out and vMenu shows the server's own `vMenu.Enhanced.Integration.AllowlistMessage` convar. For a `waiting` frame you can send a ready made `message`, or just the `queue`, `position` and `ahead` numbers and vMenu builds the line for you. `position` is the player's place in their own queue and `ahead` is how many wait in higher priority queues. vMenu adds them up and shows one overall number, like "You are number 4 in line", because players only care where they stand, not which queues exist.

A `waiting` frame may also carry two extras. `delaySeconds` is that player's own estimated wait, which vMenu shows as an estimate and ticks down on its own between your updates. Send each player their own number, so someone further back does not see their wait drop to zero and jump up again. Every loading screen page also shows the current number of players on the server, which vMenu fills in itself. `canSkip: true` shows the player a Skip button, and when they press it vMenu sends you a `skip-queue` frame with their `id`, which you treat as a request to let them in now.

```json
{ "type": "gate", "payload": { "id": "1f3c...", "state": "waiting", "queue": "Members", "position": 3, "delaySeconds": 45, "canSkip": true } }
{ "type": "skip-queue", "payload": { "id": "1f3c..." } }
```

vMenu kicks a waiting player when it gets no `gate` frame for them for 30 seconds, so send one at least that often. A running `delaySeconds` countdown pauses this.

### When a player leaves

If a waiting player gives up on the loading screen, vMenu sends `gate-cancel` with their `id`. When a player who was already in leaves, and the queue is on, vMenu sends `player-left` with the freed count and the player's requested identifiers, so you can open the slot right away and run a reconnect window.

```json
{ "type": "gate-cancel", "payload": { "id": "1f3c..." } }
{ "type": "player-left", "payload": { "serverId": 12, "count": 31, "identifiers": { "discord": "123456789" } } }
```

If your socket drops while people are waiting, vMenu kicks everyone it was holding, since nothing can admit them any more.

## Role sync

Your integration can put players in ACE permission groups (called principals in FiveM). vMenu asks when a player joins, and for everyone online right after your `ready` frame. You answer with the groups each identifier should have.

```json
{ "type": "role-sync-request", "payload": { "players": [ { "serverId": 12, "identifiers": { "discord": "123456789" } } ] } }
{ "type": "role-sync", "payload": { "players": [ { "serverId": 12, "identifier": "discord:123456789", "principals": ["group.admin", "group.vip"] } ] } }
```

Always send the full list of groups the player should have, and an empty list removes them all. It needs `AllowActions true` plus the two `add_ace` lines from [Getting started](/vmenu/enhanced/getting-started/). vMenu only ever removes groups it added itself, and it takes them all away again when the player leaves.

## Just want weather and time

If all you need is the current weather, time, date and moon, there is a simpler option that does not need any of the above. vMenu can serve that over a plain `GET /world` request guarded by its own token. See the [World API](/vmenu/enhanced/configuration/world-api/) page.

## Support

External integrations other than SnowstormBot will not receive support. If you want something that just works, use SnowstormBot. Use of any other external integrations is at your own risk.
