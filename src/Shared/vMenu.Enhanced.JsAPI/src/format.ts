export function formatFloat(value: number): string {
  const text = value.toFixed(4).replace(/0+$/, '');

  return text.endsWith('.') ? `${text}0` : text;
}

// vMenu reads these fields as whole numbers, and a decimal makes it drop the whole payload.
export function int(value: unknown): number {
  const number = Math.trunc(Number(value));

  return Number.isFinite(number) ? number : 0;
}
