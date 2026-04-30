using Unity.Collections;

namespace Gcc.Shared {
    public struct EntityCommandBuffer {
        // public NativeQueue<int>.ParallelWriter createQueue;
        public NativeQueue<int>.ParallelWriter destroyQueue;

        public void AddCreateCommand() {
            // createQueue.Enqueue(0);
        }

        public void AddDestroyCommand(int id) {
            destroyQueue.Enqueue(id);
        }
    }
}