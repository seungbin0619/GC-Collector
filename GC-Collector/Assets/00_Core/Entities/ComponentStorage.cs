using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Gcc.Core.Entities {
    public static class ComponentStorage<T> where T : struct, IComponent {
        private static NativeArray<T> _data;
        public static NativeArray<T> Data => _data;
        
        private static NativeArray<int> _ids;
        public static NativeArray<int>.ReadOnly Ids => _ids.AsReadOnly();

        private static NativeParallelHashMap<int, int> _idToIndexMap;

        public static int Count { get; private set; }
        public static int Capacity => Environment.MaxEntities;

        static ComponentStorage() {
            _data = new(Capacity, Allocator.Persistent);
            _idToIndexMap = new(Capacity, Allocator.Persistent);
            _ids = new(Capacity, Allocator.Persistent);
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
            _ids[Count] = id;

            Count++;
        }

        public static void Add(int id) => Add(id, default);

        public static void Remove(int id) {
            if (!_idToIndexMap.TryGetValue(id, out var index)) {
                return;
            }

            int lastIndex = Count - 1;
            if (index != lastIndex) {
                var data = _data[lastIndex];
                _data[index] = data;

                int lastId = _ids[lastIndex];
                _ids[index] = lastId;
                _idToIndexMap[lastId] = index;
            }

            _idToIndexMap.Remove(id);
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

        public static int IdOf(int index) {
            if (index < 0 || index >= Count) {
                throw new IndexOutOfRangeException($"Index {index} is out of range.");
            }

            return _ids[index];
        }

        public static void Dispose() {
            Application.quitting -= Dispose;
            Environment.OnEntityDestroyed -= Remove;
            
            if (_data.IsCreated) _data.Dispose();
            if (_idToIndexMap.IsCreated) _idToIndexMap.Dispose();
            if (_ids.IsCreated) _ids.Dispose();
        }
    }
}