export const PROTOCOL_VERSION = 2;

export const SERVER_PROTOCOL_VERSION = 1;

export const VMENU_RESOURCE = 'vMenu.Enhanced';

export const PluginEvents = {
  Probe: 'vMenu.Enhanced:Plugins:Probe',
  Register: 'vMenu.Enhanced:Plugins:Register',
  Unregister: 'vMenu.Enhanced:Plugins:Unregister',
  Update: 'vMenu.Enhanced:Plugins:Update',
  Notify: 'vMenu.Enhanced:Plugins:Notify',
  Prompt: 'vMenu.Enhanced:Plugins:Prompt',
  SetTheme: 'vMenu.Enhanced:Plugins:SetTheme',
  RegisterThemes: 'vMenu.Enhanced:Plugins:RegisterThemes',
  Ready: 'vMenu.Enhanced:Plugins:Ready',
  ServerProbe: 'vMenu.Enhanced:Plugins:Server:Probe',
  ServerRegister: 'vMenu.Enhanced:Plugins:Server:Register',
  ServerDenied: 'vMenu.Enhanced:Plugins:Server:Denied',
  ServerReady: 'vMenu.Enhanced:Plugins:Server:Ready',
  readyFor: (resource: string) => `vMenu.Enhanced:Plugins:${resource}:Ready`,
  registerResultFor: (resource: string) => `vMenu.Enhanced:Plugins:${resource}:RegisterResult`,
  eventFor: (resource: string) => `vMenu.Enhanced:Plugins:${resource}:Event`,
  promptResultFor: (resource: string) => `vMenu.Enhanced:Plugins:${resource}:PromptResult`,
  themesFor: (resource: string) => `vMenu.Enhanced:Plugins:${resource}:Themes`,
  themesRegisteredFor: (resource: string) => `vMenu.Enhanced:Plugins:${resource}:ThemesRegistered`,
  serverReadyFor: (resource: string) => `vMenu.Enhanced:Plugins:${resource}:Server:Ready`,
  serverRegisterResultFor: (resource: string) => `vMenu.Enhanced:Plugins:${resource}:Server:RegisterResult`,
} as const;

export const RefreshPermissionsEvent = 'vMenu.Enhanced:Permissions:Refresh';

export const EntryTypes = {
  Button: 'button',
  Checkbox: 'checkbox',
  List: 'list',
  Slider: 'slider',
  DynamicList: 'dynamicList',
  Submenu: 'submenu',
  Separator: 'separator',
  ConfirmButton: 'confirmButton',
  ConfirmList: 'confirmList',
} as const;

export const NodeEvents = {
  Opened: 'opened',
  Closed: 'closed',
  IndexChanged: 'indexChanged',
  Highlighted: 'highlighted',
} as const;

export const SettingTypes = {
  Bool: 'bool',
  Int: 'int',
  Float: 'float',
  String: 'string',
} as const;

export const CallbackTypes = {
  ItemSelected: 'itemSelected',
  CheckboxChanged: 'checkboxChanged',
  ListIndexChanged: 'listIndexChanged',
  ListSelected: 'listSelected',
  SliderMoved: 'sliderMoved',
  SliderSelected: 'sliderSelected',
  DynamicSelected: 'dynamicSelected',
  DynamicChanging: 'dynamicChanging',
  Confirmed: 'confirmed',
  ItemHighlighted: 'itemHighlighted',
  MenuOpened: 'menuOpened',
  MenuClosed: 'menuClosed',
  MenuIndexChanged: 'menuIndexChanged',
  PlayerActionSelected: 'playerActionSelected',
  PlayerActionConfirmed: 'playerActionConfirmed',
  PlayerActionListSelected: 'playerActionListSelected',
  KeyPressed: 'keyPressed',
} as const;

export const UpdateOps = {
  SetText: 'setText',
  SetDescription: 'setDescription',
  SetLabel: 'setLabel',
  SetLockedDescription: 'setLockedDescription',
  SetConfirmationDescription: 'setConfirmationDescription',
  SetIcons: 'setIcons',
  SetChecked: 'setChecked',
  SetOptions: 'setOptions',
  SetSelectedIndex: 'setSelectedIndex',
  SetSliderPosition: 'setSliderPosition',
  SetValue: 'setValue',
  SetVisible: 'setVisible',
  SetEnabled: 'setEnabled',
  SetGate: 'setGate',
  SetLog: 'setLog',
  SetBehaviour: 'setBehaviour',
  SetItemEvents: 'setItemEvents',
  AddItems: 'addItems',
  RemoveItems: 'removeItems',
  ClearMenu: 'clearMenu',
  MoveItem: 'moveItem',
  AddPlayerActions: 'addPlayerActions',
  SetMenuTitle: 'setMenuTitle',
  SetMenuSubtitle: 'setMenuSubtitle',
  OpenMenu: 'openMenu',
  CloseMenu: 'closeMenu',
  SelectItem: 'selectItem',
  SetMenuEvents: 'setMenuEvents',
  SetFilter: 'setFilter',
  ClearFilter: 'clearFilter',
  AddKeys: 'addKeys',
  SetKeyText: 'setKeyText',
  SetKeyEnabled: 'setKeyEnabled',
  SetKeyGate: 'setKeyGate',
  MergeTranslations: 'mergeTranslations',
} as const;

