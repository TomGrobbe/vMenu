---
title: "Menu Appearance"
description: "Pick one of vMenu's seven menu skins, add skins of your own, and change the banner title's font and position."
---

Every menu comes with vMenu's own look: soft dark glass you can see the game through, gently rounded corners, and the pale blue highlight the speedometer and street name display already use. If that does not suit your server, you can hand every menu a different **skin** with one line in your `server.cfg`.

A skin changes the colours, the corners, the shadow, the text font and the picture across the top of every menu. It does not move anything around or change what is in the menus.

## The seven skins

| Value | What it looks like |
| --- | --- |
| `default` | vMenu's own style, matching its on screen displays. Soft dark glass, gently rounded, pale blue highlight. This is what you get if you do nothing. |
| `gold` | The same as `default`, with a warm gold highlight and banner instead of blue. |
| `green` | The same as `default`, with a soft green highlight and banner instead of blue. |
| `red` | The same as `default`, with a soft red highlight and banner instead of blue. |
| `dark` | A solid, almost black panel with a bright blue highlight, matching vMenu's text input box and its ticks overlay. Use it if the glass is too busy over your scenery. |
| `cartoon` | Bright and friendly, the same look as this documentation site. Cream paper rows and big rounded corners, with the title bar, the highlighted row and the description box in near black so they stand out against it. |
| `gta` | The plain Grand Theft Auto V pause menu style, drawn by MenuAPI with no skin at all. This is what older versions of vMenu looked like. |

Pick one with:

```ini
setr vMenu.Enhanced.MenuAppearance.Skin "cartoon"
ensure vMenu.Enhanced
```

The name is not case sensitive. `dark` also answers to `vmenu-dark` and `vmenudark`, `default` also answers to `vmenu`, `gold`, `green` and `red` also answer to their name with `vmenu` or `vmenu-` in front (`vmenugold` for example), and `gta` also answers to `none` and `vanilla`. Type a name vMenu does not know and you get `default`, plus one line in the client console telling you what you typed.

:::note[It changes while you watch]
Once your players are running a build that has this setting, you can change it from the live server console and every menu redraws on the spot, even one that is open at the time. There is nothing to restart.

The very first time is the exception. A brand new setting only starts listening for changes after the client has loaded the build that introduced it, so the first player of the day needs to have joined once with this version.
:::

:::note[The button hints follow the skin too]
The row of button prompts in the bottom right of the screen, the one that tells players which key does what, changes along with the skin. Pick `gta` and it looks exactly like the one the game draws itself.
:::

## More skins than these seven

vMenu's seven skins are not the end of it. Any resource on your server can hand vMenu a theme of its own, a CSS file that stays in that resource, and vMenu then treats it exactly like the seven above. The `Skin` setting accepts its name, and it is offered to your players the same way.

The simplest way to make a custom theme is to use [Custom Themes plugin](https://github.com/TomGrobbe/vMenu.CustomThemesPlugin). 
You drop a stylesheet in a folder, name it in a JSON file and restart the resource, with nothing to build and no code to write. 
Copying a theme into vMenu's own `ui/themes` folder does not work, so don't do that, also every vMenu update overwrites that folder.

If you want your players to choose their own look rather than forcing the server style, the [Theme Picker plugin](https://github.com/TomGrobbe/vMenu.ThemePicker) adds a menu for it.

Writing one of these yourself is a single event, described on the [plugin development page](/vmenu/enhanced/plugins/themes/#adding-themes-of-your-own).

## The banner title

Two more settings control the title written across the picture at the top of each menu. These work with every skin.

| Setting | Default | What it does |
| --- | --- | --- |
| `vMenu.Enhanced.MenuAppearance.TitleAlignment` | `left` | Where the title sits on the banner. Use `left`, `center` or `right`. |
| `vMenu.Enhanced.MenuAppearance.HeaderGlare` | `true` | The soft moving glow that drifts across the banner as the player turns the camera, the same one GTA Online has behind its pause menu title. Set it to `false` for a still banner. |

```ini
setr vMenu.Enhanced.MenuAppearance.Skin "dark"
setr vMenu.Enhanced.MenuAppearance.TitleAlignment "center"
setr vMenu.Enhanced.MenuAppearance.HeaderGlare "false"
```

Like the skin, both apply straight away.
