import { formatFloat } from '../format.js';
import { SERVER_PROTOCOL_VERSION, SettingTypes, type ServerRegisterRequest, type SettingNode } from '../protocol.js';

/**
 * Everything a plugin's server side declares with vMenu: a display name for the generated example
 * files, the permission names it wants under its own scope, and the convar settings vMenu should
 * describe to server owners. Names are short: vMenu composes the full
 * `vMenu.Enhanced.Plugins.<Id>.<Name>` form from the resource name.
 */
export class ServerPluginDeclaration {
  private readonly permissions: { name: string; description: string; staffOnly: boolean }[] = [];

  private readonly settings: SettingNode[] = [];

  private readonly loggedItems: { itemId: string; description: string }[] = [];

  /** @param displayName Used in the generated example files, so owners see which plugin a section belongs to. */
  constructor(readonly displayName: string) {}

  /** Declares a permission. `staffOnly` marks it as staff only in the generated permissions example. */
  addPermission(name: string, description: string, staffOnly = false): this {
    this.permissions.push({ name, description, staffOnly });

    return this;
  }

  /**
   * Lets the server owner see a line in their webhook whenever somebody uses this row. The client
   * half still has to set `log` on it. `description` is a noun phrase dropped into vMenu's own
   * wording, as in "turned the anti-grief shield on".
   */
  addLoggedItem(itemId: string, description: string): this {
    this.loggedItems.push({ itemId, description });

    return this;
  }

  /** Declares an on or off setting. */
  addBoolSetting(name: string, defaultValue: boolean, description: string): this {
    return this.add(name, SettingTypes.Bool, defaultValue ? 'true' : 'false', description);
  }

  /** Declares a whole number setting. */
  addIntSetting(name: string, defaultValue: number, description: string): this {
    return this.add(name, SettingTypes.Int, String(Math.trunc(defaultValue)), description);
  }

  /** Declares a decimal number setting. */
  addFloatSetting(name: string, defaultValue: number, description: string): this {
    return this.add(name, SettingTypes.Float, formatFloat(defaultValue), description);
  }

  /** Declares a text setting. */
  addStringSetting(name: string, defaultValue: string, description: string): this {
    return this.add(name, SettingTypes.String, defaultValue, description);
  }

  /** @internal */
  toRequest(): ServerRegisterRequest {
    return {
      protocolVersion: SERVER_PROTOCOL_VERSION,
      displayName: this.displayName,
      permissions: [...this.permissions],
      settings: [...this.settings],
      loggedItems: [...this.loggedItems],
    };
  }

  private add(name: string, type: string, defaultText: string, description: string): this {
    this.settings.push({ name, type, default: defaultText, description });

    return this;
  }
}
