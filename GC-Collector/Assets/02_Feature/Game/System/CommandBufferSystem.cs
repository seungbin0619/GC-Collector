using Gcc.Core.Entities;
using Gcc.Feature.Component;
using Gcc.Shared;
using Gcc.Shared.System;
using Unity.Collections;
using UnityEngine;

namespace Gcc.Feature.Game {
    public class CommandBufferSystem : MonoSystem, ILateUpdateSystem {
        private static NativeQueue<int> _destroyQueue;

        private void Awake() {
            _destroyQueue = new(Allocator.Persistent);
        }

        public static EntityCommandBuffer Create() {
            return new EntityCommandBuffer() {
                destroyQueue = _destroyQueue.AsParallelWriter(),
            };
        }

        public void OnLateUpdate(float deltaTime) {
            while (_destroyQueue.TryDequeue(out int id)) {
                if (!Environment.IsEntityAlive(id)) {
                    continue;
                }

                ComponentStorage<DestroyTag>.Add(id);
            }
        }

        private void OnDestroy() {
            _destroyQueue.Dispose();
        }
    }
}