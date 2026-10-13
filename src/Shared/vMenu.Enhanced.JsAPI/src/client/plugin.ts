import { currentResource, emitLocal, log, onLocal, parseJson, sanitizeId } from '../fivem.js';
import {
  CallbackTypes,
  normalizeResult,
  PluginEvents,
  PROTOCOL_VERSION,
  UpdateOps,
  VMENU_RESOURCE,
  type PluginCallback,
  type PromptRequest,
  type PromptResult,
  type RegisterRequest,
  type RegisterResult,
  type UpdateOp,
} from '../protocol.js';
import { describe, Signal, type Unsubscribe } from '../signal.js';
import { Text, type TextLike } from '../text.js';
import { int } from '../format.js';
import type { PluginItem } from './item.js';
import { PluginSubmenu } from './items.js';
import type { PluginKey } from './key.js';
import { PluginMenu } from './menu.js';
import { PluginPlayerActions } from './playerActions.js';
import { PluginSettings } from './settings.js';
import { PluginThemes } from './themes.js';
import { PluginTranslations } from './translations.js';

/** How a notification is coloured in vMenu's notification area. The plain strings work too. */
export const NotifyStyle = {
  Info: 'info',
  Success: 'success',
  Warning: 'warning',
  Error: 'error',
} as const;

/** How a notification is coloured in vMenu's notification area. */
export type NotifyStyle = (typeof NotifyStyle)[keyof typeof NotifyStyle];

/** One suggestion row the input box offers while the player types. */
export interface PromptSuggestion {
  /** What lands in the box when the suggestion is picked. */
  value: string;
  /** What the player reads in the list. The value is shown when left out. */
  description?: string;
}

/** One question for vMenu's input box. */
export interface PluginPrompt {
  title: TextLike;
  /** The longest answer allowed, 60 when left out. vMenu caps it at 500. */
  maxLength?: number;
  /** What the box starts with. */
  initialValue?: string;
  suggestions?: readonly PromptSuggestion[];
}

const CarriedItemOps = new Set<string>([
  UpdateOps.SetText,
  UpdateOps.SetDescription,
  UpdateOps.SetLabel,
  UpdateOps.SetLockedDescription,
  UpdateOps.SetConfirmationDescription,
  UpdateOps.SetIcons,
  UpdateOps.SetChecked,
  UpdateOps.SetOptions,
  UpdateOps.SetSelectedIndex,
  UpdateOps.SetSliderPosition,
  UpdateOps.SetValue,
  UpdateOps.SetVisible,
  UpdateOps.SetEnabled,
  UpdateOps.SetGate,
  UpdateOps.SetLog,
  UpdateOps.SetBehaviour,
  UpdateOps.SetItemEvents,
]);

const CarriedMenuOps = new Set<string>([
  UpdateOps.SetMenuTitle,
  UpdateOps.SetMenuSubtitle,
  UpdateOps.SetMenuEvents,
  UpdateOps.AddKeys,
]);

let instance: VMenuPlugin | undefined;

/**
 * Your plugin's client side entry point. Create it once, declare your menus, translations and
 * settings, then call `connect`. It registers again by itself whenever vMenu restarts, so everything
 * you declared and changed since is restored.
 */
export class VMenuPlugin {
  private readonly itemsById = new Map<string, PluginItem>();

  private readonly menusById = new Map<string, PluginMenu>();

  private readonly keysById = new Map<string, PluginKey>();

  private readonly pendingPrompts = new Map<number, (result: PromptResult) => void>();

  private readonly pendingItems = new Set<string>();

  private readonly pendingMenus = new Set<string>();

  private readonly dirtyFilters: PluginMenu[] = [];

  private readonly registrationAnswered: Signal<[RegisterResult]>;

  private readonly disconnected: Signal<[]>;

  private firstResult?: Promise<RegisterResult>;

  private resolveFirst?: (result: RegisterResult) => void;

  private batchOps?: UpdateOp[];

  private batchDepth = 0;

  private nextItem = 0;

  private nextMenu = 0;

  private nextPrompt = 0;

  private handlersRegistered = false;

  private connected = false;

  /** The resource this plugin runs in, its identity towards vMenu. */
  readonly resource: string;

  /** The sanitized identity used inside permission and convar names. */
  readonly id: string;

  /** Extra line under the resource name in your row's description, as a translation key. */
  descriptionKey?: string;

