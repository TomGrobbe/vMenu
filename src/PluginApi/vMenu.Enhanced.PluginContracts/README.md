# vMenu.Enhanced.PluginContracts

The shared contract between vMenu Enhanced and its plugins. It holds the event names, the protocol version and the payload types that travel between a plugin and vMenu.

You normally never reference this package directly. It comes along automatically when you reference `vMenu.Enhanced.ClientAPI` or `vMenu.Enhanced.ServerAPI`, which are the packages you actually build a plugin with.

The package version always matches the vMenu Enhanced release it belongs to, so pin it to the vMenu version your server runs.

## License

This package is licensed under the **GNU General Public License v3.0 or later**, `GPL-3.0-or-later`, which is the same license vMenu Enhanced itself uses. The full text ships inside the package as `LICENSE.md`, and also lives in the [vMenu repository](https://github.com/TomGrobbe/vMenu/blob/enhanced/LICENSE.md).

**That license comes along with your plugin.** A plugin built on this package, or on either of the two packages that bring it in, is a work based on vMenu, so your plugin has to be licensed `GPL-3.0-or-later` as well. You cannot build on this and then put your own closed license on the result. In practice:

- **Running it on your own server, and nowhere else, asks nothing of you.** Using software is not distributing it. Build whatever you like and keep it to yourself.
- **The moment you hand your plugin to somebody else, you owe them the source.** Giving it away, selling it, listing it on a store, sending it to a friend who runs another server, all of that is distribution, and every one of those means the complete source code goes with it, under this same license.
- **You are allowed to charge money.** The GPL has no problem with being paid. What it does not allow is taking the money and keeping the source to yourself, and whoever paid you is then free to pass both on to other people.
- **So if your plugin will ever run on a server that is not yours, plan on open sourcing it.** That is not an extra rule on top of the license, it is what the license already asks of you.

This is a plain language summary, not legal advice. The license text is what actually counts, so read it if the details matter to your situation.
