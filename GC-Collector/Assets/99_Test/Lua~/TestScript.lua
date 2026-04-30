local M = {}

TestValue = 100
print("<color=green>[Lua]</color> TestScript Loaded! Value: " .. TestValue)

function M.ShowMessage()
    print("<color=cyan>[Lua]</color> Live Reloading is Working! Current Value: " .. TestValue)
end

return M