  /** Your plugin's convar settings. */
  readonly settings: PluginSettings;

  /** Your plugin's translation tables. */
  readonly translations: PluginTranslations;

  /** The look of vMenu's menus for this player. */
  readonly themes: PluginThemes;

  /** The menu behind your row in vMenu's Plugins menu. */
  readonly rootMenu: PluginMenu;

  /** Actions added to every player's entry of vMenu's Online Players menu. */
  readonly playerActions: PluginPlayerActions;

  private constructor(private readonly displayName: TextLike) {
    this.resource = currentResource();
    this.id = sanitizeId(this.resource);
    this.settings = new PluginSettings(this.id);
    this.themes = new PluginThemes(this);
    this.translations = new PluginTranslations(this);
    this.playerActions = new PluginPlayerActions(this);
    this.rootMenu = new PluginMenu(this, { id: 'root', title: Text.toRef(displayName), items: [] }, displayName);
    this.registrationAnswered = new Signal(() => this.resource, 'registration answered');
    this.disconnected = new Signal(() => this.resource, 'disconnected');

    this.registerMenu(this.rootMenu);
  }

  /** Creates the plugin. One per resource: a second call returns the first instance. */
  static create(displayName: TextLike): VMenuPlugin {
    if (instance) {
      log.warn(instance.resource, 'VMenuPlugin.create was called twice, returning the first instance.');

      return instance;
    }

    instance = new VMenuPlugin(displayName);

    return instance;
  }

  /** Whether vMenu currently has this plugin registered. */
  get isConnected(): boolean {
    return this.connected;
  }

  /** Called on every registration answer, including automatic re-registrations. */
  onRegistrationAnswered(handler: (result: RegisterResult) => void): Unsubscribe {
    return this.registrationAnswered.add(handler);
  }

  /** Called when vMenu stops, after which the plugin waits to register again. */
  onDisconnected(handler: () => void): Unsubscribe {
    return this.disconnected.add(handler);
  }

  /**
   * Registers with vMenu. The promise resolves on vMenu's first answer, which can be a while when
   * vMenu starts later than your resource. It never rejects: a refusal arrives as a result with
   * `accepted` false.
   */
  connect(): Promise<RegisterResult> {
    this.firstResult ??= new Promise((resolve) => {
      this.resolveFirst = resolve;
    });

    this.ensureHandlers();

    emitLocal(PluginEvents.Probe);

    return this.firstResult;
  }

  /** Shows a message through vMenu's notification area, credited to your resource. */
  notify(style: NotifyStyle, text: TextLike, durationMs?: number): void {
    if (!this.connected) {
      return;
    }

    emitLocal(PluginEvents.Notify, JSON.stringify({ style, text: Text.toRef(text), durationMs: durationMs === undefined ? undefined : int(durationMs) }));
  }

  /** Asks the player for text through vMenu's input box. Resolves to null if they cancelled or the box was unavailable. */
  async getText(
    title: TextLike,
    options?: { maxLength?: number; initialValue?: string; suggestions?: readonly PromptSuggestion[] },
  ): Promise<string | null> {
    const answers = await this.getTexts([{ title, ...options }]);

    return answers && answers.length > 0 ? answers[0]! : null;
  }

  /** Asks several questions one after another. Resolves to null if the player cancelled any of them. */
  getTexts(prompts: readonly PluginPrompt[]): Promise<string[] | null> {
    if (prompts.length === 0 || !this.connected) {
      return Promise.resolve(null);
    }

    const requestId = ++this.nextPrompt;

    const request: PromptRequest = {
      requestId,
      prompts: prompts.map((prompt) => ({
        title: Text.toRef(prompt.title),
        maxLength: int(prompt.maxLength ?? 60),
        initial: String(prompt.initialValue ?? ''),
        suggestions:
          prompt.suggestions && prompt.suggestions.length > 0
            ? prompt.suggestions.map((suggestion) => ({ value: suggestion.value, description: suggestion.description }))
            : undefined,
      })),
    };

    const pending = new Promise<PromptResult>((resolve) => this.pendingPrompts.set(requestId, resolve));

    emitLocal(PluginEvents.Prompt, JSON.stringify(request));

    return pending.then((result) => (result.cancelled || !Array.isArray(result.answers) ? null : [...result.answers]));
  }

