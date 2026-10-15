// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import { searchForWorkspaceRoot } from 'vite';

// Google Analytics 4 Measurement ID (loaded on every docs page)
const GA4_ID = 'G-5GXWQVLZ8Z';

// https://astro.build/config
export default defineConfig({
  // Final home of the docs. Only affects canonical/OG metadata + sitemap.
  // Served today at wiki.vespura.com/vmenu/ (testing) and later docs.vespura.com/vmenu/.
  site: 'https://docs.vespura.com',
  // FULL browser path to the docs. The vMenu repo is a GitHub Pages *project*
  // site, so GitHub serves the artifact root at <domain>/vmenu/ already. CI
  // publishes this site into the hub's /vmenu/enhanced/ folder, so the docs
  // land at /vmenu/enhanced/. Do NOT shorten this to '/enhanced'.
  base: '/vmenu/enhanced',
  vite: { server: { fs: { allow: [searchForWorkspaceRoot(process.cwd()), '../assets/shared/config'] } } },
  redirects: {
    '/moving-your-data/': '/vmenu/enhanced/backing-up-your-data/',
  },
  integrations: [
    starlight({
      title: 'vMenu Enhanced',
      description: 'Documentation for vMenu Enhanced, the new vMenu for FiveM Enhanced and FiveM Legacy.',
      favicon: '/favicon.png',
      customCss: ['./src/styles/cartoon.css'],
      // Give Expressive Code an explicit dark + light theme pair. Setting custom
      // themes turns off Starlight's UI theme colors for code blocks, so each
      // block now takes its background from its own theme. That fixes the earlier
      // low contrast issue where dark theme tokens were drawn on a light
      // background. Starlight needs at least one dark and one light theme here.
      expressiveCode: { themes: ['github-dark', 'github-light'] },
      logo: { src: './src/assets/logo.png', alt: 'vMenu Enhanced', replacesTitle: false },
      // Prepend a "Back to all docs" pill to the header nav (see SocialIcons.astro),
      // linking back up to the /vmenu/ chooser.
      components: {
        SocialIcons: './src/components/SocialIcons.astro',
        // Default the docs to dark mode; the toggle still saves the choice to
        // localStorage. Both files fall back to dark when nothing is stored.
        ThemeProvider: './src/components/ThemeProvider.astro',
        ThemeSelect: './src/components/ThemeSelect.astro',
      },
      // Retarget the header logo + title link at the site root (see src/routeData.ts).
      routeMiddleware: './src/routeData.ts',
      head: [
        { tag: 'link', attrs: { rel: 'preconnect', href: 'https://fonts.googleapis.com' } },
        { tag: 'link', attrs: { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: true } },
        {
          tag: 'link',
          attrs: {
            rel: 'stylesheet',
            href: 'https://fonts.googleapis.com/css2?family=Fredoka:wght@400;500;600;700&display=swap',
          },
        },
        // Google Analytics 4
        {
          tag: 'script',
          attrs: { async: true, src: `https://www.googletagmanager.com/gtag/js?id=${GA4_ID}` },
        },
        {
          tag: 'script',
          content: `window.dataLayer=window.dataLayer||[];function gtag(){dataLayer.push(arguments);}gtag('js',new Date());gtag('config','${GA4_ID}');`,
        },
      ],
      social: [
        { icon: 'github', label: 'GitHub', href: 'https://github.com/TomGrobbe/vMenu' },
        { icon: 'discord', label: 'Discord', href: 'https://vespura.com/discord' },
      ],
      sidebar: [
        { label: 'Home', link: '/' },
        {
          label: 'Getting Started',
          items: [
            { label: 'Where to begin', link: '/getting-started/' },
            { label: 'Installing on FiveM Enhanced', link: '/installing/fivem-enhanced/' },
            { label: 'Installing on FiveM Legacy', link: '/installing/fivem-legacy/' },
            { label: 'Running on Linux', link: '/running-on-linux/' },
          ],
        },
        { label: 'Key Bindings', link: '/key-bindings/' },
        { label: 'Menu Appearance', link: '/menu-appearance/' },
        {
          label: 'Saved Data',
          items: [
            { label: 'Backing Up Your Data', link: '/backing-up-your-data/' },
            { label: 'Edit Your Data', link: '/data-editor/' },
            { label: 'Compare Or Merge Data', link: '/data-compare/' },
            { label: 'Import Saved Data From Old vMenu', link: '/old-vmenu-import/' },
          ],
        },
        { label: 'Custom Character Creator', link: '/character-creator/' },
        { label: 'Admin Menu', link: '/admin-menu/' },
        { label: 'Bully Menu', link: '/bully-menu/' },
        { label: 'Webhook Logging', link: '/webhook-logging/' },
        { label: 'Exports', link: '/exports/' },
        {
          label: 'Integrations',
          items: [
            { label: 'SnowstormBot (Discord)', link: '/integrations/' },
            { label: 'Custom integrations', link: '/integrations/custom/' },
          ],
        },
        {
          label: 'Configuration',
          items: [
            { label: 'blips.json', link: '/configuration/blips-json/' },
            { label: 'extras.json', link: '/configuration/extras-json/' },
            { label: 'vehicle-categories.json', link: '/configuration/vehicle-categories-json/' },
            { label: 'weather-cycle.json', link: '/configuration/weather-cycle-json/' },
            { label: 'World API', link: '/configuration/world-api/' },
          ],
        },
        {
          label: 'Plugins',
          items: [
            { label: 'What plugins are', link: '/plugins/' },
            { label: 'Installing plugins', link: '/plugins/installing/' },
            {
              label: 'Making a plugin',
              items: [
                { label: 'Getting started', link: '/plugins/developing/' },
                { label: 'Menus', link: '/plugins/menus/' },
                { label: 'Standalone menus', link: '/plugins/standalone-menus/' },
                { label: 'Text and messages', link: '/plugins/text/' },
                { label: 'Permissions and settings', link: '/plugins/permissions/' },
                { label: 'Themes', link: '/plugins/themes/' },
              ],
            },
            { label: 'C# setup', link: '/plugins/csharp/' },
            { label: 'Lua setup', link: '/plugins/lua/' },
            { label: 'JavaScript setup', link: '/plugins/javascript/' },
            { label: 'TypeScript setup', link: '/plugins/typescript/' },
          ],
        },
        {
          label: 'Links',
          items: [
            { label: 'vMenu Legacy docs', link: 'https://docs.vespura.com/vmenu/legacy/', attrs: { target: '_blank' } },
            { label: 'Releases', link: 'https://github.com/TomGrobbe/vMenu/releases/', attrs: { target: '_blank' } },
            { label: 'Patreon', link: 'https://www.patreon.com/vespura', attrs: { target: '_blank' } },
            { label: 'vespura.com', link: 'https://vespura.com/', attrs: { target: '_blank' } },
          ],
        },
      ],
    }),
  ],
});
