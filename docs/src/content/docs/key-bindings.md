---
title: "Key Bindings"
description: "The keys vMenu Enhanced uses by default, and how you and your players can change them."
---

vMenu Enhanced puts a handful of things on keys. Every one of them is a real FiveM key binding, which
means two useful things. Your players can rebind any of them for themselves under **Settings, Key
Bindings, FiveM** in the pause menu, and for some of them you can change the starting key for
everybody with a convar in your `server.cfg`.

## The defaults

| What it does | Keyboard | Controller | Convar |
| --- | --- | --- | --- |
| Open and close the menu | `M` | not bound | `vMenu.Enhanced.KeyBindings.MenuToggleKey` |
| Turn noclip on and off | `F2` | not bound | `vMenu.Enhanced.KeyBindings.NoClipToggleKey` |
| Run the teleport the player picked | `F10` | not bound | `vMenu.Enhanced.KeyBindings.TeleportKey` |
| Flip a helmet visor, held | `F11` | D-pad right | none, see below |
| Expand or zoom the minimap | `Z` | D-pad down | none, see below |
| Point your finger | `B` | right stick click | none, see below |
| Character creator auto camera | `N` | right bumper | none, see below |

Noclip also uses `W`, `S`, `A`, `D`, `Q`, `Z`, `Left Shift`, `Left Ctrl` and `H` while it is switched
on. Those are ordinary bindings too, so they show up in the same settings list and can be changed the
same way.

## The keys with no convar

The minimap, visor, pointing and auto camera keys have no convar, and they won't get one either. They are personal
comfort keys rather than anything a server needs a say in, the defaults match what GTA Online already
uses, and every player can move them for themselves in the pause menu settings. That is the right
place for them, so there is nothing here for you to set.

## The minimap key

Your players can choose what the key does under **Misc Settings**. Depending on what option they
chose there, the radar will either expand or zoom out. The effect lasts for 10 seconds or until the
key is pressed again.

## The visor key
Some helmets on the multiplayer peds have a visor or gadget that can be flipped up or down. Holding down
the Visor key while on foot or on a motorcyle will allow you to flip your visor/gadget. Just like in GTA Online.

## The auto camera key

This one only does anything while the character creator is open. It turns the creator's automatic
camera on and off, so the camera either follows whatever you are editing or stays exactly where you
put it. The same switch sits at the top of the creator page, called **Disable Auto Camera**, and both
of them remember your choice between sessions.

## The pointing key

Press it once to start pointing your finger at whatever you are looking at, and once again to stop,
the same gesture GTA Online has. Your players switch it on under **Misc Settings**, and it does
nothing for anybody who leaves it off.

## Changing a default

Put the convar in your `server.cfg` **before** the line that starts vMenu Enhanced:

```ini
setr vMenu.Enhanced.KeyBindings.TeleportKey "F7"
setr vMenu.Enhanced.KeyBindings.NoClipToggleKey "F3"
ensure vMenu.Enhanced
```

Key names come from the
[FiveM keyboard list](https://docs.fivem.net/docs/game-references/input-mapper-parameter-ids/keyboard/).
The keys you can set here are keyboard only, so there is no controller button to go with them.

:::caution[A default only applies to players who never changed it]
FiveM remembers each player's own choice and that choice always wins. So if somebody has already
rebound a key by hand, changing the convar does nothing for them, and they will have to change it
back themselves. This matters most for the teleport key, which used to be `F11` and is now `F10`.
Anybody who deliberately set teleport to `F11` in an older build keeps it there, and it will now sit
on top of the visor key until they move one of the two.
:::
