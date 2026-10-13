import type { TextRef } from './protocol.js';

/** Anything that can be shown as text: a plain string is a literal, a {@link Text} can also be a translation key. */
export type TextLike = string | Text;

/**
 * A piece of display text, either a literal or a key into your plugin's translation tables. A plain
 * string is a literal, so translating is always deliberate. vMenu resolves keys against the tables
 * you registered, following its selected language with your English table as the fallback.
 */
export class Text {
  private constructor(
    private readonly value: string,
    private readonly isKey: boolean,
    private readonly args?: Readonly<Record<string, TextLike>>,
  ) {}

  /** Text shown exactly as written, never translated. */
  static literal(text: string): Text {
    return new Text(text, false);
  }

  /**
   * A translation key, optionally with named placeholder values. In the translated string,
   * `{name}` is replaced with the matching value, which can itself be a literal or another key.
   */
  static key(key: string, args?: Record<string, TextLike>): Text {
    return new Text(key, true, args);
  }

  /** @internal */
  static toRef(text: TextLike | null | undefined): TextRef | undefined {
    if (text === null || text === undefined) {
      return undefined;
    }

    if (!(text instanceof Text)) {
      return { text: String(text) };
    }

    if (!text.isKey) {
      return { text: text.value };
    }

    const reference: TextRef = { key: text.value };

    if (text.args) {
      const args: Record<string, TextRef> = {};
      let any = false;

      for (const [name, value] of Object.entries(text.args)) {
        const argument = Text.toRef(value);

        if (argument) {
          args[name] = argument;
          any = true;
        }
      }

      if (any) {
        reference.args = args;
      }
    }

    return reference;
  }

  /** @internal */
  static toRefs(options: readonly TextLike[]): TextRef[] {
    const refs: TextRef[] = [];

    for (const option of options) {
      const reference = Text.toRef(option);

      if (reference) {
        refs.push(reference);
      }
    }

    return refs;
  }
}
