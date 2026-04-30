using Gcc.Core.Entities;
using Gcc.Feature.Component;
using Gcc.Shared;
using Unity.Collections;
using UnityEngine;

namespace Gcc.Feature.Game {
    public class CommandBufferSystem : MonoBehaviour {
        private static NativeQueue<int> _destroyQueue;

        private void Awake() {
            _destroyQueue = new(Allocator.Persistent);
        }

        public static EntityCommandBuffer Create() {
            return new EntityCommandBuffer() {
                destroyQueue = _destroyQueue.AsParallelWriter(),
            };
        }

        private void LateUpdate() {
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