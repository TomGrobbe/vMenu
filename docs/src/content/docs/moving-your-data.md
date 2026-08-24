---
title: "Moving Your Data"
description: "How to copy everything vMenu has saved on your computer into one code, and restore it somewhere else."
---

## Why you want a backup

vMenu Enhanced saves your data on your own computer using KVP, the same as vMenu Legacy did. On FiveM Enhanced that storage is no longer shared between servers, so joining a different server, or even the same server with a different IP, hostname or join link, leaves your data behind. It is still on disk, it just only comes back when you join the same way again. This may be a bug, it has been reported, and these docs will be updated if it changes.

That is what the import and export tool is for. It also covers the rarer problems: KVP occasionally gets corrupted, computers die, and storage gets deleted by accident.

## Making a code

The menu is at **Misc Settings, Import & Export**. The first row tells you what is currently saved on this computer.

1. Pick **Show My Transfer Code**.
2. A box opens with the code already selected. Press `Ctrl+C`, or click **Copy**.
3. Paste it somewhere it will survive, such as a text file or a message to yourself.
4. Press `Escape` to close.

The code is one long run of letters and numbers starting with `VME1`. It is compressed, so even a big collection is usually a few thousand characters. Line breaks do not matter, so if your chat app wraps it, paste it back exactly as it is.

## Bringing it back

Also under **Misc Settings, Import & Export**, and there are two ways.

**Paste A Code** adds everything from the code to whatever is already saved. Anything here that is not in the code stays, and anything in both is replaced by the code's version. This is the one to use almost always, and it is safe to run twice.

**Paste A Code And Replace Everything** deletes every vehicle, ped, loadout and setting on the computer first, then reads the code. You end up with exactly what was in the code. It asks for confirmation, and you want it only when you are deliberately starting over from a known good copy.

Either way, paste with `Ctrl+V` and press `Enter`. vMenu tells you how many things it brought back, and everything takes effect straight away.

## Looking inside a code

The [Edit Your Data](/vmenu/enhanced/data-editor/) page unpacks a code in your browser and shows you every saved vehicle, ped, character, loadout and setting it holds. You can rename things, change them, throw out what you no longer want, and download a fresh code along with a backup of the one you started with.

## What is in the code

In it:

- Saved vehicles, and the categories you sorted them into
- Saved peds, and their categories
- Custom characters, their categories, and every outfit and hair style saved against them
- Weapon loadouts
- Every vMenu setting you have changed, including your language and which side the menu sits on

Not in it:

- Plugin settings. Plugins keep their own preferences under their own resource name, which vMenu cannot read or write.

## When something goes wrong

**The code is damaged or incomplete.** Usually a partial copy. Check you have the whole thing, from `VME1` to the end.

**The code was made by a newer version.** Update vMenu Enhanced and try again, or ask whoever made the code for one made with your version.

**A few items were skipped.** vMenu says how many and writes a console line for each. Press `F8` to read them. The usual reason is harmless: something saved on this computer was written by a newer vMenu than the one that made the code, so merging left it alone rather than downgrading it.

## For server owners

Importing from another server cannot bypass your rules. Restored settings, saved cars and weapon loadouts all go through the same permission and configuration checks as anything else.

Peds and characters saved by a recent vMenu record which clothing pack each piece came from, so they survive being restored on a server with a different set of packs. Anything your server does not have goes back to the model default, and the player is told how many pieces were missing. Saves written before that was recorded can still come back wearing the wrong thing, and the only fix is saving over them once.
