import { CallbackTypes, EntryTypes, NodeEvents, UpdateOps, type ItemNode, type KeyNode, type MenuNode, type PluginCallback, type UpdateOp } from '../protocol.js';
import { Signal, type Unsubscribe } from '../signal.js';
import { Text, type TextLike } from '../text.js';
import { sameText } from './diff.js';
import type { PluginHost } from './host.js';
import { PluginItem, type ItemOptions } from './item.js';
import {
  PluginButton,
  PluginCheckbox,
  PluginConfirmButton,
  PluginConfirmList,
  PluginDynamicList,
  PluginList,
  PluginSeparator,
  PluginSlider,
  PluginSubmenu,
} from './items.js';
import { PluginKey } from './key.js';
import { readBool } from './preferences.js';
import { int } from '../format.js';

/** Extra properties for a confirm button or confirm list. */
export interface ConfirmOptions extends ItemOptions {
  /** What the row asks before its second press. */
  confirmationDescription?: TextLike;
}

/** Extra properties for a checkbox. */
export interface CheckboxOptions extends ItemOptions {
  /** Whether the box starts ticked. */
  checked?: boolean;
  /**
   * Saves the player's choice in this resource's key value store and restores it on the next start.
   * Pass a stable `id` along with it: generated ids follow creation order, so reordering your code
   * would hand a saved value to the wrong box.
   */
  persist?: boolean;
}

/** Extra properties for a list. */
export interface ListOptions extends ItemOptions {
  /** The option selected at the start, counted from 0. */
  selectedIndex?: number;
}

/** Extra properties for a slider. */
export interface SliderOptions extends ItemOptions {
  /** Draws a mark in the middle of the bar. */
  showDivider?: boolean;
}

/** Extra properties for a submenu row. */
export interface SubmenuOptions extends ItemOptions {
  /** The new menu's title. Falls back to the row's text. */
  title?: TextLike;
  /** The bar under the new menu's banner. */
  subtitle?: TextLike;
}

/** Extra properties for a key. */
export interface KeyOptions {
  /** A controller button, for example "RUP_INDEX". Without one the key shows no button while the player uses a controller. */
  defaultButton?: string;
  /** What the game's key settings list it as. Falls back to the text. */
  description?: TextLike;
  /** A game control index to suppress while the menu is open, for a default key the game already uses. */
  shadowedControl?: number;
}

/**
 * One of your plugin's menus: the root under your row in vMenu's Plugins menu, or a submenu. Rows
 * added before connecting ride along with the registration, rows added later appear live.
 */
export class PluginMenu {
  private readonly itemList: PluginItem[] = [];

  private readonly keyList: PluginKey[] = [];

  private titleValue?: TextLike;

  private subtitleValue?: TextLike;

  private readonly opened: Signal<[]>;

  private readonly closed: Signal<[]>;

  private readonly indexChanged: Signal<[number, number]>;

  private insertIndex?: number;

  private filterFn?: (item: PluginItem) => boolean;

  /** @internal */
  readonly node: MenuNode;

  /** @internal */
  constructor(
    private readonly plugin: PluginHost,
    node: MenuNode,
    title?: TextLike,
    subtitle?: TextLike,
  ) {
    this.node = node;
    this.titleValue = title;
    this.subtitleValue = subtitle;
    this.opened = new Signal(() => plugin.resource, 'opened');
    this.closed = new Signal(() => plugin.resource, 'closed');
    this.indexChanged = new Signal(() => plugin.resource, 'indexChanged');
  }

  /** The menu's id, how vMenu refers to it. */
  get id(): string {
    return this.node.id;
  }

  /** Every row, in order. */
  get items(): readonly PluginItem[] {
    return this.itemList;
  }

  /** Every key of this menu. */
  get keys(): readonly PluginKey[] {
    return this.keyList;
  }

  /** Whether `filter` is hiding rows right now. */
  get isFiltered(): boolean {
    return this.filterFn !== undefined;
  }

  /** The title in the menu's banner. */
  get title(): TextLike | undefined {
    return this.titleValue;
  }

