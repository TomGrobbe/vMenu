const natives = globalThis as unknown as {
  GetCurrentResourceName(): string;
  GetConvar(name: string, fallback: string): string;
  GetResourceKvpString(key: string): string | null | undefined;
  SetResourceKvp(key: string, value: string): void;
  IsPlayerAceAllowed(playerSource: string, object: string): boolean;
  emit(eventName: string, ...args: unknown[]): void;
  on(eventName: string, handler: (...args: any[]) => void): void;
};

declare const console: {
  log(message: string): void;
  warn(message: string): void;
  error(message: string): void;
};

export const currentResource = (): string => natives.GetCurrentResourceName();

export const emitLocal = (eventName: string, ...args: unknown[]): void => natives.emit(eventName, ...args);

export const onLocal = (eventName: string, handler: (...args: any[]) => void): void => natives.on(eventName, handler);

export const getConvar = (name: string, fallback: string): string => natives.GetConvar(name, fallback) ?? fallback;

export const getKvp = (key: string): string | undefined => natives.GetResourceKvpString(key) ?? undefined;

export const setKvp = (key: string, value: string): void => natives.SetResourceKvp(key, value);

export const isAceAllowed = (playerSource: string, object: string): boolean =>
  Boolean(natives.IsPlayerAceAllowed(playerSource, object));

export const log = {
  info: (resource: string, message: string): void => console.log(`[${resource}] ${message}`),
  warn: (resource: string, message: string): void => console.warn(`[${resource}] ${message}`),
  error: (resource: string, message: string): void => console.error(`[${resource}] ${message}`),
};

export function parseJson<T>(json: unknown): T | undefined {
  if (typeof json !== 'string') {
    return undefined;
  }

  try {
    return JSON.parse(json) as T;
  } catch {
    return undefined;
  }
}

export function sanitizeId(resource: string): string {
  return resource.replace(/[^A-Za-z0-9]/g, '_');
}
