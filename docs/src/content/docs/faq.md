---
title: "F.A.Q."
---

## Read this before asking for support.

---

#### **Q:** How do I change the 'M' (menu toggle) key to 'some other key'?

> **A:** Look [here](../configuration/) for the convar (configuration) options.

---

#### **Q:** How do I make this menu _admin only_?

> **A**: Look at the [configuration page](../configuration/), make the menu staff only by setting the convar and giving your staff members the `vMenu.Staff` ace.

---

#### **Q:** How do I change the name of my addon car in vMenu?

> **A**: Go to [this page](/vmenu/legacy/configuration/addons-json/#renaming-vehicles).

---

#### **Q:** I installed vMenu but I only see the Misc settings and About menu.

> **A**: This usually means vMenu is not fully set up yet, which is common when you are still using the default permissions file. Please take a look at the [installation instructions](../installation/), they are quick to follow. Most likely the permissions.cfg is not in the correct location, or the correct `exec` line for your setup was not added **ABOVE** the `start vMenu` line.
> You may also have edited the permissions.cfg and accidentally changed which menus players can access, so please double check it and compare it to the original. You can also use the [permissions generator](https://vespura.com/vmenu/permissions-generator/) if you would like some help putting it together.

---

#### **Q**: When I install vMenu the sky starts flickering.

> **A**: This usually happens when more than one weather and/or time (sync) script is running at once (for example vSync). Disable the other scripts and it should be fine. If you would rather use another script for weather and time, you can disable vMenu's weather and time sync options using the convars (explained on the [Configuration Options](../configuration/) page). If you are not running multiple time or weather scripts, a graphics mod is often the cause, so try removing it. vMenu's weather and time options work well for the vast majority of users once other time and weather scripts and graphics mods have been removed.

---

#### **Q**: The permissions.cfg is not being executed, or my changes are not being saved.

> **A**: vMenu does **not** read the permissions.cfg on its own. You are responsible for executing this file by adding the `exec <path_to_permissions.cfg>` command in your server.cfg before starting vMenu. Please make sure it is being executed. If you see a message in the server console like `No such config file: permissions.cfg`, then the permissions.cfg is not in the correct folder, or your `exec` command is pointing to the wrong file path. If you get an error saying `No such command: setr`, then please read the question below.

---

#### **Q**: I get an error saying `No such command: setr` in the server console.

> **A**: Please update your server, you are using an outdated version of FXServer. You need to be using at least version 801 or above. This only applies to vMenu v1.4.0 and up.

---

#### **Q**: vMenu v1.4.0 no longer works, or the permissions and options no longer work since v1.4.0.

> **A**: Most likely it is one of the following:
>
> 1. You might be using the old convars, check the configuration page for a list of the most recent convar names.
> 2. Make sure to use `setr` instead of `set` to configure the convars.
> 3. If that does not help, update your server. You are using an outdated version of FXServer. You need to be using at least version 801 or above. This only applies to vMenu v1.4.0 and up.

---

#### **Q**: How do I set the default voice proximity?

> **A**: This is intentionally not possible. I prefer not to let servers override a player's own preferences for settings like voice chat proximity. If a player sets it to a certain distance and expects it to stay that way, having the server change it globally without their knowledge would be a poor experience and a privacy concern. If you want to manage this on the server side, you can use another resource, and simply remove all voice chat permissions from everyone so vMenu does not touch any voice chat settings.

---

#### **Q**: How do I disable voice chat?

> **A**: Simply remove all voice chat permissions from everyone, that will stop vMenu from touching any voice chat settings.

---

#### **Q**: Help, MenuAPI / vMenu v2.1.0 doesn't work with ultra wide monitors.

> **A**: Your menu probably looks something like this:
>
> ![](https://www.vespura.com/hi/i/2018-12-27_19-16_03e47_443.png)
>
> To fix this, either:
>
> - Set your aspect ratio to 16:9 or below
> - Don't use windowed mode (use borderless or fullscreen)
>
> Or:
>
> - Left-align the menu (misc settings > disable 'right align menu'). This one cannot be fixed, since it is a GTA issue.

---

#### **Q**: Where are vMenu's vehicles, peds, preferences etc. saved?

> **A**: They are saved on the client's computer, in the following folder: `%appdata%\CitizenFX\kvs\`. Please note that you should not edit these files unless you know what you are doing. No support is provided for this, since it is a FiveM feature rather than a vMenu feature.

---

#### **Q**: Can you add database support for permissions/bans/whatever?

> **A**: It is technically possible, but I have decided not to add built in database support for this.

---

#### **Q**: How can I ban someone that's offline?

> **A**: This is not something I plan to add support for, but it is possible if you know what you are doing, by editing the bans.json file while the server is offline. Please be careful, because a mistake in that file can break or remove your existing bans. No support is provided for this.

---

#### **Q**: How can I change the arrow keys to be numpad like lambda menu?

> **A**: This is not possible, since it is not supported by GTA V.

---

#### **Q**: When I install vMenu everyone has godmode / pvp is disabled!

> **A**: This is not caused by vMenu. You likely disabled scripthook, so PVP is no longer on by default. Add a PVP resource such as vBasic.

---

#### **Q**: How do I get different identifiers for my users / how do I get the 'FiveM License'?

> **A**: Install [**'WhatsMyId'**](https://forum.cfx.re/t/whatsmyid/49426), and let them join the server. Or try joining `vespura.com:30122` in FiveM (the server may be offline sometimes).

---

#### **Q**: vMenu doesn't work.

> **A**: vMenu is working for many thousands of servers, so the setup is most likely the cause. Please read through the documentation carefully, since most issues come down to a small configuration step that was missed. If you have gone through everything and it still does not work for you, feel free to ask for help on the forum topic.

---

#### **Q**: vMenu version `x` is buggy, I reverted to `some older version`.

> **A**: Without details it is not possible to investigate. Please provide as much information as you can so the problem can be looked into. Reverting to an older version also means your players miss out on the latest features, fixes, and performance improvements, so it is best to report the specific issue instead. If you can share what is going wrong, it is much easier to help.

---

#### **Q**: Where can I find the beta/dev/pre-release builds?

> **A**: The latest [beta build of vMenu](https://ci.appveyor.com/project/TomGrobbe/vmenu/build/artifacts) can contain bugs, so be careful when using it. Please do not put it on your live server unless you know what you are doing and you have tested it to make sure there are no bugs.

---

#### **Q**: vMenu doesn't work on any server I join.

> **A**: This can be caused by a lot of things. Try to re-install **both** GTA V and FiveM first, since it is usually a corrupted DLL file somewhere in your GTA V installation or in your FiveM cache files. If it still does not work and you need help, ask on the forum topic and someone from the community may be able to help you. Please provide a client log (`<your fivem folder>\FiveM Application Data\CitizenFX.log`) in that case.

---

#### **Q**: How do I allow players to spawn saved vehicles but not use the vehicle spawner menu.

> **A**: Give them access to all `vMenu.VehicleSpawner.<category>` categories that you want, then give them full access to the `vMenu.SavedVehicles.<x>` submenu. Just don't give them the `vMenu.VehicleSpawner.Menu` permission and you'll be all set.

---

#### **Q**: My question is not listed here?

> **A**: Please check the forum topic, since your question may already have been answered there. You can use CTRL + F to search within that topic.

Please take a moment to read through these docs before asking for help, since most questions are already answered here. It really does make it easier to help you.

---

## Appreciate my work?

Consider supporting me on [Patreon](https://www.patreon.com/vespura)!

## 1-click installation with Zap Hosting

[![](https://zap-hosting.com/interface/_images/banner/gameserver/fivem-affiliate-banner-1006x180.png)](https://zap-hosting.com/vespura)
Zap Hosting provides a simple 1 click installation method for vMenu! Click [this link](https://zap-hosting.com/vespura) to get a Zap server and use code `Vespura-a-3715` at checkout for 20% off your purchase!