  /**
   * Groups every change made inside `changes` into one update, so many small changes cost vMenu a
   * single repaint. Nesting is fine: only the outermost batch sends. The batch ends when `changes`
   * returns, so changes made after an `await` inside it are sent on their own.
   */
  batch<T>(changes: () => T): T {
    this.batchOps ??= [];
    this.batchDepth++;

    try {
      return changes();
    } finally {
      this.endBatch();
    }
  }

  /** @internal */
  nextItemId(): string {
    return `i${++this.nextItem}`;
  }

  /** @internal */
  nextMenuId(): string {
    return `m${++this.nextMenu}`;
  }

  /** @internal */
  registerMenu(menu: PluginMenu): void {
    this.menusById.set(menu.id, menu);
  }

  /** @internal */
  registerItem(item: PluginItem): void {
    this.itemsById.set(item.id, item);
  }

  /** @internal */
  registerKey(key: PluginKey): void {
    if (this.keysById.has(key.id)) {
      log.warn(this.resource, `Key id '${key.id}' is used twice, vMenu will skip the second one.`);

      return;
    }

    this.keysById.set(key.id, key);
  }

  /** @internal */
  unregisterItem(item: PluginItem): void {
    this.itemsById.delete(item.id);

    if (!(item instanceof PluginSubmenu)) {
      return;
    }

    this.menusById.delete(item.menu.id);

    for (const key of item.menu.keys) {
      this.keysById.delete(key.id);
    }

    for (const child of item.menu.items) {
      this.unregisterItem(child);
    }
  }

  /** @internal */
  emitOp(op: UpdateOp): void {
    if (!this.connected) {
      return;
    }

    const batch = this.batchOps;

    if (!batch) {
      this.send([op]);

      return;
    }

    if (this.carriedByPendingAdd(op)) {
      return;
    }

    if (op.op === UpdateOps.AddItems) {
      for (const node of op.items ?? []) {
        this.pendingItems.add(node.id);

        if (node.menu) {
          this.pendingMenus.add(node.menu.id);
        }
      }
    }

    batch.push(op);
  }

  /** @internal */
  filterChanged(menu: PluginMenu): void {
    if (!this.connected) {
      return;
    }

    if (!this.batchOps) {
      this.send([menu.filterOp()]);

      return;
    }

    this.markFilterDirty(menu);
  }

  /** @internal */
  emitAdd(menu: PluginMenu, item: PluginItem, add: UpdateOp): void {
    if (!this.connected) {
      return;
    }

    if (this.batchOps) {
      this.emitOp(add);

      if (menu.hasFilter) {
        this.markFilterDirty(menu);
      }

      return;
    }

    const ops: UpdateOp[] = [add];

    if (menu.hides(item)) {
      ops.push({ op: UpdateOps.SetFilter, menuId: menu.id, itemIds: [item.id], flag: true });
    }

    this.send(ops);
  }

  /** @internal */
  mergeTranslations(code: string, entries: Record<string, string>): void {
    if (this.connected) {
      this.emitOp({ op: UpdateOps.MergeTranslations, language: code, entries });
    }
  }

  private endBatch(): void {
    if (--this.batchDepth > 0) {
      return;
    }

    const ops = this.batchOps;

    if (!ops) {
      return;
    }

    this.batchOps = undefined;
    this.pendingItems.clear();
    this.pendingMenus.clear();

    for (const menu of this.dirtyFilters) {
      ops.push(menu.filterOp());
    }

    this.dirtyFilters.length = 0;

    if (ops.length === 0 || !this.connected) {
      return;
    }

    this.send(ops);
  }

  private markFilterDirty(menu: PluginMenu): void {
    if (!this.dirtyFilters.includes(menu)) {
      this.dirtyFilters.push(menu);
    }
  }

  private carriedByPendingAdd(op: UpdateOp): boolean {
    if (CarriedItemOps.has(op.op)) {
      return op.itemId !== undefined && this.pendingItems.has(op.itemId);
    }

    if (CarriedMenuOps.has(op.op)) {
      return op.menuId !== undefined && this.pendingMenus.has(op.menuId);
    }

    return false;
  }

  private send(ops: UpdateOp[]): void {
    const payload = ops.map((op) =>
      op.op === UpdateOps.AddItems && op.items ? { ...op, items: op.items.map(PluginMenu.asAdded) } : op,
    );

    emitLocal(PluginEvents.Update, JSON.stringify({ ops: payload }));
  }

