---
title: "tattoos.json"
---

## About

The tattoos.json file (located in `resources\vMenu\config\`) is used to add your own custom (streamed) tattoos to the MP Character tattoos menu.

vMenu already ships with every known base game tattoo and badge up to 2026 built in, so you do **not** need to list those here. This file is only for overlays that you stream in yourself from your own resource.

The file ships empty:

```json
[]
```

Once you add at least one entry, an extra **Addon Tattoos** list appears in _MP Ped Customization_ > _Tattoos_, alongside the built in Hair, Head, Torso, Left/Right Arm, Left/Right Leg and Badge Overlays lists. If the file is empty, that menu item is hidden entirely.

## Format

Unlike the other config files, this one is a JSON **array** (square brackets), not an object. Each entry describes one tattoo:

```json
[
  {
    "gender": 2,
    "name": "mytattoos_05_A",
    "collectionName": "mytattoos_overlays"
  }
]
```

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `gender` | number | Yes | `0` for male only, `1` for female only, `2` for both. |
| `name` | string | Yes | The name of the overlay itself. |
| `collectionName` | string | Yes | The name of the collection the overlay lives in. |
| `zoneId` | number | No | Accepted, but ignored for addon tattoos. See the note below. |
| `type` | string | No | Accepted, but ignored for addon tattoos. See the note below. |

:::note
`zoneId` and `type` exist because addon tattoos share their format with vMenu's built in tattoo data, but they are **not** used for anything here. Every entry in your tattoos.json ends up in the single flat **Addon Tattoos** list no matter what you put in those two fields, so you may as well leave them out.

Also worth knowing: if you set `gender` to anything other than `0`, `1` or `2`, the tattoo will show up for nobody. And if you leave `gender` out completely it defaults to `0`, so it'll only appear for male characters.
:::

## Example

Here I've added five tattoos from two different collections, with a mix of male only, female only and unisex entries:

```json
[
  {
    "gender": 0,
    "name": "mytattoos_01_M",
    "collectionName": "mytattoos_overlays"
  },
  {
    "gender": 1,
    "name": "mytattoos_01_F",
    "collectionName": "mytattoos_overlays"
  },
  {
    "gender": 2,
    "name": "mytattoos_02_A",
    "collectionName": "mytattoos_overlays"
  },
  {
    "gender": 2,
    "name": "sleeves_fullarm_left",
    "collectionName": "custom_sleeves_overlays"
  },
  {
    "gender": 2,
    "name": "sleeves_fullarm_right",
    "collectionName": "custom_sleeves_overlays"
  }
]
```

## Finding the right names

This is the part people get stuck on, so it's worth spelling out.

When you stream a custom tattoo, your resource contains an overlay definition file (usually named something like `mytattoos_overlays.xml`) with one `<Item>` per tattoo. That file is where both of the names you need come from:

```xml
<Item>
  <uvPos x="0.720000" y="0.670000" />
  <scale x="0.280000" y="0.350000" />
  <rotation value="0.000000" />
  <nameHash>mytattoos_05_A</nameHash>
  <txdHash>mytattoos_05</txdHash>
  <txtHash>mytattoos_05</txtHash>
  <zone>ZONE_RIGHT_ARM</zone>
  <type>TYPE_TATTOO</type>
  <faction>FM</faction>
  <garment>All</garment>
  <gender>GENDER_DONTCARE</gender>
  <award />
  <awardLevel />
</Item>
```

- **`name`** is the `<nameHash>` value, so `mytattoos_05_A` in the example above. Note that this is _not_ the same as `<txdHash>`/`<txtHash>` (the texture dictionary), which is a common mix up.
- **`collectionName`** is the collection that overlay file declares. By convention this matches the overlay file's own name without the extension, so `mytattoos_overlays.xml` gives you a collection called `mytattoos_overlays`.
- **`gender`** maps from the `<gender>` field: `GENDER_MALE` (or a `_M` suffixed name) becomes `0`, `GENDER_FEMALE` (or `_F`) becomes `1`, and `GENDER_DONTCARE` becomes `2`.

Those two names are exactly the pair you'd pass to the [AddPedDecorationFromHashes][add-ped-decoration] native, which is what vMenu does under the hood:

```lua
AddPedDecorationFromHashes(ped, GetHashKey("mytattoos_overlays"), GetHashKey("mytattoos_05_A"))
```

:::tip
If you can apply your tattoo with that native but it won't show in vMenu, your `collectionName`/`name` pair is correct and the problem is somewhere in your tattoos.json. If the native doesn't work either, the problem is in your streamed resource, not in vMenu.
:::

## Creating your own tattoos

Everything above assumes you already have a working tattoo resource and just need to point vMenu at it. Making the assets themselves is a separate job that vMenu isn't involved in at all, so it isn't covered here.

## Hair tattoos and badges

vMenu sorts its **built in** overlays into the separate menu lists automatically:

- Any overlay whose name contains `hair_` goes into **Hair Tattoos**. This is why hair overlays no longer get mixed in with the head tattoos.
- Any overlay of type `TYPE_BADGE` goes into **Badge Overlays**.
- Everything else is sorted by zone into the Head, Torso, Arm and Leg lists.

That sorting only applies to the built in data. Your addon entries always land in the **Addon Tattoos** list, regardless of whether they're hair overlays, badges or regular tattoos.

:::note
Badges need a shirt to render. If you (or your players) select a badge while the character has a bare torso, nothing will appear. That's a game limitation, not a vMenu one.
:::

## Errors

:::caution
Entries are considered duplicates when both the collection name **and** the tattoo name match. The same name in two different collections is fine. If you do have a real duplicate, you'll see this in the client console and the second entry is dropped:

```
[vMenu] [Error] Your tattoos.json file contains 2 or more entries with the same collection and tattoo names! (mytattoos_overlays & mytattoos_05_A) Please remove duplicate lines!
```

If the file isn't valid JSON, you'll get this:

```
[vMenu] [ERROR] Your tattoos.json file contains a problem! Error details: ...
```

Unlike the other config files, tattoos.json is **not** checked on the server, so this one only ever shows up in the **client** console (F8). If you're wondering why your tattoos aren't loading and the server console looks clean, check there.
:::

## Default tattoos.json

```json
[]
```

## Appreciate my work?

Consider supporting me on [Patreon](https://www.patreon.com/vespura)!

[add-ped-decoration]: https://docs.fivem.net/natives/?_0x5F5D1665E352A839=
