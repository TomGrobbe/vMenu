import { CallbackTypes, EntryTypes, UpdateOps, type ItemNode, type PluginCallback } from '../protocol.js';
import type { Unsubscribe } from '../signal.js';
import { Text, type TextLike } from '../text.js';
import type { PluginHost } from './host.js';
import { PluginItem, type ItemOptions } from './item.js';
import { PluginSeparator } from './items.js';
import { int } from '../format.js';

/**
 * The player a player action was used on, read from vMenu's Online Players snapshot. The snapshot
 * can be stale and the player may have left, so your server side must check the target still exists
 * before acting on it.
 */
export interface PlayerTarget {
  /** The player's server id. */
  readonly serverId: number;
  /** The player's name as vMenu showed it. */
  readonly name: string;
}

function target(callback: PluginCallback): PlayerTarget | undefined {
  return typeof callback.targetServerId === 'number'
    ? { serverId: callback.targetServerId, name: callback.targetName ?? '' }
    : undefined;
}

/** A player action the player presses. */
export class PluginPlayerButton extends PluginItem {
  private readonly selected = this.signal<[PlayerTarget]>('selected');

  /** Called when the action is used on a player, with that player as the target. */
  onSelected(handler: (target: PlayerTarget) => void): Unsubscribe {
    return this.selected.add(handler);
  }

  /** @internal */
  override handle(callback: PluginCallback): void {
    super.handle(callback);

    const player = target(callback);

    if (callback.type === CallbackTypes.PlayerActionSelected && player) {
      this.selected.fire(player);
    }
  }
}

/** A player action that asks for a second press before it does anything. */
export class PluginPlayerConfirmButton extends PluginItem {
  private confirmationDescriptionValue?: TextLike;

  private readonly confirmed = this.signal<[PlayerTarget]>('confirmed');

  /** What the row asks before its second press. Empty uses vMenu's own wording. */
  get confirmationDescription(): TextLike | undefined {
    return this.confirmationDescriptionValue;
  }

  set confirmationDescription(value: TextLike | undefined) {
    this.confirmationDescriptionValue = value;
    this.node.confirmationDescription = Text.toRef(value);
    this.emit({
      op: UpdateOps.SetConfirmationDescription,
      itemId: this.id,
      textValue: this.node.confirmationDescription,
    });
  }

  /** Called on the confirming second press, with the targeted player. */
  onConfirmed(handler: (target: PlayerTarget) => void): Unsubscribe {
    return this.confirmed.add(handler);
  }

  /** @internal */
  override handle(callback: PluginCallback): void {
    super.handle(callback);

    const player = target(callback);

    if (callback.type === CallbackTypes.PlayerActionConfirmed && player) {
      this.confirmed.fire(player);
    }
  }
}

/** A player action with a set of options. */
export class PluginPlayerList extends PluginItem {
  private readonly selected = this.signal<[PlayerTarget, number]>('selected');

  /** The current selection, counted from 0. Shared across every player the menu shows, since the same rows serve them all. */
  get selectedIndex(): number {
    return this.node.selectedIndex ?? 0;
  }

  set selectedIndex(value: number) {
    this.node.selectedIndex = int(value);
    this.emit({ op: UpdateOps.SetSelectedIndex, itemId: this.id, index: this.node.selectedIndex });
  }

  /** Replaces the options, optionally moving the selection at the same time. */
  setOptions(options: readonly TextLike[], selectedIndex?: number): void {
    this.node.options = Text.toRefs(options);
    selectedIndex = selectedIndex === undefined ? undefined : int(selectedIndex);

    if (selectedIndex !== undefined) {
      this.node.selectedIndex = selectedIndex;
    }

    this.emit({ op: UpdateOps.SetOptions, itemId: this.id, options: this.node.options, index: selectedIndex });
  }

  /** Called when the action is used on a player, with the target and the chosen index. */
  onSelected(handler: (target: PlayerTarget, index: number) => void): Unsubscribe {
    return this.selected.add(handler);
  }

  /** @internal */
  override handle(callback: PluginCallback): void {
    super.handle(callback);

    const player = target(callback);

    if (callback.type === CallbackTypes.PlayerActionListSelected && player) {
      if (typeof callback.selectedIndex === 'number') {
        this.node.selectedIndex = callback.selectedIndex;
      }

      this.selected.fire(player, callback.selectedIndex ?? 0);
    }
  }
}

/**
 * Actions vMenu adds to the "Plugin Actions" submenu of every player's entry in its Online Players
 * menu. The same rows serve every player, and the target is handed to your handler when one fires.
 */
export class PluginPlayerActions {
  private readonly itemList: PluginItem[] = [];

  /** @internal */
  readonly nodes: ItemNode[] = [];

  /** @internal */
  constructor(private readonly plugin: PluginHost) {}

  /** Every action, in order. */
  get items(): readonly PluginItem[] {
    return this.itemList;
  }

  /** Adds an action the player presses. */
  addButton(text: TextLike, options?: ItemOptions): PluginPlayerButton {
    return this.attach(new PluginPlayerButton(this.newNode(EntryTypes.Button, text, options), text), options);
  }

  /** Adds an action that asks for a second press. */
  addConfirmButton(
    text: TextLike,
    options?: ItemOptions & { confirmationDescription?: TextLike },
  ): PluginPlayerConfirmButton {
    const item = new PluginPlayerConfirmButton(this.newNode(EntryTypes.ConfirmButton, text, options), text);

    if (options?.confirmationDescription !== undefined) {
      item.confirmationDescription = options.confirmationDescription;
    }

    return this.attach(item, options);
  }

  /** Adds an action with a set of options. */
  addList(text: TextLike, values: readonly TextLike[], options?: ItemOptions & { selectedIndex?: number }): PluginPlayerList {
    const node = this.newNode(EntryTypes.List, text, options);

    node.options = Text.toRefs(values);
    node.selectedIndex = int(options?.selectedIndex ?? 0);

    return this.attach(new PluginPlayerList(node, text), options);
  }

  /** Adds a row that only shows text. */
  addSeparator(text: TextLike, options?: ItemOptions): PluginSeparator {
    return this.attach(new PluginSeparator(this.newNode(EntryTypes.Separator, text, options), text), options);
  }

  /** Removes one action from every player's entry. */
  remove(item: PluginItem): void {
    const index = this.itemList.indexOf(item);

    if (index < 0) {
      return;
    }

    this.itemList.splice(index, 1);

    const nodeIndex = this.nodes.indexOf(item.node);

    if (nodeIndex >= 0) {
      this.nodes.splice(nodeIndex, 1);
    }

    this.plugin.unregisterItem(item);
    this.plugin.emitOp({ op: UpdateOps.RemoveItems, itemIds: [item.id] });
  }

  private newNode(type: string, text: TextLike, options: ItemOptions | undefined): ItemNode {
    return { id: options?.id ?? this.plugin.nextItemId(), type, text: Text.toRef(text) };
  }

  private attach<T extends PluginItem>(item: T, options: ItemOptions | undefined): T {
    item.applyOptions(options);
    item.plugin = this.plugin;

    this.itemList.push(item);
    this.nodes.push(item.node);

    this.plugin.registerItem(item);
    this.plugin.emitOp({ op: UpdateOps.AddPlayerActions, items: [item.node] });

    return item;
  }
}
