---
title: "Moving Your Data"
description: "How to copy everything vMenu has saved on your computer into one code, and restore it somewhere else."
---

## Why should you regularly backup your data? 

vMenu Enhanced uses KVP for storing player data on the players' computer. This is how vMenu Legacy
did it as well, and works fairly well. Unfortunately FiveM for GTAV Enhanced no longer stores player 
data between different servers (this might be a bug, I reported it but haven't heard back yet, 
if this changes I will be updating these docs). This means that if you connect to another server,
or right now even connect to the same server but with a different IP, hostname or join link, your
data will not be there when you load in. It still exists on disk, but it's only reused whenever
you join the same server with the same connection method and IP again.

That's why I've made an import and export tool so you can share you data between sessions.

Another reason why you should keep a backup at all times is because sometimes (very rarely) KVP
gets corrupted. In which case you'll be glad to have a backup at the ready. Even if KVP doesn't
corrupt, your computer might stop working one day, or you may accidentally delete your KVP storage
without realizing it. Having a backup is just a smart idea.

## Making a code

The menu is at **Misc Settings, Import & Export**.

The first row tells you what is currently saved on this computer, so you can see at a glance whether
there is anything worth backing up. Then:

1. Pick **Show My Transfer Code**.
2. A box opens with the code already selected. Press `Ctrl+C`, or click **Copy**.
3. Paste it somewhere it will survive. A text file, a note, a message to yourself.
4. Press `Escape` to close.

The code is one long unbroken run of letters and numbers starting with `VME1`. It is compressed, so
even a big collection usually comes out as a few thousand characters rather than a few hundred
thousand. Line breaks do not matter. If your chat app wraps it across ten lines, paste it back in
exactly as it is and vMenu will sort it out.

## Bringing it back

Also under **Misc Settings, Import & Export**, and there are two ways to do it.

**Paste A Code** adds everything from the code to whatever is already saved. If you have a vehicle
here that is not in the code, it stays. If a vehicle exists in both, the one in the code wins. This is
the one to use almost always, and it is safe to run twice, because pasting the same code again just
writes the same things over themselves.

**Paste A Code And Replace Everything** deletes every vehicle, ped, loadout and setting saved on the
computer first, and then reads the code. You end up with exactly what was in the code and nothing
else. It asks for confirmation before it does anything, and you want it only when you are deliberately
starting over from a known good copy.

Either way, paste the code into the box with `Ctrl+V` and press `Enter`. vMenu tells you how many
things it brought back, and everything takes effect straight away. You do not need to reconnect.

## What is and is not in the code

In it:

- Saved vehicles, and the categories you sorted them into
- Saved peds, and their categories
- Weapon loadouts
- Every vMenu setting you have changed, including your language and which side the menu sits on

Not in it:

- Settings that belong to plugins. Plugins keep their own preferences under their own resource name,
  which vMenu cannot read or write, so those do not travel.

## When something goes wrong

If the box says the code is damaged or incomplete, the usual cause is a partial copy. Go back to
where you saved it and make sure you have the whole thing, from `VME1` all the way to the end.

If vMenu says the code was made by a newer version of itself, then it was, and this build does not
know what some of the things inside it mean. Update vMenu Enhanced and try again, or ask whoever made
the code for one made with the version you are running.

If a few items are skipped, vMenu says how many and writes a line to the console for each one. Press
`F8` to read them. The most common reason is the friendly one: something saved on this computer was
written by a newer vMenu than the one that made the code, so merging left it alone rather than downgrading it.

## For server owners

You don't have to worry that being able to import preferences and saved items from another server will cause problems
on your server. Anything restricted by permissions will still have the same permission check applied when settings
are restored. Same goes for any configuration options.
Any saved cars or weapon loadouts that may be added, will still follow the permissions and configuration.
Peds may appear broken between servers, but that's to be expected if different servers have different clothing collections
that may conflict with their ids.