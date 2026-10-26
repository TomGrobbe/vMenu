---
title: "Custom Character Creator"
description: "Building a freemode online character in vMenu Enhanced, and the two files a server owner can use to add tattoos and ready made outfits."
---

The Custom Character Creator is where a player builds one of the two "freemode" characters, the same kind GTA Online makes you build the first time you play. You pick two parents, mix their faces, push the sliders around, then add hair, makeup, tattoos and clothes.

It sits under **Player Related Options, Custom Character Creator**, right below Player Appearance. The two do different jobs: Player Appearance changes the clothes on whatever ped you are wearing, the Custom Character Creator makes the person underneath.

## For players

### Making one

Pick **Create Male Character** or **Create Female Character**. You cannot do this from inside a vehicle, because the creator freezes you in place and points a camera at you.

| Page | What is on it |
| --- | --- |
| Randomize Character | Rolls a whole new person, face and clothes included |
| Character Inheritance | The two parents, and how much of each one shows |
| Character Appearance | Hair, beard, eyebrows, makeup and the rest of the skin layers |
| Character Face Shape Options | Twenty sliders for the shape of the face itself |
| Character Tattoo Options | Tattoos and badges, grouped by where they go |
| Character Clothes | What the character is wearing |
| Character Props | Hats, glasses, earrings, watches and bracelets |
| Clothing Collections | The same clothes, grouped by the pack they came from |
| Facial Expression | The mood the face rests in |
| Character Category | Which group this one is saved into |

Backing out is blocked on purpose. Save the character, or use **Exit Without Saving**, which asks you to press it twice.

### Moving the camera

| What you want | Keyboard and mouse | Controller |
| --- | --- | --- |
| Look from another angle | Move the mouse | Right stick |
| Swing around the character | `A` and `D` | Left stick left and right |
| Zoom in and out | `W` and `S` | Right and left trigger |
| Raise and lower the camera | `E` and `Q` | Left stick up and down |
| Put the camera back where it started | `C` | Right stick click |
| Turn the whole character around | `Space` | `X` on Xbox, square on PlayStation |

The camera follows whichever row you are on, so scrolling onto the shoes row shows you the shoes. From there it is yours to move and it stays put while you keep working on the same page. The character looks back at the camera on their own, until you walk far enough round the back that a neck cannot turn any further.

**Disable Auto Camera** at the top of the creator stops the camera jumping around, so it stays exactly where you left it. `N` on the keyboard, or the right bumper on a controller, does the same without opening the page. Like every key in vMenu Enhanced it can be rebound, and the choice is remembered between sessions.

### Matching the arms to your clothes

A freemode character's arms are not part of the top you wear. The game keeps them in a slot called **Hands / Upper Body**, and that slot decides whether you get bare arms, rolled up sleeves, a jacket sleeve or gloves. So picking a leather jacket does not on its own give you leather sleeves.

GTA Online picks the arms for you every time you change clothes in a shop, and the creator now does the same. Change a top, undershirt or pair of trousers and the arms follow. Gloves survive the change too.

Turn off **Match Arms To Clothes** at the top of the **Character Clothes** page if you would rather choose them yourself. The setting is remembered between sessions.

Two things worth knowing:

- Changing the Hands / Upper Body row yourself is never overridden. Your choice stays until the next time you change a top.
- The gloves that go with the flapper dress are the one pair that comes off when you change tops.

This only happens inside the creator. **Player Appearance** stays completely manual.

### Saving one you did not make here

If something else on the server dressed you as a freemode character, **Save Current Character** takes a full copy: the face, the skin layers, the hair, the tattoos and the clothes. vMenu Legacy could not do this.

### Outfits and hair styles

A saved character is three separate things. The **character** is the person: the face, the skin tone, the tattoos, and there is one of those. An **outfit** is a set of clothes, and a **style** is a haircut with its beard and makeup. You can save as many outfits and styles as you like against one character, so it can have a work outfit and a going out outfit without being saved twice.

Both live under **Saved Characters**, pick a character, then **Outfits** or **Hair & Makeup Styles**. Wear something you like, pick **Save Current As New**, and give it a name.

Every character also keeps whatever it is wearing right now, separately from those lists. Editing a character, or putting a saved outfit on, only changes that. A saved outfit is only ever written to when you ask for it by name, with **Save Current As New** or **Replace**, so the creator can never quietly rewrite one. You are also free to delete every saved outfit a character has, it carries on wearing what it had on.

### Coming back as yourself

**Set As Default Character** means you turn into that character whenever you respawn, and when you first join. Pick it again to turn it off.

### Seeing one before you spawn it

While you scroll through **Saved Characters**, the character you are on stands beside the menu wearing what it would spawn in, so you can tell your saves apart without trying each one on. It stays there when you open that character. Hold **Q** or **E** to turn it around. Saved peds get the same preview in the **Saved Peds** menu.

It is on by default. Turn off **3D Ped Preview** under **Miscellaneous Settings** if you would rather not see it.

### Your saves travel with you

Characters are saved on your own computer, like saved vehicles and peds, and they are included in the backup code on the [Backing Up Your Data](/vmenu/enhanced/backing-up-your-data/) page. Make a backup before you rely on them.

## For server owners

### Tattoos

The tattoo list is [tattoos.json](/vmenu/enhanced/configuration/tattoos-json/).

### Ready made outfits

Outfits you publish for everybody are in [clothing-presets.json](/vmenu/enhanced/configuration/clothing-presets-json/).

### Outfits that came with the game

The **GTA Online Outfits** page needs no configuration. It reads the outfits Rockstar shipped with each content pack straight out of the game and groups them by pack. It is read only, and it has its own permission so you can hide it.

### Clothes that survive an update

Every outfit vMenu saves records which content pack each piece came from, as well as its number. That matters because the numbers move. Install one new clothing pack and every piece that sorts after it shifts up, so an outfit saved last week comes back as somebody else's shirt.

Recording the pack means the shirt is found again by name and number within that pack, which does not move. If a pack is genuinely gone from your server, that slot goes back to the model default and vMenu says how many pieces it could not find.

This applies to saved peds too. Saves written by an older vMenu keep working as they did, and pick up the extra information the next time they are saved over.

## Permissions

| Permission | What it allows |
| --- | --- |
| `...CharacterCreator.Menu` | Seeing the menu at all |
| `...CharacterCreator.Create` | Building a new character, and editing a saved one |
| `...CharacterCreator.Save` | Saving to their own collection |
| `...CharacterCreator.Spawn` | Turning into a saved character |
| `...CharacterCreator.Manage` | Renaming, cloning, categorising and deleting |
| `...CharacterCreator.SetDefault` | Choosing who to respawn as |
| `...CharacterCreator.Presets` | Seeing and wearing your published outfits |
| `...CharacterCreator.PresetsManage` | Publishing and removing them, which writes the config file |
| `...CharacterCreator.OnlineOutfits` | Seeing and wearing the game's own preset outfits |

All of them start with `vMenu.Enhanced.Menus.`, and `...CharacterCreator.All` grants the lot. They are listed in the `config/permissions.cfg.example` file your server writes on every start.
