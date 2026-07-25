import { defineRouteMiddleware } from '@astrojs/starlight/route-data';

export const onRequest = defineRouteMiddleware((context) => {
  // Point the header logo + "vMenu (legacy)" title at the site root rather than
  // this doc set's own home, so visitors can get back to the index that lists
  // every doc set. Starlight otherwise links it to `base` (/vmenu/legacy/).
  context.locals.starlightRoute.siteTitleHref = '/';
});
