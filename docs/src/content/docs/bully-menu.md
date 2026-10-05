---
title: "Bully Menu"
description: "How to switch the Bully menu on, the permissions behind it, and what is in each of its menus."
---

The Bully menu lets staff do mean things to players. It is switched off by default.

## Settings

To turn it on, add this to your `configuration.cfg`:

```cfg
setr vMenu.Enhanced.Bully.Enabled true
```

| Setting | What it does |
| --- | --- |
| `vMenu.Enhanced.Bully.Enabled` | Turns the Bully menu, and the Electric Door Handles option for personal vehicles, on or off. Default false. |
| `vMenu.Enhanced.Bully.DisableExplosions` | Set it to true to turn off Explode and Up-n-Atomizer. Default false. |
| `vMenu.Enhanced.Bully.DisableParticleEffects` | Set it to true to turn off Fireworks and Beast Scare, and the smoke cloud of Transform Vehicle. Default false. |

While the Bully menu is off, every Bully menu and option is hidden. Switching it off while effects are running stops all of them. How often somebody can use it is limited by `vMenu.Enhanced.OnlinePlayers.ActionLimit`, the same as the Online Players actions.

### Why you might want the two Disable settings

Some anti cheat resources watch for explosions and particle effects (the little visual effects the game uses for things like smoke, sparks and fireworks). The Bully menu makes these happen on the game of the player being bullied, so an anti cheat can think that player is cheating and kick or ban them. If your anti cheat does that, set the matching Disable setting to true:

```cfg
setr vMenu.Enhanced.Bully.DisableExplosions true
setr vMenu.Enhanced.Bully.DisableParticleEffects true
```

The options you turn off stay in the menu with a lock on them, and their description tells staff that the server owner turned them off.

## Permissions

| Permission | What it allows | Default |
| --- | --- | --- |
| `vMenu.Enhanced.Menus.Bully.Menu` | Everything in the Bully menu, and Bully Actions in Online Players. | `group.admin` |
| `vMenu.Enhanced.Menus.PersonalVehicle.Electrocute` | Electric Door Handles in the Personal Vehicle menu. | `builtin.everyone` |

Both are listed in the `config/permissions.cfg.example` file your server writes on every start. Every Bully action is written to the staff webhook log.

## Menus

### Online Players → a player → Bully Actions

Everything here happens to the selected player. After you pick something, the menu tells you whether it actually happened. If it did not, it tells you why, for example because the player already has that one going, or has no vehicle for it.

- **Mean Tricks:** Explode, Up-n-Atomizer, Tase, Set On Fire, Ragdoll, Jump, Dance, Drunk, Drugged, Screen Colours
- **Scares:** Jumpscare Sound, Fireworks, Beast Scare, Evil Clone, Random Teleport, Alien Abduction
- **Attackers:** Carjackers, Mugger, Killer Clowns
- **Vehicle:** Floating Vehicle, Transform Vehicle
- **Stays On Until You Switch It Off:** Electric Door Handles, Gang Attack, Invert Vehicle Controls, Possessed Pedals, Slippery Tyres, Glitched Body

Jump, Dance, Evil Clone, Random Teleport and Alien Abduction need the player on foot. Floating Vehicle and Transform Vehicle need the player driving.

Transform Vehicle gives the old vehicle back after fifteen seconds, at the spot where the replacement is. Whoever is still sitting in the replacement at that moment ends up back in the old vehicle. Anybody who got out stays out. If the replacement disappears early, for example because somebody deleted it, the old vehicle comes back right away and everybody who was riding in it is put back inside.

### Main menu → Bully

- **Bully Yourself:** the same options as Bully Actions, aimed at you.
- **Bully Everyone:** the same options without the ones that stay on, aimed at every player. Has an Include Yourself tick box. Killer Clowns and Mugger are sent once to every group of players standing close together, instead of once per player, so a crowd does not fill the server with clowns. Everybody in a group is within 75 metres of everybody else in that group, so they can all see each other. The clowns then split up, so different clowns go after different players in that group, and the mugger runs from one player to the next.
- **Server Wide Effects:** Gang Attack, Invert Vehicle Controls, Possessed Pedals and Slippery Tyres, for every player, including anybody who joins later.
- **Active Effects:** every effect that is still on, and who it is on. Untick a row to stop it.
- **Stop Every Bully Effect:** stops everything on everybody, as far as it can. A few effects that are already halfway through, like a random teleport, still finish on their own.

### Personal Vehicle

- **Electric Door Handles:** anybody other than the owner who tries to open a door of the vehicle gets an electric shock. It switches off when the personal vehicle changes, is forgotten, or the owner leaves.
