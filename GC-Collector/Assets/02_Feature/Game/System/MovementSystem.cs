using Gcc.Core.Entities;
using Gcc.Shared;
using Gcc.Shared.Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Gcc.Feature.Game {
    public class MovementSystem : MonoBehaviour {
        private const int BatchSize = 64;

        [SerializeField]
        private Transform targetTransform;
        
        [BurstCompile]
        struct MovementJob : IJobParallelFor {
            private const float Threshold = 0.01f;

            private NativeArray<MoveComponent> _array;
            private NativeArray<int>.ReadOnly _ids;
            private readonly EntityCommandBuffer _buffer;
            private readonly float2 _targetPosition;
            private readonly float _deltaTime;

            public MovementJob(NativeArray<MoveComponent> array, NativeArray<int>.ReadOnly ids, EntityCommandBuffer buffer, float2 targetPosition, float deltaTime) {
                _array = array;
                _ids = ids;

                _buffer = buffer;
                _targetPosition = targetPosition;
                _deltaTime = deltaTime;
            }

            public void Execute(int index) {
                var data = _array[index];

                float2 direction = _targetPosition - data.position;

                if (math.lengthsq(direction) > Threshold) {
                    data.velocity = math.normalize(direction) * data.speed;
                } else {
                    data.velocity = float2.zero;

                    _buffer.AddDestroyCommand(_ids[index]);
                }

                data.position += data.velocity * _deltaTime;
                _array[index] = data;
            }
        }

        private void Update() {
            int count = ComponentStorage<MoveComponent>.Count;
            if (count == 0) {
                return;
            }

            var targetPosition = new float2(targetTransform.position.x, targetTransform.position.y);
            var job = new MovementJob(
                ComponentStorage<MoveComponent>.Data,
                ComponentStorage<MoveComponent>.Ids,
                CommandBufferSystem.Create(),
                targetPosition,
                Time.deltaTime);

            job.Schedule(count, BatchSize).Complete();
        }
    }
}