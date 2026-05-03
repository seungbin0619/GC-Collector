using UnityEngine;

namespace Gcc.Shared.System {
    public abstract class MonoSystem : MonoBehaviour, ISystem {
        public int updateOrder => 0;

        protected virtual void OnEnable() {
            SystemDispatcher.Register(this);
        }

        protected virtual void OnDisable() {
            SystemDispatcher.Unregister(this);
        }
    }
}