# vMenu documentation

The vMenu docs site. Built with [Astro Starlight](https://starlight.astro.build/)
and a custom theme. Source lives here, the built site is published into
the hub repo (`TomGrobbe/TomGrobbe.github.io`) and served at:

| URL                | Content                          |
| ------------------ | -------------------------------- |
| `/vmenu/`          | Landing page (Legacy + Enhanced) |
| `/vmenu/legacy/`   | This documentation (Starlight)   |
| `/vmenu/enhanced/` | Coming-soon placeholder          |

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
repo under a `vmenu/` folder. It runs on:

- pushes to `master` that touch `docs/**`

GitHub Pages serves a _project_ site at the case-sensitive `/<RepoName>/` path, so
lowercase `/vmenu/` cannot come from this (`vMenu`) repo's own Pages. Instead the
docs are published into the hub site as a lowercase folder, keeping the old docs url the same.

The workflow adds a `.nojekyll` file to the hub automatically so Astro's `_astro/`
asset folder is served correctly.
