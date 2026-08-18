# vMenu Enhanced

vMenu Enhanced is a server side menu for FiveM for GTA V Enhanced, providing full configuration and permissions support.
It is a from-scratch remake of the original vMenu for FiveM (legacy), containing a complete refactor and performance optimized approach to how the menu is built.
It also now supports full localization (translations) and custom plugins that anyone can write to create new menu's inside vMenu (see the bottom of this readme).

---

# Download & Installation & Permissions

## Download

Click [here](https://github.com/TomGrobbe/vMenu/releases) to go to the releases page and download it.

---

## Installation

Please follow the instructions over at the [vMenu docs](https://docs.vespura.com/vmenu/enhanced/getting-started/)

## ZAP-Hosting

You can use ZAP-Hosting's one-click installer for vMenu.
Right now only legacy is supported, but vMenu Enhanced will be supported later.
Get your own FiveM server at ZAP-Hosting with a 20% discount [HERE](https://zap-hosting.com/vespura?voucher=Vespura) and make sure to use `Vespura` at checkout.

---

## Support my work

If you like my work, please consider supporting me on [**Patreon**](https://www.patreon.com/vespura). 
I've put a _lot_ of my time and hard work into these and other projects, and really appreciate any support.
All my work is open source and free to use, so please consider supporting.

---

## Trouble shooting & support

Take a look at the docs first of all. I will ignore you if your question is answered on the docs or the forum topic.

- [Docs](https://docs.vespura.com/vmenu/enhanced/)
- [Forum topic](https://forum.cfx.re/t/vmenu/88868)
- [Discord](https://vespura.com/discord)

**Note: support is given by the community.**

I sometimes reply whenever I can.
Pinging me in my Discord is not necessary, if I can help I will. 
Whenever I have time. There are lots of people in my Discord server that will happily help you if you ask nicely. 
Do not ping/mention anyone for support.

---

## Permissions & Configuration

The documentation for this has not yet been built for vMenu Enhanced.
Take a look at the [Getting Started guide](https://docs.vespura.com/vmenu/enhanced/getting-started/) on the docs to see how to setup vMenu Enhanced.
It tells you where to find the example permissions.cfg and configuration.cfg files to get started.

---

## MenuAPI

Just like vMenu legacy, vMenu Enhanced will be using [MenuAPI (MAPI)](https://github.com/TomGrobbe/MenuAPI), a custom menu API designed specifically for vMenu.



---

## Plugins (vMenu Enhanced)

vMenu Enhanced can be extended with **plugins**, which are separate FiveM resources that add their own menus inside vMenu. A plugin never patches vMenu, and vMenu does not need to know it exists beforehand. The two introduce themselves to each other while the server runs, in whatever order they happen to start.

Two NuGet packages are all you need to write one:

| Package | Used by |
| --- | --- |
| [`vMenu.Enhanced.ClientAPI`](https://www.nuget.org/packages/vMenu.Enhanced.ClientAPI/) | your plugin's client script, to declare its menus |
| [`vMenu.Enhanced.ServerAPI`](https://www.nuget.org/packages/vMenu.Enhanced.ServerAPI/) | your plugin's server script, to declare its permissions and settings |

Both bring [`vMenu.Enhanced.PluginContracts`](https://www.nuget.org/packages/vMenu.Enhanced.PluginContracts/) along with them. That one is the shared protocol between vMenu and a plugin, and is not something you reference yourself. Every package is published alongside the vMenu Enhanced release it belongs to and carries that release's version number, so pin them to the vMenu version your server actually runs.

Where to read more:

- [What plugins are](https://docs.vespura.com/vmenu/enhanced/plugins/), if you have not met one yet
- [Installing plugins](https://docs.vespura.com/vmenu/enhanced/plugins/installing/), for server owners
- [Making a plugin](https://docs.vespura.com/vmenu/enhanced/plugins/developing/), for developers
- [vMenu.ExamplePlugin](https://github.com/TomGrobbe/vMenu.ExamplePlugin), a complete working plugin to copy from

A plugin built on these packages is a work based on vMenu, so it carries the same license vMenu does. That means open sourcing it if you hand it to anybody else, free or paid. See [License](#license) below, and the [licensing section of the plugin docs](https://docs.vespura.com/vmenu/enhanced/plugins/developing/#license) for what it asks of you in practice.

---

## License

**The [LICENSE.md](LICENSE.md) file is authoritative and always overrules anything mentioned here.**

vMenu Enhanced is licensed under the **[GNU General Public License v3.0 or later](LICENSE.md)** (`GPL-3.0-or-later`).

Tom Grobbe - https://www.vespura.com/

Copyright © 2017-2026

In short — this is not legal advice, read the license itself:

- You may use, modify and redistribute vMenu Enhanced, including commercially.
- If you distribute a modified version, you **must** release your full source code under the GPL-3.0 as well.
- You must keep the copyright and license notices intact, and state what you changed.
- It comes with **absolutely no warranty**.

You cannot take vMenu Enhanced, close the source, and sell it as your own proprietary product.

### Full notice

```
vMenu Enhanced
Copyright (C) 2017-2026 Tom Grobbe

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
```