export interface TextRef {
  text?: string;
  key?: string;
  args?: Record<string, TextRef>;
}

export interface GateNode {
  permission?: string;
  setting?: string;
  all?: GateNode[];
  any?: GateNode[];
}

export interface ItemNode {
  id: string;
  type: string;
  text?: TextRef;
  description?: TextRef;
  label?: TextRef;
  lockedDescription?: TextRef;
  gate?: GateNode;
  behaviour?: string;
  leftIcon?: string;
  rightIcon?: string;
  events?: string[];
  visible?: boolean;
  enabled?: boolean;
  log?: boolean;
  checked?: boolean;
  checkStyle?: string;
  options?: TextRef[];
  selectedIndex?: number;
  min?: number;
  max?: number;
  position?: number;
  showDivider?: boolean;
  value?: string;
  confirmationDescription?: TextRef;
  menu?: MenuNode;
}

export interface KeyNode {
  id: string;
  text?: TextRef;
  description?: TextRef;
  defaultKey: string;
  defaultButton?: string;
  shadowedControl?: number;
  gate?: GateNode;
  enabled?: boolean;
}

export interface MenuNode {
  id: string;
  title?: TextRef;
  subtitle?: TextRef;
  events?: string[];
  items: ItemNode[];
  keys?: KeyNode[];
}

export interface SettingNode {
  name: string;
  type: string;
  default: string;
  description: string;
}

export interface UpdateOp {
  op: string;
  itemId?: string;
  menuId?: string;
  keyId?: string;
  textValue?: TextRef;
  leftIcon?: string;
  rightIcon?: string;
  flag?: boolean;
  index?: number;
  value?: string;
  options?: TextRef[];
  gate?: GateNode;
  items?: ItemNode[];
  itemIds?: string[];
  beforeItemId?: string;
  events?: string[];
  keys?: KeyNode[];
  language?: string;
  entries?: Record<string, string>;
}

export interface PluginCallback {
  type: string;
  menuId?: string;
  itemId?: string;
  keyId?: string;
  disabledItemId?: string;
  checked?: boolean;
  oldIndex?: number;
  newIndex?: number;
  selectedIndex?: number;
  oldPosition?: number;
  newPosition?: number;
  position?: number;
  value?: string;
  currentValue?: string;
  left?: boolean;
  targetServerId?: number;
  targetName?: string;
}

export interface RegisterRequest {
  protocolVersion: number;
  displayName?: TextRef;
  descriptionKey?: string;
  translations?: Record<string, Record<string, string>>;
  settings?: SettingNode[];
  menu?: MenuNode;
  playerActions?: ItemNode[];
}

/** vMenu's answer to a registration. Errors mean it was refused, warnings mean it was accepted with parts skipped. */
export interface RegisterResult {
  /** Whether vMenu took the registration. */
  accepted: boolean;
  /** The protocol version vMenu speaks. */
  protocolVersion: number;
  /** Why the registration was refused. */
  errors: string[];
  /** What vMenu skipped while accepting it. */
  warnings: string[];
}

export interface PromptRequest {
  requestId: number;
  prompts: {
    title?: TextRef;
    maxLength: number;
    initial: string;
    suggestions?: { value: string; description?: string }[];
  }[];
}

export interface PromptResult {
  requestId: number;
  cancelled: boolean;
  busy: boolean;
  answers?: string[];
}

export interface ThemeList {
  themes?: { id: string; name: string }[];
  current?: string;
  configured?: string;
  overridden?: boolean;
}

export interface ServerRegisterRequest {
  protocolVersion: number;
  displayName: string;
  permissions?: { name: string; description: string; staffOnly: boolean }[];
  settings?: SettingNode[];
  loggedItems?: { itemId: string; description: string }[];
}

export function normalizeResult(raw: Partial<RegisterResult> | undefined): RegisterResult | undefined {
  if (!raw || typeof raw !== 'object') {
    return undefined;
  }

  return {
    accepted: raw.accepted === true,
    protocolVersion: typeof raw.protocolVersion === 'number' ? raw.protocolVersion : 0,
    errors: Array.isArray(raw.errors) ? raw.errors : [],
    warnings: Array.isArray(raw.warnings) ? raw.warnings : [],
  };
}
