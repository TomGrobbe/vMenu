#### Latest Builds
|Master (Beta)|Development (Alpha/Latest)|Production (Stable)|
|:-:|:-:|:-:|
|[![Build Status](https://travis-ci.com/TomGrobbe/vMenu.svg?token=ssVStPpK5ekxFpbVzc3k&branch=master)](https://travis-ci.com/TomGrobbe/vMenu) | [![Build Status](https://travis-ci.com/TomGrobbe/vMenu.svg?token=ssVStPpK5ekxFpbVzc3k&branch=development)](https://travis-ci.com/TomGrobbe/vMenu)|[![Build Status](https://travis-ci.com/TomGrobbe/vMenu.svg?token=ssVStPpK5ekxFpbVzc3k&branch=production)](https://travis-ci.com/TomGrobbe/vMenu)|

--------

# vMenu
vMenu is server sided menu for FiveM servers, including full\* permission support.

### Demo Screenshots
|Main Menu|Player Options|
|:-:|:-:|
|![Main Menu](https://www.vespura.com/hi/i/fef17e5.png)|![Player Options](https://www.vespura.com/hi/i/458b6e4.png)|

\*(Some features do not have permissions support as they are either harmless or it'd just be silly to deny them. However, they will be disabled if you deny access to the submenu that they are a part of (eg: unlimited stamina in Player Options will be disabled if you deny `vMenu.PlayerOptions.Menu`.))

# Download & Installation & Permissions
**Download**

Click [here](https://github.com/TomGrobbe/vMenu/releases) to go to the releases page and download it.

**Installation**
1. Download the latest version.
2. Unzip everything, and take all the files from that zip, and place it in `/resources/vMenu/` on your server.
***NOTE: YOU HAVE TO NAME THE FOLDER `vMenu` (CASE SENSITIVE), OTHERWISE THIS RESOURCE WILL NOT WORK!***
3. Go to your server.cfg file and **AT THE VERY TOP**, add this line:
```text
exec permissions.cfg
```
4. Now, place this line anywhere in your server.cfg file, as long as it's somewhere below the one from step 3.
```text
start vMenu
```
5. Go to `resources/vMenu/config/` and copy the `permissions.cfg` file to the _same folder_ wherever your **server.cfg** file is stored! So, you'll end up with `server.cfg` and `permissions.cfg` IN THE SAME FOLDER.
6. This step is optional **IF** you want to be lazy and just start playing right away. However, if you want to be able to change the time, weather and kick people from the server, or even restrict some of the features for certain players/groups, then go into your `permissions.cfg` file (the one you copied to the same folder as the server.cfg is located), and start editing it by following the instructions in there, and looking at the [permissions list here](https://github.com/TomGrobbe/vMenu/wiki/permissions).
7. Save the server.cfg file if you haven't already, and restart the server. You're now done, enjoy!
---

## NativeUI
This menu is created using [a modified version of NativeUI](https://github.com/TomGrobbe/NativeUI), originally by [Guad](https://github.com/Guad/NativeUI).

## License
Tom Grobbe - https://www.vespura.com/
Copyright © 2017-2018

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. PROPER CREDIT IS **ALWAYS** REQUIRED WHEN RELEASING MODIFIED VERSIONS OF MY WORK.
