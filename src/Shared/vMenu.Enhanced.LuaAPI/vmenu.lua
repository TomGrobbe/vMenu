-- vMenu Enhanced plugin API for Lua, built for vMenu Enhanced vversiongoeshere.
-- Copy this file into your resource and load it as a shared_script before your own scripts, see https://docs.vespura.com/vmenu/enhanced/plugins/lua/

local PROTOCOL_VERSION = 2
local SERVER_PROTOCOL_VERSION = 1
local VMENU_RESOURCE = 'vMenu.Enhanced'

local Events = {
    Probe = 'vMenu.Enhanced:Plugins:Probe',
    Register = 'vMenu.Enhanced:Plugins:Register',
    Unregister = 'vMenu.Enhanced:Plugins:Unregister',
    Update = 'vMenu.Enhanced:Plugins:Update',
    Notify = 'vMenu.Enhanced:Plugins:Notify',
    Prompt = 'vMenu.Enhanced:Plugins:Prompt',
    SetTheme = 'vMenu.Enhanced:Plugins:SetTheme',
    RegisterThemes = 'vMenu.Enhanced:Plugins:RegisterThemes',
    Ready = 'vMenu.Enhanced:Plugins:Ready',
    ServerProbe = 'vMenu.Enhanced:Plugins:Server:Probe',
    ServerRegister = 'vMenu.Enhanced:Plugins:Server:Register',
    ServerDenied = 'vMenu.Enhanced:Plugins:Server:Denied',
    ServerReady = 'vMenu.Enhanced:Plugins:Server:Ready',
    ReadyFor = function(resource) return 'vMenu.Enhanced:Plugins:' .. resource .. ':Ready' end,
    RegisterResultFor = function(resource) return 'vMenu.Enhanced:Plugins:' .. resource .. ':RegisterResult' end,
    EventFor = function(resource) return 'vMenu.Enhanced:Plugins:' .. resource .. ':Event' end,
    PromptResultFor = function(resource) return 'vMenu.Enhanced:Plugins:' .. resource .. ':PromptResult' end,
    ThemesFor = function(resource) return 'vMenu.Enhanced:Plugins:' .. resource .. ':Themes' end,
    ThemesRegisteredFor = function(resource) return 'vMenu.Enhanced:Plugins:' .. resource .. ':ThemesRegistered' end,
    ServerReadyFor = function(resource) return 'vMenu.Enhanced:Plugins:' .. resource .. ':Server:Ready' end,
    ServerRegisterResultFor = function(resource) return 'vMenu.Enhanced:Plugins:' .. resource .. ':Server:RegisterResult' end,
}

local RefreshPermissionsEvent = 'vMenu.Enhanced:Permissions:Refresh'

local EntryTypes = {
    Button = 'button',
    Checkbox = 'checkbox',
    List = 'list',
    Slider = 'slider',
    DynamicList = 'dynamicList',
    Submenu = 'submenu',
    Separator = 'separator',
    ConfirmButton = 'confirmButton',
    ConfirmList = 'confirmList',
}

local NodeEvents = {
    Opened = 'opened',
    Closed = 'closed',
    IndexChanged = 'indexChanged',
    Highlighted = 'highlighted',
}

local SettingTypes = {
    Bool = 'bool',
    Int = 'int',
    Float = 'float',
    String = 'string',
}

local CallbackTypes = {
    ItemSelected = 'itemSelected',
    CheckboxChanged = 'checkboxChanged',
    ListIndexChanged = 'listIndexChanged',
    ListSelected = 'listSelected',
    SliderMoved = 'sliderMoved',
    SliderSelected = 'sliderSelected',
    DynamicSelected = 'dynamicSelected',
    DynamicChanging = 'dynamicChanging',
    Confirmed = 'confirmed',
    ItemHighlighted = 'itemHighlighted',
    MenuOpened = 'menuOpened',
    MenuClosed = 'menuClosed',
    MenuIndexChanged = 'menuIndexChanged',
    PlayerActionSelected = 'playerActionSelected',
    PlayerActionConfirmed = 'playerActionConfirmed',
    PlayerActionListSelected = 'playerActionListSelected',
    KeyPressed = 'keyPressed',
}

local UpdateOps = {
    SetText = 'setText',
    SetDescription = 'setDescription',
    SetLabel = 'setLabel',
    SetLockedDescription = 'setLockedDescription',
    SetConfirmationDescription = 'setConfirmationDescription',
    SetIcons = 'setIcons',
    SetChecked = 'setChecked',
    SetOptions = 'setOptions',
    SetSelectedIndex = 'setSelectedIndex',
    SetSliderPosition = 'setSliderPosition',
    SetValue = 'setValue',
    SetVisible = 'setVisible',
    SetEnabled = 'setEnabled',
    SetGate = 'setGate',
    SetLog = 'setLog',
    SetBehaviour = 'setBehaviour',
    SetItemEvents = 'setItemEvents',
    AddItems = 'addItems',
    RemoveItems = 'removeItems',
    ClearMenu = 'clearMenu',
    MoveItem = 'moveItem',
    AddPlayerActions = 'addPlayerActions',
    SetMenuTitle = 'setMenuTitle',
    SetMenuSubtitle = 'setMenuSubtitle',
    OpenMenu = 'openMenu',
    CloseMenu = 'closeMenu',
    SelectItem = 'selectItem',
    SetMenuEvents = 'setMenuEvents',
    SetFilter = 'setFilter',
    ClearFilter = 'clearFilter',
    AddKeys = 'addKeys',
    SetKeyText = 'setKeyText',
    SetKeyEnabled = 'setKeyEnabled',
    SetKeyGate = 'setKeyGate',
    MergeTranslations = 'mergeTranslations',
}

local resourceName = GetCurrentResourceName()

local function log(color, message)
    print(('%s[%s] %s^7'):format(color, resourceName, message))
end

local function warn(message) log('^3', message) end

local function fail(message) log('^1', message) end

local function traceback(err)
    if debug and debug.traceback then
        return debug.traceback(tostring(err), 2)
    end

    return tostring(err)
end

local function safeCall(what, handler, ...)
    local ok, err = xpcall(handler, traceback, ...)

    if not ok then
        fail(('A %s handler threw: %s'):format(what, err))
    end
end

local function sanitizeId(resource)
    return (resource:gsub('[^%w]', '_'))
end

local function str(value)
    if type(value) == 'string' then return value end
    return nil
end

local function num(value)
    if type(value) == 'number' then return math.tointeger(value) or value end
    return nil
end

local function int(value)
    return math.tointeger(math.floor((tonumber(value) or 0) + 0.0))
end

local function shallowCopy(source)
    local copy = {}

    for key, value in pairs(source) do
        copy[key] = value
    end

    return copy
end

local function indexOf(list, value)
    for index = 1, #list do
        if rawequal(list[index], value) then return index end
    end

    return nil
end

local function lastIndexOf(list, value)
    for index = #list, 1, -1 do
        if rawequal(list[index], value) then return index end
    end

    return nil
end

local function clamp(value, low, high)
    return math.min(math.max(value, low), high)
end

local Array = {}

local function array(values)
    return setmetatable(values or {}, Array)
end

local escapes = { ['"'] = '\\"', ['\\'] = '\\\\', ['\b'] = '\\b', ['\f'] = '\\f', ['\n'] = '\\n', ['\r'] = '\\r', ['\t'] = '\\t' }

local function quote(text)
    return '"' .. text:gsub('[%c"\\]', function(char)
        return escapes[char] or ('\\u%04x'):format(char:byte())
    end) .. '"'
end

local encode

local function isArray(value)
    return getmetatable(value) == Array or rawget(value, 1) ~= nil
end

