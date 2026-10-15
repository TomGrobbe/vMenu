---
title: "Exports"
description: "Functions other resources can call to read information from vMenu."
---

:::note
This page is for people writing their own scripts. If you just run a server, you can skip it.
:::

All of these run on the **client**, so call them from a `client_script`

### `GetTemperature`

The temperature from the player's location.

```lua
local celsius = exports['vMenu.Enhanced']:GetTemperature()

if celsius and celsius < 0 then
    print('It is freezing out here.')
end
```

### `GetTemperatureAtHeight`

The temperature at a height you choose, in metres above sea level.

```lua
local ped = GetPlayerPed(GetPlayerFromServerId(serverId))
local position = GetEntityCoords(ped)

local celsius = exports['vMenu.Enhanced']:GetTemperatureAtHeight(position.z)
```

### Good to know

1. Both can give back `nil` for a short moment right after vMenu starts, before it has worked out the first value. Always check for `nil` before you use the number.
2. vMenu refreshes the value twice per second, (or faster when weather and time are changing, but expect twice per second to be safe) so calling these often is cheap.
3. The value is a decimal number, such as `23.4`. Round it yourself if you only want whole degrees.
4. The temperature value is in celsius.
