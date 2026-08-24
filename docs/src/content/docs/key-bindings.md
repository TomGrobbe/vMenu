---
title: "Key Bindings"
description: "The keys vMenu Enhanced uses by default, and how you and your players can change them."
---

Everything vMenu Enhanced puts on a key is a real FiveM key binding. Players can rebind any of them for themselves under **Settings, Key Bindings, FiveM** in the pause menu, and for some of them you can change the starting key for everybody with a convar.

## The defaults

| What it does | Keyboard | Controller | Convar |
| --- | --- | --- | --- |
| Open and close the menu | `M` | not bound | `vMenu.Enhanced.KeyBindings.MenuToggleKey` |
| Turn noclip on and off | `F2` | not bound | `vMenu.Enhanced.KeyBindings.NoClipToggleKey` |
| Run the teleport the player picked | `F10` | not bound | `vMenu.Enhanced.KeyBindings.TeleportKey` |
| Flip a helmet visor, held | `F11` | D-pad right | none |
| Expand or zoom the minimap | `Z` | D-pad down | none |
| Point your finger | `B` | right stick click | none |
| Character creator auto camera | `N` | right bumper | none |

Noclip also uses `W`, `S`, `A`, `D`, `Q`, `Z`, `Left Shift`, `Left Ctrl` and `H` while it is switched on. Those are ordinary bindings too, so they show up in the same settings list.

## Changing a default

Put the convar in your `server.cfg` **before** the line that starts vMenu Enhanced:

```ini
setr vMenu.Enhanced.KeyBindings.TeleportKey "F7"
setr vMenu.Enhanced.KeyBindings.NoClipToggleKey "F3"
ensure vMenu.Enhanced
```

Key names come from the [FiveM keyboard list](https://docs.fivem.net/docs/game-references/input-mapper-parameter-ids/keyboard/). These are keyboard only, there is no controller button to go with them.

:::caution[A default only applies to players who never changed it]
FiveM remembers each player's own choice and that choice always wins. Somebody who already rebound a key by hand keeps their key, and has to change it themselves. This matters most for the teleport key, which used to be `F11` and is now `F10`. Anybody who deliberately set teleport to `F11` in an older build now has it sitting on top of the visor key until they move one of the two.
:::

## The keys with no convar

The minimap, visor, pointing and auto camera keys have no convar and will not get one. They are personal comfort keys rather than something a server needs a say in, the defaults match GTA Online, and every player can move them in the pause menu.

- **Minimap.** Players choose under **Misc Settings** whether the key expands the radar or zooms it out. The effect lasts 10 seconds, or until the key is pressed again.
- **Visor.** Some multiplayer ped helmets have a visor or gadget. Hold the key while on foot or on a motorcycle to flip it, just like GTA Online.
- **Pointing.** Press once to point your finger at whatever you are looking at, press again to stop. Players switch it on under **Misc Settings**, and it does nothing while it is off.
- **Auto camera.** Only does something while the character creator is open. It is the same switch as **Disable Auto Camera** at the top of the creator page, and the choice is remembered between sessions.
