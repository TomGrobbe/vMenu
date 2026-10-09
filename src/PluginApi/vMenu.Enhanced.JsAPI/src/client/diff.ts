import type { GateNode, TextRef } from '../protocol.js';

export function sameText(left: TextRef | undefined, right: TextRef | undefined): boolean {
  if (left === right) {
    return true;
  }

  if (!left || !right) {
    return false;
  }

  return left.text === right.text && left.key === right.key && sameArgs(left.args, right.args);
}

export function sameTexts(left: TextRef[] | undefined, right: TextRef[] | undefined): boolean {
  if (left === right) {
    return true;
  }

  if (!left || !right || left.length !== right.length) {
    return false;
  }

  return left.every((value, index) => sameText(value, right[index]));
}

export function sameGate(left: GateNode | undefined, right: GateNode | undefined): boolean {
  if (left === right) {
    return true;
  }

  if (!left || !right) {
    return false;
  }

  return (
    left.permission === right.permission &&
    left.setting === right.setting &&
    sameGates(left.all, right.all) &&
    sameGates(left.any, right.any)
  );
}

function sameGates(left: GateNode[] | undefined, right: GateNode[] | undefined): boolean {
  if (left === right) {
    return true;
  }

  if (!left || !right || left.length !== right.length) {
    return false;
  }

  return left.every((value, index) => sameGate(value, right[index]));
}

function sameArgs(left: Record<string, TextRef> | undefined, right: Record<string, TextRef> | undefined): boolean {
  if (left === right) {
    return true;
  }

  if (!left || !right) {
    return false;
  }

  const keys = Object.keys(left);

  if (keys.length !== Object.keys(right).length) {
    return false;
  }

  return keys.every((key) => key in right && sameText(left[key], right[key]));
}
