import { currentResource, emitLocal, isAceAllowed, log, onLocal, parseJson, sanitizeId } from '../fivem.js';
import { normalizeResult, PluginEvents, RefreshPermissionsEvent, type RegisterResult } from '../protocol.js';
import { Signal, type Unsubscribe } from '../signal.js';
import type { ServerPluginDeclaration } from './declaration.js';

const PermissionPrefix = 'vMenu.Enhanced.Plugins';

const Everything = 'vMenu.Enhanced.Everything';

let declaration: ServerPluginDeclaration | undefined;

let firstResult: Promise<RegisterResult> | undefined;

let resolveFirst: ((result: RegisterResult) => void) | undefined;

let handlersRegistered = false;

let resourceName = '';

let pluginId = '';

const registrationAnswered = new Signal<[RegisterResult]>(() => resourceName, 'registration answered');

function ensureIdentity(): void {
  if (resourceName.length === 0) {
    resourceName = currentResource();
    pluginId = sanitizeId(resourceName);
  }
}

function sendRegistration(): void {
  if (declaration) {
    emitLocal(PluginEvents.ServerRegister, JSON.stringify(declaration.toRequest()));
  }
}

function onResult(json: unknown): void {
  const result = normalizeResult(parseJson<Partial<RegisterResult>>(json));

  if (!result) {
    log.warn(resourceName, 'vMenu sent a registration answer that did not parse.');

    return;
  }

  for (const error of result.errors) {
    log.error(resourceName, `vMenu refused the plugin registration: ${error}`);
  }

  for (const warning of result.warnings) {
    log.warn(resourceName, `vMenu accepted the plugin registration with a note: ${warning}`);
  }

  resolveFirst?.(result);
  resolveFirst = undefined;

  registrationAnswered.fire(result);
}

function ensureHandlers(): void {
  if (handlersRegistered) {
    return;
  }

  handlersRegistered = true;

  ensureIdentity();

  onLocal(PluginEvents.ServerReady, () => sendRegistration());
  onLocal(PluginEvents.serverReadyFor(resourceName), () => sendRegistration());
  onLocal(PluginEvents.serverRegisterResultFor(resourceName), (json: unknown) => onResult(json));
}

/**
 * The server side entry point for a plugin. Call `register` once at startup; registering again
 * after a vMenu restart happens by itself. Then use `isPlayerAllowed` to check the permissions you declared.
 */
export const VMenuServer = {
  /**
   * Declares the plugin with vMenu. The promise resolves on vMenu's first answer, which can be a
   * while when vMenu starts later than the plugin. It never rejects: a refusal arrives as a result
   * with `accepted` false.
   */
  register(pluginDeclaration: ServerPluginDeclaration): Promise<RegisterResult> {
    declaration = pluginDeclaration;
    firstResult ??= new Promise((resolve) => {
      resolveFirst = resolve;
    });

    ensureHandlers();
    emitLocal(PluginEvents.ServerProbe);

    return firstResult;
  },

  /**
   * Whether a player holds one of the plugin's own permissions, by its short name. Also honours the
   * container grants a server owner may have used instead of the exact name.
   */
  isPlayerAllowed(playerSource: string | number, permissionName: string): boolean {
    const source = String(playerSource ?? '');

    if (source.length === 0) {
      return false;
    }

    ensureIdentity();

    const scope = `${PermissionPrefix}.${pluginId}`;

    return (
      isAceAllowed(source, `${scope}.${permissionName}`) ||
      isAceAllowed(source, `${scope}.All`) ||
      isAceAllowed(source, `${PermissionPrefix}.All`) ||
      isAceAllowed(source, Everything)
    );
  },

  /**
   * The same check as `isPlayerAllowed`, but a refusal is also reported to vMenu's security webhook.
   * Use it where a legitimate client could only ever have sent the thing you are handling while
   * allowed, and keep `isPlayerAllowed` for ordinary branching.
   */
  requirePermission(playerSource: string | number, permissionName: string): boolean {
    if (VMenuServer.isPlayerAllowed(playerSource, permissionName)) {
      return true;
    }

    const source = String(playerSource ?? '');

    if (source.length > 0) {
      emitLocal(PluginEvents.ServerDenied, source, permissionName);
    }

    return false;
  },

  /** Refreshes permissions for one or more players. Passing no ids refreshes every connected player. */
  refreshPermissions(...serverIds: number[]): void {
    emitLocal(RefreshPermissionsEvent, serverIds.map((id) => Math.trunc(Number(id))));
  },

  /** Called on every registration answer, including automatic re-registrations. */
  onRegistrationAnswered(handler: (result: RegisterResult) => void): Unsubscribe {
    return registrationAnswered.add(handler);
  },
};
