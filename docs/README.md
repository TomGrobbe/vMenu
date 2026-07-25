# vMenu documentation

The vMenu docs site — built with [Astro Starlight](https://starlight.astro.build/)
and a custom "cartoon" theme. Source lives here; the built site is published into
the hub repo (`TomGrobbe/TomGrobbe.github.io`) and served at:

| URL | Content |
| --- | --- |
| `/vmenu/` | Landing page (Legacy + Enhanced) |
| `/vmenu/legacy/` | This documentation (Starlight) |
| `/vmenu/enhanced/` | Coming-soon placeholder |

(Today under `wiki.vespura.com`, later `docs.vespura.com`.)

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
repo under a lowercase `vmenu/` folder. It runs on:

- pushes to `master` that touch `docs/**`, and
- the manual **Run workflow** button (Actions → *Deploy Docs*).

GitHub Pages serves a *project* site at the case-sensitive `/<RepoName>/` path, so
lowercase `/vmenu/` cannot come from this (`vMenu`) repo's own Pages. Instead the
docs are published into the hub site as a real lowercase folder.

## One-time setup

1. **Create a deploy token.** GitHub → *Settings → Developer settings →
   Fine-grained personal access tokens*. Scope it to **only** the
   `TomGrobbe/TomGrobbe.github.io` repository with **Contents: Read and write**.
   (An SSH deploy key works too if you prefer.)

2. **Add it as a secret** in this repo: *Settings → Secrets and variables →
   Actions → New repository secret*, named **`HUB_DEPLOY_TOKEN`**.

3. **Turn off this repo's own GitHub Pages** (*Settings → Pages → Source → None*),
   since docs are served from the hub site, not from `vMenu`'s Pages. This avoids a
   stale site lingering at the old `/vMenu/` path.

The workflow adds a `.nojekyll` file to the hub automatically so Astro's `_astro/`
asset folder is served correctly.
