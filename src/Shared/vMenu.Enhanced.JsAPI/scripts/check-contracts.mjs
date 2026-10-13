import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const contracts = join(root, '../vMenu.Enhanced.PluginContracts');
const typescript = readFileSync(join(root, 'src/protocol.ts'), 'utf8');
const lua = readFileSync(join(root, '../vMenu.Enhanced.LuaAPI/vmenu.lua'), 'utf8');
const failures = [];

const read = (file) => readFileSync(join(contracts, file), 'utf8');

function constants(file) {
  return [...read(file).matchAll(/public const string (\w+) = "([^"]*)";/g)].map(([, name, value]) => ({ name, value }));
}

function expect(source, label, text, what) {
  if (!source.includes(text)) {
    failures.push(`${label} is missing ${what}: ${text}`);
  }
}

for (const file of ['UpdateOps.cs', 'CallbackTypes.cs', 'EntryTypes.cs', 'NodeEvents.cs', 'SettingNode.cs']) {
  for (const { name, value } of constants(file)) {
    expect(typescript, 'protocol.ts', `'${value}'`, `${file} ${name}`);
    expect(lua, 'vmenu.lua', `'${value}'`, `${file} ${name}`);
  }
}

const events = read('PluginEvents.cs');
const prefix = /private const string Prefix = "([^"]+)";/.exec(events)[1];

for (const [, name, suffix] of events.matchAll(/public const string (\w+) = Prefix \+ "([^"]+)";/g)) {
  expect(typescript, 'protocol.ts', `'${prefix}${suffix}'`, `PluginEvents ${name}`);
  expect(lua, 'vmenu.lua', `'${prefix}${suffix}'`, `PluginEvents ${name}`);
}

for (const [, name, suffix] of events.matchAll(/public static string (\w+)\(string resource\) => \$"\{Prefix\}:\{resource\}([^"]+)";/g)) {
  expect(typescript, 'protocol.ts', `\`${prefix}:\${resource}${suffix}\``, `PluginEvents ${name}`);
  expect(lua, 'vmenu.lua', `'${prefix}:' .. resource .. '${suffix}'`, `PluginEvents ${name}`);
}

const protocol = read('PluginProtocol.cs');
const version = /public const int Version = (\d+);/.exec(protocol)[1];
const serverVersion = /public const int ServerVersion = (\d+);/.exec(protocol)[1];
const resource = /public const string VMenuResource = "([^"]+)";/.exec(protocol)[1];

expect(typescript, 'protocol.ts', `PROTOCOL_VERSION = ${version};`, 'the client protocol version');
expect(typescript, 'protocol.ts', `SERVER_PROTOCOL_VERSION = ${serverVersion};`, 'the server protocol version');
expect(typescript, 'protocol.ts', `VMENU_RESOURCE = '${resource}';`, 'the vMenu resource name');
expect(lua, 'vmenu.lua', `PROTOCOL_VERSION = ${version}`, 'the client protocol version');
expect(lua, 'vmenu.lua', `SERVER_PROTOCOL_VERSION = ${serverVersion}`, 'the server protocol version');
expect(lua, 'vmenu.lua', `VMENU_RESOURCE = '${resource}'`, 'the vMenu resource name');

const refresh = /RefreshPermissionsEvent = "([^"]+)";/.exec(readFileSync(join(root, '../vMenu.Enhanced.ServerAPI/VMenuServer.cs'), 'utf8'))[1];

expect(typescript, 'protocol.ts', `'${refresh}'`, 'the permission refresh event');
expect(lua, 'vmenu.lua', `'${refresh}'`, 'the permission refresh event');

if (failures.length > 0) {
  console.error('The JS or Lua plugin API is out of step with PluginContracts:');

  for (const failure of failures) {
    console.error(`  ${failure}`);
  }

  process.exit(1);
}

console.log('The JS and Lua plugin APIs match PluginContracts.');
