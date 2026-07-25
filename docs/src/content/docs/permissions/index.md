---
title: "Permissions"
---

-------

## About the permissions system
vMenu uses the default Aces & Principals system available in FiveM. I've made a full guide about what they are, and how resource developers can use them ([that guide can be found here](https://forum.fivem.net/t/basic-aces-principals-overview-guide/90917?u=vespura)). Although you don't need to know evertyhing inside that guide to setup vMenu permissions, I still recommend looking through that guide if you want to learn more advanced tricks about using and setting up aces & principals for vMenu.


-------------

## The basics
**Here you'll find the short version of what you need to understand before you can start creating your permissions:**

1. **Permissions are called Aces**
<br>Aces (`vMenu.PlayerOptions.Menu`, for example) are what you want to _give_ to your players. Having such Ace/permission, you'll be able to access different parts of the menu and perform different actions.
2. **Groups are called Princials**
<br>Giving permissions to every single player would be a waste of time and just completely useless. That's where Principals (groups) come in to play. You can give permissions to a specific Principal, and add Players to a principal. You can also add principals to other principals, allowing a inheritance-like permissions system to improve your setup. <br>You can create as many groups/Principals as you want, and you can also name them whatever you want.

--------

## Important info when setting up permissions
The default `permissions.cfg` file provided in the resource download, will have all the information you need to get started, but we'll explain some things here as well.

- First of all, all changes you make to the `permissions.cfg` file will require a **server restart**. Simply restarting the resource (which is **NOT RECOMMENDED** anyway) will not update the changes you've made to the permissions. This is because the permissions file only gets executed when the server is (re)started.
- Another very common misconception: "vMenu doesn't read the permissions file, my permissions don't work, I need help!" vMenu does **NOT** read the permissions.cfg file. You execute the permissions.cfg file yourself by adding `exec <pathToPermissionsFile>/permissions.cfg` to your server.cfg. The commands inside the permissions.cfg will then be executed by the server. THEY ARE NOT EXECUTED BY vMenu!!! vMenu simply 'asks' the server "Is this player allowed to do `<insert permission name here>`, and the server will then return true or false. vMenu does not know _at all_ about the permissions.cfg itself.
- Finally, make sure to always edit the correct `permissions.cfg`. If you have the permissions.cfg file located inside the `/resources/vMenu/config/` folder, and your execute command (in the server.cfg) looks like this: `exec resources/vMenu/config/permissions.cfg` then always edit the permissions.cfg that's located in the config folder! **If** you have the permissions.cfg in the same folder where the server.cfg is located, then edit the permissions.cfg inside that same folder where the server.cfg is. In both cases, editing the wrong file will result in no changes _at all_ when you restart the server.
- If you're going to be changing the permisisons, make sure to stop the server. Especially when using zap-hosting. Then once you're done editing the permissions.cfg, start the server back up.
- Permissions that are granted to the `builtin.everyone` group, are added to _all_ players. There's no way to restrict this. `builtin.everyone` really means "everyone", so admins, moderators, and everyone else is included!!!
- Want to create a new group? It's easy, just add players to it or assign permissions to it and you're good to go. You can call groups whatever you like. For example: `group.superadmin` or `group.owner` or `group.snail` or `group.vip`.

-----------

## Setting up permissions (very basic)


:::caution

**Very important**<br><br>**DO NOT SET ANY PERMISSION TO `deny`!!!! If you don't want someone to have a specific permission, then just COMMENT OUT (put a `#` in front of the line) a permission. Alternatively, just remove the entire line, however I'd recommend just commenting it out so you don't have to look on the docs to find it again if you want to use it later.**

:::




### Creating a group and adding players

To create a Principal/Group and add a player to it, add this somewhere at the top of your `permissions.cfg` file, in this example we'll use the steam identifier:
```yaml
add_principal identifier.steam:<steam_id_here> group.<groupName>
```

For example, if I wanted to add myself to the "admin" group, I'd do it like this:
```yaml
add_principal identifier.steam:110000101234567 group.admin
```

---------

### Giving a group some permissions

Then if I wanted to give myself access to the full "Online Players" menu, I would give myself the ace for it like this:
```yaml
add_ace group.admin "vMenu.OnlinePlayers.Menu" allow
add_ace group.admin "vMenu.OnlinePlayers.All" allow
```

------

### Summary

Summary: My permissions.cfg file would now look like this:
```yaml
add_principal identifier.steam:110000101234567 group.admin
add_ace group.admin "vMenu.OnlinePlayers.Menu" allow
add_ace group.admin "vMenu.OnlinePlayers.All" allow
```

-----------

### Denying certain permissions

