---
title: "Changelog"
---

----------------

## vMenu v3.8.43 (July 25 2026)

### Fixed
- Corrected the vehicle spawn delay convar name to `vmenu_vehicle_spawn_delay` in the default `permissions.cfg`, by Tom Grobbe.
- Corrected several commented-out permission (ace) names in the default `permissions.cfg` so they work when uncommented, by Tom Grobbe.

### Changed
- Audited and corrected the configuration documentation, adding missing convars, fixing outdated ones, and documenting the `fxmanifest.lua` options, by Tom Grobbe.
- Audited and corrected the permissions documentation, adding missing permission nodes and weapons, fixing a broken link, and expanding the setup guide, by Tom Grobbe.
- Continued migrating and polishing the documentation site, including theme, styling, landing page, and wording improvements, by Tom Grobbe.

----------------

## vMenu v3.8.24 (July 25 2026)

### Changed
- Began migrating the documentation from the separate docs repository into GitHub Pages, by Tom Grobbe. This release is a work in progress test of the deployment.

----------------

## vMenu v3.8.23 (July 22 2026)

### Fixed
- Added the missing templates folder, by Ricky Merc.

----------------

## vMenu v3.8.22 (July 8 2026)

### Fixed
- Fixed an RGB paint material issue in CommonFunctions, by Ricky Merc (#629).

----------------

## vMenu v3.8.21 (July 3 2026)

### Fixed
- Fixed permission checking for addon vehicle models, which was broken, by Ricky Merc (#626).

----------------

## vMenu v3.8.20 (July 1 2026)

### Fixed
- Added the missing templates folder, by Ricky Merc.

----------------

## vMenu v3.8.14, v3.8.15 and v3.8.19 (June 5 2026)

### Added
- Added a supplemental permissions pilot, by Tom Grobbe (#619).

### Changed
- Updated the gitignore file, by Tom Grobbe.

### Fixed
- Fixed several vehicle bugs related to spelling and missing text fields, by Ricky Merc (#609).
- Fixed the vehicle headlight index going out of range, by Ricky Merc (#609).
- Fixed a bug with underglow colour grabbing, by Ricky Merc (#609).
- Fixed the continuous integration workflow to truncate the Discord commit field, avoiding a 400 error on long messages, by Tom Grobbe.

----------------

## vMenu v3.8.8 (April 6 2026)

### Added
- Added support for custom tattoos, hair tattoos, and all known tattoos and badges up to 2026, by Christopher M.
- Added support for custom blendable faces and granular face skin selection, along with an `extra_blendable_faces` entry in `addons.json`, by Christopher M.
- Added a `tattoos.json` config file, by Christopher M.
- Added the ability to save addon weapons to Weapon Loadouts, by Tom Grobbe (#604).
- Added an explicit on and off button for overriding a vehicle's default radio, by Christopher M.
- Added the `Left Ctrl` and `Enter` keybind to the Player Appearance menu and the MP Ped Customization menu, by Christopher M.
- Added updated data for the mp2025_02 DLC, by L'kid (#552).
- Added missing DrivingFlags, by L'kid (#545).

### Changed
- Overhauled the paint and other RGB capable options and removed unused functions, by Ricky Merc.
- Moved neon colors into VehicleData and refactored the statebag logic to use pattern matching, by Christopher M.
- Now stores the prior ped drawable and prop when browsing ped collections, by Christopher M.
- Config files are now included using globbing, so that all JSON files in the config folder are loaded, by Christopher M.
- Removed the data export experimental feature, by Christopher M.
- Development builds are now drafted as releases instead of being published automatically, and the build number is now included in the GitHub release title for development pre-releases, by Tom Grobbe.
- The fxmanifest.lua file is now included in the repository instead of being downloaded on build, builds now also run for pull requests, and the fx_version was updated to cerulean, by Tom Grobbe.

### Fixed
- Fixed a JSON crash on saved weapon loadout data, by Grav (#551).
- Fixed the radio override not applying when a vehicle is spawned while the player is already in a vehicle, by Christopher M.
- Fixed the version number in the DLL, by Tom Grobbe.

----------------

## vMenu v3.8.7 (April 4 2026)

### Changed
- Bumped MenuAPI, the menu UI library that vMenu is built on, to version 3.2.4, by cm8263 (#584). This update changes the control value checks in anticipation of Title Update 3788, which was not compatible.
- Added Git workflows and removed AppVeyor, and fixed a missing source branch.

### Fixed
- Fixed a runtime dynamic cast error, so that vMenu no longer errors when checking whether the `vmenu_onesync` convar is set to `true`, by cm8263 (#586).

Please note that v3.8.5 was pulled because it contained a bug. v3.8.7 is the first working release after it, and it includes the changes that were in v3.8.5.

----------------

## vMenu v3.8.4 (November 9 2025)

### Added
- Added vehicle color presets. Vehicle color customization has been moved to a new sub-menu within the vehicle color menu, and a new option lets you select from a vehicle's preset colors. The presets are determined by the colors set in a vehicle's meta files, by Toycarium (#501).

### Changed
- Removed the disused `vmenu_disable_server_info_convars` server convar, which was a leftover that is no longer used anywhere in the codebase, by TheIndra55 (#539).

### Fixed
- Fixed a typo in the server event permissions check that caused a permissions regression introduced in #533, by cm8263 (#540).

----------------

## vMenu v3.8.3 (November 1 2025)

### Added
- Added a new `vMenu.OnlinePlayers.SendMessage` permission that is now required to send private messages. The permission is assigned to everyone by default in `permissions.cfg`, but existing servers will need to add the line `add_ace builtin.everyone "vMenu.OnlinePlayers.SendMessage" allow` to their `permissions.cfg` to let players private message each other. Existing servers that update without adding this will stop their players from private messaging each other, by cm8263 (#533).

### Changed
- Refactored several server events to be less vulnerable to abuse by malicious actors, improved permissions checking, and updated some logic to use server-sided natives where appropriate. The changed systems include summon player (inside and outside vehicles), clear area, private messages, kicking passengers out of a Personal Vehicle, kick player, kill player, saving teleport locations, teleport to player, and join and leave notifications, by cm8263 (#533).

----------------

## vMenu v3.8.2 (October 14 2025)

### Added
- Added an option in `permissions.cfg` to check vehicles for damage before changing extras. It also allows for a specific engine and body damage threshold to be met first, and adds a new permission that allows bypassing of this check, by Tyler-9822 (#513). Bypass permissions for the damage check before an extra change were also added, by cm8263 (#528).

### Changed
- Enabled options on "missing" saved vehicles, allowing the renaming, recategorization, replacement, and deletion of saved vehicles that are not currently spawnable because the model is missing. The ordering was also tweaked so that all unspawnable vehicles are grouped at the bottom of the respective menus, instead of scattered throughout the various menus, by cm8263 (#522).

### Fixed
- Fixed the MP Ped inheritance sliders, so that the "Head Shape Mix" and "Body Skin Mix" sliders correctly update the ped, by SkylarPlayz348 (#519).
- Fixed vehicle colors being saved incorrectly, where a vehicle's custom primary and secondary color would be used when saving a vehicle's color even if custom RGB colors had not been used, resulting in incorrect colors when the vehicle was spawned in. This fix only applies to vehicles saved after this update. Previously saved vehicles will be incorrect until re-saved with the correct colors, by cm8263 (#521).
- Fixed vehicle light blackout not syncing between clients, so that it is now correctly synced across all players and not just the player who enabled the option in their menu, by cm8263 (#524).
- Fixed a saved MP Ped's face resetting to default when editing its appearance, which was a regression introduced in #515. A check was also added for previously incorrectly saved ped values, so they are accounted for when editing appearance, by cm8263 (#529).
- Fixed the saved MP Ped appearance menu values not updating, where the items in the appearance menu of a saved MP Ped would be set to their default values, such as `#0 of 100`, instead of the values matching the ped currently being edited, by cm8263 (#529).
- Fixed the saved MP Ped category not being reflected in the ped edit menu, where the current category of a saved ped would not be selected from the list of available categories and instead always showed as "Uncategorised", by cm8263 (#529).

----------------

## vMenu v3.8.1 (September 30 2025)

### Changed
- Changed the default hair color of newly created MP Peds to black instead of bright green, by cm8263 (#515).

### Fixed
- Fixed weather and time change server events not checking whether the requesting player has the correct permissions before updating, which allowed malicious actors to change both without permission, by poco8537 (#430).
- Fixed an issue where malicious actors could unfreeze, but not freeze, time without permission, by cm8263.
- Fixed missing vehicle modkits, where modkit types 47 (Right Door) and 49 (Lightbar) did not appear in the Vehicle Mods menu, by cm8263 (#514).
- Fixed a regression introduced in #475, where newly created MP Peds could not have their hair color changed from bright green without first saving the character and then changing the hair color, by cm8263 (#515).
- Fixed an oversight where `extras.json` was not included in the release files of prior vMenu versions, by cm8263 (#516).

----------------

## vMenu v3.8.0 (September 7 2025)

### Fixed
- Fixed teleport to player, by prikolium-cfx (#500).
- Fixed weather sync when players do not have access to the WeatherOptionsMenu, by Tom Grobbe.

----------------

## vMenu v3.7.0 (June 24 2025)

### Added
- Added configurable labels for vehicle extras using a JSON config file, by DeckardCain001 (#275).
- Added the Weapon Options menu to the Addon Weapons menu, by blackfirefly000 (#428).
- Added new license plate styles to the Vehicle Options menu, by christopher-nzcs (#452).
- Added new content from the Agents of Sabotage DLC (mp2024_02), by MichaelCoding25 (#463).
- Expanded the self driving options, by Coriana (#453).
- Added character randomization to the MP Ped Creator menu, by cm8263 (#475).
- Added an MP Ped preview for saved MP Peds, by cm8263 (#476).
- Enabled addon MK2 weapon tints, by blackfirefly000 (#450).
- Added an "Update Character Clothing" option to the Saved MP Ped menu, by cm8263 (#479).
- Added Ped Collections to the Player Appearance menu, by cm8263 (#480).
- Added an event trigger for the Infinite Fuel toggle, by coleminer0112 (#270).
- Added new content from the Money Fronts DLC (mp2025_01), by MichaelCoding25 (#489).

### Changed
- Removed unnecessary new Ped() values, by RickyB505 (#464).
- Allowed vehicle lights to work when blackout is enabled, by rondonpd (#409).
- Updated appveyor.yml, by christopher-nzcs (#478).
- Updated ConfigManager.cs, by christopher-nzcs (#483).
- Updated EventManager.cs, by christopher-nzcs (#482).

### Fixed
- Fixed a missing wheel type, by SkylarPlayz348 (#448).
- Fixed vehicle categories not listing for saved vehicles by class, by cm8263 (#466).
- Fixed a vehicle name typo, by MichaelCoding25 (#470).
- Fixed the license plate names list, by MichaelCoding25 (#487).
- Fixed the license plate style ordering, by MichaelCoding25 (#490).

----------------

## vMenu v3.6.5 (September 20 2024)

### Added
- Added blood options to the Player Options menu, by aka-lucifer (#414).
- Added a boat anchoring option, by Mathu-lmn (#410).
- Added the ability to select custom vehicle colours using RGB values in the Vehicle Options menu, by JayPaulinCodes (#395).

### Changed
- Updated natives and project configuration, by Mathu-lmn (#411).
- Updated the client project to .NET 4.6.2 (previously 4.5.2, matching the server project).
- Updated the build output locations, so the build folder is now placed in the main solution directory instead of outside the solution folder.

### Fixed
- Fixed a typo in PlayerAppearance, by cm8263 (#420).
- Fixed the vehicle customization menu crashing on game builds below 2372, by Mathu-lmn (#425).
- Fixed a ped model typo, by MichaelCoding25 (#421).

----------------

## vMenu v3.6.3 (July 1 2024)

### Added
- Added new content from the Bottom Dollar Bounties DLC (mp2024_01), by MichaelCoding25 (#403).

### Changed
- Made minor grammar fixes and improvements to README.md, by Michael21107 (#398).
- Updated the ZAP-Hosting section, by OfficialSkypo (#389).

### Fixed
- Fixed replacing a saved vehicle, so it now keeps the existing vehicle's category, by cm8263 (#404).

----------------

## vMenu v3.6.2 (February 9 2024)

### Added
- Added a feature requested in a FiveM discussion on Discord, by RickyB505 (#317).
- Added custom categories for saved MP characters and vehicles, by cm8263 (#357).
- Added new content from The Chop Shop DLC (mp2023_02), by MichaelCoding25 (#363).
- Added a permission for showing ped and prop dimensions and related options, by NickReagan (#300).

### Changed
- Disposed of the Noclip scaleform when it is not in use, by cm8263 (#315).
- Performed code cleanup, by MichaelCoding25 (#370).

### Fixed
- Fixed an issue in MpPedCustomization, by XdGoldenTigerOfficial (#369).
- Added missing weapon permissions, by MichaelCoding25 (#368).
- Fixed a controller problem, by RickyB505 (#373).

----------------

## vMenu v3.6.0 (August 12 2023)

Thanks for all the community provided pull requests ❤️!
Special thanks to everyone who helped test this release, and to [Golden](https://github.com/XdGoldenTigerOfficial) for doing a lot of community PR reviews and merges!

This update contains mainly bug fixes, some small new features.
The latest DLC vehicles, peds and weapons should now also be included.

### Permission changes
- New permission: `vMenu.VehicleOptions.DestroyEngine`, default for `builtin.everyone`: `allow`. Allows the player to destroy their engine in the Vehicle Options menu, from the Vehicle Options.
- New permission: `vMenu.PersonalVehicle.ToggleStance`, default for `builtin.everyone`: `allow`. Allows the player to toggle vehicle stance (for supported vehicles only), from the Personal Vehicle menu.
- New weapon permissions, all allowed by default to `builtin.everyone`:
  - `vMenu.WeaponOptions.HazardCan`
  - `vMenu.WeaponOptions.EMPLauncher`
  - `vMenu.WeaponOptions.HeavyRifle`
  - `vMenu.WeaponOptions.FertilizerCan`
  - `vMenu.WeaponOptions.StunGunMP`
  - `vMenu.WeaponOptions.PrecisionRifle`
  - `vMenu.WeaponOptions.TacticalRifle`
  - `vMenu.WeaponOptions.PistolXM3`
  - `vMenu.WeaponOptions.CandyCane`
  - `vMenu.WeaponOptions.RailgunXM3`
  - `vMenu.WeaponOptions.AcidPackage`
  - `vMenu.WeaponOptions.TecPistol`

### Config changes
- New setting `keep_player_head_props`: Sets whether or not players can lose their head props when they are hit/pushed.
  > Default setting: `setr keep_player_head_props true`

  Possible Values:
  - `true` Props will stay on player (default vMenu and GTA Online behavior)
  - `false` vMenu will not touch this feature, by default this means that head props will fall off when the player is hit, which is the default behavior for GTA V Single Player peds.

- New setting `vmenu_using_chameleon_colours`: Allows the use of chameleon colors for supported vehicles.
  > Default setting: `setr vmenu_using_chameleon_colours false`

  **Important**: You must be streaming the chameleon colours in order for this to function properly (if the current DLC version of your server does not provide them already).

  Possible Values:
  - `true` Enable the chameleon colour vehicle paints category in the primary colours menu.
  - `false` chameleon colors will not be available in the menu.

### Changes
Here's a list of all the changes in this update.

#### Fixes
- Fix: NewLoadSceneStart now stops in the teleport method. ([PR #287 by QuadrupleTurbo](https://github.com/TomGrobbe/vMenu/pull/287)).
- Fix: `STREAMER216` name typo. ([PR #306 By MichaelCoding25](https://github.com/TomGrobbe/vMenu/pull/306)).

#### Improvements
- Improved: Better performance of Location Display (PLD). ([PR #252 by DevBlocky](https://github.com/TomGrobbe/vMenu/pull/252)).

#### New features
- New: Added new content from The Criminal Enterprises, The Contract and LS Drug Wars DLCs (mpsum2, mpsecurity, mpchristmas3). ([PR #290 by MichaelCoding25](https://github.com/TomGrobbe/vMenu/pull/290)).
  <br>This PR also includes:
  - Added low grip tires option
  - Added new destroy vehicle engine option
  - Added new vehicle stance option
  - Update outdated native names
  - Fixed unlimited ammo description typo
  - Fixed unlimited ammo perms
- New: Added new content from San Andreas Mercenaries (mp2023_01) DLC. ([PR #303 by MichaelCoding25](https://github.com/TomGrobbe/vMenu/pull/303))
- New: Added gallery options to Recording Options submenu. ([PR #274 by freedy69](https://github.com/TomGrobbe/vMenu/pull/274)).
- New: Added Chameleon Colours for supporting vehicles. ([Direct contribution by Golden](https://github.com/TomGrobbe/vMenu/commits?author=XdGoldenTigerOfficial)).
- New: Added 'No Helicopter Turbulence' option in the Vehicle Options menu (this uses the permission of NoPlaneTurbulence). (New feature by Vespura).
- New: Added 'Keep Player Head Props' setting that will allow players to keep their head props stay attached if the player is hit or pushed. (New feature by Vespura).

#### Other changes
- Cleaned up the code, and made some readme/license changes.
- Updated the CitizenFX client/server libraries.
- Deleted the old GitHub Issues Template for feature request.


----------------

## vMenu v3.5.0 (December 4 2021)

Thanks for all the community provided pull requests ❤️! It's great to see that people are still interested in keeping this resource up to date.
This update contains mainly bug fixes and performance improvements, as well as the latest DLC vehicles.

### Changes
- Fix: Default Radio Station typo ([PR #256](https://github.com/TomGrobbe/vMenu/pull/256)).
- Fix: Various permission bugs regarding staff only mode. This was caused by some performance improvements I've made in the past, in combination with server owners who setup their permissions in a way that non-staff players still had access to some of the menu's functions, but weren't allowed to access it because of their missing staff permission. This caused some tick functions to crash, resulting in a severe loss of performance. Note: None of this resulted in players being able to access things they shouldn't have access to. It just caused crashes and performance issues.
- Fix: Some internal calls weren't awaited when spawning vehicles.
- Fix: Fix instructional buttons not scaling to >16:9 resolutions ([PR #259](https://github.com/TomGrobbe/vMenu/pull/259)).
- Fix: Use correct female inheritance for MP peds ([PR #261](https://github.com/TomGrobbe/vMenu/pull/261)).
- Fix: Fix faraway spectate teleporting/GPS routes ([PR #262](https://github.com/TomGrobbe/vMenu/pull/262)).
- Improvement: Improve performance for players that aren't allowed to use vMenu at all (ie with the staff only setting). Instead of 0.3-0.4ms it should now idle around 0.1ms for those people.
- Improvement: Updated server/client dependencies again, it's been a while!
- New: Add missing DLC vehicles ([PR #258](https://github.com/TomGrobbe/vMenu/pull/258)).
- New: Add confirmation prompt for replacing saved vehicle ([PR #263](https://github.com/TomGrobbe/vMenu/pull/263)).
- Removed: Remove SetClockDate(). This function recently started to cause issues due to a GTA V bug. For some people this caused massive FPS drops, this change should solve that. More info [here](https://discord.com/channels/192358910387159041/455024366091108352/916720155491459123).

----------------

## vMenu v3.4.0 (May 25 2021)

### Pull requests
- Merged PR #237 (Increased max distance to lock car doors (personal vehicle menu))
- Merged PR #238 (OneSync Infinity fixes)
- Merged PR #239 (Fixed swapped death notifications/join and leave notifications permissions.)
- Merged PR #245 (Added entity spawner to misc settings > dev tools) and improved the code slightly.
- Merged PR #244 (Fixed invisibility) and improved the code slightly.
- Merged PR #249 (Added missing casino/cayo perioco DLC vehicles and weapons).
- Manually fixed the issue mentioned in PR #235

### (Performance) improvements
- Only start tick functions for specific menu's if the user is allowed to open the menu.
- No longer run any tick functions if the user is not allowed to use vMenu at all.
- Remove unused code from tick functions, and merge them into other functions that only get called once.
- Improved setting weather types from client menu.
- Various small code improvements and cleanup.

### New features
- Added a manual snow toggle in the weather options that allows you to enable snow effects for any weather type, not just XMAS. You still get the best results if you use XMAS weather though. (use the `vmenu_enable_snow` convar to set the default value)
- Ability to disable vMenu's player stats control completely (use the `vmenu_disable_player_stats_setup` convar).
- Added warnings to certain features that are incompatible with OneSync Infinity.

### Fixed
- Current weather type not being selected correctly in some cases.

### Permission changes
- Added `vMenu.WeaponOptions.CeramicPistol`
- Added `vMenu.WeaponOptions.NavyRevolver`
- Added `vMenu.WeaponOptions.PericoPistol`
- Added `vMenu.WeaponOptions.MilitaryRifle`
- Added `vMenu.WeaponOptions.CombatShotgun`
- Added `vMenu.MiscSettings.EntitySpawner` (moderators only by default)

### Config changes
- Added `vmenu_handle_invisibility` to prevent vMenu from setting the player's invisibility every frame. Useful if you have other resources that want to override vMenu's player invisibility option.
- Added `vmenu_disable_player_stats_setup` to prevent vMenu from setting any player stats like stamina, shooting, driving ability, etc.
- Added `vmenu_enable_snow` to manually enable snow on the ground without having the 'xmas' weather type selected.

----------------

## vMenu v3.3.1 (November 1 2020)

### Note before updating

_Ignore this part if you've already upgraded to v3.3.0-pre, you can skip to the **Changes (summary)** section_

SQLite support has been dropped. This caused too many crashes and was completely incompatible with recent (Linux) FXServer artifacts.
The new ban system uses the default server side storage built in to FXServer. To migrate (copy) your old `vmenu_bans.db` or `bans.json` file, follow the following steps. If you don't have a `vmenu_bans.db` file, and only a `bans.json` file skip to step 2.

- **Step 1.** Upload your `vmenu_bans.db` file to the online migration tool, and place the `bans.json` file you get here: `/resources/vMenu/bans.json` (the migration tool is no longer available)
- **Step 2.** [Update to the latest version of vMenu](/vmenu/legacy/installation/), start the server and type `vmenuserver migrate` in the server console. This may take some time depending on the amount of bans you have stored. **ONLY RUN THIS COMMAND ONCE, OTHERWISE YOUR BANS WILL BE DUPLICATED**
- **Step 3** Make sure the bans are imported by joining the server and checking the banned players menu. After that, delete the `bans.json` and `vmenu_bans.db` files, as you'll no longer need them.


### Changes (summary)

#### v3.3.1 changes

- Added drawing of Network Owner of entities to developer tools. [PR by Explooosion-code](https://github.com/TomGrobbe/vMenu/pull/231).
- Fixed and added missing street wheel options in the Vehicle Options mod menu.
- Slightly alter the vehicle options menu layout to have the most commonly used features at the top.
- Enable follow-cam mode in NoClip by default and fixed issues regarding this follow-cam mode.
- Change how noclip handles the speed modifier, this now calculates the speed properly, regardless of FPS.
- Cleaned up old/unused code.
- Add 15 missing mpsummer 2020 DLC vehicles. (Only available if the server streams them or if the server enforces the new game build)
- Updated MenuAPI
- The vehicle spawner menu now shows some experimental vehicle stats when selecting a vehicle.
- MenuAPI small performance improvement. [PR by blattersturm](https://github.com/TomGrobbe/MenuAPI/pull/34)
- Some more fixes for the (not yet released) import/export ability for vMenu.
- Improvements for working with OneSync/Infinity. [PR by blattersturm](https://github.com/TomGrobbe/vMenu/pull/233)
- Added default radio station when spawning a vehicle. [PR by Explooosion-code](https://github.com/TomGrobbe/vMenu/pull/232)

#### v3.3.0-pre changes (v3.3.0-pre was a Pre-Release version, published on August 29 2020)

_If you haven't updated to v3.3.0-pre yet, read this part as well before updating to v3.3.1_

- New bans system. The JSON and SQLite implementations have been removed.
- Fix death notifications & restore appearance on re-spawn bugs.
- Some WIP NUI features, not yet available.
- Some deploy config changes for AppVeyor.
- Missing 1868 (and some -not all- 2060) DLC vehicles added.
- Add support for game builds 1868 and 2060 by adding the new vehicle class and vehicle wheel type options.
- Fix vehicle wheel type list bug.
- Rework weather and time options, **note that the convars (config options) for these features have been changed!**
- Experimental feature: add support for addon weapon components, untested. This is a new section (`weapon_components`) in the `addons.json` file.
- Fixed a [bug](https://github.com/TomGrobbe/vMenu/commit/0c642cc0e691520fe8829f1f9bce033428c739b0) which caused some vehicle modifications to apply incorrectly when spawning a saved vehicle.
- Updated CFX dependencies.
- Update MenuAPI dependency.
- Merge a [bugfix PR](https://github.com/TomGrobbe/vMenu/pull/227) which adds additional checks before locking personal vehicles using the remote options.
- Merge a [bugfix PR](https://github.com/TomGrobbe/vMenu/pull/228) which addresses an overhead player names issue.

### convar changes (permissions.cfg):

**Removed options:**

- `vmenu_allow_random_blackout` This feature has been removed.
- `vmenu_smooth_time_transitions` This feature has been removed.
- `vmenu_bans_database_filepath` All SQLite/bans.json options have been removed.
- `vmenu_bans_use_database` All SQLite/bans.json options have been removed.

**Added options:**

- `vmenu_blackout_enabled` This allows you to set the (default) state for blackout mode.
- `vmenu_sync_to_machine_time` Allows you to sync the time to the server's real system time (the custom in-game hour duration setting will be ignored if this is enabled and you won't be able to change the time manually).
- `vmenu_weather_change_duration` Allows you to configure how long weather changes take.

**Changed/renamed options:**

- `vmenu_default_time_hour` has been renamed to `vmenu_current_hour`
- `vmenu_default_time_min` has been renamed to `vmenu_current_minute`
- `vmenu_default_weather` has been renamed to `vmenu_current_weather`

### Addons.json changes

A new section has been added for `weapon_components` in which you can add the names of addon (streamed) weapon components, note that this is untested. They will automatically be matched for compatible weapons and appear as components which you can toggle in the weapon options menu.


----------------

## v3.2.0 and 3.2.1 (April 26 2020 and May 21 2020)

(Combined these changelogs because they are both very small updates, individual changes from [v3.1.3 to v3.2.0 can be found here](https://github.com/TomGrobbe/vMenu/compare/v3.1.3...v3.2.0) and [v3.2.0 to v3.2.1 here](https://github.com/TomGrobbe/vMenu/compare/v3.2.0...v3.2.1))

* Updated README and a few other Git(Hub) related files.
* Converted the `__resource.lua` file into a `fxmanifest.lua` file.
* Cleaned up some code.
* Removed the feature where your ped would keep its head gear on while in a vehicle, this apparently caused sync issues for other players whenever you got out of the vehicle.
* Added support for Benny's (1) and Benny's (2) vehicle wheel types. Apparently [these were missing](https://forum.cfx.re/t/vmenu-v3-2-0/88868/4606?u=vespura) but luckily someone reported that :tada:
* Added a paint fade/enveff scale feature (which is also saved/re-applied for saved vehicles now)
* Updated MenuAPI version

### Some WIP changes

Note, they don't do anything yet.

* Added a button to the misc settings menu which will be used in the future to be able to easily import/export saved data. (hopefully)

----------------

## v3.1.3 (December 17 2019)

See this changelog [here](https://github.com/TomGrobbe/vMenu/releases/tag/v3.1.3)

----------------

## v3.1.0 and v3.1.2 (September 6 2019 and October 15 2019)

[Changes for 3.1.0](https://github.com/TomGrobbe/vMenu/compare/v3.0.3...v3.1.0)

### Changes for v3.1.2

* Removed the update checker.
* Merged 2 pull requests.
* Updated MenuAPI version & CFX API dependency versions.

----------------

## v3.0.3 (April 25 2019)

### Permission changes

* `vMenu.MiscSettings.TeleportToCoord` Allows you to teleport to any coordinate you enter. (allowed by default)
* `vMenu.MiscSettings.TeleportSaveLocation` Allows you to save a new location to the "Teleport Locations" menu, which will also get saved to the locations.json file. (not allowed by default if you use the new permissions.cfg in this update, otherwise EVERYONE may be able to use this so be careful!)

### Convar changes

* `setr vmenu_default_ban_message_information "Please contact the staff team by going (support url) if you want to appeal this ban"` : This message gets added at the end of all ban messages, use this to show users where they can contact the server staff team in case they want to appeal it or if they have any questions.

### Changelog

* Updated MenuAPI version.
* Fix invalid weapon name labels and add weapon description data (the latter is not used atm, but will be in the future)
* Add option to teleport to coordinates in the teleport options menu.
* Add option to save new teleport locations in-game.
* SQLite / banmanager fixes.
* Add option to disable the controller menu toggle key (in misc settings menu)
* Fix KVP cleanup.
* Add timecycle modifier saved settings. (Last used timecycle modifier settings will now be saved on reboot)
* Add default ban message + convar to change it.
* Add weather changed (client side) event that gets triggered when the weather changes.
* Add model name to saved vehicles spawn button.
* PR #165 by [imckl](https://github.com/imckl): Use localized mod type names for the mod options in Vehicle Options mod menu.
* Add (currently unused) internal function to get a list of all vehicles.
* Fix potential text issue for tempbanning players (discussion about this issue can be found [here](https://github.com/TomGrobbe/vMenu/issues/169))
* Try/Catch all SQLite code, to try and prevent fatal server crashes that randomly occur.
* Add finger pointing when pressing B or quickly double tapping the point control on controller (which is the right analog stick/look backwards key)
* Add body blemishes to the mp ped creator, apparently these were missing. As far as I know this wasn't reported, so it went unnoticed for a while.
* No longer toggle the mini-map radar size when the control for it is disabled (by another resource or mod).

----------------

## v3.0.2 (March 21 2019)

_Only some critical fix with SQLite on linux servers, nothing else changed._

----------------

## v3.0.1 (March 19 2019)

Small update to address the missing "Save Ped" button in the player appearance menu.
This update also adds a feature that has been requested many times, which is database support for bans. However, please keep your expectations in check, because of the following:

* This is implemented using SQLite, not your normal MySQL database used for many frameworks.
* This is not 100% tested, use at your own risk right now.
* You can only use the SQL DB, **OR** the bans.json at a time, you can not use both.

The SQLite database file will be automatically created if it doesn't exist. By default, this is in the same folder where your server.cfg file is located. You can change this using convars.

If you currently already have a list of banned players, make sure to switch the convar option to use the JSON file, then boot the server, and use the `vmenuserver migrate` command from the server console. After that switch the convar to use the SQLite db file and restart the server.

### New convars in this update

* `vmenu_bans_database_filepath` default: `""` the file path for the database file. If you enter a custom filepath, then the filepath MUST end with a trailing `/` , for example:

``` yaml
  setr vmenu_bans_database_filepath "resources/vMenu/"
  ```

* `vmenu_bans_use_database` default: `true` enables or disables the SQLite database for storing banned players.

SQLite was not stress-tested, use at your own risk.

### Download

[Here](https://github.com/TomGrobbe/vMenu/releases/tag/v3.0.1)

----------------

## v3.0.0 (March 18 2019)

### Notes before updating ⚠

* This update changes some file names and project dependencies, including:
  * `MenuAPI.net.dll` is now named `MenuAPI.dll` , please remove the old `MenuAPI.net.dll` file from your vMenu folder.
  * `vMenuShared.net.dll` is no longer used, you can remove this from the vMenu folder.
* Make sure to update to the new `__resource.lua` file when installing this update!

### Permission changes (v3.0.0)

* `vMenu.OnlinePlayers.SeePrivateMessages` This is not allowed for anyone by default, but if you really want it enabled then I suggest you only enable this for the server owner/head of your staff team. It allows you to see all PM's from everyone on the server. You can send a PM using the new option in the online players menu, and disable PM's (for yourself, so nobody can PM you and you can't PM anyone else) in the Misc Settings menu. Sending messages is allowed for everyone.
* `vMenu.MiscSettings.OverheadNames` Allows you to enable overhead playernames. Note this feature causes issues with other resources that manage this, so don't give this permission to users if you already have some other resource managing this.

### Convar changes ( v3.0.0)

* `vmenu_smooth_time_transitions` (default: true) Enables 'smooth' time transitions. If you advance time then the time will transition 'smoothly' up to the new time. If you reverse time by less than 2 hours difference, the time will jump straight to that past time. If you reverse time with MORE than 2 hours time difference then that will cause a smooth transition to the next day.
* `vmenu_enable_animals_spawn_menu` (default: false) Enables the Animals spawn options in the Player Appearance menu.
* `vmenu_pvp_mode` (default: 0) Sets the PVP mode, 0 = do nothing, 1 = enable pvp (friendly fire) for everyone, 2 = disable pvp (friendly fire) for everyone.
* `vmenu_disable_server_info_convars` (default: false) Disables the "vMenu Version" and "vMenu UUID" on the serverlist display.
* `vmenu_player_names_distance` (default: `500.0` ) Distance for playerblips to showup. This is using "game units" as measurement. It's unknown what this is in relation to meters or something similar, but 500.0 seems fine in most cases.
* `vmenu_disable_entity_outlines_tool` (default: false) Disables the entity model outlines, model hashes, entity handles development tools section.

### All changes

* `MenuAPI.net.dll` has been renamed to `MenuAPI.dll` . This is now no longer in the `client_scripts` section of the `__resource.lua` , but instead in the `files` section.
* Add 'smooth' camera transitions in the MP Characters creation/editor menu. (See here for a 🎥 preview!: [here](https://streamable.com/6hten))
* Add a config option to allow server owners to set a preferred distance for the player overhead names to be visible, min range is `100.0` , default is `500.0`
* Add a cooldown progress indicator to the weather options menu whenever the weather is currently changing.
* Add a PVP configuration option for server owners to 'do nothing, enable or disable' pvp.
* Add a rejoin session button to the connection options menu. If you leave the session, you can rejoin it using that button.
* Add expanded radar keybind options, enable this in the keybind options menu (misc settings) and press `Z` ( `input_multiplayer_info` ) or `dpad_down` to toggle the expanded minimap. This will automatically return to the normal map after 8 seconds, or press the toggle key _again_ to collapse the radar manually.
* Add option to disable server info convars.
* Add overhead playernames option (in the Misc Settings menu).
* Add ped/vehicle/prop model dimensions drawing options.
* Add ped/vehicle/prop model entity handle drawing options.
* Add ped/vehicle/prop model hash drawing options.
* Add ped/vehicle/prop model outline 'global disabled' convar option to allow server owners to disable this if they feel it is not suited for their server.
* Add private message block/disable option to the Misc Settings menu.
* Add private message option to the Online Players menu.
* Add recording hotkeys to the keybinds menu options.
* Add smooth time transitions.
* Add timecycle modifiers options into the Misc Settings menu.
* Added 'tattoo' clothing badges/overlays option to the MP Character 'tattoos' section. (There's more than 700 of these overlays...)
* CFX: Because of the update to the nuget packages, the vMenuShared project is no longer used. Instead a new folder containing 2 classes is a shared folder for both the client and server project. This means that the `vMenuShared.net.dll` is no longer used, you can remove this from the vMenu folder.
* Change the method used to disconnect yourself using the connection options menu, no longer requiring the player to be kicked from the server side.
* Completely rebuild the Player Appearance menu. Including: animals, male/female ped submenus, online player ped submenu, ability to filter/search for a specific ped, the "fake" names that were often requested are now added, and more.
* Completely refactor internal handling of tattoos data.
* Disable first person camera when currently using any animal ped, because that causes crashes (both game crashes and birds "crashing" into the ground because you can't fly when in first person).
* Disable the MP Character editor while inside a vehicle.
* Disable the vehicle headlights button when using a controller and toggling the helmet/visor (ped prop, using DPAD_RIGHT).
* Don't show the IP of a player in the "show player identifiers" (Online Players menu) option.
* Fix and improve internal functions related to helmet visor/gadget toggling, possible because of a new FiveM PR.
* Fix issue [#152](https://github.com/TomGrobbe/vMenu/issues/152).
* Fix issue [#154](https://github.com/TomGrobbe/vMenu/pull/154).
* Fix vehicle submenu not updating the first time it is opened after moving it to the new submenu section.
* Improve bans.json performance by disabling 'pretty/indented formatting' for when there's more than 100 players banned.
* Keep the menu open (and disable all controls) when the input field is active instead of hiding the menu.
* Main Menu: Moved (organized) a lot of options into new sub-sections, resulting in a much smaller, and easier to use main menu.
* Merged pull request [#158 by attributeerror](https://github.com/TomGrobbe/vMenu/pull/158), this adds the key fob animations used in GTA: O's personal vehicle menu to the Personal Vehicle submenu in vMenu.
* Prevent quitting the session when you are the host. This would kick all other players and prevent any new players from joining.
* Removed unused internal functions, and cleaned up massively in general.
* Set vehicle model (spawn) names as the right side label for all vehicles in the vehicle spawner menu.
* Try/Catch decorator natives to fix crashes caused by some buggy client mods.
* Update client and server CFX API dependencies to the new *official* NuGet packages.
* Update MAPI version.
* Update the about vMenu section.
* Update the README.md file.
* Made sure to make the Menu _snailsome_ :mascot: again, like I used to do in the early changelogs.

----------------

## v2.2.2 (February 11 2019)

### Permission changes (v2.2.2)

* `vMenu.VehicleOptions.SpecialGod` This permission is now removed, all god mode options will now fall under the 'vMenu. VehicleOptions. God' permission instead.
* `vMenu.VehicleOptions.BikeSeatbelt` Allows you to enable the bike seatbelt option, preventing you from falling off bikes, ATVs, etc.
* `vMenu.VehicleOptions.Invisible` Allows you to make a vehicle invisible.
* `vMenu.VehicleOptions.InfiniteFuel` Allows you to enable the Infinite Fuel option in the vehicle options menu (requires FRFuel).
* `vMenu.PersonalVehicle.ExclusiveDriver` Allows you to become the exclusive driver of this vehicle.
* `vMenu.PlayerAppearance.AddonPeds` Allows you to spawn an addon ped from the list. (This is not required to spawn addon peds using the 'spawn by name' option.)

### Convar changes (v2.2.2)

* `vmenu_disable_daily_update_checks` Default: `false` , allows you to disable the daily update checks. Does not disable the update checks when (re)starting the server/resource.

### All changes (v2.2.2)

* Update client and server CFX API dependencies.
* Various MAPI (MenuAPI) version updates.
* Internal refactoring of code and moving data classes to sub-folders.
* Completely re-write the vehicle options -> vehicle colors sub-menu (still no RGB support sadly).
* (Dynamic) Weather changes now last only 30 seconds instead of 45, this is because the visual effect only lasts for around 15 seconds, so it was pointless to have to wait 30 seconds in order to change the weather again. Only having to wait 15 seconds as a 'cool-down' to prevent buggy changes is a lot better.
* While the weather is changing, you will now no longer be able to select another weather type until the current transition is completed. This takes 30 seconds (at most). The weather options will be grayed out and locked while you are unable to change the weather.
* Disable dynamic weather automatically if the weather type is changed/set to `XMAS` , `HALLOWHEEN` , or `NEUTRAL` . It can still be re-enabled if you switch to another weather type and then manually re-enable the dynamic weather changes.
* Pre-fill the current license plate text in the "Enter new license plate text" input box when changing the license plate text.
* Add vehicle invisibility option.
* Re-code the parachute options menu.
* Add unlimited parachutes option to the new parachutes menu.
* Change the way 'never wanted' is managed internally.
* Add a 'bike seatbelt' option.
* Mark vehicles as "not wanted" and "not stolen" when spawning a new vehicle.
* Add full customization to the auto-pilot options menu. You can now create your own custom driving style in-game.
* Add new hot-key/instructional buttons to the vehicle options -> vehicle mod menu to allow for easy vehicle doors opening/closing while modifying your car.
* Merged PR By [PNWParksFan](https://github.com/pnwparksfan): Added bomb bay door support to doors menu.
* Added `extra1` and `extra2` doors to the vehicle doors menu. (Not present on any stock GTA V vehicles (AFAIK), but they are available on some modded vehicles.)
* Change the way safe-teleporting works. It's now slightly slower, but should be 99% accurate all the time.
* Add a lot more customization options for vehicle godmode. You can now specify specific damage types to be enabled/disabled. Note some god mode options override other (god mode) options.
* Add a 'show vehicle health' option to the vehicle options menu.
* Added the ability to remove vehicle doors from inside the vehicle doors menu. _Note that the option "delete removed doors from world" is **NOT** synced for other players, to all other players it will look like the doors just fall off, they won't get removed from the world._
* Re-add interior lights toggle option to the 'vehicle lights' list item, since this doesn't crash the game anymore like it used to 9 months ago.
* Add 'Exclusive Driver' option to the personal vehicle options menu.
* Add daily update checks (can be disabled using convars).
* Players with the 'vMenu. Staff' permission will now _always_ receive a notification if vMenu is outdated. Even if the convar for update notifications is disabled. "Normal" players will not receive these notifications if the convar is disabled. If the convar is set to enabled (default) then everyone will receive a notification if vMenu is outdated.
* Add a filter option to the banned players menu. Press the jump key ( `| 22 | INPUT_JUMP | SPACEBAR | X |` ) while the banned players menu is open to filter the list based on (part of) a username. Press the jump key again and leave the input box empty to reset the filter. Alternatively, clicking on a banned players record or backing out of the menu will also reset the filter.
* Add 'max textures' indicator to clothing and props lists in the MP character creator menu. This makes it easier to find all texture variations for a specific clothing item or ped prop.
* Add 'Neck Thickness' option to the face shape features menu in the MP character creator menu. This should have been added earlier, but it was never included due to a typo in a for-loop.
* The Addons Vehicle menu now contains vehicle classes. Each addon car is sorted into its correct vehicle class category. This should hopefully prevent 200+ vehicles in one single menu.
* The update checker will now check if the server is a ZAP-Hosting server. If it is, then it will check the one-click installer version first before marking the vMenu version as outdated. This is done to hopefully prevent unnecessary spam in the console when the one-click installer isn't updated yet.
* Add 'infinite fuel' option to the vehicle options menu. This requires FRFuel to be installed.
* Added permission for the addon peds submenu.

### Fixes (v2.2.2)

* Fixed: Removed the wind speed 'bug'. This was accidentally added when I was testing wind options, shouldn't have been pushed to production. Thanks for the multiple reports about this.
* Fixed: Reset the dynamic weather timer to the configured delay whenever the weather is manually changed.
* Fixed: The server side weather command is now fixed.
* Fixed: Permissions will now be checked correctly on initial join when setting the player's swim and run speed multipliers.
* Fixed: Correctly set/update the license selected plate type inside the list option whenever switching vehicles or re-opening the vehicle options menu.
* Fixed: Hide _all_ vehicle options sub-menus when not in any vehicle.
* Fixed: A couple of crashes/null object exceptions.
* Fixed: No-clip should now work again when you're in a vehicle that's completely broken/dead.
* Fixed: Hazard lights / indicator lights should now always turn off when you toggle them off for all vehicles. This apparently didn't work for some vehicles like taxis, and it seems it went unreported for a while. 😕
* Fixed: Removed the 'spawned saved weapon loadout' message on first join if a weapon loadout was set to 'default' and 'equip on (re)spawn' was enabled.
* Fixed: Fix players not receiving the 'vMenu is outdated' notification even if it was set to enabled (default) in the permissions.cfg.
* Fixed: A typo in the banned players menu has been fixed. ('Again' instead of 'Agian').
* Fixed: Prevent empty player names in ban records in the bans.json.
* Fixed: Update checker is now fixed for ZAP-Hosting servers, there was a small bug preventing new servers from being registered from some ZAP-Hosting IP addresses.

----------------

## v2.2.1 (January 24 2019)

### Convar changes in v2.2.1

* `vmenu_auto_ban_cheaters_ban_message` Use this to set a custom auto-ban-cheaters ban reason/message.

### Permission changes in v2.2.1

#### new

* `vMenu.PlayerOptions.StayInVehicle` Gives you access to the "stay in vehicle" option that prevents you from being dragged out of your vehicle.
* `vMenu.VehicleOptions.FixOrDestroyTires` Allows you to use the tire fix/destroy list option to fix or destroy specific vehicle tires through the menu.

#### New submenu

* `vMenu.PersonalVehicle.Menu` Allows you to use the menu and set a vehicle as your personal vehicle.
* `vMenu.PersonalVehicle.All` Allows you to use all features below.
* `vMenu.PersonalVehicle.ToggleEngine` Allows you to toggle the engine remotely for your personal vehicle.
* `vMenu.PersonalVehicle.ToggleLights` Allows you to toggle the lights state remotely for your personal vehicle.
* `vMenu.PersonalVehicle.KickPassengers` Allows you to kick all passengers from your vehicle, they will have a 10 second timer to stop the vehicle if they're driving it. If they refuse to stop then they will be forcefully kicked out of the vehicle after 10 seconds. If they do stop the vehicle within those 10 seconds they'll automatically be tasked to get out of the vehicle.
* `vMenu.PersonalVehicle.LockDoors` This allows you to lock and unlock your personal vehicle's doors for all players. Anyone inside the vehicle is still able to get out of the vehicle if the doors get locked. If you are close to the vehicle, you can quickly double tap E on keyboard or L3 on controller (the vehicle horn button) to toggle locking/unlocking your doors.
* `vMenu.PersonalVehicle.AddBlip` Allows you to add a blip for your personal vehicle.
* `vMenu.PersonalVehicle.SoundHorn` Allows you to remotely sound the horn for 1 second. It does not work well if you're inside the vehicle, so only use it whenever you're outside of the vehicle for the best effect.
* `vMenu.PersonalVehicle.ToggleAlarm` Remotely toggles the alarm on/off.

### Other changes

* Add suggestion by @NickM0822 ( `NickM0822 14 days ago: Add the ability to set a message for the auto ban of cheaters (like what it would say on the ban message)` ).

You can now use this convar to set a custom auto-ban-cheaters ban message:

``` yaml
setr vmenu_auto_ban_cheaters_ban_message "You have been automatically banned. If you believe this was done by error, please contact the server owner for support."
```

* Update MenuAPI dependency version to v1.7 (and to v1.8 a few commits later to fix another bug) to fix a conflicting keybind for when the menu is open and you try to use the new DLC vehicle functions like boost or shooting rockets when using a controller/mouse.
* Some internal cleanup of various files/code parts.
* Fix null exception related to restoring weapons and the new weapon loadouts menu that occurred on re-spawn.
* Change the dev permission to be disabled by default, and to only work whenever server debugging mode is enabled _before_ the server is started. Requiring _actual_ server owner intervention before I can help them debug stuff. This was something that shouldn't have been designed the way it was in the first place, but it's now changed.
* Re-format locations.json and add some missing default locations for blips like barber shops and misc stores.
* Update the GitHub bug report template.
* Fixed a crash caused when the addons.json file contained multiple 'addons' using the same name. This is a user-error bug, but could've been prevented. This is now fixed, however you'll get a warning telling you to fix your file. It'll still work as intended in the mean time.
* Fix weapons restore issue on skin-change (For more info see this topic [comment](https://forum.fivem.net/t/vmenu/88868/2519?u=vespura).)
* Add missing DLC weapon (weapon_stone_hatchet), as reported by @throwarray.
* Add a warning message to the server console in case people didn't execute the permissions.cfg or set vMenu up to ignore permissions. Looks like this:

![alt](https://www.vespura.com/hi/i/2019-01-24_23-10_733d1_542.png)

There is currently no option to disable this warning, that'll come in a future update.

* Add a 'stay in vehicle' option. This when enabled (in the player options menu) you will not be able to be dragged out of your vehicle by NPC peds that get angry at you.
* Add a vehicle tires fix/destroy feature in the Vehicle Options menu.
* Fix a crash caused whenever a user's PC had broken timezones in their registry, causing an `InvalidTimeZoneException` to be triggered whenever using `DateTime` in the vMenu constructor, causing vMenu to stop working completely.
* Add a server-console-only command to ban players. try `/vmenuserver help` for info.
* Add Personal Vehicle menu.

----------------

## v2.2.0 (January 10 2019)

### Permissions changes (v2.2.0)

#### Weapon Options Menu

* `vMenu.WeaponOptions.PlasmaPistol` (new dlc pistol)
* `vMenu.WeaponOptions.PlasmaCarbine` (new dlc smg)
* `vMenu.WeaponOptions.PlasmaMinigun` (new dlc minigun)

#### Weapon Loadouts Menu

* `vMenu.WeaponLoadouts.Menu` Access the new weapon loadouts menu.
* `vMenu.WeaponLoadouts.All` Use all options in the new weapon loadouts menu.
* `vMenu.WeaponLoadouts.Equip` Allows you to equip a weapon loadout, weapons which are a not allowed via the Weapon Options menu will not be spawned in.
* `vMenu.WeaponLoadouts.EquipOnRespawn` Allows you to automatically equip a specific saved weapon loadout whenever you (re)spawn, also works if you've just joined the server.

### Fixed (v2.2.0)

* Fix speed display function showing KPH instead of MPH even if MPH was toggled on, and KPH was off.
* Fix the menu toggle key not being set correctly in all places.
* Prevent ultra wide aspect ratios from causing problems by notifying the user, and forcing the menu to left-align if an unsupported aspect ratio is being used.
* Fix "tatttoo" typo in some places, not all internal code can be changed for backwards compatibility with previous save files and servers using older versions.
* Fix 'M' key triggering a bugged 'noclip' menu state whenever the player is not allowed to use the main menu (staff only is enabled).
* Fix 'Noclip is now active...' message appearing when it shouldn't.
* Fix ammo count not being set and/or saved correctly in multiple cases.
* Fix weapon loadouts permissions.
* Fix bullet proof tires not being set and/or saved correctly in some cases.
* Fix 'Spawn Ped By Name' permissions bug.
* Fix dev permissions (can still be toggled off).
* Fix build warnings of vMenu by using a custom FXServer build for server API version.
* Fixed critical bug caused by a Natives repo PR by myself, this caused player appearance props to break.
* Fix some potential permissions issue when using Spawn Weapon By Name (previously when allowed, gave you access to spawn _any_ weapon, this is no longer the case, unless it's an addon weapon then you're still allowed to spawn any addon weapon because those are not permissions restricted).
* Fix Appveyor config for auto publishing new builds.

### Changed (v2.2.0)

* Update MenuAPI version.
* Refactor some code and clean up multiple parts of the code.
* Move files/classes into subfolders to try and organize the internal structure a bit.
* Tiresmoke is a bit more user friendly now because it'll automatically select your current tiresmoke color when opening the vehicle mod menu.
* Changed or removed some default location blips.
* Some repo cleanups, like deleting travis.yml, deleting CNAME file, deleting an unused JS script file and more.
* No longer require vMenu to have access to the `command.sets` permission, but use the native instead for guaranteed access.
* Move permissions to the vMenuShared project. PermissionsManager.cs now manages all permissions client- and server-side.
* Readme and license copyright year/date changes.
* ⚠ Changed how addons are loaded, now done client side. Server side only checks for errors and prints them to the server console as a warning, it does not send the file to the client anymore. This requires you to override your `__resource.lua` file and MAKE SURE THAT THE `addons.json` FILE IS IN THE `resources\vMenu\config\` FOLDER!
* Add a way for me to print an extra 'update checker' message in the server console if needed. Only used whenever I need to send an important message to server owners without having to bump the vMenu version.

### Added (v2.2.0)

* Added a 'saved weapons' (Weapon Loadouts) menu.
* Add (re)spawning with a default loadout equipped.
* Add a lot of new default location blips.
* Added facial expressions to characters created with the MP Character Creator menu.
* Add DLC weapons from Arnea Wars DLC (1604).
* Add permissions for those weapons.
* Add colored headlights option to the Vehicle Options -> Mod Menu.

----------------

## v2.1.0 (December 18 2018)

### Added permissions

* `vMenu.OnlinePlayers.Identifiers`
* `vMenu.MiscSettings.DriftMode`

### Added convars (config options)

* `setr vmenu_server_info_message "About this server, discord: vespura.com/discord"`
* `setr vmenu_server_info_website_url "www.vespura.com"`
* `setr vmenu_teleport_to_wp_keybind_key 168 # 168 / F7 by default`
* `setr vmenu_disable_spawning_as_default_character false`

### Changes (v2.1.0)

* Fixed snowball pickup animations & notifications.
* Improve GetUserInput function.
* Converted vMenu to [MenuAPI](https://forum.fivem.net/t/c-menuapi-mapi-v1-4/204992?u=vespura).
* Remove NativeUI.
* Fixed spectating.
* Added tattoos to the MP Character creator menu.
* Weapon wheel is now disabled while the menu is open and using the scrollwheel on the mouse to scroll through the menu. If you press TAB or use the controller then you can still switch weapons.
* Fix a lot of bugs (most of them were introduced after converting to MenuAPI initially, but also some bugs prior to that have been fixed).
* Cleanup and remove a lot of dead/old/unused code.
* Added a 'print identifiers' button to the online players menu.
* Added a 'set custom speed limit' option for the vehicle speed limit feature.
* A lot of internal/structural changes to classes like Common Functions and other parts of the code that get used a lot.
* Added a new info button for server owners to configure their own server name, website, etc inside the about menu. No this will not change the menu name from the player name to your custom name, and I will not add that either.
* Implement keybinds. (See Misc Settings > Keybinds Menu).
* Add drift mode keybind.
* Add teleport to waypoint keybind.
* Set the player back into their vehicle if they changed from ped model/skins.
* Restore armor whenever a player switches ped models/skins.
* Add a deprecated notification to the Save Ped function whenever an MP Ped is saved.
* Hide the radar/minimap when the MP Character Creator is open.
* Add missing user defaults for keybind options.
* Change 'max armor' into a list containing multiple armor types/stages (no armor, light armor, standard armor, etc).
* Fix teleport into vehicles when using 'Teleport Into Player Vehicle' in the Online Players menu.
* Fix bullet proof tires bug.
* Add new DLC vehicles, they are not available in-game yet but once they are then vMenu will already support them.
* Added some hardcoded debug permissions for myself. All you need to do to remove this (if you don't want me to help you out in case you have problems with vMenu) is remove the line containing `vMenu.Dev` from the default permissions.cfg. All this does is give me access to debugging commands for whenever debugging is required on the server itself. It does not give me any permissions outside of vMenu. You can checkout the features affected [here](https://github.com/TomGrobbe/vMenu/commit/414e98f9a6a3e30677e8250c5588e023f8d91784). This vMenu. Dev permission will only work for _my_ identifier, so it won't work if you give that to someone random.
* Change the 'Kill Player' option in the Online Players menu. This will now notify the person of whoever pressed the Kill Player button.
* The NoClip menu is now no longer visible when you activate NoClip. You'll get a notification whenever you use it explaining the new interface.
* Fix some user defaults not getting saved correctly.
* Fixed saved vehicles crashing the game (bug introduced after converting to MenuAPI, this was not an issue before in any production version of vMenu and only affected the dev builds).
* Switch to Appveyor for all future GitHub deployments and dev builds. This replaces travis completely.
* Fix location & coordinates drawing. And move back the time display to its old location. (It's still a separate toggle though.)
* The menu now works correctly both left and right aligned. You can now instantly switch between left/right menu alignment by toggling the option in the Misc Settings menu.
* Fix the 'look at' function myself in vMenu's code by changing parameters, since the PR that fixes that has not been built on the FiveM production channel yet (at the time of that commit).
* Re-add the notification for whenever you try to exit the MP Character creator without saving.
* README.md has been changed slightly.
* Fix _rare_ weapon attachments bug that was randomly introduced without the code for it ever changing. Most likely something internally in GTA (or maybe even FiveM, doubt that though) has changed that broke vMenu's previous setup. All data related to weapons has now been refactored, reworked and should be a lot better now. Also removed duplicate entries and reduced memory usage because of this.
* Change some descriptions, and other text entries in the menu.
* Add a Remove All Tattoos button to the tattoos menu.
* Renaming a saved MP Character now puts the old name in the input box by default. Which should help you change any typos or other small name changes.
* Fix changing wheel type resetting menu index.
* Fix window tint showing as 'Green \[7/7\]' when it should have been 'Stock \[1/7\]'.
* Add FPS warning for Show Location option. It's very laggy and I haven't been able to fix it.
* Properly fix picking up snowballs.
* Added a 'default character' option. Go to your 'Saved Characters' in the MP Characters menu and select 'Set As Default Character' on one of them. Next time you (re)spawn you'll be spawning as that character automatically. Server owners can disable this feature globally on the server by setting a convar. Players can disable this themselves in the Misc Settings menu by toggling the Spawn As Default Character checkbox.
* Added a system that will cleanup old/unused KVP entries.
* Add a 'dump' command for debugging purposes. Which dumps all vMenu data on the client side in the client console. ( `/vmenuclient dump` ? it can cause lag spikes!).
* Other small UX improvements in multiple places, too much to list here all individually.

And of course made sure this menu is _still_ snailsome!

----------------

## v2.0.0 (December 7 2018)

vMenu v2 contains a lot of performance improvements (also due to the changes that I've PR'ed to the FiveM CitizenFX API set which is used by vMenu), no more (unused) memory buildups to some ridiculously high value, MP (freemode) Character customization support and much more. Read below for a full list of changes.

* Fixed multiple 'null object' exceptions.
* Fix the player stamina toggle option.
* Fix special vehicle god mode not being saved correctly.
* Fix Christmas weather particles and vehicle trails/footstep tracks not being loaded correctly in some cases.
* Fix / improve teleport to waypoint option. It should be a lot more accurate now.
* Fixed a NativeUI bug.
* Fix player names in notifications.
* Fixed UX issue with the 'Delete Vehicle' option.
* Pressing `ESC` while a menu is open will no longer trigger the pause menu. Use `P` on keyboard instead if you want to open the pause menu. Pressing `start` on a controller still allows you to open the pause menu.
* Completely re-code the Online Players menu. Removed all memory leaks in that menu because of this.
* Fixed memory issues by preventing (unused) memory from building up to some high values like 500 MiB in some rare cases.
* Added MP Character customization.\*
* Add a 'Disable Plane Turbulence' option in the vehicle options menu. Only works for planes. No this can't be changed to support helicopters.
* Add a 'Keep Vehicle Clean' option in the vehicle options menu.
* Added default location blips to the locations.json file.
* Add picking up snowballs when the weather is set to 'xmas' and the player is on foot and unarmed. Only works if you have permission to spawn snowballs through the weapon options menu. This is done to prevent abuse.
* If you have a specific helmet that has a visor or a gadget that you can toggle (like nightvision goggles for example) you can now hold F11 while on a bike or on foot, (and while vMenu is closed). This will play an animation on your player and will flip the visor/gadget up/down (and switch to the proper component variation).
* Added a 'vehicle dimensions debug' option to the misc settings. It draws the outlines of the vehicle model, as well as the vehicle handle (entity id).
* Added speed limit options, PR by [ToastinYou](https://github.com/ToastinYou). Later improved by me by adding some notifications and further improvements.
* Added a Draw Time function to the misc setting menu. This replaces the time display in the Show Location option.
* Added a `vMenu.OnlinePlayers.ViewBannedPlayers` permission that allows players to _see_ the list of banned players in-game. This will _not_ allow those players to _unban_ those banned players. Only players with the `vMenu.OnlinePlayers.Unban` permission will be able to unban players from this list.
* Remove old export related to headblend data and use the CFX C# API set instead. This also removes the .js file.
* Remove time display in the Show Location option. This has been moved to the Draw Time function.
* Update NativeUI with some fixes and very slight improvements.
* Properly implement player blips sprites.
* Re-write player blips functionality. Players too far away will no longer have blips shown on the minimap, only on the main pause menu map. Also players in vehicles that are too far away, will still have the correct vehicle blip now because I switched to decorators. In case that fails, the old system will automatically take over and attempt to show the correct blip.
* Change vehicle neon/under-glow colors to be the 'official R*' colors.
* Some internal changes were made to the dependency structure.
* Added `vMenu.OnlinePlayers.ViewBannedPlayers` which allows players to see a list of banned players in-game. This does not give them access to the unban option.
* Added `vMenu.VehicleOptions.KeepClean` which allows players to enable the Keep Vehicle Clean option.
* Added `vMenu.VehicleOptions.SpeedLimiter` which allows players to use the speed limit option.
* Added `vMenu.VehicleOptions.DisableTurbulence` which allows players to disable plane turbulence.
* Added `vMenu.VehicleOptions.Flares` and `vMenu.VehicleOptions.PlaneBombs` which both of those are currently unused, but will be a thing in the future.

\* _MP Character customization is a new menu which can be found just below the Player Appearance menu. You will need the `vMenu.PlayerAppearance.Menu` permission to use it. There are no other permissions inside this menu that can restrict any of the features, because it's all "1 big feature"._

### Really important notes about MP Character customization

1. You can NOT save existing peds made through vMenu, or some other mod/resource. You can only save/create/edit/spawn characters created through this new menu inside vMenu. This is due to GTA limitations.
2. You should NOT edit your saved character through the 'Player Appearance' menu after you've created it in the 'MP Character' menu. Customizations done there will NOT be saved to your character.
3. Some options like Tattoos are not (yet) available. Tattoos probably won't be added due to a large number of impossible challenges to overcome. Mainly 'getting' the current tattoos on a ped, and 'removing a specific tattoo'. Which are both impossible. And to address the common question of "but SkinControl has it": it doesn't. SkinControl basically gave up on that part in their code because it's impossible, and they manually set everything to -1 (making it useless for our purpose).

Some cool but useless stats about this update: **40** files changed. **77, 544** additions and **1, 295** deletions.

----------------

## v1.5.0 (November 24 2018)

### New permissions (v1.5.0)

* `vMenu.PlayerOptions.MaxHealth` This allows the player to heal themselves in the Player Options menu.
* `vMenu.PlayerOptions.MaxArmor` This allows the player to give themselves max armor in the Player Options menu.
* `vMenu.PlayerOptions.CleanPlayer` This allows the player to clean their player clothes in the Player Options menu.
* `vMenu.PlayerOptions.DryPlayer` This allows the player to make their clothes dry in the Player Options menu.
* `vMenu.PlayerOptions.WetPlayer` This allows the player to make their clothes wet in the Player Options menu.
* `vMenu.PlayerOptions.VehicleAutoPilotMenu` This allows the player to use the vehicle auto pilot options, which is located in the Player Options menu.
* `vMenu.MiscSettings.RestoreAppearance` If this is allowed and the player enabled it in the Misc Settings menu, then whenever the player dies and respawns it will restore their previous skin. Might not work correctly for certain servers that have custom respawn logic implemented, or for the freemode peds.
* `vMenu.MiscSettings.RestoreWeapons` If this is allowed and the player enabled it in the Misc Settings menu, then whenever the player dies and respawns it will restore all their weapons and attachments. Might not work correctly on certain servers that have custom respawn logic.

### Removed permissions (v1.5.0)

* `vMenu.PlayerOptions.Functions` This permission is no longer used. It has been replaced with some of the added permissions in the list above.

### New config options (v1.5.0)

* `vmenu_freeze_time` Freeze the time by default when the server starts.

(can be set to true or false, default: `setr vmenu_freeze_time false` )

* `vmenu_quit_session_in_rockstar_editor` When you set this to true, it will leave the current game session if a player uses the rockstar editor button in the recording options menu.

(can be set to true or false, default: `setr vmenu_quit_session_in_rockstar_editor false` )

### Changes ('tl;dr' version)

* Bug fixes.
* Improvements.
* Some things have moved in the menu.
* Some (sub)menus have been rewritten or changed in some other noticeable way.
* New features.
* Still snailsome 🐌

### Changes (long version)

* Prevent vMenu crashing when server owners made a mistake in their locations.json file.
* Add an option for the server owner to configure, which (if enabled) will force players to quit the session as soon as they press the 'Rockstar Editor' button in the recording options menu.
* Fixed a player blips issue on OneSync enabled servers.
* Made player blips group correctly instead of creating a new entry for every single player.
* Also fixed some other random issues with player blips. In the mean time figured out a lot more info about undocumented blip natives (PR's were made for the Natives reference.)
* Update my NativeUI fork for some slight performance improvements. (PR by @d0p3t)
* Create a new submenu in the Player Options menu called "Auto Pilot Options" which can be used for starting, stopping and changing (auto) driving options.
* Add a weather command ( `vmenuserver weather <weatherType | dynamic <true | false>>` ) to be used in the server console only.
* Add a time command ( `vmenuserver time <freeze <true | false> | <hour> <minute>>` ) to be used in the server console only.
* Do some major cleaning up regarding travis builds. Also implement appveyor builds for development artifacts.
* Removed all existing MP Ped customization related code. I'm re-coding everything related to mp peds so stay tuned.
* Re-coded the player appearance options menu, fixed some bugs while I was at it.
* Changed text on some notifications & debug logging.
* Fixed death notifications sometimes not showing up when a vehicle or other (non-player) entity killed a player.
* Added a 'Restore Player Appearance' and a 'Restore Weapons' option to the Misc Settings menu. Pretty self explanatory.
* Fix multiple onListChange and onListSelect event bugs. (these were getting duplicated and causing problems).
* 'Clear Area' in the Misc Settings menu is now synced for all players.
* The 'commit suicide' option in the Player Options menu has some better logic to decide what type of animation it should be (gun or the pill) and if it's a gun, then it'll choose a specific gun based on an internal preference list.

The pistol animation now actually has a real firing gun in it as well.

* Completely rewrite the Saved Vehicles menu, yes I've been listening to your requests. Addon vehicles menu will be getting a similar face lift in the near future, stay tuned.
* Remove all unnecessary loading of streamed assets. And unload the ones that aren't needed anymore as soon as possible.
* Add some new config options (check the list above).
* Did some testing with wind noise, let me know if you notice a difference in wind noise after this update.
* Add a new "Illuminated Clothing Animation" option to the Player Appearance menu. If you have illuminated clothing, you can turn it on, off, make it fade or flash the lights. Should be synced for all players, however players that join later on might not see this. This is due to GTA V decorator limitations for the Player Ped.
* 2 small changes in the Weather Options menu (2 buttons are now checkboxes which update properly based on changes by other players as well).
* Internal: implement new storage data type (int).
* Internal: Add some new User Defaults settings & saving.
* Internal: Reset the experimental features to be enabled/disabled based on the config option in the `__resource.lua` file.
* Fixed some typos.
* Change how vMenu hides hud elements as well as change when vMenu should hide certain hud elements created by vMenu.
* No-reload option is now changed, it acts more like infinite ammo but this way it stops the rapid-fire revolvers and stops other mk2 weapons from completely going insane. Also fixes an issue when no-reload was on while in a vehicle.
* NoClip: change (increase) max speed and add some more in-between speeds as well.
* As mentioned before, the 'Player Functions' in 'Player Options' is now separated into multiple buttons, each having its own permission node.
* Create a small JS script which will be used in a future version of vMenu. To be continued...
* Some MP Ped re-structuring and preparations for future development related to mp peds.
* Fix fast running & fast swimming options not being set/saved correctly.
* Add missing DLC vehicles to the Vehicle Spawner menu.
* Fix bullet proof tires option not working correctly. Will now also be locked if godmode is enabled and restore to the previous setting if godmode is disabled.

Pfew, that's all. I sure hope you enjoy the update.

----------------

## v1.4.0 (October 6 2018)

* Fixed time jumping/glitchiness that appeared since v1.3.0.
* Added missing DLC vehicles into the Vehicle Spawner menu.
* Upgrade vMenu's config system once again.

**THIS UPDATE REMOVES THE config.ini FILE AND INSTEAD USES NEW CONVARS IN THE PERMISSIONS. CFG FILE.**

*__Note that this update requires your FXServer artifacts to be up to date (around 801 and up should be fine).__*
Info on how to setup the new config system can be found on the [configuration](/vmenu/legacy/configuration/) page.

* Improved server-side weather management.
* Added a new submenu called 'Recording Options', which allows you to start/stop recording using the in-game recording feature. It also allows you to enter the Rockstar Editor.
* Fixed vehicle extras being toggled on multiple vehicles.
* Fix small server console logging format mistake.
* Add 2 new options to the Time Options menu to allow for setting a custom hour & minute of the day.
* Fix pre-set teleport locations from teleporting to a safe location instead of the actual configured coordinates.

----------------

## v1.3.0 Changes (October 3 2018)

### Big/important update

* (fixed) Spawning new vehicles now correctly re-sets the forward speed.
* Removed a lot of unused stuff.
* Created a new ConfigurationManager. All vMenu configuration options are now handled through a new file called `config.ini` which can be found inside the `\config\` folder. **Convars are no longer being used, from this version on!**
* Added ELS compatibility config option.
* Added a new voice chat option, that will display a small icon in the bottom left corner, indicating whether your microphone is muted or if you're talking to the server through voice chat.
* Added a lot of new configuration options, please check the [updated docs](/vmenu/legacy/configuration/) page for info on all the new config options, and how to use them in the new config file!
* Added `locations.json` . This file is used for configuring pre-set locations to either teleport to or to add as a blip on the map. (permissions support included)
* vMenu can now be used without having to set up permissions! Simply disable the permissions usage in the config section. Doing this will allow **all** options, **except for actions like kick, ban, unban, kill, etc**.

### New permissions

* `vMenu.Staff`
* `vMenu.MiscSettings.LocationBlips`
* `vMenu.MiscSettings.TeleportLocations`

----------------

## v1.2.3 & v1.2.4 (October 3 2018)

* Fixed release publishing issues

----------------

## v1.2.2 (September 25 2018)

* fixed compatibility issue with other resources trying to disable the minimap.
* (maybe) fix some issues with tempbanning (it now uses the exact same functions that perm banning uses, only the date is different)
* update server console logging to include colors for warning/info/error messages. Also update console logging to be more universal and over all just improved messages.
* fix the 'replace previous vehicle' checkbox not showing the correct on/off state (it was showing the 'default' value for the 'spawn inside car' toggle.)
* add multiple client side notifications above the minimap for multiple actions. Things like banning should now always notify the original (staff) user if the ban was successful or failed, and if it failed it should provide a description of _why_ it failed.
* fixed/changed control keys for menu navigation (it's still up/down arrow keys and scrolling, but no longer some other random keys that were overlapping).
* catch errors when parsing addons.json, if you mess up your addons.json file, you'll now get a decent error message in the server console exactly showing you where the problem is. (looks like this:) ![alt](https://www.vespura.com/hi/i/2018-09-25_14-16_6a5f9_19.png)
* added a new config option to the __resource.lua file to allow you to keep all (previously) spawned vehicles persistent if you turn off the "replace previous vehicle" option in your vehicle spawner menu. (disabled by default).

----------------

## v1.2.1 (September 16 2018)

* fixes multiple banning related critical bugs.
* Auto-banning of cheaters is now **_disabled_** by default, can be enabled using this convar: `set vMenuBanCheaters "true"`.
* The `vMenuLogBanActions` and `vMenuLogKickActions` options are now **_enabled_** by default.
* Fixed compatibility issues between the Player Blips and the option in "Online Players" to draw a route to a player.

----------------

## v1.2.0 (September 7 2018)

* (multiple commits) Added player blips! **(note, this is still a work in progress! It might not be 100% :bug: -free!)**
* [cec725b](https://github.com/TomGrobbe/vMenu/commit/cec725b127bd2c1525395ce0186eaacec1bd2fdd) Fixed a resolutions bug in NativeUI.
* [829ba7f](https://github.com/TomGrobbe/vMenu/commit/829ba7fcdcd60ccf0a9542b972e244a4ad5ae86b) Fixed issue [#109](https://github.com/TomGrobbe/vMenu/issues/109).
* [886d1bc](https://github.com/TomGrobbe/vMenu/commit/886d1bc814888902409ae5bf763c552771faa9ed) Added missing permissions node for vehicle 'flash highbeams on honk' option.
* [ec7fed0](https://github.com/TomGrobbe/vMenu/commit/ec7fed03a43990968a7158d246ae74b5cec35b16) Changed the order/layout of some menus.
* [4ee8b66](https://github.com/TomGrobbe/vMenu/commit/4ee8b6649130fb7976d46deb08a04c4b408e111e) Changed some descriptions and other text strings.
* [598a435](https://github.com/TomGrobbe/vMenu/commit/598a4356fe7ed42b9db4a20cd39f9b7f60b9e9b2) Improved readability and removed some clutter/messy things.
* [91eba95](https://github.com/TomGrobbe/vMenu/commit/91eba95dae9e6e2c684443a41162493776d16f5a) Finally removed the default `"adder"` text inside the textbox when spawning a vehicle by name. It's now empty whenever you want to spawn a vehicle, meaning you can start typing right away.
* [bb21065](https://github.com/TomGrobbe/vMenu/commit/bb2106504dc48dfa09019735859beca1e3b47fbe)

Saved vehicles can now be spawned even with the "Vehicle Spawner" menu "disabled" (no permissions) as long as the saved vehicle belongs in a vehicle class that has been allowed.
For example. If you do *not* give players access to the vehiclespawner menu, but you still want them to be able to spawn saved vehicles from the "offroad" class, simply give them the "vMenu. VehicleSpawner. OffRoad" permission and they'll still be able to spawn that vehicle using the saved vehicles menu.

* [24957c6](https://github.com/TomGrobbe/vMenu/commit/24957c6956354dd331c35ecc0c35896903efdd22)

Maybe fix some tempbanning issues, not sure. (see also: [bc4ea4c](https://github.com/TomGrobbe/vMenu/commit/bc4ea4c16fbe8bd8020bd37d8798f784aadac4b0))

* [9273b46](https://github.com/TomGrobbe/vMenu/commit/9273b46f7d66e0fd01191ee16bff74038a0e76a8) Completely changed the 'set waypoint to player' feature, it's now a live-route instead of a the old waypoint system.
* [0eb208c](https://github.com/TomGrobbe/vMenu/commit/0eb208c1d58355e28874fa565feafc7de454ee2c) Fixed bug that caused vMenu to freeze/crash when spawning a vehicle from the 'trains' category.
* [804cbb7](https://github.com/TomGrobbe/vMenu/commit/804cbb7c20804b7e09706aaa42e16554b3d4aaf7) Add permission node to the "connection options" menu in misc settings.

### New permission nodes in v1.2.0

* `vMenu.WeaponOptions.SpawnByName` Allows players to spawn any vehicle by name.
* `vMenu.VehicleOptions.FlashHighbeamsOnHonk` Allows players to use the "Flash Highbeams on Honk" feature.
* `vMenu.MiscSettings.PlayerBlips` Allows players to enable player blips on the map.
* `vMenu.MiscSettings.ConnectionMenu` Allows players to use the "Connection Options" submenu inside the "Misc Settings" menu.

----------------

## v1.1.7 (September 2 2018) (Hotfix)

* Small update, fixes a bug that caused files like bans.json breaking whenever weird characters (or non-ascii) characters were used (often in someone's username).
* While I was fixing that, I've also added formatting to the bans.json file for easier reading. **_note that you should not edit the bans.json directly, ESPECIALLY WHENEVER THE SERVER IS RUNNING._**
* [Download](https://github.com/TomGrobbe/vMenu/releases/tag/v1.1.7)

----------------

## v1.1.6 (July 20 2018)

### UC

* Add version to server convar

### Documentation & config

* Some changes and improvements in the README.md and other .md files
* Improve default permissions.cfg file, group inheritance is now fixed in recent server builds, so the default permissions.cfg file has implemented the group inheritance again. I suggest you rewrite your permissions file (use the new default one as a template) if you want to keep it clean and use proper group inheritance.

### PO

* Add unlimited stamina permission ( `vMenu.PlayerOptions.UnlimitedStamina` )

### FC

* Nerf player stats, they used to be way too OP (mainly weapon/shooting stats were insane)
* Fix #95 and slight performance improvements.
* (WIP: location and player blips, not yet fully implemented so will not work but preparation work has been done so it can be implemented soon)

### UD

* Refactor some default settings for new players. (won't affect your current saved settings)

### MS

* along with FC (WIP: location and player blips, not yet fully implemented so will not work but preparation work has been done so it can be implemented soon)

### project files

* Changed debugging to embedded for better stack traces.

### CF

* Merge pull request #103 (fix tyre smoke color bug)

### VW

* refactor all weapons & their names + components (& component names)
* add missing weapon components

### Server changes

* Added unban command that can only be executed by the server console to unban a player.

To unban a player using the server console, type `vmenuserver unban "<playername>"` (replacing `<playername>` with the real player's name, NOT case sensitive, allows spaces in player names as long as you surround the name with `""`.

----------------

## v1.1.5 (June 6 2018)

* Fixed a bug that crashed the game when unlimited ammo was turned on and you equipped an MK2 weapon.
* Also improved the update checker.

----------------

## v1.1.4 (June 3 2018)

### Added (v1.1.4)

* [ab87d35](https://github.com/TomGrobbe/vMenu/commit/ab87d354ab18dc3727190e82e7dbab66f6c244a5) added: add notifications that will appear in-game each time you join when the resource is outdated, to notify the server owner of a new update.
* [50b7a91](https://github.com/TomGrobbe/vMenu/commit/50b7a91196445d3b669071475f45b8d47b090d5e) added: new vehicle 'special' godmode feature (use this for Phantom Wedge and similar vehicles).
* [88e55e2](https://github.com/TomGrobbe/vMenu/commit/88e55e2a795e03a9e68441d68d8251998fb74937) added: new kick-action log feature. Requires convar `set vMenuLogKickActions "true"` in server.cfg file. (Must be ABOVE `start vMenu` in server.cfg). Actions will be logged to "/resources/vMenu/vmenu.log" (bans will now also be logged to that file).
* [31bbe7a](https://github.com/TomGrobbe/vMenu/commit/31bbe7a5bb684cee78a69cdee8d5a2dceed2f33d) added: quit game, quit session and disconnect from server buttons to a new misc settings submenu. Currently does not require any permissions to use. Will be added in the future.
* [c7f75cd](https://github.com/TomGrobbe/vMenu/commit/c7f75cd5672d6ce1787836e51f6276029052560d) added: more notifications/subtitles for some menu related actions.
* [93c8c8d](https://github.com/TomGrobbe/vMenu/commit/93c8c8d172912b332a822d9574169ca873050a2b) added: new/more callback notifications for when players get kicked or when the kick action fails.
* [c391fc4](https://github.com/TomGrobbe/vMenu/commit/c391fc410a825d6cd8d9d3379d21175c5b2c0d76) added: whenever you spawn a new vehicle, the vehicle will now get the same speed & RPM of your previous vehicle (only works if you were inside a vehicle that was on the move whenever you spawned the new vehicle). So you will no longer stop dead in your tracks whenever spawning a new vehicle.
* [e208ddc](https://github.com/TomGrobbe/vMenu/commit/e208ddcf2c004dfe1abf4bd0f04cb5b1ebc6703b) added: added all missing dlc weapons (new permissions will be listed below).

### Changed (v1.1.4)

* [bfa1368](https://github.com/TomGrobbe/vMenu/commit/bfa1368b2b9b2a6552c1b4b9d308c9a1eefe33f9) update: small performance updates.
* [eb7e3d8](https://github.com/TomGrobbe/vMenu/commit/eb7e3d8100c5eb785bf57a4b149d5fd40694308b) update: improve logging of bans and changed the log format to be easier to read when viewing the file. Same format is used for new kick-action log feature.
* [9f60c33](https://github.com/TomGrobbe/vMenu/commit/9f60c33761335a80fe4115adce372426f01e7fbd) update: (dev change) modified notifications and subtitle classes.
* [63d7667](https://github.com/TomGrobbe/vMenu/commit/63d7667c3d83170027c4eff27331e7d4b88ff9b2) update: (dev change) cleaned up code internally.
* [07c28be](https://github.com/TomGrobbe/vMenu/commit/07c28be661bca26fdb517583f54738eb6da5e164) update: (dev change) added more comments to some parts of the code.
* [e9df970](https://github.com/TomGrobbe/vMenu/commit/e9df97005c2c04cbca80d9b584255d161fd803f8) update: (dev change) testing dll for nativeui (this commit was broken, has been fixed in future commit).
* [4d2bd20](https://github.com/TomGrobbe/vMenu/commit/4d2bd200e15c96d1bd459d36f972a66492f9bc0f) update: (dev change) moved the order in which a menu gets created and refreshed/updated.
* [eb7f014](https://github.com/TomGrobbe/vMenu/commit/eb7f0140bfca8556f88d7c14a7457d04c62511c1) update: (dev change) added loading/saving of json strings to user saved data. (used for experimental features)
* [0fe49a0](https://github.com/TomGrobbe/vMenu/commit/0fe49a08977aca71a35d7ae6181786af3f58f6d2) update: more attempts at fixing the random issue someone was having regarding banning players breaking randomly. (not really any progress on finding the cause)
* [a89b479](https://github.com/TomGrobbe/vMenu/commit/a89b4798ed391e9ef8ca5d89d4cbf304f7116b5a) update: notifications above the minimap will now blink by default, this is useful when multiple messages are shown at the same time, and the same message already on the screen is being shown again, then you will now see which notification just got triggered.
* [4b588b9](https://github.com/TomGrobbe/vMenu/commit/4b588b9ba5a0b59624a2d0612086ded0056303ac) update: cleaned up online players menu/actions and added more notifications so the user knows what happened.
* [b7926d0](https://github.com/TomGrobbe/vMenu/commit/b7926d000847d6efd925e194ddb820d4bd2342d6) update: changed the order of some things inside the online players menu when performing some staff action on the target player.
* [c975692](https://github.com/TomGrobbe/vMenu/commit/c97569271130af8673c5b4e1a188fd1581ba9cac) update: (ban manager) improved logging on the server and improved the callback notifications displayed to the source user whenever a staff action failed.
* [27c9255](https://github.com/TomGrobbe/vMenu/commit/27c925572200107d55521d4bf902c73f329485f6) update: improved debug logging (server side)
* [3378f00](https://github.com/TomGrobbe/vMenu/commit/3378f00cdb9ab875862eb3c075f411fbc77fdebb) update: disable unsafe mp character customization options for now. Will be added in the future but currently it will break everything, so please don't try to enable this as it will screw up and modify your ped saves.
* [2d2af92](https://github.com/TomGrobbe/vMenu/commit/2d2af92e517ffffaa7ff0da8fe9dce7dad99608e) update: (dev change) sort weapon names/hashes/permissions A-Z for easier expansion and still being able to see what's going on.

### Fixed (v1.1.4)

* [339c823](https://github.com/TomGrobbe/vMenu/commit/339c823dc9ff083da45eb2448213c2377e0c2eb9) fixed: Fixed fast running/swimming not saving correctly and disabling itself randomly.
* [b4e98dc](https://github.com/TomGrobbe/vMenu/commit/b4e98dc8771c11418fe9a52abf25cd0acabc6268) fixed: flashing high-beams user preference not saving correctly.
* [628eb3f](https://github.com/TomGrobbe/vMenu/commit/628eb3faca6b6f722e4ec8a4bfb93869473bfaab) fixed: added 'null' check to prevent errors in console when first joining.
* [4887b73](https://github.com/TomGrobbe/vMenu/commit/4887b7364b6395f4e89c86c6abb9654d24c5ef99) fixed: quit game and quit session functions accessibility issues have been fixed.
* [b8c1be5](https://github.com/TomGrobbe/vMenu/commit/b8c1be5e05144eec400f712502deb07119bbc119) fixed/update: added some null checks and more to attempt to debug a reported issue that still can't be reproduced, even with the new `Testing Group` on my discord server... 😕
* [0a83cbf](https://github.com/TomGrobbe/vMenu/commit/0a83cbf2b9365d932b297d09608c9c0f8f1b354c) fixed: some peds in the peds spawn list spawned the wrong ped (was only an issue in the dev branch, this was never a bug in the production version of vMenu).
* [efe8175](https://github.com/TomGrobbe/vMenu/commit/efe8175c5d36982c01128eea007fedbce19b97ab) fixed: fixed/improved the new 'special godmode' feature.
* [ab44ce0](https://github.com/TomGrobbe/vMenu/commit/ab44ce07a3f7df724146d2b9997ab73512b30390) fixed: rewrote part of a menu to hopefully prevent vehicle extra's/components being toggled on your previous vehicle whenever you toggle extras on your new vehicle. Didn't happen all the time, but if you had this issue then it should be fixed in this update.
* [21e5e19](https://github.com/TomGrobbe/vMenu/commit/21e5e198c12367af4a01d4083db7aba4326c35b8) fixed: fix for this issue: [#84](https://github.com/TomGrobbe/vMenu/issues/84).

### Experimental stuff

* [e263f13](https://github.com/TomGrobbe/vMenu/commit/e263f13edbb1177569f712f2276e9c2130cac0a5), [0ea71c1](https://github.com/TomGrobbe/vMenu/commit/0ea71c15c0eb00683a116bd26dba1500a9dae9f1), [834be6f](https://github.com/TomGrobbe/vMenu/commit/834be6f9b3bd202562bd86ae1499e7570de7702a), [6499072](https://github.com/TomGrobbe/vMenu/commit/64990727bfac73430f5e759af57d24f817e46c7e), [bd9a0b9](https://github.com/TomGrobbe/vMenu/commit/bd9a0b9d85b329e162340f9de3c082172267898e) & [39ed5fc](https://github.com/TomGrobbe/vMenu/commit/39ed5fcc199cb1df967b117ab70b978fbe0fb56d) experimental update: (experimental, not available in this release) starting to test things out related to MP character customization.
* [87aea60](https://github.com/TomGrobbe/vMenu/commit/87aea60e89c9b00e92753e42ba65f6f71537873d) experimental update: starting to do some things with a 'features config file'. Not yet implemented in this new release, but is a work in progress.

### Travis

* [ca73f46](https://github.com/TomGrobbe/vMenu/commit/ca73f464867e31894c48ed40ef1d2cebf34f1858) travis: attempting some fixes to get travis working again. (not all commits are listed here, because it's not important)
* [9b5b17f](https://github.com/TomGrobbe/vMenu/commit/9b5b17f9f784d8c4774012d4873c0932e2370fd2) (travis: added development branch to trigger test builds.)

### New permissions in this update

* `vMenu.VehicleOptions.SpecialGod` This allows you to use the new special vehicle god mode feature.
* (all new weapon permissions have been included in this list: [weapon permissions](/vmenu/legacy/permissions/weapon-permissions/))

### New convars / settings in this update

* `set vMenuLogKickActions "true"` add this ABOVE `start vMenu` in your server.cfg if you want to log all staff kick actions to a file.

----------------

## v1.1.3 (April 30 2018)

* Important update, bug fixes, performance improvements, new features and support for future updates regarding saved vehicles/peds. [THIS UPDATE IS AN ABSOLUTE MUST, PLEASE UPDATE ASAP.](https://github.com/TomGrobbe/vMenu/releases/tag/v1.1.3)

----------------

## v1.1.2 (April 22 2018)

### New (v1.1.2)

* added an indicator in the weather options menu to show what the current weather type is.
* added walking style options for the multiplayer male/female ped in the player appearance menu.
* added an unban menu. requires the `vMenu.OnlinePlayers.Unban` permission. The menu can be accessed from the Main Menu.
* added an option to log all ban/unban actions of staff-members to a file. to enable this, add `set vMenuLogBanActions true` to your server.cfg file (somewhere above `start vMenu` ).
* [dev] added events that get triggered on the server side when a player was successfully kicked, banned or unbanned (only useful for resource developers).

### Changes (v1.1.2)

* removed src folder from auto-generated download.
* fixed index out of range exception when pressing "enter" in an empty submenu.
* fixed typo making the "no bike helmet" option save correctly now when pressing "save preferences".
* fixed a mistake in the default permissions.cfg file that prevented the `DontBanMe` permission from working (if you customized/changed it, then it would've worked just fine)
* fixed and cleaned up some random minor things, and changed license because too many people were abusing the old license.

----------------

## v1.1.1 (April 16 2018)

* Added _full_ weapons permissions support (each weapon has its own permission now).
* Added gameplay camera rotation locking.
* Fixed kicking, summoning and killing.
* Added ban and tempban options.
* Fixed more stuff.

----------------

## v1.1.0 (April 9 2018)

### Added (v1.1.0)

* 2a1fdde Clear Area option in Misc Settings (requires: `vMenu.MiscSettings.ClearArea` permission). It will clear the area around you (100 meters) of all damage to the world, objects, props, broken street lights (and similar world-details), vehicles, peds, etc, etc.
* 72d3fc9 Added a new convar ( `set vMenuDisableDynamicWeather "true"` ) that when set will disable dynamic weather changes by default when the server starts. It can still be turned on if players  have permission to toggle it, but it'll be disabled by default with this convar set.
* 706efdf Added the `KAMACHO` . Apparently this car was missing from the off-road category, but nobody noticed or bothered reporting it, so this has now been fixed.

### Changes (v1.1.0)

* 3a0beb3 Changed the design of the Weapon Options menu. Every weapon is now in their correct Weapon Category. Reducing the 82 items long list in some better organized submenus. Full permissions support will be added soon.
* c0d8518 Changed the weather system to increase the chance of "good weather" and reduce the chance of "bad weather" (rain/thunder/overcast).
* 454703a Some internal cleaning up.

### Fixes and improvements (v1.1.0)

* 9de2eea Fixed addon list permission bug for vehicle spawning. This list will now also check for vehicle classes permissions.
* 65ce0b5 Improved some "spacer items centering" internal functions (used in the new Weapon Options menu and the Vehicle Colors submenu).
* 087a589 Fixed/improved vehicle and player freezing/unfreezing, allowing for better compatibility with other resources relying on the player to be frozen without external resources trying to unfreeze the entity constantly.
* d1e5b0f Fixed an issue which allowed people to abuse the kick/summon/kill options by injecting code into the game and creating fake events. It's unlikely this has been used a lot, if at all, so it's nothing to worry about. Especially because it's now fixed 😊.
* c1e8aa8 Fixed saved vehicles being spawned multiple times when opening/closing the menu a lot and disabling "replace previous vehicle". Thanks to [Deltanic](https://github.com/Deltanic) for reporting this some time ago and helping me test/debug this issue.
* fa28ac9 Also reported by Deltanic, was that sometimes your car would not be deleted/placed on the ground correctly when already in a vehicle and spawning a new one. This is now changed so if you disabled "replace previous vehicle" and you spawn a new car while already being inside one, your old one won't be deleted and your new car will spawn in front of your old car.

----------------

## v1.0.9 + v1.0.9 hotfix (March 20 2018 + April 3 2018)

* **_Hotfix has just been released, fixing a corrupt dependency file._**

### Bug fixes, new features and some features have changed. Also quite a few internal changes

* Added more realistic suicide options. (random: 50/50 chance: either take the pill, or shoot yourself in the head (currently slightly bugged because the gun does not make any sound when the animation is playing, still need to figure out how to solve that).
* Changed unlimited stamina to use stats instead of a repeating native call. Also included driving, flying, shooting, and other MP stats to be 100% at all times. This also greatly increases the max ammo count for weapons 🙂
* Improved performance and fix incompatibility issues with _some_ other [random] resources. Help text being drawn by buggy resources could cause issues for vMenu. Not sure if it's fixed 100% but I guess we'll find out for sure when this update is shipped out.
* Added more debugging when debug mode is on. I'm trying to figure out an issue relating vehicles not being deleted or spawning x amount of times instead of only once when toggling "Replace Previous Vehicle" on/off repeatedly, this bug is still in this version as I'm still not sure how to solve it. For the time being, just keep Replace Previous Vehicle turned on to prevent this.
* Fixed another couple of NativeUI bugs, hopefully reducing the weird positioning on some systems -although the previous 6 million of those "fixes" didn't seem to work either so I doubt it'll improve anything- it's worth a try.
* Switched to another version of Newtonsoft. Json. It's now compatible for client and server side. Saving things to the client will now be done using Newtonsoft. Json instead of my own hacky json parser functions. This will allow for improved saving in the future. For now I've just tried to keep compatibility with existing save files and also keep backwards compatibility for servers using older versions of vMenu. However if I decide to update the system completely, it might mean you either lose your custom saves, or you lose access to them on certain servers using outdated versions of vMenu. Though that's planned for another update.
* Added the `vMenu.VehicleSpawner.DisableReplacePrevious` permission. This is now required in order to be able to turn _off_ the "Replace Previous Vehicle" toggle. This is to give server owners the ability to protect their servers against vehicle spawn spam.
* Last but not least, some misc improvements, not very noticeable but it was needed to get the resource more organized.

----------------

## v1.0.8 (March 20 2018)

* Fixed addon.json file not being loaded properly.
* Fixed weapon spawning and setting all ammo permissions.
* Added new DLC vehicles to the correct vehicle classes/categories. (Note, at the time of writing this you still need to get [Blü's resource to add the vehicles here](https://forum.fivem.net/t/mp-assault-vehicles-in-fivem/93153?u=vespura).
* Added night vision and thermal vision in the Misc Settings menu. Required permissions are: `vMenu.MiscSettings.NightVision` and `vMenu.MiscSettings.ThermalVision` .
* Updated instructions, readme and permissions list on the GitHub wiki pages as well as in the provided readme.md and permissions.md files in the repository.
* Also updated the Travis builds, however this has nothing to do with vMenu features/bugfixes.

----------------

## v1.0.7 (March 19 2018)

### Changes (v1.0.7)

* Added temporary workaround for engine on/off toggle bug.
* Cleaned up code.

### Removals (v1.0.7)

* Disabled saved car spawning if the car is not available on the current server (addon/future dlc cars for example).

### Additions (v1.0.7)

* Added addon vehicles, peds and weapons lists, which can be customized by the server owner (checkout the new addons.json file). If you want to be able to spawn addon vehicles from the addons list, then add this permission: `vMenu.VehicleSpawner.Addon` . If you want to be able to spawn weapon addons from the addons list, then add this permission: `vMenu.WeaponOptions.Spawn` . If you want to spawn addon peds from the list, then make sure you have this permission: `vMenu.PlayerAppearance.SpawnNew` .
* Added "Spawn Weapon By Name" option.
* Added "Spawn Player Model By Name" option.
* Added "Set All Ammo" and "Refill All Ammo" buttons to set/refill the ammo in _all_ of your weapons at once. Requires the `vMenu.WeaponOptions.SetAllAmmo` permission (that single permission will grant access to both those options).
* Added parachute options menu to the "Weapon Options" menu. Equip primary and reserve parachutes, change the style of each of them and set a smoke trail color. Note changing smoke color is a bit buggy. To change it, stop using smoke for about 5-10 seconds, then scroll through the colors a few times and then stop to end up on the color that you want. Then start using smoke again.
* Added permission support for spawning weapons: `vMenu.WeaponOptions.Spawn` is now required to be able to spawn any weapon. (Note, if you have the `vMenu.WeaponOptions.GetAll` permission, you'll still be able to use the "Get All Weapons" button even if you don't have the `vMenu.WeaponOptions.Spawn` permission.
* Added driving tasks options. (Player Options > Player Actions > Drive To Waypoint / Drive Around Randomly). Note that it's still a WIP and you can't set a custom driving style just yet. It's currently set to "rushed".

### Fixes (v1.0.7)

* Fixed a small issue regarding the window title when getting user input (the black input box).
* Fixed saved cars (old format) not being converted/saved correctly to the new format. The save was fine, but it used the wrong save name when re-saving. This is now fixed.
* Attempted another fix/improvement that has to do with the car spawning and deleting bug. It works fine for me, not sure if others will have issues with it. Just note that if you see something like this in the console: `Invalid network/entity ID` that is **NOT** caused by vMenu. It's _another_ script that fails to handle Network ID's correctly.

----------------

## v1.0.6 (March 15 2018)

### Fixes (v1.0.6)

* Fixed 2k/ultra wide monitors from breaking badly when opening the menu (sometimes).
* Improved teleporting to waypoint, it's now a _lot_ less common to get the "could not teleport" error and be teleported randomly near the original waypoint, instead you'll now often be teleported exactly to the waypoint (if possible).
* Almost certainly fixed the vehicle despawn issue. Can't confirm that it's 100% fixed as I've spent 5 minutes spawning new vehicles non stop, and it worked fine every time, but I've not tested it a 2 million and 10th times... Only time will tell if this actually worked.
* Solved some other small bugs I noticed while trying to figure out the bug mentioned above and also fixed those and cleaned up the code a lot regarding the spawning of vehicles.

----------------

## v1.0.5 (March 13 2018)

* Fixes the menu not being displayed on 4k screen resolutions. Also a typo in the version checker has been fixed, thanks @Zach for pointing it out.

----------------

## v1.0.4 (March 12 2018)

* Bug fixes, some visual improvements, added No Clip (requires the `vMenu.NoClip` permission, activate it by pressing `F2` by default, button can be changed by adding `set vMenuNoClipKey xxx` in your server.cfg file (xxx being a valid key code of course)). For the full patch notes check the release on GitHub.

----------------

## v1.0.3 (March 9 2018)

* Fixed some (small) permissions related bugs, also fixed the "super fast scrolling" bug for players playing at \>60fps.

----------------

## v1.0.2 (March 8 2018)

* Small bugfixes

----------------

## v1.0.1 (March 8 2018) (Hotfix)

* Fixed a critical permissions bug for when you setup the permissions using `.All`.

----------------

## v1.0.0 (March 8 2018)

* First release.

----------------

