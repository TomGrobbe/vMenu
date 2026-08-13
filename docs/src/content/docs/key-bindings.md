---
title: "Key Bindings"
description: "The keys vMenu Enhanced uses by default, and how you and your players can change them."
---

vMenu Enhanced puts a handful of things on keys. Every one of them is a real FiveM key binding, which
means two useful things. Your players can rebind any of them for themselves under **Settings, Key
Bindings, FiveM** in the pause menu, and you can change the starting key for everybody with a convar
in your `server.cfg`.

## The defaults

| What it does | Keyboard | Controller | Convar |
| --- | --- | --- | --- |
| Open and close the menu | `M` | not bound | `vMenu.Enhanced.KeyBindings.MenuToggleKey` |
| Turn noclip on and off | `F2` | not bound | `vMenu.Enhanced.KeyBindings.NoClipToggleKey` |
| Run the teleport the player picked | `F10` | not bound | `vMenu.Enhanced.KeyBindings.TeleportKey` |
| Flip a helmet visor, held | `F11` | D-pad right | `vMenu.Enhanced.KeyBindings.VisorToggleKey` and `vMenu.Enhanced.KeyBindings.VisorToggleButton` |
| Expand or zoom the minimap | `Z` | D-pad down | none, see below |

Noclip also uses `W`, `S`, `A`, `D`, `Q`, `Z`, `Left Shift`, `Left Ctrl` and `H` while it is switched
on. Those are ordinary bindings too, so they show up in the same settings list and can be changed the
same way.

## The minimap key

This one has no convar, and it won't get one either. Your players can choose what the key does
under **Misc Settings**, and they can change the keybind themselves in the pause menu settings.

Depending on what option they chose in the Misc Settings, the radar will either expand or zoom out.
The effect lasts for 10 seconds or until the key is pressed again.

## The visor key
Some helmets on the multiplayer peds have a visor or gadget that can be flipped up or down. Holding down
the Visor key while on foot or on a motorcyle will allow you to flip your visor/gadget. Just like in GTA Online.


## Changing a default

Put the convar in your `server.cfg` **before** the line that starts vMenu Enhanced:

```cfg
setr vMenu.Enhanced.KeyBindings.TeleportKey "F7"
setr vMenu.Enhanced.KeyBindings.VisorToggleKey "F8"
ensure vMenu.Enhanced
```

Key names for the keyboard come from the
[FiveM keyboard list](https://docs.fivem.net/docs/game-references/input-mapper-parameter-ids/keyboard/),
and button names for the controller come from the
[FiveM controller list](https://docs.fivem.net/docs/game-references/input-mapper-parameter-ids/pad_digitalbutton/).

:::caution[A default only applies to players who never changed it]
FiveM remembers each player's own choice and that choice always wins. So if somebody has already
rebound a key by hand, changing the convar does nothing for them, and they will have to change it
back themselves. This matters most for the teleport key, which used to be `F11` and is now `F10`.
Anybody who deliberately set teleport to `F11` in an older build keeps it there, and it will now sit
on top of the visor key until they move one of the two.
:::
