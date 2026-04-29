using UnityEngine;
using Gcc.Core.Lua; // 작성하신 네임스페이스 [cite: 2536]

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