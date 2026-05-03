using Gcc.Core.Entities;
using Gcc.Feature.Component;
using Gcc.Shared.System;
using UnityEngine;

namespace Gcc.Feature.Game {
    public class CleanUpSystem : MonoSystem, ILateUpdateSystem {
        public void OnLateUpdate(float deltaTime) {
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