  private ensureHandlers(): void {
    if (this.handlersRegistered) {
      return;
    }

    this.handlersRegistered = true;

    onLocal(PluginEvents.Ready, () => this.sendRegistration());
    onLocal(PluginEvents.readyFor(this.resource), () => this.sendRegistration());
    onLocal(PluginEvents.registerResultFor(this.resource), (json: unknown) => this.onRegisterResult(json));
    onLocal(PluginEvents.eventFor(this.resource), (json: unknown) => this.onCallback(json));
    onLocal(PluginEvents.promptResultFor(this.resource), (json: unknown) => this.onPromptResult(json));
    onLocal(PluginEvents.themesFor(this.resource), (json: unknown) => this.themes.handle(json));
    onLocal('onResourceStop', (stopped: unknown) => this.onResourceStop(stopped));
  }

  private sendRegistration(): void {
    emitLocal(PluginEvents.Register, JSON.stringify(this.buildRequest()));
  }

  private buildRequest(): RegisterRequest {
    const request: RegisterRequest = {
      protocolVersion: PROTOCOL_VERSION,
      displayName: Text.toRef(this.displayName),
      descriptionKey: this.descriptionKey,
      menu: this.rootMenu.node,
      playerActions: this.playerActions.nodes.length > 0 ? this.playerActions.nodes : undefined,
    };

    if (Object.keys(this.translations.tables).length > 0) {
      request.translations = this.translations.tables;
    }

    if (this.settings.nodes.length > 0) {
      request.settings = [...this.settings.nodes];
    }

    return request;
  }

  private onRegisterResult(json: unknown): void {
    const result = normalizeResult(parseJson<Partial<RegisterResult>>(json));

    if (!result) {
      log.warn(this.resource, 'vMenu sent a registration answer that did not parse.');

      return;
    }

    for (const error of result.errors) {
      log.error(this.resource, `vMenu refused the plugin registration: ${error}`);
    }

    for (const warning of result.warnings) {
      log.warn(this.resource, `vMenu accepted the plugin registration with a note: ${warning}`);
    }

    this.connected = result.accepted;

    if (!this.connected) {
      this.cancelPendingPrompts();
    } else {
      this.batch(() => {
        for (const menu of this.menusById.values()) {
          if (menu.hasFilter) {
            this.filterChanged(menu);
          }
        }
      });
    }

    this.resolveFirst?.(result);
    this.resolveFirst = undefined;

    this.registrationAnswered.fire(result);
  }

  private onCallback(json: unknown): void {
    const callback = parseJson<PluginCallback>(json);

    if (!callback) {
      return;
    }

    try {
      switch (callback.type) {
        case CallbackTypes.MenuOpened:
        case CallbackTypes.MenuClosed:
        case CallbackTypes.MenuIndexChanged: {
          const menu = callback.menuId === undefined ? undefined : this.menusById.get(callback.menuId);

          menu?.handleMenu(callback);
          break;
        }

        case CallbackTypes.KeyPressed: {
          const key = callback.keyId === undefined ? undefined : this.keysById.get(callback.keyId);

          key?.handle({ item: this.itemOrUndefined(callback.itemId), disabledItem: this.itemOrUndefined(callback.disabledItemId) });
          break;
        }

        default: {
          const item = callback.itemId === undefined ? undefined : this.itemsById.get(callback.itemId);

          item?.handle(callback);
          break;
        }
      }
    } catch (error) {
      log.error(this.resource, `A menu callback handler threw: ${describe(error)}`);
    }
  }

  private itemOrUndefined(id: string | undefined | null): PluginItem | undefined {
    return id === undefined || id === null ? undefined : this.itemsById.get(id);
  }

  private onPromptResult(json: unknown): void {
    const result = parseJson<PromptResult>(json);

    if (!result) {
      return;
    }

    const pending = this.pendingPrompts.get(result.requestId);

    if (pending) {
      this.pendingPrompts.delete(result.requestId);
      pending(result);
    }
  }

  private cancelPendingPrompts(): void {
    for (const pending of this.pendingPrompts.values()) {
      pending({ requestId: 0, cancelled: true, busy: false });
    }

    this.pendingPrompts.clear();
  }

  private onResourceStop(stopped: unknown): void {
    if (typeof stopped !== 'string' || stopped.toLowerCase() !== VMENU_RESOURCE.toLowerCase()) {
      return;
    }

    this.connected = false;
    this.cancelPendingPrompts();
    this.disconnected.fire();
  }
}
