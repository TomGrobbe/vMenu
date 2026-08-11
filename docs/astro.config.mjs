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
  // site, so GitHub serves the artifact root at <domain>/vmenu/ already. CI
  // publishes this site into the hub's /vmenu/enhanced/ folder, so the docs
  // land at /vmenu/enhanced/. Do NOT shorten this to '/enhanced'.
  base: '/vmenu/enhanced',
  integrations: [
    starlight({
      title: 'vMenu Enhanced',
      description: 'Documentation for vMenu Enhanced, a version of vMenu for FiveM Enhanced.',
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
        { label: 'Getting Started', link: '/getting-started/' },
        { label: 'Key Bindings', link: '/key-bindings/' },
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
