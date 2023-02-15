using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
#if UNITY_EDITOR
[CustomEditor(typeof(BasicSpawner))]
public class BasicSpawnerInspector : Editor
{
    public SerializedProperty
        Activator,
        UseChildren;

    private void OnEnable()
    {
        Activator = serializedObject.FindProperty("m_Activator");
        UseChildren = serializedObject.FindProperty("m_UseChildren");
    }

    public override void OnInspectorGUI()
    {

        BasicSpawner group = (BasicSpawner)target;

        if (group.m_UseChildren)
        {
            EditorGUILayout.PropertyField(Activator);
            EditorGUILayout.PropertyField(UseChildren);
        }
        else
        {
            DrawDefaultInspector();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
