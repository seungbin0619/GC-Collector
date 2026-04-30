using Gcc.Core.Entities;
using Unity.Mathematics;

namespace Gcc.Shared.Component {
    public struct RigidbodyComponent : IComponent {
        public float2 position;
        public float2 velocity;
        public float speed;
        public float radius;
    }
}