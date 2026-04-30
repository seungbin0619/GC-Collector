using UnityEngine;
using Gcc.Core.Lua;

public class LuaTest : MonoBehaviour {
    void Start() {
        LuaManager.DoString("require('TestScript')");

        LuaManager.GlobalSet("playerName", "Tester_Root");
        LuaManager.DoString("print('<color=yellow>[C#->Lua]</color> Player Name set to: ' .. playerName)");
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            LuaManager.DoString("require('TestScript').ShowMessage()");
        }
    }
}