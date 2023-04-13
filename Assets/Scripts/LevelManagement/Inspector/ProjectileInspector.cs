using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
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
        DamageAll_Prop,
        VelocityLimit_Prop,
        LifeTime_Prop,
        ExplosionRadius_prop,
        HasTrail_prop;

    private void OnEnable()
    {
        HasTrail_prop = serializedObject.FindProperty("m_HasTrail");
        LifeTime_Prop = serializedObject.FindProperty("m_LifeTime");
        VelocityLimit_Prop = serializedObject.FindProperty("m_VelocityLimit");
        DamageAll_Prop = serializedObject.FindProperty("m_DamageAll");
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

        EditorGUILayout.LabelField("Core Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(Damage_prop);
        EditorGUILayout.PropertyField(LaunchForce_prop);
        EditorGUILayout.PropertyField(LifeTime_Prop);
        EditorGUILayout.PropertyField(HasTrail_prop);
        EditorGUILayout.LabelField(Environment.NewLine);

        EditorGUILayout.LabelField("Optional Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(DamageAll_Prop);
        EditorGUILayout.PropertyField(EnableGravity_prop);
        EditorGUILayout.PropertyField(IsExplosive_prop);
        EditorGUILayout.PropertyField(IsTracking_prop);
        EditorGUILayout.LabelField(Environment.NewLine);

        EditorGUILayout.LabelField("Prefab Self Reference", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(Prefab_prop);
        EditorGUILayout.LabelField(Environment.NewLine);

        if (projectile.IsExplosive)
        {
            EditorGUILayout.LabelField("Explosion Reference and Radius", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(ExplosionPrefab_prop);
            EditorGUILayout.PropertyField(ExplosionRadius_prop);
            EditorGUILayout.LabelField(Environment.NewLine);
        }

        if (projectile.IsTracking)
        {
            EditorGUILayout.LabelField("Tracking Force and Velocity Limit", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(TrackingForce_prop);
            EditorGUILayout.PropertyField(VelocityLimit_Prop);
            EditorGUILayout.LabelField(Environment.NewLine);
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif