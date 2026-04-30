using System;
using Unity.Collections;
using UnityEngine;

namespace Gcc.Core.Entities {
    public static class Environment {
        public const int MaxEntities = 8192;
        private static int _nextId = 1;
        private static NativeQueue<int> _freeIds;
        private static NativeList<int> _entities;
        public static NativeList<int> Entities => _entities;
        private static NativeParallelHashMap<int, int> _idToIndexMap;

        public static event Action<int> OnEntityCreated;
        public static event Action<int> OnEntityDestroyed;

        static Environment() {
            _entities = new(MaxEntities, Allocator.Persistent);
            _idToIndexMap = new(MaxEntities, Allocator.Persistent);
            _freeIds = new(Allocator.Persistent);

            OnEntityCreated = null;
            OnEntityDestroyed = null;
            
            Application.quitting += Dispose;
        }

        public static int CreateEntity() {
            if (!_freeIds.IsCreated || !_freeIds.TryDequeue(out int id)) {
                id = _nextId++;
            }

            _entities.Add(id);
            _idToIndexMap.Add(id, _entities.Length - 1);

            OnEntityCreated?.Invoke(id);
            return id;
        }

        public static void DestroyEntity(int id) {
            if (!_idToIndexMap.TryGetValue(id, out int index)) {
                return;
            }
                
            int lastIndex = _entities.Length - 1;
            if (index != lastIndex) {
                int lastId = _entities[lastIndex];

                _entities[index] = lastId;
                _idToIndexMap[lastId] = index;
            }

            _entities.RemoveAtSwapBack(lastIndex);
            _idToIndexMap.Remove(id);
            _freeIds.Enqueue(id);

            OnEntityDestroyed?.Invoke(id);
        }

        private static void Dispose() {
            Application.quitting -= Dispose;

            if (_entities.IsCreated) _entities.Dispose();
            if (_idToIndexMap.IsCreated) _idToIndexMap.Dispose();
            if (_freeIds.IsCreated) _freeIds.Dispose();
        }
    }
}