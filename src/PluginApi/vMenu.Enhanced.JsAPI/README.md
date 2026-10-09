# @vespura/vmenu-plugin

The TypeScript and JavaScript plugin API for vMenu Enhanced, the version of vMenu that runs on FiveM Enhanced (GTA V Enhanced).

A plugin is a normal FiveM resource of your own. With this package, your resource can add menus to vMenu, under a Plugins entry on its main menu. You describe the menu in code and the package talks to vMenu over events for you. vMenu never needs to know about your resource ahead of time.

Everything the C# plugin API can do is here too. That covers buttons, checkboxes, lists, sliders, dynamic lists, submenus, confirm rows, separators, keys, player actions, translations, permission and setting gates, notifications, text prompts and themes.

```ts
import { Gate, NotifyStyle, Text, VMenuPlugin } from '@vespura/vmenu-plugin/client';

const plugin = VMenuPlugin.create('My Plugin');

plugin.translations.add('en', { greet: 'Say hello' });

const enabled = plugin.settings.bool('Enabled', true, 'Turns my plugin on or off.');

const greet = plugin.rootMenu.addButton(Text.key('greet'), {
  gate: Gate.permission('Greet').and(Gate.setting(enabled)),
});

greet.onSelected(() => plugin.notify(NotifyStyle.Success, 'Hello!'));

await plugin.connect();
```

Your server script declares the permissions and settings through `@vespura/vmenu-plugin/server`:

```ts
import { ServerPluginDeclaration, VMenuServer } from '@vespura/vmenu-plugin/server';

await VMenuServer.register(
  new ServerPluginDeclaration('My Plugin')
    .addPermission('Greet', 'Lets someone use the greet button.')
    .addBoolSetting('Enabled', true, 'Turns my plugin on or off.'),
);
```

The package ships ES modules, so bundle your client and server scripts with a bundler such as esbuild before FiveM loads them. Writing plain JavaScript without a bundler? The vMenu Enhanced release zip also has a single `vmenu.js` file in its `plugin-api` folder, to copy into your resource, with the exact same API on a global `vMenu` object.

The package version always matches the vMenu Enhanced release it belongs to. A full example resource lives in the [vMenu.ExamplePlugin](https://github.com/TomGrobbe/vMenu.ExamplePlugin) repository, and the documentation is at [docs.vespura.com](https://docs.vespura.com/vmenu/enhanced/plugins/typescript/).

## License

This package is licensed under the **GNU General Public License v3.0 or later**, `GPL-3.0-or-later`, the same license vMenu Enhanced itself uses. A plugin built on it is a work based on vMenu, so the license comes along with your plugin. Running it on your own server asks nothing of you. The moment you hand your plugin to somebody else, you owe them the complete source under this same license. The [C# package's readme](https://www.nuget.org/packages/vMenu.Enhanced.ClientAPI) explains this in more detail. This is a plain language summary, not legal advice.
