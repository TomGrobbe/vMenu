import { UpdateOps, type KeyNode } from '../protocol.js';
import { Signal, type Unsubscribe } from '../signal.js';
import { Text, type TextLike } from '../text.js';
import { sameGate, sameText } from './diff.js';
import { Gate, type GateLike } from './gate.js';
import type { PluginHost } from './host.js';
import type { PluginItem } from './item.js';

/**
 * Which row the cursor was on when a key was pressed. At most one of the two is set, and neither
 * when the menu is empty or the row is not one of yours.
 */
export interface PluginKeyPress {
  /** The row under the cursor, when the player may use it. */
  readonly item?: PluginItem;
  /** The row under the cursor, when it is locked by its gate or disabled. Acting on it anyway is your call. */
  readonly disabledItem?: PluginItem;
}

/**
 * A key that works while its menu is open, shown as an instructional button at the bottom of the
 * screen. The player can rebind it in the game's key settings, where it is listed under vMenu with
 * your plugin's name in front.
 */
export class PluginKey {
  private textValue?: TextLike;

  private gateValue?: GateLike;

  private readonly pressed: Signal<[PluginKeyPress]>;

  /** @internal */
  readonly node: KeyNode;

  /** @internal */
  constructor(
    private readonly plugin: PluginHost,
    node: KeyNode,
    text: TextLike,
  ) {
    this.node = node;
    this.textValue = text;
    this.pressed = new Signal(() => plugin.resource, 'pressed');
  }

  /** The key's id, which also names the binding in the player's key settings. */
  get id(): string {
    return this.node.id;
  }

  /** The instructional button's label. Empty hides the button, the key still works. */
  get text(): TextLike | undefined {
    return this.textValue;
  }

  set text(value: TextLike | undefined) {
    this.textValue = value;

    const text = Text.toRef(value);

    if (sameText(this.node.text, text)) {
      return;
    }

    this.node.text = text;
    this.plugin.emitOp({ op: UpdateOps.SetKeyText, keyId: this.id, textValue: text });
  }

  /** A disabled key does nothing and shows no button. */
  get enabled(): boolean {
    return this.node.enabled !== false;
  }

  set enabled(value: boolean) {
    if (this.enabled === value) {
      return;
    }

    this.node.enabled = value;
    this.plugin.emitOp({ op: UpdateOps.SetKeyEnabled, keyId: this.id, flag: value });
  }

  /** While the gate fails the key does nothing and shows no button. */
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
    this.plugin.emitOp({ op: UpdateOps.SetKeyGate, keyId: this.id, gate });
  }

  /** Called when the player presses the key, with the row the cursor was on. */
  onPressed(handler: (press: PluginKeyPress) => void): Unsubscribe {
    return this.pressed.add(handler);
  }

  /** @internal */
  handle(press: PluginKeyPress): void {
    this.pressed.fire(press);
  }
}
