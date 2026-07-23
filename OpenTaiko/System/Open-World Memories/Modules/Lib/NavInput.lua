---@diagnostic disable: undefined-global, undefined-field, lowercase-global, need-check-nil
-- nav_input.lua — handles common keybinds for directional navigation

local NavInput = {}

-- ─── Player input sets ────────────────────────────────────────────────────────

local inputSets = {
    -- per-player
    { right = "RBlue",       left = "LBlue",      decide1 = "RRed",    decide2 = "LRed",    cancel = nil },
    { right = "RBlue2P",     left = "LBlue2P",    decide1 = "RRed2P",  decide2 = "LRed2P",  cancel = nil },
    { right = "RBlue3P",     left = "LBlue3P",    decide1 = "RRed3P",  decide2 = "LRed3P",  cancel = nil },
    { right = "RBlue4P",     left = "LBlue4P",    decide1 = "RRed4P",  decide2 = "LRed4P",  cancel = nil },
    { right = "RBlue5P",     left = "LBlue5P",    decide1 = "RRed5P",  decide2 = "LRed5P",  cancel = nil },
    -- universal
    pad = {
      right = "RightChange", left = "LeftChange", decide1 = "Decide",  decide2 = "Decide",  cancel = "Cancel" },
    keyboard = {
      right = "RightArrow",  left = "LeftArrow",  decide1 = "Return",  decide2 = "Return",  cancel = "Escape",
      down  = "DownArrow",   up   = "UpArrow" },
}

local binding_player_defs = {
    right = { { nav = "right" } },
    rightOtherPlayer = { { navKbd = "right", navPadOther = "right" } },
    rightKeyboard = { { navKbd = "right" } },
    left = { { nav = "left" } },
    leftOtherPlayer = { { navKbd = "left", navPadOther = "left" } },
    leftKeyboard = { { navKbd = "left" } },
    up = { { navKbd = "up" } },
    upOrPadLeft = { { navKbd = "up", navPad = "left" } },
    down = { { navKbd = "down" } },
    downOrPadRight = { { navKbd = "down", navPad = "right" } },
    decide = { { nav = "decide1" }, { nav = "decide2" } },
    cancel = { { nav = "cancel" } },
}

local binding_union_defs = {
    right = { { nav = "right", navPadOther = "right" } },
    rightKeyboard = { { navKbd = "right" } },
    left = { { nav = "left", navPadOther = "left" } },
    leftKeyboard = { { navKbd = "left" } },
    up = { { navKbd = "up" } },
    upOrPadLeft = { { navKbd = "up", navPad = "left", navPadOther = "left" } },
    down = { { navKbd = "down" } },
    downOrPadRight = { { navKbd = "down", navPad = "right", navPadOther = "right" } },
    decide = { { nav = "decide1", navPadOther = "decide1" }, { nav = "decide2", navPadOther = "decide2" } },
    cancel = { { nav = "cancel", navPadOther = "cancel" } },
}

local function tryInputPad(event, pad)
    return pad ~= nil and INPUT[event](INPUT, pad)
end
local function tryInputKeyboard(event, key)
    return key ~= nil and INPUT["Keyboard" .. event](INPUT, key)
end

local function inputHandler(event, player, def)
    if event == nil or event == "" then event = "Pressed" end
    local inputSetP = inputSets[player] or {}

    local handlers = {}
    for i, v in ipairs(def) do
        local navKbd = v.navKbd or v.nav
        local navPad = v.navPad or v.nav
        local navPadOther = v.navPadOther
        local tryPadOther = (navPadOther == nil) and function(event) return false end or function (event)
            for p = 1, 5, 1 do
                if p ~= player and tryInputPad(event, inputSets[p][navPadOther]) then
                    return true
                end
            end
            return false
        end
        handlers[i] = function (useUniversal)
            if useUniversal == nil then useUniversal = true end
            return tryInputPad(event, inputSetP[navPad]) or tryPadOther(event)
                or (useUniversal and (tryInputPad(event, inputSets.pad[navPad]) or tryInputKeyboard(event, inputSets.keyboard[navKbd])))
        end
    end

    local chainedHandler = nil
    for i = #handlers, 1, -1 do
        local chainedHandlerI = chainedHandler
        local handlerI = handlers[i]
        chainedHandler = (chainedHandlerI == nil) and handlerI or function (useUniversal)
            return handlerI(useUniversal) or chainedHandlerI(useUniversal)
        end
    end
    return chainedHandler or function (useUniversal) return false end
end

-- 1 for P1, 2 for P2, and so on; 0 for using only universal navigation keys; "" or omitted for using the union of all players' keys
function NavInput.getPn(player)
    if player == nil then player = "" end
    if NavInput.p[player] ~= nil then
        return NavInput.p[player]
    end
    local binding_defs = (player == "") and binding_union_defs or binding_player_defs
    local navPn = {}
    -- navPn.right() aka. navPn.rightPressed(), navPn.rightPressing(), navPn.rightReleased(), and so on.
    for i, event in ipairs{ "", "Pressed", "Pressing", "Released" } do
        for binding, def in pairs(binding_defs) do
            navPn[binding .. event] = inputHandler(event, player, def)
        end
    end
    return navPn
end

-- add universal and per-player input functions
NavInput.p = {}
for p = 0, 5, 1 do
    NavInput.p[p] = NavInput.getPn(p)
end

-- add shorthand for universal input functions
NavInput.p[""] = NavInput.getPn()
for k, v in pairs(NavInput.p[""]) do
    NavInput[k] = v
end

return NavInput
