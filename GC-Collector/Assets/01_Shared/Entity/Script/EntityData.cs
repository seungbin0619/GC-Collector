using Unity.Mathematics;
using XLua;

namespace Gcc.Shared {
    [GCOptimize]
    public struct EntityData {
        public int id;
        public float2 position;
        public float hp;
    }
}