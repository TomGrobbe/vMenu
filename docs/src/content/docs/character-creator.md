---
title: "Custom Character Creator"
description: "Building a freemode online character in vMenu Enhanced, and the two files a server owner can use to add tattoos and ready made outfits."
---

## What it is

The Custom Character Creator is where a player builds one of the two "freemode" characters, the same
kind of character GTA Online makes you build the first time you play it. You pick two parents, mix
their faces together, push twenty sliders around until the face is yours, then add hair, makeup,
tattoos and clothes.

You will find it under **Player Related Options, Custom Character Creator**, right below Player
Appearance. Those two menus do different jobs. Player Appearance changes the clothes on whatever ped
you happen to be wearing. The Custom Character Creator makes the person underneath the clothes.

## For players

### Making one

Pick **Create Male Character** or **Create Female Character** and the creator opens. You cannot do
this from inside a vehicle, because the creator freezes you in place and points a camera at you.

From there the pages are:

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

While the creator is open the camera follows whichever row you are on, so scrolling onto the shoes
row shows you the shoes. From there the camera is yours to move, and it stays where you put it while
you keep working on the same page. Opening a different page hands it back to that page's own angle.

| What you want | Keyboard and mouse | Controller |
| --- | --- | --- |
| Look at the character from another angle | Move the mouse | Right stick |
| Swing around the character | `A` and `D` | Left stick left and right |
| Zoom in and out | `W` and `S` | Right and left trigger |
| Raise and lower the camera | `E` and `Q` | Left stick up and down |
| Put the camera back where it started | `C` | Right stick click |
| Turn the whole character around | `Space` | `X` on Xbox, square on PlayStation |

The character looks back at the camera on their own, so you always get the face while you are moving
around. Walk the camera far enough round the back and they face front again, because a neck only
turns so far. The buttons along the bottom of the screen cover the rest.

If you would rather the camera never moved itself, there is a **Disable Auto Camera** switch at the
very top of the creator. Turn it on and the camera stops jumping to the shoes, the face or whatever
else the row you are on is about, and simply stays where you left it. `N` on the keyboard, or the
right bumper on a controller, does the same thing without opening the menu page, so you can flip it
while you are lining a shot up. Like every key in vMenu Enhanced you can rebind that one, and the
choice is remembered between sessions.

Backing out is deliberately blocked while you are in there. Save the character, or use **Exit Without
Saving** if you have changed your mind. That one asks you to press it twice.

### Matching the arms to your clothes

A freemode character's arms are not part of the top you are wearing. The game keeps them in a slot of
their own, called **Hands / Upper Body**, and that slot is what decides whether you get bare arms,
rolled up sleeves, a leather jacket sleeve or a pair of gloves. So picking a leather jacket does not
on its own give you leather sleeves. Something has to pick the arms for you.

GTA Online does that automatically every time you change clothes in a shop, and the creator now does
the same. Scroll onto a different top, undershirt or pair of trousers and the arms change to suit it.
Gloves survive the change too, so putting a new jacket on no longer leaves you bare handed.

If you would rather choose the arms yourself, there is a **Match Arms To Clothes** switch at the top
of the **Character Clothes** page. Turn it off and nothing touches the Hands / Upper Body row but you.
It is remembered between sessions.

Two things worth knowing:

- Changing the Hands / Upper Body row yourself never gets overridden. Your choice stays until the next
  time you change a top, so you can always overrule the automatic pick.
- The one thing it does not carry over is the pair of gloves that goes with the flapper dress. Those
  come off when you change tops. Every other pair the game knows about is kept.

This only happens inside the Custom Character Creator. **Player Appearance** is unchanged and stays
completely manual, because that menu is for dressing any ped however you like.

### Saving one you did not make here

If something else on the server dressed you as a freemode character, **Save Current Character** takes
a full copy of it: the face, the skin layers, the hair, the tattoos and the clothes. vMenu Legacy
could not do this and said so in its own menu. It can now.

### Outfits and hair styles

A saved character is three separate things, and this is the part that is new.

The **character** is the person: the face, the skin tone, the tattoos. There is one of those.

An **outfit** is a set of clothes, and a **style** is a haircut with its beard and makeup. You can
save as many of each as you like against one character and switch between them, so a character can
have a work outfit and a going out outfit without being saved twice.

Both live under **Saved Characters**, pick a character, then **Outfits** or **Hair & Makeup Styles**.
Wear something you like, pick **Save Current As New**, and give it a name.

Alongside those lists every character quietly keeps the clothes and hair it is wearing right now, and
that one is not part of the lists. Editing a character, or putting a saved outfit on, changes only
that. So a saved outfit is only ever written to when you ask for it by name, with **Save Current As
New** or **Replace**, and going into the creator can never quietly rewrite one. That also means you
are free to delete every saved outfit a character has. It carries on wearing what it had on.

