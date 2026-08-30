---
title: "Menu Appearance"
description: "Pick one of vMenu's four menu skins, add skins of your own, and change the banner title's font and position."
---

Every menu comes with vMenu's own look: soft dark glass you can see the game through, gently rounded corners, and the pale blue highlight the speedometer and street name display already use. If that does not suit your server, you can hand every menu a different **skin** with one line in your `server.cfg`.

A skin changes the colours, the corners, the shadow, the text font and the picture across the top of every menu. It does not move anything around or change what is in the menus.

## The four skins

| Value | What it looks like |
| --- | --- |
| `default` | vMenu's own style, matching its on screen displays. Soft dark glass, gently rounded, pale blue highlight. This is what you get if you do nothing. |
| `dark` | A solid, almost black panel with a bright blue highlight, matching vMenu's text input box and its ticks overlay. Use it if the glass is too busy over your scenery. |
| `cartoon` | Bright and friendly, the same look as this documentation site. Cream paper rows and big rounded corners, with the title bar, the highlighted row and the description box in near black so they stand out against it. |
| `gta` | The plain Grand Theft Auto V pause menu style, drawn by MenuAPI with no skin at all. This is what older versions of vMenu looked like. |

Pick one with:

```ini
setr vMenu.Enhanced.MenuAppearance.Skin "cartoon"
ensure vMenu.Enhanced
```

The name is not case sensitive. `dark` also answers to `vmenu-dark` and `vmenudark`, `default` also answers to `vmenu`, and `gta` also answers to `none` and `vanilla`. Type a name vMenu does not know and you get `default`, plus one line in the client console telling you what you typed.

:::note[It changes while you watch]
Once your players are running a build that has this setting, you can change it from the live server console and every menu redraws on the spot, even one that is open at the time. There is nothing to restart.

The very first time is the exception. A brand new setting only starts listening for changes after the client has loaded the build that introduced it, so the first player of the day needs to have joined once with this version.
:::

:::note[Why the cartoon skin has dark patches]
Grand Theft Auto's own text colours, the ones that turn a word yellow or red in a description, are fixed values chosen for a dark menu. Yellow and white simply vanish on cream paper. So the description box and the highlighted row are dark on this skin, which keeps every one of those colours readable while the rows themselves stay bright.
:::

:::caution[The button hints stay as they are]
The row of button prompts along the bottom of the screen is drawn by the game itself, not by vMenu, so no skin can reach it. It keeps the Grand Theft Auto look whichever skin you pick.
:::

## More skins than these four

vMenu's four skins are not the end of it. Any resource on your server can hand vMenu a theme of its own, a CSS file that stays in that resource, and vMenu then treats it exactly like the four above. The `Skin` setting accepts its name, and it is offered to your players the same way.

The simplest way to make a custom theme is to use [Custom Themes plugin](https://github.com/TomGrobbe/vMenu.CustomThemesPlugin). 
You drop a stylesheet in a folder, name it in a JSON file and restart the resource, with nothing to build and no code to write. 
Copying a theme into vMenu's own `ui/themes` folder does not work, so don't do that, also every vMenu update overwrites that folder.

If you want your players to choose their own look rather than forcing the server style, the [Theme Picker plugin](https://github.com/TomGrobbe/vMenu.ThemePicker) adds a menu for it.

Writing one of these yourself is a single event, described on the [plugin development page](/vmenu/enhanced/plugins/developing/#adding-themes-of-your-own).

## The banner title

Three more settings control the title written across the picture at the top of each menu. These work with every skin.

| Setting | Default | What it does |
| --- | --- | --- |
| `vMenu.Enhanced.MenuAppearance.TitleAlignment` | `left` | Where the title sits on the banner. Use `left`, `center` or `right`. |
| `vMenu.Enhanced.MenuAppearance.TitleFont` | `chaletcomprimecologne` | Which font the title is written in. |
| `vMenu.Enhanced.MenuAppearance.HeaderGlare` | `true` | The soft moving glow that drifts across the banner as the player turns the camera, the same one GTA Online has behind its pause menu title. Set it to `false` for a still banner. |

The fonts you can choose from are `chaletlondon`, `housescript`, `monospace`, `chaletcomprimecologne` and `pricedown`, which is the font from the Grand Theft Auto logo. A plain number works too, for a font another resource has added to the game itself.

```ini
setr vMenu.Enhanced.MenuAppearance.Skin "dark"
setr vMenu.Enhanced.MenuAppearance.TitleAlignment "center"
setr vMenu.Enhanced.MenuAppearance.TitleFont "pricedown"
setr vMenu.Enhanced.MenuAppearance.HeaderGlare "false"
```

Like the skin, all three apply straight away.

:::note[Why the title font is separate]
Each font is written at its own size and sits at its own height so it lines up on the banner properly. That is why the font is its own setting instead of something a skin picks: a skin that swapped the font would knock the title out of place.
:::