If I wanted to allow everything in the Online Players menu EXCEPT for the teleporting option, then I'd have to add all _allowed_ permissions manually, and LEAVE OUT the ones I don't want.
There's no way to allow "everything (.All)" and then "deny" certain permissions.
Example:
```yaml
# Add players to group
add_principal identifier.steam:110000101234567 group.admin


# Allow all permissions EXCEPT for the teleport to player permission
add_ace group.admin "vMenu.OnlinePlayers.Menu" allow
#add_ace group.admin "vMenu.OnlinePlayers.All" allow
#add_ace group.admin "vMenu.OnlinePlayers.Teleport" allow
add_ace group.admin "vMenu.OnlinePlayers.Waypoint" allow
add_ace group.admin "vMenu.OnlinePlayers.Spectate" allow
add_ace group.admin "vMenu.OnlinePlayers.Summon" allow
add_ace group.admin "vMenu.OnlinePlayers.Kill" allow
add_ace group.admin "vMenu.OnlinePlayers.Kick" allow
add_ace group.admin "vMenu.OnlinePlayers.TempBan" allow
add_ace group.admin "vMenu.OnlinePlayers.PermBan" allow
add_ace group.admin "vMenu.OnlinePlayers.Unban" allow
```

See how the `.Teleport` line is commented out? That will `deny` that option in the menu. Note that you also MUST comment out the `.All` permission for a (sub)menu if you want to deny a permission. `.All` permissions will always override every other option inside the menu to `allow`.



:::caution

**Very important**<br><br>**DO NOT SET ANY PERMISSION TO `deny`!!!! If you don't want someone to have a specific permission, then just COMMENT OUT (put a `#` in front of the line) a permission. Alternatively, just remove the entire line, however I'd recommend just commenting it out so you don't have to look on the docs to find it again if you want to use it later.**

:::




----------

### Setting up group inheritance

It's a waste of time to manually add all permissions to every single group. There's a much better and faster way to go about it; using group inheritance.

The example in the default permissions.cfg file looks like this:
```yaml
#############################################
#        SETTING UP GROUP INHERITANCE       #
#############################################
## Setup group inheritance, it's probably best you don't touch this unless you know what you're doing.
add_principal group.admin group.moderator
```

**Setting up inheritance is definitely not hard, you just have to use your brain and some common sense.**

----

### #1 MOST COMMON 'noob' mistake when it comes to setting up inheritance

Most (new) users that setup vMenu ask why their inheritance isn't working, and they send me this:

```yaml
add_principal group.superadmin group.admin group.mod group.leo group.whatever
```

This is _very_ wrong.

I need to stress this again at this point. vMenu does _not_ read the permissions.cfg file. Each line inside the permissions.cfg, will be executed by the server as a **command**.
The `add_principal` command takes 2 arguments, not 3, not 4, not 5. No, only 2. A `parent` and a `child` argument. That's 2. Not 1, not 3, not 4. ONLY **2**! Got it? Good!

The syntax is like this:

```yaml
add_principal parent.principal child.principal
```

An easy way to remember this is: I want to give `<parent>` access to everything `<child>` has, but not the other way around.


So, in our first example:

```yaml
add_principal group.admin group.moderator
```

And using the 'easy way to remember the syntax':

> _I want to give `group.admin` access to everything `group.moderator` has, but not the other way around._


-----

So, now that you know that, let's go back to the (bad) version that people often send me. Doing it correctly, it **should** look like this:

```yaml
# I want to give group.superadmin everything from group.admin
add_principal group.superadmin group.admin
# I want to give group.admin everything from group.mod
add_principal group.admin group.mod
# I want to give group.mod everything from group.leo
add_principal group.mod group.leo
# I want to give group.leo everything from group.whatever
add_principal group.leo group.whatever
```

----


:::note

Note, inheritance works recursive. In the example above, `group.superadmin` gets _all_ permissions of all the other groups. `group.admin` gets all permissions of `group.mod` _and all groups below that_.

:::




That's all there is to it. You now know everything you need to know to setup vMenu's permissions. If you have any questions, be sure to read the [FAQ page](/vmenu/legacy/faq) first, then read this page again, then read [this topic](https://forum.fivem.net/t/basic-aces-principals-overview-guide/90917?u=vespura), then ask on the [vMenu topic](https://vespura.com/vmenu) (note that there is only community support on the forum topic because vMenu is no longer supported by myself). You could also check the `#vmenu-help-archived` channel in my [Discord server](https://vespura.com/discord), don't ask for any support in my Discord though because you'll get permanently muted for your inability to read.

----

## Appreciate my work?
Consider supporting me on [Patreon](https://www.patreon.com/vespura)!
