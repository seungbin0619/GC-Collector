using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using Gcc.Shared;
using Unity.Mathematics;

public static class LuaGenConfig {
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new() {
        // unity types
        typeof(GameObject),
        typeof(Transform),
        typeof(Time),
        typeof(Debug),
        typeof(Vector3),
        typeof(Vector2),
        typeof(Quaternion),
        typeof(Mathf),

        // custom types
        typeof(EntityData),
        typeof(EntityData[]),
        
        // mathematics
        typeof(float2),
        typeof(float3)
    };

    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new() {
        typeof(Action),
        typeof(Action<string>),
        typeof(Action<double>),
        typeof(Action<EntityData[]>),
        typeof(Func<double, double, double>),
        typeof(System.Collections.IEnumerator)
    };

    [GCOptimize]
    public static List<Type> GCOptimize = new() {
        typeof(EntityData),
        typeof(float2),
        typeof(float3)
    };
}