import { CallbackTypes, UpdateOps, type ItemNode, type PluginCallback } from '../protocol.js';
import type { Unsubscribe } from '../signal.js';
import { Text, type TextLike } from '../text.js';
import { sameTexts } from './diff.js';
import { PluginItem } from './item.js';
import type { PluginMenu } from './menu.js';
import { writeBool } from './preferences.js';
import { int } from '../format.js';

/** A row the player presses. */
export class PluginButton extends PluginItem {
  private readonly selected = this.signal<[]>('selected');

  /** Called when the player presses the row. */
  onSelected(handler: () => void): Unsubscribe {
    return this.selected.add(handler);
  }

  /** @internal */
  override handle(callback: PluginCallback): void {
    super.handle(callback);

    if (callback.type === CallbackTypes.ItemSelected) {
      this.selected.fire();
    }
  }
}

/** A row that asks for a second press before it does anything. */
export class PluginConfirmButton extends PluginItem {
  private confirmationDescriptionValue?: TextLike;

  private readonly confirmed = this.signal<[]>('confirmed');

  /** What the row asks before its second press. Empty uses vMenu's own wording. */
  get confirmationDescription(): TextLike | undefined {
    return this.confirmationDescriptionValue;
  }

  set confirmationDescription(value: TextLike | undefined) {
    this.confirmationDescriptionValue = value;
    this.setTextField('confirmationDescription', value, UpdateOps.SetConfirmationDescription);
  }

  /** Called on the confirming second press, never on the first. */
  onConfirmed(handler: () => void): Unsubscribe {
    return this.confirmed.add(handler);
  }

  /** @internal */
  override handle(callback: PluginCallback): void {
    super.handle(callback);

    if (callback.type === CallbackTypes.Confirmed) {
      this.confirmed.fire();
    }
  }
}

/** A row with a tick box. */
export class PluginCheckbox extends PluginItem {
  private readonly changed = this.signal<[boolean]>('changed');

  /** @internal */
  persistedValue = false;

  /** Whether the state is saved in this resource's key value store and restored on start. */
  get persisted(): boolean {
    return this.persistedValue;
  }

  /** Whether the box is ticked. */
  get checked(): boolean {
    return this.node.checked === true;
  }

  set checked(value: boolean) {
    if (this.checked === value) {
      return;
    }

    this.node.checked = value;
    this.remember(value);
    this.emit({ op: UpdateOps.SetChecked, itemId: this.id, flag: value });
  }

  /** Called when the player toggles the box. The new state is already in `checked`. */
  onChanged(handler: (checked: boolean) => void): Unsubscribe {
    return this.changed.add(handler);
  }

  /** @internal */
  override handle(callback: PluginCallback): void {
    super.handle(callback);

    if (callback.type === CallbackTypes.CheckboxChanged && typeof callback.checked === 'boolean') {
      this.node.checked = callback.checked;
      this.remember(callback.checked);
      this.changed.fire(callback.checked);
    }
  }

  private remember(state: boolean): void {
    if (this.persistedValue) {
      writeBool(this.id, state);
    }
  }
}

/** A row the player scrolls through a set of options on. */
export class PluginList extends PluginItem {
  private readonly indexChanged = this.signal<[number, number]>('indexChanged');

  private readonly selected = this.signal<[number]>('selected');

  /** The selected option, counted from 0. */
  get selectedIndex(): number {
    return this.node.selectedIndex ?? 0;
  }

  set selectedIndex(value: number) {
    value = int(value);

    if (this.selectedIndex === value) {
      return;
    }

    this.node.selectedIndex = value;
    this.emit({ op: UpdateOps.SetSelectedIndex, itemId: this.id, index: value });
  }

  /** Replaces the options, optionally moving the selection at the same time. */
  setOptions(options: readonly TextLike[], selectedIndex?: number): void {
    const refs = Text.toRefs(options);

    selectedIndex = selectedIndex === undefined ? undefined : int(selectedIndex);

    if (sameTexts(this.node.options, refs) && (selectedIndex === undefined || selectedIndex === this.selectedIndex)) {
      return;
    }

    this.node.options = refs;

    if (selectedIndex !== undefined) {
      this.node.selectedIndex = selectedIndex;
    }

    this.emit({ op: UpdateOps.SetOptions, itemId: this.id, options: refs, index: selectedIndex });
  }

  /** Called when the player scrolls the value. The new index is already in `selectedIndex`. */
  onIndexChanged(handler: (oldIndex: number, newIndex: number) => void): Unsubscribe {
    return this.indexChanged.add(handler);
  }

  /** Called when the player presses the row, with the index they had selected. */
  onSelected(handler: (index: number) => void): Unsubscribe {
    return this.selected.add(handler);
  }

