AddEventHandler('vMenu.Enhanced:StructNative', function(hash, slots, args, done)
    if GetInvokingResource() ~= GetCurrentResourceName() then
        return
    end

    local blob = string.rep('\0', math.max(slots * 8, 64))

    local params = {}
    for i = 1, #args do
        params[i] = args[i]
    end
    params[#params + 1] = blob
    params[#params + 1] = Citizen.ResultAsInteger()

    local result = Citizen.InvokeNative(hash, table.unpack(params))

    local values = {}
    for i = 1, slots do
        values[i] = string.unpack('<i8', blob, (i - 1) * 8 + 1)
    end

    done(result or 0, values)
end)