  set title(value: TextLike | undefined) {
    this.titleValue = value;

    const title = Text.toRef(value);

    if (sameText(this.node.title, title)) {
      return;
    }

    this.node.title = title;
    this.plugin.emitOp({ op: UpdateOps.SetMenuTitle, menuId: this.id, textValue: title });
  }

  /** The bar under the banner. */
  get subtitle(): TextLike | undefined {
    return this.subtitleValue;
  }

  set subtitle(value: TextLike | undefined) {
    this.subtitleValue = value;

    const subtitle = Text.toRef(value);

    if (sameText(this.node.subtitle, subtitle)) {
      return;
    }

    this.node.subtitle = subtitle;
    this.plugin.emitOp({ op: UpdateOps.SetMenuSubtitle, menuId: this.id, textValue: subtitle });
  }

  /** Called when the player opens this menu. */
  onOpened(handler: () => void): Unsubscribe {
    const unsubscribe = this.opened.add(handler);

    this.subscribeMenuEvent(NodeEvents.Opened);

    return unsubscribe;
  }

  /** Called when the player leaves this menu, including into a submenu. */
  onClosed(handler: () => void): Unsubscribe {
    const unsubscribe = this.closed.add(handler);

    this.subscribeMenuEvent(NodeEvents.Closed);

    return unsubscribe;
  }

  /** Called when the cursor moves, with the old and new row index counted from 0. Chatty. */
  onIndexChanged(handler: (oldIndex: number, newIndex: number) => void): Unsubscribe {
    const unsubscribe = this.indexChanged.add(handler);

    this.subscribeMenuEvent(NodeEvents.IndexChanged);

    return unsubscribe;
  }

  /** Adds a row the player presses. */
  addButton(text: TextLike, options?: ItemOptions): PluginButton {
    return this.attach(new PluginButton(this.newNode(EntryTypes.Button, text, options), text), options);
  }

  /** Adds a row that asks for a second press before it does anything. */
  addConfirmButton(text: TextLike, options?: ConfirmOptions): PluginConfirmButton {
    const item = new PluginConfirmButton(this.newNode(EntryTypes.ConfirmButton, text, options), text);

    if (options?.confirmationDescription !== undefined) {
      item.confirmationDescription = options.confirmationDescription;
    }

    return this.attach(item, options);
  }

  /** Adds a row with a tick box. */
  addCheckbox(text: TextLike, options?: CheckboxOptions): PluginCheckbox {
    const node = this.newNode(EntryTypes.Checkbox, text, options);

    node.checked = options?.checked ?? false;

    const checkbox = new PluginCheckbox(node, text);

    if (options?.persist) {
      checkbox.persistedValue = true;

      const stored = readBool(node.id);

      if (stored !== undefined) {
        node.checked = stored;
      }
    }

    return this.attach(checkbox, options);
  }

  /** Adds a row the player scrolls through a set of options on. */
  addList(text: TextLike, values: readonly TextLike[], options?: ListOptions): PluginList {
    const node = this.newNode(EntryTypes.List, text, options);

    node.options = Text.toRefs(values);
    node.selectedIndex = int(options?.selectedIndex ?? 0);

    return this.attach(new PluginList(node, text), options);
  }

  /** Adds a list that asks for a second press before it does anything. */
  addConfirmList(text: TextLike, values: readonly TextLike[], options?: ListOptions & ConfirmOptions): PluginConfirmList {
    const node = this.newNode(EntryTypes.ConfirmList, text, options);

    node.options = Text.toRefs(values);
    node.selectedIndex = int(options?.selectedIndex ?? 0);

    const item = new PluginConfirmList(node, text);

    if (options?.confirmationDescription !== undefined) {
      item.confirmationDescription = options.confirmationDescription;
    }

    return this.attach(item, options);
  }

  /** Adds a row with a bar the player drags between `min` and `max`. */
  addSlider(text: TextLike, min: number, max: number, position: number, options?: SliderOptions): PluginSlider {
    const node = this.newNode(EntryTypes.Slider, text, options);

    node.min = int(min);
    node.max = int(max);
    node.position = int(position);
    node.showDivider = options?.showDivider ?? false;

    return this.attach(new PluginSlider(node, text), options);
  }

