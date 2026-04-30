using Gcc.Core.Entities;
using Unity.Mathematics;

namespace Gcc.Shared.Component {
    public struct MoveComponent : IComponent {
        public float2 position;
        public float2 velocity;
        public float speed;
    }
}