### Coming back as yourself

**Set As Default Character** on any saved character means you turn into them whenever you respawn, and
when you first join. Pick it again to turn it off.

### Your saves travel with you

Characters are saved on your own computer, the same as saved vehicles and saved peds, and they are
included in the transfer code described on the [Moving Your Data](/vmenu/enhanced/moving-your-data/)
page. Make one before you rely on them.

## For server owners

There are two files, both in `resources/vMenu.Enhanced/config/`.

### Tattoos

`config/tattoos.json` is the list of every tattoo and badge the creator offers. It ships filled in
with every base game tattoo up to 2026, so you only need to open it if you stream in tattoos of your
own.

Unlike every other config file, this one is read by the **player's** game rather than by the server.
That means two things. A player who is already connected will not see your changes until they
reconnect, and the file is downloaded by everybody who joins, so it is worth keeping tidy.

The shape is one object with a version and a list:

```json
{
  "version": 1,
  "tattoos": [
    { "collection": "mytattoos_overlays", "name": "mytattoos_01_M", "zone": "torso", "gender": "male" },
    { "collection": "mytattoos_overlays", "name": "mytattoos_02_A", "zone": "addon", "gender": "both", "label": "Anchor" }
  ]
}
```

| Field | Required | What it means |
| --- | --- | --- |
| `collection` | Yes | The name of the overlay collection the tattoo lives in |
| `name` | Yes | The name of the overlay itself |
| `zone` | Yes | Which list it shows up in, see below |
| `gender` | No | `male`, `female`, or anything else for both. Defaults to both |
| `label` | No | What to call it on screen. Without one it is numbered, the same as every base game tattoo |

`zone` is one of `hair`, `head`, `torso`, `leftArm`, `rightArm`, `leftLeg`, `rightLeg`, `badge` or
`addon`. The first seven are the parts of the body, `badge` is the badge overlay list, and `addon` is
a list of its own that only appears when you have put something in it. If you are adding tattoos of
your own, `addon` keeps them together and away from the base game ones, but nothing stops you putting
them in a body zone instead if that suits you better.

:::note
Two commands help when a tattoo is not showing up. `vmenu_character_tattoos` lists every decoration on
the ped you are wearing and says whether the file knows what each one is, and `vmenu_character` prints
the whole character. Both only report while the debugging convar is on.
:::

### Ready made outfits

`config/clothing-presets.json` holds outfits you publish for everybody on the server, such as a set of
police uniforms. It ships empty:

```json
[]
```

You do not have to write this one by hand. Give somebody the
`vMenu.Enhanced.Menus.CharacterCreator.PresetsManage` permission and they can build an outfit in game,
then use **Outfit Presets, Server Presets, Save Clothes To Server Presets**. vMenu writes the file and
sends the new list to everybody who is already connected, so there is nothing to restart.

If you would rather edit it directly, it looks like this:

```json
[
  {
    "name": "Police",
    "description": "Department outfits",
    "presets": [
      {
        "name": "Patrol Officer",
        "description": "Standard patrol",
        "gender": "male",
        "components": [
          { "slot": 11, "collection": "", "localDrawable": 55, "drawable": 55, "texture": 0, "palette": 0 }
        ],
        "props": []
      }
    ]
  }
]
```

`gender` matters. A drawable number means a different piece of clothing on the male model and the
female one, so an outfit saved on one is hidden on the other rather than applied wrongly. Leave it out
and the outfit shows up for both.

:::caution
Writing this file needs `add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced` in your
server.cfg, the same line the permissions example file needs. Step 3 of
[Getting Started](/vmenu/enhanced/getting-started/) covers it.
:::

### Outfits that came with the game

Alongside your own presets there is a **GTA Online Outfits** page. Nothing is configured for that one.
It reads the outfits Rockstar shipped with each content pack straight out of the game and groups them
by the pack they came from. It is read only, and it has its own permission so you can hide it if you
would rather players only used yours.

### Clothes that survive an update

Every outfit vMenu saves now records which content pack each piece of clothing came from, as well as
its number. That matters because the numbers move. Install one new clothing pack and every piece that
sorts after it shifts up, so an outfit saved last week comes back as somebody else's shirt.

Recording the pack means the shirt is found again by name and number within that pack, which does not
move. If a pack is genuinely gone from your server, that slot goes back to the model default and vMenu
says how many pieces it could not find, rather than dressing the player in whatever now happens to sit
at that number.

This applies to saved peds as well, not only to characters. Saves written by an older vMenu keep
working exactly as they did, and pick up the extra information the next time they are saved over.

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

All of them start with `vMenu.Enhanced.Menus.`, and `...CharacterCreator.All` grants the lot. They are
listed in the `config/permissions.cfg.example` file your server writes on every start, so there is
nothing to copy from here.
