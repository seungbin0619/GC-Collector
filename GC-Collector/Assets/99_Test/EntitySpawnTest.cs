using Gcc.Core.Entities;
using Gcc.Shared.Component;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntitySpawnTest : MonoBehaviour {
    [Button("Spawn Entities")]
    private void Spawn() {
        for (int i = 0; i < 1000; i++) {
            int id = Environment.CreateEntity();
            Vector2 position = Random.insideUnitCircle * 15f;

            float posX = position.x;
            float posY = position.y;

            ComponentStorage<RigidbodyComponent>.Add(id, new RigidbodyComponent() {
                position = new float2(posX, posY),
                speed = Random.Range(0.5f, 1.5f),
                radius = 0.2f,
            });
        }

        Debug.Log($"total entities: {ComponentStorage<RigidbodyComponent>.Count}");
    }
}