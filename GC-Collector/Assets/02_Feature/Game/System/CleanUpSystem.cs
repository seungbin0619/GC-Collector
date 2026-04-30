using Gcc.Core.Entities;
using Gcc.Feature.Component;
using UnityEngine;

namespace Gcc.Feature.Game {
    public class CleanUpSystem : MonoBehaviour {
        private void LateUpdate() {
            int count = ComponentStorage<DestroyTag>.Count;
            if (count == 0) {
                return;
            }

            for (int i = count - 1; i >= 0; i--) {
                int id = ComponentStorage<DestroyTag>.IdOf(i);

                Environment.DestroyEntity(id);
            }
        }
    }
}