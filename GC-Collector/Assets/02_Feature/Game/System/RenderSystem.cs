using UnityEngine;
using Gcc.Core.Entities;
using Gcc.Shared.Component;
using Gcc.Shared.System;

public class RenderSystem : MonoSystem, IUpdateSystem {
    public Mesh entityMesh;
    public Material entityMaterial;

    private Matrix4x4[] _matrices = new Matrix4x4[1023];

    public void OnUpdate(float deltaTime) {
        int count = ComponentStorage<RigidbodyComponent>.Count;
        if (count == 0) return;

        var data = ComponentStorage<RigidbodyComponent>.Data;
        int remaining = count;
        int currentIndex = 0;

        while (remaining > 0) {
            int drawCount = Mathf.Min(remaining, 1023);

            for (int i = 0; i < drawCount; i++) {
                int dataIndex = currentIndex + i;
                var move = data[dataIndex];

                _matrices[i] = Matrix4x4.TRS(
                    new Vector3(move.position.x, move.position.y, 0), 
                    Quaternion.identity, 
                    Vector3.one * 0.2f
                );
            }

            Graphics.DrawMeshInstanced(entityMesh, 0, entityMaterial, _matrices, drawCount);

            remaining -= drawCount;
            currentIndex += drawCount;
        }
    }
}