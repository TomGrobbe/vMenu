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

Noclip also uses `W`, `S`, `A`, `D`, `Q`, `Z`, `Left Shift`, `Left Ctrl` and `H` while it is switched
on. Those are ordinary bindings too, so they show up in the same settings list and can be changed the
same way.

## About the visor key

This one is held rather than tapped, and there is a good reason for it.

On a controller the default is D-pad right, which is also the button that works your headlights when
you are on a bike. That is not an accident. It is what GTA itself does: tap the button for the
headlights, hold it for your visor. vMenu does the same, and while it is working out which one you
meant it holds the headlights off for that fraction of a second so a hold does not flick them on and
off on the way past.

It only does anything if you are wearing a helmet that actually has a visor. Most hats do not, and
pressing the key while wearing one of those does nothing at all, which is not an error.

Goggles are a special case. The game only has an animation for pushing goggles up while you are
standing, so trying it while riding tells you so rather than playing the wrong animation.

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
