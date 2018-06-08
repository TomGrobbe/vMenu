#### Latest Builds

|Master (Beta)|Development (Alpha/Latest)|Production (Stable)|
|:-:|:-:|:-:|
|[![Build Status](https://travis-ci.org/TomGrobbe/vMenu.svg?branch=master)](https://travis-ci.org/TomGrobbe/vMenu) | [![Build Status](https://travis-ci.org/TomGrobbe/vMenu.svg?branch=development)](https://travis-ci.org/TomGrobbe/vMenu) | [![Build Status](https://travis-ci.org/TomGrobbe/vMenu.svg?branch=production)](https://travis-ci.org/TomGrobbe/vMenu) |


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
1. Download the latest release [HERE](https://github.com/TomGrobbe/vMenu/releases) (under "Assets", click vMenu-`{version}`.zip)
2. Extract the folder `vMenu` and place it inside your `/resources/` folder (THE FOLDER NAME IS CASE SENSITIVE, THE RESOURCE WILL BREAK IF YOU FAIL TO NAME IT CORRECTLY). If you did it correctly, you'll end up with the `__resource.lua` file being right here: `/resources/vMenu/__resource.lua`.
3. Next, copy the `permissions.cfg` file from `/resources/vMenu/config/permissions.cfg` to the same folder where your `server.cfg` file is located.
4. Then open your `server.cfg` file and add this to the very top of the file: `exec permissions.cfg`. If you're updating from an older version of vMenu (before v1.1.5) then make sure to add `add_ace resource.vMenu command.sets allow` to the bottom of your `permissions.cfg` file.
5. While still having the `server.cfg` file open, go to wherever you want your resources to start, and add `start vMenu` on a new line.
6. Restart the server and you should be able to use all basic functions. For more advanced configuration options, checkout the [configuration wiki page](https://github.com/TomGrobbe/vMenu/wiki/Configuration). For info on how to setup the permissions, checkout the `PERMISSIONS.md` file inside the downloaded vMenu zip file, or checkout the [Permissions wiki page](https://github.com/TomGrobbe/vMenu/wiki/Permissions).

## Zap Hosting
If you're using Zap Hosting, you may find that moving the `permissions.cfg` file to the same folder as your `server.cfg` file may not work correctly (it could get reset every time you restart your server).

If this is the case, leave your `permissions.cfg` file here: `/resources/vMenu/config/permissions.cfg` and add the following to the very top of your server.cfg file: `exec resources/vMenu/config/permissions.cfg` (instead of `exec permissions.cfg`).

--------

## Permissions & Configuration
Checkout the [vMenu wiki](https://github.com/TomGrobbe/vMenu/wiki/) for more information about setting up permissions and configuring options for this menu.


--------


## NativeUI
This menu is created using [a modified version of NativeUI](https://github.com/TomGrobbe/NativeUI), originally by [Guad](https://github.com/Guad/NativeUI).


--------

## License
Tom Grobbe - https://www.vespura.com/
Copyright © 2017-2018

THIS PROJECT USES A CUSTOM LICENSE. MAKE SURE TO READ IT BEFORE THINKING ABOUT DOING ANYTHING WITH VMENU.

YOU ARE ALLOWED TO USE VMENU ON AS MANY SERVERS AS YOU WANT.
_YOU ARE ALSO ALLOWED TO EDIT THIS RESOURCE TO ADD/CHANGE/REMOVE WHATEVER YOU WANT._ 
**YOU ARE HOWEVER _NOT_ ALLOWED TO RE-RELEASE (EDITED OR NON-EDITED) VERSIONS OF THIS RESOURCE WITHOUT WRITTEN PERMISSIONS BY MYSELF (TOM GROBBE / VESPURA). FOR ADDED FEATURES/CHANGES, FEEL FREE TO CREATE A FORK & CREATE A PULL REQUEST.**

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
