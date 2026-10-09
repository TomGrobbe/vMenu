---
title: "TypeScript setup"
description: "Setting up a vMenu Enhanced plugin in TypeScript: the npm package, bundling and the manifest."
---

The TypeScript plugin API is the npm package `@vespura/vmenu-plugin`, with its types included. The rest of the API is on [Making a plugin](/vmenu/enhanced/plugins/developing/). The [TypeScript example](https://github.com/TomGrobbe/vMenu.ExamplePlugin/tree/main/typescript) is a complete project to copy.

## Installing the package

```
npm install @vespura/vmenu-plugin@<your vMenu version> --save-exact
```

The package version always matches the vMenu Enhanced release it belongs to. Pin it to the vMenu your server runs, and raise it when you update vMenu.

| Import from                       | Used by            |
| --------------------------------- | ------------------ |
| `@vespura/vmenu-plugin/client`    | your client script |
| `@vespura/vmenu-plugin/server`    | your server script |

## Bundling

The package ships ES modules, and FiveM loads plain scripts. Bundle each half into one file with esbuild, or any bundler you like:

```js
import { build } from 'esbuild';

const shared = { bundle: true, format: 'iife', target: 'es2020' };

await build({ ...shared, entryPoints: ['src/client.ts'], platform: 'browser', outfile: 'dist/client.js' });
await build({ ...shared, entryPoints: ['src/server.ts'], platform: 'node', outfile: 'dist/server.js' });
```

The plugin API ends up inside both files, so the resource needs nothing else at runtime, and start order does not matter. Since the output is a plain script, use `.then()` at the top level rather than a top level `await`.

## Manifest

```lua
fx_version 'cerulean'
games { 'gta5' }

client_script 'dist/client.js'
server_script 'dist/server.js'
```

## Things specific to TypeScript

Everything on the [JavaScript setup](/vmenu/enhanced/plugins/javascript/) page applies, with imports in place of the global `vMenu` object:

```ts
import { Gate, Text, VMenuPlugin } from '@vespura/vmenu-plugin/client';
import { ServerPluginDeclaration, VMenuServer } from '@vespura/vmenu-plugin/server';
```

Every row type has its own class, such as `PluginCheckbox` or `PluginList`, so `instanceof` narrows a row you got back from a key press or a filter. The options for each `add` method are typed too, so your editor tells you which ones a row takes.
