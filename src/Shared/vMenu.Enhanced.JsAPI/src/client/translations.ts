import type { PluginHost } from './host.js';

/**
 * Your plugin's translation tables: language code to key to text. An English table under "en" is
 * required as soon as any of your texts use keys, and it is the fallback whenever the selected
 * language has no entry. vMenu follows its selected language live.
 */
export class PluginTranslations {
  /** @internal */
  readonly tables: Record<string, Record<string, string>> = {};

  /** @internal */
  constructor(private readonly plugin: PluginHost) {}

  /** Adds or extends one language's table. Later entries win over earlier ones. */
  add(languageCode: string, entries: Readonly<Record<string, string>>): void {
    const code = languageCode.trim().toLowerCase();
    const table = (this.tables[code] ??= {});
    const merged: Record<string, string> = {};

    for (const [key, value] of Object.entries(entries)) {
      table[key] = value;
      merged[key] = value;
    }

    this.plugin.mergeTranslations(code, merged);
  }
}
