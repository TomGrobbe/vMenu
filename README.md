#### Latest Builds

**Latest Traivs builds**

|Master (Beta)|Development (Alpha/Latest)|Production (Stable)|
|:-:|:-:|:-:|
|[![Build Status](https://travis-ci.org/TomGrobbe/vMenu.svg?branch=master)](https://travis-ci.org/TomGrobbe/vMenu) | [![Build Status](https://travis-ci.org/TomGrobbe/vMenu.svg?branch=development)](https://travis-ci.org/TomGrobbe/vMenu) | [![Build Status](https://travis-ci.org/TomGrobbe/vMenu.svg?branch=production)](https://travis-ci.org/TomGrobbe/vMenu) |

**Latest AppVeyor build**

|Build|
|:-:|
|[![Build status](https://ci.appveyor.com/api/projects/status/inf2kinljmp8a5ul?svg=true)](https://ci.appveyor.com/project/TomGrobbe/vmenu)|

--------


# vMenu
vMenu is server sided menu for FiveM servers, including full\* permission support.

### Demo Screenshots

|Main Menu|Player Options|
|:-:|:-:|
|![Main Menu](https://www.vespura.com/hi/i/fef17e5.png)|![Player Options](https://www.vespura.com/hi/i/458b6e4.png)|

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

--------

## Permissions 
Click [here](https://docs.vespura.com/vmenu/permissions-ref) for permissions information.

## Configuration
Click [here](https://docs.vespura.com/vmenu/configuration) for configuration options information.


--------


## NativeUI
This menu is created using [a modified version of NativeUI](https://github.com/TomGrobbe/NativeUI), originally by [Guad](https://github.com/Guad/NativeUI).


--------

## License
**For an updated license, check the license.md file. That file will always overrule anything mentioned in the readme.md**


Tom Grobbe - https://www.vespura.com/

Copyright © 2017-2018


THIS PROJECT USES A CUSTOM LICENSE. MAKE SURE TO READ IT BEFORE THINKING ABOUT DOING ANYTHING WITH VMENU.


- YOU ARE ALLOWED TO USE VMENU ON AS MANY SERVERS AS YOU WANT.
- _YOU ARE ALSO ALLOWED TO EDIT THIS RESOURCE TO ADD/CHANGE/REMOVE WHATEVER YOU WANT._ (see the exception to this rule in the "credits" section below)
- **YOU ARE HOWEVER _NOT_ ALLOWED TO RE-RELEASE (EDITED OR NON-EDITED) VERSIONS OF THIS RESOURCE WITHOUT WRITTEN PERMISSIONS BY MYSELF (TOM GROBBE / VESPURA). FOR ADDED FEATURES/CHANGES, FEEL FREE TO CREATE A FORK & CREATE A PULL REQUEST.**


**Credits**

Never should you change the credits (for example in the in-game menu's about section) to claim this menu to be your own. 90% of the users will recognize this menu as being vMenu, so changing the name of it and removing the credits section, is just useless. You're just being extremely rude and nodoby likes you anymore if they find out you're a big fat liar.


THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 

