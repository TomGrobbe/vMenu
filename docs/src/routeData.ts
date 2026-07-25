import { defineRouteMiddleware } from '@astrojs/starlight/route-data';

export const onRequest = defineRouteMiddleware((context) => {
  // Point the header logo + title link at the vMenu chooser (one level up from
  // this doc set), so visitors can switch between the different vMenu doc sets
  // rather than jumping all the way to the site root. Base-agnostic: /vmenu/
  // legacy/ -> /vmenu/. Mirrors the "Back to all docs" pill in SocialIcons.astro.
  const chooser = import.meta.env.BASE_URL.replace(/\/(legacy|enhanced|redm)\/?$/, '/');
  context.locals.starlightRoute.siteTitleHref = chooser;
});
