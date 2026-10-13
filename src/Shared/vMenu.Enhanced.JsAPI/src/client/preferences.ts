import { getKvp, setKvp } from '../fivem.js';

const KeyPrefix = 'vmenu_plugin_pref_';

export function readBool(itemId: string): boolean | undefined {
  const raw = getKvp(KeyPrefix + itemId);

  if (raw === 'true') {
    return true;
  }

  if (raw === 'false') {
    return false;
  }

  return undefined;
}

export function writeBool(itemId: string, value: boolean): void {
  setKvp(KeyPrefix + itemId, value ? 'true' : 'false');
}
