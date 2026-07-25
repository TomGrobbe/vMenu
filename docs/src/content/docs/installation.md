---
title: "Installation"
---

## Installing vs Updating

If you're updating vMenu, instead of installing it from scratch, you need to make sure that you REPLACE **ALL** files, **EXCEPT** your `permissions.cfg` and all files in the `vMenu/config/` folder!

:::caution

**IMPORTANT**: Before installing vMenu, make sure your **[FXServer artifacts](https://runtime.fivem.net/artifacts/fivem/)** are up to date. Please update the server first, since many issues are caused by outdated artifacts.

:::

:::caution

**IMPORTANT**: If you're updating vMenu from any version below v3.3.0, and you want to keep your saved bans, please read the changelog for the update [here](https://github.com/TomGrobbe/vMenu/releases/tag/v3.3.0-pre).

:::

## Installation steps
1. Go to the RELEASES page (on the GitHub repo), and download "vMenu-\<version\>.zip", or use the "Download vMenu" button on the left side of this page to automatically download the latest version.
2. Once you've got your zip file, extract the files and copy everything into `/resources/vMenu/` so that you end up with the `fxmanifest.lua` (previously `__resource.lua`) file right here: `/resources/vMenu/fxmanifest.lua`.

:::note

If you're trying to join the server, and it gives you an error saying "Could not load resource vMenu" or something similar, then make sure that you've installed vMenu inside `/resources/vMenu/` and NOT inside `/resources/vMenu/vMenu/`! Also note that the resource folder name **MUST** be called `vMenu` (Case Sensitive!!!) or the script will not work.

:::

3. Now that you've got your files inside `/resources/vMenu/`, go into the `/resources/vMenu/config/` folder and edit the `permissions.cfg` file to your liking.
4. Go to your `server.cfg` file, and add `exec @vMenu/config/permissions.cfg` **ABOVE** the `ensure vMenu` line (add `ensure vMenu` if you haven't done that already). It's very important that you FIRST execute the permissions file, and THEN start or ensure vMenu. Otherwise vMenu will not function correctly!

:::tip

<br><br>Alternatively, if you don't want to go through any of these installation steps, you can install vMenu using their one click installer! Don't have a FiveM server yet? Click [here](https://zap-hosting.com/vespura2) to get a server, and use the code `Vespura-a-3715` at checkout for a 10% discount!

:::

5. Save the server.cfg file and start your server. Once you're in, you should be able to access most menus just fine without having to configure anything inside the `permissions.cfg`. This is because it is set up to have certain permissions for everyone by default, and only the administrator and moderator sensitive options have been removed from the default permissions file.

Congratulations, you've just installed vMenu in its most basic, plug and play configuration.

* To learn more about the **configuration options** that vMenu has to offer, check out the [Configuration](/vmenu/legacy/configuration/) page.
* To learn more about how to **set up the permissions.cfg file**, take a look at the [Permissions Reference](/vmenu/legacy/permissions/) page.

## 1-click installation with Zap Hosting
[![](https://zap-hosting.com/interface/_images/banner/gameserver/fivem-affiliate-banner-1006x180.png)](https://zap-hosting.com/vespura)
Zap Hosting provides a simple 1 click installation method for vMenu! Click [this link](https://zap-hosting.com/vespura) to get a Zap server and use code `Vespura-a-3715` at checkout for 20% off your purchase!

## Support / Trouble Shooting
vMenu (Legacy) is no longer actively supported — it now only receives small updates when community pull requests are merged or when a FiveM update adds new content, while work is underway on **vMenu Enhanced**. You're still welcome to ask questions in my [Discord](https://vespura.com/discord) server or on the forum topic, but any help is community-provided and not guaranteed. Most issues are already answered there or here in the docs. See the [Troubleshooting & Support](/vmenu/legacy/support/) page for more details.

## F.A.Q.
Check out the [F.A.Q. page](/vmenu/legacy/faq/).

## Appreciate my work?
Consider supporting me on [Patreon](https://www.patreon.com/vespura)!
