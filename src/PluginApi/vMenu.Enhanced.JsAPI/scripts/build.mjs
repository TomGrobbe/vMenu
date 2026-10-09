import { execFileSync } from 'node:child_process';
import { copyFileSync, readFileSync, rmSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { build } from 'esbuild';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const { version } = JSON.parse(readFileSync(join(root, 'package.json'), 'utf8'));

rmSync(join(root, 'dist'), { recursive: true, force: true });

const shared = {
  bundle: true,
  target: 'es2020',
  platform: 'neutral',
  define: { __VMENU_VERSION__: JSON.stringify(version) },
  logLevel: 'warning',
};

await build({
  ...shared,
  entryPoints: { client: join(root, 'src/client/index.ts'), server: join(root, 'src/server/index.ts') },
  format: 'esm',
  outdir: join(root, 'dist'),
});

await build({
  ...shared,
  entryPoints: [join(root, 'src/iife.ts')],
  format: 'iife',
  globalName: 'vMenu',
  outfile: join(root, 'dist/vmenu.js'),
  banner: {
    js: [
      `// vMenu Enhanced plugin API for JavaScript, built for vMenu Enhanced v${version}.`,
      '// Copy this file into your resource and load it before your own scripts, see https://docs.vespura.com/vmenu/enhanced/plugins/javascript/',
    ].join('\n'),
  },
  footer: { js: 'globalThis.vMenu = vMenu;' },
});

execFileSync(process.execPath, [join(root, 'node_modules/typescript/bin/tsc'), '-p', join(root, 'tsconfig.json')], {
  stdio: 'inherit',
});

copyFileSync(join(root, '../../../LICENSE.md'), join(root, 'LICENSE.md'));

console.log(`Built @vespura/vmenu-plugin ${version}.`);
