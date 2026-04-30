using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Gcc.Core.Utility;
using UnityEngine;
using UnityEngine.LowLevel;
using XLua;

namespace Gcc.Core.Lua {
    public static class LuaManager {
        private static LuaEnv _env;

        private static Dictionary<string, string> _nameToPathMap = null;
        private static Dictionary<string, string> _pathToNameMap = null;

        private static readonly ConcurrentQueue<string> _pendingReloads = new();
        private static FileSystemWatcher _watcher;

        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize() {
            Shutdown();
            InitializeIfNeeded();
        }

        private static void InitializeIfNeeded() {
            if (_env != null) return;

            _env = new();
            _env.AddLoader(new(GetLuaScriptBytes));

            InjectLuaTick();
            SubscribeApplicationQuit();
            GenerateLuaScriptsMap();
            SetupLiveReloading();
        }

        private static byte[] GetLuaScriptBytes(ref string name) {
            if (_nameToPathMap?.TryGetValue(name, out var script) != true) {
                return null;
            }

            if (!File.Exists(script)) {
                Debug.LogError($"Lua script not found: {script}");
                return null;
            }

            return File.ReadAllBytes(script);
        }

        private static void InjectLuaTick() {
            var loopSystem = PlayerLoop.GetCurrentPlayerLoop();
            var luaTickSystem = new PlayerLoopSystem {
                type = typeof(LuaManager),
                updateDelegate = Tick,
            };

            for (int i = 0; i < loopSystem.subSystemList.Length; i++) {
                if (loopSystem.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Update)) {
                    var updateSubSystems = loopSystem.subSystemList[i].subSystemList;
                    var newSubSystems = new PlayerLoopSystem[updateSubSystems.Length + 1];

                    for (int j = 0; j < updateSubSystems.Length; j++) {
                        newSubSystems[j] = updateSubSystems[j];
                    }

                    newSubSystems[^1] = luaTickSystem;
                    loopSystem.subSystemList[i].subSystemList = newSubSystems;

                    break;
                }
            }

            PlayerLoop.SetPlayerLoop(loopSystem);
        }

        private static void SubscribeApplicationQuit() {
            Application.quitting += Shutdown;
        }

        private static void GenerateLuaScriptsMap() {
            _nameToPathMap ??= new();
            _pathToNameMap ??= new();

            _nameToPathMap.Clear();
            _pathToNameMap.Clear();

            var luaFiles = Directory.GetFiles(Application.dataPath, "*.lua", SearchOption.AllDirectories);
            foreach (var file in luaFiles) {
                var path = new UnityPath(file);
                var name = Path.GetFileNameWithoutExtension(path);

                if (!_nameToPathMap.ContainsKey(name)) {
                    _nameToPathMap[name] = path;
                    _pathToNameMap[path] = name;
                }
                else  {
                    Debug.LogWarning($"Duplicate Lua script name detected: {name}. Path: {path}, Existing Path: {_nameToPathMap[name]}");
                    continue;
                }
            }
        }

        private static void SetupLiveReloading() {
            if (_watcher != null) {
                return;
            }

            _watcher = new(Application.dataPath, "*.lua") {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };

            _watcher.Changed += OnLuaFileChanged;
        }

        private static void OnLuaFileChanged(object sender, FileSystemEventArgs e) {
            var path = new UnityPath(e.FullPath);
            
            _pendingReloads.Enqueue(path);
        }

        private static void Tick() {
            _env?.Tick();

            while (_pendingReloads.TryDequeue(out var path)) {
                ReloadLuaScript(path);
            }
        }

        private static void ReloadLuaScript(string path) {
            try {
                if (_pathToNameMap?.TryGetValue(path, out var name) == true) {
                    _env.DoString($"package.loaded['{name}'] = nil");
                    _env.DoString($"require('{name}')");
                }
            }
            catch (IOException e) {
                Debug.LogError($"Error occurred while reloading Lua script: {path}. Error: {e.Message}");
            }
        }

        private static void Shutdown() {
            Application.quitting -= Shutdown;

            _watcher?.Dispose();
            _watcher = null;

            _env?.Dispose();
            _env = null;
        }

        public static void DoString(string chunk) => _env?.DoString(chunk);
        public static void GlobalSet<T>(string name, T value) => _env?.Global.Set(name, value);
    }
}
