import type { UpdateOp } from '../protocol.js';
import type { PluginItem } from './item.js';
import type { PluginKey } from './key.js';
import type { PluginMenu } from './menu.js';

export interface PluginHost {
  readonly resource: string;
  readonly isConnected: boolean;
  nextItemId(): string;
  nextMenuId(): string;
  registerMenu(menu: PluginMenu): void;
  registerItem(item: PluginItem): void;
  registerKey(key: PluginKey): void;
  unregisterItem(item: PluginItem): void;
  emitOp(op: UpdateOp): void;
  emitAdd(menu: PluginMenu, item: PluginItem, add: UpdateOp): void;
  filterChanged(menu: PluginMenu): void;
  mergeTranslations(code: string, entries: Record<string, string>): void;
}
