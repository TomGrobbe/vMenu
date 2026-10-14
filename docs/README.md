# vMenu Enhanced documentation

The vMenu Enhanced docs site. Built with [Astro Starlight](https://starlight.astro.build/)
and a custom theme. Source lives here (on the `main` branch), the built site is
published into the hub repo (`TomGrobbe/TomGrobbe.github.io`) and served at:

| URL                | Content                              | Owned by         |
| ------------------ | ------------------------------------ | ---------------- |
| `/vmenu/`          | Landing page                         | main branch      |
| `/vmenu/legacy/`   | Original vMenu docs, v3.9.2 and below | legacy branch, frozen |
| `/vmenu/enhanced/` | This documentation (Starlight)       | main branch      |

Published site is live at [docs.vespura.com/vmenu](https://docs.vespura.com/vmenu)

## Local development

Requires Node 20.19+ or 22.12+.

```sh
cd docs
npm install
npm run dev      # local preview
npm run build    # production build into ./dist
```

## How it deploys

`.github/workflows/docs.yml` builds the site and pushes the output into the hub
repo under the `vmenu/enhanced/` folder, and copies `landing/index.html` to
`vmenu/index.html`. It runs on:

- pushes to `main` that touch `docs/**`

It never touches `vmenu/legacy/`. Those are the original vMenu's docs, published by the
legacy branch, which no longer gets updates.

GitHub Pages serves a _project_ site at the case-sensitive `/<RepoName>/` path, so
lowercase `/vmenu/` cannot come from this (`vMenu`) repo's own Pages. Instead the
docs are published into the hub site as a lowercase folder, keeping the old docs url the same.

The workflow adds a `.nojekyll` file to the hub automatically so Astro's `_astro/`
asset folder is served correctly.
