using Gcc.Core.Entities;
using Gcc.Shared.Entity.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Gcc.Feature.Game {
    public class MovementSystem : MonoBehaviour {
        [BurstCompile]
        struct MovementJob : IJobParallelFor {
            private NativeArray<MoveComponent> _array;
            private float _deltaTime;

            public MovementJob(NativeArray<MoveComponent> array, float deltaTime) {
                _array = array;
                _deltaTime = deltaTime;
            }

            public void Execute(int index) {
                var data = _array[index];
                data.position += data.velocity * _deltaTime;
                _array[index] = data;
            }
        }

        private void Update() {
            int count = ComponentStorage<MoveComponent>.Count;
            if (count == 0) {
                return;
            }

            var job = new MovementJob(ComponentStorage<MoveComponent>.Data, Time.deltaTime);
            var handle = job.Schedule(count, 64);

            handle.Complete();
        }
    }
}