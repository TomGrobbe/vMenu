import type { GateNode } from '../protocol.js';
import type { PluginBoolSetting } from './settings.js';

/** A gate, or a permission's short name as a shortcut for `Gate.permission(name)`. */
export type GateLike = Gate | string;

/**
 * Decides whether a menu item is available to the player, evaluated live by vMenu. Combine gates
 * with `and`, `or`, `Gate.all` and `Gate.any`. Names are short: vMenu scopes them to your plugin, so
 * you can never gate on another plugin's permissions or settings.
 */
export class Gate {
  private constructor(private readonly node: GateNode) {}

  /** One of the permissions your server side declared, by its short name. */
  static permission(shortName: string): Gate {
    return new Gate({ permission: shortName });
  }

  /** One of your bool settings: the item is available while the convar reads true. */
  static setting(setting: PluginBoolSetting | string): Gate {
    return new Gate({ setting: typeof setting === 'string' ? setting : setting.name });
  }

  /** Passes when every gate passes. */
  static all(...gates: GateLike[]): Gate {
    return new Gate({ all: gates.map(Gate.toNode) });
  }

  /** Passes when at least one gate passes. */
  static any(...gates: GateLike[]): Gate {
    return new Gate({ any: gates.map(Gate.toNode) });
  }

  /** Passes when this gate and the other one both pass. */
  and(other: GateLike): Gate {
    return Gate.all(this, other);
  }

  /** Passes when this gate or the other one passes. */
  or(other: GateLike): Gate {
    return Gate.any(this, other);
  }

  /** @internal */
  static toNode(gate: GateLike): GateNode {
    return typeof gate === 'string' ? { permission: gate } : gate.node;
  }
}
