using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

#if UNITY_EDITOR
[CustomEditor(typeof(SetGroupActive))]
public class GroupActivatorInspector : Editor
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

        SetGroupActive group = (SetGroupActive)target;

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