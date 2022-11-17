using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlatformBase))]
public class BaseInspector : Editor
{
    public SerializedProperty
        maxSpeed_Prop,
        maxForce_Prop,
        isDamp_Prop,
        Type_Prop;
        

    private void OnEnable()
    {
        maxForce_Prop = serializedObject.FindProperty("m_MaxForce");
        maxSpeed_Prop = serializedObject.FindProperty("m_MaxSpeed");
        Type_Prop = serializedObject.FindProperty("m_TranslationType");
        isDamp_Prop = serializedObject.FindProperty("m_IsDamp");
        
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PlatformBase _base = (PlatformBase)target;
        TranslationType type = _base.m_TranslationType;

        EditorGUILayout.PropertyField(Type_Prop);

        switch (type)
        {
            case TranslationType.LINEAR:
                break;
            case TranslationType.DAMP:
                EditorGUILayout.PropertyField(isDamp_Prop);
                break;
            case TranslationType.STEERING:
                EditorGUILayout.PropertyField(maxSpeed_Prop);
                EditorGUILayout.PropertyField(maxForce_Prop);
                break;
            default:
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
