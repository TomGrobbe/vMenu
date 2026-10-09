---
title: "JavaScript setup"
description: "Setting up a vMenu Enhanced plugin in plain JavaScript: the vmenu.js file, the manifest and what differs from C#."
---

The plain JavaScript plugin API is one file, `vmenu.js`, and needs no build step. The rest of the API is on [Making a plugin](/vmenu/enhanced/plugins/developing/). The [JavaScript example](https://github.com/TomGrobbe/vMenu.ExamplePlugin/tree/main/javascript) is a complete resource to copy.

Using a bundler anyway? The npm package from the [TypeScript setup](/vmenu/enhanced/plugins/typescript/) is the same API as ES modules, and works for JavaScript too.

## Getting the file

Every vMenu Enhanced zip on the [releases page](https://github.com/TomGrobbe/vMenu/releases/latest) has a `plugin-api` folder with `vmenu.js` in it. Take the one from the vMenu version your server runs and copy it into your resource. Its first line says which vMenu version it was built for.

Your resource ships its own copy, so it never loads anything from vMenu's folder, and start order does not matter. Replace the file when you update vMenu.

## Manifest

Load it as a shared script, before your own scripts. It gives both halves a global `vMenu` object.

```lua
fx_version 'cerulean'
games { 'gta5' }

shared_script 'vmenu.js'
client_script 'client.js'
server_script 'server.js'
```

Pull what you need off the global at the top of each script:

```js
const { VMenuPlugin, Text, Gate } = vMenu;              // client
const { VMenuServer, ServerPluginDeclaration } = vMenu; // server
```

## Things specific to JavaScript

| Topic | Detail |
| --- | --- |
| Entry points | `VMenuPlugin.create(name)` on the client. `new ServerPluginDeclaration(name)` and `VMenuServer.register(declaration)` on the server. |
| Properties | Plain camelCase properties: `row.description = x`, `row.checked`, `menu.subtitle`. Setting one updates the live menu. |
| Options | Optional arguments go in an object: `menu.addCheckbox('Music', { id: 'music', checked: true, persist: true, description: '...' })`. |
| Events | `row.onSelected(fn)`, `menu.onOpened(fn)` and so on. Each returns a function that unsubscribes. A handler that throws is logged and the others still run. |
| Waiting | `connect`, `getText`, `getTexts` and `VMenuServer.register` return promises, and never reject. |
| Positions | Counted from 0. |
| Text | A plain string is a literal. `Text.key('greet', { name: 'world' })` for a translation key. |
| Gates | `Gate.permission` and `Gate.setting`, combined with `.and()`, `.or()`, `Gate.all` and `Gate.any`. A plain string is a permission gate. |
| Settings | `setting.value` reads the convar. |
| Batches | `plugin.batch(() => { ... })`. The batch ends when the function returns, so changes after an `await` inside it go out on their own. |
| Inserting rows | `menu.insertAt(2, () => { ... })` |
| Notifications | `plugin.notify('success', text)`, with `info`, `success`, `warning` or `error`. |
| Players | A player action hands your handler `{ serverId, name }`. |
