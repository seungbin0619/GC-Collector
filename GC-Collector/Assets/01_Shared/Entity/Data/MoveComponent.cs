using Gcc.Core.Entities;
using Unity.Mathematics;

namespace Gcc.Shared.Entity.Data {
    public struct MoveComponent : IComponentData {
        public float2 position;
        public float2 velocity;
    }
}