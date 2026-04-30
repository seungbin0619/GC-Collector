using Gcc.Core.Entities;
using Gcc.Shared.Entity.Data;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

public class EntitySpawnTest : MonoBehaviour {
    [Button("Spawn Entities")]
    private void Spawn() {
        for (int i = 0; i < 1000; i++) {
            int id = Environment.CreateEntity();
            Vector2 position = UnityEngine.Random.insideUnitCircle * 5f;

            float posX = position.x;
            float posY = position.y;

            float velX = UnityEngine.Random.Range(-0.1f, 0.1f);
            float velY = UnityEngine.Random.Range(-0.1f, 0.1f);

            ComponentStorage<MoveComponent>.Add(id, new MoveComponent() {
                position = new float2(posX, posY),
                velocity = new float2(velX, velY)
            });
        }

        Debug.Log($"total entities: {ComponentStorage<MoveComponent>.Count}");
    }
}