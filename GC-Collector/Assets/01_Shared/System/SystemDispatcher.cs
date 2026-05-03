using System.Collections.Generic;
using UnityEngine;

namespace Gcc.Shared.System {
    public class SystemDispatcher : MonoBehaviour {
        private static SystemDispatcher _instance = null;
        private static readonly List<ISystem> _systems = new();

        private static readonly List<ISystem> _pendingAdditions = new();
        private static readonly List<ISystem> _pendingRemovals = new();
        
        private static bool _isDirty = false;

        public static void Register(ISystem system) {
            if (!_systems.Contains(system)) {
                _pendingAdditions.Add(system);
                _isDirty = true;

                EnsureInitialize();
            }
        }

        public static void Unregister(ISystem system) {
            if (_pendingAdditions.Contains(system)) {
                _pendingAdditions.Remove(system);
                return;
            }

            if (_systems.Contains(system) && !_pendingRemovals.Contains(system)) {
                _pendingRemovals.Add(system);
            }
        }

        public static void EnsureInitialize() {
            if (_instance == null) {
                GameObject go = new("[System Dispatcher]");
                _instance = go.AddComponent<SystemDispatcher>();

                DontDestroyOnLoad(go);
            }
        }

        private static void ProcessPendingAndSort() {
            bool changed = false;

            if (_pendingRemovals.Count > 0) {
                for (int i = 0; i < _pendingRemovals.Count; i++) {
                    _systems.Remove(_pendingRemovals[i]);
                }

                _pendingRemovals.Clear();
                changed = true;
            }


            if (_pendingAdditions.Count > 0) {
                for (int i = 0; i < _pendingAdditions.Count; i++) {
                    _systems.Add(_pendingAdditions[i]);
                }

                _pendingAdditions.Clear();
                changed = true;
            }

            if (_isDirty && changed) {
                _systems.Sort((a, b) => a.updateOrder.CompareTo(b.updateOrder));
                _isDirty = false;
            }
        }

        private static void OnUpdate(float deltaTime) {
            ProcessPendingAndSort();

            for (int i = 0; i < _systems.Count; i++) {
                if (_systems[i] is IUpdateSystem updateSystem) {
                    updateSystem.OnUpdate(deltaTime);
                }
            }
        }

        private static void OnLateUpdate(float deltaTime) {
            ProcessPendingAndSort();

            for (int i = 0; i < _systems.Count; i++) {
                if (_systems[i] is ILateUpdateSystem lateUpdateSystem) {
                    lateUpdateSystem.OnLateUpdate(deltaTime);
                }
            }
        }

        private void Update() {
            OnUpdate(Time.deltaTime);
        }

        private void LateUpdate() {
            OnLateUpdate(Time.deltaTime);
        }
    }
}