encode = function(value)
    local kind = type(value)

    if kind == 'nil' then
        return 'null'
    elseif kind == 'boolean' then
        return value and 'true' or 'false'
    elseif kind == 'number' then
        if math.type(value) == 'integer' then return tostring(value) end
        if value ~= value or value == math.huge or value == -math.huge then return 'null' end
        local whole = math.tointeger(value)
        if whole then return tostring(whole) end
        return ('%.17g'):format(value)
    elseif kind == 'string' then
        return quote(value)
    elseif kind == 'table' then
        local parts = {}

        if isArray(value) then
            for index = 1, #value do
                parts[index] = encode(value[index])
            end

            return '[' .. table.concat(parts, ',') .. ']'
        end

        for key, field in pairs(value) do
            if type(key) == 'string' and type(field) ~= 'function' then
                parts[#parts + 1] = quote(key) .. ':' .. encode(field)
            end
        end

        return '{' .. table.concat(parts, ',') .. '}'
    end

    return 'null'
end

local function decode(text)
    if type(text) ~= 'string' then return nil end

    local ok, value = pcall(json.decode, text)

    if ok and type(value) == 'table' then return value end

    return nil
end

local function normalizeResult(raw)
    if type(raw) ~= 'table' then return nil end

    local result = { Accepted = raw.accepted == true, ProtocolVersion = num(raw.protocolVersion) or 0, Errors = {}, Warnings = {} }

    if type(raw.errors) == 'table' then
        for _, error in ipairs(raw.errors) do
            if type(error) == 'string' then result.Errors[#result.Errors + 1] = error end
        end
    end

    if type(raw.warnings) == 'table' then
        for _, warning in ipairs(raw.warnings) do
            if type(warning) == 'string' then result.Warnings[#result.Warnings + 1] = warning end
        end
    end

    return result
end

local function reportResult(result)
    for _, error in ipairs(result.Errors) do
        fail('vMenu refused the plugin registration: ' .. error)
    end

    for _, warning in ipairs(result.Warnings) do
        warn('vMenu accepted the plugin registration with a note: ' .. warning)
    end
end

local function newSignal(name)
    local handlers = {}
    local signal = {}

    function signal.add(handler)
        if type(handler) ~= 'function' then
            error(('%s needs a function'):format(name), 3)
        end

        handlers[#handlers + 1] = handler

        return function()
            local index = indexOf(handlers, handler)

            if index then table.remove(handlers, index) end
        end
    end

    function signal.fire(...)
        local snapshot = { table.unpack(handlers) }

        for index = 1, #snapshot do
            safeCall(name, snapshot[index], ...)
        end
    end

    return signal
end

local function await(promiseValue, callback, what)
    if callback ~= nil then
        if type(callback) ~= 'function' then
            error(what .. ' takes a function as its callback', 3)
        end

        promiseValue:next(function(value) safeCall(what, callback, value) end)

        return nil
    end

    if not coroutine.isyieldable() then
        error(what .. ' waits for vMenu, so call it inside CreateThread or pass a callback', 3)
    end

    return Citizen.Await(promiseValue)
end

---@class vMenuText
local TextMeta = {}
TextMeta.__index = TextMeta

---Display text: a plain string is a literal, `vMenu.Text.Key` makes a translation key.
local Text = {}

---Text shown exactly as written, never translated.
---@param text string
---@return vMenuText
function Text.Literal(text)
    return setmetatable({ _value = tostring(text), _isKey = false }, TextMeta)
end

---A translation key, optionally with named placeholder values. In the translated string, `{name}` is
---replaced with the matching value, which can itself be a literal or another key.
---@param key string
---@param args? table<string, string|vMenuText>
---@return vMenuText
function Text.Key(key, args)
    return setmetatable({ _value = tostring(key), _isKey = true, _args = args }, TextMeta)
end

local function textRef(text)
    if text == nil then return nil end

    if type(text) ~= 'table' or getmetatable(text) ~= TextMeta then
        return { text = tostring(text) }
    end

    if not text._isKey then
        return { text = text._value }
    end

    local reference = { key = text._value }

    if type(text._args) == 'table' then
        local args, any = {}, false

        for name, value in pairs(text._args) do
            local argument = textRef(value)

            if type(name) == 'string' and argument then
                args[name] = argument
                any = true
            end
        end

        if any then reference.args = args end
    end

    return reference
end

local function textRefs(values)
    local refs = array()

    for _, value in ipairs(values or {}) do
        local reference = textRef(value)

        if reference then refs[#refs + 1] = reference end
    end

    return refs
end

local function sameText(left, right)
    if left == right then return true end
    if left == nil or right == nil then return false end
    if left.text ~= right.text or left.key ~= right.key then return false end

    local leftArgs, rightArgs = left.args, right.args

    if leftArgs == rightArgs then return true end
    if leftArgs == nil or rightArgs == nil then return false end

    for name, value in pairs(leftArgs) do
        if not sameText(value, rightArgs[name]) then return false end
    end

    for name in pairs(rightArgs) do
        if leftArgs[name] == nil then return false end
    end

    return true
end

local function sameTexts(left, right)
    if left == right then return true end
    if left == nil or right == nil or #left ~= #right then return false end

    for index = 1, #left do
        if not sameText(left[index], right[index]) then return false end
    end

    return true
end

local sameGate

local function sameGates(left, right)
    if left == right then return true end
    if left == nil or right == nil or #left ~= #right then return false end

    for index = 1, #left do
        if not sameGate(left[index], right[index]) then return false end
    end

    return true
end

sameGate = function(left, right)
    if left == right then return true end
    if left == nil or right == nil then return false end

    return left.permission == right.permission
        and left.setting == right.setting
        and sameGates(left.all, right.all)
        and sameGates(left.any, right.any)
end

---@class vMenuGate
local GateMeta = {}
GateMeta.__index = GateMeta

local function gateNode(gate)
    if type(gate) == 'string' then return { permission = gate } end
    if type(gate) == 'table' and getmetatable(gate) == GateMeta then return gate._node end

    error('expected a gate or a permission name', 3)
end

local function newGate(node)
    return setmetatable({ _node = node }, GateMeta)
end

GateMeta.__band = function(left, right)
    return newGate({ all = array({ gateNode(left), gateNode(right) }) })
end

GateMeta.__bor = function(left, right)
    return newGate({ any = array({ gateNode(left), gateNode(right) }) })
end

---Decides whether a row is available to the player, evaluated live by vMenu. Combine gates with `&` and `|`.
---Names are short: vMenu scopes them to your plugin.
local Gate = {}

---One of the permissions your server side declared, by its short name.
---@param shortName string
---@return vMenuGate
function Gate.Permission(shortName)
    return newGate({ permission = shortName })
end

---One of your bool settings: the row is available while the convar reads true.
---@param setting table|string A setting from `plugin.Settings:Bool`, or its short name.
---@return vMenuGate
function Gate.Setting(setting)
    return newGate({ setting = type(setting) == 'table' and setting.Name or setting })
end

---Passes when every gate passes.
---@param ... vMenuGate|string
---@return vMenuGate
function Gate.All(...)
    local nodes = array()

    for _, gate in ipairs({ ... }) do nodes[#nodes + 1] = gateNode(gate) end

    return newGate({ all = nodes })
end

---Passes when at least one gate passes.
---@param ... vMenuGate|string
---@return vMenuGate
function Gate.Any(...)
    local nodes = array()

    for _, gate in ipairs({ ... }) do nodes[#nodes + 1] = gateNode(gate) end

    return newGate({ any = nodes })
end

local function formatFloat(value)
    local text = ('%.4f'):format(value):gsub('0+$', '')

    if text:sub(-1) == '.' then text = text .. '0' end

    return text
end

vMenu = {
    ---The vMenu Enhanced version this file was built for.
    Version = 'versiongoeshere',
    Text = Text,
    Gate = Gate,
}

if not IsDuplicityVersion() then
    local KvpPrefix = 'vmenu_plugin_pref_'

    local CarriedItemOps = {
        [UpdateOps.SetText] = true,
        [UpdateOps.SetDescription] = true,
        [UpdateOps.SetLabel] = true,
        [UpdateOps.SetLockedDescription] = true,
        [UpdateOps.SetConfirmationDescription] = true,
        [UpdateOps.SetIcons] = true,
        [UpdateOps.SetChecked] = true,
        [UpdateOps.SetOptions] = true,
        [UpdateOps.SetSelectedIndex] = true,
        [UpdateOps.SetSliderPosition] = true,
        [UpdateOps.SetValue] = true,
        [UpdateOps.SetVisible] = true,
        [UpdateOps.SetEnabled] = true,
        [UpdateOps.SetGate] = true,
        [UpdateOps.SetLog] = true,
        [UpdateOps.SetBehaviour] = true,
        [UpdateOps.SetItemEvents] = true,
    }

    local CarriedMenuOps = {
        [UpdateOps.SetMenuTitle] = true,
        [UpdateOps.SetMenuSubtitle] = true,
        [UpdateOps.SetMenuEvents] = true,
        [UpdateOps.AddKeys] = true,
    }

    local function readBool(itemId)
        local raw = GetResourceKvpString(KvpPrefix .. itemId)

        if raw == 'true' then return true end
        if raw == 'false' then return false end

        return nil
    end

    local function writeBool(itemId, value)
        SetResourceKvp(KvpPrefix .. itemId, value and 'true' or 'false')
    end

    local Setting = {}
    Setting.__index = Setting

    ---The current value, read from the convar every time.
    function Setting:GetValue()
        if self.Type == SettingTypes.Bool then
            return GetConvar(self.FullName, self.Default and 'true' or 'false'):lower() == 'true'
        elseif self.Type == SettingTypes.Int then
            local raw = GetConvar(self.FullName, ''):match('^%s*([+-]?%d+)%s*$')

            return raw and math.tointeger(tonumber(raw)) or self.Default
        elseif self.Type == SettingTypes.Float then
            return tonumber(GetConvar(self.FullName, '')) or self.Default
        end

        local raw = GetConvar(self.FullName, self.Default)

        return raw == '' and self.Default or raw
    end

    local Settings = {}
    Settings.__index = Settings

    local function newSettings(pluginId)
        return setmetatable({ _prefix = 'vMenu.Enhanced.Plugins.' .. pluginId .. '.', _nodes = array() }, Settings)
    end

    function Settings:_declare(name, settingType, defaultValue, defaultText, description)
        self._nodes[#self._nodes + 1] = { name = name, type = settingType, default = defaultText, description = description or '' }

        return setmetatable({ Name = name, FullName = self._prefix .. name, Type = settingType, Default = defaultValue }, Setting)
    end

    ---Declares an on or off setting. Declare it in your server script too.
    ---@param name string
    ---@param defaultValue boolean
    ---@param description string
    function Settings:Bool(name, defaultValue, description)
        return self:_declare(name, SettingTypes.Bool, defaultValue == true, defaultValue and 'true' or 'false', description)
    end

    ---Declares a whole number setting.
    function Settings:Int(name, defaultValue, description)
        local value = int(defaultValue)

        return self:_declare(name, SettingTypes.Int, value, tostring(value), description)
    end

    ---Declares a decimal number setting.
    function Settings:Float(name, defaultValue, description)
        return self:_declare(name, SettingTypes.Float, defaultValue + 0.0, formatFloat(defaultValue), description)
    end

    ---Declares a text setting.
    function Settings:String(name, defaultValue, description)
        return self:_declare(name, SettingTypes.String, tostring(defaultValue), tostring(defaultValue), description)
    end

    local Item = {}
    Item.__index = Item

    local function class(base)
        local cls = setmetatable({}, { __index = base })
        cls.__index = cls

        return cls
    end

    local Button = class(Item)
    local ConfirmButton = class(Item)
    local Checkbox = class(Item)
    local List = class(Item)
    local ConfirmList = class(List)
    local Slider = class(Item)
    local DynamicList = class(Item)
    local Separator = class(Item)
    local Submenu = class(Item)
    local PlayerButton = class(Item)
    local PlayerConfirmButton = class(Item)
    local PlayerList = class(Item)

    local function newItem(cls, node, text)
        local item = setmetatable({ Id = node.id, _node = node, _text = text, _signals = {} }, cls)

        return item
    end

    function Item:_signal(name)
        local signal = self._signals[name]

        if not signal then
            signal = newSignal(name)
            self._signals[name] = signal
        end

        return signal
    end

    function Item:_fire(name, ...)
        local signal = self._signals[name]

        if signal then signal.fire(...) end
    end

    function Item:_emit(op)
        if self._plugin then self._plugin:_emitOp(op) end
    end

    function Item:_setTextField(field, value, opName)
        local next = textRef(value)

        if sameText(self._node[field], next) then return end

        self._node[field] = next
        self:_emit({ op = opName, itemId = self.Id, textValue = next })
    end

    function Item:_subscribeNodeEvent(name)
        local events = self._node.events

        if not events then
            events = array()
            self._node.events = events
        end

        if indexOf(events, name) then return end

        events[#events + 1] = name
        self:_emit({ op = UpdateOps.SetItemEvents, itemId = self.Id, events = array({ table.unpack(events) }) })
    end

    ---The row's text.
    function Item:GetText() return self._text end

    ---@param value string|vMenuText|nil
    function Item:SetText(value)
        self._text = value
        self:_setTextField('text', value, UpdateOps.SetText)
    end

    ---The line shown under the menu while the row is highlighted.
    function Item:GetDescription() return self._description end

    function Item:SetDescription(value)
        self._description = value
        self:_setTextField('description', value, UpdateOps.SetDescription)
    end

    ---Right aligned text. Ignored by rows whose label the menu draws itself.
    function Item:GetLabel() return self._label end

    function Item:SetLabel(value)
        self._label = value
        self:_setTextField('label', value, UpdateOps.SetLabel)
    end

    ---What the row says while its gate locks it. Empty uses vMenu's own wording.
    function Item:GetLockedDescription() return self._lockedDescription end

    function Item:SetLockedDescription(value)
        self._lockedDescription = value
        self:_setTextField('lockedDescription', value, UpdateOps.SetLockedDescription)
    end

    ---Decides whether the row is available, evaluated live by vMenu.
    function Item:GetGate() return self._gate end

    ---@param value vMenuGate|string|nil
    function Item:SetGate(value)
        self._gate = value

        local gate = value ~= nil and gateNode(value) or nil

        if sameGate(self._node.gate, gate) then return end

        self._node.gate = gate
        self:_emit({ op = UpdateOps.SetGate, itemId = self.Id, gate = gate })
    end

    ---What a failing gate does to the row: true hides it, false greys it out with a lock.
    function Item:GetHideWhenLocked() return self._node.behaviour == 'hide' end

    function Item:SetHideWhenLocked(value)
        local behaviour = value and 'hide' or 'lock'

        if self._node.behaviour == behaviour then return end

        self._node.behaviour = behaviour
        self:_emit({ op = UpdateOps.SetBehaviour, itemId = self.Id, value = behaviour })
    end

    ---Whether the row is shown at all.
    function Item:GetVisible() return self._node.visible ~= false end

    function Item:SetVisible(value)
        value = value == true

        if self:GetVisible() == value then return end

        self._node.visible = value
        self:_emit({ op = UpdateOps.SetVisible, itemId = self.Id, flag = value })
    end

    ---A disabled row is greyed out but still visible. Independent of the gate.
    function Item:GetEnabled() return self._node.enabled ~= false end

    function Item:SetEnabled(value)
        value = value == true

        if self:GetEnabled() == value then return end

        self._node.enabled = value
        self:_emit({ op = UpdateOps.SetEnabled, itemId = self.Id, flag = value })
    end

    ---Asks vMenu to log use of this row to the server owner's webhook. The server half must declare the same id with `AddLoggedItem`.
    function Item:GetLog() return self._node.log == true end

    function Item:SetLog(value)
        value = value == true

        if self:GetLog() == value then return end

        self._node.log = value
        self:_emit({ op = UpdateOps.SetLog, itemId = self.Id, flag = value })
    end

    ---Icon names from the vMenu icon set, for example "LOCK" or "STAR".
    ---@param leftIcon? string
    ---@param rightIcon? string
    function Item:SetIcons(leftIcon, rightIcon)
        if self._node.leftIcon == leftIcon and self._node.rightIcon == rightIcon then return end

        self._node.leftIcon = leftIcon
        self._node.rightIcon = rightIcon
        self:_emit({ op = UpdateOps.SetIcons, itemId = self.Id, leftIcon = leftIcon, rightIcon = rightIcon })
    end

    ---Called while the player's cursor sits on this row. Chatty, subscribe deliberately.
    ---@param handler fun()
    ---@return fun() unsubscribe
    function Item:OnHighlighted(handler)
        local unsubscribe = self:_signal('highlighted').add(handler)

        self:_subscribeNodeEvent(NodeEvents.Highlighted)

        return unsubscribe
    end

    function Item:_applyOptions(options)
        if type(options) ~= 'table' then return end

        if options.Description ~= nil then self:SetDescription(options.Description) end
        if options.Label ~= nil then self:SetLabel(options.Label) end
        if options.LockedDescription ~= nil then self:SetLockedDescription(options.LockedDescription) end
        if options.Gate ~= nil then self:SetGate(options.Gate) end
        if options.HideWhenLocked ~= nil then self:SetHideWhenLocked(options.HideWhenLocked) end
        if options.Visible ~= nil then self:SetVisible(options.Visible) end
        if options.Enabled ~= nil then self:SetEnabled(options.Enabled) end
        if options.Log ~= nil then self:SetLog(options.Log) end
        if options.LeftIcon ~= nil or options.RightIcon ~= nil then self:SetIcons(options.LeftIcon, options.RightIcon) end
        if options.ConfirmationDescription ~= nil and self.SetConfirmationDescription then
            self:SetConfirmationDescription(options.ConfirmationDescription)
        end
    end

    function Item:_handle(callback)
        if callback.type == CallbackTypes.ItemHighlighted then self:_fire('highlighted') end
    end

    ---Called when the player presses the row.
    ---@param handler fun()
    ---@return fun() unsubscribe
    function Button:OnSelected(handler) return self:_signal('selected').add(handler) end

    function Button:_handle(callback)
        Item._handle(self, callback)

        if callback.type == CallbackTypes.ItemSelected then self:_fire('selected') end
    end

    ---What the row asks before its second press. Empty uses vMenu's own wording.
    function ConfirmButton:GetConfirmationDescription() return self._confirmationDescription end

    function ConfirmButton:SetConfirmationDescription(value)
        self._confirmationDescription = value
        self:_setTextField('confirmationDescription', value, UpdateOps.SetConfirmationDescription)
    end

    ---Called on the confirming second press, never on the first.
    ---@param handler fun()
    ---@return fun() unsubscribe
    function ConfirmButton:OnConfirmed(handler) return self:_signal('confirmed').add(handler) end

    function ConfirmButton:_handle(callback)
        Item._handle(self, callback)

        if callback.type == CallbackTypes.Confirmed then self:_fire('confirmed') end
    end

    ---Whether the state is saved in this resource's key value store and restored on start.
    function Checkbox:GetPersisted() return self._persisted == true end

    ---Whether the box is ticked.
    function Checkbox:GetChecked() return self._node.checked == true end

    function Checkbox:SetChecked(value)
        value = value == true

        if self:GetChecked() == value then return end

        self._node.checked = value
        if self._persisted then writeBool(self.Id, value) end
        self:_emit({ op = UpdateOps.SetChecked, itemId = self.Id, flag = value })
    end

    ---Called when the player toggles the box, with the new state.
    ---@param handler fun(checked: boolean)
    ---@return fun() unsubscribe
    function Checkbox:OnChanged(handler) return self:_signal('changed').add(handler) end

    function Checkbox:_handle(callback)
        Item._handle(self, callback)

        if callback.type == CallbackTypes.CheckboxChanged and type(callback.checked) == 'boolean' then
            self._node.checked = callback.checked
            if self._persisted then writeBool(self.Id, callback.checked) end
            self:_fire('changed', callback.checked)
        end
    end

    ---The selected option, counted from 1.
    function List:GetSelectedIndex() return (self._node.selectedIndex or 0) + 1 end

    function List:SetSelectedIndex(value)
        local index = int(value) - 1

        if (self._node.selectedIndex or 0) == index then return end

        self._node.selectedIndex = index
        self:_emit({ op = UpdateOps.SetSelectedIndex, itemId = self.Id, index = index })
    end

    ---Replaces the options, optionally moving the selection (counted from 1) at the same time.
    ---@param values (string|vMenuText)[]
    ---@param selectedIndex? integer
    function List:SetOptions(values, selectedIndex)
        local refs = textRefs(values)
        local index = selectedIndex ~= nil and int(selectedIndex) - 1 or nil

        if sameTexts(self._node.options, refs) and (index == nil or index == (self._node.selectedIndex or 0)) then return end

        self._node.options = refs
        if index ~= nil then self._node.selectedIndex = index end
        self:_emit({ op = UpdateOps.SetOptions, itemId = self.Id, options = refs, index = index })
    end

    ---Called when the player scrolls the value, with the old and new index counted from 1.
    ---@param handler fun(oldIndex: integer, newIndex: integer)
    ---@return fun() unsubscribe
    function List:OnIndexChanged(handler) return self:_signal('indexChanged').add(handler) end

    ---Called when the player presses the row, with the index they had selected, counted from 1.
    ---@param handler fun(index: integer)
    ---@return fun() unsubscribe
    function List:OnSelected(handler) return self:_signal('selected').add(handler) end

    function List:_handle(callback)
        Item._handle(self, callback)

        if callback.type == CallbackTypes.ListIndexChanged and num(callback.newIndex) then
            self._node.selectedIndex = num(callback.newIndex)
            self:_fire('indexChanged', (num(callback.oldIndex) or 0) + 1, num(callback.newIndex) + 1)
        elseif callback.type == CallbackTypes.ListSelected and num(callback.selectedIndex) then
            self:_fire('selected', num(callback.selectedIndex) + 1)
        elseif callback.type == CallbackTypes.Confirmed and num(callback.selectedIndex) then
            self:_fire('confirmed', num(callback.selectedIndex) + 1)
        end
    end

    ConfirmList.GetConfirmationDescription = ConfirmButton.GetConfirmationDescription
    ConfirmList.SetConfirmationDescription = ConfirmButton.SetConfirmationDescription

    ---Called on the confirming second press, with the confirmed index counted from 1.
    ---@param handler fun(index: integer)
    ---@return fun() unsubscribe
    function ConfirmList:OnConfirmed(handler) return self:_signal('confirmed').add(handler) end

    ---The lowest position.
    function Slider:GetMin() return self._node.min or 0 end

    ---The highest position.
    function Slider:GetMax() return self._node.max or 0 end

    ---Where the bar sits.
    function Slider:GetPosition() return self._node.position or self:GetMin() end

    function Slider:SetPosition(value)
        value = int(value)

        if self:GetPosition() == value then return end

        self._node.position = value
        self:_emit({ op = UpdateOps.SetSliderPosition, itemId = self.Id, index = value })
    end

    ---Called while the player drags the bar, with the old and new position.
    ---@param handler fun(oldPosition: integer, newPosition: integer)
    ---@return fun() unsubscribe
    function Slider:OnMoved(handler) return self:_signal('moved').add(handler) end

    ---Called when the player presses the row, with the position it sat at.
    ---@param handler fun(position: integer)
    ---@return fun() unsubscribe
    function Slider:OnSelected(handler) return self:_signal('selected').add(handler) end

    function Slider:_handle(callback)
        Item._handle(self, callback)

        if callback.type == CallbackTypes.SliderMoved and num(callback.newPosition) then
            self._node.position = num(callback.newPosition)
            self:_fire('moved', num(callback.oldPosition) or 0, num(callback.newPosition))
        elseif callback.type == CallbackTypes.SliderSelected and num(callback.position) then
            self:_fire('selected', num(callback.position))
        end
    end

    ---The value the row shows.
    function DynamicList:GetValue() return self._node.value or '' end

    function DynamicList:SetValue(value)
        value = tostring(value)

        if self:GetValue() == value then return end

        self._node.value = value
        self:_emit({ op = UpdateOps.SetValue, itemId = self.Id, value = value })
    end

    ---Sets the function that works out the next value when the player scrolls. It gets the current
    ---value and whether they went left, and returns the new value. The answer lands one beat after the press.
    ---@param producer fun(currentValue: string, left: boolean): string|nil
    function DynamicList:SetChangeRequested(producer) self._changeRequested = producer end

    ---Called when the player presses the row, with the value it showed.
    ---@param handler fun(value: string)
    ---@return fun() unsubscribe
    function DynamicList:OnSelected(handler) return self:_signal('selected').add(handler) end

    function DynamicList:_handle(callback)
        Item._handle(self, callback)

        if callback.type == CallbackTypes.DynamicChanging and type(callback.left) == 'boolean' then
            if self._changeRequested then
                local ok, next = xpcall(self._changeRequested, traceback, str(callback.currentValue) or '', callback.left)

                if not ok then
                    fail('A dynamic list ChangeRequested function threw: ' .. tostring(next))
                elseif type(next) == 'string' then
                    self:SetValue(next)
                end
            end
        elseif callback.type == CallbackTypes.DynamicSelected then
            self:_fire('selected', str(callback.value) or '')
        end
    end

    local function target(callback)
        local serverId = num(callback.targetServerId)

        if serverId then return { ServerId = serverId, Name = str(callback.targetName) or '' } end

        return nil
    end

    ---Called when the action is used on a player, with that player as `{ ServerId, Name }`.
    ---@param handler fun(target: { ServerId: integer, Name: string })
    ---@return fun() unsubscribe
    function PlayerButton:OnSelected(handler) return self:_signal('selected').add(handler) end

    function PlayerButton:_handle(callback)
        Item._handle(self, callback)

        local player = target(callback)

        if callback.type == CallbackTypes.PlayerActionSelected and player then self:_fire('selected', player) end
    end

    PlayerConfirmButton.GetConfirmationDescription = ConfirmButton.GetConfirmationDescription

    function PlayerConfirmButton:SetConfirmationDescription(value)
        self._confirmationDescription = value
        self._node.confirmationDescription = textRef(value)
        self:_emit({ op = UpdateOps.SetConfirmationDescription, itemId = self.Id, textValue = self._node.confirmationDescription })
    end

    ---Called on the confirming second press, with the targeted player as `{ ServerId, Name }`.
    ---@param handler fun(target: { ServerId: integer, Name: string })
    ---@return fun() unsubscribe
    function PlayerConfirmButton:OnConfirmed(handler) return self:_signal('confirmed').add(handler) end

    function PlayerConfirmButton:_handle(callback)
        Item._handle(self, callback)

        local player = target(callback)

        if callback.type == CallbackTypes.PlayerActionConfirmed and player then self:_fire('confirmed', player) end
    end

    ---The current selection, counted from 1. Shared across every player the menu shows.
    function PlayerList:GetSelectedIndex() return (self._node.selectedIndex or 0) + 1 end

    function PlayerList:SetSelectedIndex(value)
        local index = int(value) - 1

        self._node.selectedIndex = index
        self:_emit({ op = UpdateOps.SetSelectedIndex, itemId = self.Id, index = index })
    end

    ---Replaces the options, optionally moving the selection (counted from 1) at the same time.
    function PlayerList:SetOptions(values, selectedIndex)
        local index = selectedIndex ~= nil and int(selectedIndex) - 1 or nil

        self._node.options = textRefs(values)
        if index ~= nil then self._node.selectedIndex = index end
        self:_emit({ op = UpdateOps.SetOptions, itemId = self.Id, options = self._node.options, index = index })
    end

    ---Called when the action is used on a player, with the target and the chosen index counted from 1.
    ---@param handler fun(target: { ServerId: integer, Name: string }, index: integer)
    ---@return fun() unsubscribe
    function PlayerList:OnSelected(handler) return self:_signal('selected').add(handler) end

    function PlayerList:_handle(callback)
        Item._handle(self, callback)

        local player = target(callback)

        if callback.type == CallbackTypes.PlayerActionListSelected and player then
            local selected = num(callback.selectedIndex)

            if selected then self._node.selectedIndex = selected end
            self:_fire('selected', player, (selected or 0) + 1)
        end
    end

    local Key = {}
    Key.__index = Key

    ---The instructional button's label. Empty hides the button, the key still works.
    function Key:GetText() return self._text end

    function Key:SetText(value)
        self._text = value

        local text = textRef(value)

        if sameText(self._node.text, text) then return end

        self._node.text = text
        self._plugin:_emitOp({ op = UpdateOps.SetKeyText, keyId = self.Id, textValue = text })
    end

    ---A disabled key does nothing and shows no button.
    function Key:GetEnabled() return self._node.enabled ~= false end

    function Key:SetEnabled(value)
        value = value == true

        if self:GetEnabled() == value then return end

        self._node.enabled = value
        self._plugin:_emitOp({ op = UpdateOps.SetKeyEnabled, keyId = self.Id, flag = value })
    end

    ---While the gate fails the key does nothing and shows no button.
    function Key:GetGate() return self._gate end

    function Key:SetGate(value)
        self._gate = value

        local gate = value ~= nil and gateNode(value) or nil

        if sameGate(self._node.gate, gate) then return end

        self._node.gate = gate
        self._plugin:_emitOp({ op = UpdateOps.SetKeyGate, keyId = self.Id, gate = gate })
    end

    ---Called when the player presses the key, with `{ Item, DisabledItem }`: the row under the cursor
    ---when it is usable, or when it is locked or disabled. At most one of the two is set.
    ---@param handler fun(press: { Item: table|nil, DisabledItem: table|nil })
    ---@return fun() unsubscribe
    function Key:OnPressed(handler) return self._pressed.add(handler) end

    local Menu = {}
    Menu.__index = Menu

    local function newMenu(plugin, node, title, subtitle)
        return setmetatable({
            Id = node.id,
            _plugin = plugin,
            _node = node,
            _items = {},
            _keys = {},
            _title = title,
            _subtitle = subtitle,
            _opened = newSignal('opened'),
            _closed = newSignal('closed'),
            _indexChanged = newSignal('indexChanged'),
        }, Menu)
    end

    ---Every row, in order. A copy, so changing it changes nothing.
    function Menu:GetItems() return { table.unpack(self._items) } end

    ---Every key of this menu. A copy.
    function Menu:GetKeys() return { table.unpack(self._keys) } end

    ---Whether `Filter` is hiding rows right now.
    function Menu:IsFiltered() return self._filter ~= nil end

    ---The title in the menu's banner.
    function Menu:GetTitle() return self._title end

    function Menu:SetTitle(value)
        self._title = value

        local title = textRef(value)

        if sameText(self._node.title, title) then return end

        self._node.title = title
        self._plugin:_emitOp({ op = UpdateOps.SetMenuTitle, menuId = self.Id, textValue = title })
    end

    ---The bar under the banner.
    function Menu:GetSubtitle() return self._subtitle end

    function Menu:SetSubtitle(value)
        self._subtitle = value

        local subtitle = textRef(value)

        if sameText(self._node.subtitle, subtitle) then return end

        self._node.subtitle = subtitle
        self._plugin:_emitOp({ op = UpdateOps.SetMenuSubtitle, menuId = self.Id, textValue = subtitle })
    end

    function Menu:_subscribeMenuEvent(name)
        local events = self._node.events

        if not events then
            events = array()
            self._node.events = events
        end

        if indexOf(events, name) then return end

        events[#events + 1] = name
        self._plugin:_emitOp({ op = UpdateOps.SetMenuEvents, menuId = self.Id, events = array({ table.unpack(events) }) })
    end

    ---Called when the player opens this menu.
    ---@param handler fun()
    ---@return fun() unsubscribe
    function Menu:OnOpened(handler)
        local unsubscribe = self._opened.add(handler)

        self:_subscribeMenuEvent(NodeEvents.Opened)

        return unsubscribe
    end

    ---Called when the player leaves this menu, including into a submenu.
    ---@param handler fun()
    ---@return fun() unsubscribe
    function Menu:OnClosed(handler)
        local unsubscribe = self._closed.add(handler)

        self:_subscribeMenuEvent(NodeEvents.Closed)

        return unsubscribe
    end

    ---Called when the cursor moves, with the old and new row index counted from 1. Chatty.
    ---@param handler fun(oldIndex: integer, newIndex: integer)
    ---@return fun() unsubscribe
    function Menu:OnIndexChanged(handler)
        local unsubscribe = self._indexChanged.add(handler)

        self:_subscribeMenuEvent(NodeEvents.IndexChanged)

        return unsubscribe
    end

    function Menu:_newNode(entryType, text, options)
        local id = type(options) == 'table' and options.Id or nil

        return { id = id and tostring(id) or self._plugin:_nextItemId(), type = entryType, text = textRef(text) }
    end

    function Menu:_attach(item, options)
        item:_applyOptions(options)
        item._plugin = self._plugin

        local before

        if self._insertAt ~= nil and self._insertAt < #self._items then
            before = self._items[self._insertAt + 1].Id

            table.insert(self._items, self._insertAt + 1, item)
            table.insert(self._node.items, self._insertAt + 1, item._node)

            self._insertAt = self._insertAt + 1
        else
            self._items[#self._items + 1] = item
            self._node.items[#self._node.items + 1] = item._node

            if self._insertAt ~= nil then self._insertAt = #self._items end
        end

        self._plugin:_registerItem(item)
        self._plugin:_emitAdd(self, item, { op = UpdateOps.AddItems, menuId = self.Id, items = array({ item._node }), beforeItemId = before })

        return item
    end

    ---Adds a row the player presses. `options` takes `Id`, `Description`, `Label`, `LockedDescription`,
    ---`Gate`, `HideWhenLocked`, `Visible`, `Enabled`, `Log`, `LeftIcon` and `RightIcon`.
    ---@param text string|vMenuText
    ---@param options? table
    function Menu:AddButton(text, options)
        return self:_attach(newItem(Button, self:_newNode(EntryTypes.Button, text, options), text), options)
    end

    ---Adds a row that asks for a second press. `options` also takes `ConfirmationDescription`.
    function Menu:AddConfirmButton(text, options)
        return self:_attach(newItem(ConfirmButton, self:_newNode(EntryTypes.ConfirmButton, text, options), text), options)
    end

    ---Adds a row with a tick box. `options` also takes `Checked`, and `Persist` to save the player's
    ---choice in this resource's key value store. Pass a stable `Id` along with `Persist`.
    function Menu:AddCheckbox(text, options)
        local node = self:_newNode(EntryTypes.Checkbox, text, options)

        node.checked = type(options) == 'table' and options.Checked == true or false

        local checkbox = newItem(Checkbox, node, text)

        if type(options) == 'table' and options.Persist then
            checkbox._persisted = true

            local stored = readBool(node.id)

            if stored ~= nil then node.checked = stored end
        end

        return self:_attach(checkbox, options)
    end

    local function listNode(menu, entryType, text, values, options)
        local node = menu:_newNode(entryType, text, options)

        node.options = textRefs(values)
        node.selectedIndex = type(options) == 'table' and options.SelectedIndex and int(options.SelectedIndex) - 1 or 0

        return node
    end

    ---Adds a row the player scrolls through `values` on. `options` also takes `SelectedIndex`, counted from 1.
    ---@param values (string|vMenuText)[]
    function Menu:AddList(text, values, options)
        return self:_attach(newItem(List, listNode(self, EntryTypes.List, text, values, options), text), options)
    end

    ---Adds a list that asks for a second press. `options` also takes `SelectedIndex` and `ConfirmationDescription`.
    function Menu:AddConfirmList(text, values, options)
        return self:_attach(newItem(ConfirmList, listNode(self, EntryTypes.ConfirmList, text, values, options), text), options)
    end

    ---Adds a row with a bar the player drags between `min` and `max`. `options` also takes `ShowDivider`.
    ---@param min integer
    ---@param max integer
    ---@param position integer
    function Menu:AddSlider(text, min, max, position, options)
        local node = self:_newNode(EntryTypes.Slider, text, options)

        node.min = int(min)
        node.max = int(max)
        node.position = int(position)
        node.showDivider = type(options) == 'table' and options.ShowDivider == true or false

        return self:_attach(newItem(Slider, node, text), options)
    end

    ---Adds a row whose value your code works out each time the player scrolls it, through `SetChangeRequested`.
    ---@param initialValue string
    function Menu:AddDynamicList(text, initialValue, options)
        local node = self:_newNode(EntryTypes.DynamicList, text, options)

        node.value = tostring(initialValue or '')

        return self:_attach(newItem(DynamicList, node, text), options)
    end

    ---Adds a row that only shows text, used to split a menu into sections.
    function Menu:AddSeparator(text, options)
        return self:_attach(newItem(Separator, self:_newNode(EntryTypes.Separator, text, options), text), options)
    end

    ---Adds a row that opens a new menu, reached through the returned row's `Menu` field. `options` also
    ---takes `Title`, which falls back to the row's text, and `Subtitle`.
    function Menu:AddSubmenu(text, options)
        local node = self:_newNode(EntryTypes.Submenu, text, options)
        local title = type(options) == 'table' and options.Title or text
        local subtitle = type(options) == 'table' and options.Subtitle or nil

        node.menu = { id = self._plugin:_nextMenuId(), title = textRef(title), subtitle = textRef(subtitle), items = array() }

        local menu = newMenu(self._plugin, node.menu, title, subtitle)
        local item = newItem(Submenu, node, text)

        item.Menu = menu
        self._plugin:_registerMenu(menu)

        return self:_attach(item, options)
    end

    ---Adds a key that works while this menu is open, with an instructional button. Keep the id stable:
    ---it names the binding in the player's key settings. `options` takes `DefaultButton` (a controller
    ---button such as "RUP_INDEX"), `Description` and `ShadowedControl`.
    ---@param id string Letters, digits and underscores, unique within your plugin.
    ---@param text string|vMenuText
    ---@param defaultKey string A keyboard key name as the game knows it, for example "X" or "F5".
    ---@param options? table
    function Menu:AddKey(id, text, defaultKey, options)
        options = type(options) == 'table' and options or {}

        local node = {
            id = tostring(id),
            text = textRef(text),
            description = textRef(options.Description),
            defaultKey = tostring(defaultKey),
            defaultButton = options.DefaultButton,
            shadowedControl = options.ShadowedControl and int(options.ShadowedControl) or nil,
        }

        self._node.keys = self._node.keys or array()
        self._node.keys[#self._node.keys + 1] = node

        local key = setmetatable({ Id = node.id, _plugin = self._plugin, _node = node, _text = text, _pressed = newSignal('pressed') }, Key)

        self._keys[#self._keys + 1] = key
        self._plugin:_registerKey(key)
        self._plugin:_emitOp({ op = UpdateOps.AddKeys, menuId = self.Id, keys = array({ node }) })

        return key
    end

    ---Rows added inside `add` go in at position `index`, counted from 1, one after another, instead of at the bottom.
    ---@param index integer
    ---@param add fun()
    function Menu:InsertAt(index, add)
        local previous = self._insertAt

        self._insertAt = clamp(int(index) - 1, 0, #self._items)

        local ok, err = xpcall(add, traceback)

        self._insertAt = previous

        if not ok then error(err, 0) end
    end

    ---Moves a row of this menu to position `index`, counted from 1.
    function Menu:Move(item, index)
        local from = indexOf(self._items, item)

        if not from then return end

        local to = clamp(int(index), 1, #self._items)

        if to == from then return end

        table.remove(self._items, from)
        table.insert(self._items, to, item)

        table.remove(self._node.items, from)
        table.insert(self._node.items, to, item._node)

        local before = self._items[to + 1] and self._items[to + 1].Id or nil

        self._plugin:_emitOp({ op = UpdateOps.MoveItem, itemId = item.Id, beforeItemId = before })
    end

    ---Shows only the rows `keep` returns true for. Rows added later are checked too. Call it again after
    ---changing what it looks at.
    ---@param keep fun(item: table): boolean
    function Menu:Filter(keep)
        self._filter = keep
        self._plugin:_filterChanged(self)
    end

    ---Shows every row again after `Filter`.
    function Menu:ClearFilter()
        if self._filter == nil then return end

        self._filter = nil
        self._plugin:_filterChanged(self)
    end

    function Menu:_hides(item)
        if self._filter == nil then return false end

        local ok, keep = xpcall(self._filter, traceback, item)

        if not ok then
            fail('A menu filter threw: ' .. tostring(keep))

            return false
        end

        return not keep
    end

    function Menu:_filterOp()
        if self._filter == nil then return { op = UpdateOps.ClearFilter, menuId = self.Id } end

        local hidden = array()

        for _, item in ipairs(self._items) do
            if self:_hides(item) then hidden[#hidden + 1] = item.Id end
        end

        return { op = UpdateOps.SetFilter, menuId = self.Id, itemIds = hidden }
    end

    ---Removes one row. For a submenu row, everything beneath it goes too.
    function Menu:Remove(item)
        local index = lastIndexOf(self._items, item)

        if not index then return end

        table.remove(self._items, index)

        local nodeIndex = lastIndexOf(self._node.items, item._node)

        if nodeIndex then table.remove(self._node.items, nodeIndex) end

        self._plugin:_unregisterItem(item)
        self._plugin:_emitOp({ op = UpdateOps.RemoveItems, itemIds = array({ item.Id }) })
    end

    ---Removes every row.
    function Menu:Clear()
        for _, item in ipairs(self._items) do
            self._plugin:_unregisterItem(item)
        end

        self._items = {}
        self._node.items = array()

        self._plugin:_emitOp({ op = UpdateOps.ClearMenu, menuId = self.Id })
    end

    ---Opens this menu on screen, closing whatever vMenu menu was open.
    function Menu:Open() self._plugin:_emitOp({ op = UpdateOps.OpenMenu, menuId = self.Id }) end

    ---Closes this plugin's menu if one is open.
    function Menu:Close() self._plugin:_emitOp({ op = UpdateOps.CloseMenu, menuId = self.Id }) end

    ---Moves the cursor to a row of this menu, as if the player had moved there.
    function Menu:Select(item)
        if indexOf(self._items, item) then
            self._plugin:_emitOp({ op = UpdateOps.SelectItem, menuId = self.Id, itemId = item.Id })
        end
    end

    function Menu:_handleMenu(callback)
        if callback.type == CallbackTypes.MenuOpened then
            self._opened.fire()
        elseif callback.type == CallbackTypes.MenuClosed then
            self._closed.fire()
        elseif callback.type == CallbackTypes.MenuIndexChanged and num(callback.newIndex) then
            self._indexChanged.fire((num(callback.oldIndex) or 0) + 1, num(callback.newIndex) + 1)
        end
    end

    local function asAdded(node)
        if node.menu == nil then return node end

        local copy = shallowCopy(node)
        local menu = shallowCopy(node.menu)

        menu.items = array()
        copy.menu = menu

        return copy
    end

    local PlayerActions = {}
    PlayerActions.__index = PlayerActions

    ---Every action, in order. A copy.
    function PlayerActions:GetItems() return { table.unpack(self._items) } end

    function PlayerActions:_attach(item, options)
        item:_applyOptions(options)
        item._plugin = self._plugin

        self._items[#self._items + 1] = item
        self._nodes[#self._nodes + 1] = item._node

        self._plugin:_registerItem(item)
        self._plugin:_emitOp({ op = UpdateOps.AddPlayerActions, items = array({ item._node }) })

        return item
    end

    ---Adds an action the player presses on another player.
    function PlayerActions:AddButton(text, options)
        return self:_attach(newItem(PlayerButton, Menu._newNode(self, EntryTypes.Button, text, options), text), options)
    end

    ---Adds an action that asks for a second press. `options` also takes `ConfirmationDescription`.
    function PlayerActions:AddConfirmButton(text, options)
        return self:_attach(newItem(PlayerConfirmButton, Menu._newNode(self, EntryTypes.ConfirmButton, text, options), text), options)
    end

    ---Adds an action with a set of options. `options` also takes `SelectedIndex`, counted from 1.
    function PlayerActions:AddList(text, values, options)
        return self:_attach(newItem(PlayerList, listNode(self, EntryTypes.List, text, values, options), text), options)
    end

    ---Adds a row that only shows text.
    function PlayerActions:AddSeparator(text, options)
        return self:_attach(newItem(Separator, Menu._newNode(self, EntryTypes.Separator, text, options), text), options)
    end

    ---Removes one action from every player's entry.
    function PlayerActions:Remove(item)
        local index = indexOf(self._items, item)

        if not index then return end

        table.remove(self._items, index)

        local nodeIndex = indexOf(self._nodes, item._node)

        if nodeIndex then table.remove(self._nodes, nodeIndex) end

        self._plugin:_unregisterItem(item)
        self._plugin:_emitOp({ op = UpdateOps.RemoveItems, itemIds = array({ item.Id }) })
    end

    PlayerActions._newNode = Menu._newNode

    local Translations = {}
    Translations.__index = Translations

    ---Adds or extends one language's table. Later entries win. An "en" table is required as soon as any
    ---text uses keys, and it is the fallback.
    ---@param languageCode string
    ---@param entries table<string, string>
    function Translations:Add(languageCode, entries)
        local code = tostring(languageCode):match('^%s*(.-)%s*$'):lower()
        local merged, any = {}, false

        for key, value in pairs(entries or {}) do
            if type(key) == 'string' then
                merged[key] = tostring(value)
                any = true
            end
        end

        if not any then return end

        local existing = self._tables[code] or {}

        for key, value in pairs(merged) do existing[key] = value end

        self._tables[code] = existing
        self._plugin:_mergeTranslations(code, merged)
    end

    local Themes = {}
    Themes.__index = Themes

    ---Every theme vMenu offers as `{ Id, Name, IsCurrent }`, in order. Empty until vMenu has sent them. A copy.
    function Themes:GetAvailable() return { table.unpack(self._available) } end

    ---The id of the theme on screen, nil until vMenu has said what it is.
    function Themes:GetCurrentId() return self._currentId end

    ---The id the server's own setting asks for, which is where `Reset` goes.
    function Themes:GetConfiguredId() return self._configuredId end

    ---Whether a plugin is overriding the server's setting for this player right now.
    function Themes:IsOverridden() return self._overridden == true end

    ---Called whenever the list or the theme on screen changed. Read `GetAvailable` from here rather than right after connecting.
    ---@param handler fun()
    ---@return fun() unsubscribe
    function Themes:OnChanged(handler) return self._changed.add(handler) end

    function Themes:_send(themeId)
        if not self._plugin:IsConnected() then return end

        TriggerEvent(Events.SetTheme, encode({ theme = themeId }))
    end

    ---Puts vMenu's menus in a theme for this player, for as long as the client runs.
    ---@param themeId string
    function Themes:Set(themeId) self:_send(tostring(themeId)) end

    ---Drops the override and goes back to the theme the server's setting asks for.
    function Themes:Reset() self:_send(nil) end

    function Themes:_handle(json)
        local list = decode(json)

        if not list then return end

        local current = str(list.current)
        local available = {}

        for _, theme in ipairs(type(list.themes) == 'table' and list.themes or {}) do
            local id = str(theme.id) or ''

            available[#available + 1] = { Id = id, Name = str(theme.name) or '', IsCurrent = current ~= nil and id:lower() == current:lower() }
        end

        self._available = available
        self._currentId = current
        self._configuredId = str(list.configured)
        self._overridden = list.overridden == true
        self._changed.fire()
    end

    local Plugin = {}
    Plugin.__index = Plugin

    local instance

    ---Creates your plugin, its client side entry point. One per resource: a second call returns the first.
    ---Declare your menus, translations and settings, then call `plugin:Connect()`.
    ---@param displayName string|vMenuText Shown as your row in vMenu's Plugins menu.
    function vMenu.CreatePlugin(displayName)
        if instance then
            warn('vMenu.CreatePlugin was called twice, returning the first instance.')

            return instance
        end

        local plugin = setmetatable({
            Resource = resourceName,
            Id = sanitizeId(resourceName),
            _displayName = displayName,
            _itemsById = {},
            _menusById = {},
            _keysById = {},
            _pendingPrompts = {},
            _pendingItems = {},
            _pendingMenus = {},
            _dirtyFilters = {},
            _batchDepth = 0,
            _nextItem = 0,
            _nextMenu = 0,
            _nextPrompt = 0,
            _connected = false,
            _registrationAnswered = newSignal('registration answered'),
            _disconnected = newSignal('disconnected'),
        }, Plugin)

        plugin.Settings = newSettings(plugin.Id)
        plugin.Translations = setmetatable({ _plugin = plugin, _tables = {} }, Translations)
        plugin.Themes = setmetatable({ _plugin = plugin, _available = {}, _changed = newSignal('themes changed') }, Themes)
        plugin.PlayerActions = setmetatable({ _plugin = plugin, _items = {}, _nodes = array() }, PlayerActions)
        plugin.RootMenu = newMenu(plugin, { id = 'root', title = textRef(displayName), items = array() }, displayName)

        plugin:_registerMenu(plugin.RootMenu)

        instance = plugin

        return plugin
    end

    ---Whether vMenu currently has this plugin registered.
    function Plugin:IsConnected() return self._connected end

    ---Extra line under the resource name in your row's description, as a translation key.
    ---@param key string|nil
    function Plugin:SetDescriptionKey(key) self._descriptionKey = key end

    function Plugin:GetDescriptionKey() return self._descriptionKey end

    ---Called on every registration answer, including automatic re-registrations, with
    ---`{ Accepted, ProtocolVersion, Errors, Warnings }`.
    ---@param handler fun(result: table)
    ---@return fun() unsubscribe
    function Plugin:OnRegistrationAnswered(handler) return self._registrationAnswered.add(handler) end

    ---Called when vMenu stops, after which the plugin waits to register again.
    ---@param handler fun()
    ---@return fun() unsubscribe
    function Plugin:OnDisconnected(handler) return self._disconnected.add(handler) end

    ---Registers with vMenu and returns vMenu's first answer as `{ Accepted, ProtocolVersion, Errors, Warnings }`.
    ---That can take a while when vMenu starts later than your resource. Call it inside `CreateThread`,
    ---or pass a callback to get the answer without waiting.
    ---@param callback? fun(result: table)
    function Plugin:Connect(callback)
        if not self._firstResult then self._firstResult = promise.new() end

        self:_ensureHandlers()

        TriggerEvent(Events.Probe)

        return await(self._firstResult, callback, 'Connect')
    end

    ---Shows a message through vMenu's notification area, credited to your resource.
    ---@param style 'info'|'success'|'warning'|'error'
    ---@param text string|vMenuText
    ---@param durationMs? integer
    function Plugin:Notify(style, text, durationMs)
        if not self._connected then return end

        TriggerEvent(Events.Notify, encode({ style = style or 'info', text = textRef(text), durationMs = durationMs and int(durationMs) or nil }))
    end

    ---Asks several questions one after another through vMenu's input box. Each prompt is a table with
    ---`Title`, and optionally `MaxLength` (60 by default), `InitialValue` and `Suggestions`
    ---(`{ Value, Description }` rows). Returns the answers, or nil if the player cancelled any of them.
    ---Call it inside `CreateThread`, or pass a callback.
    ---@param prompts table[]
    ---@param callback? fun(answers: string[]|nil)
    function Plugin:GetTexts(prompts, callback)
        local result = promise.new()

        if type(prompts) ~= 'table' or #prompts == 0 or not self._connected then
            result:resolve(nil)

            return await(result, callback, 'GetTexts')
        end

        self._nextPrompt = self._nextPrompt + 1

        local requestId = self._nextPrompt
        local nodes = array()

        for _, prompt in ipairs(prompts) do
            local node = { title = textRef(prompt.Title), maxLength = int(prompt.MaxLength or 60), initial = tostring(prompt.InitialValue or '') }

            if type(prompt.Suggestions) == 'table' and #prompt.Suggestions > 0 then
                node.suggestions = array()

                for _, suggestion in ipairs(prompt.Suggestions) do
                    node.suggestions[#node.suggestions + 1] = { value = tostring(suggestion.Value), description = suggestion.Description }
                end
            end

            nodes[#nodes + 1] = node
        end

        self._pendingPrompts[requestId] = function(answer)
            if answer.cancelled == true or type(answer.answers) ~= 'table' then
                result:resolve(nil)
            else
                local answers = {}

                for index, value in ipairs(answer.answers) do answers[index] = tostring(value) end

                result:resolve(answers)
            end
        end

        TriggerEvent(Events.Prompt, encode({ requestId = requestId, prompts = nodes }))

        return await(result, callback, 'GetTexts')
    end

    ---Asks the player for text through vMenu's input box. `options` takes `MaxLength`, `InitialValue` and
    ---`Suggestions`. Returns the text, or nil if they cancelled. Call it inside `CreateThread`, or pass a callback.
    ---@param title string|vMenuText
    ---@param options? table
    ---@param callback? fun(text: string|nil)
    function Plugin:GetText(title, options, callback)
        if type(options) == 'function' and callback == nil then
            callback, options = options, nil
        end

        options = type(options) == 'table' and options or {}

        local prompt = { Title = title, MaxLength = options.MaxLength, InitialValue = options.InitialValue, Suggestions = options.Suggestions }

        if callback ~= nil then
            return self:GetTexts({ prompt }, function(answers) callback(answers and answers[1] or nil) end)
        end

        local answers = self:GetTexts({ prompt })

        return answers and answers[1] or nil
    end

    ---Groups every change made inside `changes` into one update, so vMenu repaints once. Nesting is fine.
    ---@param changes fun()
    function Plugin:Batch(changes)
        self._batch = self._batch or {}
        self._batchDepth = self._batchDepth + 1

        local results = table.pack(xpcall(changes, traceback))

        self:_endBatch()

        if not results[1] then error(results[2], 0) end

        return table.unpack(results, 2, results.n)
    end

    function Plugin:_endBatch()
        self._batchDepth = self._batchDepth - 1

        if self._batchDepth > 0 then return end

        local ops = self._batch

        if not ops then return end

        self._batch = nil
        self._pendingItems = {}
        self._pendingMenus = {}

        for _, menu in ipairs(self._dirtyFilters) do
            ops[#ops + 1] = menu:_filterOp()
        end

        self._dirtyFilters = {}

        if #ops == 0 or not self._connected then return end

        self:_send(ops)
    end

    function Plugin:_nextItemId()
        self._nextItem = self._nextItem + 1

        return 'i' .. self._nextItem
    end

    function Plugin:_nextMenuId()
        self._nextMenu = self._nextMenu + 1

        return 'm' .. self._nextMenu
    end

    function Plugin:_registerMenu(menu) self._menusById[menu.Id] = menu end

    function Plugin:_registerItem(item) self._itemsById[item.Id] = item end

    function Plugin:_registerKey(key)
        if self._keysById[key.Id] then
            warn(("Key id '%s' is used twice, vMenu will skip the second one."):format(key.Id))

            return
        end

        self._keysById[key.Id] = key
    end

    function Plugin:_unregisterItem(item)
        self._itemsById[item.Id] = nil

        if getmetatable(item) ~= Submenu then return end

        self._menusById[item.Menu.Id] = nil

        for _, key in ipairs(item.Menu._keys) do
            self._keysById[key.Id] = nil
        end

        for _, child in ipairs(item.Menu._items) do
            self:_unregisterItem(child)
        end
    end

    function Plugin:_carriedByPendingAdd(op)
        if CarriedItemOps[op.op] then return op.itemId ~= nil and self._pendingItems[op.itemId] == true end
        if CarriedMenuOps[op.op] then return op.menuId ~= nil and self._pendingMenus[op.menuId] == true end

        return false
    end

    function Plugin:_emitOp(op)
        if not self._connected then return end

        local batch = self._batch

        if not batch then
            self:_send({ op })

            return
        end

        if self:_carriedByPendingAdd(op) then return end

        if op.op == UpdateOps.AddItems then
            for _, node in ipairs(op.items or {}) do
                self._pendingItems[node.id] = true

                if node.menu then self._pendingMenus[node.menu.id] = true end
            end
        end

        batch[#batch + 1] = op
    end

    function Plugin:_markFilterDirty(menu)
        if not indexOf(self._dirtyFilters, menu) then
            self._dirtyFilters[#self._dirtyFilters + 1] = menu
        end
    end

    function Plugin:_filterChanged(menu)
        if not self._connected then return end

        if not self._batch then
            self:_send({ menu:_filterOp() })

            return
        end

        self:_markFilterDirty(menu)
    end

    function Plugin:_emitAdd(menu, item, add)
        if not self._connected then return end

        if self._batch then
            self:_emitOp(add)

            if menu._filter ~= nil then self:_markFilterDirty(menu) end

            return
        end

        local ops = { add }

        if menu:_hides(item) then
            ops[#ops + 1] = { op = UpdateOps.SetFilter, menuId = menu.Id, itemIds = array({ item.Id }), flag = true }
        end

        self:_send(ops)
    end

    function Plugin:_mergeTranslations(code, entries)
        if self._connected then
            self:_emitOp({ op = UpdateOps.MergeTranslations, language = code, entries = entries })
        end
    end

    function Plugin:_send(ops)
        local payload = array()

        for index, op in ipairs(ops) do
            if op.op == UpdateOps.AddItems and op.items then
                local copy = shallowCopy(op)

                copy.items = array()

                for itemIndex, node in ipairs(op.items) do copy.items[itemIndex] = asAdded(node) end

                payload[index] = copy
            else
                payload[index] = op
            end
        end

        TriggerEvent(Events.Update, encode({ ops = payload }))
    end

    function Plugin:_buildRequest()
        local request = {
            protocolVersion = PROTOCOL_VERSION,
            displayName = textRef(self._displayName),
            descriptionKey = self._descriptionKey,
            menu = self.RootMenu._node,
        }

        if #self.PlayerActions._nodes > 0 then request.playerActions = self.PlayerActions._nodes end
        if next(self.Translations._tables) ~= nil then request.translations = self.Translations._tables end
        if #self.Settings._nodes > 0 then request.settings = self.Settings._nodes end

        return request
    end

    function Plugin:_sendRegistration()
        TriggerEvent(Events.Register, encode(self:_buildRequest()))
    end

    function Plugin:_cancelPendingPrompts()
        local pending = self._pendingPrompts

        self._pendingPrompts = {}

        for _, resolve in pairs(pending) do
            resolve({ cancelled = true })
        end
    end

    function Plugin:_onRegisterResult(json)
        local result = normalizeResult(decode(json))

        if not result then
            warn('vMenu sent a registration answer that did not parse.')

            return
        end

        reportResult(result)

        self._connected = result.Accepted

        if not self._connected then
            self:_cancelPendingPrompts()
        else
            self:Batch(function()
                for _, menu in pairs(self._menusById) do
                    if menu._filter ~= nil then self:_filterChanged(menu) end
                end
            end)
        end

        if self._firstResult and not self._firstResolved then
            self._firstResolved = true
            self._firstResult:resolve(result)
        end

        self._registrationAnswered.fire(result)
    end

    function Plugin:_item(id)
        if type(id) ~= 'string' then return nil end

        return self._itemsById[id]
    end

    function Plugin:_onCallback(json)
        local callback = decode(json)

        if not callback then return end

        local ok, err = xpcall(function()
            local kind = callback.type

            if kind == CallbackTypes.MenuOpened or kind == CallbackTypes.MenuClosed or kind == CallbackTypes.MenuIndexChanged then
                local menu = type(callback.menuId) == 'string' and self._menusById[callback.menuId] or nil

                if menu then menu:_handleMenu(callback) end
            elseif kind == CallbackTypes.KeyPressed then
                local key = type(callback.keyId) == 'string' and self._keysById[callback.keyId] or nil

                if key then key._pressed.fire({ Item = self:_item(callback.itemId), DisabledItem = self:_item(callback.disabledItemId) }) end
            else
                local item = self:_item(callback.itemId)

                if item then item:_handle(callback) end
            end
        end, traceback)

        if not ok then fail('A menu callback handler threw: ' .. tostring(err)) end
    end

    function Plugin:_onPromptResult(json)
        local result = decode(json)

        if not result then return end

        local requestId = num(result.requestId)
        local pending = requestId and self._pendingPrompts[requestId]

        if pending then
            self._pendingPrompts[requestId] = nil
            pending(result)
        end
    end

    function Plugin:_onResourceStop(stopped)
        if type(stopped) ~= 'string' or stopped:lower() ~= VMENU_RESOURCE:lower() then return end

        self._connected = false
        self:_cancelPendingPrompts()
        self._disconnected.fire()
    end

    function Plugin:_ensureHandlers()
        if self._handlersRegistered then return end

        self._handlersRegistered = true

        AddEventHandler(Events.Ready, function() self:_sendRegistration() end)
        AddEventHandler(Events.ReadyFor(self.Resource), function() self:_sendRegistration() end)
        AddEventHandler(Events.RegisterResultFor(self.Resource), function(json) self:_onRegisterResult(json) end)
        AddEventHandler(Events.EventFor(self.Resource), function(json) self:_onCallback(json) end)
        AddEventHandler(Events.PromptResultFor(self.Resource), function(json) self:_onPromptResult(json) end)
        AddEventHandler(Events.ThemesFor(self.Resource), function(json) self.Themes:_handle(json) end)
        AddEventHandler('onResourceStop', function(stopped) self:_onResourceStop(stopped) end)
    end

    local function serverOnly(name)
        return function() error(('vMenu.%s is server side only, call it from a server_script'):format(name), 2) end
    end

    vMenu.ServerPluginDeclaration = serverOnly('ServerPluginDeclaration')
    vMenu.Server = setmetatable({}, { __index = function(_, name) return serverOnly('Server.' .. tostring(name)) end })
else
    local PermissionPrefix = 'vMenu.Enhanced.Plugins'
    local Everything = 'vMenu.Enhanced.Everything'
    local pluginId = sanitizeId(resourceName)

    local Declaration = {}
    Declaration.__index = Declaration

    ---Starts the server side declaration: the permissions and settings your plugin brings. Names are
    ---short, vMenu composes `vMenu.Enhanced.Plugins.<Id>.<Name>` from your resource name.
    ---@param displayName string Used in the generated example files.
    function vMenu.ServerPluginDeclaration(displayName)
        return setmetatable({ DisplayName = tostring(displayName), _permissions = array(), _settings = array(), _loggedItems = array() }, Declaration)
    end

    ---Declares a permission. `staffOnly` marks it as staff only in the generated permissions example.
    ---@param name string
    ---@param description string
    ---@param staffOnly? boolean
    function Declaration:AddPermission(name, description, staffOnly)
        self._permissions[#self._permissions + 1] = { name = name, description = description or '', staffOnly = staffOnly == true }

        return self
    end

    ---Lets the server owner see a line in their webhook whenever somebody uses this row. The client half
    ---still has to set `Log` on it. `description` is a noun phrase dropped into vMenu's own wording.
    ---@param itemId string
    ---@param description string
    function Declaration:AddLoggedItem(itemId, description)
        self._loggedItems[#self._loggedItems + 1] = { itemId = itemId, description = description or '' }

        return self
    end

    function Declaration:_add(name, settingType, defaultText, description)
        self._settings[#self._settings + 1] = { name = name, type = settingType, default = defaultText, description = description or '' }

        return self
    end

    ---Declares an on or off setting.
    function Declaration:AddBoolSetting(name, defaultValue, description)
        return self:_add(name, SettingTypes.Bool, defaultValue and 'true' or 'false', description)
    end

    ---Declares a whole number setting.
    function Declaration:AddIntSetting(name, defaultValue, description)
        return self:_add(name, SettingTypes.Int, tostring(int(defaultValue)), description)
    end

    ---Declares a decimal number setting.
    function Declaration:AddFloatSetting(name, defaultValue, description)
        return self:_add(name, SettingTypes.Float, formatFloat(defaultValue), description)
    end

    ---Declares a text setting.
    function Declaration:AddStringSetting(name, defaultValue, description)
        return self:_add(name, SettingTypes.String, tostring(defaultValue), description)
    end

    local declaration
    local firstResult
    local firstResolved = false
    local handlersRegistered = false
    local registrationAnswered = newSignal('registration answered')

    local function sendRegistration()
        if not declaration then return end

        TriggerEvent(Events.ServerRegister, encode({
            protocolVersion = SERVER_PROTOCOL_VERSION,
            displayName = declaration.DisplayName,
            permissions = declaration._permissions,
            settings = declaration._settings,
            loggedItems = declaration._loggedItems,
        }))
    end

    local function onResult(json)
        local result = normalizeResult(decode(json))

        if not result then
            warn('vMenu sent a registration answer that did not parse.')

            return
        end

        reportResult(result)

        if firstResult and not firstResolved then
            firstResolved = true
            firstResult:resolve(result)
        end

        registrationAnswered.fire(result)
    end

    local function playerSource(source)
        if source == nil then return '' end

        return tostring(math.tointeger(source) or source)
    end

    ---The server side entry point for a plugin.
    vMenu.Server = {}

    ---Declares the plugin with vMenu and returns vMenu's first answer as `{ Accepted, ProtocolVersion, Errors, Warnings }`.
    ---Registering again after a vMenu restart happens by itself. Call it inside `CreateThread`, or pass a callback.
    ---@param pluginDeclaration table From `vMenu.ServerPluginDeclaration`.
    ---@param callback? fun(result: table)
    function vMenu.Server.Register(pluginDeclaration, callback)
        declaration = pluginDeclaration
        firstResult = firstResult or promise.new()

        if not handlersRegistered then
            handlersRegistered = true

            AddEventHandler(Events.ServerReady, sendRegistration)
            AddEventHandler(Events.ServerReadyFor(resourceName), sendRegistration)
            AddEventHandler(Events.ServerRegisterResultFor(resourceName), onResult)
        end

        TriggerEvent(Events.ServerProbe)

        return await(firstResult, callback, 'Register')
    end

    ---Whether a player holds one of your plugin's permissions, by its short name. Also honours the container
    ---grants a server owner may have used instead of the exact name.
    ---@param source integer|string
    ---@param permissionName string
    ---@return boolean
    function vMenu.Server.IsPlayerAllowed(source, permissionName)
        local player = playerSource(source)

        if player == '' then return false end

        local scope = PermissionPrefix .. '.' .. pluginId

        return IsPlayerAceAllowed(player, scope .. '.' .. permissionName)
            or IsPlayerAceAllowed(player, scope .. '.All')
            or IsPlayerAceAllowed(player, PermissionPrefix .. '.All')
            or IsPlayerAceAllowed(player, Everything)
    end

    ---The same check as `IsPlayerAllowed`, but a refusal is also reported to vMenu's security webhook. Use it
    ---where a legitimate client could only have sent the request while allowed.
    ---@param source integer|string
    ---@param permissionName string
    ---@return boolean
    function vMenu.Server.RequirePermission(source, permissionName)
        if vMenu.Server.IsPlayerAllowed(source, permissionName) then return true end

        local player = playerSource(source)

        if player ~= '' then TriggerEvent(Events.ServerDenied, player, permissionName) end

        return false
    end

    ---Refreshes permissions for one or more players by server id. Passing none refreshes everyone.
    ---@param ... integer
    function vMenu.Server.RefreshPermissions(...)
        local ids = {}

        for _, id in ipairs({ ... }) do ids[#ids + 1] = int(id) end

        TriggerEvent(RefreshPermissionsEvent, ids)
    end

    ---Called on every registration answer, including automatic re-registrations.
    ---@param handler fun(result: table)
    ---@return fun() unsubscribe
    function vMenu.Server.OnRegistrationAnswered(handler)
        return registrationAnswered.add(handler)
    end

    vMenu.CreatePlugin = function()
        error('vMenu.CreatePlugin is client side only, call it from a client_script', 2)
    end
end
