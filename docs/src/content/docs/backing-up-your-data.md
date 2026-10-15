---
title: "Backing Up Your Data"
description: "How to back up everything vMenu has saved on your computer as one code, and restore it later or on another server."
---

vMenu saves your vehicles, peds, characters, loadouts and settings on your own computer, using KVP. KVP is FiveM's small storage space for each server. It can get corrupted, deleted by accident, or lost with your computer, and every server has its own, so nothing you save on one server shows up on another.

A backup code fixes all of that. It is one piece of text holding everything vMenu saved, which you can keep somewhere safe and paste back in whenever you need it, on the same server or a different one.

:::note[Only for vMenu Enhanced]
The original vMenu, v3.9.2 and older, has no backup feature. To bring its saves over to vMenu Enhanced, use [Import Saved Data From Old vMenu](/vmenu/enhanced/old-vmenu-import/). It turns them into a code you can paste in.
:::

## Making a backup

1. Open **Misc Settings, Import & Export**. The first row shows what is saved on this computer.
2. Pick **Show My Transfer Code**. A box opens with the code already selected.
3. Press `Ctrl+C`, or click **Copy**, and paste it somewhere safe, like a text file or a message to yourself.

The code starts with `VME1` and is usually a few thousand characters long. Line breaks do not matter, so it is fine if your chat app wraps it.

## Restoring a backup

Open **Misc Settings, Import & Export** again, pick one of these, paste with `Ctrl+V` and press `Enter`:

- **Paste A Code** adds everything from the code to what you already have. Things that exist in both are replaced by the code's version. This is the one you almost always want, and running it twice does no harm.
- **Paste A Code And Replace Everything** first deletes everything vMenu saved on this computer, so you end up with exactly what was in the code. It asks you to confirm.

Everything takes effect straight away.

:::caution[Between FiveM Enhanced and FiveM Legacy]
You can restore a backup made on FiveM Enhanced on a FiveM Legacy server, and the other way round. Most things come through fine, but some, like custom characters, can come out wrong. The two games do not always use the same numbers for the same clothes and other items, so a piece can turn into something else.
:::

## What a backup holds

Saved vehicles, peds, custom characters with their outfits and hair styles, weapon loadouts, all their categories, and every vMenu setting you changed. Plugin settings are not included, because plugins store those themselves.

## Editing or combining backups

- [Edit Your Data](/vmenu/enhanced/data-editor/) opens a code in your browser, so you can look through it, rename or remove things, and download a new code.
- [Compare Or Merge Data](/vmenu/enhanced/data-compare/) takes two codes, for example one from each server you play on, shows what is different, and merges them into one.

## When something goes wrong

- **The code is damaged.** Usually only part of it was copied. Make sure you have all of it, from `VME1` to the end.
- **The code was made by a newer version.** Update vMenu, or ask for a code made with your version.
- **A few items were skipped.** Press `F8` to see which ones. Usually the copy on this computer was newer than the one in the code, so vMenu kept it.

## For server owners

Restoring a backup cannot get around your rules. Everything it brings back goes through the same permission and configuration checks as anything else. Clothing from packs your server does not have goes back to the default, and the player is told how many pieces were missing. Very old saves can still come back wearing the wrong clothes, and saving them once more fixes that.
