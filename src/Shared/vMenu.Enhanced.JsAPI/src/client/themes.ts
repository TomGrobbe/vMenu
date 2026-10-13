import { emitLocal, parseJson } from '../fivem.js';
import { PluginEvents, type ThemeList } from '../protocol.js';
import { Signal, type Unsubscribe } from '../signal.js';

/** One of vMenu's looks. Hand the `id` to `themes.set` to put the menus in it, and show the `name` to the player. */
export interface PluginTheme {
  readonly id: string;
  readonly name: string;
  /** Whether this is the theme on screen right now. */
  readonly isCurrent: boolean;
}

/**
 * The look of vMenu's menus, as seen by this player only. vMenu hands the list over right after the
 * plugin registers and again on every change, so read `available` from `onChanged` rather than
 * straight after connecting. A theme set here lasts as long as the client runs.
 */
export class PluginThemes {
  private availableValue: readonly PluginTheme[] = [];

  private currentIdValue?: string;

  private configuredIdValue?: string;

  private overriddenValue = false;

  private readonly changed: Signal<[]>;

  /** @internal */
  constructor(private readonly plugin: { readonly resource: string; readonly isConnected: boolean }) {
    this.changed = new Signal(() => plugin.resource, 'themes changed');
  }

  /** Every theme vMenu offers, in the order it lists them. Empty until vMenu has sent them. */
  get available(): readonly PluginTheme[] {
    return this.availableValue;
  }

  /** The id of the theme on screen, undefined until vMenu has said what it is. */
  get currentId(): string | undefined {
    return this.currentIdValue;
  }

  /** The id the server's own setting asks for, which is where `reset` goes. */
  get configuredId(): string | undefined {
    return this.configuredIdValue;
  }

  /** Whether a plugin is overriding the server's setting for this player right now. */
  get isOverridden(): boolean {
    return this.overriddenValue;
  }

  /** Called whenever the list or the theme on screen changed, including when somebody else changed it. */
  onChanged(handler: () => void): Unsubscribe {
    return this.changed.add(handler);
  }

  /** Puts vMenu's menus in a theme for this player. An id vMenu does not know is ignored. */
  set(themeId: string): void {
    this.send(themeId);
  }

  /** Drops the override and goes back to the theme the server's setting asks for. */
  reset(): void {
    this.send(undefined);
  }

  /** @internal */
  handle(json: unknown): void {
    const list = parseJson<ThemeList>(json);

    if (!list) {
      return;
    }

    const current = list.current?.toLowerCase();

    this.availableValue = (Array.isArray(list.themes) ? list.themes : [])
      .filter((theme) => typeof theme?.id === 'string')
      .map((theme) => ({
        id: theme.id,
        name: typeof theme.name === 'string' ? theme.name : theme.id,
        isCurrent: theme.id.toLowerCase() === current,
      }));
    this.currentIdValue = list.current ?? undefined;
    this.configuredIdValue = list.configured ?? undefined;
    this.overriddenValue = list.overridden === true;

    this.changed.fire();
  }

  private send(themeId: string | undefined): void {
    if (!this.plugin.isConnected) {
      return;
    }

    emitLocal(PluginEvents.SetTheme, JSON.stringify({ theme: themeId ?? null }));
  }
}
