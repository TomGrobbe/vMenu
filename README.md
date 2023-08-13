#### Latest Builds

##### Latest AppVeyor build

|Latest Build|
|:-:|
|[![Build status](https://ci.appveyor.com/api/projects/status/inf2kinljmp8a5ul?svg=true)](https://ci.appveyor.com/project/TomGrobbe/vmenu)|

--------


# vMenu
vMenu is server sided menu for FiveM servers, including full\* permission support.


\*(Some features do not have permissions support as they are either harmless or it'd just be silly to deny them. However, they will be disabled if you deny access to the submenu that they are a part of (eg: unlimited stamina in Player Options will be disabled if you deny `vMenu.PlayerOptions.Menu`.))

--------

# Download & Installation & Permissions

## Download

Click [here](https://github.com/TomGrobbe/vMenu/releases) to go to the releases page and download it.

--------

## Installation
Please follow the instructions over at the [vMenu docs](https://docs.vespura.com/vmenu/installation)

## Zap Hosting
If you're using Zap Hosting, you may find that moving the `permissions.cfg` file to the same folder as your `server.cfg` file may not work correctly (it could get reset every time you restart your server).

If this is the case, leave your `permissions.cfg` file here: `/resources/vMenu/config/permissions.cfg` and add the following to the very top of your server.cfg file: `exec resources/vMenu/config/permissions.cfg` (instead of `exec permissions.cfg`).

You can also use ZAP Hosting's one-click installer for vMenu. Get a ZAP-Hosting server with a 10% lifetime discount [HERE](https://zap-hosting.com/vespura) and make sure to use `Vespura-a-3715` at checkout.

--------

## Support
If you like my work, please consider supporting me on [**Patreon**](https://www.patreon.com/vespura). I've put a _lot_ of my time and hard work into these and other projects.

--------

## Trouble shooting & support
Take a look at the docs first of all. I will ignore you if your question is answered on the docs or the forum topic.

- [docs](https://docs.vespura.com/vmenu/)
- [forum topic](https://vespura.com/vmenu)
- [discord](https://vespura.com/discord)


--------

## Permissions 
Click [here](https://docs.vespura.com/vmenu/permissions-ref) for permissions information.

## Configuration
Click [here](https://docs.vespura.com/vmenu/configuration) for configuration options information.


--------


## MenuAPI
Starting from vMenu v2.1.0, vMenu will be using [MenuAPI (MAPI)](https://github.com/TomGrobbe/MenuAPI), a custom menu API designed specifically for vMenu by me.

vMenu v2.0.0 and earlier was [using a modified version of NativeUI](https://github.com/TomGrobbe/NativeUI), originally by [Guad](https://github.com/Guad/NativeUI), but converted to FiveM by the CitizenFX Collectives and myself (updated/refactored).


--------

## License
**For an updated license, check the license.md file. That file will always overrule anything mentioned in the readme.md**

Tom Grobbe - https://www.vespura.com/

Copyright © 2017-2023

You can use and edit this code to your liking as long as you don't ever claim it to be your own code and always provide proper credit. 
You're **not** allowed to sell vMenu or any code you take from it.
If you want to release your own version of vMenu, you have to link the original GitHub repo, or release it via a Forked repo.
