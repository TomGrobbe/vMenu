---
title: "Filmmaker Menu"
description: "The freecam, its keys, and the filmmaker tools for saved shots, camera paths and target lock."
---

The freecam detaches the camera from your character so you can fly it anywhere on the map. Your character stays where it was. The Filmmaker menu holds the tools for setting up cinematic shots with it.

## Freecam

Press `F3` to start or stop the freecam, or use the **Freecam** checkbox at the top of the Filmmaker menu. There is no distance limit and the map loads around the camera.

Hold `F3` for half a second to stop the freecam and stay where the camera is. Your character is put on the ground below the camera instead of going back. A vehicle you left behind is not brought along.

Vehicles cannot be spawned while the freecam is on.

While the freecam is on, your character is invisible and travels with the camera, because OneSync only shows players the world around their own character. Other players do not see you. When you stop, your character is put back where you left it, back in its vehicle seat if it was in one. The doors of that vehicle are locked until you are back.

Coming back from more than 300 m away fades the screen out and in while the area loads. Player blips and overhead names hide somebody in the freecam the same way they hide somebody in noclip.

The freecam and noclip cannot be on at the same time. Starting one stops the other.

## Keys

The default keys follow the Rockstar Editor. Every one of them is a normal FiveM key binding, so players can change them under **Settings, Key Bindings, FiveM** in the pause menu.

| What it does                  | Keyboard                     | Controller         |
| ----------------------------- | ---------------------------- | ------------------ |
| Start or stop the freecam     | `F3`                         | not bound          |
| Stop and stay at the camera   | hold `F3`                    | not bound          |
| Move                          | `W` `A` `S` `D`              | left stick         |
| Look around                   | mouse                        | right stick        |
| Move up / down                | `Page Up` / `Page Down`      | `RB` / `LB`        |
| Zoom in / out                 | `Numpad +` / `Numpad -`      | `RT` / `LT`        |
| Roll left / right             | `Arrow Left` / `Arrow Right` | D-pad left / right |
| Reset roll and zoom           | `R`                          | `L3`               |
| Move faster, held             | `Left Shift`                 | `A`                |
| Move slower, held             | `Left Ctrl`                  | `X`                |
| Change the speed              | mouse wheel                  | not bound          |
| Add a keyframe to the path    | `Insert`                     | not bound          |
| Delete the last keyframe      | `Delete`                     | not bound          |
| Play or stop the path         | `Space`                      | not bound          |
| Hide the HUD                  | `H`                          | not bound          |
| Show or hide the button hints | `K`                          | not bound          |

The start key can be changed for everybody with `vMenu.Enhanced.KeyBindings.FreecamToggleKey`. While a menu is open, the mouse wheel (roll controls) and `Delete` are ignored.

## Filmmaker menu

| Submenu        | What is in it                                                                                                                                                                   |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Camera         | Speed, look sensitivity, smoothing, invert look, zoom, roll and the button hints.                                                                                               |
| Effects        | Hide HUD, letterbox bars, motion blur and camera shake.                                                                                                                         |
| Depth Of Field | Blur in front of and behind a focus point. Focus on a set distance or on the locked target. **Aperture** sets how thin the sharp zone is, higher means more blur on both sides. |
| Target Lock    | Aim at or follow a player, ped, vehicle or object.                                                                                                                              |
| Saved Shots    | Save the camera position, angle, zoom and focus, and jump back to it later.                                                                                                     |
| Camera Path    | Keyframes the camera glides between, with playback controls and saved paths.                                                                                                    |

## Target lock

| Option         | What it does                                                                                                                          |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Look At Target | The camera keeps pointing at the target while you fly around. The aim offsets move that point.                                        |
| Follow Target  | The camera rides along with the target at the distance it had when you turned this on.                                                |
| Follow Mode    | **Full** turns with the target in every direction. **Heading Only** only turns left and right with it. **Position Only** never turns. |

The lock lets go on its own when the target disappears.

## Camera paths

A path is a list of keyframes. Each keyframe stores where the camera is, where it looks, the roll, the zoom and the focus distance.

| Keyframe setting      | What it does                                                                                                                       |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| Time To Next Keyframe | How long the camera takes to reach the next keyframe.                                                                              |
| Blend To Next         | **Cut** holds the keyframe and then jumps. **Linear** moves in a straight line. **Smooth** curves through the keyframes around it. |
| Easing                | **Ease In** starts slow, **Ease Out** ends slow, or both.                                                                          |

| Playback setting                   | What it does                                                                         |
| ---------------------------------- | ------------------------------------------------------------------------------------ |
| Loop                               | **Once** stops at the end, **Loop** starts over, **Ping Pong** plays back and forth, **Circuit** flies on from the last keyframe back to the first, using the last keyframe's settings. |
| Playback Speed                     | From 0.25x to 4x.                                                                    |
| Start Delay                        | Waits on the first keyframe before moving.                                           |
| Hide HUD / Letterbox While Playing | Only while the path plays.                                                           |

| Path option                 | What it does                                                                                                                                                |
| --------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Go To Keyframe              | Moves the freecam to a keyframe so you fly on from there. Left and right step to the previous and next one, wrapping around at the ends.                    |
| Go To First / Last Keyframe | Moves the freecam to the first or last keyframe.                                                                                                            |
| Stay At The End             | On by default. When a path finishes, the freecam stays where it ended and you fly on from there. Turned off, it goes back to where you were before playing. |

A path recorded while **Follow Target** is on is stored relative to the target. It plays back around whatever target is locked when you play it, so the same shot works on any car or player.

A path keeps its changes automatically once it has a name. Saved shots and saved paths are stored on the player's own PC, the same as their other saved data.

## Permissions

| Permission                                     | What it allows                                                            | Default            |
| ---------------------------------------------- | ------------------------------------------------------------------------- | ------------------ |
| `vMenu.Enhanced.Freecam`                       | Starting the freecam, with the key or the checkbox in the Filmmaker menu. | `builtin.everyone` |
| `vMenu.Enhanced.Menus.Filmmaker.Menu`          | Opening the Filmmaker menu, and the Camera submenu.                       | `builtin.everyone` |
| `vMenu.Enhanced.Menus.Filmmaker.Effects`       | The Effects submenu.                                                      | `builtin.everyone` |
| `vMenu.Enhanced.Menus.Filmmaker.DepthOfField`  | The Depth Of Field submenu.                                               | `builtin.everyone` |
| `vMenu.Enhanced.Menus.Filmmaker.TargetLock`    | The Target Lock submenu.                                                  | `builtin.everyone` |
| `vMenu.Enhanced.Menus.Filmmaker.TargetPlayers` | Locking onto other players from the nearby players list.                  | `builtin.everyone` |
| `vMenu.Enhanced.Menus.Filmmaker.Shots`         | The Saved Shots submenu.                                                  | `builtin.everyone` |
| `vMenu.Enhanced.Menus.Filmmaker.Paths`         | The Camera Path submenu.                                                  | `builtin.everyone` |

`vMenu.Enhanced.Menus.Filmmaker.All` grants all of the menu permissions at once. Taking away `vMenu.Enhanced.Freecam` while somebody is in the freecam puts them straight back on their character.
