// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// Google Analytics 4 Measurement ID (loaded on every docs page)
const GA4_ID = 'G-5GXWQVLZ8Z';

// https://astro.build/config
export default defineConfig({
  // Final home of the docs. Only affects canonical/OG metadata + sitemap.
  // Served today at wiki.vespura.com/vmenu/ (testing) and later docs.vespura.com/vmenu/.
  site: 'https://docs.vespura.com',
  // FULL browser path to the docs. The vMenu repo is a GitHub Pages *project*
  // site, so GitHub serves the artifact root at <domain>/vmenu/ already. The
  // CI build strips the leading `vmenu/` that Astro nests output under, so the
  // docs land at /vmenu/legacy/. Do NOT shorten this to '/legacy'.
  base: '/vmenu/legacy',
  integrations: [
    starlight({
      title: 'vMenu (legacy)',
      description: 'Documentation for vMenu — a server-sided trainer/menu for FiveM.',
      favicon: '/favicon.png',
      customCss: ['./src/styles/cartoon.css'],
      // Give Expressive Code an explicit dark + light theme pair. Setting custom
      // themes turns off Starlight's UI theme colors for code blocks, so each
      // block now takes its background from its own theme. That fixes the earlier
      // low contrast issue where dark theme tokens were drawn on a light
      // background. Starlight needs at least one dark and one light theme here.
      expressiveCode: { themes: ['github-dark', 'github-light'] },
      logo: { src: './src/assets/logo.png', alt: 'vMenu', replacesTitle: false },
      // Add an "vMenu Enhanced Docs (coming soon)" link into the header nav.
      components: { SocialIcons: './src/components/SocialIcons.astro' },
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
        { label: 'Installation', link: '/installation/' },
        {
          label: 'Configuration',
          items: [
            { label: 'Configuration Options', link: '/configuration/' },
            { label: 'addons.json', link: '/configuration/addons-json/' },
            { label: 'extras.json', link: '/configuration/extras-json/' },
            { label: 'locations.json', link: '/configuration/locations-json/' },
            { label: 'model-whitelists.json', link: '/configuration/model-whitelists-json/' },
            { label: 'tattoos.json', link: '/configuration/tattoos-json/' },
          ],
        },
        {
          label: 'Permissions',
          items: [
            { label: 'Permissions', link: '/permissions/' },
            { label: 'Permissions Reference', link: '/permissions/permissions/' },
            { label: 'Supplemental Permissions', link: '/permissions/supplemental-permissions/' },
            { label: 'Weapon Permissions', link: '/permissions/weapon-permissions/' },
            { label: 'Default Permissions.cfg', link: '/permissions/default-permissions/' },
          ],
        },
        { label: 'Troubleshooting & Support', link: '/support/' },
        { label: 'F.A.Q.', link: '/faq/' },
        { label: 'Changelog', link: '/changelog/' },
        {
          label: 'Links',
          items: [
            { label: 'Download vMenu', link: 'https://github.com/TomGrobbe/vMenu/releases/latest/', attrs: { target: '_blank' } },
            { label: 'FiveM Forum', link: 'https://forum.fivem.net/t/vMenu/88868?u=vespura', attrs: { target: '_blank' } },
            { label: 'Patreon', link: 'https://www.patreon.com/vespura', attrs: { target: '_blank' } },
            { label: 'vespura.com', link: 'https://vespura.com/', attrs: { target: '_blank' } },
          ],
        },
      ],
    }),
  ],
});
