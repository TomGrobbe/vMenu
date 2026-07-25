---
title: "F.A.Q."
---

## Read this before asking for support.


:::caution

**IMPORTANT**: If you're having issues with vMenu crashing on your (Linux) server, it's recommended to update to vMenu v3.3.0-pre or above. More info [here](https://github.com/TomGrobbe/vMenu/releases/tag/v3.3.0-pre).

:::



----

#### **Q:** How do I change the 'M' (menu toggle) key to 'some other key'?
> **A:** Look [here](../configuration/) for the convar (configuration) options.

---

#### **Q:** How do I make this menu _admin only_?
> **A**: Look at the [configuration page](../configuration/), make the menu staff only by setting the convar and giving your staff members the `vMenu.Staff` ace.

---

#### **Q:** How do I change the name of my addon car in vMenu?
> **A**: Go to [this page](/vmenu/legacy/configuration/addons-json/#renaming-vehicles).

---

#### **Q:** I installed vMenu correctly but I only see Misc settings and About menu! HELP!!!!
> **A**: First of all, you did _not_ install vMenu correctly (if you use the default permissions file). Second: look at the [installation instructions](../installation/)! They are _very easy_ to follow. Most likely you didn't put the permissions.cfg in the correct location or you didn't add the correct `exec` line (correct one for your setup) **ABOVE** the `start vMenu` line.
Alternatively you may have altered the permissions.cfg file and made a mistake giving players access to specific menus, double check your permissions file and compare it to the original. Or go to [this site](https://vespura.com/vmenu/permissions-generator/) for a permissions generator if you can't figure out what life is.

---

#### **Q**: When I install vMenu the sky starts flickering... HELP!
> **A**: You're using multiple weather and/or time (sync) scripts (for example vSync). Disable those and you'll be fine. If you want to use the other script instead of vMenu's weather/time options, then disable the weather/time sync options using the convars (explained on the [Configuration Options](../configuration/) page). If you're not using multiple time/weather scripts, then stop using your broken graphics mod, 9<span>/</span>10 times it's caused by that. If you're not using a graphics mod either: there's no point in reporting this because I can't fix it, vMenu weather/time options work fine for 99% of the users, I've not had a single person come to me saying it is broken when they removed graphics mods & all other time/weather scripts.

---

#### **Q**: The permissions.cfg is not being executed/my changes are not being saved!
> **A**: First of all, vMenu does **NOT**, I repeat, does **NOT** _read_ the permissions.cfg. _You_ are responsible for executing this file (by adding the `exec <path_to_permissions.cfg>` command in the server.cfg) before starting vMenu. Make sure it is getting executed. If you see a message in the server console saying something like `No such config file: permissions.cfg` then you haven't placed the permissions.cfg in the correct folder or your `exec` command is using the wrong file path to your permissions.cfg. If you get an error in the server console saying `No such command: setr` then read the question below.

---

#### **Q**: I get an error saying `No such command: setr` in the server console!
> **A**: Update your server. You are using an outdated version of FXServer. You need to be using at least version 801 or above. This is only the case for vMenu v1.4.0 and up.

---

#### **Q**: vMenu v1.4.0 no longer works or the permissions and/or options no longer work since v1.4.0!
> **A**: Most likely it's one of the following:
>
> 1. You might be using the old convars, check the configuration page for a list of the most recent convar names.
> 2. Make sure to use `setr` instead of `set` to configure the convars.
> 3. If that doesn't work, update your server. You are using an outdated version of FXServer. You need to be using at least version 801 or above. This is only the case for vMenu v1.4.0 and up.

---

#### **Q**: How to set the default voice proximity?
> **A**: You can't. Reason: I won't allow servers to override user defaults/user preference for settings like voice chat proximity. If a player set it to x meters, and they think they've set it to that, and then the server owner changes it to be global all of the sudden without the user knowing then that's really fucked up and a serious concern. Use another resource if you want to manage it on the server side, and simply remove all voice chat permissions from everyone, that will stop vMenu from touching any voice chat stuff.

---

#### **Q**: How do I disable voice chat?
> **A**: Simply remove all voice chat permissions from everyone, that will stop vMenu from touching any voice chat stuff.

---

#### **Q**: Help, MenuAPI / vMenu v2.1.0 doesn't work with ultra wide monitors.
> **A**: Your menu probably looks something like this:
>
> ![](https://www.vespura.com/hi/i/2018-12-27_19-16_03e47_443.png)
>
> To fix this, either:
> - Set your aspect ratio to 16:9 or below
> - Don't use windowed mode (use borderless or fullscreen)
>
> Or:
> - Left-align the menu (misc settings > disable 'right align menu'). This can't be fixed, it's a GTA issue.


---

#### **Q**: Where are vMenu's vehicles, peds, preferences etc. saved?
> **A**: They are saved on the client's computer, in the following folder: `%appdata%\CitizenFX\kvs\`. Note however that you can't edit these files unless you know what you're doing. No support provided because this is a FiveM feature, not a vMenu feature.

---

#### **Q**: Can you add database support for permissions/bans/whatever?
> **A**: _Can_ I? **Yes**. _Will_ I? **No.**

---

#### **Q**: How can I ban someone that's offline?
> **A**: You can't do that easily and I won't add support for it either. You can do it if you know what you're doing, by editing the bans.json file when the server is offline. If you fuck that up though, then most likely all your bans will be broken/gone. No support provided.

---

#### **Q**: How can I change the arrow keys to be numpad like lambda menu?
> **A**: You can't. This is not supported by GTA V. Period. Stop asking already.

---

#### **Q**: When I install vMenu everyone has godmode / pvp is disabled!
> **A**: No, this is not caused by vMenu. You just disabled scripthook and now you no longer have PVP on by default, get a PVP resource like vBasic.

---

#### **Q**: How do I get different identifiers for my users / how do I get the 'FiveM License'?
> **A**: Install [**'WhatsMyId'**](https://forum.cfx.re/t/whatsmyid/49426), and let them join the server. Or try joining `vespura.com:30122` in FiveM (server may be offline sometimes).

---

#### **Q**: vMenu doesn't work.
> **A**: It does, you just set it up wrong. Don't believe me? Ask the other 50k server owners that have vMenu working without _any_ issues. Still don't believe me? Read the documentation! Still nothing? Well, vMenu might not be something for you then.

---

#### **Q**: vMenu version `x` is buggy, I reverted to `some older version`.
> **A**: No info = no fix. Provide info so I can look into it, and do some research. I'll ignore you if you're just an idiot doing things wrong. And you're just pissing off your players by not updating and forcing them to use an old version that doesn't include the latest features and fixes/performance improvements. If you provide no info, then don't bother posting about it 'not working/being buggy'. I'll just remove your comment with pleasure.

---

#### **Q**: Where can I find the beta/dev/pre-release builds?
> **A**: Latest [beta build of vMenu](https://ci.appveyor.com/project/TomGrobbe/vmenu/build/artifacts) can contain bugs, be careful when using it. Don't put it on your live server unless you know what you're doing and you've tested it to make sure there aren't any bugs.

---

#### **Q**: vMenu doesn't work on any server I join.
> **A**: This can be caused by a lot of things. Try to re-install **both** GTA V and FiveM first though, usually it's a corrupted DLL file somewhere in your GTA V installation or in your FiveM cache files. If it still doesn't work and you need help, ask for help on the forum topic and maybe someone from the community can help you. Do provide a client log (`<your fivem folder>\FiveM Application Data\CitizenFX.log`) in that case.

---

#### **Q**: How do I allow players to spawn saved vehicles but not use the vehicle spawner menu.
> **A**: Give them access to all `vMenu.VehicleSpawner.<category>` categories that you want, then give them full access to the `vMenu.SavedVehicles.<x>` submenu. Just don't give them the `vMenu.VehicleSpawner.Menu` permission and you'll be all set.


---

#### **Q**: My question is not listed here?
> **A**: Look on the forum topic, chances are it has been asked before. Just use CTRL + F on the forum and a _very advanced_ search feature will show up, allowing you to search for things _inside_ that topic!

**I WILL IGNORE YOU IF YOU ASK SOMETHING THAT'S ANSWERED ON THESE DOCS, SERIOUSLY PUT SOME EFFORT INTO THIS IF YOU WANT HELP, YOU LAZY ASS!**

---

## Appreciate my work?
Consider supporting me on [Patreon](https://www.patreon.com/vespura)!



## 1-click installation with Zap Hosting
[![](https://zap-hosting.com/interface/_images/banner/gameserver/fivem-affiliate-banner-1006x180.png)](https://zap-hosting.com/vespura)
Zap Hosting provides a simple 1 click installation method for vMenu! Click [this link](https://zap-hosting.com/vespura) to get a Zap server and use code `Vespura-a-3715` at checkout for 20% off your purchase!