  /** Adds a row whose value your code works out each time the player scrolls it, through `changeRequested`. */
  addDynamicList(text: TextLike, initialValue: string, options?: ItemOptions): PluginDynamicList {
    const node = this.newNode(EntryTypes.DynamicList, text, options);

    node.value = String(initialValue);

    return this.attach(new PluginDynamicList(node, text), options);
  }

  /** Adds a row that only shows text, used to split a menu into sections. */
  addSeparator(text: TextLike, options?: ItemOptions): PluginSeparator {
    return this.attach(new PluginSeparator(this.newNode(EntryTypes.Separator, text, options), text), options);
  }

  /** Adds a row that opens a new menu, reached through the returned row's `menu`. */
  addSubmenu(text: TextLike, options?: SubmenuOptions): PluginSubmenu {
    const node = this.newNode(EntryTypes.Submenu, text, options);
    const title = options?.title ?? text;

    node.menu = {
      id: this.plugin.nextMenuId(),
      title: Text.toRef(title),
      subtitle: Text.toRef(options?.subtitle),
      items: [],
    };

    const menu = new PluginMenu(this.plugin, node.menu, title, options?.subtitle);

    this.plugin.registerMenu(menu);

    return this.attach(new PluginSubmenu(node, text, menu), options);
  }

  /**
   * Adds a key that works while this menu is open, with an instructional button at the bottom of the
   * screen. Keep the id stable: it names the binding in the player's key settings, so changing it
   * loses a key they picked themselves.
   * @param id Letters, digits and underscores, unique within your plugin.
   * @param text The instructional button's label.
   * @param defaultKey A keyboard key name as the game knows it, for example "X" or "F5".
   */
  addKey(id: string, text: TextLike, defaultKey: string, options?: KeyOptions): PluginKey {
    const node: KeyNode = {
      id,
      text: Text.toRef(text),
      description: Text.toRef(options?.description),
      defaultKey,
      defaultButton: options?.defaultButton,
      shadowedControl: options?.shadowedControl === undefined ? undefined : int(options.shadowedControl),
    };

    this.node.keys ??= [];
    this.node.keys.push(node);

    const key = new PluginKey(this.plugin, node, text);

    this.keyList.push(key);
    this.plugin.registerKey(key);
    this.plugin.emitOp({ op: UpdateOps.AddKeys, menuId: this.id, keys: [node] });

    return key;
  }

  /**
   * Rows added inside `add` go in at `index`, counted from 0, one after another, instead of at the
   * bottom. Adding goes back to the bottom once `add` returns.
   */
  insertAt(index: number, add: () => void): void {
    const previous = this.insertIndex;

    this.insertIndex = clamp(int(index), 0, this.itemList.length);

    try {
      add();
    } finally {
      this.insertIndex = previous;
    }
  }

  /** Moves a row of this menu to `index`, counted from 0. A submenu row keeps its menu, and the highlighted row stays highlighted. */
  move(item: PluginItem, index: number): void {
    const from = this.itemList.indexOf(item);

    if (from < 0) {
      return;
    }

    const to = clamp(int(index), 0, this.itemList.length - 1);

    if (to === from) {
      return;
    }

    this.itemList.splice(from, 1);
    this.itemList.splice(to, 0, item);

    this.node.items.splice(from, 1);
    this.node.items.splice(to, 0, item.node);

    const before = to + 1 < this.itemList.length ? this.itemList[to + 1]!.id : undefined;

    this.plugin.emitOp({ op: UpdateOps.MoveItem, itemId: item.id, beforeItemId: before });
  }

  /**
   * Shows only the rows `keep` answers true for. Rows added later are checked too: inside a batch
   * when the batch ends, so properties set right after adding count, otherwise as they come in. Call
   * it again after changing what it looks at.
   */
  filter(keep: (item: PluginItem) => boolean): void {
    this.filterFn = keep;
    this.plugin.filterChanged(this);
  }

  /** Shows every row again after `filter`. */
  clearFilter(): void {
    if (this.filterFn === undefined) {
      return;
    }

    this.filterFn = undefined;
    this.plugin.filterChanged(this);
  }

  /** Removes one row. For a submenu row, everything beneath it goes too. */
  remove(item: PluginItem): void {
    if (!this.removeLocal(item)) {
      return;
    }

    this.plugin.emitOp({ op: UpdateOps.RemoveItems, itemIds: [item.id] });
  }

