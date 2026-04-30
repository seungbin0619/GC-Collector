using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Gcc.Core.Entities {
    public static class ComponentStorage<T> where T : struct, IComponentData {
        private static NativeArray<T> _data;
        public static NativeArray<T> Data => _data;
        private static NativeParallelHashMap<int, int> _idToIndexMap;
        private static NativeParallelHashMap<int, int> _indexToIdMap;

        public static int Count { get; private set; }
        public static int Capacity => Environment.MaxEntities;

        static ComponentStorage() {
            _data = new(Capacity, Allocator.Persistent);
            _idToIndexMap = new(Capacity, Allocator.Persistent);
            _indexToIdMap = new(Capacity, Allocator.Persistent);
            Count = 0;

            Environment.OnEntityDestroyed += Remove;
            Application.quitting += Dispose;
        }

        public static void Add(int id, T component) {
            if (Count >= Capacity) {
                // ...
                return;
            }

            _data[Count] = component;
            _idToIndexMap.TryAdd(id, Count);
            _indexToIdMap.TryAdd(Count, id);

            Count++;
        }

        public static void Remove(int id) {
            if (!_idToIndexMap.TryGetValue(id, out var index)) {
                return;
            }

            int lastIndex = Count - 1;
            if (index != lastIndex) {
                var data = _data[lastIndex];
                _data[index] = data;

                if (_indexToIdMap.TryGetValue(lastIndex, out var lastId)) {
                    _idToIndexMap[lastId] = index;
                    _indexToIdMap[index] = lastId;
                }
            }

            _idToIndexMap.Remove(id);
            _indexToIdMap.Remove(lastIndex);

            Count--;
        }

        public static unsafe ref T Get(int id) {
            if (!_idToIndexMap.TryGetValue(id, out var index)) {
                // ...
                throw new KeyNotFoundException($"Component with ID {id} not found.");
            }

            void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(_data);
            return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, index);
        }

        public static void Dispose() {
            Application.quitting -= Dispose;
            Environment.OnEntityDestroyed -= Remove;
            
            if (_data.IsCreated) _data.Dispose();
            if (_idToIndexMap.IsCreated) _idToIndexMap.Dispose();
            if (_indexToIdMap.IsCreated) _indexToIdMap.Dispose();
        }
    }
}