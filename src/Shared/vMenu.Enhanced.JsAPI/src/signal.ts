import { log } from './fivem.js';

/** Call it to stop receiving the event you subscribed to. */
export type Unsubscribe = () => void;

export class Signal<T extends unknown[]> {
  private handlers: ((...args: T) => void)[] = [];

  constructor(
    private readonly resource: () => string,
    private readonly name: string,
  ) {}

  get hasHandlers(): boolean {
    return this.handlers.length > 0;
  }

  add(handler: (...args: T) => void): Unsubscribe {
    this.handlers.push(handler);

    return () => {
      this.handlers = this.handlers.filter((existing) => existing !== handler);
    };
  }

  fire(...args: T): void {
    for (const handler of [...this.handlers]) {
      try {
        handler(...args);
      } catch (error) {
        log.error(this.resource(), `A ${this.name} handler threw: ${describe(error)}`);
      }
    }
  }
}

export function describe(error: unknown): string {
  return error instanceof Error ? (error.stack ?? error.message) : String(error);
}
