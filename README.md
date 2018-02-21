#### Latest Builds
|Master (Stable)|Development (Beta)|
|:-|:-|
|[![Build Status](https://travis-ci.com/TomGrobbe/vMenu.svg?token=ssVStPpK5ekxFpbVzc3k&branch=master)](https://travis-ci.com/TomGrobbe/vMenu) | [![Build Status](https://travis-ci.com/TomGrobbe/vMenu.svg?token=ssVStPpK5ekxFpbVzc3k&branch=development)](https://travis-ci.com/TomGrobbe/vMenu)|


--------


# vMenu
vMenu is a custom built server sided trainer, with basic permissions support, whenever possible using labels to automatically translate many menu options to the player's game language, and much more.

## Features
**Almost all features have custom permissions options.**

#### Legenda
|Prefix|Description|
|:-:|:-|
|🔴|This means that this is just a simple button, press it and only one action gets executed.|
|☑|This option is a checkbox/toggle, and it's enabled by default.|
|⬜|This option is a checkbox/toggle, and it's disabled by default.|
|📃|This item is a list containing multiple (sub)items to choose from.|
|▶|This button opens a submenu containing more options.|

### Online Player Options:
+ 🔴 Teleport to player
+ 🔴 Teleport into player's vehicle
+ 🔴 Set waypoint to player
+ 🔴 Spectate player
+ 🔴 Summon player
+ 🔴 Kill player
+ 🔴 Kick player

### Player Options:
+ ⬜ God Mode
+ ⬜ Invisibility
+ ☑ Unlimited Stamina
+ ⬜ Fast run
+ ⬜ Fast swim
+ ⬜ Super jump
+ ⬜ No ragdoll
+ ⬜ Never wanted
+ 📃 Set wanted level
+ ⬜ Everyone ignores player
+ 📃 Player Options: Clean/Heal/Max Armor
+ 📃 Player Actions: Commit Suicide/Drive To Waypoint/Drive Wander
+ 📃 Player Scenarios: play all (human ped) scenarios
+ ⬜ Freeze/unfreeze yourself

### Vehicle Options
+ ⬜ Vehicle God Mode
+ 🔴 Fix Vehicle
+ 🔴 Clean vehicle
+ 📃 Set dirt level of vehicle
+ 🔴 Set license playte text and type
+ ▶ Vehicle Colors
+ ▶ Vehicle Doors Management
+ ▶ Vehicle Windows Management
+ ▶ Vehicle Components
+ ▶ Vehicle Liveries
+ 🔴 Delete vehicle
+ ⬜ Engine Torque Multiplier
+ 📃 Select Engine Torque Multiplier Amount
+ ⬜ Engine Power Multiplier
+ 📃 Select Engine Power Multiplier Amount
+ 🔴 Toggle Vehicle Alarm
+ ☑ Leave Engine Running
+ ⬜ No Siren
+ 🔴 Cycle through vehicle seats
+ ⬜ No bike helmet

### Vehicle Spawner
+ 🔴 Spawn By Name
+ ☑ Spawn Inside Vehicle
+ ☑ Replace Old Vehicle
+ ▶ (all vehicle categories are individual submenus)

### Saved Vehicles
+ 🔴 Save Current Vehicle
+ 📃 Load Saved Vehicle
+ 📃 Delete Saved Vehicle

### Player Skin Options
+ 🔴 Spawn Ped By Name
+ ▶/📃 Spawn Ped By List
+ ▶ Ped Appearance Editor
+ ▶ Saved Peds/Skins

### Time Options
+ ⬜ Freeze Time
+ 📃 Set Time (choose from a list of preset times)
+ 🔴 Set Time (custom hour/minute selection)

### Weather Options
+ ☑ Dynamic Weather Changes
+ ☑ Blackouts Can Occur During Thunder 
+ ⬜ Blackout (manual toggle)
+ 📃 Select Weather Type

### Weapon Options
+ 🔴 Add All Weapons
+ 🔴 Remove All Weapons
+ 🔴 Remove Current Weapon
+ 🔴 Get Max Ammo
+ ⬜ Unlimited Ammo
+ ⬜ No Reload
+ 📃 Select Weapon From List
+ 🔴 Select Weapon By Name
+ ▶ Customize Weapon

### Misc Settings
- ☑ Player Death Notifications
- ☑ Join/Leave Notifications
- 🔴 Teleport To Waypoint
- ⬜ Show Coordinates
- ☑ Show Current Location
- ⬜ Hide Radar
- ⬜ Hide Hud
- ⬜ Speedometer KM/h
- ⬜ Speedometer MPH

### Voice Chat Options
- TBA


## NativeUI
This menu is created using [a modified version of NativeUI](https://github.com/TomGrobbe/NativeUI), originally by [Guad](https://github.com/Guad/NativeUI).

## License
Tom Grobbe - https://www.vespura.com/
Copyright © 2017-2018

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. PROPER CREDIT IS **ALWAYS** REQUIRED WHEN RELEASING MODIFIED VERSIONS OF MY WORK.
