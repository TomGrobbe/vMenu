---
title: "Running on Linux"
description: "Why vMenu Enhanced sends its internet traffic through a small JavaScript helper on Linux, and how to switch that by hand."
---

On a Linux server running FiveM Enhanced, vMenu's C# code cannot currently check the security certificates of websites. That breaks everything vMenu sends over the internet, like update checks, webhook logging and integrations. This is a FiveM bug, and it has been reported.

Until it is fixed, vMenu hands that traffic to a small JavaScript helper that ships with it, `server/net_bridge.js`. JavaScript on the same server does not have the bug. This happens by itself, you do not need to set anything up. Your console shows a line saying the net bridge is in use when it starts.

## Switching it by hand

If you ever need to choose yourself, add one of these to your `server.cfg`:

```ini
set vMenu.Enhanced.NetBridge native
set vMenu.Enhanced.NetBridge js
```

`native` always uses the normal C# code, and `js` always uses the helper, also on Windows. Leave the line out and vMenu picks by itself.

:::note[On FiveM Legacy]
vMenu on FiveM Legacy always uses the helper, on Linux and on Windows alike, so this setting does nothing there.
:::