  /** @internal */
  override handle(callback: PluginCallback): void {
    super.handle(callback);

    switch (callback.type) {
      case CallbackTypes.ListIndexChanged:
        if (typeof callback.newIndex === 'number') {
          this.node.selectedIndex = callback.newIndex;
          this.indexChanged.fire(callback.oldIndex ?? 0, callback.newIndex);
        }

        break;

      case CallbackTypes.ListSelected:
        if (typeof callback.selectedIndex === 'number') {
          this.selected.fire(callback.selectedIndex);
        }

        break;

      case CallbackTypes.Confirmed:
        if (typeof callback.selectedIndex === 'number') {
          this.raiseConfirmed(callback.selectedIndex);
        }

        break;
    }
  }

  protected raiseConfirmed(_index: number): void {}
}

/** A list that asks for a second press before it does anything. */
export class PluginConfirmList extends PluginList {
  private confirmationDescriptionValue?: TextLike;

  private readonly confirmed = this.signal<[number]>('confirmed');

  /** What the row asks before its second press. Empty uses vMenu's own wording. */
  get confirmationDescription(): TextLike | undefined {
    return this.confirmationDescriptionValue;
  }

  set confirmationDescription(value: TextLike | undefined) {
    this.confirmationDescriptionValue = value;
    this.setTextField('confirmationDescription', value, UpdateOps.SetConfirmationDescription);
  }

  /** Called on the confirming second press, with the index that was confirmed. */
  onConfirmed(handler: (index: number) => void): Unsubscribe {
    return this.confirmed.add(handler);
  }

  protected override raiseConfirmed(index: number): void {
    this.confirmed.fire(index);
  }
}

/** A row with a bar the player drags between a minimum and a maximum. */
export class PluginSlider extends PluginItem {
  private readonly moved = this.signal<[number, number]>('moved');

  private readonly selected = this.signal<[number]>('selected');

  /** The lowest position. */
  get min(): number {
    return this.node.min ?? 0;
  }

  /** The highest position. */
  get max(): number {
    return this.node.max ?? 0;
  }

  /** Where the bar sits. */
  get position(): number {
    return this.node.position ?? this.min;
  }

  set position(value: number) {
    value = int(value);

    if (this.position === value) {
      return;
    }

    this.node.position = value;
    this.emit({ op: UpdateOps.SetSliderPosition, itemId: this.id, index: value });
  }

  /** Called while the player drags the bar. The new position is already in `position`. */
  onMoved(handler: (oldPosition: number, newPosition: number) => void): Unsubscribe {
    return this.moved.add(handler);
  }

  /** Called when the player presses the row, with the position it sat at. */
  onSelected(handler: (position: number) => void): Unsubscribe {
    return this.selected.add(handler);
  }

  /** @internal */
  override handle(callback: PluginCallback): void {
    super.handle(callback);

    switch (callback.type) {
      case CallbackTypes.SliderMoved:
        if (typeof callback.newPosition === 'number') {
          this.node.position = callback.newPosition;
          this.moved.fire(callback.oldPosition ?? 0, callback.newPosition);
        }

        break;

      case CallbackTypes.SliderSelected:
        if (typeof callback.position === 'number') {
          this.selected.fire(callback.position);
        }

        break;
    }
  }
}

/** A row whose value your code works out each time the player scrolls it. */
export class PluginDynamicList extends PluginItem {
  private readonly selected = this.signal<[string]>('selected');

  /**
   * Produces the next value when the player scrolls: the current value and whether they went left.
   * The answer crosses back to vMenu as an update, so it lands one beat after the press.
   */
  changeRequested?: (currentValue: string, left: boolean) => string | null | undefined;

  /** The value the row shows. */
  get value(): string {
    return this.node.value ?? '';
  }

  set value(value: string) {
    value = String(value);

    if (this.value === value) {
      return;
    }

    this.node.value = value;
    this.emit({ op: UpdateOps.SetValue, itemId: this.id, value });
  }

  /** Called when the player presses the row, with the value it showed. */
  onSelected(handler: (value: string) => void): Unsubscribe {
    return this.selected.add(handler);
  }

  /** @internal */
  override handle(callback: PluginCallback): void {
    super.handle(callback);

    switch (callback.type) {
      case CallbackTypes.DynamicChanging:
        if (typeof callback.left === 'boolean') {
          const next = this.changeRequested?.(callback.currentValue ?? '', callback.left);

          if (next !== null && next !== undefined) {
            this.value = next;
          }
        }

        break;

      case CallbackTypes.DynamicSelected:
        this.selected.fire(callback.value ?? '');
        break;
    }
  }
}

/** A row that only shows text, used to split a menu into sections. */
export class PluginSeparator extends PluginItem {}

/** A row that opens another menu. */
export class PluginSubmenu extends PluginItem {
  /** @internal */
  constructor(node: ItemNode, text: TextLike, menu: PluginMenu) {
    super(node, text);
    this.menu = menu;
  }

  /** The menu this row opens. Add its rows through this. */
  readonly menu: PluginMenu;
}
