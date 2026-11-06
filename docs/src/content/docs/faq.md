---
title: "FAQ"
description: "Answers to questions that come up often about vMenu Enhanced."
---

## A clothing item says it could not be applied

:::note[FiveM Legacy]
This is far more likely to happen on FiveM Legacy than on FiveM Enhanced.
:::

Sometimes you scroll to a piece of clothing or a prop, the number goes up, but your character keeps wearing the previous item. The row then says **not applied**, the description shows a red line with the number that failed, and a short notification pops up.

This means the game refused to put that item on. vMenu asked for it, the game says it exists, but it never loads it. It happens in Player Appearance and in the Custom Character Creator alike, and vMenu cannot force it.

Some items take a moment to load. When that takes longer than a second, the description shows an orange line saying the item is taking a while to load. vMenu waits up to 4 seconds before it counts the item as not applied.

The numbers in the menu start counting at 1, so the ID in that red line is the number you see in the menu. The game itself starts counting at 0, so the real ID in the game is always 1 lower. ID 446 in the menu is ID 445 in the game.

You can keep scrolling. vMenu skips past items that fail, so you can still reach everything after them. Jump To A Number works too.

### Why it happens

The usual cause is the clothing files on the server or on your game:

1. A streamed clothing pack replaces one of the base game items with a broken file.
2. A clothing pack was made for a different game build than the one the server runs.
3. Your own game files for that item are damaged. Verifying your game files _could_ fix that, but it's not guaranteed.
4. Just a random FiveM/GTA bug. Nothing we can do about this. We've seen this happen to native R\* games clothing items, without any streamed clothing items present. So there's no way to reliably tell what exactly causes it.

### What a server owner can do

Write down the numbers that fail.
Take out your streamed clothing packs one at a time and check if the item works again.
The pack that fixes it once removed is the one with the broken file.

If the item still fails with every clothing pack removed, the problem is with that item in the game itself, and the only thing to do is skip it.
