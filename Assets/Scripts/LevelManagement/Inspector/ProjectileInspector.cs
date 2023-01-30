using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(Projectile))]
public class ProjectileInspector : Editor
{
    public SerializedProperty
        Prefab_prop,
        ExplosionPrefab_prop,
        EnableGravity_prop,
        IsTracking_prop,
        IsExplosive_prop,
        LaunchForce_prop,
        TrackingForce_prop,
        Damage_prop,
        ExplosionRadius_prop;

    private void OnEnable()
    {
        Prefab_prop = serializedObject.FindProperty("m_Prefab");
        ExplosionPrefab_prop = serializedObject.FindProperty("m_ExplosionPrefab");
        EnableGravity_prop = serializedObject.FindProperty("m_EnableGravity");
        IsTracking_prop = serializedObject.FindProperty("m_IsTracking");
        LaunchForce_prop = serializedObject.FindProperty("m_LaunchForce");
        TrackingForce_prop = serializedObject.FindProperty("m_TrackingForce");
        Damage_prop = serializedObject.FindProperty("m_DamageAmount");
        ExplosionRadius_prop = serializedObject.FindProperty("m_ExplosionRadius");
        IsExplosive_prop = serializedObject.FindProperty("m_IsExplosive");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Projectile projectile = (Projectile)target;

        EditorGUILayout.PropertyField(Damage_prop);
        EditorGUILayout.PropertyField(LaunchForce_prop);
        

        EditorGUILayout.PropertyField(EnableGravity_prop);
        EditorGUILayout.PropertyField(IsExplosive_prop);
        EditorGUILayout.PropertyField(IsTracking_prop);
        
        
        EditorGUILayout.PropertyField(Prefab_prop);

        if (projectile.IsExplosive)
        {
            EditorGUILayout.PropertyField(ExplosionPrefab_prop);
            EditorGUILayout.PropertyField(ExplosionRadius_prop);
        }

        if (projectile.IsTracking)
        {
            EditorGUILayout.PropertyField(TrackingForce_prop);
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif