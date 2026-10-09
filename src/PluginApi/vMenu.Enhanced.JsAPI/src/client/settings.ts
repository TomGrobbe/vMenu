import { getConvar } from '../fivem.js';
import { formatFloat } from '../format.js';
import { SettingTypes, type SettingNode } from '../protocol.js';

/**
 * One of your plugin's convar settings. The full convar name is
 * `vMenu.Enhanced.Plugins.<Id>.<Name>`, set by the server owner with `setr`, and readable here
 * because replicated convars reach every resource.
 */
export abstract class PluginSetting<T> {
  /** @internal */
  constructor(name: string, fullName: string, defaultValue: T) {
    this.name = name;
    this.fullName = fullName;
    this.defaultValue = defaultValue;
  }

  /** The short name you declared. */
  readonly name: string;

  /** The composed convar name a server owner sets. */
  readonly fullName: string;

  /** The value used while the server owner has not set the convar. */
  readonly defaultValue: T;

  /** The current value, read from the convar every time. */
  abstract get value(): T;

  protected raw(fallback: string): string {
    return getConvar(this.fullName, fallback);
  }
}

/** A setting that is on or off. Gates can use it through `Gate.setting`. */
export class PluginBoolSetting extends PluginSetting<boolean> {
  get value(): boolean {
    return this.raw(this.defaultValue ? 'true' : 'false').toLowerCase() === 'true';
  }
}

/** A whole number setting. */
export class PluginIntSetting extends PluginSetting<number> {
  get value(): number {
    const raw = this.raw('').trim();

    return /^[+-]?\d+$/.test(raw) ? Number.parseInt(raw, 10) : this.defaultValue;
  }
}

/** A decimal number setting. */
export class PluginFloatSetting extends PluginSetting<number> {
  get value(): number {
    const raw = this.raw('').trim();
    const parsed = Number(raw);

    return raw.length > 0 && Number.isFinite(parsed) ? parsed : this.defaultValue;
  }
}

/** A text setting. An empty convar falls back to the default. */
export class PluginStringSetting extends PluginSetting<string> {
  get value(): string {
    const raw = this.raw(this.defaultValue);

    return raw.length === 0 ? this.defaultValue : raw;
  }
}

/**
 * Your plugin's settings. Declaring one here lets your menu gate on it and lets vMenu track it for
 * live refresh. Declare the same settings in your server script too, so they appear in the template
 * vMenu writes for the server owner.
 */
export class PluginSettings {
  private readonly prefix: string;

  /** @internal */
  readonly nodes: SettingNode[] = [];

  /** @internal */
  constructor(pluginId: string) {
    this.prefix = `vMenu.Enhanced.Plugins.${pluginId}.`;
  }

  /** Declares an on or off setting. */
  bool(name: string, defaultValue: boolean, description: string): PluginBoolSetting {
    this.declare(name, SettingTypes.Bool, defaultValue ? 'true' : 'false', description);

    return new PluginBoolSetting(name, this.prefix + name, defaultValue);
  }

  /** Declares a whole number setting. */
  int(name: string, defaultValue: number, description: string): PluginIntSetting {
    const value = Math.trunc(defaultValue);

    this.declare(name, SettingTypes.Int, String(value), description);

    return new PluginIntSetting(name, this.prefix + name, value);
  }

  /** Declares a decimal number setting. */
  float(name: string, defaultValue: number, description: string): PluginFloatSetting {
    this.declare(name, SettingTypes.Float, formatFloat(defaultValue), description);

    return new PluginFloatSetting(name, this.prefix + name, defaultValue);
  }

  /** Declares a text setting. */
  string(name: string, defaultValue: string, description: string): PluginStringSetting {
    this.declare(name, SettingTypes.String, defaultValue, description);

    return new PluginStringSetting(name, this.prefix + name, defaultValue);
  }

  private declare(name: string, type: string, defaultText: string, description: string): void {
    this.nodes.push({ name, type, default: defaultText, description });
  }
}
