---
title: "Getting Started"
description: "How to get started with vMenu Enhanced for FiveM Enhanced."
---

:::caution[Coming soon]
vMenu Enhanced is in early alpha. Installation and configuration steps are not finalised yet, so this page is a placeholder for now.
:::

## Requirements

- A FiveM **Enhanced** server.

## Installation

### Filesystem permissions

vMenu Enhanced writes files into its own resource folder (used for creating template files).
FiveM Enhanced blocks resources from writing to disk unless you explicitly grant permission,
so add the following line to your `server.cfg` **before** the line that starts vMenu Enhanced:

```cfg
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

Both names in that command are resource names: the first is the resource being granted access
(vMenu Enhanced), the second is the resource whose folder it may write to (its own).
It's important for this reason (and others) that you call the resource `vMenu.Enhanced`.

:::danger[Order matters]
The permission must be set before the resource starts. If `ensure vMenu.Enhanced` comes first,
vMenu Enhanced will not be able to save anything and you will see filesystem errors in the server
console.
:::

## Where things stand

Setup instructions, configuration, and permissions documentation will land here as vMenu Enhanced becomes usable.
In the meantime, follow along on the [GitHub repository](https://github.com/TomGrobbe/vMenu/) and the [Discord](https://vespura.com/discord).

If you are running the current stable version for FiveM Legacy, use the [vMenu Legacy documentation](/vmenu/legacy/) instead.
