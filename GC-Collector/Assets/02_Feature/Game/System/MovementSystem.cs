using Gcc.Core.Entities;
using Gcc.Shared;
using Gcc.Shared.Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Gcc.Feature.Game {
    public class MovementSystem : MonoBehaviour {
        private const int BatchSize = 64;
        private const float CellSize = 1.0f;

        [SerializeField]
        private Transform targetTransform;

        [BurstCompile]
        private static int GetCellHash(float2 position) {
            int x = (int)math.floor(position.x / CellSize);
            int y = (int)math.floor(position.y / CellSize);

            return x * 73856093 ^ y * 19349663;
        }

        [BurstCompile]
        struct BuildGridHashJob : IJobParallelFor {
            [ReadOnly] 
            private NativeArray<RigidbodyComponent> _data;
            private NativeParallelMultiHashMap<int, int>.ParallelWriter _gridHashMap;

            public BuildGridHashJob(NativeArray<RigidbodyComponent> data, NativeParallelMultiHashMap<int, int>.ParallelWriter gridHashMap) {
                _data = data;
                _gridHashMap = gridHashMap;
            }

            public void Execute(int id) {
                int hash = GetCellHash(_data[id].position);

                _gridHashMap.Add(hash, id);
            }
        }
        
        [BurstCompile]
        struct MovementJob : IJobParallelFor {
            private const float Threshold = 0.01f;

            [NativeDisableParallelForRestriction]
            private NativeArray<RigidbodyComponent> _array;

            [ReadOnly] 
            private NativeArray<int>.ReadOnly _ids;

            [ReadOnly]
            private NativeParallelMultiHashMap<int, int>.ReadOnly _gridHashMap;

            private readonly EntityCommandBuffer _buffer;
            private readonly float2 _targetPosition;
            private readonly float _deltaTime;

            public MovementJob(
                    NativeArray<RigidbodyComponent> array, 
                    NativeArray<int>.ReadOnly ids, 
                    NativeParallelMultiHashMap<int, int>.ReadOnly gridHashMap, 
                    EntityCommandBuffer buffer, 
                    float2 targetPosition, 
                    float deltaTime) {
                _array = array;
                _ids = ids;
                _gridHashMap = gridHashMap;

                _buffer = buffer;
                _targetPosition = targetPosition;
                _deltaTime = deltaTime;
            }

            public void Execute(int index) {
                var data = _array[index];
                var force = float2.zero;
                int baseX = (int)math.floor(data.position.x / CellSize);
    int baseY = (int)math.floor(data.position.y / CellSize);

    // 2. [핵심 변경점] 내 주변 3x3 (총 9칸)을 모두 뒤집니다!
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            // 주변 칸의 해시값 계산
            int neighborHash = (baseX + x) * 73856093 ^ (baseY + y) * 19349663;

            // 해당 칸에 누군가 있다면 검사 시작
            if (_gridHashMap.TryGetFirstValue(neighborHash, out int otherId, out var iterator)) {
                do {
                    if (otherId == index) continue;

                    var otherData = _array[otherId];
                    float2 diff = data.position - otherData.position;
                    float distanceSq = math.lengthsq(diff);
                    float minDistance = data.radius + otherData.radius;

                    if (distanceSq > 0.0001f && distanceSq < minDistance * minDistance) {
                        float distance = math.sqrt(distanceSq);
                        float overlap = minDistance - distance;

                        // 밀어내는 힘! (50.0f 정도로 강하게 줘보세요)
                        force += (diff / distance) * overlap * 5.0f;
                    }
                } while (_gridHashMap.TryGetNextValue(out otherId, ref iterator));
            }
        }
    }

                float2 direction = _targetPosition - data.position;
                if (math.lengthsq(direction) > Threshold) {
                    data.velocity = math.normalize(direction) * data.speed;
                } else {
                    data.velocity = float2.zero;
                    _buffer.AddDestroyCommand(_ids[index]);
                }

                data.position += (data.velocity + force) * _deltaTime;
                _array[index] = data;
            }
        }

        private void Update() {
            int count = ComponentStorage<RigidbodyComponent>.Count;
            if (count == 0) {
                return;
            }

            var gridHashMap = new NativeParallelMultiHashMap<int, int>(count, Allocator.TempJob);
            var buildGridHashJob = new BuildGridHashJob(
                ComponentStorage<RigidbodyComponent>.Data,
                gridHashMap.AsParallelWriter());

            var buildHandle = buildGridHashJob.Schedule(count, BatchSize);

            var targetPosition = new float2(targetTransform.position.x, targetTransform.position.y);
            var job = new MovementJob(
                ComponentStorage<RigidbodyComponent>.Data,
                ComponentStorage<RigidbodyComponent>.Ids,
                gridHashMap.AsReadOnly(),
                CommandBufferSystem.Create(),
                targetPosition,
                Time.deltaTime);

            job.Schedule(count, BatchSize, buildHandle).Complete();
            gridHashMap.Dispose();
        }
    }
}