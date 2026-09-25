---
title: "Integrations"
description: "Connect vMenu Enhanced to outside tools like SnowstormBot."
---

vMenu Enhanced can be linked to outside tools that watch and manage the resource from somewhere else.

## SnowstormBot (Discord)

[SnowstormBot (Discord)](https://snowstormbot.net) is a Discord bot that connects to your FiveM server through vMenu Enhanced. Once it is linked, it can show your server's online status and player count in Discord and on its dashboard.

### Currently implemented features

- Show your server's online status and player count in Discord and on the dashboard
- Live player map
- Control players (kick, set health, armor, toggle noclip, send messages, teleportation, etc.)
- Spawn vehicles
- Manage weather and time
- Send in-game announcements
- Discord role based allowlist, so only members with the right roles can join
- Join queue with priority queues, spike protection, a bypass and a reconnect window, plus a live queue embed in Discord
- Discord role sync to in-game permission groups (needs `AllowActions` and the two `add_ace` lines from [Getting started](/vmenu/enhanced/getting-started/))

### Coming soon

- Live player map split into different views per Discord role (eg. Admins can see everyone, Dispatchers can only see certain people)
- Lockdown/maintenance mode, which requires a password to join the server

Setting it up is done entirely on SnowstormBot's side, so follow their guide:

- [Connect a FiveM server (vMenu Enhanced)](https://docs.snowstormbot.net/features/vmenu-integration)

Using the vMenu integration on SnowstormBot requires a paid subscription. The paid subscriptions are currently in early access, and will become available to everyone at a later date.

## Building your own

SnowstormBot is the recommended option if you are a server owner and you want to get up and running right away. If you are a developer, you can build your own tool against the same endpoints that vMenu exposes. Keep in mind that no support will be provided for developing or using any external integration other than SnowstormBot.

- [Custom integrations](/vmenu/enhanced/integrations/custom/)
