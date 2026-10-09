import { CallbackTypes, NodeEvents, UpdateOps, type ItemNode, type PluginCallback, type TextRef, type UpdateOp } from '../protocol.js';
import { Signal, type Unsubscribe } from '../signal.js';
import { Text, type TextLike } from '../text.js';
import { sameGate, sameText } from './diff.js';
import { Gate, type GateLike } from './gate.js';
import type { PluginHost } from './host.js';

/** Properties every row accepts when you add it. Setting them here sends them along with the row itself. */
export interface ItemOptions {
  /** A stable id of your own. Generated when left out. Unique within your plugin. */
  id?: string;
  description?: TextLike;
  label?: TextLike;
  lockedDescription?: TextLike;
  gate?: GateLike;
  hideWhenLocked?: boolean;
  visible?: boolean;
  enabled?: boolean;
  log?: boolean;
  leftIcon?: string;
  rightIcon?: string;
}

/**
 * One row in your plugin's menus. Setting a property updates the row live once the plugin is
 * connected, and everything set before connecting rides along with the registration.
 */
export abstract class PluginItem {
  private textValue?: TextLike;

  private descriptionValue?: TextLike;

  private labelValue?: TextLike;

  private lockedDescriptionValue?: TextLike;

  private gateValue?: GateLike;

  private readonly highlighted: Signal<[]>;

  /** @internal */
  plugin?: PluginHost;

  /** @internal */
  readonly node: ItemNode;

  /** @internal */
  constructor(node: ItemNode, text: TextLike) {
    this.node = node;
    this.textValue = text;
    this.highlighted = this.signal('highlighted');
  }

  /** The row's id, how vMenu refers to it. */
  get id(): string {
    return this.node.id;
  }

  /** The row's text. */
  get text(): TextLike | undefined {
    return this.textValue;
  }

  set text(value: TextLike | undefined) {
    this.textValue = value;
    this.setTextField('text', value, UpdateOps.SetText);
  }

  /** The line shown under the menu while the row is highlighted. */
  get description(): TextLike | undefined {
    return this.descriptionValue;
  }

  set description(value: TextLike | undefined) {
    this.descriptionValue = value;
    this.setTextField('description', value, UpdateOps.SetDescription);
  }

  /** Right aligned text. Ignored by rows whose label the menu draws itself. */
  get label(): TextLike | undefined {
    return this.labelValue;
  }

  set label(value: TextLike | undefined) {
    this.labelValue = value;
    this.setTextField('label', value, UpdateOps.SetLabel);
  }

  /** What the row says while its gate locks it. Empty uses vMenu's own wording. */
  get lockedDescription(): TextLike | undefined {
    return this.lockedDescriptionValue;
  }

  set lockedDescription(value: TextLike | undefined) {
    this.lockedDescriptionValue = value;
    this.setTextField('lockedDescription', value, UpdateOps.SetLockedDescription);
  }

  /** Decides whether the row is available, evaluated live by vMenu. */
  get gate(): GateLike | undefined {
    return this.gateValue;
  }

  set gate(value: GateLike | undefined) {
    this.gateValue = value;

    const gate = value === undefined ? undefined : Gate.toNode(value);

    if (sameGate(this.node.gate, gate)) {
      return;
    }

    this.node.gate = gate;
    this.emit({ op: UpdateOps.SetGate, itemId: this.id, gate });
  }

  /** What a failing gate does to the row: greyed out with a lock, or gone entirely. */
  get hideWhenLocked(): boolean {
    return this.node.behaviour?.toLowerCase() === 'hide';
  }

  set hideWhenLocked(value: boolean) {
    const behaviour = value ? 'hide' : 'lock';

    if (this.node.behaviour === behaviour) {
      return;
    }

    this.node.behaviour = behaviour;
    this.emit({ op: UpdateOps.SetBehaviour, itemId: this.id, value: behaviour });
  }

  /** Whether the row is shown at all. */
  get visible(): boolean {
    return this.node.visible !== false;
  }

  set visible(value: boolean) {
    if (this.visible === value) {
      return;
    }

    this.node.visible = value;
    this.emit({ op: UpdateOps.SetVisible, itemId: this.id, flag: value });
  }

  /** A disabled row is greyed out but still visible. Independent of the gate. */
  get enabled(): boolean {
    return this.node.enabled !== false;
  }

  set enabled(value: boolean) {
    if (this.enabled === value) {
      return;
    }

    this.node.enabled = value;
    this.emit({ op: UpdateOps.SetEnabled, itemId: this.id, flag: value });
  }

  /**
   * Ask vMenu to log use of this row to the server owner's webhook. Does nothing on its own: the
   * plugin's server half has to declare the same id with `addLoggedItem`.
   */
  get log(): boolean {
    return this.node.log === true;
  }

  set log(value: boolean) {
    if (this.log === value) {
      return;
    }

    this.node.log = value;
    this.emit({ op: UpdateOps.SetLog, itemId: this.id, flag: value });
  }

  /** Icon names from the vMenu icon set, for example "LOCK" or "STAR". */
  setIcons(leftIcon?: string, rightIcon?: string): void {
    if (this.node.leftIcon === leftIcon && this.node.rightIcon === rightIcon) {
      return;
    }

    this.node.leftIcon = leftIcon;
    this.node.rightIcon = rightIcon;
    this.emit({ op: UpdateOps.SetIcons, itemId: this.id, leftIcon, rightIcon });
  }

  /** Called while the player's cursor sits on this row. Chatty, subscribe deliberately. */
  onHighlighted(handler: () => void): Unsubscribe {
    const unsubscribe = this.highlighted.add(handler);

    this.subscribeNodeEvent(NodeEvents.Highlighted);

    return unsubscribe;
  }

  /** @internal */
  applyOptions(options: ItemOptions | undefined): void {
    if (!options) {
      return;
    }

    if (options.description !== undefined) this.description = options.description;
    if (options.label !== undefined) this.label = options.label;
    if (options.lockedDescription !== undefined) this.lockedDescription = options.lockedDescription;
    if (options.gate !== undefined) this.gate = options.gate;
    if (options.hideWhenLocked !== undefined) this.hideWhenLocked = options.hideWhenLocked;
    if (options.visible !== undefined) this.visible = options.visible;
    if (options.enabled !== undefined) this.enabled = options.enabled;
    if (options.log !== undefined) this.log = options.log;
    if (options.leftIcon !== undefined || options.rightIcon !== undefined) this.setIcons(options.leftIcon, options.rightIcon);
  }

  /** @internal */
  handle(callback: PluginCallback): void {
    if (callback.type === CallbackTypes.ItemHighlighted) {
      this.highlighted.fire();
    }
  }

  protected signal<T extends unknown[]>(name: string): Signal<T> {
    return new Signal<T>(() => this.plugin?.resource ?? '', name);
  }

  protected subscribeNodeEvent(name: string): void {
    this.node.events ??= [];

    if (this.node.events.includes(name)) {
      return;
    }

    this.node.events.push(name);
    this.emit({ op: UpdateOps.SetItemEvents, itemId: this.id, events: [...this.node.events] });
  }

  protected emit(op: UpdateOp): void {
    this.plugin?.emitOp(op);
  }

  protected setTextField(
    field: 'text' | 'description' | 'label' | 'lockedDescription' | 'confirmationDescription',
    value: TextLike | undefined,
    opName: string,
  ): void {
    const next: TextRef | undefined = Text.toRef(value);

    if (sameText(this.node[field], next)) {
      return;
    }

    this.node[field] = next;
    this.emit({ op: opName, itemId: this.id, textValue: next });
  }
}