  /** Removes every row. */
  clear(): void {
    for (const item of this.itemList) {
      this.plugin.unregisterItem(item);
    }

    this.itemList.length = 0;
    this.node.items.length = 0;

    this.plugin.emitOp({ op: UpdateOps.ClearMenu, menuId: this.id });
  }

  /** Opens this menu on screen, closing whatever vMenu menu was open. */
  open(): void {
    this.plugin.emitOp({ op: UpdateOps.OpenMenu, menuId: this.id });
  }

  /** Closes this plugin's menu if one is open. */
  close(): void {
    this.plugin.emitOp({ op: UpdateOps.CloseMenu, menuId: this.id });
  }

  /**
   * Moves the cursor to a row of this menu, as if the player had moved there. Does nothing for a
   * hidden row or one from another menu.
   */
  select(item: PluginItem): void {
    if (this.itemList.includes(item)) {
      this.plugin.emitOp({ op: UpdateOps.SelectItem, menuId: this.id, itemId: item.id });
    }
  }

  /** @internal */
  get hasFilter(): boolean {
    return this.filterFn !== undefined;
  }

  /** @internal */
  hides(item: PluginItem): boolean {
    return this.filterFn !== undefined && !this.filterFn(item);
  }

  /** @internal */
  filterOp(): UpdateOp {
    const keep = this.filterFn;

    if (keep === undefined) {
      return { op: UpdateOps.ClearFilter, menuId: this.id };
    }

    return { op: UpdateOps.SetFilter, menuId: this.id, itemIds: this.itemList.filter((item) => !keep(item)).map((item) => item.id) };
  }

  /** @internal */
  handleMenu(callback: PluginCallback): void {
    switch (callback.type) {
      case CallbackTypes.MenuOpened:
        this.opened.fire();
        break;

      case CallbackTypes.MenuClosed:
        this.closed.fire();
        break;

      case CallbackTypes.MenuIndexChanged:
        if (typeof callback.newIndex === 'number') {
          this.indexChanged.fire(callback.oldIndex ?? 0, callback.newIndex);
        }

        break;
    }
  }

  /** @internal */
  static asAdded(node: ItemNode): ItemNode {
    if (!node.menu) {
      return node;
    }

    const { items: _items, ...menu } = node.menu;

    return { ...node, menu: { ...menu, items: [] } };
  }

  private newNode(type: string, text: TextLike, options: ItemOptions | undefined): ItemNode {
    return { id: options?.id ?? this.plugin.nextItemId(), type, text: Text.toRef(text) };
  }

  private attach<T extends PluginItem>(item: T, options: ItemOptions | undefined): T {
    item.applyOptions(options);
    item.plugin = this.plugin;

    let before: string | undefined;

    if (this.insertIndex !== undefined && this.insertIndex < this.itemList.length) {
      before = this.itemList[this.insertIndex]!.id;

      this.itemList.splice(this.insertIndex, 0, item);
      this.node.items.splice(this.insertIndex, 0, item.node);

      this.insertIndex++;
    } else {
      this.itemList.push(item);
      this.node.items.push(item.node);

      if (this.insertIndex !== undefined) {
        this.insertIndex = this.itemList.length;
      }
    }

    this.plugin.registerItem(item);
    this.plugin.emitAdd(this, item, { op: UpdateOps.AddItems, menuId: this.id, items: [item.node], beforeItemId: before });

    return item;
  }

  private removeLocal(item: PluginItem): boolean {
    const index = this.itemList.lastIndexOf(item);

    if (index < 0) {
      return false;
    }

    this.itemList.splice(index, 1);

    const nodeIndex = this.node.items.lastIndexOf(item.node);

    if (nodeIndex >= 0) {
      this.node.items.splice(nodeIndex, 1);
    }

    this.plugin.unregisterItem(item);

    return true;
  }

  private subscribeMenuEvent(name: string): void {
    this.node.events ??= [];

    if (this.node.events.includes(name)) {
      return;
    }

    this.node.events.push(name);
    this.plugin.emitOp({ op: UpdateOps.SetMenuEvents, menuId: this.id, events: [...this.node.events] });
  }
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}
