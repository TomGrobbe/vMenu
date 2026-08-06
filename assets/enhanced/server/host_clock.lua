-- Only here because the C# server runtime cannot read the system clock: DateTime.UtcNow dies inside
-- Windows leap second detection, which the runtime does not implement. Lua's os.time() is the way in.
-- os.time() takes no argument here on purpose: that form is seconds since the Unix epoch, which is
-- the same number in every timezone. Passing a table would read it as local time instead.
-- The C# side owns the timing and passes the convar name, so this stays a single write.
-- AddEventHandler and not RegisterNetEvent, so no client can reach it.
AddEventHandler('vMenu.Enhanced:Clock:Publish', function(convar)
    SetConvarReplicated(convar, string.format('%d', os.time()))
end)
